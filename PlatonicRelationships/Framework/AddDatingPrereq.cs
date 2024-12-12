using System.Collections.Generic;
using StardewModdingAPI;

namespace PlatonicRelationships.Framework;

/// <summary>
/// Changes all the vanilla 10 heart events that doesn't have a "Dating" prerequisite to have one
/// so that they don't play while platonically at 10 hearts
/// </summary>
class AddDatingPrereq
{
    public bool CanEdit(IAssetName asset)
    {
        return (
            asset.IsEquivalentTo("Data/Events/Farm") ||
            asset.IsEquivalentTo("Data/Events/Beach") ||
            asset.IsEquivalentTo("Data/Events/Mine") ||
            asset.IsEquivalentTo("Data/Events/HaleyHouse") ||
            asset.IsEquivalentTo("Data/Events/Forest") ||
            asset.IsEquivalentTo("Data/Events/ScienceHouse") ||
            asset.IsEquivalentTo("Data/Events/Mountain")
        );
    }

    public void Edit(IAssetData asset)
    {
        // TODO: add kill contexts for platonic versions of event ("/k <event id>")
        IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
        if (asset.NameWithoutLocale.IsEquivalentTo("Data/Events/Farm"))
        {
            this.changeMails(data);
        }
        else if (asset.NameWithoutLocale.IsEquivalentTo("Data/Events/Beach"))
        {
            if (data.TryGetValue("43/f Elliott 2500/w sunny/t 700 1300", out string oldScript))
            {
                data.Add("43/f Elliott 2500 /D Elliott/w sunny /t 700 1300", oldScript);
                data.Remove("43/f Elliott 2500/w sunny/t 700 1300");
            }
        }
        else if (asset.NameWithoutLocale.IsEquivalentTo("Data/Events/Mine"))
        {
            if (data.TryGetValue("901756/f Abigail 2500/t 1700 2400/o Abigail", out string oldScript))
            {
                data.Add("901756/f Abigail 2500/D Abigail/t 1700 2400/o Abigail", oldScript);
                data.Remove("901756/f Abigail 2500/t 1700 2400/o Abigail");
            }
        }
        else if (asset.NameWithoutLocale.IsEquivalentTo("Data/Events/HaleyHouse"))
        {
            if (data.TryGetValue("15/f Haley 2500/p Haley", out string oldScript))
            {
                data.Add("15/f Haley 2500/D Haley/p Haley", oldScript);
                data.Remove("15/f Haley 2500/p Haley");
            }
        }
        else if (asset.NameWithoutLocale.IsEquivalentTo("Data/Events/Forest"))
        {
            if (data.TryGetValue("54/f Leah 2500/t 1100 1600/z winter", out string oldScript))
            {
                data.Add("54/f Leah 2500/D Leah/t 1100 1600/z winter", oldScript);
                data.Remove("54/f Leah 2500/t 1100 1600/z winter");
            }
        }
        else if (asset.NameWithoutLocale.IsEquivalentTo("Data/Events/ScienceHouse"))
        {
            if (data.TryGetValue("10/f Maru 2500/t 900 1600", out string oldScript))
            {
                data.Add("10/f Maru 2500/D Maru/t 900 1600", oldScript);
                data.Remove("10/f Maru 2500/t 900 1600");
            }
        }
        else if (asset.NameWithoutLocale.IsEquivalentTo("Data/Events/Mountain"))
        {
            if (data.TryGetValue("384882/f Sebastian 2500/t 2000 2400", out string oldScript))
            {
                data.Add("384882/f Sebastian 2500/D Sebastian/t 2000 2400", oldScript);
                data.Remove("384882/f Sebastian 2500/t 2000 2400");
            }
        }
    }

    public void changeMails(IDictionary<string, string> data)
    {
        if (data.Remove("2346094/f Elliott 2502/x elliottBoat"))
            data.Add("2346094/f Elliott 2502/D Elliott/x elliottBoat", "null");

        if (data.Remove("2346093/f Harvey 2502/x harveyBalloon"))
            data.Add("2346093/f Harvey 2502/D Harvey/x harveyBalloon", "null");

        if (data.Remove("2346092/f Sam 2502/x samMessage"))
            data.Add("2346092/f Sam 2502/D Sam/x samMessage", "null");

        if (data.Remove("2346091/f Alex 2502/x joshMessage"))
            data.Add("2346091/f Alex 2502/D Alex/x joshMessage", "null");

        if (data.Remove("2346096/f Penny 2505/x pennySpa"))
            data.Add("2346096/f Penny 2505/D Penny/x pennySpa", "null");
    }
}
