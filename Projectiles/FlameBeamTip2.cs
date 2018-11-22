﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles
{
    public class FlameBeamTip2 : ModProjectile
    {
    	public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Beam");
		}
    	
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.aiStyle = 4;
            projectile.friendly = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.magic = true;
        }

        public override void AI()
        {
        	projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
        	if (projectile.ai[0] == 0f)
			{
        		projectile.alpha -= 50;
        		if (projectile.alpha <= 0)
				{
					projectile.alpha = 0;
					projectile.ai[0] = 1f;
					if (projectile.ai[1] == 0f)
					{
						projectile.ai[1] += 1f;
						projectile.position += projectile.velocity * 1f;
					}
        		}
        	}
        	else
			{
				if (projectile.alpha < 170 && projectile.alpha + 5 >= 170)
				{
					for (int num55 = 0; num55 < 8; num55++)
					{
						int num56 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 60, projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 200, default(Color), 1f);
						Main.dust[num56].noGravity = true;
						Main.dust[num56].velocity *= 0.5f;
					}
				}
				projectile.alpha += 7;
				if (projectile.alpha >= 255)
				{
					projectile.Kill();
					return;
				}
        	}
            if (Main.rand.Next(4) == 0)
            {
            	Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 60, 0f, 0f);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 3; k++)
            {
            	Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 60, 0f, 0f);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
        	target.immune[projectile.owner] = 8;
        	target.AddBuff(BuffID.OnFire, 240);
        }
    }
}