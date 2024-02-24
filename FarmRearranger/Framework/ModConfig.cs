namespace FarmRearranger.Framework
{
    class ModConfig
    {
        //players can set the farm rearranger to work anywhere, just with a warning that it could behave oddly
        public bool CanArrangeOutsideFarm { get; set; } = false;

        //the price farm rearranger is sold for
        public int Price { get; set; } = 25000;

        //the friendship points before robin sends the letter
        public int FriendshipPointsRequired { get; set; } = 2000;
    }
}
