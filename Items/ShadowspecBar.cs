﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items
{
    public class ShadowspecBar : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadowspec Bar");
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(8, 10));
        }

        public override void SetDefaults()
        {
            item.width = 15;
            item.height = 12;
            item.maxStack = 999;
            item.value = 1000000;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            foreach (TooltipLine line2 in list)
            {
                if (line2.mod == "Terraria" && line2.Name == "ItemName")
                {
                    line2.overrideColor = new Color(108, 45, 199);
                }
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "BarofLife", 3);
            recipe.AddIngredient(null, "Phantoplasm", 3);
            recipe.AddIngredient(null, "NightmareFuel", 3);
            recipe.AddIngredient(null, "EndothermicEnergy", 3);
            recipe.AddIngredient(null, "CalamitousEssence");
            recipe.AddIngredient(null, "DarksunFragment");
            recipe.AddIngredient(null, "HellcasterFragment");
            recipe.AddTile(null, "DraedonsForge");
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
}