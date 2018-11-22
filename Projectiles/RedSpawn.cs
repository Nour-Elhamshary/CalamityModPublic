using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles
{
	public class RedSpawn : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spawn");
		}
    	
		public override void SetDefaults()
		{
			projectile.width = 6;
			projectile.height = 6;
			projectile.aiStyle = 1;
			projectile.scale = 1f;
			projectile.penetrate = 1;
			projectile.timeLeft = 20;
			projectile.tileCollide = false;
			aiType = ProjectileID.Bullet;
		}
		
		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			projectile.ai[1]++;
			
			if (projectile.ai[1] >= 0)
			{
				NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("CalamitasRun"));
				projectile.ai[1] = -30;
			}
		}
	}
}