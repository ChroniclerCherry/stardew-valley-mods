using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace UpgradeEmptyCabins.Framework
{
    /// <summary>Handles console commands registered by the mod.</summary>
    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Strings contain renovation IDs, which must match exactly.")]
    internal class CommandHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Show the UI to choose a cabin to upgrade.</summary>
        private readonly Action AskForUpgrade;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="askForUpgrade">Show the UI to choose a cabin to upgrade.</param>
        public CommandHandler(IMonitor monitor, Action askForUpgrade)
        {
            this.Monitor = monitor;
            this.AskForUpgrade = askForUpgrade;
        }

        /// <summary>Register the console commands.</summary>
        /// <param name="commandHelper">The console commands API.</param>
        public void Register(ICommandHelper commandHelper)
        {
            // general
            commandHelper.Add(
                "upgrade_cabins",
                """
                If Robin is free, show the menu to upgrade cabins.

                Usage:
                    upgrade_cabins
                """,
                this.HandleUpgradeCabins
            );
            commandHelper.Add(
                "list_cabins",
                """
                List all empty cabins in the save. The number before each cabin can be used in other commands, like `renovate_cabin 1` to renovate the first one.

                Usage:
                    list_cabins
                """,
                this.HandleListCabins
            );
            commandHelper.Add(
                "list_renovations",
                """
                List the renovation names that can be used with the `toggle_renovate` command.
                
                Usage:
                    list_renovations
                """,
                this.HandleListRenovations
            );

            // specific cabins
            commandHelper.Add(
                "renovate_cabin",
                """
                Remove cribs and add all extra rooms in an empty cabin.

                Usage:
                    renovate_cabin <cabin number>

                Enter `list_cabins` to see a list of cabin numbers.
                """,
                this.HandleRenovateCabin
            );
            commandHelper.Add(
                "set_crib_style",
                """
                Set the crib style for an empty cabin.

                Usage:
                    set_crib_style <cabin number> <style>

                Enter `list_cabins` to see a list of cabin numbers. The crib style should usually be '0' (none) or '1' (added).
                """,
                this.HandleSetCribStyle
            );
            commandHelper.Add(
                "toggle_renovation",
                """
                Toggle a renovation for an empty cabin.

                Usage:
                    toggle_renovation <cabin number> <renovation ID>

                Enter `list_cabins` to see a list of cabin numbers, and `list_renovations` for a list of renovation IDs.
                """,
                this.HandleToggleRenovation
            );
        }


        /*********
        ** Private methods
        *********/
        /****
        ** General commands
        ****/
        /// <summary>Handle the <c>upgrade_cabins</c> console command.</summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleUpgradeCabins(string commandName, string[] args)
        {
            if (!this.AssertSaveLoaded(out string? error))
            {
                this.LogCommandError(commandName, error);
                return;
            }

            this.AskForUpgrade();
        }

        /// <summary>Handle the <c>list_cabins</c> console command.</summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleListCabins(string commandName, string[] args)
        {
            if (!this.AssertSaveLoaded(out string? error))
            {
                this.LogCommandError(commandName, error);
                return;
            }

            // get cabins
            var cabins = ModUtility.GetEmptyCabins().ToArray();
            if (cabins.Length == 0)
            {
                this.Monitor.Log("You don't have any empty cabins in this save.", LogLevel.Info);
                return;
            }

            // list cabin info
            StringBuilder summary = new StringBuilder();
            summary.AppendLine($"You have {cabins.Length} empty cabin{(cabins.Length > 1 ? "s" : "")} in this save:");

            for (int i = 0; i < cabins.Length; i++)
            {
                (Building cabin, Cabin indoors) = cabins[i];

                summary
                    .Append($"  {i + 1}. {ModUtility.GetCabinDescription(cabin)}, ")
                    .Append(indoors.upgradeLevel switch
                    {
                        0 => "not upgraded yet.",
                        1 => $"upgrade level {indoors.upgradeLevel} (kitchen).",
                        2 => $"upgrade level {indoors.upgradeLevel} (kitchen + extra rooms).",
                        3 => $"upgrade level {indoors.upgradeLevel} (kitchen + extra rooms + cellar).",
                        _ => $"upgrade level {indoors.upgradeLevel}."
                    })
                    .Append(ReferenceEquals(Game1.player.currentLocation, indoors)
                        ? " You are here."
                        : ""
                    )
                    .AppendLine();
            }

            summary
                .AppendLine()
                .AppendLine("You can use the cabin number in other commands, like `remove_seed_box 1` to remove it from the first cabin above.");

            this.Monitor.Log(summary.ToString(), LogLevel.Info);
        }

        /// <summary>Handle the <c>list_renovations</c> console command.</summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleListRenovations(string commandName, string[] args)
        {
            if (!this.AssertSaveLoaded(out string? error))
            {
                this.LogCommandError(commandName, error);
                return;
            }

            this.Monitor.Log("renovation_bedroom_open, renovation_southern_open, renovation_corner_open, renovation_extendedcorner_open, renovation_dining_open, renovation_diningroomwall_open, renovation_cubby_open, renovation_farupperroom_open", LogLevel.Info);
        }

        /****
        ** Specific cabin commands
        ****/
        /// <summary>Handle the <c>renovate_cabin</c> console command.</summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleRenovateCabin(string commandName, string[] args)
        {
            // read args
            if (!this.AssertSaveLoaded(out string? error) || !this.TryGetCabinFromArg(args, 0, out Building? cabin, out Cabin? indoors, out error))
            {
                this.LogCommandError(commandName, error);
                return;
            }

            // validate upgrade level
            if (indoors.upgradeLevel < 2)
            {
                this.Monitor.Log($"Can't apply renovations to {ModUtility.GetCabinDescription(cabin)}: cabins need upgrade level 2 for renovations, but it only has upgrade level {indoors.upgradeLevel}. You can run `upgrade_cabins` to upgrade it.", LogLevel.Error);
                return;
            }

            // apply
            StringBuilder summary = new();
            {
                summary.AppendLine($"Adding all renovations to {ModUtility.GetCabinDescription(cabin)}...");

                ISet<string> mail = indoors.owner.mailReceived;
                foreach (string renovationId in new[] { "renovation_bedroom_open", "renovation_southern_open", "renovation_corner_open", "renovation_extendedcorner_open", "renovation_dining_open", "renovation_diningroomwall_open", "renovation_cubby_open", "renovation_farupperroom_open" })
                {
                    if (mail.Contains(renovationId))
                        summary.AppendLine($"  - {renovationId}: already applied.");
                    else
                    {
                        if (renovationId == "renovation_diningroomwall_open")
                            mail.Remove(renovationId);
                        else
                            mail.Add(renovationId);

                        summary.AppendLine($"  - {renovationId}: added.");
                    }
                }

                indoors.cribStyle.Set(0);
                summary.AppendLine("  - cribs: removed.");
            }
            this.Monitor.Log(summary.ToString(), LogLevel.Info);
        }

        /// <summary>Handle the <c>set_crib_style</c> console command.</summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleSetCribStyle(string commandName, string[] args)
        {
            // read args
            if (!this.AssertSaveLoaded(out string? error) || !this.TryGetCabinFromArg(args, 0, out Building? cabin, out Cabin? indoors, out error) || !ArgUtility.TryGetInt(args, 1, out int cribStyle, out error))
            {
                this.LogCommandError(commandName, error);
                return;
            }

            // apply
            indoors.cribStyle.Value = cribStyle;
            this.Monitor.Log($"Crib style {cribStyle} set for {ModUtility.GetCabinDescription(cabin)}.", LogLevel.Info);
        }

        /// <summary>Handle the <c>toggle_renovation</c> console command.</summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleToggleRenovation(string commandName, string[] args)
        {
            // read args
            if (!this.AssertSaveLoaded(out string? error) || !this.TryGetCabinFromArg(args, 0, out Building? cabin, out Cabin? indoors, out error) || !ArgUtility.TryGet(args, 1, out string renovationId, out error, allowBlank: false))
            {
                this.LogCommandError(commandName, error);
                return;
            }

            // apply
            StringBuilder summary = new();
            {
                summary.AppendLine($"Toggling renovation '{renovationId}' for {ModUtility.GetCabinDescription(cabin)}...");

                ISet<string> mail = indoors.owner.mailReceived;
                if (renovationId == "renovation_diningroomwall_open")
                {
                    if (mail.Add("renovation_dining_open"))
                    {
                        mail.Remove(renovationId);
                        summary.AppendLine($"  Added 'renovation_dining_open{renovationId}' and removed '{renovationId}'.");
                    }
                    else if (mail.Contains("renovation_dining_open") && !mail.Contains("renovation_diningroomwall_open"))
                    {
                        mail.Add(renovationId);
                        summary.AppendLine($"  Added '{renovationId}'.");
                    }
                    else
                    {
                        mail.Remove(renovationId);
                        summary.AppendLine($"  Removed '{renovationId}'.");
                    }
                }
                else if (mail.Contains(renovationId))
                {
                    if (renovationId == "renovation_corner_open")
                    {
                        mail.Remove("renovation_extendedcorner_open");
                        summary.AppendLine("  Removed 'renovation_extendedcorner_open'.");
                    }

                    if (renovationId == "renovation_dining_open" && mail.Contains("renovation_diningroomwall_open"))
                    {
                        mail.Remove("renovation_diningroomwall_open");
                        summary.AppendLine("  Removed 'renovation_diningroomwall_open'.");
                    }

                    mail.Remove(renovationId);
                    summary.AppendLine($"  Removed '{renovationId}'.");
                }
                else
                {
                    if (renovationId == "renovation_extendedcorner_open")
                    {
                        mail.Add("renovation_corner_open");
                        summary.AppendLine("  Added 'renovation_corner_open'.");
                    }

                    mail.Add(renovationId);
                    summary.AppendLine($"  Added '{renovationId}'.");
                }
            }
            this.Monitor.Log(summary.ToString(), LogLevel.Info);
        }

        /****
        ** Helpers
        ****/
        /// <summary>Log an error indicating a command failed.</summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="error">The error phrase indicating what went wrong.</param>
        private void LogCommandError(string commandName, string error)
        {
            this.Monitor.Log($"The '{commandName}' command failed: {error}. You can enter `help {commandName}` for instructions.", LogLevel.Error);
        }

        /// <summary>Assert that a save was loaded before running the current command.</summary>
        /// <param name="error">An error indicating a save must be loaded, if applicable.</param>
        /// <returns>Returns whether a save is loaded.</returns>
        private bool AssertSaveLoaded([NotNullWhen(false)] out string? error)
        {
            if (!Context.IsWorldReady)
            {
                error = "you must load a save to use this command";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>Try to get the cabin matching a cabin number argument.</summary>
        /// <param name="args">The arguments to read.</param>
        /// <param name="index">The index in <paramref name="args"/> containing the cabin number.</param>
        /// <param name="cabin">The cabin building, if found.</param>
        /// <param name="indoors">The cabin interior, if found.</param>
        /// <param name="error">The error message if the cabin could not be found.</param>
        /// <returns>Returns whether a cabin was successfully matched.</returns>
        private bool TryGetCabinFromArg(string[] args, int index, [NotNullWhen(true)] out Building? cabin, [NotNullWhen(true)] out Cabin? indoors, [NotNullWhen(false)] out string? error)
        {
            if (!ArgUtility.TryGetInt(args, index, out int cabinNumber, out error))
            {
                cabin = null;
                indoors = null;
                return false;
            }

            (cabin, indoors) = ModUtility.GetEmptyCabins().Skip(cabinNumber - 1).FirstOrDefault();
            if (cabin is null || indoors is null)
            {
                cabin = null;
                indoors = null;
                error = $"required index {index} ({nameof(cabinNumber)}) has value {cabinNumber}, which doesn't match any cabin listed by the `list_cabins` command";
                return false;
            }

            error = null;
            return true;
        }
    }
}
