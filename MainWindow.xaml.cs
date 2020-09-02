using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using FSUIPC;
using System.Windows.Threading;
using System.Windows.Forms;

namespace CaptureFS
{
    public partial class MainWindow : Window
    {
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        const int SRCCOPY = 0x00CC0020;

        const int CAPTUREBLT = 0x40000000;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public Process process;
        public IntPtr hwnd;
        public int image_counter = 1;
        public DispatcherTimer timerFS;
        public string imagePath;

        public MainWindow()
        {
            Setup();
            InitializeComponent();
            HandleDroneActions(false);
            SetupUI();
        }

        private void Setup()
        {
            imagePath = string.Empty;
            process = Process.GetProcesses().Where(p => p.ProcessName == "FlightSimulator").Single();
            hwnd = process.MainWindowHandle;
            timerFS = new DispatcherTimer();
            timerFS.Interval = new TimeSpan(0, 0, 2);
            timerFS.Tick += timerFS_Tick;
            timerFS.IsEnabled = false;

        }
        private void HandleDroneActions(Boolean _mode)
        {
            if (_mode)
            {
                rdNone.IsChecked = true;
                rdNone.IsEnabled = true;
                rdForward.IsEnabled = true;
                rdBackwards.IsEnabled = true;
                rdLeft.IsEnabled = true;
                rdRight.IsEnabled = true;
                rdLookup.IsEnabled = true;
                rdLookdown.IsEnabled = true;
                rdLookleft.IsEnabled = true;
                rdLookright.IsEnabled = true;
                rdIncreaseAlt.IsEnabled = true;
                rdDecreaseAlt.IsEnabled = true;
            }
            else
            {
                rdNone.IsChecked = true;
                rdNone.IsEnabled = false;
                rdForward.IsEnabled = false;
                rdBackwards.IsEnabled = false;
                rdLeft.IsEnabled = false;
                rdRight.IsEnabled = false;
                rdLookup.IsEnabled = false;
                rdLookdown.IsEnabled = false;
                rdLookleft.IsEnabled = false;
                rdLookright.IsEnabled = false;
                rdIncreaseAlt.IsEnabled = false;
                rdDecreaseAlt.IsEnabled = false;

            }
        }
        private void SetupUI()
        {
            lblPath.Content = imagePath;
        }
        private void timerFS_Tick(object sender, EventArgs e)
        {
            if (timerFS.IsEnabled == true)
            {
                if (FSUIPCConnection.IsOpen)
                {
                    if (rdForward.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.W);
                    }
                    if (rdBackwards.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.S);
                    }
                    if (rdLeft.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.A);
                    }
                    if (rdRight.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.D);
                    }
                    if (rdLookup.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.NumPad8);
                    }
                    if (rdLookdown.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.NumPad2);
                    }
                    if (rdLookleft.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.NumPad4);
                    }
                    if (rdLookright.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.NumPad6);
                    }
                    if (rdIncreaseAlt.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.R);
                    }
                    if (rdDecreaseAlt.IsChecked == true)
                    {
                        FSUIPCConnection.SendKeyToFS(Keys.F);
                    }
                }
                SavePicture();
            }
        }
        private void btnFS_Click(object sender, RoutedEventArgs e)
        {
            if (FSUIPCConnection.IsOpen)
            {
                HandleDroneActions(false);
                FSUIPCConnection.Close();
            }
            else
            {
                lblStatus.Content = "Looking for a flight simulator...";
                try
                {
                    FSUIPCConnection.Open();
                    HandleDroneActions(true);
                }
                catch (FSUIPCException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Connection Failed. ");
                }
            }
            setConnectionStatus();

        }
        private void setConnectionStatus()
        {
            if (FSUIPCConnection.IsOpen)
            {
                btnFS.Content = "Disconnect";
                lblStatus.Content = "Connected to " + FSUIPCConnection.FlightSimVersionConnected.ToString();
                lblStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                btnFS.Content = "Connect";
                lblStatus.Content = "Disconnected";
                lblStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        private void SavePicture()
        {
            var img = CaptureWindow(hwnd);
            var fullpath = string.Format(@"{1}\{0:000}.png", image_counter, imagePath);
            img.Save(fullpath, ImageFormat.Png);
            lblImagesSaved.Content = string.Format("{0:000}", image_counter);
            image_counter++;
        }
        public Bitmap CaptureWindow(IntPtr hWnd)
        {
            RECT region;

            GetWindowRect(hWnd, out region);

            return this.CaptureRegion(System.Drawing.Rectangle.FromLTRB(region.left, region.top, region.right, region.bottom));
        }
        public Bitmap CaptureRegion(System.Drawing.Rectangle region)
        {
            IntPtr desktophWnd;
            IntPtr desktopDc;
            IntPtr memoryDc;
            IntPtr bitmap;
            IntPtr oldBitmap;
            bool success;
            Bitmap result;

            desktophWnd = GetDesktopWindow();
            desktopDc = GetWindowDC(desktophWnd);
            memoryDc = CreateCompatibleDC(desktopDc);
            bitmap = CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
            oldBitmap = SelectObject(memoryDc, bitmap);

            success = BitBlt(memoryDc, 0, 0, region.Width, region.Height, desktopDc, region.Left, region.Top, SRCCOPY | CAPTUREBLT);

            try
            {
                if (!success)
                {
                    throw new Win32Exception();
                }

                result = System.Drawing.Image.FromHbitmap(bitmap);
            }
            finally
            {
                SelectObject(memoryDc, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDc);
                ReleaseDC(desktophWnd, desktopDc);
            }

            return result;
        }
        private void btnPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                imagePath = dialog.SelectedPath;
                SetupUI();
            }
        }
        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (imagePath != String.Empty)
            {
                if (timerFS.IsEnabled == true)
                {
                    timerFS.IsEnabled = false;
                    btnCapture.Content = "Start";
                    image_counter = 1;
                    lblImagesSaved.Content = string.Format("{0:000}", 0);
                }
                else
                {
                    timerFS.IsEnabled = true;
                    btnCapture.Content = "Stop";
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Path is empty!", "Error");
            }
        }
        private void timeInterval_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            timerFS.Interval = (TimeSpan)e.NewValue;
        }
        private void LeftMouseDown_Event(object sender, EventArgs e)
        {
            this.DragMove();
        }
    }
}
