using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using TipBot_BL;

namespace TipBot_Service {
    public partial class TipBot : ServiceBase {
        public TipBot() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
           DiscordClientNew.WriteToFile("Service Starting");
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            try
            {
                Thread thread = new Thread(StartService);
                thread.Start();
            }
            catch (Exception e){
                DiscordClientNew.WriteToFile(e.Message + Environment.NewLine + Environment.NewLine + e.InnerException);
            }
            
            
        }

        protected override void OnStop() {
            DiscordClientNew.WriteToFile($"Service Ending");
        }

        private void StartService() {
            try{
                var discordClient = new DiscordClientNew();
                //    System.Threading.Thread.Sleep(120);
                discordClient.RunBotAsync();
            }
            catch (Exception e){
                DiscordClientNew.WriteToFile(e.Message + Environment.NewLine + Environment.NewLine + e.InnerException);
            }
            
            Thread.Sleep(-1);
        }


        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                DiscordClientNew.WriteToFile(e.ExceptionObject.ToString());
            }
            DiscordClientNew.WriteToFile(e.ExceptionObject.ToString());
        }
    }
}
