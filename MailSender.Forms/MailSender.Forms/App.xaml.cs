using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MailSender.Forms.Services;
using MailSender.Forms.Views;
using NLog;
using NLog.Targets;
using NLog.Config;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace MailSender.Forms
{
    public partial class App : Application
    {
        //TODO: Replace with *.azurewebsites.net url after deploying backend to Azure
        public static string AzureBackendUrl = "http://localhost:5000";
        public static bool UseMockDataStore = true;

        public App()
        {
            InitializeComponent();

            RegisteNlog();
            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();

            MainPage = new MainPage();
        }

        private void RegisteNlog()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets
            //var consoleTarget = new ColoredConsoleTarget("target1")
            //{
            //    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}  ${exception}"
            //};
            //config.AddRuleForAllLevels(consoleTarget); // all to console
            //config.AddTarget(consoleTarget);

            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/${shortdate}.log",
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}  ${exception}"
            };
            config.AddTarget(fileTarget);

            // Step 3. Define rules
            config.AddRuleForOneLevel(LogLevel.Debug, fileTarget); // only errors to file

            // Step 4. Activate the configuration
            LogManager.Configuration = config;

            // Example usage
            Logger logger = LogManager.GetLogger("Example");
            logger.Trace("trace log message");
            logger.Debug("debug log message");
            logger.Info("info log message");
            logger.Warn("warn log message");
            logger.Error("error log message");
            logger.Fatal("fatal log message");
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
