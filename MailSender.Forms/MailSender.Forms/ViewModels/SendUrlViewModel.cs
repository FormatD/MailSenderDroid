using CodeMill.VMFirstNav;
using MailSender.Utils;
using MailSender.Utils.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MailSender.Forms.ViewModels
{
    public class SendUrlViewModel : BaseViewModel
    {
        private readonly DownloadService _downloadService;
        private readonly IMessage _messageService;

        public SendUrlViewModel()
        {
            _downloadService = new DownloadService(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ReportProgress);

            _messageService = DependencyService.Get<IMessage>();

            DownloadAndSend = new Command(SendAndDownload, () => true);
            ViewProgress = new Command(async () => await NavigationService.Instance.PushAsync(ProgressViewModel), () => ProgressViewModel != null);
        }

        public string Url { get; set; }

        public string FileName { get; set; }

        public string SendToAddress { get; set; }

        public string LogContent => string.Join(Environment.NewLine, Logs ?? Enumerable.Empty<string>());

        public IList<string> Logs { get; set; } = new List<string>();

        public ICommand DownloadAndSend { get; }

        public ICommand ViewProgress { get; }

        public ProgressViewModel ProgressViewModel { get; private set; }

        private void ReportProgress(DownloadProgressChangedEventArgs arg)
        {
            try
            {
                if (arg != null)
                    AddLog($"{Url},Finished:{arg.ProgressPercentage}%");
            }
            catch (Exception)
            {

            }
        }

        private void AddLog(string log)
        {
            Logs.Add(log);
            OnPropertyChanged(nameof(LogContent));
        }

        private async void SendAndDownload()
        {
            try
            {
                IsBusy = true;
                AddLog("Start Downloading.");
                var file = await _downloadService.DownloadFile(Url);
                AddLog("Start Sending.");
                var mailService = new MailSendService(
                    ConfigLoader.LoadConfig(),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                AddLog("Spliting files.");
                var tasks = await mailService.SplitTasks(file);
                ProgressViewModel = new ProgressViewModel(tasks);
                AddLog("Sending files.");
                mailService.SendEmail(tasks);
                AddLog("Send sucessed.");

                _messageService.LongAlert($"Finished.");
            }
            catch (Exception ex)
            {
                AddLog("Send fialed.");
                _messageService.LongAlert($"Error encountered:{ex.ToString()}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
