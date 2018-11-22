using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Weapons
{
	public class ClockGatlignum : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Clock Gatlignum");
			Tooltip.SetDefault("33% chance to not consume ammo");
		}

	    public override void SetDefaults()
	    {
	        item.damage = 45;
	        item.ranged = true;
	        item.width = 66;
	        item.height = 34;
	        item.useTime = 2;
	        item.reuseDelay = 10;
	        item.useAnimation = 6;
	        item.useStyle = 5;
	        item.noMelee = true;
	        item.knockBack = 3.75f;
	        item.value = 1200000;
	        item.rare = 8;
	        item.UseSound = SoundID.Item31;
	        item.autoReuse = true;
	        item.shoot = 10;
	        item.shootSpeed = 20f;
	        item.useAmmo = 97;
	    }
	    
	    public override Vector2? HoldoutOffset()
		{
			return new Vector2(-5, 0);
		}
	    
	    public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
		    float SpeedX = speedX + (float) Main.rand.Next(-15, 16) * 0.05f;
		    float SpeedY = speedY + (float) Main.rand.Next(-15, 16) * 0.05f;
		    Projectile.NewProjectile(position.X, position.Y, SpeedX, SpeedY, 242, damage, knockBack, player.whoAmI, 0.0f, 0.0f);
		    return false;
		}
	    
	    public override bool ConsumeAmmo(Player player)
	    {
	    	if (Main.rand.Next(0, 100) <= 33)
	    		return false;
	    	return true;
	    }
	
	    public override void AddRecipes()
	    {
	        ModRecipe recipe = new ModRecipe(mod);
	        recipe.AddIngredient(ItemID.Gatligator);
	        recipe.AddIngredient(ItemID.VenusMagnum);
	        recipe.AddIngredient(ItemID.HallowedBar, 5);
	        recipe.AddIngredient(ItemID.Ectoplasm, 5);
	        recipe.AddTile(TileID.MythrilAnvil);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
	    }
	}
}