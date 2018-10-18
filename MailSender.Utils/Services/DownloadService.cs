using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MailSender.Utils.Services
{
    public class DownloadService
    {
        private readonly string _saveDirectory;
        private readonly Action<DownloadProgressChangedEventArgs> _progressAction;
        private int _lastPercent = -1;

        public DownloadService(string saveDirectory, Action<DownloadProgressChangedEventArgs> progressAction = null)
        {
            _saveDirectory = saveDirectory;
            _progressAction = progressAction;
        }

        public async Task<string> DownloadFile(string url)
        {
            var webClient = new MyWebClient();

            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

            var downloadedData = await webClient.DownloadDataTaskAsync(new Uri(url));
            string fileName = "";

            // Try to extract the filename from the Content-Disposition header
            if (!String.IsNullOrEmpty(webClient.ResponseHeaders["Content-Disposition"]))
            {
                fileName = webClient.ResponseHeaders["Content-Disposition"].Substring(webClient.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
            }
            else
            {
                var uri = webClient.ResponseUri?.ToString();

                fileName = string.IsNullOrWhiteSpace(uri)
                    ? "temp.download"
                    : Path.GetFileName(uri) ?? "temp.download";
            }

            var fullPath = Path.Combine(_saveDirectory, fileName);
            File.WriteAllBytes(fullPath, downloadedData);

            return fullPath;
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == _lastPercent)
                return;
            _lastPercent = e.ProgressPercentage;
            _progressAction?.Invoke(e);
        }
    }



    class MyWebClient : WebClient
    {
        Uri _responseUri;

        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            _responseUri = response.ResponseUri;
            return response;
        }
    }
}