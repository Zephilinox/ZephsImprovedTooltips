using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ZephsImprovedTooltips
{

public class Settings
{
    public enum ReforgeVisibility
    {
        [Label("Never Show")]
        NeverShow,
        [Label("Show If Tinkerer Exists")]
        ShowIfTinkererExists,
        [Label("Always Show")]
        AlwaysShow
    }

    public enum SellVisibility
    {
        [Label("Never Show")]
        NeverShow,
        [Label("Always Show")]
        AlwaysShow,
    }

    public ReforgeVisibility reforgeVisiblity = ReforgeVisibility.ShowIfTinkererExists;
    public SellVisibility sellVisibility = SellVisibility.AlwaysShow;

    public Color copperColour = new Color(246, 138, 96);
    public Color silverColour = new Color(181, 192, 193);
    public Color goldColour = new Color(221, 199, 91);
    public Color platColour = new Color(220, 220, 198);
    public Color reforgeColour = new Color(80, 140, 80);
    public Color highlightColour = new Color(255, 180, 0);
    public Color modColour = new Color(200, 40, 200);
    public bool useHighlightColour = true;
    public bool showModName = true;
    public bool showAmmunition = true;
    public bool enabled = true;

}

}