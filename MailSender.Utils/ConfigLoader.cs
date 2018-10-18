using System.IO;
using MailSender.Utils.Services;
using static System.Environment;
using Environment = System.Environment;

namespace MailSender.Utils
{
    public class ConfigLoader
    {

        public static Config LoadConfig()
        {
            var configFile = GetConfigPath();
            if (string.IsNullOrWhiteSpace(configFile) || !File.Exists(configFile))
                return new Config();

            return File.ReadAllText(configFile).FromJson<Config>();
        }

        private static string GetConfigPath()
        {
            return Path.Combine(Environment.GetFolderPath(SpecialFolder.Personal), "mail.config");
        }

        public static void SaveConfig(Config config)
        {
            File.WriteAllText(GetConfigPath(), config.ToJson());
        }
    }
}

