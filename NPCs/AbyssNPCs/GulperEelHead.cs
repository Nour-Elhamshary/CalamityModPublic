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

namespace CalamityMod.NPCs.AbyssNPCs
{
	public class GulperEelHead : ModNPC
	{
        public bool detectsPlayer = false;
        public bool flies = true;
        public const int minLength = 20;
        public const int maxLength = 21;
        public float speed = 5f; //10
        public float turnSpeed = 0.075f; //0.15
        bool TailSpawned = false;

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gulper Eel");
		}
		
		public override void SetDefaults()
		{
			npc.damage = 120;
			npc.width = 66; //36
			npc.height = 86; //20
			npc.defense = 50;
			npc.lifeMax = 35000;
			npc.aiStyle = -1;
            aiType = -1;
            for (int k = 0; k < npc.buffImmune.Length; k++)
            {
                npc.buffImmune[k] = true;
            }
            npc.knockBackResist = 0f;
			npc.value = Item.buyPrice(0, 5, 0, 0);
			npc.behindTiles = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit9;
			npc.DeathSound = SoundID.NPCDeath13;
			npc.netAlways = true;
		}
		
		public override void AI()
		{
            if ((Main.player[npc.target].Center - npc.Center).Length() < ((Main.player[npc.target].GetModPlayer<CalamityPlayer>(mod).anechoicPlating ||
                Main.player[npc.target].GetModPlayer<CalamityPlayer>(mod).anechoicCoating) ? 100f : 600f) *
                (Main.player[npc.target].GetModPlayer<CalamityPlayer>(mod).fishAlert ? 3f : 1f) ||
                npc.justHit)
            {
                detectsPlayer = true;
            }
            npc.chaseable = detectsPlayer;
            if (npc.ai[3] > 0f)
            {
                npc.realLife = (int)npc.ai[3];
            }
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
            {
                npc.TargetClosest(true);
            }
            npc.velocity.Length();
            if (Main.netMode != 1)
            {
                if (!TailSpawned && npc.ai[0] == 0f)
                {
                    int Previous = npc.whoAmI;
                    for (int num36 = 0; num36 < maxLength; num36++)
                    {
                        int lol = 0;
                        if (num36 >= 0 && num36 < minLength)
                        {
                            if (num36 == 0)
                            {
                                lol = NPC.NewNPC((int)npc.position.X + (npc.width / 2), (int)npc.position.Y + (npc.height / 2), mod.NPCType("GulperEelBody"), npc.whoAmI);
                            }
                            else
                            {
                                lol = NPC.NewNPC((int)npc.position.X + (npc.width / 2), (int)npc.position.Y + (npc.height / 2), mod.NPCType("GulperEelBodyAlt"), npc.whoAmI);
                            }
                        }
                        else
                        {
                            lol = NPC.NewNPC((int)npc.position.X + (npc.width / 2), (int)npc.position.Y + (npc.height / 2), mod.NPCType("GulperEelTail"), npc.whoAmI);
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
            }
            int num180 = (int)(npc.position.X / 16f) - 1;
            int num181 = (int)((npc.position.X + (float)npc.width) / 16f) + 2;
            int num182 = (int)(npc.position.Y / 16f) - 1;
            int num183 = (int)((npc.position.Y + (float)npc.height) / 16f) + 2;
            if (num180 < 0)
            {
                num180 = 0;
            }
            if (num181 > Main.maxTilesX)
            {
                num181 = Main.maxTilesX;
            }
            if (num182 < 0)
            {
                num182 = 0;
            }
            if (num183 > Main.maxTilesY)
            {
                num183 = Main.maxTilesY;
            }
            if (npc.velocity.X < 0f)
            {
                npc.spriteDirection = -1;
            }
            else if (npc.velocity.X > 0f)
            {
                npc.spriteDirection = 1;
            }
            bool canFly = flies;
            if (Main.player[npc.target].dead)
            {
                npc.TargetClosest(false);
            }
            npc.alpha -= 42;
            if (npc.alpha < 0)
            {
                npc.alpha = 0;
            }
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 5600f || !NPC.AnyNPCs(mod.NPCType("GulperEelTail")))
            {
                npc.active = false;
            }
            float num188 = speed;
            float num189 = turnSpeed;
            Vector2 vector18 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
            float num191 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2);
            float num192 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2);
            int num42 = -1;
            int num43 = (int)(Main.player[npc.target].Center.X / 16f);
            int num44 = (int)(Main.player[npc.target].Center.Y / 16f);
            for (int num45 = num43 - 2; num45 <= num43 + 2; num45++)
            {
                for (int num46 = num44; num46 <= num44 + 15; num46++)
                {
                    if (WorldGen.SolidTile2(num45, num46))
                    {
                        num42 = num46;
                        break;
                    }
                }
                if (num42 > 0)
                {
                    break;
                }
            }
            if (num42 > 0)
            {
                num42 *= 16;
                float num47 = (float)(num42 - 200); //800
                if (!detectsPlayer)
                {
                    num192 = num47;
                    if (Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < 250f)
                    {
                        if (npc.velocity.X > 0f)
                        {
                            num191 = Main.player[npc.target].Center.X + 300f;
                        }
                        else
                        {
                            num191 = Main.player[npc.target].Center.X - 300f;
                        }
                    }
                }
            }
            float num48 = num188 * 1.3f;
            float num49 = num188 * 0.7f;
            float num50 = npc.velocity.Length();
            if (num50 > 0f)
            {
                if (num50 > num48)
                {
                    npc.velocity.Normalize();
                    npc.velocity *= num48;
                }
                else if (num50 < num49)
                {
                    npc.velocity.Normalize();
                    npc.velocity *= num49;
                }
            }
            if (!detectsPlayer)
            {
                for (int num51 = 0; num51 < 200; num51++)
                {
                    if (Main.npc[num51].active && Main.npc[num51].type == npc.type && num51 != npc.whoAmI)
                    {
                        Vector2 vector3 = Main.npc[num51].Center - npc.Center;
                        if (vector3.Length() < 400f)
                        {
                            vector3.Normalize();
                            vector3 *= 1000f;
                            num191 -= vector3.X;
                            num192 -= vector3.Y;
                        }
                    }
                }
            }
            else
            {
                for (int num52 = 0; num52 < 200; num52++)
                {
                    if (Main.npc[num52].active && Main.npc[num52].type == npc.type && num52 != npc.whoAmI)
                    {
                        Vector2 vector4 = Main.npc[num52].Center - npc.Center;
                        if (vector4.Length() < 60f)
                        {
                            vector4.Normalize();
                            vector4 *= 200f;
                            num191 -= vector4.X;
                            num192 -= vector4.Y;
                        }
                    }
                }
            }
            num191 = (float)((int)(num191 / 16f) * 16);
            num192 = (float)((int)(num192 / 16f) * 16);
            vector18.X = (float)((int)(vector18.X / 16f) * 16);
            vector18.Y = (float)((int)(vector18.Y / 16f) * 16);
            num191 -= vector18.X;
            num192 -= vector18.Y;
            float num193 = (float)System.Math.Sqrt((double)(num191 * num191 + num192 * num192));
            if (npc.ai[1] > 0f && npc.ai[1] < (float)Main.npc.Length)
            {
                try
                {
                    vector18 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                    num191 = Main.npc[(int)npc.ai[1]].position.X + (float)(Main.npc[(int)npc.ai[1]].width / 2) - vector18.X;
                    num192 = Main.npc[(int)npc.ai[1]].position.Y + (float)(Main.npc[(int)npc.ai[1]].height / 2) - vector18.Y;
                }
                catch
                {
                }
                npc.rotation = (float)System.Math.Atan2((double)num192, (double)num191) + 1.57f;
                num193 = (float)System.Math.Sqrt((double)(num191 * num191 + num192 * num192));
                int num194 = npc.width;
                num193 = (num193 - (float)num194) / num193;
                num191 *= num193;
                num192 *= num193;
                npc.velocity = Vector2.Zero;
                npc.position.X = npc.position.X + num191;
                npc.position.Y = npc.position.Y + num192;
                if (num191 < 0f)
                {
                    npc.spriteDirection = -1;
                }
                else if (num191 > 0f)
                {
                    npc.spriteDirection = 1;
                }
            }
            else
            {
                num193 = (float)System.Math.Sqrt((double)(num191 * num191 + num192 * num192));
                float num196 = System.Math.Abs(num191);
                float num197 = System.Math.Abs(num192);
                float num198 = num188 / num193;
                num191 *= num198;
                num192 *= num198;
                if ((npc.velocity.X > 0f && num191 > 0f) || (npc.velocity.X < 0f && num191 < 0f) || (npc.velocity.Y > 0f && num192 > 0f) || (npc.velocity.Y < 0f && num192 < 0f))
                {
                    if (npc.velocity.X < num191)
                    {
                        npc.velocity.X = npc.velocity.X + num189;
                    }
                    else
                    {
                        if (npc.velocity.X > num191)
                        {
                            npc.velocity.X = npc.velocity.X - num189;
                        }
                    }
                    if (npc.velocity.Y < num192)
                    {
                        npc.velocity.Y = npc.velocity.Y + num189;
                    }
                    else
                    {
                        if (npc.velocity.Y > num192)
                        {
                            npc.velocity.Y = npc.velocity.Y - num189;
                        }
                    }
                    if ((double)System.Math.Abs(num192) < (double)num188 * 0.2 && ((npc.velocity.X > 0f && num191 < 0f) || (npc.velocity.X < 0f && num191 > 0f)))
                    {
                        if (npc.velocity.Y > 0f)
                        {
                            npc.velocity.Y = npc.velocity.Y + num189 * 2f;
                        }
                        else
                        {
                            npc.velocity.Y = npc.velocity.Y - num189 * 2f;
                        }
                    }
                    if ((double)System.Math.Abs(num191) < (double)num188 * 0.2 && ((npc.velocity.Y > 0f && num192 < 0f) || (npc.velocity.Y < 0f && num192 > 0f)))
                    {
                        if (npc.velocity.X > 0f)
                        {
                            npc.velocity.X = npc.velocity.X + num189 * 2f; //changed from 2
                        }
                        else
                        {
                            npc.velocity.X = npc.velocity.X - num189 * 2f; //changed from 2
                        }
                    }
                }
                else
                {
                    if (num196 > num197)
                    {
                        if (npc.velocity.X < num191)
                        {
                            npc.velocity.X = npc.velocity.X + num189 * 1.1f; //changed from 1.1
                        }
                        else if (npc.velocity.X > num191)
                        {
                            npc.velocity.X = npc.velocity.X - num189 * 1.1f; //changed from 1.1
                        }
                        if ((double)(System.Math.Abs(npc.velocity.X) + System.Math.Abs(npc.velocity.Y)) < (double)num188 * 0.5)
                        {
                            if (npc.velocity.Y > 0f)
                            {
                                npc.velocity.Y = npc.velocity.Y + num189;
                            }
                            else
                            {
                                npc.velocity.Y = npc.velocity.Y - num189;
                            }
                        }
                    }
                    else
                    {
                        if (npc.velocity.Y < num192)
                        {
                            npc.velocity.Y = npc.velocity.Y + num189 * 1.1f;
                        }
                        else if (npc.velocity.Y > num192)
                        {
                            npc.velocity.Y = npc.velocity.Y - num189 * 1.1f;
                        }
                        if ((double)(System.Math.Abs(npc.velocity.X) + System.Math.Abs(npc.velocity.Y)) < (double)num188 * 0.5)
                        {
                            if (npc.velocity.X > 0f)
                            {
                                npc.velocity.X = npc.velocity.X + num189;
                            }
                            else
                            {
                                npc.velocity.X = npc.velocity.X - num189;
                            }
                        }
                    }
                }
            }
            npc.rotation = (float)System.Math.Atan2((double)npc.velocity.Y, (double)npc.velocity.X) + 1.57f;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion)
            {
                return detectsPlayer;
            }
            return null;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (npc.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Vector2 center = new Vector2(npc.Center.X, npc.Center.Y);
            Vector2 vector11 = new Vector2((float)(Main.npcTexture[npc.type].Width / 2), (float)(Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2));
            Vector2 vector = center - Main.screenPosition;
            vector -= new Vector2((float)mod.GetTexture("NPCs/AbyssNPCs/GulperEelHeadGlow").Width, (float)(mod.GetTexture("NPCs/AbyssNPCs/GulperEelHeadGlow").Height / Main.npcFrameCount[npc.type])) * 1f / 2f;
            vector += vector11 * 1f + new Vector2(0f, 0f + 4f + npc.gfxOffY);
            Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(127 - npc.alpha, 127 - npc.alpha, 127 - npc.alpha, 0).MultiplyRGBA(Microsoft.Xna.Framework.Color.LightYellow);
            Main.spriteBatch.Draw(mod.GetTexture("NPCs/AbyssNPCs/GulperEelHeadGlow"), vector,
                new Microsoft.Xna.Framework.Rectangle?(npc.frame), color, npc.rotation, vector11, 1f, spriteEffects, 0f);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.player.GetModPlayer<CalamityPlayer>(mod).ZoneAbyssLayer3 && spawnInfo.water && !NPC.AnyNPCs(mod.NPCType("GulperEelHead")))
            {
                return 0.05f;
            }
            if (spawnInfo.player.GetModPlayer<CalamityPlayer>(mod).ZoneAbyssLayer4 && spawnInfo.water && !NPC.AnyNPCs(mod.NPCType("GulperEelHead")))
            {
                return 0.1f;
            }
            return 0f;
        }

        public override void NPCLoot()
        {
            if (Main.rand.Next(1000) == 0 && CalamityWorld.revenge)
            {
                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("HalibutCannon"));
            }
            if (NPC.downedPlantBoss || CalamityWorld.downedCalamitas)
            {
                if (Main.rand.Next(2) == 0)
                {
                    Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("Lumenite"), Main.rand.Next(6, 9));
                }
                if (Main.expertMode && Main.rand.Next(2) == 0)
                {
                    Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("Lumenite"), Main.rand.Next(2, 4));
                }
                if (Main.rand.Next(2) == 0)
                {
                    Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("DepthCells"), Main.rand.Next(6, 9));
                }
                if (Main.expertMode && Main.rand.Next(2) == 0)
                {
                    Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("DepthCells"), Main.rand.Next(2, 4));
                }
            }
        }

        public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 5; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
			}
			if (npc.life <= 0)
			{
				for (int k = 0; k < 15; k++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
				}
			}
		}

        public override bool CheckActive()
        {
            if (npc.timeLeft <= 0 && Main.netMode != 1)
            {
                for (int k = (int)npc.ai[0]; k > 0; k = (int)Main.npc[k].ai[0])
                {
                    if (Main.npc[k].active)
                    {
                        Main.npc[k].active = false;
                        if (Main.netMode == 2)
                        {
                            Main.npc[k].life = 0;
                            Main.npc[k].netSkip = -1;
                            NetMessage.SendData(23, -1, -1, null, k, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                }
            }
            return true;
        }

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            player.AddBuff(BuffID.Bleeding, 300, true);
            player.AddBuff(mod.BuffType("CrushDepth"), 300, true);
            if (CalamityWorld.revenge)
            {
                player.AddBuff(mod.BuffType("MarkedforDeath"), 300);
                player.AddBuff(mod.BuffType("Horror"), 300, true);
            }
        }
	}
}