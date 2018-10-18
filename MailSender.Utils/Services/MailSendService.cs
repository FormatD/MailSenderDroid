using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;

namespace MailSender.Utils.Services
{
    public class MailSendService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;
        private readonly string _baseFolder;
        private string _taskFile;
        private readonly string _tempFileFolder;
        private const string TaskFolder = "tasks";

        public MailSendService(Config config, string baseFolder, string tempFileFolder)
        {
            _config = config;
            _baseFolder = baseFolder;
            _tempFileFolder = tempFileFolder;
        }

        public async Task<IEnumerable<SubTask>> SplitTasks(string file, bool reuse = false)
        {
            if (!File.Exists(file))
            {
                _logger.Error("file \"{0}\" was not existed.", file);
                return new List<SubTask>();
            }
            var fileName = Path.GetFileNameWithoutExtension(file);

            var size = new FileInfo(file).Length;
            _taskFile = Path.Combine(_baseFolder, TaskFolder, $"{fileName}_{size}");

            EnsureDirectory(Path.Combine(_baseFolder, TaskFolder));

            var fileList = SplitFile(file).ToList();

            if (reuse)
            {
                var finishedFragements = File.ReadAllLines(_taskFile);
                fileList = fileList.Where(x => !finishedFragements.Contains(x.CurrentFile)).ToList();
            }
            else
            {
                File.Delete(_taskFile);
            }

            return SplitFile(file);
        }

        public void SendEmail(IEnumerable<SubTask> fileList, string subject = null)
        {
            var queue = new ConcurrentQueue<string>();

            subject = subject ?? Path.GetFileNameWithoutExtension(fileList.FirstOrDefault().CurrentFile);


            foreach (var fileFragement in fileList)
            {
                //queue.Enqueue(fileFragement);

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connected += (s, e) => _logger.Info("Connected.");
                    client.Disconnected += (s, e) => _logger.Info("Disconnected.");

                    client.Connect(_config.SmtpServerAddress, _config.SmtpServerPort, false);
                    fileFragement.State = SubTask.TaskState.Sending;
                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(_config.SendFrom, _config.UserPass);
                    client.MessageSent += MessageSent;

                    var message = CreateMessage(fileFragement, subject);
                    client.Send(message);

                    //try
                    //{
                    //    while (queue.Count > _config.MaxSendQueueCount)
                    //    {
                    //        Thread.Sleep(300);
                    //    }
                    //    _logger.Info($"begin send file {fileFragement}.");
                    //    // client.Send(message);
                    //    client.Send(message);
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.Error($"Exception caught in SendEmail(): {ex}");
                    //}

                    fileFragement.State = SubTask.TaskState.Sended;
                    client.Disconnect(true);
                }
            }

            while (queue.Any())
            {
                Thread.Sleep(100);
            }
        }

        private void MessageSent(object sender, MessageSentEventArgs e)
        {
            //var sendArg = e.UserState as SendArg;
            //if (sendArg == null)
            //    return;

            //if (e.Error != null)
            //{
            //    _logger.Warn($"Error when send file {sendArg.File} ,the {sendArg.RetryTime + 1} times.");
            //    _logger.Warn(e.Error.ToString());
            //    //if (sendArg.RetryTime < 3)
            //    sendArg.Client.SendAsync(sendArg.Message, sendArg.Retry());
            //}
            //else
            //{
            //    _logger.Info($"Sucess when send file {sendArg.File} ,the {sendArg.RetryTime + 1} times.");

            //    File.AppendAllText(_taskFile, sendArg.File + System.Environment.NewLine);

            //    string fileName;
            //    while (!sendArg.QueuedFiles.TryDequeue(out fileName))
            //    {
            //    }
            //}
        }

        private static void EnsureDirectory(string folderName)
        {
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
        }

        private IEnumerable<SubTask> SplitFile(string file)
        {
            var fileList = new List<SubTask>();
            var fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
                yield break;

            if (fileInfo.Length <= _config.MaxSize)
            {
                yield return new SubTask { SourceFile = file, CurrentFile = file, State = SubTask.TaskState.Splited };
                yield break;
            }

            var tempFileBaseDir = Path.Combine(_tempFileFolder, "temp_" + Path.GetFileNameWithoutExtension(file));
            EnsureDirectory(tempFileBaseDir);

            var nameOnly = Path.GetFileName(file);

            var offset = 0;
            var index = 1;
            using (var fs = File.OpenRead(file))
            {
                byte[] fileBytes = new byte[_config.MaxSize];

                var readByte = fs.Read(fileBytes, offset, _config.MaxSize);

                while (readByte > 0)
                {
                    var targetBytes = readByte == _config.MaxSize ? fileBytes : fileBytes.Take(readByte).ToArray();
                    var fileName = Path.Combine(tempFileBaseDir, $"{nameOnly}.{index:D3}");
                    File.WriteAllBytes(fileName, targetBytes);

                    yield return new SubTask { SourceFile = file, CurrentFile = fileName, State = SubTask.TaskState.Splited };

                    offset += readByte;
                    index++;
                    readByte = fs.Read(fileBytes, 0, _config.MaxSize);
                }
            }

            yield break;
        }

        private MimeMessage CreateMessage(SubTask subTask, string subject = null)
        {

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config.SendFrom, _config.SendFrom));

            foreach (var mailAddress in _config.SendTo.Split(';'))
            {
                message.To.Add(new MailboxAddress(mailAddress, mailAddress));
            }
            message.Subject = subject ?? $"{subTask.SourceFile}.";

            message.Body = new TextPart("plain")
            {
                Text = $"file of {subTask.CurrentFile}."
            };

            var attachment = new MimePart("image", "gif")
            {
                //ContentObject = new ContentObject(File.OpenRead(path), ContentEncoding.Default),
                Content = new MimeContent(File.OpenRead(subTask.CurrentFile)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(subTask.CurrentFile)
            };

            // now create the multipart/mixed container to hold the message text and the
            // image attachment
            var multipart = new Multipart("mixed")
            {
                new TextPart("plain")
                {
                    Text = $"file of {subTask.CurrentFile}."
                },
                attachment
            };

            // now set the multipart/mixed as the message body
            message.Body = multipart;

            return message;
        }
    }

    public class SubTask
    {
        public TaskState State { get; set; }

        public string SourceFile { get; set; }

        public string CurrentFile { get; set; }

        public enum TaskState
        {
            Splited = 1,
            Sending = 2,
            Sended = 3,
        }
    }
}