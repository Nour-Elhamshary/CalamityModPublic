﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Utilities;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityMod.Projectiles;

namespace CalamityMod.NPCs.Yharon
{
	[AutoloadBossHead]
	public class Yharon : ModNPC
	{
		private Rectangle safeBox = default(Rectangle);
        private bool protectionBoost = false;
        private bool moveCloser = false;
        private bool phaseOneLoot = false;
        private bool dropLoot = false;
        private int healCounter = 0;

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Jungle Dragon, Yharon");
			Main.npcFrameCount[npc.type] = 7;
		}
		
		public override void SetDefaults()
		{
			npc.npcSlots = 50f;
			npc.damage = 330;
			npc.width = 200; //150
			npc.height = 200; //100
			npc.defense = 200;
			npc.lifeMax = CalamityWorld.revenge ? 2625000 : 2375000;
            if (CalamityWorld.death)
            {
                npc.lifeMax = 3125000;
            }
            if (CalamityWorld.bossRushActive)
            {
                npc.lifeMax = CalamityWorld.death ? 10000000 : 9000000;
            }
            npc.knockBackResist = 0f;
			npc.aiStyle = -1; //new
            aiType = -1; //new
			npc.value = Item.buyPrice(10, 0, 0, 0);
			npc.boss = true;
			for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
				npc.buffImmune[BuffID.Ichor] = false;
                npc.buffImmune[BuffID.CursedInferno] = false;
                npc.buffImmune[mod.BuffType("DemonFlames")] = false;
                npc.buffImmune[mod.BuffType("Shred")] = false;
            }
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.netAlways = true;
			music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/YHARON");
            if (CalamityWorld.downedProvidence && CalamityWorld.downedDoG && CalamityWorld.downedBuffedMothron || CalamityWorld.bossRushActive)
            {
                music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/YHARONREBIRTH");
            }
            npc.HitSound = SoundID.NPCHit56;
			npc.DeathSound = SoundID.NPCDeath60;
			bossBag = mod.ItemType("YharonBag");
		}
		
		public override void AI()
		{
            #region StartupOrSwitchToAI2
            dropLoot = (double)npc.life <= (double)npc.lifeMax * 0.1;
            if (Main.raining)
            {
                Main.raining = false;
                if (Main.netMode == 2)
                {
                    NetMessage.SendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
                }
            }
            if (npc.localAI[2] == 1f)
            {
                if ((!CalamityWorld.downedProvidence || !CalamityWorld.downedDoG) && !CalamityWorld.bossRushActive)
                {
                    phaseOneLoot = true;
                    npc.DeathSound = null;
                    npc.dontTakeDamage = true;
                    npc.chaseable = false;
                    npc.velocity.Y = npc.velocity.Y - 0.4f;
                    if (npc.alpha < 255)
                    {
                        npc.alpha += 5;
                        if (npc.alpha > 255)
                        {
                            npc.alpha = 255;
                        }
                    }
                    if (npc.timeLeft > 55)
                    {
                        npc.timeLeft = 55;
                    }
                    if (npc.timeLeft < 5)
                    {
                        string key = "Mods.CalamityMod.DargonBossText3";
                        Color messageColor = Color.Orange;
                        if (Main.netMode == 0)
                        {
                            Main.NewText(Language.GetTextValue(key), messageColor);
                        }
                        else if (Main.netMode == 2)
                        {
                            NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
                        }
                        npc.localAI[2] = 2f;
                        npc.boss = false;
                        npc.life = 0;
                        if (dropLoot)
                        {
                            npc.NPCLoot();
                        }
                        npc.active = false;
                        npc.netUpdate = true;
                    }
                    return;
                }
                if (!CalamityWorld.downedBuffedMothron && !CalamityWorld.bossRushActive)
                {
                    phaseOneLoot = true;
                    npc.DeathSound = null;
                    npc.dontTakeDamage = true;
                    npc.chaseable = false;
                    npc.velocity.Y = npc.velocity.Y - 0.4f;
                    if (npc.alpha < 255)
                    {
                        npc.alpha += 5;
                        if (npc.alpha > 255)
                        {
                            npc.alpha = 255;
                        }
                    }
                    if (npc.timeLeft > 55)
                    {
                        npc.timeLeft = 55;
                    }
                    if (npc.timeLeft < 5)
                    {
                        string key = "Mods.CalamityMod.DargonBossText2";
                        Color messageColor = Color.Orange;
                        if (Main.netMode == 0)
                        {
                            Main.NewText(Language.GetTextValue(key), messageColor);
                        }
                        else if (Main.netMode == 2)
                        {
                            NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
                        }
                        npc.localAI[2] = 0f;
                        npc.boss = false;
                        npc.life = 0;
                        if (dropLoot)
                        {
                            npc.NPCLoot();
                        }
                        npc.active = false;
                        npc.netUpdate = true;
                    }
                    return;
                }
                phaseOneLoot = false;
                Yharon_AI2();
                return;
            }
            bool revenge = (CalamityWorld.revenge || CalamityWorld.bossRushActive);
			bool expertMode = Main.expertMode;
			bool phase2Check = (double)npc.life <= (double)npc.lifeMax * (revenge ? 0.85 : 0.7); //Check for phase 2.  In phase 1 for 15% or 30%
			bool phase3Check = (double)npc.life <= (double)npc.lifeMax * (revenge ? 0.6 : 0.4); //Check for phase 3.  In phase 2 for 25% or 30%
			bool phase4Check = (double)npc.life <= (double)npc.lifeMax * (revenge ? 0.25 : 0.2); //Check for phase 4.  In phase 3 for 35% or 20%
            if (CalamityWorld.death && !CalamityWorld.bossRushActive)
            {
                phase2Check = (double)npc.life <= (double)npc.lifeMax * 0.9; //Check for phase 2.  In phase 1 for 10%
                phase3Check = (double)npc.life <= (double)npc.lifeMax * 0.8; //Check for phase 3.  In phase 2 for 10%
                phase4Check = (double)npc.life <= (double)npc.lifeMax * 0.3; //Check for phase 4.  In phase 3 for 50%
            }
            bool phase5Check = (double)npc.life <= (double)npc.lifeMax * 0.1; //Check for phase 5.  In phase 4 for 15% or 10% or 20%
            bool phase2Change = npc.ai[0] > 5f; //Phase 2 stuff
            bool phase3Change = npc.ai[0] > 12f; //Phase 3 stuff
            bool phase4Change = npc.ai[0] > 20f; //Phase 4 stuff
            int flareCount = 3;
            bool isCharging = npc.ai[3] < 20f; //10
            if (npc.localAI[1] == 0f) 
			{
				npc.localAI[1] = 1f;
                Vector2 vectorPlayer = new Vector2(Main.player[npc.target].position.X, Main.player[npc.target].position.Y);
				safeBox.X = (int)(vectorPlayer.X - (revenge ? 3000f : 3500f));
				safeBox.Y = (int)(vectorPlayer.Y - (revenge ? 6000f : 7000f));
				safeBox.Width = revenge ? 6000 : 7000;
				safeBox.Height = revenge ? 12000 : 14000;
				if (Main.netMode != 1)
				{
					Projectile.NewProjectile(Main.player[npc.target].position.X + (revenge ? 3000f : 3500f), Main.player[npc.target].position.Y + 100f, 0f, 0f, mod.ProjectileType("SkyFlareRevenge"), 0, 0f, Main.myPlayer, 0f, 0f);
					Projectile.NewProjectile(Main.player[npc.target].position.X - (revenge ? 3000f : 3500f), Main.player[npc.target].position.Y + 100f, 0f, 0f, mod.ProjectileType("SkyFlareRevenge"), 0, 0f, Main.myPlayer, 0f, 0f);
				}
			}
            #endregion
            #region SpeedOrDamageChecks
            float teleportLocation = 0f;
			int teleChoice = Main.rand.Next(2);
			if (teleChoice == 0)
			{
				teleportLocation = revenge ? 500f : 600f;
			}
			else
			{
				teleportLocation = revenge ? -500f : -600f;
			}
			if (phase4Change)
			{
				npc.defense = 140;
			}
			else if (phase3Change)
			{
				npc.defense = 160;
			} 
			else if (phase2Change)
			{
				npc.defense = 180;
			} 
			else 
			{
				npc.defense = 200;
			}
			int aiChangeRate = expertMode ? 36 : 38;
			float npcVelocity = expertMode ? 0.7f : 0.69f;
			float scaleFactor = expertMode ? 11f : 10.8f;
			if (phase4Change || npc.GetGlobalNPC<CalamityGlobalNPC>(mod).enraged)
			{
				npcVelocity = 0.95f;
				scaleFactor = 14f;
				aiChangeRate = 25;
			}
			else if (phase3Change)
			{
				npcVelocity = 0.9f;
				scaleFactor = 13f;
				aiChangeRate = 25;
			} 
			else if (phase2Change && isCharging)
			{
				npcVelocity = (expertMode ? 0.8f : 0.78f);
				scaleFactor = (expertMode ? 12.2f : 12f);
				aiChangeRate = (expertMode ? 36 : 38);
			}
			else if (isCharging && !phase2Change && !phase3Change && !phase4Change) 
			{
				aiChangeRate = 25;
			}
			int chargeTime = expertMode ? 38 : 40;
            int chargeTime2 = expertMode ? 34 : 36;
			float chargeSpeed = expertMode ? 22f : 20.5f; //17 and 16
            float chargeSpeed2 = expertMode ? 37f : 34f;
			if (phase4Change || npc.GetGlobalNPC<CalamityGlobalNPC>(mod).enraged) //phase 4
			{
				chargeTime = 28;
				chargeSpeed = 31f; //27
			} 
			else if (phase3Change) //phase 3
			{
				chargeTime = 32;
                chargeTime2 = 27;
				chargeSpeed = 25f; //27
                chargeSpeed2 = 41f;
			}
			else if (isCharging && phase2Change) //phase 2
			{
				chargeTime = expertMode ? 35 : 37;
                chargeTime2 = expertMode ? 31 : 33;
				if (expertMode)
				{
					chargeSpeed = 24f; //21
                    chargeSpeed2 = 39f;
				}
			}
            #endregion
            #region VariablesForChargingEtc
            int num1454 = 80;
			int num1455 = 4;
			float num1456 = 0.3f;
			float scaleFactor11 = 5f;
			int num1457 = 90;
			int num1458 = 180;
			int num1459 = 180;
			int num1460 = 30;
			int num1461 = 120;
			int num1462 = 4;
			float scaleFactor13 = 20f;
			float num1463 = 6.28318548f / (float)(num1461 / 2);
			int num1464 = 75;
            #endregion
            #region DespawnOrEnrage
            Vector2 vectorCenter = npc.Center;
			Player player = Main.player[npc.target];
			if (npc.target < 0 || npc.target == 255 || player.dead || !player.active) 
			{
				npc.TargetClosest(true);
				player = Main.player[npc.target];
				npc.netUpdate = true;
			}
			if (player.dead)
			{
				npc.velocity.Y = npc.velocity.Y - 0.4f;
				if (npc.timeLeft > 150)
				{
					npc.timeLeft = 150;
				}
                if (npc.ai[0] > 12f)
                {
                    npc.ai[0] = 13f;
                }
                else if (npc.ai[0] > 5f)
				{
					npc.ai[0] = 6f;
				}
				else
				{
					npc.ai[0] = 0f;
				}
				npc.ai[2] = 0f;
			}
			else if (npc.timeLeft < 3600)
			{
				npc.timeLeft = 3600;
			}
			if (!Main.player[npc.target].Hitbox.Intersects(safeBox))
			{
				aiChangeRate = 15;
				protectionBoost = true;
				npc.damage = npc.defDamage * 5;
				chargeSpeed += 25f;
			}
			else
			{
				protectionBoost = false;
			}
            #endregion
            #region Rotation
            if (npc.localAI[0] == 0f) 
			{
				npc.localAI[0] = 1f;
				npc.alpha = 255;
				npc.rotation = 0f;
				if (Main.netMode != 1) 
				{
					npc.ai[0] = -1f;
					npc.netUpdate = true;
				}
			}
			float npcRotation = (float)Math.Atan2((double)(player.Center.Y - vectorCenter.Y), (double)(player.Center.X - vectorCenter.X));
			if (npc.spriteDirection == 1)
			{
				npcRotation += 3.14159274f;
			}
			if (npcRotation < 0f) 
			{
				npcRotation += 6.28318548f;
			}
			if (npcRotation > 6.28318548f) 
			{
				npcRotation -= 6.28318548f;
			}
			if (npc.ai[0] == -1f) //spawn
			{
				npcRotation = 0f;
			}
			if (npc.ai[0] == 3f) //tornado
			{
				npcRotation = 0f;
			}
			if (npc.ai[0] == 4f) //enter new phase
			{
				npcRotation = 0f;
			}
			if (npc.ai[0] == 9f) //tornado
			{
				npcRotation = 0f;
			}
			if (npc.ai[0] == 10f) //enter new phase
			{
				npcRotation = 0f;
			}
			if (npc.ai[0] == 16f) //tornado
			{
				npcRotation = 0f;
			}
            if (npc.ai[0] == 20f) //tornado
            {
                npcRotation = 0f;
            }
            float npcRotationSpeed = 0.04f;
			if (npc.ai[0] == 1f || npc.ai[0] == 5f || npc.ai[0] == 7f || npc.ai[0] == 11f || npc.ai[0] == 14f || npc.ai[0] == 18f) //charge
			{
				npcRotationSpeed = 0f;
			}
			if (npc.ai[0] == 8f || npc.ai[0] == 12f || npc.ai[0] == 15f || npc.ai[0] == 19f) //circle
			{
				npcRotationSpeed = 0f;
			}
			if (npc.ai[0] == 3f) //tornado
			{
				npcRotationSpeed = 0.01f;
			}
			if (npc.ai[0] == 4f) //enter new phase
			{
				npcRotationSpeed = 0.01f;
			}
			if (npc.ai[0] == 9f || npc.ai[0] == 16f || npc.ai[0] == 20f) //tornado
			{
				npcRotationSpeed = 0.01f;
			}
			if (npc.rotation < npcRotation)
			{
				if ((double)(npcRotation - npc.rotation) > 3.1415926535897931) 
				{
					npc.rotation -= npcRotationSpeed;
				} 
				else
				{
					npc.rotation += npcRotationSpeed;
				}
			}
			if (npc.rotation > npcRotation) 
			{
				if ((double)(npc.rotation - npcRotation) > 3.1415926535897931) 
				{
					npc.rotation += npcRotationSpeed;
				} 
				else
				{
					npc.rotation -= npcRotationSpeed;
				}
			}
			if (npc.rotation > npcRotation - npcRotationSpeed && npc.rotation < npcRotation + npcRotationSpeed) 
			{
				npc.rotation = npcRotation;
			}
			if (npc.rotation < 0f) 
			{
				npc.rotation += 6.28318548f;
			}
			if (npc.rotation > 6.28318548f) 
			{
				npc.rotation -= 6.28318548f;
			}
			if (npc.rotation > npcRotation - npcRotationSpeed && npc.rotation < npcRotation + npcRotationSpeed) 
			{
				npc.rotation = npcRotation;
			}
            #endregion
            #region AlphaAndInitialSpawnEffects
            if (npc.ai[0] != -1f && npc.ai[0] < 9f)
            {
                bool colliding = Collision.SolidCollision(npc.position, npc.width, npc.height);
                if (colliding)
                {
                    npc.alpha += 15;
                }
                else
                {
                    npc.alpha -= 15;
                }
                if (npc.alpha < 0)
                {
                    npc.alpha = 0;
                }
                if (npc.alpha > 150)
                {
                    npc.alpha = 150;
                }
            }
			if (npc.ai[0] == -1f) //initial spawn effects
			{
				npc.dontTakeDamage = true;
				npc.chaseable = false;
				npc.velocity *= 0.98f;
				int num1467 = Math.Sign(player.Center.X - vectorCenter.X);
				if (num1467 != 0)
				{
					npc.direction = num1467;
					npc.spriteDirection = npc.direction;
				}
				if (npc.ai[2] > 20f) 
				{
					npc.velocity.Y = -2f;
					npc.alpha -= 5;
					bool colliding = Collision.SolidCollision(npc.position, npc.width, npc.height);
					if (colliding) 
					{
						npc.alpha += 15;
					}
					if (npc.alpha < 0) 
					{
						npc.alpha = 0;
					}
					if (npc.alpha > 150) 
					{
						npc.alpha = 150;
					}
				}
				if (npc.ai[2] == (float)(num1457 - 30)) 
				{
					int num1468 = 72;
					for (int num1469 = 0; num1469 < num1468; num1469++) 
					{
						Vector2 vector169 = Vector2.Normalize(npc.velocity) * new Vector2((float)npc.width / 2f, (float)npc.height) * 0.75f * 0.5f;
						vector169 = vector169.RotatedBy((double)((float)(num1469 - (num1468 / 2 - 1)) * 6.28318548f / (float)num1468), default(Vector2)) + npc.Center;
						Vector2 value16 = vector169 - npc.Center;
						int num1470 = Dust.NewDust(vector169 + value16, 0, 0, 244, value16.X * 2f, value16.Y * 2f, 100, default(Color), 1.4f);
						Main.dust[num1470].noGravity = true;
						Main.dust[num1470].noLight = true;
						Main.dust[num1470].velocity = Vector2.Normalize(value16) * 3f;
					}
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1464) 
				{
					npc.ai[0] = 0f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}
			}
            #endregion
            #region Phase1
            else if (npc.ai[0] == 0f && !player.dead) 
			{
				npc.dontTakeDamage = false;
				npc.chaseable = true;
				if (npc.ai[1] == 0f) 
				{
					npc.ai[1] = (float)(300 * Math.Sign((vectorCenter - player.Center).X));
				}
				Vector2 value17 = player.Center + new Vector2(npc.ai[1], -200f) - vectorCenter;
				Vector2 vector170 = Vector2.Normalize(value17 - npc.velocity) * scaleFactor;
				if (npc.velocity.X < vector170.X) 
				{
					npc.velocity.X = npc.velocity.X + npcVelocity;
					if (npc.velocity.X < 0f && vector170.X > 0f) 
					{
						npc.velocity.X = npc.velocity.X + npcVelocity;
					}
				} 
				else if (npc.velocity.X > vector170.X) 
				{
					npc.velocity.X = npc.velocity.X - npcVelocity;
					if (npc.velocity.X > 0f && vector170.X < 0f) 
					{
						npc.velocity.X = npc.velocity.X - npcVelocity;
					}
				}
				if (npc.velocity.Y < vector170.Y) 
				{
					npc.velocity.Y = npc.velocity.Y + npcVelocity;
					if (npc.velocity.Y < 0f && vector170.Y > 0f) 
					{
						npc.velocity.Y = npc.velocity.Y + npcVelocity;
					}
				} 
				else if (npc.velocity.Y > vector170.Y) 
				{
					npc.velocity.Y = npc.velocity.Y - npcVelocity;
					if (npc.velocity.Y > 0f && vector170.Y < 0f) 
					{
						npc.velocity.Y = npc.velocity.Y - npcVelocity;
					}
				}
				int num1471 = Math.Sign(player.Center.X - vectorCenter.X);
				if (num1471 != 0)
				{
					if (npc.ai[2] == 0f && num1471 != npc.direction)
					{
						npc.rotation = 3.14159274f;
					}
					npc.direction = num1471;
					if (num1471 != 0) 
					{
						npc.direction = num1471;
						npc.rotation = 0f;
						npc.spriteDirection = -npc.direction;
					}
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)aiChangeRate) 
				{
					int aiState = 0;
					switch ((int)npc.ai[3]) //0 2 4 6 1 3 5 7 repeat
					{
						case 0:
						case 1:
						case 2:
						case 3:
						case 4:
                            aiState = 1; //normal charges
                            break;
						case 5:
                            aiState = 5; //fast charge
							break;
						case 6:
							npc.ai[3] = 1f;
							aiState = 2; //fireball attack
							break;
						case 7:
							npc.ai[3] = 0f;
							aiState = 3; //tornadoes
							break;
					}
					if (phase2Check)
					{
						aiState = 4;
					}
					if (aiState == 1) 
					{
						npc.ai[0] = 1f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
						npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * chargeSpeed;
						npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
						if (num1471 != 0)
						{
							npc.direction = num1471;
							if (npc.spriteDirection == 1)
							{
								npc.rotation += 3.14159274f;
							}
							npc.spriteDirection = -npc.direction;
						}
					} 
					else if (aiState == 2) 
					{
						npc.ai[0] = 2f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					} 
					else if (aiState == 3) 
					{
						npc.ai[0] = 3f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					} 
					else if (aiState == 4) 
					{
						npc.ai[0] = 4f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
                    else if (aiState == 5)
                    {
                        npc.ai[0] = 5f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * chargeSpeed2;
                        npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
                        if (num1471 != 0)
                        {
                            npc.direction = num1471;
                            if (npc.spriteDirection == 1)
                            {
                                npc.rotation += 3.14159274f;
                            }
                            npc.spriteDirection = -npc.direction;
                        }
                    }
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 1f) //charge attack
			{
				int num1473 = 7;
				for (int num1474 = 0; num1474 < num1473; num1474++)
				{
					Vector2 vector171 = Vector2.Normalize(npc.velocity) * new Vector2((float)(npc.width + 50) / 2f, (float)npc.height) * 0.75f;
					vector171 = vector171.RotatedBy((double)(num1474 - (num1473 / 2 - 1)) * 3.1415926535897931 / (double)((float)num1473), default(Vector2)) + vectorCenter;
					Vector2 value18 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - 1.57079637f).ToRotationVector2() * (float)Main.rand.Next(3, 8);
					int num1475 = Dust.NewDust(vector171 + value18, 0, 0, 244, value18.X * 2f, value18.Y * 2f, 100, default(Color), 1.4f);
					Main.dust[num1475].noGravity = true;
					Main.dust[num1475].noLight = true;
					Main.dust[num1475].velocity /= 4f;
					Main.dust[num1475].velocity -= npc.velocity;
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)chargeTime) 
				{
					npc.ai[0] = 0f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] += 2f;
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 2f) //fireball attack
			{
				if (npc.ai[1] == 0f) 
				{
					npc.ai[1] = (float)(300 * Math.Sign((vectorCenter - player.Center).X));
				}
				Vector2 value19 = player.Center + new Vector2(npc.ai[1], -200f) - vectorCenter;
				Vector2 vector172 = Vector2.Normalize(value19 - npc.velocity) * scaleFactor11;
				if (npc.velocity.X < vector172.X) 
				{
					npc.velocity.X = npc.velocity.X + num1456;
					if (npc.velocity.X < 0f && vector172.X > 0f) 
					{
						npc.velocity.X = npc.velocity.X + num1456;
					}
				} 
				else if (npc.velocity.X > vector172.X) 
				{
					npc.velocity.X = npc.velocity.X - num1456;
					if (npc.velocity.X > 0f && vector172.X < 0f) 
					{
						npc.velocity.X = npc.velocity.X - num1456;
					}
				}
				if (npc.velocity.Y < vector172.Y) 
				{
					npc.velocity.Y = npc.velocity.Y + num1456;
					if (npc.velocity.Y < 0f && vector172.Y > 0f) 
					{
						npc.velocity.Y = npc.velocity.Y + num1456;
					}
				} 
				else if (npc.velocity.Y > vector172.Y) 
				{
					npc.velocity.Y = npc.velocity.Y - num1456;
					if (npc.velocity.Y > 0f && vector172.Y < 0f) 
					{
						npc.velocity.Y = npc.velocity.Y - num1456;
					}
				}
				if (npc.ai[2] == 0f) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
				if (npc.ai[2] % (float)num1455 == 0f) //fire flare bombs from mouth
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                    if (Main.netMode != 1) 
					{
						if (NPC.CountNPCS(mod.NPCType("DetonatingFlare")) < flareCount)
						{
							Vector2 vector6 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
							NPC.NewNPC((int)vector6.X, (int)vector6.Y - 100, mod.NPCType("DetonatingFlare"), 0, 0f, 0f, 0f, 0f, 255);
						}
						int damage = expertMode ? 75 : 90; //700
						Vector2 vector173 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
						Projectile.NewProjectile((int)vector173.X, (int)vector173.Y - 70, 0f, 0f, mod.ProjectileType("FlareBomb"), damage, 0f, Main.myPlayer, 0f, 0f);
					}
				}
				int num1476 = Math.Sign(player.Center.X - vectorCenter.X);
				Vector2 dir2 = npc.position - Main.player[npc.target].position;
				if (num1476 != 0)
				{
					npc.direction = num1476;
					if (npc.spriteDirection != -npc.direction) 
					{
						npc.rotation += 6.28318548f;
						if (npc.rotation > 6.28318548f)
						{
							npc.rotation = 0f;
							if(dir2.X < 0)
							{								
								npc.direction = -1;
							}
							else 
							{
								npc.direction = 1;
							}
						}
					}
					npc.spriteDirection = -npc.direction;
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1454) 
				{
					npc.ai[0] = 0f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 3f) //Fire small flares
			{
				npc.velocity *= 0.98f;
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
				if (npc.ai[2] == (float)(num1457 - 30)) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
				if (Main.netMode != 1 && npc.ai[2] == (float)(num1457 - 30)) 
				{
					Projectile.NewProjectile(vectorCenter.X, vectorCenter.Y, 0f, 0f, mod.ProjectileType("Flare"), 0, 0f, Main.myPlayer, 1f, (float)(npc.target + 1)); //changed
					Projectile.NewProjectile(vectorCenter.X, vectorCenter.Y, (float)(-(float)npc.direction * 2), 8f, mod.ProjectileType("Flare"), 0, 0f, Main.myPlayer, 0f, 0f); //changed
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1457) 
				{
					npc.ai[0] = 0f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 4f) //enter phase 2
			{
				npc.dontTakeDamage = true;
				npc.chaseable = false;
				npc.velocity *= 0.98f;
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
				if (npc.ai[2] == (float)(num1458 - 60)) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1458) 
				{
					npc.ai[0] = 6f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] = 0f;
					npc.netUpdate = true;
					return;
				}
			}
            else if (npc.ai[0] == 5f)
            {
                int num1473 = 14;
                for (int num1474 = 0; num1474 < num1473; num1474++)
                {
                    Vector2 vector171 = Vector2.Normalize(npc.velocity) * new Vector2((float)(npc.width + 50) / 2f, (float)npc.height) * 0.75f;
                    vector171 = vector171.RotatedBy((double)(num1474 - (num1473 / 2 - 1)) * 3.1415926535897931 / (double)((float)num1473), default(Vector2)) + vectorCenter;
                    Vector2 value18 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - 1.57079637f).ToRotationVector2() * (float)Main.rand.Next(3, 8);
                    int num1475 = Dust.NewDust(vector171 + value18, 0, 0, 244, value18.X * 2f, value18.Y * 2f, 100, default(Color), 1.4f); //changed
                    Main.dust[num1475].noGravity = true;
                    Main.dust[num1475].noLight = true;
                    Main.dust[num1475].velocity /= 4f;
                    Main.dust[num1475].velocity -= npc.velocity;
                }
                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)chargeTime2)
                {
                    npc.ai[0] = 0f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] += 2f;
                    npc.netUpdate = true;
                    return;
                }
            }
            #endregion
            #region Phase2
            else if (npc.ai[0] == 6f && !player.dead) //phase 2
			{
                npc.dontTakeDamage = false;
                npc.chaseable = true;
                if (npc.ai[1] == 0f) 
				{
					npc.ai[1] = (float)(300 * Math.Sign((vectorCenter - player.Center).X));
				}
				Vector2 value20 = player.Center + new Vector2(npc.ai[1], -200f) - vectorCenter;
				Vector2 vector175 = Vector2.Normalize(value20 - npc.velocity) * scaleFactor;
				if (npc.velocity.X < vector175.X) 
				{
					npc.velocity.X = npc.velocity.X + npcVelocity;
					if (npc.velocity.X < 0f && vector175.X > 0f) 
					{
						npc.velocity.X = npc.velocity.X + npcVelocity;
					}
				} 
				else if (npc.velocity.X > vector175.X) 
				{
					npc.velocity.X = npc.velocity.X - npcVelocity;
					if (npc.velocity.X > 0f && vector175.X < 0f) 
					{
						npc.velocity.X = npc.velocity.X - npcVelocity;
					}
				}
				if (npc.velocity.Y < vector175.Y) 
				{
					npc.velocity.Y = npc.velocity.Y + npcVelocity;
					if (npc.velocity.Y < 0f && vector175.Y > 0f) 
					{
						npc.velocity.Y = npc.velocity.Y + npcVelocity;
					}
				} 
				else if (npc.velocity.Y > vector175.Y) 
				{
					npc.velocity.Y = npc.velocity.Y - npcVelocity;
					if (npc.velocity.Y > 0f && vector175.Y < 0f) 
					{
						npc.velocity.Y = npc.velocity.Y - npcVelocity;
					}
				}
				int num1477 = Math.Sign(player.Center.X - vectorCenter.X);
				if (num1477 != 0)
				{
					if (npc.ai[2] == 0f && num1477 != npc.direction)
					{
						npc.rotation = 3.14159274f;
					}
					npc.direction = num1477;
					if (num1477 != 0) 
					{
						npc.direction = num1477;
						npc.rotation = 0f;
						npc.spriteDirection = -npc.direction;
					}
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)aiChangeRate) 
				{
					int aiState = 0;
					switch ((int)npc.ai[3]) //0 2 4 6 8 1 3 5 7 9 repeat
					{
						case 0:
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
                            aiState = 1;
                            break;
                        case 6:
                            aiState = 5;
                            break;
                        case 7:
							aiState = 6;
							break;
						case 8:
							npc.ai[3] = 1f;
							aiState = 2;
							break;
						case 9:
							npc.ai[3] = 0f;
							aiState = 3;
							break;
					}
					if (phase3Check)
					{
						aiState = 4;
					}
					if (aiState == 1) 
					{
						npc.ai[0] = 7f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
						npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * chargeSpeed;
						npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
						if (num1477 != 0) 
						{
							npc.direction = num1477;
							if (npc.spriteDirection == 1) 
							{
								npc.rotation += 3.14159274f;
							}
							npc.spriteDirection = -npc.direction;
						}
					} 
					else if (aiState == 2) 
					{
						npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * scaleFactor13;
						npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
						if (num1477 != 0) 
						{
							npc.direction = num1477;
							if (npc.spriteDirection == 1) 
							{
								npc.rotation += 3.14159274f;
							}
							npc.spriteDirection = -npc.direction;
						}
						npc.ai[0] = 8f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					} 
					else if (aiState == 3) 
					{
						npc.ai[0] = 9f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					} 
					else if (aiState == 4) 
					{
						npc.ai[0] = 10f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
                    else if (aiState == 5)
                    {
                        npc.ai[0] = 11f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * chargeSpeed2;
                        npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
                        if (num1477 != 0)
                        {
                            npc.direction = num1477;
                            if (npc.spriteDirection == 1)
                            {
                                npc.rotation += 3.14159274f;
                            }
                            npc.spriteDirection = -npc.direction;
                        }
                    }
                    else if (aiState == 6)
                    {
                        npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * scaleFactor13;
                        npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
                        if (num1477 != 0)
                        {
                            npc.direction = num1477;
                            if (npc.spriteDirection == 1)
                            {
                                npc.rotation += 3.14159274f;
                            }
                            npc.spriteDirection = -npc.direction;
                        }
                        npc.ai[0] = 12f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                    }
                    npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 7f) //charge
			{
				int num1479 = 7;
				for (int num1480 = 0; num1480 < num1479; num1480++) 
				{
					Vector2 vector176 = Vector2.Normalize(npc.velocity) * new Vector2((float)(npc.width + 50) / 2f, (float)npc.height) * 0.75f;
					vector176 = vector176.RotatedBy((double)(num1480 - (num1479 / 2 - 1)) * 3.1415926535897931 / (double)((float)num1479), default(Vector2)) + vectorCenter;
					Vector2 value21 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - 1.57079637f).ToRotationVector2() * (float)Main.rand.Next(3, 8);
					int num1481 = Dust.NewDust(vector176 + value21, 0, 0, 244, value21.X * 2f, value21.Y * 2f, 100, default(Color), 1.4f); //changed
					Main.dust[num1481].noGravity = true;
					Main.dust[num1481].noLight = true;
					Main.dust[num1481].velocity /= 4f;
					Main.dust[num1481].velocity -= npc.velocity;
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)chargeTime) 
				{
					npc.ai[0] = 6f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] += 2f;
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 8f)
			{
				if (npc.ai[2] == 0f) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
				if (npc.ai[2] % (float)num1462 == 0f) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                    if (Main.netMode != 1) 
					{
						if (NPC.CountNPCS(mod.NPCType("DetonatingFlare2")) < flareCount)
						{
							Vector2 vector6 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
							NPC.NewNPC((int)vector6.X, (int)vector6.Y - 100, mod.NPCType("DetonatingFlare2"), 0, 0f, 0f, 0f, 0f, 255);
						}
						Vector2 vector173 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
						Projectile.NewProjectile((int)vector173.X, (int)vector173.Y - 100, (float)Main.rand.Next(-400, 401) * 0.13f, (float)Main.rand.Next(-30, 31) * 0.13f, mod.ProjectileType("FlareDust"), 0, 0f, Main.myPlayer, 0f, 0f); //changed
					}
				}
				npc.velocity = npc.velocity.RotatedBy((double)(-(double)num1463 * (float)npc.direction), default(Vector2));
				npc.rotation -= num1463 * (float)npc.direction;
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1461) 
				{
					npc.ai[0] = 6f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 9f)
			{
				npc.velocity *= 0.98f;
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
				if (npc.ai[2] == (float)(num1457 - 30)) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
				if (Main.netMode != 1 && npc.ai[2] == (float)(num1457 - 30)) 
				{
					Projectile.NewProjectile(vectorCenter.X, vectorCenter.Y, 0f, 0f, mod.ProjectileType("BigFlare"), 0, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
                }
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1457) 
				{
					npc.ai[0] = 6f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 10f) //start phase 3
			{
				npc.dontTakeDamage = true;
				npc.chaseable = false;
				npc.velocity *= 0.98f;
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
				if (npc.ai[2] == (float)(num1459 - 60)) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1459) 
				{
					npc.ai[0] = 13f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] = 0f;
					npc.netUpdate = true;
					return;
				}
			}
            else if (npc.ai[0] == 11f)
            {
                int num1479 = 14;
                for (int num1480 = 0; num1480 < num1479; num1480++)
                {
                    Vector2 vector176 = Vector2.Normalize(npc.velocity) * new Vector2((float)(npc.width + 50) / 2f, (float)npc.height) * 0.75f;
                    vector176 = vector176.RotatedBy((double)(num1480 - (num1479 / 2 - 1)) * 3.1415926535897931 / (double)((float)num1479), default(Vector2)) + vectorCenter;
                    Vector2 value21 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - 1.57079637f).ToRotationVector2() * (float)Main.rand.Next(3, 8);
                    int num1481 = Dust.NewDust(vector176 + value21, 0, 0, 244, value21.X * 2f, value21.Y * 2f, 100, default(Color), 1.4f); //changed
                    Main.dust[num1481].noGravity = true;
                    Main.dust[num1481].noLight = true;
                    Main.dust[num1481].velocity /= 4f;
                    Main.dust[num1481].velocity -= npc.velocity;
                }
                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)chargeTime2)
                {
                    npc.ai[0] = 6f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] += 2f;
                    npc.netUpdate = true;
                    return;
                }
            }
            else if (npc.ai[0] == 12f)
            {
                if (npc.ai[2] == 0f)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
                if (npc.ai[2] % (float)num1462 == 0f)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                    if (Main.netMode != 1)
                    {
                        int damage = expertMode ? 75 : 90; //700
                        Vector2 vector173 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
                        float speed = 0.01f;
                        Vector2 vectorShoot = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f + 30f);
                        float playerX = player.position.X + (float)player.width * 0.5f - vectorShoot.X;
                        float playerY = player.position.Y - vectorShoot.Y;
                        float playerXY = (float)Math.Sqrt((double)(playerX * playerX + playerY * playerY));
                        playerXY = speed / playerXY;
                        playerX *= playerXY;
                        playerY *= playerXY;
                        Projectile.NewProjectile((int)vector173.X, (int)vector173.Y - 100, playerX, playerY, mod.ProjectileType("FlareDust2"), damage, 0f, Main.myPlayer, 0f, 0f);
                    }
                }
                npc.velocity = npc.velocity.RotatedBy((double)(-(double)num1463 * (float)npc.direction), default(Vector2));
                npc.rotation -= num1463 * (float)npc.direction;
                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)num1461)
                {
                    npc.ai[0] = 6f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] += 2f;
                    npc.netUpdate = true;
                    return;
                }
            }
            #endregion
            #region Phase3
            else if (npc.ai[0] == 13f && !player.dead)
			{
				npc.dontTakeDamage = phase5Check;
				npc.chaseable = true;
				if (npc.ai[1] == 0f) 
				{
					npc.ai[1] = (float)(300 * Math.Sign((vectorCenter - player.Center).X));
				}
				Vector2 value20 = player.Center + new Vector2(npc.ai[1], -200f) - vectorCenter;
				Vector2 vector175 = Vector2.Normalize(value20 - npc.velocity) * scaleFactor;
				if (npc.velocity.X < vector175.X) 
				{
					npc.velocity.X = npc.velocity.X + npcVelocity;
					if (npc.velocity.X < 0f && vector175.X > 0f) 
					{
						npc.velocity.X = npc.velocity.X + npcVelocity;
					}
				} 
				else if (npc.velocity.X > vector175.X) 
				{
					npc.velocity.X = npc.velocity.X - npcVelocity;
					if (npc.velocity.X > 0f && vector175.X < 0f) 
					{
						npc.velocity.X = npc.velocity.X - npcVelocity;
					}
				}
				if (npc.velocity.Y < vector175.Y) 
				{
					npc.velocity.Y = npc.velocity.Y + npcVelocity;
					if (npc.velocity.Y < 0f && vector175.Y > 0f) 
					{
						npc.velocity.Y = npc.velocity.Y + npcVelocity;
					}
				} 
				else if (npc.velocity.Y > vector175.Y) 
				{
					npc.velocity.Y = npc.velocity.Y - npcVelocity;
					if (npc.velocity.Y > 0f && vector175.Y < 0f) 
					{
						npc.velocity.Y = npc.velocity.Y - npcVelocity;
					}
				}
				int num1477 = Math.Sign(player.Center.X - vectorCenter.X);
				if (num1477 != 0) 
				{
					if (npc.ai[2] == 0f && num1477 != npc.direction)
					{
						npc.rotation = 3.14159274f;
					}
					npc.direction = num1477;
					if (num1477 != 0) 
					{
						npc.direction = num1477;
						npc.rotation = 0f;
						npc.spriteDirection = -npc.direction;
					}
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)aiChangeRate)
				{
					int aiState = 0;
					switch ((int)npc.ai[3]) //0 2 4 6 8 9 1 3 5 7 10 repeat
					{
						case 0:
						case 1:
						case 2:
						case 3:
                            aiState = ((CalamityWorld.death && !CalamityWorld.bossRushActive) ? 5 : 1); //normal charges
                            break;
                        case 4:
                        case 5:
						case 6:
                            aiState = 5; //fast charges
                            break;
                        case 7:
                            aiState = 3; //big tornado
                            break;
                        case 8:
                            aiState = 6; //slow flare bombs
                            break;
                        case 9:
                            npc.ai[3] = 1f;
                            aiState = 7; //small tornado
							break;
						case 10:
							npc.ai[3] = 0f;
							aiState = 2; //flare circle
							break;
					}
					if (phase4Check)
					{
						aiState = 4;
					}
					if (aiState == 1) 
					{
						npc.ai[0] = 14f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
						npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * chargeSpeed;
						npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
						if (num1477 != 0) 
						{
							npc.direction = num1477;
							if (npc.spriteDirection == 1) 
							{
								npc.rotation += 3.14159274f;
							}
							npc.spriteDirection = -npc.direction;
						}
					}
					else if (aiState == 2) 
					{
						npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * scaleFactor13;
						npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
						if (num1477 != 0) 
						{
							npc.direction = num1477;
							if (npc.spriteDirection == 1) 
							{
								npc.rotation += 3.14159274f;
							}
							npc.spriteDirection = -npc.direction;
						}
						npc.ai[0] = 15f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
					else if (aiState == 3) 
					{
						npc.ai[0] = 16f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
					else if (aiState == 4) 
					{
						npc.ai[0] = 17f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
                    else if (aiState == 5)
                    {
                        npc.ai[0] = 18f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * chargeSpeed2;
                        npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
                        if (num1477 != 0)
                        {
                            npc.direction = num1477;
                            if (npc.spriteDirection == 1)
                            {
                                npc.rotation += 3.14159274f;
                            }
                            npc.spriteDirection = -npc.direction;
                        }
                    }
                    else if (aiState == 6)
                    {
                        npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * scaleFactor13;
                        npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
                        if (num1477 != 0)
                        {
                            npc.direction = num1477;
                            if (npc.spriteDirection == 1)
                            {
                                npc.rotation += 3.14159274f;
                            }
                            npc.spriteDirection = -npc.direction;
                        }
                        npc.ai[0] = 19f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                    }
                    else if (aiState == 7)
                    {
                        npc.ai[0] = 20f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                    }
                    npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 14f) //charge
			{
                npc.dontTakeDamage = phase5Check;
                npc.chaseable = true;
                int num1479 = 7;
				for (int num1480 = 0; num1480 < num1479; num1480++) 
				{
					Vector2 vector176 = Vector2.Normalize(npc.velocity) * new Vector2((float)(npc.width + 50) / 2f, (float)npc.height) * 0.75f;
					vector176 = vector176.RotatedBy((double)(num1480 - (num1479 / 2 - 1)) * 3.1415926535897931 / (double)((float)num1479), default(Vector2)) + vectorCenter;
					Vector2 value21 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - 1.57079637f).ToRotationVector2() * (float)Main.rand.Next(3, 8);
					int num1481 = Dust.NewDust(vector176 + value21, 0, 0, 244, value21.X * 2f, value21.Y * 2f, 100, default(Color), 1.4f);
					Main.dust[num1481].noGravity = true;
					Main.dust[num1481].noLight = true;
					Main.dust[num1481].velocity /= 4f;
					Main.dust[num1481].velocity -= npc.velocity;
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)chargeTime) 
				{
					npc.ai[0] = 13f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] += 2f;
					npc.netUpdate = true;
					return;
				}
			} 
			else if (npc.ai[0] == 15f)
			{
                npc.dontTakeDamage = phase5Check;
                npc.chaseable = true;
                if (npc.ai[2] == 0f) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
				if (npc.ai[2] % (float)num1462 == 0f) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                    if (Main.netMode != 1) 
					{
						if (NPC.CountNPCS(mod.NPCType("DetonatingFlare2")) < flareCount && NPC.CountNPCS(mod.NPCType("DetonatingFlare")) < flareCount)
						{
							Vector2 vector6 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
							NPC.NewNPC((int)vector6.X, (int)vector6.Y - 100, (Main.rand.Next(2) == 0 ? mod.NPCType("DetonatingFlare") : mod.NPCType("DetonatingFlare2")), 0, 0f, 0f, 0f, 0f, 255);
						}
						Vector2 vector = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
						Projectile.NewProjectile((int)vector.X, (int)vector.Y - 100, (float)Main.rand.Next(-401, 401) * 0.13f, (float)Main.rand.Next(-31, 31) * 0.13f, mod.ProjectileType("FlareDust"), 0, 0f, Main.myPlayer, 0f, 0f); //changed
				        Projectile.NewProjectile((int)vector.X, (int)vector.Y - 100, (float)Main.rand.Next(-31, 31) * 0.13f, (float)Main.rand.Next(-151, 151) * 0.13f, mod.ProjectileType("FlareDust"), 0, 0f, Main.myPlayer, 0f, 0f); //changed
					}
				}
				npc.velocity = npc.velocity.RotatedBy((double)(-(double)num1463 * (float)npc.direction), default(Vector2));
				npc.rotation -= num1463 * (float)npc.direction;
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1461) 
				{
					npc.ai[0] = 13f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}
			}
			else if (npc.ai[0] == 16f)
			{
                npc.dontTakeDamage = phase5Check;
                npc.chaseable = true;
                npc.velocity *= 0.98f;
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
				if (npc.ai[2] == (float)(num1457 - 30)) 
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
				if (Main.netMode != 1 && npc.ai[2] == (float)(num1457 - 30)) 
				{
					Projectile.NewProjectile(vectorCenter.X, vectorCenter.Y, 0f, 0f, mod.ProjectileType("BigFlare"), 0, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
                }
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1457) 
				{
					npc.ai[0] = 13f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
                    npc.ai[3] += 3f;
					npc.netUpdate = true;
					return;
				}
			}
			else if (npc.ai[0] == 17f)
			{
				npc.dontTakeDamage = true;
				npc.chaseable = false;
				if (npc.ai[2] < (float)(num1459 - 90))
				{
					bool colliding = Collision.SolidCollision(npc.position, npc.width, npc.height);
					if (colliding)
					{
						npc.alpha += 15;
					}
					else
					{
						npc.alpha -= 15;
					}
					if (npc.alpha < 0)
					{
						npc.alpha = 0;
					}
					if (npc.alpha > 150)
					{
						npc.alpha = 150;
					}
				}
				else if (npc.alpha < 255)
				{
					npc.alpha += 4;
					if (npc.alpha > 255)
					{
						npc.alpha = 255;
					}
				}
				npc.velocity *= 0.98f;
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
				if (npc.ai[2] == (float)(num1459 - 60))
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1459)
				{
					npc.ai[0] = 21f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] = 0f;
					npc.netUpdate = true;
					return;
				}
			}
            else if (npc.ai[0] == 18f)
            {
                npc.dontTakeDamage = phase5Check;
                npc.chaseable = true;
                int num1479 = 14;
                for (int num1480 = 0; num1480 < num1479; num1480++)
                {
                    Vector2 vector176 = Vector2.Normalize(npc.velocity) * new Vector2((float)(npc.width + 50) / 2f, (float)npc.height) * 0.75f;
                    vector176 = vector176.RotatedBy((double)(num1480 - (num1479 / 2 - 1)) * 3.1415926535897931 / (double)((float)num1479), default(Vector2)) + vectorCenter;
                    Vector2 value21 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - 1.57079637f).ToRotationVector2() * (float)Main.rand.Next(3, 8);
                    int num1481 = Dust.NewDust(vector176 + value21, 0, 0, 244, value21.X * 2f, value21.Y * 2f, 100, default(Color), 1.4f);
                    Main.dust[num1481].noGravity = true;
                    Main.dust[num1481].noLight = true;
                    Main.dust[num1481].velocity /= 4f;
                    Main.dust[num1481].velocity -= npc.velocity;
                }
                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)chargeTime2)
                {
                    npc.ai[0] = 13f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] += 2f;
                    npc.netUpdate = true;
                    return;
                }
            }
            else if (npc.ai[0] == 19f)
            {
                npc.dontTakeDamage = phase5Check;
                npc.chaseable = true;
                if (npc.ai[2] == 0f)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
                if (npc.ai[2] % (float)num1462 == 0f)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                    if (Main.netMode != 1)
                    {
                        int damage = expertMode ? 75 : 90; //700
                        Vector2 vector173 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
                        float speed = 0.01f;
                        Vector2 vectorShoot = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f + 30f);
                        float playerX = player.position.X + (float)player.width * 0.5f - vectorShoot.X;
                        float playerY = player.position.Y - vectorShoot.Y;
                        float playerXY = (float)Math.Sqrt((double)(playerX * playerX + playerY * playerY));
                        playerXY = speed / playerXY;
                        playerX *= playerXY;
                        playerY *= playerXY;
                        Projectile.NewProjectile((int)vector173.X, (int)vector173.Y - 100, playerX, playerY, mod.ProjectileType("FlareDust2"), damage, 0f, Main.myPlayer, 0f, 0f);
                    }
                }
                npc.velocity = npc.velocity.RotatedBy((double)(-(double)num1463 * (float)npc.direction), default(Vector2));
                npc.rotation -= num1463 * (float)npc.direction;
                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)num1461)
                {
                    npc.ai[0] = 13f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] += 1f;
                    npc.netUpdate = true;
                    return;
                }
            }
            else if (npc.ai[0] == 20f)
            {
                npc.dontTakeDamage = phase5Check;
                npc.chaseable = true;
                npc.velocity *= 0.98f;
                npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
                if (npc.ai[2] == (float)(num1457 - 30))
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
                if (CalamityWorld.death && !CalamityWorld.bossRushActive)
                {
                    if (Main.netMode != 1 && npc.ai[2] == (float)(num1457 - 30))
                    {
                        Projectile.NewProjectile(vectorCenter.X, vectorCenter.Y, 0f, 0f, mod.ProjectileType("BigFlare"), 0, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
                    }
                }
                else
                {
                    if (Main.netMode != 1 && npc.ai[2] == (float)(num1457 - 30))
                    {
                        Projectile.NewProjectile(vectorCenter.X, vectorCenter.Y, 0f, 0f, mod.ProjectileType("Flare"), 0, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
                    }
                }
                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)num1457)
                {
                    npc.ai[0] = 13f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                    return;
                }
            }
            #endregion
            #region Phase4
            else if (npc.ai[0] == 21f && !player.dead)
			{
				npc.dontTakeDamage = phase5Check;
				npc.chaseable = false;
				if (npc.alpha < 255)
				{
					npc.alpha += 25;
					if (npc.alpha > 255)
					{
						npc.alpha = 255;
					}
				}
				if (npc.ai[1] == 0f)
				{
					npc.ai[1] = (float)(360 * Math.Sign((vectorCenter - player.Center).X));
				}
				Vector2 value7 = player.Center + new Vector2(npc.ai[1], teleportLocation) - vectorCenter; //teleport distance
				Vector2 desiredVelocity = Vector2.Normalize(value7 - npc.velocity) * scaleFactor;
				npc.SimpleFlyMovement(desiredVelocity, npcVelocity);
				int num32 = Math.Sign(player.Center.X - vectorCenter.X);
				if (num32 != 0)
				{
					if (npc.ai[2] == 0f && num32 != npc.direction)
					{
						npc.rotation = 3.14159274f;
					}
					npc.direction = num32;
					if (num32 != 0) 
					{
						npc.direction = num32;
						npc.rotation = 0f;
						npc.spriteDirection = -npc.direction;
					}
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)aiChangeRate)
				{
					int aiState = 0;
					switch ((int)npc.ai[3])
					{
						case 0: //skip 1
						case 2:
						case 3: //skip 4
						case 5:
						case 6:
						case 7: //skip 8
						case 9:
						case 10:
						case 11:
						case 12: //skip 13
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18: //skip 19
							aiState = 1;
							break;
						case 1: //+3
						case 4: //+4
						case 8: //+5
						case 13: //+6
                        case 19:
							aiState = 2;
							break;
					}
                    if (phase5Check)
                    {
                        aiState = 4;
                    }
                    if (aiState == 1)
					{
						npc.ai[0] = 22f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
						npc.velocity = Vector2.Normalize(player.Center - vectorCenter) * chargeSpeed;
						npc.rotation = (float)Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X);
						if (num32 != 0) 
						{
							npc.direction = num32;
							if (npc.spriteDirection == 1) 
							{
								npc.rotation += 3.14159274f;
							}
							npc.spriteDirection = -npc.direction;
						}
					}
					else if (aiState == 2)
					{
						npc.ai[0] = 23f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
					else if (aiState == 3)
					{
						npc.ai[0] = 24f;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
                    else if (aiState == 4)
                    {
                        npc.ai[0] = 25f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                    }
                    npc.netUpdate = true;
					return;
				}
			}
			else if (npc.ai[0] == 22f)
			{
				npc.dontTakeDamage = phase5Check;
				npc.chaseable = true;
				npc.alpha -= 25;
				if (npc.alpha < 0)
				{
					npc.alpha = 0;
				}
				int num34 = 7;
				for (int m = 0; m < num34; m++)
				{
					Vector2 vector11 = Vector2.Normalize(npc.velocity) * new Vector2((float)(npc.width + 50) / 2f, (float)npc.height) * 0.75f;
					vector11 = vector11.RotatedBy((double)(m - (num34 / 2 - 1)) * 3.1415926535897931 / (double)((float)num34), default(Vector2)) + vectorCenter;
					Vector2 value8 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - 1.57079637f).ToRotationVector2() * (float)Main.rand.Next(3, 8);
					int num35 = Dust.NewDust(vector11 + value8, 0, 0, 244, value8.X * 2f, value8.Y * 2f, 100, default(Color), 1.4f);
					Main.dust[num35].noGravity = true;
					Main.dust[num35].noLight = true;
					Main.dust[num35].velocity /= 4f;
					Main.dust[num35].velocity -= npc.velocity;
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)chargeTime)
				{
					npc.ai[0] = 21f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] += 1f;
					npc.netUpdate = true;
					return;
				}
			}
			else if (npc.ai[0] == 23f)
			{
				npc.dontTakeDamage = true;
				npc.chaseable = false;
				if (npc.alpha < 255)
				{
					npc.alpha += 17;
					if (npc.alpha > 255)
					{
						npc.alpha = 255;
					}
				}
				npc.velocity *= 0.98f;
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
				if (npc.ai[2] == (float)(num1460 / 2))
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
				if (Main.netMode != 1 && npc.ai[2] == (float)(num1460 / 2))
				{
					if (npc.ai[1] == 0f)
					{
						npc.ai[1] = (float)(300 * Math.Sign((vectorCenter - player.Center).X));
					}
					Vector2 center = player.Center + new Vector2(-npc.ai[1], teleportLocation); //teleport distance
					vectorCenter = (npc.Center = center);
					int num36 = Math.Sign(player.Center.X - vectorCenter.X);
					npc.rotation -= num1463 * (float)npc.direction;
				}
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1460)
				{
					npc.ai[0] = 21f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] += 1f;
                    if (npc.ai[3] == 5f && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(vectorCenter.X, vectorCenter.Y, 0f, 0f, mod.ProjectileType("BigFlare"), 0, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
                    }
                    if (npc.ai[3] >= 20f) //14
					{
						npc.ai[3] = 0f;
                    }
					npc.netUpdate = true;
					return;
				}
			}
			else if (npc.ai[0] == 24f)
			{
				npc.dontTakeDamage = phase5Check;
				npc.chaseable = true;
				if (npc.ai[2] == 0f)
				{
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
				npc.velocity = npc.velocity.RotatedBy((double)(-(double)num1463 * (float)npc.direction), default(Vector2));
				npc.rotation -= num1463 * (float)npc.direction;
				npc.ai[2] += 1f;
				if (npc.ai[2] >= (float)num1461)
				{
					npc.ai[0] = 21f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] += 1f;
					npc.netUpdate = true;
				}
			}
            else if (npc.ai[0] == 25f) //start phase 5
            {
                npc.alpha = 0;
                npc.dontTakeDamage = true;
                npc.chaseable = false;
                npc.velocity *= 0.98f;
                npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 0f, 0.02f);
                if (npc.ai[2] == (float)(num1459 - 60))
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                }
                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)num1459)
                {
                    npc.localAI[2] = 1f;
                    npc.ai[0] = 0f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;
                    return;
                }
            }
            #endregion
        }

        #region AI2
        public void Yharon_AI2()
        {
            bool revenge = (CalamityWorld.revenge || CalamityWorld.bossRushActive);
            bool expertMode = Main.expertMode;
            bool phase2 = (double)npc.life <= (double)npc.lifeMax * 0.66;
            bool phase3 = (double)npc.life <= (double)npc.lifeMax * 0.33;
            bool tantrum = (double)npc.life <= (double)npc.lifeMax * 0.04;
            npc.defense = 100;
            npc.alpha -= 25;
            if (npc.alpha < 0)
            {
                npc.alpha = 0;
            }
            if (!moveCloser)
            {
                music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/DragonGod");
                moveCloser = true;
                if ((CalamityWorld.death || CalamityWorld.bossRushActive) && Main.netMode != 1)
                {
                    NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, mod.NPCType("DetonatingFlare3"), 0, 0f, 0f, 0f, 0f, 255);
                }
                string key = "Mods.CalamityMod.FlameText";
                Color messageColor = Color.Orange;
                if (Main.netMode == 0)
                {
                    Main.NewText(Language.GetTextValue(key), messageColor);
                }
                else if (Main.netMode == 2)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
                }
            }
            if (npc.localAI[3] < 900f)
            {
                npc.localAI[3] += 1f;
                int heal = 5; //180 heals 50% and 30%
                healCounter += 1;
                if (healCounter >= heal)
                {
                    healCounter = 0;
                    if (Main.netMode != 1)
                    {
                        int fractionHealed = revenge ? 240 : 300; //100000 / 240 * 180 = 75000 healed = 75% heal
                        if (CalamityWorld.death || CalamityWorld.bossRushActive)
                        {
                            fractionHealed = 200; //100000 / 200 * 180 = 90000 healed = 90% heal
                        }
                        int healAmt = npc.lifeMax / fractionHealed;
                        if (healAmt > npc.lifeMax - npc.life)
                        {
                            healAmt = npc.lifeMax - npc.life;
                        }
                        if (healAmt > 0)
                        {
                            npc.life += healAmt;
                            npc.HealEffect(healAmt, true);
                            npc.netUpdate = true;
                        }
                    }
                }
            }
            else
            {
                npc.dontTakeDamage = false;
                npc.chaseable = true;
            }
            NPCUtils.TargetClosestBetsy(npc, false, null);
            NPCAimedTarget targetData = npc.GetTargetData(true);
            if (!targetData.Hitbox.Intersects(safeBox))
            {
                protectionBoost = true;
                npc.damage = npc.defDamage * 5;
                if (npc.timeLeft > 150)
                {
                    npc.timeLeft = 150;
                }
            }
            else
            {
                protectionBoost = false;
                if (npc.timeLeft < 3600)
                {
                    npc.timeLeft = 3600;
                }
            }
            int num = -1;
            float num2 = 1f;
            int num4 = expertMode ? 106 : 125; //600
            float num5 = 10f;
            float num6 = revenge ? 0.6f : 0.55f; //0.45
            float scaleFactor = revenge ? 10f : 9f; //7.5
            float num7 = 30f;
            float num8 = 30f;
            float scaleFactor2 = revenge ? 26f : 24.5f; //23
            float num9 = 600f;
            float num10 = 12f;
            float num11 = 40f;
            float num12 = (CalamityWorld.death && !CalamityWorld.bossRushActive) ? 40f : 80f;
            float num13 = num11 + num12;
            float num14 = 1500f;
            float num15 = 60f;
            float scaleFactor3 = 13f;
            float amount = 0.0333333351f;
            float scaleFactor4 = revenge ? 16f : 14.5f; //12
            int num16 = 10;
            int num17 = 6 * num16;
            float num18 = 60f;
            float num19 = num15 + (float)num17 + num18;
            float num20 = 60f;
            float num21 = 1f;
            float num22 = 6.28318548f * (num21 / num20);
            float num23 = 0.1f;
            float scaleFactor5 = revenge ? 38f : 36.5f; //32
            if (npc.GetGlobalNPC<CalamityGlobalNPC>(mod).enraged)
            {
                num6 = 0.65f;
                scaleFactor = 11f;
                scaleFactor2 = 28f;
                scaleFactor4 = 18f;
                scaleFactor5 = 40f;
            }
            float num24 = 90f;
            float num25 = 20f;
            float arg_F9_0 = npc.ai[0];
            float num26;
            if (npc.ai[0] == 0f)
            {
                float[] expr_115_cp_0 = npc.ai;
                int expr_115_cp_1 = 1;
                num26 = expr_115_cp_0[expr_115_cp_1] + 1f;
                expr_115_cp_0[expr_115_cp_1] = num26;
                if (num26 >= num5)
                {
                    npc.ai[1] = 0f;
                    npc.ai[0] = 1f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[0] == 1f)
            {
                if (npc.ai[2] == 0f)
                {
                    npc.ai[2] = (float)((npc.Center.X < targetData.Center.X) ? 1 : -1);
                }
                Vector2 destination = targetData.Center + new Vector2(-npc.ai[2] * 300f, -200f);
                Vector2 desiredVelocity = npc.DirectionTo(destination) * scaleFactor;
                npc.SimpleFlyMovement(desiredVelocity, num6);
                int num27 = (npc.Center.X < targetData.Center.X) ? 1 : -1;
                npc.direction = (npc.spriteDirection = num27);
                float[] expr_225_cp_0 = npc.ai;
                int expr_225_cp_1 = 1;
                num26 = expr_225_cp_0[expr_225_cp_1] + 1f;
                expr_225_cp_0[expr_225_cp_1] = num26;
                if (num26 >= num7)
                {
                    int num28 = 1;
                    if (phase3)
                    {
                        switch ((int)npc.ai[3])
                        {
                            case 0:
                                num28 = 6; //tornado
                                break;
                            case 1:
                                num28 = 2; //charge
                                break;
                            case 2:
                                num28 = 5; //fire circle
                                break;
                            case 3:
                                num28 = 4; //splitting fireballs
                                break;
                            case 4:
                                num28 = 3; //homing fireballs
                                break;
                            case 5:
                                num28 = 6; //tornado
                                break;
                            case 6:
                            case 7:
                                num28 = 2; //double charge
                                break;
                            case 8:
                                num28 = 5; //fire circle
                                break;
                            case 9:
                                num28 = 4; //splitting fireballs
                                break;
                        }
                    }
                    else if (phase2)
                    {
                        switch ((int)npc.ai[3])
                        {
                            case 0:
                                num28 = 6; //tornado
                                break;
                            case 1:
                            case 2:
                                num28 = 2; //charge
                                break;
                            case 3:
                                num28 = 5; //fire circle
                                break;
                            case 4:
                                num28 = 3; //homing fireballs
                                break;
                            case 5:
                            case 6:
                                num28 = 4; //double splitting fireballs
                                break;
                            case 7:
                            case 8:
                                num28 = 2; //double charge
                                break;
                            case 9:
                                num28 = 5; //fire circle
                                break;
                        }
                    }
                    else
                    {
                        switch ((int)npc.ai[3])
                        {
                            case 0:
                                num28 = 6; //tornado
                                break;
                            case 1:
                            case 2:
                                num28 = 2; //double charge
                                break;
                            case 3:
                                num28 = 3; //homing fireballs
                                break;
                            case 4:
                                num28 = 4; //splitting fireballs
                                break;
                            case 5:
                            case 6:
                                num28 = 2; //double charge
                                break;
                            case 7:
                                num28 = 5; //fire circle
                                break;
                        }
                    }
                    npc.ai[0] = (float)num28;
                    if (tantrum)
                    {
                        npc.ai[0] = 2f;
                    }
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] += 1f;
                    npc.netUpdate = true;
                    float aiLimit = 8f;
                    if (phase2)
                    {
                        aiLimit = 10f;
                    }
                    if (npc.ai[3] >= aiLimit)
                    {
                        npc.ai[3] = 0f;
                    }
                    switch (num28)
                    {
                        case 2: //charge
                            {
                                Vector2 vector = npc.DirectionTo(targetData.Center);
                                npc.spriteDirection = ((vector.X > 0f) ? 1 : -1);
                                npc.rotation = vector.ToRotation();
                                if (npc.spriteDirection == -1)
                                {
                                    npc.rotation += 3.14159274f;
                                }
                                npc.velocity = vector * scaleFactor2;
                                break;
                            }
                        case 3: //homing fireballs
                            {
                                Vector2 vector2 = new Vector2((float)((targetData.Center.X > npc.Center.X) ? 1 : -1), 0f);
                                npc.spriteDirection = ((vector2.X > 0f) ? 1 : -1);
                                npc.velocity = vector2 * -2f;
                                break;
                            }
                        case 5: //spin move
                            {
                                Vector2 vector3 = npc.DirectionTo(targetData.Center);
                                npc.spriteDirection = ((vector3.X > 0f) ? 1 : -1);
                                npc.rotation = vector3.ToRotation();
                                if (npc.spriteDirection == -1)
                                {
                                    npc.rotation += 3.14159274f;
                                }
                                npc.velocity = vector3 * scaleFactor5;
                                break;
                            }
                    }
                }
            }
            else if (npc.ai[0] == 2f)
            {
                if (npc.ai[1] == 0f)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
                float[] expr_498_cp_0 = npc.ai;
                int expr_498_cp_1 = 1;
                num26 = expr_498_cp_0[expr_498_cp_1] + 1f;
                expr_498_cp_0[expr_498_cp_1] = num26;
                if (num26 >= num8)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                }
            }
            else if (npc.ai[0] == 3f) //homing fireball spit
            {
                npc.ai[1] += 1f;
                int num29 = (npc.Center.X < targetData.Center.X) ? 1 : -1;
                npc.ai[2] = (float)num29;
                if (npc.ai[1] < num11)
                {
                    Vector2 vector4 = targetData.Center + new Vector2((float)num29 * -num9, -250f);
                    Vector2 value = npc.DirectionTo(vector4) * num10;
                    if (npc.Distance(vector4) < num10)
                    {
                        npc.Center = vector4;
                    }
                    else
                    {
                        npc.position += value;
                    }
                    if (Vector2.Distance(vector4, npc.Center) < 16f)
                    {
                        npc.ai[1] = num11 - 1f;
                    }
                    num2 = 1.5f;
                }
                if (npc.ai[1] == num11)
                {
                    int num30 = (targetData.Center.X > npc.Center.X) ? 1 : -1;
                    npc.velocity = new Vector2((float)num30, 0f) * 22f; //10f
                    npc.direction = (npc.spriteDirection = num30);
                }
                if (npc.ai[1] >= num11)
                {
                    if (npc.ai[1] % 10 == 0 && Main.netMode != 1)
                    {
                        float num33 = 30f;
                        Vector2 position = npc.Center + new Vector2((110f /* 110f */ + num33) * (float)npc.direction, -20f /* 20f */).RotatedBy((double)npc.rotation, 
                            default(Vector2));
                        Projectile.NewProjectile(position, npc.velocity, mod.ProjectileType("YharonFireballHoming"), num4, 0f, Main.myPlayer, 0f, 0f);
                    }
                    num2 = 1.5f;
                    if (Math.Abs(targetData.Center.X - npc.Center.X) > 550f && Math.Abs(npc.velocity.X) < 20f)
                    {
                        npc.velocity.X = npc.velocity.X + (float)Math.Sign(npc.velocity.X) * 0.5f;
                    }
                }
                if (npc.ai[1] >= num13)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                }
            }
            else if (npc.ai[0] == 4f) //splitting fireball spit
            {
                int num31 = (npc.Center.X < targetData.Center.X) ? 1 : -1;
                npc.ai[2] = (float)num31;
                if (npc.ai[1] < num15)
                {
                    Vector2 vector5 = targetData.Center + new Vector2((float)num31 * -num14, -350f);
                    Vector2 value2 = npc.DirectionTo(vector5) * scaleFactor3;
                    npc.velocity = Vector2.Lerp(npc.velocity, value2, amount);
                    int num32 = (npc.Center.X < targetData.Center.X) ? 1 : -1;
                    npc.direction = (npc.spriteDirection = num32);
                    if (Vector2.Distance(vector5, npc.Center) < 16f)
                    {
                        npc.ai[1] = num15 - 1f;
                    }
                    num2 = 1.5f;
                }
                else if (npc.ai[1] == num15)
                {
                    Vector2 vector6 = npc.DirectionTo(targetData.Center);
                    vector6.Y *= 0.25f;
                    vector6 = vector6.SafeNormalize(Vector2.UnitX * (float)npc.direction);
                    npc.spriteDirection = ((vector6.X > 0f) ? 1 : -1);
                    npc.rotation = vector6.ToRotation();
                    if (npc.spriteDirection == -1)
                    {
                        npc.rotation += 3.14159274f;
                    }
                    npc.velocity = vector6 * scaleFactor4;
                }
                else
                {
                    npc.position.X = npc.position.X + npc.DirectionTo(targetData.Center).X * 7f;
                    npc.position.Y = npc.position.Y + npc.DirectionTo(targetData.Center + new Vector2(0f, -400f)).Y * 6f;
                    if (npc.ai[1] <= num19 - num18)
                    {
                        num2 = 1.5f;
                    }
                    float num33 = 30f;
                    Vector2 position = npc.Center + new Vector2((110f /* 110f */ + num33) * (float)npc.direction, -20f).RotatedBy((double)npc.rotation, 
                        default(Vector2));
                    int num34 = (int)(npc.ai[1] - num15 + 1f);
                    if (num34 <= num17 && num34 % num16 == 0 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(position, npc.velocity, mod.ProjectileType("YharonFireball"), num4, 0f, Main.myPlayer, 0f, 0f); //change
                    }
                }
                if (npc.ai[1] > num19 - num18)
                {
                    npc.velocity.Y = npc.velocity.Y - 0.1f;
                }
                npc.ai[1] += 1f;
                if (npc.ai[1] >= num19)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                }
            }
            else if (npc.ai[0] == 5f)
            {
                npc.velocity = npc.velocity.RotatedBy((double)(-(double)num22 * (float)npc.direction), default(Vector2));
                npc.position.Y = npc.position.Y - num23;
                npc.position += npc.DirectionTo(targetData.Center) * 10f;
                npc.rotation -= num22 * (float)npc.direction;
                num2 *= 0.7f;
                if (npc.ai[1] == 1f)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoarShort"), (int)npc.position.X, (int)npc.position.Y);
                }
                float[] expr_B0F_cp_0 = npc.ai;
                int expr_B0F_cp_1 = 1;
                num26 = expr_B0F_cp_0[expr_B0F_cp_1] + 1f;
                expr_B0F_cp_0[expr_B0F_cp_1] = num26;
                if (npc.ai[1] % 12 == 0 && Main.netMode != 1)
                {
                    float num33 = 30f;
                    Vector2 position = npc.Center + new Vector2((110f /* 110f */ + num33) * (float)npc.direction, -20f /* 20f */).RotatedBy((double)npc.rotation,
                        default(Vector2));
                    int projectileType = (((npc.ai[1] / 2) % 12 == 0) ? mod.ProjectileType("YharonFireball") : mod.ProjectileType("YharonFireballHoming"));
                    Projectile.NewProjectile(position, npc.velocity, projectileType, num4, 0f, Main.myPlayer, 0f, 0f);
                }
                if (num26 >= num20)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.velocity /= 2f;
                }
            }
            else if (npc.ai[0] == 6f) //birb spawn
            {
                if (npc.ai[1] == 0f)
                {
                    Vector2 destination2 = targetData.Center + new Vector2(0f, -200f);
                    Vector2 desiredVelocity2 = npc.DirectionTo(destination2) * scaleFactor * 2f;
                    npc.SimpleFlyMovement(desiredVelocity2, num6 * 2f);
                    int num35 = (npc.Center.X < targetData.Center.X) ? 1 : -1;
                    npc.direction = (npc.spriteDirection = num35);
                    npc.ai[2] += 1f;
                    if (npc.Distance(targetData.Center) < 1000f || npc.ai[2] >= 180f) //450f
                    {
                        npc.ai[1] = 1f;
                        npc.netUpdate = true;
                    }
                }
                else
                {
                    if (npc.ai[1] == 1f)
                    {
                        Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/YharonRoar"), (int)npc.position.X, (int)npc.position.Y);
                    }
                    if (npc.ai[1] < num25)
                    {
                        npc.velocity *= 0.95f;
                    }
                    else
                    {
                        npc.velocity *= 0.98f;
                    }
                    if (npc.ai[1] == num25)
                    {
                        if (npc.velocity.Y > 0f)
                        {
                            npc.velocity.Y = npc.velocity.Y / 3f;
                        }
                        npc.velocity.Y = npc.velocity.Y - 3f;
                    }
                    num2 *= 0.85f;
                    bool flag3 = npc.ai[1] == 20f || npc.ai[1] == 45f || npc.ai[1] == 70f;
                    if (NPC.CountNPCS(mod.NPCType("Bumblefuck3")) > 4)
                    {
                        flag3 = false;
                    }
                    if (flag3 && Main.netMode != 1)
                    {
                        Vector2 vector7 = npc.Center + (6.28318548f * Main.rand.NextFloat()).ToRotationVector2() * new Vector2(2f, 1f) * 300f * (0.6f + Main.rand.NextFloat() * 0.4f);
                        if (Vector2.Distance(vector7, targetData.Center) > 100f)
                        {
                            Point point2 = vector7.ToPoint();
                            NPC.NewNPC(point2.X, point2.Y, mod.NPCType("Bumblefuck3"), npc.whoAmI, 0f, 0f, 0f, 0f, 255);
                        }
                        for (int num41 = 0; num41 < 2; num41++)
                        {
                            NPC.NewNPC((int)npc.Center.X + (Main.rand.Next(2) == 0 ? 800 : -800), (int)npc.Center.Y - 200, mod.NPCType("Bumblefuck3"), 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    npc.ai[1] += 1f;
                }
                if (npc.ai[1] >= num24)
                {
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, mod.ProjectileType("BigFlare"), 0, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
                    Boom(600, num4);
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                }
            }
            npc.localAI[0] += num2;
            if (npc.localAI[0] >= 36f)
            {
                npc.localAI[0] = 0f;
            }
            if (num != -1)
            {
                npc.localAI[0] = (float)(num * 4);
            }
            float[] expr_11FC_cp_0 = npc.localAI;
            int expr_11FC_cp_1 = 1;
            num26 = expr_11FC_cp_0[expr_11FC_cp_1] + 1f;
            expr_11FC_cp_0[expr_11FC_cp_1] = num26;
            if (num26 >= 60f)
            {
                npc.localAI[1] = 0f;
            }
            float num42 = npc.DirectionTo(targetData.Center).ToRotation();
            float num43 = 0.04f;
            switch ((int)npc.ai[0])
            {
                case 2:
                case 5:
                    num43 = 0f;
                    break;
                case 3:
                    num43 = 0.01f;
                    num42 = 0f;
                    if (npc.spriteDirection == -1)
                    {
                        num42 -= 3.14159274f;
                    }
                    if (npc.ai[1] >= num11)
                    {
                        num42 += (float)npc.spriteDirection * 3.14159274f / 12f;
                        num43 = 0.05f;
                    }
                    break;
                case 4:
                    num43 = 0.01f;
                    num42 = 3.14159274f;
                    if (npc.spriteDirection == 1)
                    {
                        num42 += 3.14159274f;
                    }
                    break;
                case 6:
                    num43 = 0.02f;
                    num42 = 0f;
                    if (npc.spriteDirection == -1)
                    {
                        num42 -= 3.14159274f;
                    }
                    break;
            }
            if (npc.spriteDirection == -1)
            {
                num42 += 3.14159274f;
            }
            if (num43 != 0f)
            {
                npc.rotation = npc.rotation.AngleTowards(num42, num43);
            }
        }
        #endregion

        #region Drawing
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (npc.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            float num66 = 0f;
            Vector2 vector11 = new Vector2((float)(Main.npcTexture[npc.type].Width / 2), (float)(Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2));
            Microsoft.Xna.Framework.Rectangle frame6 = npc.frame;
            Microsoft.Xna.Framework.Color color9 = Lighting.GetColor((int)((double)npc.position.X + (double)npc.width * 0.5) / 16, (int)(((double)npc.position.Y + (double)npc.height * 0.5) / 16.0));
            if (npc.localAI[2] == 1f && !phaseOneLoot)
            {
                SpriteEffects spriteEffects2 = spriteEffects ^ SpriteEffects.FlipHorizontally;
                if (npc.localAI[3] < 900f)
                {
                    color9 = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, 0);
                }
                Main.spriteBatch.Draw(Main.npcTexture[npc.type], 
                    new Vector2(npc.position.X - Main.screenPosition.X + (float)(npc.width / 2) - (float)Main.npcTexture[npc.type].Width * npc.scale / 2f + vector11.X * npc.scale, 
                    npc.position.Y - Main.screenPosition.Y + (float)npc.height - (float)Main.npcTexture[npc.type].Height * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + vector11.Y * npc.scale + num66 + npc.gfxOffY), 
                    new Microsoft.Xna.Framework.Rectangle?(frame6), 
                    npc.GetAlpha(color9), 
                    npc.rotation, 
                    vector11, 
                    npc.scale, 
                    spriteEffects2,
                    0f);
                return false;
            }
			Microsoft.Xna.Framework.Color alpha15 = npc.GetAlpha(color9);
			float num212 = 1f - (float)npc.life / (float)npc.lifeMax;
			num212 *= num212;
			alpha15.R = (byte)((float)alpha15.R * num212);
			alpha15.G = (byte)((float)alpha15.G * num212);
			alpha15.B = (byte)((float)alpha15.B * num212);
			alpha15.A = (byte)((float)alpha15.A * num212);
			for (int num213 = 0; num213 < 4; num213++) 
			{
				Vector2 position9 = npc.position;
				float num214 = Math.Abs(npc.Center.X - Main.player[Main.myPlayer].Center.X);
				float num215 = Math.Abs(npc.Center.Y - Main.player[Main.myPlayer].Center.Y);
				if (num213 == 0 || num213 == 2) 
				{
					position9.X = Main.player[Main.myPlayer].Center.X + num214;
				} 
				else 
				{
					position9.X = Main.player[Main.myPlayer].Center.X - num214;
				}
				position9.X -= (float)(npc.width / 2);
				if (num213 == 0 || num213 == 1) 
				{
					position9.Y = Main.player[Main.myPlayer].Center.Y + num215;
				} 
				else
				{
					position9.Y = Main.player[Main.myPlayer].Center.Y - num215;
				}
				position9.Y -= (float)(npc.height / 2);
				Main.spriteBatch.Draw(Main.npcTexture[npc.type], new Vector2(position9.X - Main.screenPosition.X + (float)(npc.width / 2) - (float)Main.npcTexture[npc.type].Width * npc.scale / 2f + vector11.X * npc.scale, position9.Y - Main.screenPosition.Y + (float)npc.height - (float)Main.npcTexture[npc.type].Height * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + vector11.Y * npc.scale + num66 + npc.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(frame6), alpha15, npc.rotation, vector11, npc.scale, spriteEffects, 0f);
			}
			Main.spriteBatch.Draw(Main.npcTexture[npc.type], new Vector2(npc.position.X - Main.screenPosition.X + (float)(npc.width / 2) - (float)Main.npcTexture[npc.type].Width * npc.scale / 2f + vector11.X * npc.scale, npc.position.Y - Main.screenPosition.Y + (float)npc.height - (float)Main.npcTexture[npc.type].Height * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + vector11.Y * npc.scale + num66 + npc.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(frame6), npc.GetAlpha(color9), npc.rotation, vector11, npc.scale, spriteEffects, 0f);
			return false;
		}
        #endregion

        #region Loot
        public override void NPCLoot()
		{
            if (!dropLoot)
            {
                return;
            }
            if (npc.localAI[2] == 1f)
            {
                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("BossRush"));
            }
            if (Main.rand.Next(10) == 0 && npc.localAI[2] == 1f)
			{
				Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("YharonTrophy"));
			}
            if (CalamityWorld.armageddon)
            {
                for (int i = 0; i < 10; i++)
                {
                    npc.DropBossBags();
                }
            }
            if (Main.expertMode)
			{
                if (npc.localAI[2] == 1f)
                {
                    npc.DropItemInstanced(npc.position, npc.Size, mod.ItemType("HellcasterFragment"), Main.rand.Next(22, 29), true);
                }
				npc.DropBossBags();
			}
			else
			{
                if (npc.localAI[2] == 1f)
                {
                    npc.DropItemInstanced(npc.position, npc.Size, mod.ItemType("HellcasterFragment"), Main.rand.Next(15, 23), true);
                }
                if (Main.rand.Next(7) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("YharonMask"));
				}
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("AngryChickenStaff"));
				}
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("PhoenixFlameBarrage"));
				}
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("DragonsBreath"));
				}
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("DragonRage"));
				}
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("ProfanedTrident"));
				}
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("TheBurningSky"));
				}
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("ChickenCannon"));
				}
			}
		}
        #endregion

        public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = mod.ItemType("SupremeHealingPotion");
		}

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[npc.target];
            if (player.vortexStealthActive && projectile.ranged)
            {
                damage /= 2;
                crit = false;
            }
            if (((projectile.type == ProjectileID.HallowStar || projectile.type == ProjectileID.CrystalShard) && projectile.ranged) ||
                projectile.type == mod.ProjectileType("TerraBulletSplit") || projectile.type == mod.ProjectileType("TerraArrow2"))
            {
                damage /= 8;
            }
        }

        #region DamageFormula
        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
            if (damage > npc.lifeMax / 10)
            {
                damage = 0;
                return false;
            }
            double newDamage = (damage + (int)((double)defense * 0.25));
            if (newDamage < 1.0)
			{
				newDamage = 1.0;
			}
			if (newDamage >= 1.0)
			{
                if (npc.localAI[2] == 1f)
                {
                    float protection = (((npc.ichor || npc.onFire2) ? 0.17f : 0.22f) +
                    (protectionBoost ? 0.68f : 0f)); //0.85 or 0.9
                    if (CalamityWorld.defiled)
                    {
                        protection += (1f - protection) * 0.5f;
                    }
                    newDamage = (double)((int)((double)(1f - protection) * newDamage));
                }
                else
                {
                    float protection = (((npc.ichor || npc.onFire2) ? 0.12f : 0.17f) +
                    (protectionBoost ? 0.73f : 0f)); //0.85 or 0.9
                    if (CalamityWorld.defiled)
                    {
                        protection += (1f - protection) * 0.5f;
                    }
                    newDamage = (double)((int)((double)(1f - protection) * newDamage));
                }
				if (newDamage < 1.0)
				{
					newDamage = 1.0;
				}
			}
			damage = newDamage;
			return true;
		}
        #endregion

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			scale = 2f;
			return null;
		}
		
		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			cooldownSlot = 1;
			return true;
		}

        #region FindFrame
        public override void FindFrame(int frameHeight)
		{
			if ((npc.localAI[2] != 1f && (npc.ai[0] == 0f || npc.ai[0] == 6f || npc.ai[0] == 13f || npc.ai[0] == 21f)) || 
                (npc.localAI[2] == 1f && (npc.ai[0] == 5f || npc.ai[0] < 3f))) //idle
			{
				int num84 = 4; //5
				if (npc.localAI[2] != 1f && (npc.ai[0] == 6f || npc.ai[0] == 13f || npc.ai[0] == 21f)) //Phase ai switch
				{
					num84 = 3; //4
				}
				npc.frameCounter += 1.0;
				if (npc.frameCounter > (double)num84)
				{
					npc.frameCounter = 0.0;
					npc.frame.Y = npc.frame.Y + frameHeight;
				}
				if (npc.frame.Y >= frameHeight * 5) //6
				{
					npc.frame.Y = 0;
				}
			}
			if ((npc.localAI[2] != 1f && (npc.ai[0] == 1f || npc.ai[0] == 5f || npc.ai[0] == 7f || npc.ai[0] == 11f || 
                npc.ai[0] == 14f || npc.ai[0] == 18f || npc.ai[0] == 22f)) || (npc.localAI[2] == 1f && (npc.ai[0] == 6f))) //Charging or birb spawn
			{
				npc.frame.Y = frameHeight * 5; //6
			}
			if ((npc.localAI[2] != 1f && (npc.ai[0] == 2f || npc.ai[0] == 8f || npc.ai[0] == 12f || npc.ai[0] == 15f || npc.ai[0] == 19f || npc.ai[0] == 23f)) || 
                (npc.localAI[2] == 1f && (npc.ai[0] == 4f || npc.ai[0] == 3f))) //Fireball spit or circle or flamethrower
			{
				npc.frame.Y = frameHeight * 5; //6
			}
			if (npc.localAI[2] != 1f && (npc.ai[0] == 3f || npc.ai[0] == 9f || npc.ai[0] == -1f || npc.ai[0] == 16f || npc.ai[0] == 20f || npc.ai[0] == 24f)) //Summon tornadoes
			{
				int num85 = 90;
				if (npc.ai[2] < (float)(num85 - 30) || npc.ai[2] > (float)(num85 - 10))
				{
					npc.frameCounter += 1.0;
					if (npc.frameCounter > 4.0) //5
					{
						npc.frameCounter = 0.0;
						npc.frame.Y = npc.frame.Y + frameHeight;
					}
					if (npc.frame.Y >= frameHeight * 5) //6
					{
						npc.frame.Y = 0;
					}
				}
				else
				{
					npc.frame.Y = frameHeight * 5; //6
					if (npc.ai[2] > (float)(num85 - 20) && npc.ai[2] < (float)(num85 - 15))
					{
						npc.frame.Y = frameHeight * 6; //7
					}
				}
			}
			if (npc.localAI[2] != 1f && (npc.ai[0] == 4f || npc.ai[0] == 10f || npc.ai[0] == 17f || npc.ai[0] == 25f)) //Enter new phase
			{
				int num86 = 180;
				if (npc.ai[2] < (float)(num86 - 60) || npc.ai[2] > (float)(num86 - 20))
				{
					npc.frameCounter += 1.0;
					if (npc.frameCounter > 4.0) //5
					{
						npc.frameCounter = 0.0;
						npc.frame.Y = npc.frame.Y + frameHeight;
					}
					if (npc.frame.Y >= frameHeight * 5) //6
					{
						npc.frame.Y = 0;
					}
				}
				else
				{
					npc.frame.Y = frameHeight * 5; //6
					if (npc.ai[2] > (float)(num86 - 50) && npc.ai[2] < (float)(num86 - 25))
					{
						npc.frame.Y = frameHeight * 6; //7
					}
				}
			}
		}
        #endregion

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax * 0.8f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.8f);
		}

        public void Boom(int timeLeft, int damage)
        {
            if (Main.netMode != 1)
            {
                Vector2 valueBoom = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                float spreadBoom = 15f * 0.0174f;
                double startAngleBoom = Math.Atan2(npc.velocity.X, npc.velocity.Y) - spreadBoom / 2;
                double deltaAngleBoom = spreadBoom / 8f;
                double offsetAngleBoom;
                int iBoom;
                for (iBoom = 0; iBoom < 25; iBoom++)
                {
                    offsetAngleBoom = (startAngleBoom + deltaAngleBoom * (iBoom + iBoom * iBoom) / 2f) + 32f * iBoom;
                    int boom1 = Projectile.NewProjectile(valueBoom.X, valueBoom.Y, (float)(Math.Sin(offsetAngleBoom) * 5f), (float)(Math.Cos(offsetAngleBoom) * 5f), mod.ProjectileType("FlareBomb"), damage, 0f, Main.myPlayer, 0f, 0f);
                    int boom2 = Projectile.NewProjectile(valueBoom.X, valueBoom.Y, (float)(-Math.Sin(offsetAngleBoom) * 5f), (float)(-Math.Cos(offsetAngleBoom) * 5f), mod.ProjectileType("FlareBomb"), damage, 0f, Main.myPlayer, 0f, 0f);
                    Main.projectile[boom1].timeLeft = timeLeft;
                    Main.projectile[boom2].timeLeft = timeLeft;
                }
            }
        }

        #region HitEffect
        public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 5; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
			}
			if (npc.life <= 0)
			{
                Boom(150, 1000);
				npc.position.X = npc.position.X + (float)(npc.width / 2);
				npc.position.Y = npc.position.Y + (float)(npc.height / 2);
				npc.width = 300;
				npc.height = 280;
				npc.position.X = npc.position.X - (float)(npc.width / 2);
				npc.position.Y = npc.position.Y - (float)(npc.height / 2);
				for (int num621 = 0; num621 < 40; num621++)
				{
					int num622 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 244, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num622].velocity *= 3f;
					if (Main.rand.Next(2) == 0)
					{
						Main.dust[num622].scale = 0.5f;
						Main.dust[num622].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
					}
				}
				for (int num623 = 0; num623 < 70; num623++)
				{
					int num624 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 244, 0f, 0f, 100, default(Color), 3f);
					Main.dust[num624].noGravity = true;
					Main.dust[num624].velocity *= 5f;
					num624 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 244, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num624].velocity *= 2f;
				}
			}
		}
        #endregion
    }
}