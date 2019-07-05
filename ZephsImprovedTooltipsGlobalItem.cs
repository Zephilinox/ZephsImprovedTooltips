using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;

namespace ZephsImprovedTooltipsGlobalItem
{
	class ZephsImprovedTooltipsGlobalItem : GlobalItem
	{
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
            int totalValue = (int)(item.GetStoreValue() / 3.0f);

            if (item.maxStack > 1 || totalValue == 0 || !NPC.savedGoblin)
            {
                line.text = "";
                return;
            }
            else
            {
                line.text = "Reforges for ";
                line.overrideColor = new Color(80, 140, 80);
            }

            int plat, gold, silver, copper;
            splitValue(totalValue, out plat, out gold, out silver, out copper);
            line.text += valueAsString(plat, gold, silver, copper);
        }

        public void sellPriceTooltip(Item item, TooltipLine line)
        {
            int totalValue = (int)((item.GetStoreValue() * item.stack) / 5.0f);

            if (item.type == ItemID.CopperCoin ||
                item.type == ItemID.SilverCoin ||
                item.type == ItemID.GoldCoin ||
                item.type == ItemID.PlatinumCoin ||
                totalValue == 0)
            {
                line.text = "";
                return;
            }
            else
            {
                line.text = "Sells for ";
            }

            int plat, gold, silver, copper;
            splitValue(totalValue, out plat, out gold, out silver, out copper);
            line.text += valueAsString(plat, gold, silver, copper);

            if (plat > 0)
            {
                line.overrideColor = new Color(220, 220, 198);
            }
            else if (gold > 0)
            {
                line.overrideColor = new Color(221, 199, 91);
            }
            else if (silver > 0)
            {
                line.overrideColor = new Color(181, 192, 193);
            }
            else
            {
                line.overrideColor = new Color(246, 138, 96);
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            const bool useColour = false;
            const string startColour = useColour ? "[c/ffc055:" : "";
            const string endColour = useColour ? "]" : "";
            const bool colourAllPercentages = false;

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

                if (useColour && (line.Name == "Damage" ||
                    line.Name == "CritChance" ||
                    line.Name == "PickPower" ||
                    line.Name == "AxePower" ||
                    line.Name == "HammerPower" ||
                    line.Name == "Defense"))
                {
                    int spaceIndex = line.text.IndexOf(' ');
                    if (spaceIndex >= 0)
                    {
                        line.text = startColour + line.text.Substring(0, spaceIndex) +  endColour + line.text.Substring(spaceIndex, line.text.Length - spaceIndex);
                    }
                }

                if (line.Name == "Damage" && ammoDamage > 0)
                {
                    int spaceIndex = line.text.IndexOf(" ");
                    if (spaceIndex >= 0)
                    {
                        line.text = line.text.Substring(0, spaceIndex) + "+" + ammoDamage + line.text.Substring(spaceIndex, line.text.Length - spaceIndex);
                    }
                }
                else if (line.Name == "Speed")
                {
                    //todo: come up with my own names rather than use vanilla, faster and more accurate
                    line.text = startColour + attacksPerSecond.ToString("0.#") + endColour + " attacks per second (" + line.text.Substring(0, line.text.Length - 6) + ")"; //5 = 5 in speed + space

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
                else if (line.Name == "Knockback")
                {
                    if (item.knockBack > 0)
                    {
                        line.text = startColour + item.knockBack.ToString("0.#") + endColour + " knockback (" + line.text.Substring(0, line.text.Length - 10) + ")"; //10 = 9 characters in knockback + space
                    }

                    if (ammoDamage > 0)
                    {
                        TooltipLine l = new TooltipLine(mod, "Ammo", "");
                        l.text = "Using " + ammoItem.Name;
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
                else if (!line.isModifier &&
                    !line.isModifierBad &&
                    useColour &&
                    colourAllPercentages)
                {
                    int numberStartIndex = -1;
                    int numberEndIndex = -1;
                    bool percentFound = false;

                    for (int charIndex = 0; charIndex < line.text.Length; ++charIndex)
                    {
                        if (line.text[charIndex] == '[' ||
                            line.text[charIndex] == ']')
                        {
                            //someone is using custom thingies, ignore it
                            percentFound = false;
                            numberStartIndex = -1;
                            numberEndIndex = -1;
                            continue;
                        }

                        if (numberStartIndex < 0)
                        {
                            if (line.text[charIndex] >= '0' || line.text[charIndex] <= '9')
                            {
                                numberStartIndex = charIndex;
                            }
                        }
                        else if (numberEndIndex < 0)
                        {
                            if (line.text[charIndex] < '0' || line.text[charIndex] > '9')
                            {
                                numberEndIndex = charIndex;

                                if (line.text[charIndex] == '%')
                                {
                                    if (charIndex == line.text.Length - 1 ||
                                        line.text[charIndex + 1] == ' ' || //there's a space after the %
                                        line.text[charIndex + 1] == '.' || //. after %
                                        line.text[charIndex + 1] == ',' || //etc
                                        line.text[charIndex + 1] == '\n')
                                    {
                                        percentFound = true;
                                    }
                                }
                            }
                        }

                        if (numberEndIndex >= 0) //this character turned out to be the end
                        {
                            if (percentFound)
                            {
                                line.text = line.text.Insert(numberStartIndex, "[c/ffc055:");
                                if (numberEndIndex == line.text.Length - 1) //no more characters, so append one
                                {
                                    line.text += "]";
                                }
                                else
                                {
                                    line.text = line.text.Insert(numberEndIndex + 10 + 1, "]"); //we just added 10 chars, +1 to grab %
                                }
                                charIndex += 11; //length of chars we adding
                            }
                            percentFound = false;
                            numberStartIndex = -1;
                            numberEndIndex = -1;
                        }
                    }
                }
            };

            if (Main.npcShop <= 0)
            {
                TooltipLine line = new TooltipLine(mod, "Reforge", "");
                reforgePriceTooltip(item, line);

                if (line.text != "")
                {
                    tooltips.Add(line);
                }

                TooltipLine line2 = new TooltipLine(mod, "Sell", "");
                sellPriceTooltip(item, line2);
                
                if (line2.text != "")
                {
                    tooltips.Add(line2);
                }
            }
        }
    }
}
