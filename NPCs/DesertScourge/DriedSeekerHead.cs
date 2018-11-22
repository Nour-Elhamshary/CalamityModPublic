﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles;
using Terraria.World.Generation;
using Terraria.GameContent.Generation;
using CalamityMod.Tiles;

namespace CalamityMod.NPCs.DesertScourge
{
	public class DriedSeekerHead : ModNPC
	{
		bool TailSpawned = false;
		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dried Seeker");
		}
		
		public override void SetDefaults()
		{
			npc.damage = 23; //150
			npc.npcSlots = 5f;
			npc.width = 18; //324
			npc.height = 18; //216
			npc.defense = 0;
			npc.lifeMax = 100; //250000
            if (CalamityWorld.bossRushActive)
            {
                npc.lifeMax = 50000;
            }
            npc.aiStyle = 6;
            aiType = -1;
            animationType = 10;
			npc.knockBackResist = 0f;
			npc.value = Item.buyPrice(0, 0, 0, 0);
			npc.behindTiles = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.netAlways = true;
		}
		
		public override void AI()
		{
			if (!TailSpawned)
            {
                int Previous = npc.whoAmI;
                for (int num36 = 0; num36 < 4; num36++)
                {
                    int lol = 0;
                    if (num36 >= 0 && num36 < 3)
                    {
                        lol = NPC.NewNPC((int)npc.position.X + (npc.width / 2), (int)npc.position.Y + (npc.height / 2), mod.NPCType("DriedSeekerBody"), npc.whoAmI);
                    }
                    else
                    {
                        lol = NPC.NewNPC((int)npc.position.X + (npc.width / 2), (int)npc.position.Y + (npc.height / 2), mod.NPCType("DriedSeekerTail"), npc.whoAmI);
                    }
                    Main.npc[lol].realLife = npc.whoAmI;
                    Main.npc[lol].ai[2] = (float)npc.whoAmI;
                    Main.npc[lol].ai[1] = (float)Previous;
                    Main.npc[Previous].ai[0] = (float)lol;
                    NetMessage.SendData(23, -1, -1, null, lol, 0f, 0f, 0f, 0);
                    Previous = lol;
                }
                TailSpawned = true;
            }
			if (Main.player[npc.target].dead)
			{
				npc.velocity.Y = npc.velocity.Y + 1f;
				if ((double)npc.position.Y > Main.worldSurface * 16.0)
				{
					npc.velocity.Y = npc.velocity.Y + 1f;
				}
				if ((double)npc.position.Y > Main.rockLayer * 16.0)
				{
					for (int num957 = 0; num957 < 200; num957++)
					{
						if (Main.npc[num957].aiStyle == npc.aiStyle)
						{
							Main.npc[num957].active = false;
						}
					}
				}
			}
		}
		
		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 3; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
			}
			if (npc.life <= 0)
			{
				for (int k = 0; k < 10; k++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
				}
			}
		}
		
		public override bool PreNPCLoot()
		{
			return false;
		}
		
		public override void OnHitPlayer(Player player, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				player.AddBuff(BuffID.Bleeding, 200, true);
			}
		}
	}
}