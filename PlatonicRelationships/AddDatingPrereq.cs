using StardewModdingAPI;
using System.Collections.Generic;

namespace PlatonicRelationships
{
    class AddDatingPrereq : IAssetEditor
    {
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return (
                asset.AssetNameEquals("Data/Events/Farm") ||
                asset.AssetNameEquals("Data/Events/Beach") ||
                asset.AssetNameEquals("Data/Events/Mine") ||
                asset.AssetNameEquals("Data/Events/HaleyHouse") ||
                asset.AssetNameEquals("Data/Events/Forest") ||
                asset.AssetNameEquals("Data/Events/ScienceHouse")
                );
        }

        public void Edit<T>(IAssetData asset)
        {
            // TODO: add kill contexts for platonic versions of event ("/k <event id>")
            IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
            if (asset.AssetNameEquals("Data/Events/Farm"))
            {
                changeMails(data);
            }
            else if (asset.AssetNameEquals("Data/Events/Beach"))
            {
                if (data.ContainsKey("43/f Elliott 2500/w sunny/t 700 1300"))
                {
                    data.Add("43/f Elliott 2500 /D Elliott /w sunny /t 700 1300", data["43/f Elliott 2500/w sunny/t 700 1300"]);
                    data.Remove("43/f Elliott 2500/w sunny/t 700 1300");
                }
            }
            else if (asset.AssetNameEquals("Data/Events/Mine"))
            {
                if (data.ContainsKey("901756/f Abigail 2500/t 1700 2400/o Abigail"))
                {
                    data.Add("901756/f Abigail 2500/D Abigail/t 1700 2400/o Abigail", data["901756/f Abigail 2500/t 1700 2400/o Abigail"]);
                    data.Remove("901756/f Abigail 2500/t 1700 2400/o Abigail");
                }
            }
            else if (asset.AssetNameEquals("Data/Events/HaleyHouse"))
            {
                if (data.ContainsKey("15/f Haley 2500/p Haley"))
                {
                    data.Add("15/f Haley 2500/D Haley/p Haley", data["15/f Haley 2500/p Haley"]);
                    data.Remove("15/f Haley 2500/p Haley");
                }
            }
            else if (asset.AssetNameEquals("Data/Events/Forest"))
            {
                if (data.ContainsKey("54/f Leah 2500/t 1100 1600/z winter"))
                {
                    data.Add("54/f Leah 2500/D Leah/t 1100 1600/z winter", data["54/f Leah 2500/t 1100 1600/z winter"]);
                    data.Remove("54/f Leah 2500/t 1100 1600/z winter");
                }
            }
            else if (asset.AssetNameEquals("Data/Events/ScienceHouse"))
            {
                if (data.ContainsKey("10/f Maru 2500/t 900 1600"))
                {
                    data.Add("10/f Maru 2500/D Maru/t 900 1600", data["10/f Maru 2500/t 900 1600"]);
                    data.Remove("10/f Maru 2500/t 900 1600");
                }
            }
        }

        public void changeMails(IDictionary<string, string> data)
        {
            if (data.ContainsKey("2346094/f Elliott 2502/x elliottBoat"))
            {
                data.Remove("2346094/f Elliott 2502/x elliottBoat");
                data.Add("2346094/f Elliott 2502/D Elliott/x elliottBoat", "null");
            }

            if (data.ContainsKey("2346093/f Harvey 2502/x harveyBalloon"))
            {
                data.Remove("2346093/f Harvey 2502/x harveyBalloon");
                data.Add("2346093/f Harvey 2502/D Harvey/x harveyBalloon", "null");
            }

            if (data.ContainsKey("2346093/f Harvey 2502/x harveyBalloon"))
            {
                data.Remove("2346092/f Sam 2502/x samMessage");
                data.Add("2346092/f Sam 2502/D Sam/x samMessage", "null");
            }

            if (data.ContainsKey("2346091/f Alex 2502/x joshMessage"))
            {
                data.Remove("2346091/f Alex 2502/x joshMessage");
                data.Add("2346091/f Alex 2502/D Alex/x joshMessage", "null");
            }
        }

    }
}
