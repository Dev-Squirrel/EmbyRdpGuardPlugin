using System;
using System.Text.RegularExpressions;
using System.Configuration;
using System.IO;
using System.Reflection;
using rdpguard_plugin_api;

namespace EmbyRdpGuardPlugin
{
    public class EmbyRdpGuardPlugin : IExternalLogBasedEngine
    {
        public string Name => "EmbyRdpGuardPlugin";
        public string Protocol => "EMBY";

        private static string ReadSetting(string key)
        {
            try
            {
                string configPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\EmbyRdpGuardPlugin.xml";

                var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
                var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                var appSettings = config.AppSettings.Settings;

                return appSettings[key]?.Value ?? "";
            }
            catch (ConfigurationErrorsException err)
            {
                Console.WriteLine("Message: {0}", err.Message);
                return "";
            }
        }

        public string Directory => ReadSetting("LogDirectory");
        public string FileMask => ReadSetting("LogFile");

        public bool IsFailedLoginAttempt(string _line, out string _ip, out string _username)
        {
            // Emby logs do not include the username for failed login attempts, so we set it to null.
            _ip = _username = null;

            // sample log line:
            // 2023-03-16 13:11:10.766 Warn Server: AUTH-ERROR: 192.168.0.222 - Invalid username or password entered.
            string pattern = @"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{1,4}\sWarn\sServer:\sAUTH-ERROR:\s(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\s-\sInvalid\susername\sor\spassword\sentered\.";

            var match = Regex.Match(_line, pattern);
            if (match.Success)
            {
                _ip = match.Groups[1].Value;

                return true;
            }

            return false;
        }
    }
}
