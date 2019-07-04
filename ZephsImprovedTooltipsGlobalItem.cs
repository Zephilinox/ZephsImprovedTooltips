using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace ZephsImprovedTooltipsGlobalItem
{
	class ZephsImprovedTooltipsGlobalItem : GlobalItem
	{
		public ZephsImprovedTooltipsGlobalItem()
		{
		}

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            const bool useColour = false;
            const string startColour = useColour ? "[c/ffc055:" : "";
            const string endColour = useColour ? "]" : "";
            const bool colourAllPercentages = false;

            bool firesProjectile = false;
            if (item.melee &&
                item.shoot > 0 &&
                item.useAnimation <= item.useTime)
            {
                //Swords with projectiles end up having high use-time (projectile shot delay) with low animation time (sword hits) for some reason
                //This is the opposite of how every other swords works (swords hits is use time, even if the animation takes longer)
                firesProjectile = true;
            }

            int speed;
            if (firesProjectile || 
                item.useAnimation <= 0) //Fix wooden sword/light bane/etc
            {
                speed = item.useTime;
            }
            else
            {
                speed = item.useAnimation;
            }

            //The crit takes in to account the currently held item, but we only care about the item we're moused over
            //Therefore, get the difference between the two and then apply it to the player's total crit later
            int critDiff = -Main.LocalPlayer.HeldItem.crit + item.crit;

            //takes in to account melee speed increases
            float realSpeed = speed;
            //takes in to account melee damage increases
            float realDamage = 0;
            //the average crit damage given the total crit chance, e.g. 20 damage with 50% crit is 30 crit damage
            int critDamage = 0;
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
            int dps = (int)(item.damage / (1.0f / attacksPerSecond));
            int dpsCrit = (int)(critDamage / (1.0f / attacksPerSecond));
            int totalDPS = dps + dpsCrit;

            for (int i = 0; i < tooltips.Count; ++i)
            {
                TooltipLine line = tooltips[i];
                Main.NewText(line.Name);
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
                else if (line.Name == "Speed")
                {
                    //todo: come up with my own names rather than use vanilla, faster and more accurate
                    line.text = startColour + attacksPerSecond.ToString("0.#") + endColour + " attacks per second (" + line.text.Substring(0, line.text.Length - 6) + ")"; //5 = 5 in speed + space

                    TooltipLine dpsLine = new TooltipLine(mod, "DPS", startColour + totalDPS + endColour + " damage per second");
                    if (dpsCrit > 0)
                    {
                        dpsLine.text += " (" + dpsCrit + " from crits)";
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
        }
    }
}
