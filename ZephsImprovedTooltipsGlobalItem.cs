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
            bool firesProjectile = false;
            if (item.melee &&
                item.shoot > 0 &&
                item.useAnimation <= item.useTime)
            {
                //Swords with projectiles end up having high use-time (projectile shot delay) with low animation time (sword hits) for some reason
                //This is the opposite of how every other swords works (swords hits is use time, even if the animation takes longer)
                firesProjectile = true;
            }

            int speed = (firesProjectile ? item.useAnimation : item.useTime);

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

                if (line.Name == "Speed")
                {
                    line.text += " (" + attacksPerSecond.ToString("0.#") + " times per second)";

                    if (dpsCrit > 0)
                    {
                        TooltipLine dpsLine = new TooltipLine(mod, "DPS", totalDPS + " damage per second" + " (" + dpsCrit + " from crits)");
                        tooltips.Insert(i + 1, dpsLine);
                        i++;
                    }
                    else
                    {
                        TooltipLine dpsLine = new TooltipLine(mod, "DPS", dps + " damage per second");
                        tooltips.Insert(i + 1, dpsLine);
                        i++;
                    }
                }
                else if (line.Name == "Knockback")
                {
                    if (item.knockBack > 0)
                    {
                        line.text += " (" + item.knockBack + ")";
                    }
                }                
            };
        }
    }
}
