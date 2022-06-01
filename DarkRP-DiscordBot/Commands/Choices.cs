using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Globalization;
using Discord;
using Discord.WebSocket;

namespace DarkRP_DiscordBot.Commands
{
    public static class Choices
    {
        static async Task SendEmbedMessage(SocketSlashCommand command, string type, string title, string description, Color color, string thumbnail)
        {
            int inputOptions = command.Data.Options.First().Options.Count;

            if (inputOptions != 1)
            {
                title = "¡Comando ingresado erróneamente!";
                thumbnail = "";
            }

            if (inputOptions == 0)
            {
                description = $"Por favor, utiliza una opción del comando para seleccionar un {type}.";
            }
            else if (inputOptions > 1)
            {
                description = $"Por favor, seleccione solo un {type} de una sola categoría.";
            }

            var embedMessage = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color);

            if (thumbnail != "")
            {
                using var thumbnailStream = File.OpenRead(thumbnail);

                embedMessage.WithThumbnailUrl($"attachment://{Path.GetFileName(thumbnail)}");
                await command.RespondWithFileAsync(thumbnailStream, Path.GetFileName(thumbnail), embed: embedMessage.Build(), ephemeral: true);
            }
            else
            {
                await command.RespondAsync(embed: embedMessage.Build(), ephemeral: true);
            }

            if (inputOptions == 1)
            {
                Data.TotalUse++;

                await File.WriteAllLinesAsync("uses.txt", Data.UsesArray);
            }
        }

        public static async Task HandleJobCommand(SocketSlashCommand command)
        {
            string title = "";
            string description = "";
            Color color = Color.Red;
            string thumbnail = "";

            if (command.Data.Options.First().Options.Count == 1)
            {
                int jobIndex = Convert.ToInt32(command.Data.Options.First().Options.First().Value);
                var job = Data.Jobs[jobIndex];

                title = job.Name;

                string[] weapons = new string[job.Weapons.Count];
                for (int i = 0; i < weapons.Length; i++)
                {
                    if (Data.WeaponsDictionary.ContainsKey(job.Weapons[i]))
                    {
                        weapons[i] = Data.WeaponsDictionary[job.Weapons[i]];
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

                Data.JobUse++;
            }

            await SendEmbedMessage(command, "trabajo", title, description, color, thumbnail);
        }

        public static async Task HandleShipmentCommand(SocketSlashCommand command)
        {
            string title = "";
            string description = "";
            Color color = new(140, 0, 0);
            string thumbnail = "";

            if (command.Data.Options.First().Options.Count == 1)
            {
                int shipmentIndex = Convert.ToInt32(command.Data.Options.First().Options.First().Value);
                var shipment = Data.Shipments[shipmentIndex];

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

                Data.ShipmentUse++;
            }

            await SendEmbedMessage(command, "arma", title, description, color, thumbnail);
        }

        public static async Task HandleStatusCommand(SocketSlashCommand command)
        {
            var embedMessage = new EmbedBuilder()
                .WithTitle("Estado del servidor de DarkRP")
                .WithDescription(Data.StatusText)
                .WithColor(Color.Red);

            await command.RespondAsync(embed: embedMessage.Build());
        }

        public static async Task HandleSetStatusCommand(SocketSlashCommand command)
        {
            bool canSet = false;
            var user = command.User as SocketGuildUser;

#if DEBUG
            var roleIds = Array.ConvertAll(Data.TokenAndIds[3].Split(','), x => ulong.Parse(x));
#else
            var roleIds = Array.ConvertAll(Data.TokenAndIds[4].Split(','), x => ulong.Parse(x));
#endif

            foreach (var role in user.Roles)
            {
                if (roleIds.Contains(role.Id))
                {
                    canSet = true;
                }
            }

            if (canSet)
            {
                var value = command.Data.Options.First().Options?.FirstOrDefault().Value.ToString();
                value = value.Replace("\\n", "\n");

                Data.StatusText = value;

                await File.WriteAllTextAsync("status.txt", Data.StatusText, System.Text.Encoding.Unicode);
                await command.RespondAsync($"Texto establecido a:\n```\n{Data.StatusText}\n```", ephemeral: true);
            }
            else
            {
                await command.RespondAsync("No posees el rol necesario para utilizar este comando.", ephemeral: true);
            }
        }
    }
}
