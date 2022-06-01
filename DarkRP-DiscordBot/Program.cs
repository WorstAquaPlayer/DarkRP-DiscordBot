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
using DarkRP_DiscordBot.Commands;

namespace DarkRP_DiscordBot
{
    internal class Program
    {
        private DiscordSocketClient Client;

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
                    if (!Data.WeaponsDictionary.ContainsKey(jobs[i].Weapons[j]))
                    {
                        Console.WriteLine($"Missing Weapon in Dictionary!: {jobs[i].Weapons[j]}");
                        missing++;
                    }
                }

                if (!Data.JobCategories.Contains(jobs[i].Category))
                {
                    Data.JobCategories.Add(jobs[i].Category);
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

                if (!Data.ShipmentCategories.Contains(shipments[i].Category))
                {
                    Data.ShipmentCategories.Add(shipments[i].Category);
                }
            }

            return missing;
        }

        public async Task MainAsync()
        {
            Data.Jobs = Job.GetJobListFromLua(Resources.jobs);

            var weapons_dict_text = Resources.weapons_dict.Split('\n');
            for (int i = 0; i < weapons_dict_text.Length; i++)
            {
                weapons_dict_text[i] = weapons_dict_text[i].Replace("\r", "");

                var keyValue = weapons_dict_text[i].Split(" = ");
                Data.WeaponsDictionary.Add(keyValue[0], keyValue[1]);
            }

            // order alphabetically
            Data.Jobs = Data.Jobs.OrderBy(x => x.Name).ToList();

            if (CheckJobResources(Data.Jobs) > 0)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            };

            Data.Shipments = Shipment.GetShipmentListFromLua(Resources.shipments);

            if (CheckShipmentResources(Data.Shipments) > 0)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            };

            if (!File.Exists("uses.txt"))
            {
                await File.WriteAllTextAsync("uses.txt", "", System.Text.Encoding.Unicode);
            }

            string[] usesFile = await File.ReadAllLinesAsync("uses.txt");
            Data.JobUse = int.Parse(GetTrimmedAndLoweredString(usesFile[0].Split(':')[1]));
            Console.WriteLine(Data.UsesArray[0]);

            Data.ShipmentUse = int.Parse(GetTrimmedAndLoweredString(usesFile[1].Split(':')[1]));
            Console.WriteLine(Data.UsesArray[1]);

            Data.TotalUse = int.Parse(GetTrimmedAndLoweredString(usesFile[2].Split(':')[1]));
            Console.WriteLine(Data.UsesArray[2]);

#if DEBUG
            var statusFileName = "status_debug.txt";
#else
            var statusFileName = "status.txt";
#endif

            if (!File.Exists(statusFileName))
            {
                await File.WriteAllTextAsync(statusFileName, "", System.Text.Encoding.Unicode);
            }

            Data.StatusText = await File.ReadAllTextAsync("status.txt", System.Text.Encoding.Unicode);

            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.Ready += Client_Ready;
            Client.SlashCommandExecuted += SlashCommandHandler;

            Data.TokenAndIds = Resources.discord.Split('\n');

            for (int i = 0; i < Data.TokenAndIds.Length;i++)
            {
                Data.TokenAndIds[i] = Data.TokenAndIds[i].Replace("\r", "");
            }

#if DEBUG
            string token = Data.TokenAndIds[0];
#else
            string token = Data.TokenAndIds[2];
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
            SlashCommandOptionBuilder statusOption = MakeStatusCommand();
            SlashCommandOptionBuilder setStatusOption = MakeSetStatusCommand();

            var darkrpCommand = new SlashCommandBuilder()
                .WithName("darkrp")
                .WithDescription("Comandos para obtener información del DarkRP.")
                .AddOption(jobOption)
                .AddOption(shipmentOption)
                .AddOption(statusOption)
                .AddOption(setStatusOption);

            try
            {
#if DEBUG
                ulong guildId = ulong.Parse(Data.TokenAndIds[1]);
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

            for (int i = 0; i < Data.JobCategories.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(Data.JobCategories[i]);

                var category = new SlashCommandOptionBuilder()
                    .WithName(categoryString)
                    .WithDescription($"Categoría \"{Data.JobCategories[i]}\"")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer);

                categories.Add(category);
            }

            for (int i = 0; i < Data.Jobs.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(Data.Jobs[i].Category);

                var matches = categories.Where(p => string.Equals(p.Name, categoryString, StringComparison.CurrentCulture));
                var category = matches.First();

                category.AddChoice(Data.Jobs[i].Name, i);

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
            Data.Shipments = Data.Shipments.OrderBy(x => x.SortOrder).ToList();

            for (int i = 0; i < Data.ShipmentCategories.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(Data.ShipmentCategories[i]);

                var category = new SlashCommandOptionBuilder()
                    .WithName(categoryString)
                    .WithDescription($"Categoría \"{Data.ShipmentCategories[i]}\"")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer);

                categories.Add(category);
            }

            for (int i = 0; i < Data.Shipments.Count; i++)
            {
                var categoryString = GetTrimmedAndLoweredString(Data.Shipments[i].Category);

                var matches = categories.Where(p => string.Equals(p.Name, categoryString, StringComparison.CurrentCulture));
                var category = matches.First();

                category.AddChoice(Data.Shipments[i].Name, i);

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

        SlashCommandOptionBuilder MakeStatusCommand()
        {
            var statusOption = new SlashCommandOptionBuilder()
                .WithName("status")
                .WithDescription("Muestra información acerca del estado del servidor.")
                .WithType(ApplicationCommandOptionType.SubCommand);

            return statusOption;
        }

        SlashCommandOptionBuilder MakeSetStatusCommand()
        {
            var setStatusOption = new SlashCommandOptionBuilder()
                .WithName("status-set")
                .WithDescription("Establece el texto a mostrar cuando se usa /darkrp status")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("texto", ApplicationCommandOptionType.String, "Establece el texto a mostrar cuando se usa /darkrp status", isRequired: true);

            return setStatusOption;
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "darkrp":
                    await HandleDarkRPCommand(command);
                    break;
                default:
                    Console.WriteLine($"{command.Data.Name} was executed");
                    break;
            }
        }

        async Task HandleDarkRPCommand(SocketSlashCommand command)
        {
            switch (command.Data.Options.First().Name)
            {
                case "job":
                    await Choices.HandleJobCommand(command);
                    break;
                case "shipment":
                    await Choices.HandleShipmentCommand(command);
                    break;
                case "status":
                    await Choices.HandleStatusCommand(command);
                    break;
                case "status-set":
                    await Choices.HandleSetStatusCommand(command);
                    break;
            }
        }
    }
}
