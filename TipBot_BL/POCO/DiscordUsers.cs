using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using LiteDB;
using TipBot_BL.Properties;

namespace TipBot_BL.POCO {

    public class DiscordUsers : ModuleBase<SocketCommandContext> {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public DateTime? LastReceived { get; set; }
        public DateTime? LastSent { get; set; }
        public bool? RainOptIn { get; set; }


    }
}
