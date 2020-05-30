using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EBayAPI.Infrastructure
{
    public class ConfigReader
    {
        public static string GetConnectionString(string name)
        {
            if (ConfigurationManager.ConnectionStrings[name] == null)
            {
                throw new KeyNotFoundException(string.Format("AppSetting中找不到连接字符串：{0}", name));
            }
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static string GetAppSetting(string name)
        {
            ExeConfigurationFileMap file = new ExeConfigurationFileMap();
            file.ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory+"\\App.config";
            Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(file, ConfigurationUserLevel.None);

            var myApp = (AppSettingsSection)config.GetSection("appSettings");

            if (myApp.Settings[name] == null)
            {
                throw new KeyNotFoundException(string.Format("AppSetting中找不到键：{0}", name));
            }
            return myApp.Settings[name].Value;
        }

        public static T GetAppSetting<T>(string name)
        {
            if (ConfigurationManager.AppSettings[name] == null)
            {
                throw new KeyNotFoundException(string.Format("AppSetting中找不到键：{0}", name));
            }
            return (T)Convert.ChangeType(ConfigurationManager.AppSettings[name], typeof(T));
        }

        public static bool HasAppSetting(string name)
        {
            ExeConfigurationFileMap file = new ExeConfigurationFileMap();
            file.ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory + "\\App.config";
            Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(file, ConfigurationUserLevel.None);

            return config.AppSettings.Settings.AllKeys.Contains(name);
        }
    }
}
