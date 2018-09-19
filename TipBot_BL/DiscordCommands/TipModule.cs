using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using QRCoder;
using TipBot_BL.FantasyPortfolio;
using TipBot_BL.QT;
using Color = System.Drawing.Color;

namespace TipBot_BL.DiscordCommands {
    public class TipModule : ModuleBase<SocketCommandContext> {
        //private const decimal MinimumTxnValue = (decimal)0.0001;
        private const decimal MinimumTipValue = (decimal)0.00000001;
        private const decimal MinBetAmount = (decimal)(0.01);
        public static decimal BetWin = (decimal)1.96;
        // Let it be thread-safe
        private static readonly ThreadLocal<Random> s_Gen = new ThreadLocal<Random>(
            () => new Random());

        // Thread-safe non-skewed generator
        public static Random Generator => s_Gen.Value;

        private bool CanRunTipCommands => Context.Channel.Id == Preferences.TipBotChannel || Context.Channel.Id == Preferences.FantasyChannel;
        public enum CoinSide {
            heads = 0,
            tails = 1
        }

        public static string FirstCharToUpper(string input) {
            if (string.IsNullOrEmpty(input)) {
                Console.WriteLine("Empty String");
            }
            return input?.First().ToString().ToUpper() + input?.Substring(1);
        }

        [Command("optallin")]
        public async Task OptAllIn() {
            if (DiscordClientNew._client != null) {
                var id = DiscordClientNew._client.Guilds.FirstOrDefault(d => Context.Guild != null && d.Id == Context.Guild.Id)?.Users.FirstOrDefault(d => d.Id == Context.User.Id);

                if (id != null) {
                    if (!id.GuildPermissions.Administrator) {
                        return;
                    }
                }
            }
            var ra = Context.Guild.Users.Where(d => d.Status != UserStatus.Offline && !d.IsBot);

            var optInCount = 0;

            foreach (var socketGuildUser in ra) {
                try {
                    if (!socketGuildUser.Roles.Contains(Context.Guild.Roles.FirstOrDefault(d => d.Name == "Rains"))) {
                        Context.Guild.Users.FirstOrDefault(d => d.Id == socketGuildUser.Id)?.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(d => d.Name == "Rains"));
                        //await socketGuildUser.SendMessageAsync("hello");
                        await socketGuildUser.GetOrCreateDMChannelAsync(RequestOptions.Default).Result.SendMessageAsync($"You've been automatically opted in to receive free {Preferences.BaseCurrency}! To opt out, write `-optout` in the Mynt Discord Tipbot channel");
                        Console.WriteLine($"Opted in {socketGuildUser.Username}");
                        optInCount++;
                    }
                }
                catch {
                    Console.WriteLine("Unable to send");
                }
            }
            if (optInCount > 0) {
                await ReplyAsync($"Opted {optInCount} users in");
            }
            else {
                await ReplyAsync("No more users to opt in.");
            }
        }

        [Command("invites")]
        public async Task Invites(SocketUser user = null) {
            if (user == null) {
                user = Context.Guild.GetUser(Context.User.Id);
            }
            var invites = Context.Guild.GetInvitesAsync().Result.Where(d => d.Inviter.Id == user.Id);

            int invCount = 0;
            var restInviteMetadatas = invites.ToList();
            if (restInviteMetadatas.Any()) {
                foreach (var list in restInviteMetadatas) {
                    invCount += list.Uses;
                }
            }
            await ReplyAsync($"{user.Username} has invited {invCount} {(invCount > 1 ? "people" : "person")}");
        }

