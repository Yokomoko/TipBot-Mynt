using System;
using System.Globalization;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using TipBot_BL.QT;

namespace TipBot_BL.DiscordCommands{
    public class BlockchainModule : ModuleBase<SocketCommandContext>{
        private static readonly string ExplorerURL = "https://chain.myntcurrency.org/";
        private static int TimeoutSeconds = 2;
        private readonly Task<string> TimeoutTaskStr = Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds)).ContinueWith(_ => string.Empty);
        private readonly Task<SupplyRoot> TimeoutTaskSup = Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds)).ContinueWith(_ => new SupplyRoot());
        private readonly Task<decimal> TimeoutTaskDec = Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds)).ContinueWith(_ => (decimal) 0);

        //  https://chainz.cryptoid.info/grs/api.dws?key=797f266685db&q=getdifficulty
        [Command("getbalance")]
        public async Task GetAddressBalance(string address){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
                return;
            }

            var balanceTask = GetBalance(address);
            var balanceCheck = Task.WhenAny(balanceTask, TimeoutTaskStr).Result;
            if (balanceCheck == TimeoutTaskStr){
                await ReplyAsync($"Error getting the balance from explorer for the address {address}");
                return;
            }
            else{
                var balance = balanceCheck.Result;
                if (!string.IsNullOrEmpty(balance)){
                    await ReplyAsync($"The balance for the address {address} is {balance} {Preferences.BaseCurrency}");
                    return;
                }
                else{
                    await ReplyAsync($"Error getting the balance for the address {address}");
                    return;
                }
            }
        }

        [Command("devfunds")]
        public async Task GetDevelopmentFunds(){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
                return;
            }
            var devaddress = "MVT2AJDK7CYTtWo5fX9u48eQT5ynWPyFFd";

            var balanceTask = GetBalance(devaddress);
            var balanceCheck = Task.WhenAny(balanceTask, TimeoutTaskStr).Result;
            if (balanceCheck == TimeoutTaskStr){
                await ReplyAsync($"Error getting the balance from explorer for the address {devaddress}");
                return;
            }
            else{
                var balance = balanceCheck.Result;
                if (!string.IsNullOrEmpty(balance)){
                    await ReplyAsync($"The development funds (`{devaddress}`) are currently at {balance} {Preferences.BaseCurrency}");
                    return;
                }
                else{
                    await ReplyAsync($"Error getting the balance for the address {devaddress}");
                    return;
                }
            }
        }

        [Command("supply")]
        public async Task GetCirculatingSupply(){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
                return;
            }
            var supplyTask = GetSupply();
            var su = await Task.WhenAny(supplyTask, TimeoutTaskStr);

            if (su == TimeoutTaskStr){
                await ReplyAsync("Error getting circulating supply from the block explorer");
            }
            else{
                var supply = su.Result;
                if (!string.IsNullOrEmpty(supply)){
                    try{
                        await ReplyAsync($"The circulating supply is `{decimal.Parse(supply):N0}` {Preferences.BaseCurrency}");
                    }
                    catch{
                        await ReplyAsync("Error getting circulating supply");
                    }
                }
                else{
                    await ReplyAsync("Error getting circulating supply");
                }
            }
        }

        [Command("network")]
        public async Task GetNetwork(){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
                return;
            }

            //Run all tasks at once
            var difficultyTask = GetDifficulty();
            var connectionCountTask = GetConnectionCount();
            var blockCountTask = GetBlockCount();
            var distributionTask = GetDistribution();
            var hashTask = GetNetworkHash();
            var embed = new EmbedBuilder();

            var di = await Task.WhenAny(difficultyTask, TimeoutTaskDec);
            var dist = await Task.WhenAny(distributionTask, TimeoutTaskSup);
            var connCount = Task.WhenAny(connectionCountTask, TimeoutTaskStr).Result;
            var diff = Task.WhenAny(difficultyTask, TimeoutTaskDec).Result;
            var blkCount = Task.WhenAny(blockCountTask, TimeoutTaskDec).Result;
            var h = Task.WhenAny(hashTask, TimeoutTaskDec).Result;

            if (connCount == TimeoutTaskStr || diff == TimeoutTaskDec || blkCount == TimeoutTaskDec || h == TimeoutTaskDec || di == TimeoutTaskDec){
                await ReplyAsync("Failed to get Network Details from Block Explorer.");
                return;
            }

            //Get the results of the tasks now that they've all run
            var difficulty = di.Result;
            var connectionCount = connectionCountTask.Result;
            var blockCount = blockCountTask.Result;
            var distribution = dist.Result;
            var hash = hashTask.Result;

            var sb = new StringBuilder();

            if (dist != TimeoutTaskSup){
                sb.AppendLine($"**Supply**: {distribution.supply:N2}");
                sb.AppendLine($"**Top 1-25**: {distribution.t_1_25.total:N2} ({distribution.t_1_25.percent:N2}%)");
                sb.AppendLine($"**Top 26-50**: {distribution.t_26_50.total:N2} ({distribution.t_26_50.percent:N2}%)");
                sb.AppendLine($"**Top 51-75**: {distribution.t_51_75.total:N2} ({distribution.t_51_75.percent:N2}%)");
                sb.AppendLine($"**Top 76-100**: {distribution.t_76_100.total:N2} ({distribution.t_76_100.percent:N2}%)");
                sb.AppendLine($"**101+**: {distribution.t_101plus.total:N2} ({distribution.t_101plus.percent:N2}%)");
            }
            else{
                sb.AppendLine("Distribution currently unavailable");
            }

            embed.AddInlineField("Difficulty", difficulty.ToString("N2"));
            embed.AddInlineField("Connections", connectionCount);
            embed.AddInlineField("Block Height", blockCount);

            if (hash != 0){
                embed.AddInlineField("Network Hash Rate", (hash / 1000000000).ToString("N2") + " GH/s");
            }

            embed.AddField("Distribution", sb.ToString());
            embed.WithFooter(Preferences.FooterText);
            await ReplyAsync("", false, embed);
        }

        [Command("countdown")]
        public async Task GetBlockCountdown() {
            var forkHeight = 432000;
            int amountOfSeconds = 0;

            var height = QTCommands.GetBlockHeight();
            if (height != null) {
                if (height < forkHeight) {
                    var blockCountdown = forkHeight - height;
                    amountOfSeconds = (int)blockCountdown;
                    var blockCountdowntime = new TimeSpan(0, 0, 0, amountOfSeconds * 30);

                    var embed = new EmbedBuilder();

                    var days = blockCountdowntime.Days;
                    var hours = blockCountdowntime.Hours;
                    var minutes = blockCountdowntime.Minutes;
                    var seconds = blockCountdowntime.Seconds;

                    var timeString = $"The fork away from ASICs will happen in approximately {days} Days, {hours} Hours, {minutes} minutes and {seconds} seconds!";

                    var sb = new StringBuilder();
                    sb.AppendLine($"**Current Block Height:** {height}");
                    sb.AppendLine($"**Fork Height:** {forkHeight}");
                    sb.AppendLine($"**Remaining Blocks:** {blockCountdown.Value}{Environment.NewLine}");
                    sb.AppendLine(timeString);

                    embed.AddField("Fork Countdown", sb.ToString());

                    //embed.AddInlineField("Block Height", height);
                    //embed.AddInlineField("Fork Height", forkHeight);
                    //embed.AddInlineField("Block Countdown", blockCountdown.Value.ToString() + " Blocks to go");
                    //embed.AddInlineField("Block Countdown timer", "Around " + DateTime.Now.AddTicks(blockCountdowntime.Ticks).ToString(CultureInfo.InvariantCulture) + " UTC");
                    await ReplyAsync("", false, embed);
                }
            }
        }

        [Command("distribution")]
        public async Task Distribution(){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
                return;
            }
            var distributionTask = GetDistribution();
            var dist = await Task.WhenAny(distributionTask, TimeoutTaskSup);

            var embed = new EmbedBuilder();
            if (dist == TimeoutTaskSup){
                embed.AddInlineField("Error", "Failed to get distribution from block explorer");
            }
            else{
                var distribution = dist.Result;
                embed.AddInlineField("Supply", $"{distribution.supply:N2}");
                embed.AddInlineField("Top 1-25", $"{distribution.t_1_25.total:N2} ({distribution.t_1_25.percent:N2}%)");
                embed.AddInlineField("Top 26-50", $"{distribution.t_26_50.total:N2} ({distribution.t_26_50.percent:N2}%)");
                embed.AddInlineField("Top 51-75", $"{distribution.t_51_75.total:N2} ({distribution.t_51_75.percent:N2}%)");
                embed.AddInlineField("Top 76-100", $"{distribution.t_76_100.total:N2} ({distribution.t_76_100.percent:N2}%)");
                embed.AddInlineField("101+", $"{distribution.t_101plus.total:N2} ({distribution.t_101plus.percent:N2}%)");
            }
            embed.WithFooter(Preferences.FooterText);
            await ReplyAsync("", false, embed);
        }

        public static async Task<string> GetBalance(string address){
            using (var httpClient = new HttpClient()){
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}ext/getbalance/{address}"));
                return res;
            }
        }

        public static async Task<string> GetSupply(){
            using (var httpClient = new HttpClient()){
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}ext/getmoneysupply"));
                return res;
            }
        }

        public static async Task<decimal> GetDifficulty(){
            using (var httpClient = new HttpClient()){
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}api/getdifficulty"));
                decimal diffDec = (decimal) 0;
                try{
                    decimal.TryParse(res, out diffDec);
                }
                catch{
                    //Do Nothing
                }
                return diffDec;
            }
        }

        public static async Task<decimal> GetNetworkHash(){
            using (var httpClient = new HttpClient()){
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}api/getnetworkhashps"));
                decimal hashLong = (decimal) 0;
                try{
                    decimal.TryParse(res, out hashLong);
                }
                catch{
                    //Do Nothing
                }
                return hashLong;
            }
        }

        public static async Task<string> GetConnectionCount(){
            using (var httpClient = new HttpClient()){
                httpClient.Timeout = new TimeSpan(0, 0, 15);
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}api/getconnectioncount"));
                return res;
            }
        }

        public static async Task<string> GetBlockCount(){
            using (var httpClient = new HttpClient()){
                httpClient.Timeout = new TimeSpan(0, 0, 15);
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}api/getblockcount"));
                return res;
            }
        }

        public static async Task<SupplyRoot> GetDistribution(){
            using (var httpClient = new HttpClient()){
                httpClient.Timeout = new TimeSpan(0, 0, 15);
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}ext/getdistribution"));

                return JsonConvert.DeserializeObject<SupplyRoot>(res);
            }
        }

        public static bool PingExplorerURL(){
            if (!NetworkInterface.GetIsNetworkAvailable()){
                return false;
            }
            var url = new Uri(ExplorerURL);
            var ping = new Ping();
            var result = ping.Send(url.Host);
            return result?.Status == IPStatus.Success;
        }
    }

    public class T125{
        public decimal percent { get; set; } = 0;
        public decimal total { get; set; } = 0;
    }

    public class T2650{
        public decimal percent { get; set; } = 0;
        public decimal total { get; set; } = 0;
    }

    public class T5175{
        public decimal percent { get; set; } = 0;
        public decimal total { get; set; } = 0;
    }

    public class T76100{
        public decimal percent { get; set; } = 0;
        public decimal total { get; set; } = 0;
    }

    public class T101plus{
        public decimal percent { get; set; } = 0;
        public decimal total { get; set; } = 0;
    }

    public class SupplyRoot{
        public double supply { get; set; } = 0;
        public T125 t_1_25 { get; set; } = new T125();
        public T2650 t_26_50 { get; set; } = new T2650();
        public T5175 t_51_75 { get; set; } = new T5175();
        public T76100 t_76_100 { get; set; } = new T76100();
        public T101plus t_101plus { get; set; } = new T101plus();
    }
}