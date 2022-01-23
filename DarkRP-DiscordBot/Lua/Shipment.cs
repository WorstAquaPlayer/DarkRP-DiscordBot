using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRP_DiscordBot.Lua
{
    public class Shipment
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Entity { get; set; }
        public int Price { get; set; }
        public int Amount { get; set; }
        public bool Separate { get; set; }
        public  int PriceSep { get; set; }
        public bool NoShip { get; set; }
        public string Category { get; set; }
        public int SortOrder { get; set; }
        public List<string> Allowed { get; set; }

        public static List<Shipment> GetShipmentListFromLua(string luaString)
        {
            var list = new List<Shipment>();
            luaString = luaString.Substring(luaString.IndexOf("DarkRP.createShipment"));
            var entries = Parser.GetLuaEntries(luaString);

            for (int i = 0; i < entries.Count; i++)
            {
                var shipment = new Shipment();
                shipment.Name = entries[i].Groups[3].Value;

                var keyValues = Parser.GetEntriesKeysAndValues(entries[i].Groups[5].Value);

                for (int j = 0; j < keyValues.Count; j++)
                {
                    var key = keyValues[j].Groups[1].Value;
                    var value = keyValues[j].Groups[2].Value;

                    if (value.Contains("\n--"))
                    {
                        value = value.Substring(0, value.IndexOf("\n--"));
                    }

                    if (value.EndsWith('\t'))
                    {
                        value = value.Substring(0, value.Length - 1);
                    }

                    if (value.EndsWith(','))
                    {
                        value = value.Substring(0, value.Length - 1);
                    }

                    switch (key)
                    {
                        case "model":
                            shipment.Model = Parser.GetQuotedString(value);
                            break;
                        case "entity":
                            shipment.Entity = Parser.GetQuotedString(value);
                            break;
                        case "price":
                            shipment.Price = Parser.GetIntFromValue(value);
                            break;
                        case "amount":
                            shipment.Amount = Parser.GetIntFromValue(value);
                            break;
                        case "separate":
                            shipment.Separate = bool.Parse(value);
                            break;
                        case "pricesep":
                            if (!value.Contains("nil"))
                            {
                                shipment.PriceSep = Parser.GetIntFromValue(value);
                            }
                            break;
                        case "noship":
                            shipment.NoShip = bool.Parse(value);
                            break;
                        case "category":
                            shipment.Category = Parser.GetQuotedString(value);
                            break;
                        case "sortOrder":
                            shipment.SortOrder = Parser.GetIntFromValue(value);
                            break;
                        case "allowed":
                            var allowedList = new List<string>();
                            value = value.Replace(" ", "").Replace("{", "").Replace("}", "");

                            if (value.Contains(','))
                            {
                                var splitValue = value.Split(',');

                                for (int k = 0; k < splitValue.Length; k++)
                                {
                                    allowedList.Add(splitValue[k]);
                                }
                            }
                            else
                            {
                                allowedList.Add(value);
                            }

                            shipment.Allowed = allowedList;
                            break;
                    }
                }

                list.Add(shipment);
            }

            return list;
        }
    }
}
