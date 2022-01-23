using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Net;
using System.Collections.Generic;
using DarkRP_DiscordBot.Lua;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;

namespace DarkRP_DiscordBot
{
    internal class Program
    {
        private DiscordSocketClient Client;
        string[] tokenAndIds;

        List<Job> Jobs;
        List<string> JobCategories = new List<string>();
        Dictionary<string, string> WeaponsDictionary = new Dictionary<string, string>();

        List<Shipment> Shipments;
        List<string> ShipmentCategories = new List<string>();

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        int CheckJobResources(List<Job> jobs)
        {
            int missing = 0;

            for (int i = 0; i < jobs.Count; i++)
            {
                for (int j = 0; j < jobs[i].Model.Count; j++)
                {
                    var iconPath = jobs[i].Model[j].Replace(".mdl", ".png");
                    var fullPath = Path.Combine("Resources" ,"Icons", iconPath);

                    if (!File.Exists(fullPath))
                    {
                        Console.WriteLine($"Missing Icon!: {fullPath}");
                        missing++;
                    }
                }

                for (int j = 0; j < jobs[i].Weapons.Count; j++)
                {
                    if (!WeaponsDictionary.ContainsKey(jobs[i].Weapons[j]))
                    {
                        Console.WriteLine($"Missing Weapon in Dictionary!: {jobs[i].Weapons[j]}");
                        missing++;
                    }
                }

                if (!JobCategories.Contains(jobs[i].Category))
                {
                    JobCategories.Add(jobs[i].Category);
                }
            }

            return missing;
        }

        int CheckShipmentResources(List<Shipment> shipments)
        {
            int missing = 0;

            for (int i = 0; i < shipments.Count; i++)
            {
                var iconPath = shipments[i].Model.Replace(".mdl", ".png");
                var fullPath = Path.Combine("Resources", "Icons", iconPath);

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"Missing Icon!: {fullPath}");
                    missing++;
                }

                if (!ShipmentCategories.Contains(shipments[i].Category))
                {
                    ShipmentCategories.Add(shipments[i].Category);
                }
            }

            return missing;
        }

        public async Task MainAsync()
        {
            Jobs = Job.GetJobListFromLua(Resources.jobs);

            var weapons_dict_text = Resources.weapons_dict.Split('\n');
            for (int i = 0; i < weapons_dict_text.Length; i++)
            {
                weapons_dict_text[i] = weapons_dict_text[i].Replace("\r", "");

                var keyValue = weapons_dict_text[i].Split(" = ");
                WeaponsDictionary.Add(keyValue[0], keyValue[1]);
            }

            if (CheckJobResources(Jobs) > 0)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            };

            Shipments = Shipment.GetShipmentListFromLua(Resources.shipments);

            if (CheckShipmentResources(Shipments) > 0)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            };

            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.Ready += Client_Ready;
            Client.SlashCommandExecuted += SlashCommandHandler;

            tokenAndIds = Resources.discord.Split('\n');

            for (int i = 0; i < tokenAndIds.Length;i++)
            {
                tokenAndIds[i] = tokenAndIds[i].Replace("\r", "");
            }

#if DEBUG
            string token = tokenAndIds[0];
#else
            string token = tokenAndIds[2];
