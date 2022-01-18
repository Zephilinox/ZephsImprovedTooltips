using Terraria.ModLoader;

namespace ZephsImprovedTooltips
{
	class ZephsImprovedTooltips : Mod
	{
        public static ZephsImprovedTooltips instance;
        public ZephsImprovedTooltips()
		{
            Properties = new ModProperties()
            {
                Autoload = true,
            };
		}

        public override void Load()
        {
            instance = this;
        }

        public override void Unload()
        {
            instance = null;
        }
	}
}
