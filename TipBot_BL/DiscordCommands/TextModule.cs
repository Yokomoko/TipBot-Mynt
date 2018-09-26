using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Color = System.Drawing.Color;

namespace TipBot_BL.DiscordCommands {
    public class TextModule : ModuleBase<SocketCommandContext> {

        [Command("ping")]
        public async Task PingAsync() {
            await ReplyAsync("Pong!");
        }

        [Command("commands")]
        public async Task Commands() {
            var embed = new EmbedBuilder();
            embed.WithTitle("Commands for MyntBot");

            var tipSb = new StringBuilder();
            tipSb.AppendLine("**-rain:** Rains some of your funds to other users that have opted in");
            tipSb.AppendLine("**-optin:** Opts you in to receive random Mynt rains from the tip bot");
            tipSb.AppendLine("**-optout:** Opts you outof receiving random Mynt rains from the tip bot :(");
            tipSb.AppendLine("**-address:** Gets your deposit address to use the tipping functionality - Optional: add 'qr' to the end to get a QR code (-address qr)");
            tipSb.AppendLine($"**-balance:** Gets your {Preferences.BaseCurrency} balance from the bot");
            tipSb.AppendLine("**-withdraw:** Withdraws your balance. Syntax: -withdraw [address]");
            tipSb.AppendLine("**-tip:** Tip another user. Syntax: -tip [@user] [amount] OR -tip [amount] [@user]");
            tipSb.AppendLine("**-giftrandom:** Tip random people. Syntax: -giftrandom [amount] [number of people]");
            tipSb.AppendLine("**-house:** Gets the balance of the house (i.e. the balance of the bot)");
            tipSb.AppendLine("**-houseaddress:** Gets the address of the bot. Optional: add 'qr' to the end to get a QR code (-houseaddress qr)");
            tipSb.AppendLine(Environment.NewLine);
            tipSb.AppendLine("**-flip:** Simple coin flip game! Bet your Mynt on either Heads or Tails for a chance to win more! 2% house edge Syntax: -flip [heads/tails] [amount]");
            tipSb.AppendLine(Environment.NewLine);
            tipSb.AppendLine($"**$**: Price check a coin! Syntax: $[ticker], for example: ${Preferences.BaseCurrency} (Alt: -price [ticker])");
            tipSb.AppendLine(Environment.NewLine);
            tipSb.AppendLine($"**-mining** OR **-pool**: Gives you details on how to mine {Preferences.BaseCurrency}");
            tipSb.AppendLine("**-devfunds**: Shows you the value of the current development funds");
            tipSb.AppendLine("**-pool**: Show mining pool information");
            tipSb.AppendLine("**-social**: Show all social media channels");
            embed.WithDescription(tipSb.ToString());
            embed.WithFooter(Preferences.FooterText);

            await ReplyAsync("", false, embed);
        }

        [Command("pool")]
        public async Task GetPoolAddress() {
            await ReplyAsync("", false, GetPoolEmbed());
        }

        [Command("mining")]
        public async Task GetMiningMsg() {
            await ReplyAsync("", false, GetPoolEmbed());
        }

        [Command("social")]
        public async Task GetSocialLinks(){


            var embed = new EmbedBuilder();
            embed.WithTitle($"{Preferences.BaseCurrency} Social Media");
            embed.WithDescription("Please link/follow/share our social media channels!");

            embed.AddInlineField("Twitter", "http://twitter.com/myntcurrency");
            embed.AddInlineField("Reddit", "http://reddit.com/r/myntcurrency");
            embed.AddInlineField("Telegram", "http://t.me/myntofficial");
            embed.AddInlineField("Discord", "https://discord.gg/5VRBc4q");
            embed.AddField("Bitcoin Talk", "https://bitcointalk.org/index.php?topic=4973629");

            await ReplyAsync("", false, embed);
        }


        private static EmbedBuilder GetPoolEmbed() {
            var embed = new EmbedBuilder();
            embed.WithTitle($"{Preferences.BaseCurrency} Mining Pool");
            embed.WithDescription("You want to mine MYNT?\nHaving Difficulties? Head over to the <#485466190349598740> channel to get support from the developers and the community");
            embed.WithUrl("http://pool.myntcurrency.org");

            embed.AddField("MINING POOL", "http://pool.myntcurrency.org");

            var minerSb = new StringBuilder();
            minerSb.AppendLine("CPU MINER:\nhttps://bitbucket.org/myntcurrency/mynt-core/downloads/cpuminer-opt-mynt.zip");
            minerSb.AppendLine("AMD MINER:\nhttps://bitbucket.org/myntcurrency/mynt-core/downloads/sgminer-5.6.1-nicehash-51-windows-amd64-mynt.zip");
            minerSb.AppendLine("NVIDIA MINER:\nhttps://bitbucket.org/myntcurrency/mynt-core/downloads/ccminer-2.3-mynt.zip");
            minerSb.Append(Environment.NewLine);

            embed.AddField("MINER LINKS", minerSb.ToString());

            embed.AddInlineField("Difficulty 1 (SMALL)", "stratum+tcp://pool.myntcurrency.org:3008");
            embed.AddInlineField("Difficulty 2 (MED)", "stratum+tcp://pool.myntcurrency.org:3032");
            embed.AddInlineField("Difficulty 3 (LARGE)", "stratum+tcp://pool.myntcurrency.org:3256");

            embed.WithFooter(Preferences.FooterText);

            return embed;
        }

    }
}