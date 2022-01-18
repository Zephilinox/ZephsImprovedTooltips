using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ZephsImprovedTooltips
{
	public class ZephsImprovedTooltipsGlobalItem : GlobalItem
	{
        static public Settings settings = new Settings();

		public ZephsImprovedTooltipsGlobalItem()
		{
		}

        public void splitValue(int totalCopper, out int plat, out int gold, out int silver, out int copper)
        {
            plat = totalCopper / 1000000;
            totalCopper %= 1000000;
            gold = totalCopper / 10000;
            totalCopper %= 10000;
            silver = totalCopper / 100;
            totalCopper %= 100;
            copper = totalCopper;
        }

        public string valueAsString(int plat, int gold, int silver, int copper)
        {
            string line = "";

            if (plat > 0)
            {
                line += plat + " platinum ";
            }

            if (gold > 0)
            {
                line += gold + " gold ";
            }

            if (silver > 0)
            {
                line += silver + " silver ";
            }

            //don't bother with copper if we have plat, makes the line too long and no one cares
            if (plat == 0 && copper > 0)
            {
                line += copper + " copper ";
            }

            return line;
        }

        public void reforgePriceTooltip(Item item, TooltipLine line)
        {
            bool enabled = true;
            if (settings.reforgeVisiblity == Settings.ReforgeVisibility.NeverShow
                || (settings.reforgeVisiblity == Settings.ReforgeVisibility.ShowIfTinkererExists && !NPC.savedGoblin))
                enabled = false;
                
            int totalValue = (int)(item.GetStoreValue() / 3.0f);

            if (!enabled || item.maxStack > 1 || item.vanity || totalValue == 0
                || (!item.accessory && item.defense > 0))
            {
                line.text = "";
                return;
            }

            line.text = "Reforge price: ";
            line.overrideColor = settings.reforgeColour;

            int plat, gold, silver, copper;
            splitValue(totalValue, out plat, out gold, out silver, out copper);
            line.text += valueAsString(plat, gold, silver, copper);
        }

        public void sellPriceTooltip(Item item, TooltipLine line)
        {
            bool enabled = settings.sellVisibility == Settings.SellVisibility.AlwaysShow;

            int totalValue = (int)((item.GetStoreValue() * item.stack) / 5.0f);

            if (!enabled ||
                item.type == ItemID.CopperCoin ||
                item.type == ItemID.SilverCoin ||
                item.type == ItemID.GoldCoin ||
                item.type == ItemID.PlatinumCoin ||
                totalValue == 0)
            {
                line.text = "";
                return;
            }

            line.text = "Sell price: ";

            int plat, gold, silver, copper;
            splitValue(totalValue, out plat, out gold, out silver, out copper);
            line.text += valueAsString(plat, gold, silver, copper);

            if (plat > 0)
            {
                line.overrideColor = settings.platColour;
            }
            else if (gold > 0)
            {
                line.overrideColor = settings.goldColour;
            }
            else if (silver > 0)
            {
                line.overrideColor = settings.silverColour;
            }
            else
            {
                line.overrideColor = settings.copperColour;
            }
        }

        public string colourAsHexString(Color colour)
        {
            return $"{colour.R.ToString("x2")}{colour.G.ToString("x2")}{colour.B.ToString("x2")}";
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!settings.enabled)
                return;
            
            string startColour = settings.useHighlightColour ? $"[c/{colourAsHexString(settings.highlightColour)}:" : "";
            string endColour = settings.useHighlightColour ? "]" : "";

            bool isTool = false;
            bool firesProjectile = false;

            if (item.pick > 0 ||
                item.axe > 0 ||
                item.hammer > 0)
            {
                isTool = true;
            }

            if (item.melee &&
                item.shoot > 0 &&
                item.useAnimation <= item.useTime)
            {
                //Swords with projectiles end up having high use-time (projectile shot delay) with low animation time (sword hits) for some reason
                //This is the opposite of how every other swords works (swords hits is use time, even if the animation takes longer)
                firesProjectile = true;
            }

            Item ammoItem = null;

            if (item.ranged && item.useAmmo != 0)
            {
                //try ammo slots first
                for (int i = 54; i < 58; ++i)
                {
                    Item pItem = Main.LocalPlayer.inventory[i];
                    if (pItem.ammo > 0)
                    {
                        if (pItem.ammo == item.useAmmo && pItem.Name != "")
                        {
                            ammoItem = pItem;
                            break;
                        }
                    }
                }

                //then any slot if not
                if (ammoItem == null)
                {
                    for (int i = 0; i < Main.LocalPlayer.inventory.Length; ++i)
                    {
                        Item pItem = Main.LocalPlayer.inventory[i];
                        if (pItem.ammo > 0)
                        {
                            if (pItem.ammo == item.useAmmo && pItem.Name != "")
                            {
                                ammoItem = pItem;
                                break;
                            }
                        }
                    }
                }
            }

            int speed;

            if (firesProjectile ||
                isTool)
            {
                speed = item.useAnimation;
           }
            else if (item.useAnimation > 0 &&
                item.useAnimation < 100)
            {
                speed = item.useAnimation;
            }
            else
            {
                speed = item.useTime;
            }

            //Main.NewText(item.ammo + ", " + item.useAmmo + ", " + Main.item[item.useAmmo].type + ", " + item.shoot + ", " + Main.projectile[item.shoot].arrow);
            //Main.NewText(item.useTime + ", " + item.useAnimation + ", " + speed + ", " + firesProjectile + ", " + item.reuseDelay + ", " + item.autoReuse); ;
            //The crit takes in to account the currently held item, but we only care about the item we're moused over
            //Therefore, get the difference between the two and then apply it to the player's total crit later
            int critDiff = -Main.LocalPlayer.HeldItem.crit + item.crit;

            //takes in to account melee speed increases
            float realSpeed = speed;
            //takes in to account melee damage increases
            float realDamage = 0;
            float ammoDamage = 0;

            if (ammoItem != null)
            {
                ammoDamage = ammoItem.damage;
            }

            //the average crit damage given the total crit chance, e.g. 20 damage with 50% crit is 30 crit damage
            int critDamage = 0;
            int critAmmoDamage = 0;
            if (item.melee)
            {
                realSpeed *= Main.LocalPlayer.meleeSpeed;
                realDamage = item.damage * Main.LocalPlayer.meleeDamage;
                float realCritChance =  Math.Min(1, (Main.LocalPlayer.meleeCrit + critDiff) / 100.0f); //Return 1 if above, over 100% crit doesn't actually increase damage at all
                critDamage = (int)(realDamage * (2.0f * realCritChance));
            }
            else if (item.ranged)
            {
                realDamage = item.damage * Main.LocalPlayer.rangedDamage;
                float realCritChance = Math.Min(1, (Main.LocalPlayer.rangedCrit + critDiff) / 100.0f);
                critDamage = (int)(realDamage * (2.0f * realCritChance));
                critAmmoDamage = (int)(ammoDamage * (2.0f * realCritChance));
            }
            else if (item.thrown)
            {
                realDamage = item.damage * Main.LocalPlayer.thrownDamage;
                float realCritChance = Math.Min(1, (Main.LocalPlayer.thrownCrit + critDiff) / 100.0f);
                critDamage = (int)(realDamage * (2.0f * realCritChance));
            }
            else if (item.magic)
            {
                realDamage = item.damage * Main.LocalPlayer.magicDamage;
                float realCritChance = Math.Min(1, (Main.LocalPlayer.magicCrit + critDiff) / 100.0f);
                critDamage = (int)(realDamage * (2.0f * realCritChance));
            }

            //Not sure if this should go before or after the meleeSpeed changes
            realSpeed += item.reuseDelay + (item.autoReuse ? -1 : 0);
            //60 ticks in a second, if realSpeed is <=0 just set it to cap at 60 per second, don't divide by 0/negative attacks per second)
            float attacksPerSecond = 60.0f / (realSpeed > 0 ? realSpeed : 1);
            int dps = (int)(realDamage / (1.0f / attacksPerSecond));
            int dpsCrit = (int)(critDamage / (1.0f / attacksPerSecond));
            int dpsAmmoOnly = (int)(ammoDamage / (1.0f / attacksPerSecond));
            int dpsCritAmmoOnly = (int)(critAmmoDamage / (1.0f / attacksPerSecond));
            int totalDPS = dps + dpsCrit; //excludes ammo

            for (int i = 0; i < tooltips.Count; ++i)
            {
                TooltipLine line = tooltips[i];
								
                if (settings.useHighlightColour && (line.Name == "Damage" ||
                    line.Name == "CritChance" ||
                    line.Name == "PickPower" ||
                    line.Name == "AxePower" ||
                    line.Name == "HammerPower" ||
                    line.Name == "Defense"))
                {
                    int spaceIndex = line.text.IndexOf(' ');
                    if (spaceIndex >= 0)
                    {
                        line.text = startColour + line.text.Substring(0, spaceIndex) + endColour + line.text.Substring(spaceIndex, line.text.Length - spaceIndex);
                    }
                }

                if (line.Name == "Damage" && ammoDamage > 0)
                {
                    int spaceIndex = line.text.IndexOf(" ");
                    if (spaceIndex >= 0)
                    {
                        line.text = $"{line.text.Substring(0, spaceIndex)}+{ammoDamage + line.text.Substring(spaceIndex, line.text.Length - spaceIndex)}";
                    }
                }
                else if (line.Name == "Speed" && line.text.Length >= 6)
                {
                    line.text = $"{startColour + attacksPerSecond.ToString("0.#") + endColour} attacks per second ({line.text.Substring(0, line.text.Length - 6)})"; //5 = 5 in speed + space

                    TooltipLine dpsLine = new TooltipLine(mod, "DPS", "");

                    if (ammoDamage > 0)
                    {
                        dpsLine.text = startColour + totalDPS + endColour + "+" + (dpsAmmoOnly + dpsCritAmmoOnly) + " damage per second";
                        if (dpsCrit > 0)
                        {
                            if (dpsCritAmmoOnly > 0)
                            {
                                dpsLine.text += " (" + dpsCrit + "+" + dpsCritAmmoOnly + " from crits)";
                            }
                            else
                            {
                                dpsLine.text += " (" + dpsCrit + " from crits)";
                            }
                        }
                    }
                    else
                    {
                        dpsLine.text = startColour + totalDPS + endColour + " damage per second";
                        if (dpsCrit > 0)
                        {
                            dpsLine.text += " (" + dpsCrit + " from crits)";
                        }
                    }
                    tooltips.Insert(i + 1, dpsLine);
                    i++;
                }
                else if (line.Name == "Knockback" && line.text.Length >= 10)
                {
                    if (item.knockBack > 0)
                    {
                        line.text = startColour + item.knockBack.ToString("0.#") + endColour + " knockback (" + line.text.Substring(0, line.text.Length - 10) + ")"; //10 = 9 characters in knockback + space
                    }

                    if (ammoDamage > 0 && settings.showAmmunition)
                    {
                        TooltipLine l = new TooltipLine(mod, "Ammo", "");
                        l.text = "Using Ammunition " + ammoItem.Name;
                        if (i < tooltips.Count - 1)
                        {
                            tooltips.Insert(i + 1, l);
                        }
                        else
                        {
                            tooltips.Add(l);
                        }
                    }
                }
            };

            // Player is not in an NPC's Shop
            if (Main.npcShop <= 0)
            {
                {
                    TooltipLine line = new TooltipLine(mod, "Reforge", "");
                    reforgePriceTooltip(item, line);

                    if (line.text != "")
                    {
                        tooltips.Add(line);
                    }
                }

                {
                    TooltipLine line = new TooltipLine(mod, "Sell", "");
                    sellPriceTooltip(item, line);
                    
                    if (line.text != "")
                    {
                        tooltips.Add(line);
                    }
                }
            }

            if (settings.showModName && item.modItem != null)
            {
                string startModColour = settings.showModName ? $"[c/{colourAsHexString(settings.modColour)}:" : "";
                string endModColour = settings.showModName ? "]" : "";
                tooltips.Add(new TooltipLine(mod, "ModName", $"{startModColour}{item.modItem.mod.DisplayName}{endModColour}"));
            }
        }
    }
}
