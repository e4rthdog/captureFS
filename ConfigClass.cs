using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureFS
{
    public class ConfigClass
    {
        public string ImagePath { get; set; }
        public int ImageQuality { get; set; }
        public string ImageType { get; set; }
        public string CustomActions { get; set; }
        public int TimerInterval { get; set; }
    }
}
