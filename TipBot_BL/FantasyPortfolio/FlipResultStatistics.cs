//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TipBot_BL.FantasyPortfolio
{
    using System;
    using System.Collections.Generic;
    
    public partial class FlipResultStatistics
    {
        public int id { get; set; }
        public Nullable<int> TotalFlips { get; set; }
        public Nullable<int> Wins { get; set; }
        public Nullable<int> Losses { get; set; }
        public Nullable<decimal> WinPercentage { get; set; }
        public Nullable<decimal> LossPercentage { get; set; }
        public Nullable<decimal> TotalFlipped { get; set; }
        public Nullable<decimal> PaidOut { get; set; }
        public Nullable<decimal> PaidIn { get; set; }
        public Nullable<int> HeadFlips { get; set; }
        public Nullable<int> TailFlips { get; set; }
    }
}
