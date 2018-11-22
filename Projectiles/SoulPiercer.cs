﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles
{
    public class SoulPiercer : ModProjectile
    {
    	public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Piercer");
		}
    	
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;
            projectile.friendly = true;
            projectile.magic = true;
            projectile.penetrate = -1;
            projectile.extraUpdates = 100;
            projectile.timeLeft = 180;
        }

        public override void AI()
        {
        	if (Main.rand.Next(5) == 0)
        	{
        		if (projectile.owner == Main.myPlayer)
        		{
            		Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, projectile.velocity.X * 0.35f, projectile.velocity.Y * 0.35f, mod.ProjectileType("SoulPiercerOrb"), (int)((double)projectile.damage), projectile.knockBack, projectile.owner, 0f, 0f);
        		}
        	}
        	if (projectile.velocity.X != projectile.velocity.X)
			{
				projectile.position.X = projectile.position.X + projectile.velocity.X;
				projectile.velocity.X = -projectile.velocity.X;
			}
			if (projectile.velocity.Y != projectile.velocity.Y)
			{
				projectile.position.Y = projectile.position.Y + projectile.velocity.Y;
				projectile.velocity.Y = -projectile.velocity.Y;
			}
			projectile.localAI[0] += 1f;
			if (projectile.localAI[0] > 9f)
			{
				for (int num447 = 0; num447 < 4; num447++)
				{
					Vector2 vector33 = projectile.position;
					vector33 -= projectile.velocity * ((float)num447 * 0.25f);
					projectile.alpha = 255;
					int num448 = Dust.NewDust(vector33, 1, 1, 173, 0f, 0f, 0, default(Color), 1f);
					Main.dust[num448].position = vector33;
					Main.dust[num448].scale = (float)Main.rand.Next(70, 110) * 0.013f;
					Main.dust[num448].velocity *= 0.2f;
				}
				return;
			}
        }
    }
}