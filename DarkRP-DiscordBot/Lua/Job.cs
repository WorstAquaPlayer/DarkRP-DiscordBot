using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DarkRP_DiscordBot.Lua
{
    public class Job
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public List<string> Model { get; set; }
        public string Description { get; set; }
        public List<string> Weapons { get; set; }
        public string Command { get; set; }
        public int Max { get; set; }
        public int Salary { get; set; }
        public int Admin { get; set; }
        public bool Vote { get; set; }
        public bool HasLicense { get; set; }
        public bool CanDemote { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public List<string> CustomCheck { get; set; }
        public bool Hobo { get; set; }
        public int UnlockCost { get; set; }
        public int MaxPocket { get; set; }
        public string PlayerSpawn { get; set; }
        public int Level { get; set; }
        public bool Chief { get; set; }
        public bool Mayor { get; set; }
        public bool Medic { get; set; }

        readonly static int normalsalary = 150;

        public static List<Job> GetJobListFromLua(string luaString)
        {
            var list = new List<Job>();
            var entries = Parser.GetLuaEntries(luaString);

            for (int i = 0; i < entries.Count; i++)
            {
                var job = new Job();
                job.Name = entries[i].Groups[3].Value;

                var keyValues = Parser.GetEntriesKeysAndValues(entries[i].Groups[5].Value);

                var quotePattern = @"(""|')(.*?)(""|')";

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
                        case "color":
                            var colorPattern = @".*?\((\d+),.*?(\d+),.*?(\d+)(,.*?(\d+)|\))";
                            var colorRgba = Regex.Match(value, colorPattern);

                            var r = int.Parse(colorRgba.Groups[1].Value);
                            var g = int.Parse(colorRgba.Groups[2].Value);
                            var b = int.Parse(colorRgba.Groups[3].Value);
                            var a = 255;

                            if (colorRgba.Groups[5].Success)
                            {
                                a = int.Parse(colorRgba.Groups[5].Value);

                                if (a > 255)
                                {
                                    a = 255;
                                }
                            }

                            job.Color = Color.FromArgb(a, r, g, b);
                            break;
                        case "model":
                            var modelList = new List<string>();
                            var models = Regex.Matches(value, quotePattern, RegexOptions.Singleline);

                            for (int v = 0; v < models.Count; v++)
                            {
                                modelList.Add(models[v].Groups[2].Value);
                            }

                            job.Model = modelList;
                            break;
                        case "description":
                            var descriptionPattern = @"\[\[(.*?)\]\]";
                            var descriptionMatch = Regex.Match(value, descriptionPattern, RegexOptions.Singleline);

                            job.Description = descriptionMatch.Groups[1].Value;
                            break;
                        case "weapons":
                            var weaponList = new List<string>();
                            var weapons = Regex.Matches(value, quotePattern, RegexOptions.Singleline);

                            for (int v = 0; v < weapons.Count; v++)
                            {
                                weaponList.Add(weapons[v].Groups[2].Value);
                            }

                            job.Weapons = weaponList;
                            break;
                        case "command":
                            job.Command = Parser.GetQuotedString(value);
                            break;
                        case "max":
                            job.Max = Parser.GetIntFromValue(value);
                            break;
                        case "salary":
                            var salary = value;
                            if (value.Contains("normalsalary"))
                            {
                                salary = Convert.ToString(new DataTable().Compute(value.Replace("GAMEMODE.Config.normalsalary", normalsalary.ToString()), null)).Replace(".00", "");
                            }

                            job.Salary = int.Parse(salary);
                            break;
                        case "admin":
                            job.Admin = int.Parse(value);
                            break;
                        case "vote":
                            job.Vote = bool.Parse(value);
                            break;
                        case "hasLicense":
                            job.HasLicense = bool.Parse(value);
                            break;
                        case "candemote":
                            job.CanDemote = bool.Parse(value);
                            break;
                        case "type":
                            job.Type = Parser.GetQuotedString(value);
                            break;
                        case "category":
                            job.Category = Parser.GetQuotedString(value);
                            break;
                        case "customCheck":
                            if (value.Contains("usergroup"))
                            {
                                var customCheckList = new List<string>();
                                var customChecks = Regex.Matches(value, quotePattern, RegexOptions.Singleline);

                                // the -1 prevents from adding "usergroup" to the list
                                for (int v = 0; v < customChecks.Count - 1; v++)
                                {
                                    customCheckList.Add(customChecks[v].Groups[2].Value);
                                }

                                job.CustomCheck = customCheckList;
                            }

                            break;
                        case "hobo":
                            job.Hobo = bool.Parse(value);
                            break;
                        case "unlockCost":
                            job.UnlockCost = int.Parse(value);
                            break;
                        case "maxpocket":
                            job.MaxPocket = int.Parse(value);
                            break;
                        case "PlayerSpawn":
                            job.PlayerSpawn = value;
                            break;
                        case "level":
                            job.Level = Parser.GetIntFromValue(value);
                            break;
                        case "chief":
                            job.Chief = bool.Parse(value);
                            break;
                        case "mayor":
                            job.Mayor = bool.Parse(value);
                            break;
                        case "medic":
                            job.Medic = bool.Parse(value);
                            break;
                    }
                }

                list.Add(job);
            }

            return list;
        }
    }
}
