﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class PhantomShot : ModProjectile
    {
    	public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shot");
		}
    	
        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 14;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = 4;
			cooldownSlot = 1;
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
        	projectile.penetrate--;
            if (projectile.penetrate <= 0)
            {
                projectile.Kill();
            }
            else
            {
                if (projectile.velocity.X != oldVelocity.X)
                {
                    projectile.velocity.X = -oldVelocity.X;
                }
                if (projectile.velocity.Y != oldVelocity.Y)
                {
                    projectile.velocity.Y = -oldVelocity.Y;
                }
            }
            return false;
        }

        public override void AI()
        {
        	if (projectile.ai[1] == 0f)
			{
				projectile.ai[1] = 1f;
				Main.PlaySound(SoundID.Item20, projectile.position);
			}
        	projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
			projectile.localAI[0] += 1f;
			if (projectile.localAI[0] > 9f)
			{
				projectile.alpha -= 5;
				if (projectile.alpha < 30)
				{
					projectile.alpha = 30;
				}
			}
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
        	return new Color(100, 250, 250, projectile.alpha);
        }
    }
}