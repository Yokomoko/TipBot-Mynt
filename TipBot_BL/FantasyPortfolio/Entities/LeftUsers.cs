using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TipBot_BL.DiscordCommands;

namespace TipBot_BL.FantasyPortfolio {
    public partial class LeftUsers {
        public static void AddUser(string userId, DateTime time, string guildId) {
            using (var context = new FantasyPortfolio_DBEntities()) {
                var user = context.LeftUsers.FirstOrDefault(d => d.UserId == userId);

                if (user == null) {
                    user = new LeftUsers();
                    user.UserId = userId;
                    user.GuildId = guildId;
                    context.LeftUsers.Add(user);
                }
                user.TimeLeft = time;
                context.SaveChanges();
            }
        }

        public static void RemoveUser(string userId) {
            using (var context = new FantasyPortfolio_DBEntities()){
                var user = context.LeftUsers.FirstOrDefault(d => d.UserId == userId);
                if (user != null){
                    context.LeftUsers.Remove(user);
                    context.SaveChanges();
                }
            }
        }

    }
}
