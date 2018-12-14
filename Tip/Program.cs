using System;
using System.Linq;
using Discord;
using TipBot_BL;

namespace Tip{
    class Program{
        static DiscordClientNew DiscordClient = new DiscordClientNew();

        static void Main(){
            DiscordClient.RunBotAsync();
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            Console.ReadLine();
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e){
            if (e.IsTerminating){
                Console.WriteLine(e.ExceptionObject.ToString());
                Console.ReadLine();
                Environment.Exit(1);
            }
            else{
                Console.WriteLine(e.ExceptionObject.ToString());
                try{
                    if (DiscordClientNew._client?.ConnectionState == ConnectionState.Disconnected){
                        DiscordClient.RunBotAsync();
                    }
                }
                catch{
                    //Do Nothing
                }
            }
        }
    }
}