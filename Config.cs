using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ZephsImprovedTooltips
{

[Label("Config")]
public class TooltipConfig : ModConfig
{
    public TooltipConfig()
    {
        configSettings = new Settings();
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    private Settings configSettings;

    [Label("Enable improved tooltips")]
    [DefaultValue(true)]
    public bool enabled {
        get => configSettings.enabled;
        set => configSettings.enabled = value;
    }

    [Label("Reforge Visibility")]
    [DrawTicks]
    [DefaultValue(Settings.ReforgeVisibility.ShowIfTinkererExists)]
    public Settings.ReforgeVisibility reforgeVisibility {
        get => configSettings.reforgeVisiblity;
        set => configSettings.reforgeVisiblity = value;
    }

    [Label("Sellprice Visibility")]
    [DrawTicks]
    [DefaultValue(Settings.SellVisibility.AlwaysShow)]
    public Settings.SellVisibility sellVisibility {
        get => configSettings.sellVisibility;
        set => configSettings.sellVisibility = value;
    }

    [Label("Enable Highlight Colour")]
    [Tooltip("Highlights some numbers in tooltips")]
    [DefaultValue(true)]
    public bool highlightColourEnabled {
        get => configSettings.useHighlightColour;
        set => configSettings.useHighlightColour = value;
    }

    [Label("Show Mod Name")]
    [Tooltip("Shows the mod that an item came from")]
    [DefaultValue(true)]
    public bool showModName {
        get => configSettings.showModName;
        set => configSettings.showModName = value;
    }

    [Label("Show Ammunition")]
    [DefaultValue(true)]
    public bool showAmmunition {
        get => configSettings.showAmmunition;
        set => configSettings.showAmmunition = value;
    }

    [Label("Reforge Colour")]
    [DefaultValue(typeof(Color), "80, 140, 80, 255")]
    public Color reforgeColour {
        get => configSettings.reforgeColour;
        set => configSettings.reforgeColour = value;
    }

    [Label("Highlight Colour")]
    [DefaultValue(typeof(Color), "255, 180, 0, 255")]
    public Color highlightColour {
        get => configSettings.highlightColour;
        set => configSettings.highlightColour = value;
    }

    [Label("Mod Name Colour")]
    [DefaultValue(typeof(Color), "200, 40, 200, 255")]
    public Color modColour {
        get => configSettings.modColour;
        set => configSettings.modColour = value;
    }

    public override void OnChanged() {
        ZephsImprovedTooltipsGlobalItem.settings = configSettings;
    }
}

}