using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TipBot_BL.FantasyPortfolio;

namespace TipBot_BL.DiscordCommands {
    public class TickerModule : ModuleBase<SocketCommandContext> {
        public static CoinMarketCap.CoinMarketCapClient priceClientNew = new CoinMarketCap.CoinMarketCapClient();

        [Command("price")]
        public async Task GetPrice(string ticker) {
            if (Context.Channel.Id != Preferences.PriceCheckChannel) {
                await ReplyAsync($"Please use the <#{Preferences.PriceCheckChannel}> channel!");
                return;
            }
            var embed = await GetPriceEmbed(ticker);
            await ReplyAsync("", false, embed);
        }

        public async Task<Embed> GetPriceEmbed(string ticker) {
            var tickerFormatted = ticker.ToUpper().Trim();
            long? tickerId;

            using (var context = new FantasyPortfolio_DBEntities()) {
                var coin = context.Coins.FirstOrDefault(d => d.TickerName.ToUpper().Trim() == tickerFormatted);
                if (coin == null) {
                    var listings = await priceClientNew.GetListingsAsync();
                    var listingSingle = listings.Data.FirstOrDefault(d => d.Symbol == tickerFormatted);

                    if (listingSingle != null) {
                        coin = new Coin {
                            TickerId = (int)listingSingle.Id,
                            TickerName = listingSingle.Symbol,
                            LastUpdated = new DateTime(2000, 1, 1)
                        };
                        context.Coins.Add(coin);
                        context.SaveChanges();
                    }
                }
                tickerId = coin?.TickerId;
            }
            if (tickerId != null) {
                var tickerResponse = await priceClientNew.GetTickerAsync((int)tickerId, "BTC");
                var emb = new EmbedBuilder();
                emb.WithTitle($"Price of {tickerResponse.Data.Name} [{tickerFormatted}]");
                var sb = new StringBuilder();
                sb.AppendLine($"**Rank:** {tickerResponse.Data.Rank}");
                sb.Append(Environment.NewLine);

                foreach (var quote in tickerResponse.Data.Quotes) {
                    if (quote.Key == "USD") {
                        sb.AppendLine("**Price " + quote.Key + ":** " + "$" + decimal.Parse(Math.Round(quote.Value.Price.GetValueOrDefault(0), 5).ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint));
                    }
                    else {
                        sb.AppendLine("**Price " + quote.Key + ":** " + decimal.Parse(Math.Round(quote.Value.Price.GetValueOrDefault(0), 8).ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent) + " " + quote.Key);
                    }
                }

                sb.Append(Environment.NewLine);
                sb.AppendLine($"**Market Cap: **${tickerResponse.Data.Quotes.FirstOrDefault(d => d.Key == "USD").Value.MarketCap:n}");
                sb.AppendLine($"**24h volume: **${tickerResponse.Data.Quotes.FirstOrDefault(d => d.Key == "USD").Value.Volume24H:n}");
                sb.AppendLine($"**Supply: **{tickerResponse.Data.TotalSupply:n}");
                sb.Append(Environment.NewLine);
                sb.AppendLine($"**Change 1h: **{tickerResponse.Data.Quotes.FirstOrDefault(d => d.Key == "USD").Value.PercentChange1H:n}%");
                sb.AppendLine($"**Change 24h: **{tickerResponse.Data.Quotes.FirstOrDefault(d => d.Key == "USD").Value.PercentChange24H:n}%");
                sb.AppendLine($"**Change 7 days: **{tickerResponse.Data.Quotes.FirstOrDefault(d => d.Key == "USD").Value.PercentChange7D:n}%");

                emb.WithDescription(sb.ToString());
                try {
                    emb.WithUrl($"https://coinmarketcap.com/currencies/{tickerResponse.Data.WebsiteSlug}/");
                }
                catch {
                    // ignored
                }

                emb.ThumbnailUrl = $"https://s2.coinmarketcap.com/static/img/coins/32x32/{tickerId}.png";

                emb.WithFooter("MyntTip Price Checker | By Yokomoko");
                return emb;
            }
            return null;
        }



    }
}