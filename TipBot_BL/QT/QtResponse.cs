using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace TipBot_BL.QT {
    public class QtResponses {
        public List<QtResponse> Responses { get; set; }
    }

    public class QtResponse {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }

    public class QTCommands {

        public static bool CheckBalance(ulong userId, decimal amount) {
            try {
                if (decimal.Parse(GetBalance(userId).Result, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent) >= amount) {
                    return true;
                }
            }
            catch {
                return false;
            }
            return false;
        }

        public static string SendTip(ulong fromuserId, ulong touserId, decimal amount) {
            if (decimal.Parse(GetBalance(fromuserId).Result) >= amount) {
                var obj = GroestlJson.TipBotRequest("move", new List<string> { fromuserId.ToString(), touserId.ToString(), amount.ToString() });
                return $"{JsonConvert.DeserializeObject<QtResponse>(obj).Result}";
            }
            return "Not enough funds";
        }

        public static QtResponse Withdraw(ulong userId, string address) {
            var obj = GroestlJson.TipBotRequest("sendfrom", new List<string> { userId.ToString(), address, (decimal.Parse(GetBalance(userId).Result, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent) - (decimal)0.00015000).ToString() });
            var response = JsonConvert.DeserializeObject<QtResponse>(obj);
            return response;
        }

        public static string GetAccountAddress(ulong userId) {
            var obj = GroestlJson.TipBotRequest("getaccountaddress", new List<string> { userId.ToString() });
            return $"{JsonConvert.DeserializeObject<QtResponse>(obj).Result}";
        }

        public static QtResponse GetBalance(ulong userId) {
            try {
                var obj = GroestlJson.TipBotRequest("getbalance", new List<string> { userId.ToString() });
                var response = JsonConvert.DeserializeObject<QtResponse>(obj);
                if (!string.IsNullOrEmpty(response.Result)) {
                    response.Result = decimal.Parse(response.Result, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent).ToString();
                }
                return response;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return null;
            }
        }



        public static string Rain(ulong userId, List<ulong> users, int amount, int numberOfPeople = 5) {
            var sb = new StringBuilder();

            foreach (var user in users) {
                GroestlJson.TipBotRequest("sendfrom", new List<string> { userId.ToString(), GetAddress(user), (amount / numberOfPeople).ToString() });
            }

            sb.AppendLine($"{String.Join(", ", users)}, congratulations! You have been awarded {0} Mynt");
            return sb.ToString();
        }


        public static string GetAddress(ulong userId) {
            try {
                var obj = GroestlJson.TipBotRequest("getaccountaddress", new List<string> { userId.ToString() });
                return JsonConvert.DeserializeObject<QtResponse>(obj).Result;
            }
            catch {
                return "Error getting your wallet. Please Contact Yokomoko.";
            }
        }
    }
}
