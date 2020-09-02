using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Configuration = SharpConfig.Configuration;

namespace CaptureFS
{
    public class Util
    {
        const string configFile = "CaptureFS.cfg";
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
        public static ConfigClass LoadConfig(string _section)
        {
            Configuration cfg = Configuration.LoadFromFile(configFile);
            cfg = Configuration.LoadFromFile(configFile);
            return cfg[_section].ToObject<ConfigClass>();
        }
        public static void SaveConfig(ConfigClass _config)
        {
            Configuration cfg = Configuration.LoadFromFile(configFile);
            cfg["MAIN"].GetValuesFrom(_config);
            cfg.SaveToFile("CaptureFS.cfg");
        }
    }
}
