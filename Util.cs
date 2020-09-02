using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CaptureFS
{
    public class Util
    {
        public static string GetVersion()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("CaptureFS.version.txt"));
            var _ret = reader.ReadToEnd().Replace("\n", "").ToString();
            _ret = _ret.Substring(0, _ret.LastIndexOf("-")).Replace('-', '.');
            return _ret;
        }
        public static string GetCopyright()
        {
            var year = DateTime.Now.Year.ToString();
            return String.Format("© {0} - Elias Stassinos", year);
        }
    }
}
