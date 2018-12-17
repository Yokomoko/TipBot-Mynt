using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace TipBot_BL.DiscordCommands{
    public class BlockchainModule : ModuleBase<SocketCommandContext>{
        private static readonly string ExplorerURL = "https://chain.myntcurrency.org/";

        //  https://chainz.cryptoid.info/grs/api.dws?key=797f266685db&q=getdifficulty
        [Command("getbalance")]
        public async Task GetAddressBalance(string address){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
            }
            var balance = await GetBalance(address);

            if (!string.IsNullOrEmpty(balance)){
                await ReplyAsync($"The balance for the address {address} is {balance} {Preferences.BaseCurrency}");
            }
            else{
                await ReplyAsync($"Error getting the balance for the address {address}");
            }
        }

        [Command("devfunds")]
        public async Task GetDevelopmentFunds(){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
            }
            var devaddress = "MVT2AJDK7CYTtWo5fX9u48eQT5ynWPyFFd";
            var balance = await GetBalance(devaddress);
            if (!string.IsNullOrEmpty(balance)){
                await ReplyAsync($"The development funds (`{devaddress}`) are currently at {balance} {Preferences.BaseCurrency}");
            }
            else{
                await ReplyAsync($"Error getting the balance for the address {devaddress}");
            }
        }

        [Command("supply")]
        public async Task GetCirculatingSupply(){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
            }
            var supply = await GetSupply();
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

        [Command("network")]
        public async Task GetNetwork(){
            if (!PingExplorerURL()){
                await ReplyAsync($"Error Contacting {ExplorerURL}");
            }

            //Run all tasks at once
            var difficultyTask = GetDifficulty();
            var connectionCountTask = GetConnectionCount();
            var blockCountTask = GetBlockCount();
            var distributionTask = GetDistribution();
            var hashTask = GetNetworkHash();
            var embed = new EmbedBuilder();

            //Get the results of the tasks now that they've all run
            var difficulty = await difficultyTask;
            var connectionCount = await connectionCountTask;
            var blockCount = await blockCountTask;
            var distribution = await distributionTask;
            var hash = await hashTask;

            var sb = new StringBuilder();
            sb.AppendLine($"**Supply**: {distribution.supply:N2}");
            sb.AppendLine($"**Top 1-25**: {distribution.t_1_25.total:N2} ({distribution.t_1_25.percent:N2}%)");
            sb.AppendLine($"**Top 26-50**: {distribution.t_26_50.total:N2} ({distribution.t_26_50.percent:N2}%)");
            sb.AppendLine($"**Top 51-75**: {distribution.t_51_75.total:N2} ({distribution.t_51_75.percent:N2}%)");
            sb.AppendLine($"**Top 76-100**: {distribution.t_76_100.total:N2} ({distribution.t_76_100.percent:N2}%)");
            sb.AppendLine($"**101+**: {distribution.t_101plus.total:N2} ({distribution.t_101plus.percent:N2}%)");

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

        [Command("distribution")]
        public async Task Distribution(){
            var embed = new EmbedBuilder();
            var distribution = await GetDistribution();

            embed.AddInlineField("Supply", $"{distribution.supply:N2}");
            embed.AddInlineField("Top 1-25", $"{distribution.t_1_25.total:N2} ({distribution.t_1_25.percent:N2}%)");
            embed.AddInlineField("Top 26-50", $"{distribution.t_26_50.total:N2} ({distribution.t_26_50.percent:N2}%)");
            embed.AddInlineField("Top 51-75", $"{distribution.t_51_75.total:N2} ({distribution.t_51_75.percent:N2}%)");
            embed.AddInlineField("Top 76-100", $"{distribution.t_76_100.total:N2} ({distribution.t_76_100.percent:N2}%)");
            embed.AddInlineField("101+", $"{distribution.t_101plus.total:N2} ({distribution.t_101plus.percent:N2}%)");

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
            var ping = new System.Net.NetworkInformation.Ping();
            var result = ping.Send(url.Host);
            return result?.Status == IPStatus.Success;
        }
    }

    public class T125{
        public decimal percent { get; set; }
        public decimal total { get; set; }
    }

    public class T2650{
        public decimal percent { get; set; }
        public decimal total { get; set; }
    }

    public class T5175{
        public decimal percent { get; set; }
        public decimal total { get; set; }
    }

    public class T76100{
        public decimal percent { get; set; }
        public decimal total { get; set; }
    }

    public class T101plus{
        public decimal percent { get; set; }
        public decimal total { get; set; }
    }

    public class SupplyRoot{
        public double supply { get; set; }
        public T125 t_1_25 { get; set; }
        public T2650 t_26_50 { get; set; }
        public T5175 t_51_75 { get; set; }
        public T76100 t_76_100 { get; set; }
        public T101plus t_101plus { get; set; }
    }
}