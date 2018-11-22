﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles;

namespace CalamityMod.NPCs.SlimeGod
{
	[AutoloadBossHead]
	public class SlimeGodCore : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("The Slime God");
		}
		
		public override void SetDefaults()
		{
			npc.damage = 50;
			npc.npcSlots = 10f;
			npc.width = 44; //324
			npc.height = 44; //216
			npc.defense = 0;
			npc.lifeMax = CalamityWorld.revenge ? 3750 : 3000;
            if (CalamityWorld.death)
            {
                npc.lifeMax = 5250;
            }
            if (CalamityWorld.bossRushActive)
            {
                npc.lifeMax = CalamityWorld.death ? 3000000 : 2700000;
            }
            npc.aiStyle = -1; //new
            aiType = -1; //new
			npc.knockBackResist = 0f;
			npc.value = Item.buyPrice(0, 8, 0, 0);
			npc.alpha = 80;
			animationType = 10;
			npc.boss = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/SlimeGod");
            bossBag = mod.ItemType("SlimeGodBag");
		}
		
		public override void AI()
		{
			bool expertMode = (Main.expertMode || CalamityWorld.bossRushActive);
			bool revenge = (CalamityWorld.revenge || CalamityWorld.bossRushActive);
			Player player = Main.player[npc.target];
			int randomDust = Main.rand.Next(2);
			if (randomDust == 0)
			{
				randomDust = 173;
			}
			else
			{
				randomDust = 260;
			}
			int num658 = Dust.NewDust(npc.position, npc.width, npc.height, randomDust, npc.velocity.X, npc.velocity.Y, 255, new Color(0, 80, 255, 80), npc.scale * 1.5f);
			Main.dust[num658].noGravity = true;
			Main.dust[num658].velocity *= 0.5f;
			bool flag100 = false;
            if ((NPC.AnyNPCs(mod.NPCType("SlimeGod")) ||
                NPC.AnyNPCs(mod.NPCType("SlimeGodSplit")) ||
                NPC.AnyNPCs(mod.NPCType("SlimeGodRun")) ||
                NPC.AnyNPCs(mod.NPCType("SlimeGodRunSplit"))) && 
                !CalamityWorld.bossRushActive)
            {
                flag100 = true;
            }
			if (!player.active || player.dead)
			{
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				if (!player.active || player.dead)
				{
					npc.velocity = new Vector2(0f, 30f);
                    if ((double)npc.position.Y > Main.rockLayer * 16.0)
                    {
                        for (int x = 0; x < 200; x++)
                        {
                            if (Main.npc[x].type == mod.NPCType("SlimeGod") || Main.npc[x].type == mod.NPCType("SlimeGodSplit") ||
                                Main.npc[x].type == mod.NPCType("SlimeGodRun") || Main.npc[x].type == mod.NPCType("SlimeGodRunSplit"))
                            {
                                Main.npc[x].active = false;
                                Main.npc[x].netUpdate = true;
                            }
                        }
                        npc.active = false;
                        npc.netUpdate = true;
                    }
                    return;
				}
			}
			else if (npc.timeLeft < 2400)
			{
				npc.timeLeft = 2400;
			}
			if (!flag100)
			{
				npc.damage = 75;
				if (Main.netMode != 1)
				{
                    npc.localAI[1] += (npc.GetGlobalNPC<CalamityGlobalNPC>(mod).enraged ? 2f : 1f);
                    if (expertMode && Main.rand.Next(2) == 0)
                    {
                        if (npc.localAI[0] >= 75f)
						{
							npc.localAI[0] = 0f;
							npc.TargetClosest(true);
							if (Collision.CanHit(npc.position, npc.width, npc.height, player.position, player.width, player.height))
							{
								float num179 = revenge ? 2f : 3f;
								Vector2 value9 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
								float num180 = player.position.X + (float)player.width * 0.5f - value9.X;
								float num181 = Math.Abs(num180) * 0.1f;
								float num182 = player.position.Y + (float)player.height * 0.5f - value9.Y - num181;
								float num183 = (float)Math.Sqrt((double)(num180 * num180 + num182 * num182));
								npc.netUpdate = true;
								num183 = num179 / num183;
								num180 *= num183;
								num182 *= num183;
								int num184 = 21;
								int num185 = Main.rand.Next(2);
								if (num185 == 0)
								{
									num185 = mod.ProjectileType("AbyssMine");
								}
								else
								{
									num185 = mod.ProjectileType("AbyssMine2");
                                    num184 = 19;
                                }
								value9.X += num180;
								value9.Y += num182;
								num180 = player.position.X + (float)player.width * 0.5f - value9.X;
								num182 = player.position.Y + (float)player.height * 0.5f - value9.Y;
								num183 = (float)Math.Sqrt((double)(num180 * num180 + num182 * num182));
								num183 = num179 / num183;
								num180 *= num183;
								num182 *= num183;
								Projectile.NewProjectile(value9.X, value9.Y, num180, num182, num185, num184, 0f, Main.myPlayer, 0f, 0f);
							}
						}
					}
					else if (npc.localAI[1] >= 75f)
					{
						npc.localAI[1] = 0f;
						npc.TargetClosest(true);
						if (Collision.CanHit(npc.position, npc.width, npc.height, player.position, player.width, player.height))
						{
							float num179 = revenge ? 6f : 5f;
							Vector2 value9 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
							float num180 = player.position.X + (float)player.width * 0.5f - value9.X;
							float num181 = Math.Abs(num180) * 0.1f;
							float num182 = player.position.Y + (float)player.height * 0.5f - value9.Y - num181;
							float num183 = (float)Math.Sqrt((double)(num180 * num180 + num182 * num182));
							npc.netUpdate = true;
							num183 = num179 / num183;
							num180 *= num183;
							num182 *= num183;
							int num184 = expertMode ? 16 : 18;
							int num185 = Main.rand.Next(2);
							if (num185 == 0)
							{
								num185 = mod.ProjectileType("AbyssBallVolley");
							}
							else
							{
								num185 = mod.ProjectileType("AbyssBallVolley2");
                                num184 = expertMode ? 14 : 16;
                            }
							value9.X += num180;
							value9.Y += num182;
							for (int num186 = 0; num186 < 2; num186++)
							{
								num180 = player.position.X + (float)player.width * 0.5f - value9.X;
								num182 = player.position.Y + (float)player.height * 0.5f - value9.Y;
								num183 = (float)Math.Sqrt((double)(num180 * num180 + num182 * num182));
								num183 = num179 / num183;
								num180 += (float)Main.rand.Next(-30, 31);
								num182 += (float)Main.rand.Next(-30, 31);
								num180 *= num183;
								num182 *= num183;
								Projectile.NewProjectile(value9.X, value9.Y, num180, num182, num185, num184, 0f, Main.myPlayer, 0f, 0f);
							}
						}
					}
				}
			}
			npc.TargetClosest(true);
			float num1372 = 6f;
			if (!flag100)
			{
				num1372 = 14f;
			}
			else if (revenge)
			{
				num1372 = 10f;
			}
            if (CalamityWorld.bossRushActive)
            {
                num1372 = 21f;
            }
            if (npc.GetGlobalNPC<CalamityGlobalNPC>(mod).enraged)
            {
                num1372 += 8f;
            }
			Vector2 vector167 = new Vector2(npc.Center.X + (float)(npc.direction * 20), npc.Center.Y + 6f);
			float num1373 = player.position.X + (float)player.width * 0.5f - vector167.X;
			float num1374 = player.Center.Y - vector167.Y;
			float num1375 = (float)Math.Sqrt((double)(num1373 * num1373 + num1374 * num1374));
			float num1376 = num1372 / num1375;
			num1373 *= num1376;
			num1374 *= num1376;
			npc.ai[0] -= 1f;
			if (num1375 < 200f || npc.ai[0] > 0f)
			{
				if (num1375 < 200f)
				{
					npc.ai[0] = 20f;
				}
				if (npc.velocity.X < 0f)
				{
					npc.direction = -1;
				}
				else
				{
					npc.direction = 1;
				}
				return;
			}
			npc.velocity.X = (npc.velocity.X * 50f + num1373) / 51f;
			npc.velocity.Y = (npc.velocity.Y * 50f + num1374) / 51f;
			if (num1375 < 350f)
			{
				npc.velocity.X = (npc.velocity.X * 10f + num1373) / 11f;
				npc.velocity.Y = (npc.velocity.Y * 10f + num1374) / 11f;
			}
			if (num1375 < 300f)
			{
				npc.velocity.X = (npc.velocity.X * 7f + num1373) / 8f;
				npc.velocity.Y = (npc.velocity.Y * 7f + num1374) / 8f;
			}
			return;
		}
		
		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.HealingPotion;
		}
		
		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 5; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, 4, hitDirection, -1f, 0, default(Color), 1f);
			}
			if (npc.life <= 0)
			{
				npc.position.X = npc.position.X + (float)(npc.width / 2);
				npc.position.Y = npc.position.Y + (float)(npc.height / 2);
				npc.width = 40;
				npc.height = 40;
				npc.position.X = npc.position.X - (float)(npc.width / 2);
				npc.position.Y = npc.position.Y - (float)(npc.height / 2);
				for (int num621 = 0; num621 < 40; num621++)
				{
					int num622 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 4, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num622].velocity *= 3f;
					if (Main.rand.Next(2) == 0)
					{
						Main.dust[num622].scale = 0.5f;
						Main.dust[num622].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
					}
				}
				for (int num623 = 0; num623 < 70; num623++)
				{
					int num624 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 4, 0f, 0f, 100, default(Color), 3f);
					Main.dust[num624].noGravity = true;
					Main.dust[num624].velocity *= 5f;
					num624 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 4, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num624].velocity *= 2f;
				}
			}
		}
		
		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax * 0.8f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.8f);
		}
		
		public override void OnHitPlayer(Player player, int damage, bool crit)
		{
			player.AddBuff(BuffID.VortexDebuff, 180, true);
			if (CalamityWorld.revenge)
			{
				player.AddBuff(mod.BuffType("Horror"), 300, true);
				player.AddBuff(mod.BuffType("MarkedforDeath"), 300);
			}
		}
	}
}