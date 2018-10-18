using MailSender.Utils;
using MailSender.Utils.Services;
using System.Windows.Input;
using Xamarin.Forms;

namespace MailSender.Forms.ViewModels
{
    public class SettingViewModel : BaseViewModel
    {

        private SettingViewModel()
        {
            SaveCommand = new Command(() => ConfigLoader.SaveConfig(ToConfig()));
        }

        public static SettingViewModel Load()
        {
            return From(ConfigLoader.LoadConfig());
        }

        public static SettingViewModel From(Config config)
        {
            return new SettingViewModel
            {
                SendFrom = config.SendFrom,
                SendTo = config.SendTo,
                UserPass = config.UserPass,
                MaxSendQueueCount = config.MaxSendQueueCount,
                MaxSize = config.MaxSize,
                SmtpServerAddress = config.SmtpServerAddress,
                SmtpServerPort = config.SmtpServerPort,
            };
        }

        public Config ToConfig()
        {
            return new Config
            {
                SendFrom = this.SendFrom,
                SendTo = this.SendTo,
                UserPass = this.UserPass,
                MaxSendQueueCount = this.MaxSendQueueCount,
                MaxSize = this.MaxSize,
                SmtpServerAddress = this.SmtpServerAddress,
                SmtpServerPort = this.SmtpServerPort,
            };
        }

        public string SendFrom { get; set; } = "sample@live.cn";

        public string SendTo { get; set; } = "dengqianjun@huawei.com";

        public int MaxSize { get; set; } = 9000000;
        public string SmtpServerAddress { get; set; } = "smtp-mail.outlook.com";
        public int SmtpServerPort { get; set; } = 587;
        public string UserName { get; set; } = "sample@live.cn";
        public string UserPass { get; set; } = "";

        public string PackagesSavePath { get; set; } = @"D:\Microsoft.Nuget.Packages\";

        public int MaxSendQueueCount { get; set; } = 1;

        public ICommand SaveCommand { get; private set; }

    }
}
