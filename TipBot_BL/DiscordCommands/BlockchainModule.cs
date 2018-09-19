using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace TipBot_BL.DiscordCommands {
    public class BlockchainModule : ModuleBase<SocketCommandContext> {
        private static readonly string ExplorerURL = "http://explorer.myntcurrency.org:3001/";
        //  https://chainz.cryptoid.info/grs/api.dws?key=797f266685db&q=getdifficulty
        [Command("getbalance")]
        public async Task GetAddressBalance(string address) {
            var balance = await GetBalance(address);

            if (!string.IsNullOrEmpty(balance)) {
                await ReplyAsync($"The balance for the address {address} is {balance} {Preferences.BaseCurrency}");
            }
            else {
                await ReplyAsync($"Error getting the balance for the address {address}");
            }
        }
        [Command("devfunds")]
        public async Task GetDevelopmentFunds() {
            var devaddress = "MVT2AJDK7CYTtWo5fX9u48eQT5ynWPyFFd";
            var balance = await GetBalance(devaddress);
            if (!string.IsNullOrEmpty(balance)) {
                await ReplyAsync($"The development funds (`{devaddress}`) are currently at {balance} {Preferences.BaseCurrency}");
            }
            else {
                await ReplyAsync($"Error getting the balance for the address {devaddress}");
            }
        }

        [Command("supply")]
        public async Task GetCirculatingSupply() {
            var supply = await GetSupply();
            if (!string.IsNullOrEmpty(supply)) {
                try {
                    await ReplyAsync($"The circulating supply is `{decimal.Parse(supply):N0}` {Preferences.BaseCurrency}");
                }
                catch {
                    await ReplyAsync("Error getting circulating supply");
                }
            }
            else {
                await ReplyAsync("Error getting circulating supply");
            }
        }

        public static async Task<string> GetBalance(string address) {
            using (var httpClient = new HttpClient()) {
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}ext/getbalance/{address}"));
                return res;
            }
        }

        public static async Task<string> GetSupply() {
            using (var httpClient = new HttpClient()) {
                var res = await httpClient.GetStringAsync(new Uri($"{ExplorerURL}ext/getmoneysupply"));
                return res;
            }
        }



    }
}
