using DarkRP_DiscordBot.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRP_DiscordBot
{
    public class Data
    {
        public static List<Job> Jobs;
        public static List<string> JobCategories = new List<string>();
        public static Dictionary<string, string> WeaponsDictionary = new Dictionary<string, string>();

        public static List<Shipment> Shipments;
        public static List<string> ShipmentCategories = new List<string>();

        public static string StatusText;

        public static string[] TokenAndIds;

        static string JobUseString = "Succesful job uses: ";
        static string shipmentUseString = "Succesful shipment uses: ";
        static string TotalUseString = "Total succesful uses: ";

        public static string[] UsesArray = new string[3]
        {
            $"{JobUseString}0",
            $"{shipmentUseString}0",
            $"{TotalUseString}0"
        };

        static int jobUse = 0;
        public static int JobUse
        {
            get { return jobUse; }
            set
            {
                jobUse = value;
                UsesArray[0] = $"{JobUseString}{jobUse}";
            }
        }

        static int shipmentUse = 0;
        public static int ShipmentUse
        {
            get { return shipmentUse; }
            set
            {
                shipmentUse = value;
                UsesArray[1] = $"{shipmentUseString}{shipmentUse}";
            }
        }

        static int totalUse = 0;
        public static int TotalUse
        {
            get { return totalUse; }
            set
            {
                totalUse = value;
                UsesArray[2] = $"{TotalUseString}{totalUse}";
            }
        }
    }
}
