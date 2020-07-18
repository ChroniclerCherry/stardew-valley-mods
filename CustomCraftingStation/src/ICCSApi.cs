using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCraftingStation.src
{
    public interface ICCSApi
    {
        public void SetCCSCraftingMenuOverride(bool menuOverride);
    }

    public class CCSApi : ICCSApi
    {
        public void SetCCSCraftingMenuOverride(bool menuOverride)
        {
            ModEntry.MenuOverride = menuOverride;
        }
    }
}
