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
	public class PaintballBlaster : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Speed Blaster");
		}

	    public override void SetDefaults()
	    {
	        item.damage = 26;
	        item.ranged = true;
	        item.width = 54;
	        item.height = 26;
	        item.useAnimation = 24;
	        item.reuseDelay = 9;
	        item.useTime = 4;
	        item.useStyle = 5;
	        item.noMelee = true;
	        item.knockBack = 2.25f;
	        item.value = 300000;
	        item.rare = 5;
	        item.UseSound = null;
	        item.autoReuse = true;
	        item.shootSpeed = 20f;
	        item.shoot = 587;
	    }
	    
	    public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			float num72 = item.shootSpeed;
	    	Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
	    	float num78 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
			float num79 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
			if (player.gravDir == -1f)
			{
				num79 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector2.Y;
			}
			float num80 = (float)Math.Sqrt((double)(num78 * num78 + num79 * num79));
			float num81 = num80;
			if ((float.IsNaN(num78) && float.IsNaN(num79)) || (num78 == 0f && num79 == 0f))
			{
				num78 = (float)player.direction;
				num79 = 0f;
				num80 = num72;
			}
			else
			{
				num80 = num72 / num80;
			}
	    	float num208 = num78;
			float num209 = num79;
			num208 += (float)Main.rand.Next(-1, 2) * 0.5f;
			num209 += (float)Main.rand.Next(-1, 2) * 0.5f;
			if (Collision.CanHitLine(player.Center, 0, 0, vector2 + new Vector2(num208, num209) * 2f, 0, 0))
			{
				vector2 += new Vector2(num208, num209);
			}
			Projectile.NewProjectile(position.X, position.Y - player.gravDir * 4f, num208, num209, 587, damage, knockBack, player.whoAmI, 0f, (float)Main.rand.Next(12) / 6f);
	    	return false;
		}
	
	    public override void AddRecipes()
	    {
	        ModRecipe recipe = new ModRecipe(mod);
	        recipe.AddIngredient(ItemID.PainterPaintballGun);
	        recipe.AddIngredient(ItemID.SoulofSight, 5);
	        recipe.AddIngredient(ItemID.HallowedBar, 9);
	        recipe.AddTile(TileID.MythrilAnvil);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
	    }
	}
}