﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles
{
    public class MagnaBlast : ModProjectile
    {
    	public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blast");
		}
    	
        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 32;
            projectile.scale = 0.9f;
            projectile.friendly = true;
            projectile.magic = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
        }
        
        public override void AI()
		{
        	if (projectile.scale <= 2.2f)
        	{
        		projectile.scale *= 1.05f;
        	}
        	projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
        	Lighting.AddLight(projectile.Center, ((255 - projectile.alpha) * 0.1f) / 255f, ((255 - projectile.alpha) * 0.1f) / 255f, ((255 - projectile.alpha) * 1f) / 255f);
        	projectile.localAI[0] += 1f;
			if (projectile.localAI[0] > 4f)
			{
				for (int num468 = 0; num468 < 3; num468++)
				{
					int num469 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 59, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num469].noGravity = true;
					Main.dust[num469].velocity *= 0f;
				}
			}
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 10; k++)
            {
            	Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 59, projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f);
            }
        }
    }
}