#endif

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        string GetTrimmedAndLoweredString(string stringToTrim)
        {
            if (stringToTrim.Contains(' '))
            {
                stringToTrim = stringToTrim.Replace(" ", "");
            }

            stringToTrim = stringToTrim.ToLower();
            return stringToTrim;
        }

        public async Task Client_Ready()
        {
            SlashCommandOptionBuilder jobOption = MakeJobCommand();
            SlashCommandOptionBuilder shipmentOption = MakeShipmentCommand();

            var darkrpCommand = new SlashCommandBuilder()
                .WithName("darkrp")
                .WithDescription("Comandos para obtener información del DarkRP.")
                .AddOption(jobOption)
                .AddOption(shipmentOption);

            try
            {
#if DEBUG
                ulong guildId = ulong.Parse(tokenAndIds[1]);
                await Client.Rest.CreateGuildCommand(darkrpCommand.Build(), guildId);
#else
                await Client.CreateGlobalApplicationCommandAsync(darkrpCommand.Build());
#endif
            }
            catch (HttpException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        SlashCommandOptionBuilder MakeJobCommand()
        {
            var categories = new List<SlashCommandOptionBuilder>();
            Jobs = Jobs.OrderBy(x => x.Name).ToList();

            for (int i = 0; i < JobCategories.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(JobCategories[i]);

                var category = new SlashCommandOptionBuilder()
                    .WithName(categoryString)
                    .WithDescription($"Categoría \"{JobCategories[i]}\"")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer);

                categories.Add(category);
            }

            for (int i = 0; i < Jobs.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(Jobs[i].Category);

                var matches = categories.Where(p => string.Equals(p.Name, categoryString, StringComparison.CurrentCulture));
                var category = matches.First();

                category.AddChoice(Jobs[i].Name, i);

                int index = categories.FindIndex(p => p.Name == categoryString);
                categories[index] = category;
            }

            var jobOption = new SlashCommandOptionBuilder()
                .WithName("job")
                .WithDescription("Muestra información acerca del trabajo seleccionado.")
                .WithType(ApplicationCommandOptionType.SubCommand);

            for (int i = 0; i < categories.Count; i++)
            {
                jobOption.AddOption(categories[i]);
            }

            return jobOption;
        }

        SlashCommandOptionBuilder MakeShipmentCommand()
        {
            var categories = new List<SlashCommandOptionBuilder>();
            Shipments = Shipments.OrderBy(x => x.SortOrder).ToList();

            for (int i = 0; i < ShipmentCategories.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(ShipmentCategories[i]);

                var category = new SlashCommandOptionBuilder()
                    .WithName(categoryString)
                    .WithDescription($"Categoría \"{ShipmentCategories[i]}\"")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer);

                categories.Add(category);
            }

            for (int i = 0; i < Shipments.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(Shipments[i].Category);

                var matches = categories.Where(p => string.Equals(p.Name, categoryString, StringComparison.CurrentCulture));
                var category = matches.First();

                category.AddChoice(Shipments[i].Name, i);

                int index = categories.FindIndex(p => p.Name == categoryString);
                categories[index] = category;
            }

            var shipmentOption = new SlashCommandOptionBuilder()
                .WithName("shipment")
                .WithDescription("Muestra información acerca del arma seleccionada.")
                .WithType(ApplicationCommandOptionType.SubCommand);

            for (int i = 0; i < categories.Count; i++)
            {
                shipmentOption.AddOption(categories[i]);
            }

            return shipmentOption;
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "darkrp":
                    await HandleDarkRPCommand(command);
                    break;
                default:
                    await command.RespondAsync($"You executed {command.Data.Name}");
                    break;
            }
        }

        async Task HandleDarkRPCommand(SocketSlashCommand command)
        {
            switch (command.Data.Options.First().Name)
            {
                case "job":
                    await HandleJobCommand(command);
                    break;
                case "shipment":
                    await HandleShipmentCommand(command);
                    break;
            }
        }

        async Task HandleJobCommand(SocketSlashCommand command)
        {
            string title = "¡Comando ingresado erróneamente!";
            string description;
            Color color = Color.Red;
            string thumbnail = "";

            if (command.Data.Options.First().Options.Count == 0)
            {
                description = "Por favor, utiliza una opción del comando para seleccionar un trabajo.";
            }
            else if (command.Data.Options.First().Options.Count > 1)
            {
                description = "Por favor, seleccione solo un trabajo de una sola categoría.";
            }
            else
            {
                int jobIndex = Convert.ToInt32(command.Data.Options.First().Options.First().Value);
                var job = Jobs[jobIndex];

                title = job.Name;

                string[] weapons = new string[job.Weapons.Count];
                for (int i = 0; i < weapons.Length; i++)
                {
                    if (WeaponsDictionary.ContainsKey(job.Weapons[i]))
                    {
                        weapons[i] = WeaponsDictionary[job.Weapons[i]];
                    }
                    else
                    {
                        weapons[i] = job.Weapons[i];
                    }
                }

                string customCheck = "";

                if (job.CustomCheck == null)
                {
                    customCheck = "Ninguno";
                }
                else if (job.CustomCheck.Contains("vip-empresarial"))
                {
                    customCheck = "VIP Empresarial";
                }
                else if (job.CustomCheck.Contains("vip-jefe"))
                {
                    customCheck = "VIP Jefe";
                }
                else if (job.CustomCheck.Contains("vip-presidencial"))
                {
                    customCheck = "VIP Presidencial";
                }
                else if (job.CustomCheck.Contains("vip-presidencialplus"))
                {
                    customCheck = "VIP Presidencial Plus+";
                }
                else if (job.CustomCheck.Contains("ayudante"))
                {
                    customCheck = "Ninguno (Staff)";
                }
                else if (job.CustomCheck.Contains("mod"))
                {
                    customCheck = "Ninguno (Staff)";
                }

                string[] descriptionArray = new string[]
                {
                    "**Descripción**",
                    job.Description.Trim(),
                    "\n**Armas**",
                    string.Join(", ", weapons.OrderBy(x => x)),
                    "\n**Slots**",
                    job.Max > 0 ? job.Max.ToString() : "Ilimitado",
                    "\n**Salario**",
                    job.Salary > 0 ? $"${job.Salary.ToString("n", new CultureInfo("en-US")).Replace(".000", "")}" : "Ninguno",
                    "\n**VIP requerido**",
                    customCheck,
                    "\n**Costo**",
                    job.UnlockCost > 0 ? $"${job.UnlockCost.ToString("n", new CultureInfo("en-US")).Replace(".000", "")}" : "Ninguno",
                    "\n**Nivel requerido**",
                    job.Level > 0 ? job.Level.ToString() : "1"
                };

                description = string.Join('\n', descriptionArray);
                color = (Color)job.Color;

                var random = new Random().Next(job.Model.Count);
                var thumbnailPath = job.Model[random].Replace(".mdl", ".png");
                var fullPath = Path.Combine("Resources", "Icons", thumbnailPath);

                if (File.Exists(fullPath))
                {
                    thumbnail = fullPath;
                }
            }

            var embedJob = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color);

            if (thumbnail != "")
            {
                using var thumbnailStream = File.OpenRead(thumbnail);

                embedJob.WithThumbnailUrl($"attachment://{Path.GetFileName(thumbnail)}");
                await command.RespondWithFileAsync(thumbnailStream, Path.GetFileName(thumbnail), embed: embedJob.Build(), ephemeral: true);
            }
            else
            {
                await command.RespondAsync(embed: embedJob.Build(), ephemeral: true);
            }
        }

        async Task HandleShipmentCommand(SocketSlashCommand command)
        {
            string title = "¡Comando ingresado erróneamente!";
            string description;
            Color color = new(140, 0, 0);
            string thumbnail = "";

            if (command.Data.Options.First().Options.Count == 0)
            {
                description = "Por favor, utiliza una opción del comando para seleccionar un arma.";
            }
            else if (command.Data.Options.First().Options.Count > 1)
            {
                description = "Por favor, seleccione solo un arma de una sola categoría.";
            }
            else
            {
                int shipmentIndex = Convert.ToInt32(command.Data.Options.First().Options.First().Value);
                var shipment = Shipments[shipmentIndex];

                title = shipment.Name;

                int maxIndividualPrice = (int)((shipment.Price / shipment.Amount) * 1.5);

                string[] descriptionArray = new string[]
                {
                    "**Precio de caja (spawn)**",
                    $"${shipment.Price.ToString("n", new CultureInfo("en-US")).Replace(".000", "")}",
                    "\n**Cantidad**",
                    shipment.Amount.ToString(),
                    "\n**Precio individual máximo**",
                    $"${maxIndividualPrice.ToString("n", new CultureInfo("en-US")).Replace(".000", "")}",
                    "\n**Precio de caja máximo (venta)**",
                    $"${(maxIndividualPrice * shipment.Amount).ToString("n", new CultureInfo("en-US")).Replace(".000", "")}"
                };

                description = string.Join('\n', descriptionArray);

                var thumbnailPath = shipment.Model.Replace(".mdl", ".png");
                var fullPath = Path.Combine("Resources", "Icons", thumbnailPath);

                if (File.Exists(fullPath))
                {
                    thumbnail = fullPath;
                }
            }

            var embedShipment = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color);

            if (thumbnail != "")
            {
                using var thumbnailStream = File.OpenRead(thumbnail);

                embedShipment.WithThumbnailUrl($"attachment://{Path.GetFileName(thumbnail)}");
                await command.RespondWithFileAsync(thumbnailStream, Path.GetFileName(thumbnail), embed: embedShipment.Build(), ephemeral: true);
            }
            else
            {
                await command.RespondAsync(embed: embedShipment.Build(), ephemeral: true);
            }
        }
    }
}
