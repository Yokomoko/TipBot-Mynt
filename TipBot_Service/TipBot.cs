﻿using System.ServiceProcess;
using System.Threading;
using TipBot_BL;

namespace TipBot_Service {
    public partial class TipBot : ServiceBase {
        public TipBot() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            Thread thread = new Thread(StartService);
            thread.Start();
        }

        protected override void OnStop() {

        }

        private void StartService() {
            var discordClient = new DiscordClientNew();
        //    System.Threading.Thread.Sleep(120);
            discordClient.RunBotAsync();
            

        }
    }
}
