using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TipBot_BL.DiscordCommands;

namespace TipBot_BL.FantasyPortfolio {
    public partial class FlipResults {

        //public static GetFlipboard() {
        //    using (var context = new FantasyPortfolio_DBEntities()) {
        //        context.
        //    }
        //}
        public static List<FlipLeaderboard> GetLeaderboardBySpend() {
            using (var context = new FantasyPortfolio_DBEntities()) {
                return context.FlipLeaderboard.OrderByDescending(u => u.TotalBet.Value).Take(10).ToList();
            }
        }

        public static List<FlipLeaderboard> GetLeaderboardByWins() {
            using (var context = new FantasyPortfolio_DBEntities()) {
                return context.FlipLeaderboard.OrderByDescending(u => u.TotalWins.Value).Take(10).ToList();
            }
        }

        public static FlipResultStatistics GetStatistics() {
            using (var context = new FantasyPortfolio_DBEntities()) {
                return context.FlipResultStatistics.FirstOrDefault();
            }
        }
    }
}