        [Command("optin")]
        public async Task OptIn() {

            if (CanRunTipCommands) {
                var role = Context.Guild.Roles.FirstOrDefault(d => d.Name == "Rains");

                if (role == null) {
                    await ReplyAsync($"Unable to find role called 'Rains'. Please ensure the role exists");
                    return;
                }
                var addRoleComm = Context.Guild.Users.FirstOrDefault(d => d == Context.User)?.AddRoleAsync(role);

                var waitCount = 0;
                while (!addRoleComm.IsCompleted && waitCount < 3) {
                    Thread.Sleep(500);
                    waitCount++;
                }
                if (addRoleComm.Status != TaskStatus.RanToCompletion) {
                    await ReplyAsync($"Unable to opt you in, I couldn't change the roles. Please check my permissions and role position.");
                }
                else {
                    await ReplyAsync($"You have opted in to rains! You now have a chance to receive free {Preferences.BaseCurrency}");
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }

        [Command("flipstats")]
        public async Task FlipStats(){
            var statistics = FlipResults.GetStatistics();
            var embed = new EmbedBuilder();
            embed.WithTitle("Flip Statistics");

            embed.AddInlineField("Total Flips", statistics.TotalFlips);
            embed.AddInlineField("Wins", statistics.Wins);
            embed.AddInlineField("Losses", statistics.Losses);
            
            embed.AddInlineField("Total Flipped", $"{Math.Round((decimal) statistics.TotalFlipped, 2)} {Preferences.BaseCurrency}");
            embed.AddInlineField("User Winnings", $"{Math.Round((decimal) statistics.PaidOut,2)} {Preferences.BaseCurrency}");
            embed.AddInlineField("Bot Winnings", $"{Math.Round((decimal) statistics.PaidIn, 2)} {Preferences.BaseCurrency}");

            embed.AddInlineField("Win Percentage", Math.Round((decimal)statistics.WinPercentage, 2) * 100 + "%");
            embed.AddInlineField("Head Flips", statistics.HeadFlips);
            embed.AddInlineField("Tail Flips", statistics.TailFlips);

            await ReplyAsync("", false, embed);
        }

        [Command("optout")]
        public async Task OptOut() {
            if (CanRunTipCommands) {
                var role = Context.Guild.Roles.FirstOrDefault(d => d.Name == "Rains");

                if (role == null) {
                    await ReplyAsync($"Unable to find role called 'Rains'. Please ensure the role exists");
                    return;
                }
                var removeRoleComm = Context.Guild.Users.FirstOrDefault(d => d == Context.User)?.RemoveRoleAsync(role);

                var waitCount = 0;
                while (removeRoleComm != null && (!removeRoleComm.IsCompleted && waitCount < 3)) {
                    Thread.Sleep(500);
                    waitCount++;
                }
                if (removeRoleComm.Status != TaskStatus.RanToCompletion) {
                    await ReplyAsync($"Unable to opt you out, I couldn't change the roles. Please check my permissions and role position.");
                }
                else {
                    await ReplyAsync($"You have opted out of rains :(");
                }

            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }

        }



        [Command("address")]
        public async Task GetAddressAsync(string addr = "") {

            if (CanRunTipCommands) {
                var result = QTCommands.GetAddress(Context.User.Id);
                if (addr == "qr") {
                    QRCodeGenerator qrgen = new QRCodeGenerator();

                    QRCodeData data = qrgen.CreateQrCode($"{result}", QRCodeGenerator.ECCLevel.Q);
                    var qrcode = new Base64QRCode(data);

                    byte[] bytes = Convert.FromBase64String(qrcode.GetGraphic(4, Color.White, Color.FromArgb(54, 57, 62)));
                    MemoryStream ms = new MemoryStream(bytes);
                    await Context.Channel.SendFileAsync(ms, "qraddr.png", $"`Your deposit address is {result}`");
                }
                else {
                    var b = new EmbedBuilder();
                    b.WithDescription($"Your deposit address is: **{result}**");
                    b.WithFooter($"{Context.Guild.CurrentUser.Nickname} - Developed by Yokomoko");
                    await ReplyAsync("", false, b);

                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }

        [Command("balance")]
        public async Task GetBalanceAsync() {
            if (CanRunTipCommands) {
                var result = QTCommands.GetBalance(Context.User.Id);
                await ReplyAsync($"Your balance is {result.Result} {Preferences.BaseCurrency}");
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }

        [Command("withdraw")]
        public async Task WithdrawAsync(string address) {
            if (CanRunTipCommands) {
                var resp = QTCommands.Withdraw(Context.User.Id, address);
                if (string.IsNullOrEmpty(resp.Error)) {
                    await ReplyAsync($"Withdrawn successfully! Transaction: {Preferences.ExplorerPrefix}{resp.Result}{Preferences.ExplorerSuffix}");
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }

        }

        [Command("tip")]
        public async Task Tip(SocketUser user, string amount) {
            if (CanRunTipCommands) {
                if (decimal.TryParse(amount, out var decAmount)) {

                    if (decAmount < MinimumTipValue) {
                        await ReplyAsync($"Minimum tip amount is {MinimumTipValue}");
                        return;
                    }

                    if (QTCommands.CheckBalance(Context.User.Id, decAmount)) {
                        QTCommands.SendTip(Context.User.Id, user.Id, decAmount);
                        await ReplyAsync($"{Context.User.Mention} tipped {user.Username} {amount} {Preferences.BaseCurrency}");
                    }
                    else {
                        await ReplyAsync("You do not have enough balance to tip that much!");
                    }
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }

        [Command("tip")]
        public async Task Tip(string amount, SocketUser user) {
            if (CanRunTipCommands) {
                if (decimal.TryParse(amount, out var decAmount)) {

                    if (decAmount < MinimumTipValue) {
                        await ReplyAsync($"Minimum tip amount is {MinimumTipValue}");
                        return;
                    }

                    if (QTCommands.CheckBalance(Context.User.Id, decAmount)) {
                        QTCommands.SendTip(Context.User.Id, user.Id, decAmount);
                        await ReplyAsync($"{Context.User.Mention} tipped {user.Username} {amount} {Preferences.BaseCurrency}");
                    }
                    else {
                        await ReplyAsync("You do not have enough balance to tip that much!");
                    }
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }

        [Command("join")]
        public async Task Join() {
            if (FantasyPortfolioModule.EntryFee == 0 || QTCommands.CheckBalance(Context.User.Id, FantasyPortfolioModule.EntryFee)) {
                if (FantasyPortfolioModule.EntryFee > 0) QTCommands.SendTip(Context.User.Id, DiscordClientNew._client.CurrentUser.Id, FantasyPortfolioModule.EntryFee);
                if (Portfolio.Join(Context.User.Id.ToString())) {
                    await ReplyAsync($"You have joined round {Round.CurrentRound}. An entry fee of {FantasyPortfolioModule.EntryFee} {Preferences.BaseCurrency} has been taken.");
                }
                else {
                    await ReplyAsync("You have already joined this round!");
                }
            }
            else {
                await ReplyAsync($"You do not have the required entry fee of {FantasyPortfolioModule.EntryFee} {Preferences.BaseCurrency}. Please send some {Preferences.BaseCurrency} to your tipping account and try again.");
            }
        }


        [Command("giftrandom")]
        public async Task Rain(string amount, string numberOfPeople) {
            if (CanRunTipCommands) {
                if (int.TryParse(numberOfPeople, out var people)) {
                    var role = Context.Guild.Roles.FirstOrDefault(d => d.Name == "Rains");
                    var ra = Context.Guild.Users.Where(d => d.Roles.Contains(role));

                    var selectedUsers = ra.OrderBy(arg => Guid.NewGuid()).Where(d => d.Id != Context.User.Id).Take(people).ToList();
                    if (selectedUsers.Count >= people) {
                        if (decimal.TryParse(amount, out var rainAmount)) {
                            await ReplyAsync(SendRain(Context.User, selectedUsers, rainAmount));
                        }
                        else {
                            await ReplyAsync($"{amount} is not a valid amount to rain.");
                        }
                    }
                    else {
                        await ReplyAsync($"Not enough people to rain ({selectedUsers.Count}/{people})");
                    }
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }

        }

        [Command("rain")]
        public async Task Rain(string amount) {
            if (CanRunTipCommands) {
                var role = Context.Guild.Roles.FirstOrDefault(d => d.Name == "Rains");
                var ra = Context.Guild.Users.Where(d => d.Roles.Contains(role));

                int people;
                var selectedUsers = ra.OrderBy(arg => Guid.NewGuid()).Where(d => d.Id != Context.User.Id).ToList();

                decimal tipPeople = selectedUsers.Count / (decimal)10;

                if (tipPeople > 1) {
                    people = (int)Math.Round(tipPeople, 0);
                }
                else {
                    people = new Random().Next(1, 5);
                }
                selectedUsers = selectedUsers.Take(people).ToList();

                if (selectedUsers.Count >= people) {
                    if (decimal.TryParse(amount, out var rainAmount)) {
                        await ReplyAsync(SendRain(Context.User, selectedUsers, rainAmount));
                    }
                    else {
                        await ReplyAsync($"{amount} is not a valid amount to rain.");
                    }
                }
                else {
                    await ReplyAsync($"Not enough people to rain ({selectedUsers.Count}/{people})");
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }

        }

        [Command("house")]
        public async Task GetHouseBalance() {
            if (CanRunTipCommands) {
                await ReplyAsync($"The house balance is at {QTCommands.GetBalance(DiscordClientNew._client.CurrentUser.Id).Result} {Preferences.BaseCurrency}");
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }

        [Command("houseaddress")]
        public async Task GetHouseAddress(string qr = "") {
            if (CanRunTipCommands) {
                var houseAddr = QTCommands.GetAccountAddress(Context.Guild.CurrentUser.Id);

                if (qr.ToLower() == "qr") {
                    QRCodeGenerator qrgen = new QRCodeGenerator();
                    QRCodeData data = qrgen.CreateQrCode($"{houseAddr}", QRCodeGenerator.ECCLevel.Q);
                    var qrcode = new Base64QRCode(data);

                    byte[] bytes = Convert.FromBase64String(qrcode.GetGraphic(4, Color.White, Color.FromArgb(54, 57, 62), null));

                    MemoryStream ms = new MemoryStream(bytes);
                    await Context.Channel.SendFileAsync(ms, "qraddr.png", $"`The house deposit address is {houseAddr}`");
                }
                else {
                    EmbedBuilder b = new EmbedBuilder();
                    b.WithTitle(Context.Guild.CurrentUser.Username);
                    b.AddField("Address", houseAddr);
                    b.AddField("Developer", "Yokomoko");
                    //b.AddField("Developer Donations", "MVT2AJDK7CYTtWo5fX9u48eQT5ynWPyFFd");
                    b.WithColor(Discord.Color.Blue);
                    b.WithFooter("Payments to these addresses will help fund hosting, and support giveaway events.");
                    await ReplyAsync("", false, b);
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }

        [Command("flip")]
        public async Task Flip(string side, string amount) {
            if (CanRunTipCommands) {
                if (Enum.TryParse(side.ToLower(), out CoinSide coinSide)) {
                    if (decimal.TryParse(amount, out var betAmount)) {
                        if (QTCommands.CheckBalance(Context.User.Id, betAmount)) {
                            if (betAmount < MinBetAmount) {
                                await ReplyAsync($"Minimum bet {MinBetAmount} {Preferences.BaseCurrency}");
                                return;
                            }

                            var rewardValue = betAmount * (decimal)BetWin;
                            if (QTCommands.CheckBalance(DiscordClientNew._client.CurrentUser.Id, rewardValue)) {
                                QTCommands.SendTip(Context.User.Id, DiscordClientNew._client.CurrentUser.Id, betAmount);
                                try {
                                    var coin = (CoinSide)Generator.Next(0, 2);

                                    var embed = new EmbedBuilder();

                                    string message;

                                    if (coin == coinSide) {
                                        QTCommands.SendTip(DiscordClientNew._client.CurrentUser.Id, Context.User.Id, rewardValue);
                                        embed.AddInlineField("Flipped", FirstCharToUpper(coin.ToString()));
                                        embed.AddInlineField("Prize", $"{rewardValue} {Preferences.BaseCurrency}");
                                        embed.AddInlineField("Profit", $"{(rewardValue - betAmount)} {Preferences.BaseCurrency}");
                                        embed.WithColor(Discord.Color.Green);
                                        message = $"You won! Congratulations {Context.User.Mention}!";
                                    }
                                    else {
                                        embed.AddInlineField("Flipped", FirstCharToUpper(coin.ToString()));
                                        embed.AddInlineField("Lost", $"{betAmount} {Preferences.BaseCurrency}");
                                        embed.WithColor(Discord.Color.Red);
                                        message = $"Unlucky {Context.User.Mention}, you lost :(";
                                    }
                                    embed.WithFooter("Developed by Yokomoko (MVT2AJDK7CYTtWo5fX9u48eQT5ynWPyFFd)");
                                    await ReplyAsync(message, false, embed);

                                    using (var context = new FantasyPortfolio_DBEntities()) {
                                        var result = new FlipResults();
                                        result.DateTime = DateTime.Now;
                                        result.UserId = Context.User.Id.ToString();
                                        result.FlipResult = (byte)coin;
                                        result.UserFlip = (byte)coinSide;
                                        result.FlipValue = betAmount;
                                        context.FlipResults.Add(result);
                                        context.SaveChanges();
                                    }

                                    Console.WriteLine($"{Context.User.Id} ({Context.User.Username}) bet on {side} and flipped {coin}");
                                }
                                catch (Exception e) {
                                    Console.WriteLine(e.Message);
                                    QTCommands.SendTip(DiscordClientNew._client.CurrentUser.Id, Context.User.Id, betAmount);
                                    await ReplyAsync("Sorry something went wrong. You have been refunded your bet.");
                                }

                            }
                            else {
                                await ReplyAsync("Sorry, the bot is too poor to reward you if you won :(");
                            }
                        }
                        else {
                            await ReplyAsync("You do not have enough balance to perform this action");
                        }
                    }
                }
            }
            else {
                await ReplyAsync($"Please use the <#{Preferences.TipBotChannel}> channel");
            }
        }


        private string SendRain(SocketUser fromUser, List<SocketGuildUser> tipUsers, decimal amount) {
            if (QTCommands.CheckBalance(fromUser.Id, amount)) {
                if (amount / tipUsers.Count < (decimal)0.01) {
                    return $"Rain amount must be at least 0.01 {Preferences.BaseCurrency} per person";
                }
                foreach (var person in tipUsers) {
                    QTCommands.SendTip(Context.User.Id, person.Id, Math.Round(amount / tipUsers.Count, 7));
                }
                var mentionList = new List<string>();
                foreach (var users in tipUsers) {
                    mentionList.Add($"{DiscordClientNew._client.GetUser(users.Id).Mention}");
                }
                return $"{fromUser.Mention} made it rain :cloud_rain:! Congratulations to {(mentionList.Count == 2 ? string.Join(" and ", mentionList) : string.Join(", ", mentionList))} who {(mentionList.Count > 1 ? "have" : "has")} been awarded {Math.Round(amount / mentionList.Count, 7)} {Preferences.BaseCurrency} {(mentionList.Count > 1 ? "each" : "")}";
            }
            return "You do not have enough balance to perform this rain";
        }
    }
}