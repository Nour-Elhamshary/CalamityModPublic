using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Weapons {
public class WulfrumBlade : ModItem
{
	public override void SetStaticDefaults()
	{
		DisplayName.SetDefault("Wulfrum Blade");
	}

	public override void SetDefaults()
	{
		item.width = 46;
		item.damage = 15;
		item.melee = true;
		item.useAnimation = 20;
		item.useStyle = 1;
		item.useTime = 20;
		item.useTurn = true;
		item.knockBack = 3.75f;
		item.UseSound = SoundID.Item1;
		item.autoReuse = true;
		item.height = 54;
		item.value = 20000;
		item.rare = 1;
	}
	
	public override void AddRecipes()
	{
		ModRecipe recipe = new ModRecipe(mod);
        recipe.AddIngredient(null, "WulfrumShard", 12);
        recipe.AddTile(TileID.Anvils);
        recipe.SetResult(this);
        recipe.AddRecipe();
	}
}}
