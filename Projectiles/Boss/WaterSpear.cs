﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class WaterSpear : ModProjectile
    {
    	public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spear");
		}
    	
        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 14;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 300;
            projectile.alpha = 255;
        }

        public override void AI()
        {
        	projectile.velocity.X *= 1.005f;
        	projectile.velocity.Y *= 1.005f;
        	if (projectile.ai[1] == 0f)
        	{
        		for (int num621 = 0; num621 < 20; num621++)
				{
					int num622 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 33, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num622].velocity *= 3f;
					if (Main.rand.Next(2) == 0)
					{
						Main.dust[num622].scale = 0.5f;
						Main.dust[num622].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
					}
				}
        		projectile.ai[1] = 1f;
        		Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 21);
        	}
        	if (projectile.localAI[0] == 0f)
			{
				projectile.scale -= 0.01f;
				projectile.alpha += 10;
				if (projectile.alpha >= 100)
				{
					projectile.alpha = 100;
					projectile.localAI[0] = 1f;
				}
			}
			else if (projectile.localAI[0] == 1f)
			{
				projectile.scale += 0.01f;
				projectile.alpha -= 10;
				if (projectile.alpha <= 0)
				{
					projectile.alpha = 0;
					projectile.localAI[0] = 0f;
				}
			}
			projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
        	Lighting.AddLight(projectile.Center, ((255 - projectile.alpha) * 0f) / 255f, ((255 - projectile.alpha) * 0.05f) / 255f, ((255 - projectile.alpha) * 0.35f) / 255f);
            if (Main.rand.Next(3) == 0)
            {
            	Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 33, projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f);
            }
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, projectile.GetAlpha(lightColor), projectile.rotation, tex.Size() / 2f, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
            	Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 33, projectile.oldVelocity.X * 0.5f, projectile.oldVelocity.Y * 0.5f);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
        	target.AddBuff(BuffID.Wet, 240);
        }
    }
}