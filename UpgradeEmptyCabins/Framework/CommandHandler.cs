using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace UpgradeEmptyCabins.Framework
{
    /// <summary>Handles console commands registered by the mod.</summary>
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
            commandHelper.Add("upgrade_cabin", "If Robin is free, brings up the menu to upgrade cabins.", this.UpgradeCabinsCommand);
            commandHelper.Add("remove_seed_boxes", "Removes seed boxes from all unclaimed cabins.", this.RemoveSeedBoxesCommand);
            commandHelper.Add("remove_cabin_beds", "Removes beds from all unclaimed cabins.", this.RemoveCabinBedsCommand);
            commandHelper.Add("renovate_cabins", "Removes cribs and adds all the extra rooms to all unclaimed cabins.", this.RenovateCabinsCommand);

            // renovate specific cabins
            commandHelper.Add("list_cabins", "Lists cabin names for toggle_renovate.", this.ListCabins);
            commandHelper.Add("list_renovations", "Lists renovation names for toggle_renovate.", this.ListRenovations);
            commandHelper.Add("set_crib_style", "Sets the crib style for an unclaimed cabin.", this.SetCribStyleCommand);
            commandHelper.Add("toggle_renovate", "Toggles a renovation for an unclaimed cabin.", this.ToggleRenovateCommand);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** General
        ****/
        private void UpgradeCabinsCommand(string arg1, string[] arg2)
        {
            this.AskForUpgrade();
        }

        private void RemoveSeedBoxesCommand(string arg1, string[] arg2)
        {
            foreach ((Building cabin, Cabin indoors) in ModUtility.GetEmptyCabins())
            {
                foreach ((Vector2 tile, Object obj) in indoors.Objects.Pairs)
                {
                    if (obj is not Chest chest || !chest.giftbox.Value || chest.bigCraftable.Value)
                    {
                        continue;
                    }

                    indoors.Objects.Remove(tile);
                    this.Monitor.Log("Seed box removed from " + cabin.GetIndoorsName(), LogLevel.Info);
                }
            }
        }

        private void RemoveCabinBedsCommand(string arg1, string[] arg2)
        {
            foreach ((Building cabin, Cabin indoors) in ModUtility.GetEmptyCabins())
            {
                BedFurniture bed = null;

                foreach (Furniture furniture in indoors.furniture)
                {
                    if (furniture is BedFurniture b)
                    {
                        bed = b;
                        break;
                    }
                }

                if (bed != null)
                {
                    indoors.furniture.Remove(bed);
                    this.Monitor.Log("Bed removed from " + cabin.GetIndoorsName(), LogLevel.Info);
                }
            }
        }

        private void RenovateCabinsCommand(string arg1, string[] arg2)
        {
            string[] renos = { "renovation_bedroom_open", "renovation_southern_open", "renovation_corner_open", "renovation_extendedcorner_open", "renovation_dining_open", "renovation_diningroomwall_open", "renovation_cubby_open", "renovation_farupperroom_open" };

            foreach ((Building cabin, Cabin indoors) in ModUtility.GetEmptyCabins())
            {
                this.Monitor.Log("Cabin: " + cabin.GetIndoorsName(), LogLevel.Info);
                this.Monitor.Log("    Type: " + cabin.buildingType.Value, LogLevel.Info);
                if (indoors.upgradeLevel < 2)
                {
                    this.Monitor.Log("    Upgrade Level: " + indoors.upgradeLevel, LogLevel.Info);
                    this.Monitor.Log("    Upgrade Level 2 required for renovations. Not Renovated.", LogLevel.Info);
                    continue;
                }
                ISet<string> mail = indoors.owner.mailReceived;
                foreach (string reno in renos)
                {
                    if (mail.Contains(reno))
                        this.Monitor.Log("Renovation already done: " + reno + " " + cabin.GetIndoorsName(), LogLevel.Info);
                    else
                    {
                        if (reno == "renovation_diningroomwall_open")
                            mail.Remove(reno);
                        else
                            mail.Add(reno);
                    }
                }

                indoors.cribStyle.Set(0);
                this.Monitor.Log("    cribStyle:  " + indoors.cribStyle.Value, LogLevel.Info);
                this.Monitor.Log("    flags: " + mail, LogLevel.Info);
            }
        }

        /****
        ** Renovate specific cabins
        ****/
        private void ListCabins(string arg1, string[] arg2)
        {
            this.Monitor.Log("Upgrade Level 2 required for renovations.", LogLevel.Info);

            foreach ((Building cabin, Cabin indoors) in ModUtility.GetEmptyCabins())
            {
                this.Monitor.Log("Cabin: " + cabin.GetIndoorsName(), LogLevel.Info);
                this.Monitor.Log("    Upgrade Level: " + indoors.upgradeLevel, LogLevel.Info);
            }
        }

        private void ListRenovations(string arg1, string[] arg2)
        {
            this.Monitor.Log("renovation_bedroom_open, renovation_southern_open, renovation_corner_open, renovation_extendedcorner_open, renovation_dining_open, renovation_diningroomwall_open, renovation_cubby_open, renovation_farupperroom_open", LogLevel.Info);
        }

        private void SetCribStyleCommand(string arg1, string[] arg2)
        {
            string cabinType = arg2[0] + " Cabin"; //"Plank","Stone","Log"
            int style = int.Parse(arg2[1]);

            foreach ((Building cabin, Cabin indoors) in ModUtility.GetEmptyCabins())
            {
                if (cabin.buildingType.ToString() == cabinType)
                {
                    indoors.cribStyle.Set(style);
                    this.Monitor.Log("Cabin: " + cabin.GetIndoorsName(), LogLevel.Info);
                    this.Monitor.Log("Cabin Type: " + cabin.buildingType.Value, LogLevel.Info);
                    this.Monitor.Log("cribStyle: " + indoors.cribStyle.Value, LogLevel.Info);
                }
            }
        }

        private void ToggleRenovateCommand(string arg1, string[] arg2)
        {
            string cabinType = arg2[0]; //"Plank","Stone","Log"
            string reno = arg2[1]; //"renovation_bedroom_open", "renovation_southern_open", "renovation_corner_open", "renovation_extendedcorner_open", "renovation_dining_open", "renovation_diningroomwall_open", "renovation_cubby_open", "renovation_farupperroom_open"

            foreach ((Building cabin, Cabin indoors) in ModUtility.GetEmptyCabins())
            {
                if (cabin.GetIndoorsName() == cabinType)
                {
                    ISet<string> mail = indoors.owner.mailReceived;
                    this.Monitor.Log("Cabin: " + cabin.GetIndoorsName(), LogLevel.Info);

                    if (reno == "renovation_diningroomwall_open")
                    {
                        if (!mail.Contains("renovation_dining_open"))
                        {
                            mail.Add("renovation_dining_open");
                            mail.Remove(reno);
                            this.Monitor.Log("Renovation Added: renovation_dining_open" + reno, LogLevel.Info);
                            this.Monitor.Log("Renovation Removed: " + reno, LogLevel.Info);
                        }
                        else if (mail.Contains("renovation_dining_open") && !mail.Contains("renovation_diningroomwall_open"))
                        {
                            mail.Add(reno);
                            this.Monitor.Log("Renovation Added: " + reno, LogLevel.Info);
                        }
                        else
                        {
                            mail.Remove(reno);
                            this.Monitor.Log("Renovation Removed: " + reno, LogLevel.Info);
                        }
                    }
                    else if (mail.Contains(reno))
                    {
                        if (reno == "renovation_corner_open")
                        {
                            mail.Remove("renovation_extendedcorner_open");
                            this.Monitor.Log("Renovation Removed: renovation_extendedcorner_open", LogLevel.Info);
                        }
                        if (reno == "renovation_dining_open" && mail.Contains("renovation_diningroomwall_open"))
                        {
                            mail.Remove("renovation_diningroomwall_open");
                            this.Monitor.Log("Renovation Removed: renovation_diningroomwall_open", LogLevel.Info);
                        }
                        mail.Remove(reno);
                        this.Monitor.Log("Renovation Removed: " + reno, LogLevel.Info);
                    }
                    else
                    {
                        if (reno == "renovation_extendedcorner_open")
                        {
                            mail.Add("renovation_corner_open");
                            this.Monitor.Log("Renovation Added: renovation_corner_open", LogLevel.Info);
                        }
                        mail.Add(reno);
                        this.Monitor.Log("Renovation Added: " + reno, LogLevel.Info);
                    }
                }
            }
        }
    }
}
