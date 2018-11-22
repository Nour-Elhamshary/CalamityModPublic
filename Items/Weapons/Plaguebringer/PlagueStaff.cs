using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Weapons.Plaguebringer
{
	public class PlagueStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Plague Staff");
			Tooltip.SetDefault("Fires a spread of plague fangs");
			Item.staff[item.type] = true;
		}

	    public override void SetDefaults()
	    {
	        item.damage = 78;
	        item.magic = true;
	        item.mana = 22;
	        item.width = 46;
	        item.height = 46;
	        item.useTime = 21;
	        item.useAnimation = 21;
	        item.useStyle = 5;
	        item.noMelee = true;
	        item.knockBack = 8;
	        item.value = 1250000;
	        item.rare = 8;
	        item.UseSound = SoundID.Item43;
	        item.autoReuse = true;
	        item.shoot = mod.ProjectileType("PlagueFang");
	        item.shootSpeed = 16f;
	    }
	    
	    public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
	    {
	    	Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
			float num72 = item.shootSpeed;
	    	float num78 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
			float num79 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
			float num80 = (float)Math.Sqrt((double)(num78 * num78 + num79 * num79));
	    	int num130 = 6;
			if (Main.rand.Next(3) == 0)
			{
				num130++;
			}
			if (Main.rand.Next(4) == 0)
			{
				num130++;
			}
			if (Main.rand.Next(5) == 0)
			{
				num130++;
			}
			for (int num131 = 0; num131 < num130; num131++)
			{
				float num132 = num78;
				float num133 = num79;
				float num134 = 0.05f * (float)num131;
				num132 += (float)Main.rand.Next(-120, 121) * num134;
				num133 += (float)Main.rand.Next(-120, 121) * num134;
				num80 = (float)Math.Sqrt((double)(num132 * num132 + num133 * num133));
				num80 = num72 / num80;
				num132 *= num80;
				num133 *= num80;
				float x2 = vector2.X;
				float y2 = vector2.Y;
				Projectile.NewProjectile(x2, y2, num132, num133, mod.ProjectileType("PlagueFang"), damage, knockBack, Main.myPlayer, 0f, 0f);
			}
			return false;
		}
	}
}