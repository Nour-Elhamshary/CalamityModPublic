﻿using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CalamityMod.NPCs.VanillaNPCOverrides.Bosses
{
    public static class GolemAI
    {
        public static bool BuffedGolemAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // whoAmI variable
            NPC.golemBoss = npc.whoAmI;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Phases
            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;
            bool phase2 = lifeRatio < 0.75f;
            bool phase3 = lifeRatio < 0.5f;
            bool phase4 = lifeRatio < 0.25f;

            // Spawn parts
            if (npc.localAI[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.localAI[0] = 1f;
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X - 84, (int)npc.Center.Y - 9, NPCID.GolemFistLeft);
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X + 78, (int)npc.Center.Y - 9, NPCID.GolemFistRight);
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X - 3, (int)npc.Center.Y - 57, NPCID.GolemHead);
            }

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            // Despawn
            if (npc.target >= 0 && Main.player[npc.target].dead)
            {
                npc.TargetClosest();
                if (Main.player[npc.target].dead)
                    npc.noTileCollide = true;
            }

            // Enrage if the target isn't inside the temple
            // Turbo enrage if target isn't inside the temple and it's Boss Rush or For the Worthy
            bool enrage = true;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int num = (int)Main.player[npc.target].Center.X / 16;
                int num2 = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(num, num2);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || Main.getGoodWorld;
            }
            else
                turboEnrage = bossRush || Main.getGoodWorld;

            if (bossRush || Main.getGoodWorld)
                enrage = true;

            npc.Calamity().CurrentlyEnraged = !bossRush && (enrage || turboEnrage);

            bool reduceFallSpeed = npc.velocity.Y > 0f && Collision.SolidCollision(npc.position + Vector2.UnitY * 1.1f * npc.velocity.Y, npc.width, npc.height);

            // Alpha
            if (npc.alpha > 0)
            {
                npc.alpha -= 10;
                if (npc.alpha < 0)
                    npc.alpha = 0;

                npc.ai[1] = 0f;
            }

            // Check for body parts
            bool flag40 = NPC.AnyNPCs(NPCID.GolemHead);
            bool flag41 = NPC.AnyNPCs(NPCID.GolemFistLeft);
            bool flag42 = NPC.AnyNPCs(NPCID.GolemFistRight);
            npc.dontTakeDamage = (flag40 || flag41 || flag42) && !CalamityWorld.LegendaryMode;

            // Phase 2, check for free head
            bool flag43 = NPC.AnyNPCs(NPCID.GolemHeadFree);

            // Deactivate torches
            if (Main.netMode != NetmodeID.MultiplayerClient && Main.getGoodWorld && npc.velocity.Y > 0f)
            {
                for (int j = (int)(npc.position.X / 16f); (float)j < (npc.position.X + (float)npc.width) / 16f; j++)
                {
                    for (int k = (int)(npc.position.Y / 16f); (float)k < (npc.position.Y + (float)npc.width) / 16f; k++)
                    {
                        if (Main.tile[j, k].TileType == TileID.Torches)
                        {
                            Main.tile[j, k].Get<TileWallWireStateData>().HasTile = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendTileSquare(-1, j, k);
                        }
                    }
                }
            }

            // Spawn arm dust
            if (!Main.getGoodWorld)
            {
                if (!flag41)
                {
                    int num642 = Dust.NewDust(new Vector2(npc.Center.X - 80f, npc.Center.Y - 9f), 8, 8, 31, 0f, 0f, 100, default, 1f);
                    Dust dust = Main.dust[num642];
                    dust.alpha += Main.rand.Next(100);
                    dust.velocity *= 0.2f;
                    dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                    dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;

                    if (Main.rand.NextBool(10))
                    {
                        num642 = Dust.NewDust(new Vector2(npc.Center.X - 80f, npc.Center.Y - 9f), 8, 8, 6, 0f, 0f, 0, default, 1f);
                        if (Main.rand.Next(20) != 0)
                        {
                            Main.dust[num642].noGravity = true;
                            dust = Main.dust[num642];
                            dust.scale *= 1f + Main.rand.Next(10) * 0.1f;
                            dust.velocity.Y -= 1f;
                        }
                    }
                }
                if (!flag42)
                {
                    int num643 = Dust.NewDust(new Vector2(npc.Center.X + 62f, npc.Center.Y - 9f), 8, 8, 31, 0f, 0f, 100, default, 1f);
                    Dust dust = Main.dust[num643];
                    dust.alpha += Main.rand.Next(100);
                    dust.velocity *= 0.2f;
                    dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                    dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;

                    if (Main.rand.NextBool(10))
                    {
                        num643 = Dust.NewDust(new Vector2(npc.Center.X + 62f, npc.Center.Y - 9f), 8, 8, 6, 0f, 0f, 0, default, 1f);
                        if (Main.rand.Next(20) != 0)
                        {
                            Main.dust[num643].noGravity = true;
                            dust = Main.dust[num643];
                            dust.scale *= 1f + Main.rand.Next(10) * 0.1f;
                            dust.velocity.Y -= 1f;
                        }
                    }
                }
            }

            if (npc.noTileCollide && !Main.player[npc.target].dead)
            {
                if (npc.velocity.Y > 0f && npc.Bottom.Y > Main.player[npc.target].Top.Y)
                    npc.noTileCollide = false;
                else if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].Center, 1, 1) && !Collision.SolidCollision(npc.position, npc.width, npc.height))
                    npc.noTileCollide = false;
            }

            // Jump
            if (npc.ai[0] == 0f)
            {
                if (npc.velocity.Y == 0f)
                {
                    // Laser fire when head is dead
                    if (Main.netMode != NetmodeID.MultiplayerClient && (!flag40 || turboEnrage || CalamityWorld.LegendaryMode))
                    {
                        npc.localAI[1] += 1f;

                        float divisor = 15f -
                            (phase2 ? 4f : 0f) -
                            (phase3 ? 3f : 0f) -
                            (phase4 ? 2f : 0f);

                        if (enrage)
                            divisor = 5f;

                        if (turboEnrage && Main.getGoodWorld)
                            divisor = 2f;

                        Vector2 vector82 = new Vector2(npc.Center.X, npc.Center.Y - 60f);
                        if (npc.localAI[1] % divisor == 0f && (Vector2.Distance(Main.player[npc.target].Center, vector82) > 160f || !flag43))
                        {
                            float num673 = turboEnrage ? 12f : enrage ? 9f : 6f;
                            float num674 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector82.X;
                            float num675 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector82.Y;
                            float num676 = (float)Math.Sqrt(num674 * num674 + num675 * num675);

                            num676 = num673 / num676;
                            num674 *= num676;
                            num675 *= num676;
                            vector82.X += num674 * 3f;
                            vector82.Y += num675 * 3f;

                            int type = ProjectileID.EyeBeam;
                            int damage = npc.GetProjectileDamage(type);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int num677 = Projectile.NewProjectile(npc.GetSource_FromAI(), vector82.X, vector82.Y, num674, num675, type, damage, 0f, Main.myPlayer, 0f, 0f);
                                Main.projectile[num677].timeLeft = 480;
                                if (turboEnrage && Main.getGoodWorld)
                                    Main.projectile[num677].extraUpdates += 1;
                            }
                        }

                        if (npc.localAI[1] >= 15f)
                            npc.localAI[1] = 0f;
                    }

                    // Slow down
                    npc.velocity.X *= 0.8f;

                    // Delay before jumping
                    npc.ai[1] += 1f;
                    if (npc.ai[1] > 0f)
                    {
                        npc.ai[1] += 1f;
                        if (Main.getGoodWorld)
                            npc.ai[1] += 100f;
                        if (enrage || death)
                            npc.ai[1] += 18f;
                        else
                        {
                            if (!flag41)
                                npc.ai[1] += 6f;
                            if (!flag42)
                                npc.ai[1] += 6f;
                            if (!flag40)
                                npc.ai[1] += 6f;
                        }
                    }
                    if (npc.ai[1] >= 300f)
                    {
                        npc.ai[1] = -20f;
                        npc.frameCounter = 0.0;
                    }
                    else if (npc.ai[1] == -1f)
                    {
                        // Set jump velocity
                        npc.TargetClosest();

                        float velocityBoost = death ? 12f * (1f - lifeRatio) : 8f * (1f - lifeRatio);
                        float velocityX = 4f + velocityBoost;
                        if (enrage)
                            velocityX *= 1.5f;

                        float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                        npc.direction = playerLocation < 0 ? 1 : -1;
                        calamityGlobalNPC.newAI[1] = npc.direction;

                        npc.velocity.X = velocityX * npc.direction;

                        float distanceBelowTarget = npc.position.Y - (Main.player[npc.target].position.Y + 80f);
                        float speedMult = 1f;

                        float multiplier = turboEnrage ? 0.0025f : enrage ? 0.002f : 0.0015f;
                        if (distanceBelowTarget > 0f && ((!flag41 && !flag42) || turboEnrage || CalamityWorld.LegendaryMode))
                            speedMult += distanceBelowTarget * multiplier;

                        float speedMultLimit = turboEnrage ? 3.5f : enrage ? 3f : 2.5f;
                        if (speedMult > speedMultLimit)
                            speedMult = speedMultLimit;

                        if (Main.player[npc.target].position.Y < npc.Bottom.Y)
                            npc.velocity.Y = ((((!flag43 && !flag40) || turboEnrage || CalamityWorld.LegendaryMode) ? -15.1f : -12.1f) + (enrage ? -4f : 0f)) * speedMult;
                        else
                            npc.velocity.Y = 1f;

                        npc.noTileCollide = true;

                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;

                        npc.netUpdate = true;
                        npc.SyncExtraAI();
                    }
                }

                // Don't run custom gravity when starting a jump
                if (npc.ai[0] != 1f)
                    CustomGravity();
            }

            // Fall down
            else if (npc.ai[0] == 1f)
            {
                if (npc.velocity.Y == 0f)
                {
                    npc.TargetClosest();

                    // Play sound
                    SoundEngine.PlaySound(SoundID.Item14, npc.position);

                    npc.ai[0] = 0f;
                    calamityGlobalNPC.newAI[1] = 0f;
                    npc.SyncExtraAI();

                    // Dust and gore
                    for (int num644 = (int)npc.position.X - 20; num644 < (int)npc.position.X + npc.width + 40; num644 += 20)
                    {
                        for (int num645 = 0; num645 < 4; num645++)
                        {
                            int num646 = Dust.NewDust(new Vector2(npc.position.X - 20f, npc.position.Y + npc.height), npc.width + 20, 4, 31, 0f, 0f, 100, default, 1.5f);
                            Dust dust = Main.dust[num646];
                            dust.velocity *= 0.2f;
                        }
                        if (Main.netMode != NetmodeID.Server)
                        {
                            int num647 = Gore.NewGore(npc.GetSource_FromAI(), new Vector2(num644 - 20, npc.position.Y + npc.height - 8f), default, Main.rand.Next(61, 64), 1f);
                            Gore gore = Main.gore[num647];
                            gore.velocity *= 0.4f;
                        }
                    }

                    // Fireball explosion when head is dead
                    if (Main.netMode != NetmodeID.MultiplayerClient && (!flag40 || turboEnrage || CalamityWorld.LegendaryMode))
                    {
                        for (int num621 = 0; num621 < 10; num621++)
                        {
                            int num622 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                            Main.dust[num622].velocity.Y *= 6f;
                            Main.dust[num622].velocity.X *= 3f;
                            if (Main.rand.NextBool())
                            {
                                Main.dust[num622].scale = 0.5f;
                                Main.dust[num622].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                            }
                        }
                        for (int num623 = 0; num623 < 20; num623++)
                        {
                            int num624 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                            Main.dust[num624].noGravity = true;
                            Main.dust[num624].velocity.Y *= 10f;
                            num624 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                            Main.dust[num624].velocity.X *= 2f;
                        }

                        int totalFireballs = 5;
                        if (turboEnrage && Main.getGoodWorld)
                            totalFireballs *= 2;

                        int spawnX = npc.width / 2;
                        for (int i = 0; i < totalFireballs; i++)
                        {
                            Vector2 spawnVector = new Vector2(npc.Center.X + Main.rand.Next(-spawnX, spawnX), npc.Center.Y + npc.height / 2 * 0.8f);
                            Vector2 velocity = new Vector2(Main.rand.NextBool() ? Main.rand.Next(4, 6) : Main.rand.Next(-5, -3), Main.rand.Next(-1, 2));

                            if (death)
                                velocity *= 1.5f;

                            if (enrage)
                                velocity *= 1.25f;

                            if (turboEnrage)
                                velocity *= 1.25f;

                            int type = ProjectileID.Fireball;
                            int damage = npc.GetProjectileDamage(type);
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), spawnVector, velocity, type, damage, 0f, Main.myPlayer);
                            Main.projectile[proj].timeLeft = 240;
                            if (turboEnrage && Main.getGoodWorld)
                                Main.projectile[proj].extraUpdates += 1;
                        }

                        npc.netUpdate = true;
                    }
                }
                else
                {
                    // Velocity when falling
                    if (npc.position.X < Main.player[npc.target].position.X && npc.position.X + npc.width > Main.player[npc.target].position.X + Main.player[npc.target].width)
                    {
                        npc.velocity.X *= 0.8f;

                        if (npc.Bottom.Y < Main.player[npc.target].position.Y)
                        {
                            float fallSpeedBoost = death ? 1.2f * (1f - lifeRatio) : 0.8f * (1f - lifeRatio);
                            float fallSpeed = 0.2f + fallSpeedBoost;
                            if (enrage)
                                fallSpeed *= 1.5f;

                            npc.velocity.Y += fallSpeed;
                        }
                    }
                    else
                    {
                        float velocityXChange = death ? 0.3f : 0.2f;
                        if (npc.direction < 0)
                            npc.velocity.X -= velocityXChange;
                        else if (npc.direction > 0)
                            npc.velocity.X += velocityXChange;

                        float velocityBoost = death ? 9f * (1f - lifeRatio) : 6f * (1f - lifeRatio);
                        float velocityXCap = 3f + velocityBoost;
                        if (enrage)
                            velocityXCap *= 1.5f;

                        float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                        int directionRelativeToTarget = playerLocation < 0 ? 1 : -1;
                        bool slowDown = directionRelativeToTarget != calamityGlobalNPC.newAI[1];

                        if (slowDown)
                            velocityXCap *= 0.5f;

                        if (npc.velocity.X < -velocityXCap)
                            npc.velocity.X = -velocityXCap;
                        if (npc.velocity.X > velocityXCap)
                            npc.velocity.X = velocityXCap;
                    }

                    CustomGravity();
                }
            }

            void CustomGravity()
            {
                float gravity = turboEnrage ? (Main.getGoodWorld ? 1.2f : 0.9f) : enrage ? 0.6f : (!flag41 && !flag42) ? 0.45f : 0.3f;
                float maxFallSpeed = reduceFallSpeed ? 12f : turboEnrage ? (Main.getGoodWorld ? 40f : 30f) : enrage ? 20f : (!flag41 && !flag42) ? 15f : 10f;

                npc.velocity.Y += gravity;
                if (npc.velocity.Y > maxFallSpeed)
                    npc.velocity.Y = maxFallSpeed;
            }

            // Despawn
            int num649 = turboEnrage ? 7500 : enrage ? 6000 : 4500;
            if (Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) + Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) > num649)
            {
                npc.TargetClosest();

                if (Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) + Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) > num649)
                {
                    npc.active = false;
                    npc.netUpdate = true;
                }
            }

            return false;
        }

        public static bool BuffedGolemFistAI(NPC npc, Mod mod)
        {
            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Enrage if the target isn't inside the temple
            // Turbo enrage if target isn't inside the temple and it's Boss Rush or For the Worthy
            bool enrage = true;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int num = (int)Main.player[npc.target].Center.X / 16;
                int num2 = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(num, num2);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || Main.getGoodWorld;
            }
            else
                turboEnrage = bossRush || Main.getGoodWorld;

            if (bossRush || Main.getGoodWorld)
                enrage = true;

            float aggression = turboEnrage ? (Main.getGoodWorld ? 4f : 3f) : enrage ? 2f : death ? 1.5f : 1f;

            if (NPC.golemBoss < 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.StrikeInstantKill();

                return false;
            }

            if (npc.alpha > 0)
            {
                npc.alpha -= 10;
                if (npc.alpha < 0)
                    npc.alpha = 0;

                npc.ai[1] = 0f;
            }

            if (npc.ai[0] == 0f)
            {
                npc.noTileCollide = true;

                float num723 = 25f;
                num723 *= (aggression + 3f) / 4f;

                Vector2 vector80 = new Vector2(npc.Center.X, npc.Center.Y);
                float num724 = Main.npc[NPC.golemBoss].Center.X - vector80.X;
                float num725 = Main.npc[NPC.golemBoss].Center.Y - vector80.Y;
                num725 -= 9f;
                num724 = (npc.type != NPCID.GolemFistLeft) ? (num724 + 78f) : (num724 - 84f);
                if (Main.getGoodWorld)
                    num724 = (npc.type != NPCID.GolemFistLeft) ? (num724 - 40f) : (num724 + 40f);

                float num726 = (float)Math.Sqrt(num724 * num724 + num725 * num725);
                if (num726 < 12f + num723)
                {
                    npc.rotation = 0f;
                    npc.velocity.X = num724;
                    npc.velocity.Y = num725;

                    float num727 = aggression;
                    npc.ai[1] += num727;
                    if (npc.life < npc.lifeMax / 2)
                        npc.ai[1] += num727;
                    if (npc.life < npc.lifeMax / 4)
                        npc.ai[1] += num727;

                    if (npc.ai[1] >= 40f)
                    {
                        npc.TargetClosest();

                        if ((npc.type == NPCID.GolemFistLeft && npc.Center.X + 100f > Main.player[npc.target].Center.X) || (npc.type == NPCID.GolemFistRight && npc.Center.X - 100f < Main.player[npc.target].Center.X))
                        {
                            npc.ai[1] = 0f;
                            npc.ai[0] = 1f;
                        }
                        else
                            npc.ai[1] = 0f;
                    }
                }
                else
                {
                    num726 = num723 / num726;
                    npc.velocity.X = num724 * num726;
                    npc.velocity.Y = num725 * num726;

                    npc.rotation = (float)Math.Atan2(0f - npc.velocity.Y, 0f - npc.velocity.X);
                    if (npc.type == NPCID.GolemFistLeft)
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X);
                }
            }
            else if (npc.ai[0] == 1f)
            {
                Vector2 vector80 = new Vector2(npc.Center.X, npc.Center.Y);
                float num724 = Main.npc[NPC.golemBoss].Center.X - vector80.X;
                float num725 = Main.npc[NPC.golemBoss].Center.Y - vector80.Y;
                num725 -= 9f;
                num724 = (npc.type != NPCID.GolemFistLeft) ? (num724 + 78f) : (num724 - 84f);
                if (Main.getGoodWorld)
                    num724 = (npc.type != NPCID.GolemFistLeft) ? (num724 - 40f) : (num724 + 40f);

                npc.ai[1] += 1f;
                npc.Center = new Vector2(num724, num725);
                npc.rotation = 0f;
                npc.velocity = Vector2.Zero;
                if (npc.ai[1] <= 15f)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        Vector2 vector3 = Main.rand.NextVector2Circular(80f, 80f);
                        Vector2 vector4 = vector3 * -1f * 0.05f;
                        Vector2 vector5 = Main.rand.NextVector2Circular(20f, 20f);
                        Dust dust = Dust.NewDustPerfect(npc.Center + vector4 + vector3 + vector5, 228, vector4);
                        dust.fadeIn = 1.5f;
                        dust.scale = 0.5f;
                        if (Main.getGoodWorld)
                            dust.noLight = true;

                        dust.noGravity = true;
                    }
                }

                if (npc.ai[1] >= 30f)
                {
                    npc.noTileCollide = true;
                    npc.collideX = false;
                    npc.collideY = false;

                    float num728 = 20f;
                    num728 *= (aggression + 3f) / 4f;
                    if (num728 > 48f)
                        num728 = 48f;

                    Vector2 vector81 = new Vector2(npc.Center.X, npc.Center.Y);
                    float num729 = Main.player[npc.target].Center.X - vector81.X;
                    float num730 = Main.player[npc.target].Center.Y - vector81.Y;
                    float num731 = (float)Math.Sqrt(num729 * num729 + num730 * num730);
                    num731 = num728 / num731;
                    npc.velocity.X = num729 * num731;
                    npc.velocity.Y = num730 * num731;
                    npc.ai[0] = 2f;
                    npc.ai[1] = 0f;

                    npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X);
                    if (npc.type == NPCID.GolemFistLeft)
                        npc.rotation = (float)Math.Atan2(0f - npc.velocity.Y, 0f - npc.velocity.X);
                }
            }
            else if (npc.ai[0] == 2f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && Main.getGoodWorld)
                {
                    for (int j = (int)(npc.position.X / 16f) - 1; (float)j < (npc.position.X + (float)npc.width) / 16f + 1f; j++)
                    {
                        for (int k = (int)(npc.position.Y / 16f) - 1; (float)k < (npc.position.Y + (float)npc.width) / 16f + 1f; k++)
                        {
                            if (Main.tile[j, k].TileType == TileID.Torches)
                            {
                                Main.tile[j, k].Get<TileWallWireStateData>().HasTile = false;
                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendTileSquare(-1, j, k);
                            }
                        }
                    }
                }

                npc.ai[1] += 1f;
                if (npc.ai[1] == 1f)
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);

                if (Main.rand.NextBool())
                {
                    Vector2 vector7 = npc.velocity * 0.5f;
                    Vector2 vector8 = Main.rand.NextVector2Circular(20f, 20f);
                    Dust.NewDustPerfect(npc.Center + vector7 + vector8, 306, vector7, 0, Main.OurFavoriteColor).scale = 2f;
                }

                if (Math.Abs(npc.velocity.X) > Math.Abs(npc.velocity.Y))
                {
                    if (npc.velocity.X > 0f && npc.Center.X > Main.player[npc.target].Center.X)
                        npc.noTileCollide = false;

                    if (npc.velocity.X < 0f && npc.Center.X < Main.player[npc.target].Center.X)
                        npc.noTileCollide = false;
                }
                else
                {
                    if (npc.velocity.Y > 0f && npc.Center.Y > Main.player[npc.target].Center.Y)
                        npc.noTileCollide = false;

                    if (npc.velocity.Y < 0f && npc.Center.Y < Main.player[npc.target].Center.Y)
                        npc.noTileCollide = false;
                }

                Vector2 vector82 = new Vector2(npc.Center.X, npc.Center.Y);
                float num732 = Main.npc[NPC.golemBoss].Center.X - vector82.X;
                float num733 = Main.npc[NPC.golemBoss].Center.Y - vector82.Y;
                num732 += Main.npc[NPC.golemBoss].velocity.X;
                num733 += Main.npc[NPC.golemBoss].velocity.Y;
                num733 -= 9f;
                num732 = (npc.type != NPCID.GolemFistLeft) ? (num732 + 78f) : (num732 - 84f);
                float num734 = (float)Math.Sqrt(num732 * num732 + num733 * num733);

                if (npc.life < npc.lifeMax / 4)
                {
                    npc.knockBackResist = 0f;

                    if (num734 > 700f || npc.collideX || npc.collideY)
                    {
                        npc.noTileCollide = true;
                        npc.ai[0] = 0f;
                    }

                    return false;
                }

                bool flag41 = npc.justHit;
                if (flag41)
                {
                    if (npc.life < npc.lifeMax / 2)
                    {
                        if (npc.knockBackResist == 0f)
                            flag41 = false;

                        npc.knockBackResist = 0f;
                    }
                }

                if ((num734 > 600f || npc.collideX || npc.collideY) | flag41)
                {
                    npc.noTileCollide = true;
                    npc.ai[0] = 0f;
                }
            }
            else
            {
                if (npc.ai[0] != 3f)
                    return false;

                npc.noTileCollide = true;
                float num736 = 12f;
                float num737 = 0.4f;
                Vector2 vector83 = new Vector2(npc.Center.X, npc.Center.Y);
                float num738 = Main.player[npc.target].Center.X - vector83.X;
                float num739 = Main.player[npc.target].Center.Y - vector83.Y;
                float num740 = (float)Math.Sqrt(num738 * num738 + num739 * num739);
                num740 = num736 / num740;
                num738 *= num740;
                num739 *= num740;

                if (npc.velocity.X < num738)
                {
                    npc.velocity.X += num737;
                    if (npc.velocity.X < 0f && num738 > 0f)
                        npc.velocity.X += num737 * 2f;
                }
                else if (npc.velocity.X > num738)
                {
                    npc.velocity.X -= num737;
                    if (npc.velocity.X > 0f && num738 < 0f)
                        npc.velocity.X -= num737 * 2f;
                }

                if (npc.velocity.Y < num739)
                {
                    npc.velocity.Y += num737;
                    if (npc.velocity.Y < 0f && num739 > 0f)
                        npc.velocity.Y += num737 * 2f;
                }
                else if (npc.velocity.Y > num739)
                {
                    npc.velocity.Y -= num737;
                    if (npc.velocity.Y > 0f && num739 < 0f)
                        npc.velocity.Y -= num737 * 2f;
                }

                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X);
                if (npc.type == NPCID.GolemFistLeft)
                    npc.rotation = (float)Math.Atan2(0f - npc.velocity.Y, 0f - npc.velocity.X);
            }

            return false;
        }


        public static bool BuffedGolemHeadAI(NPC npc, Mod mod)
        {
            // Don't collide
            npc.noTileCollide = true;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            // Die if body is gone
            if (NPC.golemBoss < 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.StrikeInstantKill();

                return false;
            }

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Count body parts
            bool flag41 = NPC.AnyNPCs(NPCID.GolemFistLeft);
            bool flag42 = NPC.AnyNPCs(NPCID.GolemFistRight);
            npc.dontTakeDamage = (flag41 || flag42) && !CalamityWorld.LegendaryMode;

            // Stay in position on top of body
            npc.Center = Main.npc[NPC.golemBoss].Center - new Vector2(Main.getGoodWorld ? 2f : 3f, Main.getGoodWorld ? 37f : 57f);

            // Enrage if the target isn't inside the temple
            bool enrage = true;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int num = (int)Main.player[npc.target].Center.X / 16;
                int num2 = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(num, num2);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || Main.getGoodWorld;
            }
            else
                turboEnrage = bossRush || Main.getGoodWorld;

            if (bossRush || Main.getGoodWorld)
                enrage = true;

            // Alpha
            if (npc.alpha > 0)
            {
                npc.alpha -= 10;
                if (npc.alpha < 0)
                    npc.alpha = 0;

                npc.ai[1] = 30f;
            }

            // Spit fireballs if arms are alive
            if (npc.ai[0] == 0f)
            {
                npc.ai[1] += 1f;
                int num654 = 150;
                if (npc.ai[1] < 20f || npc.ai[1] > (num654 - 20))
                    npc.localAI[0] = 1f;
                else
                    npc.localAI[0] = 0f;

                if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] >= num654)
                {
                    npc.TargetClosest();

                    npc.ai[1] = 0f;

                    Vector2 vector81 = new Vector2(npc.Center.X, npc.Center.Y + 10f);
                    float num655 = turboEnrage ? 12f : enrage ? 10f : 8f;
                    float num656 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector81.X;
                    float num657 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector81.Y;
                    float num658 = (float)Math.Sqrt(num656 * num656 + num657 * num657);

                    num658 = num655 / num658;
                    num656 *= num658;
                    num657 *= num658;

                    int type = ProjectileID.Fireball;
                    int damage = npc.GetProjectileDamage(type);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), vector81.X, vector81.Y, num656, num657, type, damage, 0f, Main.myPlayer, 0f, 0f);

                    npc.netUpdate = true;
                }
            }

            // Shoot lasers and fireballs if arms are dead
            else if (npc.ai[0] == 1f)
            {
                // Fire projectiles from eye positions
                Vector2 vector82 = new Vector2(npc.Center.X, npc.Center.Y + 10f);
                if (Main.player[npc.target].Center.X < npc.Center.X - npc.width)
                {
                    npc.localAI[1] = -1f;
                    vector82.X -= 40f;
                }
                else if (Main.player[npc.target].Center.X > npc.Center.X + npc.width)
                {
                    npc.localAI[1] = 1f;
                    vector82.X += 40f;
                }
                else
                    npc.localAI[1] = 0f;

                // Fireballs
                float shootBoost = death ? 3f * (1f - lifeRatio) : 2f * (1f - lifeRatio);
                npc.ai[1] += 1f + shootBoost;

                int num662 = 240;
                if (npc.ai[1] < 20f || npc.ai[1] > (num662 - 20))
                    npc.localAI[0] = 1f;
                else
                    npc.localAI[0] = 0f;

                if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] >= num662)
                {
                    npc.TargetClosest();

                    npc.ai[1] = 0f;

                    float num663 = turboEnrage ? 16f : enrage ? 14f : 12f;
                    float num664 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector82.X;
                    float num665 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector82.Y;
                    float num666 = (float)Math.Sqrt(num664 * num664 + num665 * num665);

                    num666 = num663 / num666;
                    num664 *= num666;
                    num665 *= num666;

                    int type = ProjectileID.Fireball;
                    int damage = npc.GetProjectileDamage(type);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), vector82.X, vector82.Y, num664, num665, type, damage, 0f, Main.myPlayer, 0f, 0f);

                    npc.netUpdate = true;
                }

                // Lasers
                float shootBoost2 = death ? 5f * (1f - lifeRatio) : 3f * (1f - lifeRatio);
                npc.ai[2] += 1f + shootBoost2;
                if (enrage)
                    npc.ai[2] += 4f;
                if (!Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                    npc.ai[2] += 8f;

                if (npc.ai[2] >= 300f)
                {
                    npc.TargetClosest();

                    npc.ai[2] = 0f;

                    int projType = ProjectileID.EyeBeam;
                    int dmg = npc.GetProjectileDamage(projType);

                    if (npc.localAI[1] == 0f)
                    {
                        for (int num672 = 0; num672 < 2; num672++)
                        {
                            vector82 = new Vector2(npc.Center.X, npc.Center.Y - 22f);
                            if (num672 == 0)
                                vector82.X -= 18f;
                            else
                                vector82.X += 18f;

                            float num673 = 9f;
                            if (!Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                                num673 = 14f;

                            float num674 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector82.X;
                            float num675 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector82.Y;
                            float num676 = (float)Math.Sqrt(num674 * num674 + num675 * num675);

                            num676 = num673 / num676;
                            num674 *= num676;
                            num675 *= num676;
                            vector82.X += num674 * 3f;
                            vector82.Y += num675 * 3f;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int num677 = Projectile.NewProjectile(npc.GetSource_FromAI(), vector82.X, vector82.Y, num674, num675, projType, dmg, 0f, Main.myPlayer, 0f, 0f);
                                Main.projectile[num677].timeLeft = enrage ? 480 : 300;
                                if (turboEnrage && Main.getGoodWorld)
                                    Main.projectile[num677].extraUpdates += 1;

                                npc.netUpdate = true;
                            }
                        }
                    }
                    else if (npc.localAI[1] != 0f)
                    {
                        vector82 = new Vector2(npc.Center.X, npc.Center.Y - 22f);
                        if (npc.localAI[1] == -1f)
                            vector82.X -= 30f;
                        else if (npc.localAI[1] == 1f)
                            vector82.X += 30f;

                        float num678 = 9f;
                        if (!Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                            num678 = 14f;

                        float num679 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector82.X;
                        float num680 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector82.Y;
                        float num681 = (float)Math.Sqrt(num679 * num679 + num680 * num680);

                        num681 = num678 / num681;
                        num679 *= num681;
                        num680 *= num681;
                        vector82.X += num679 * 3f;
                        vector82.Y += num680 * 3f;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int num682 = Projectile.NewProjectile(npc.GetSource_FromAI(), vector82.X, vector82.Y, num679, num680, projType, dmg, 0f, Main.myPlayer, 0f, 0f);
                            Main.projectile[num682].timeLeft = enrage ? 480 : 300;
                            if (turboEnrage && Main.getGoodWorld)
                                Main.projectile[num682].extraUpdates += 1;

                            npc.netUpdate = true;
                        }
                    }
                }
            }

            // Laser fire if arms are dead
            if ((!flag41 && !flag42) || death || CalamityWorld.LegendaryMode)
            {
                npc.ai[0] = 1f;
                return false;
            }
            npc.ai[0] = 0f;

            return false;
        }

        public static bool BuffedGolemHeadFreeAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            // Die if body is gone
            if (NPC.golemBoss < 0)
            {
                calamityGlobalNPC.DR = 0.25f;
                calamityGlobalNPC.unbreakableDR = false;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = false;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.StrikeInstantKill();

                return false;
            }

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;
            float golemLifeRatio = Main.npc[NPC.golemBoss].life / (float)Main.npc[NPC.golemBoss].lifeMax;

            // Phases
            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;
            bool phase2 = lifeRatio < 0.7f || golemLifeRatio < 0.85f;
            bool phase3 = lifeRatio < 0.55f || golemLifeRatio < 0.7f;
            bool phase4 = lifeRatio < 0.4f || golemLifeRatio < 0.55f;

            // Enrage if the target isn't inside the temple
            bool enrage = true;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int num = (int)Main.player[npc.target].Center.X / 16;
                int num2 = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(num, num2);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || Main.getGoodWorld;
            }
            else
                turboEnrage = bossRush || Main.getGoodWorld;

            if (bossRush || Main.getGoodWorld)
                enrage = true;

            if (turboEnrage)
            {
                calamityGlobalNPC.DR = 0.9999f;
                calamityGlobalNPC.unbreakableDR = true;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = true;
            }

            // Float through tiles or not
            bool flag44 = false;
            if (!Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) || phase3 || turboEnrage)
            {
                npc.noTileCollide = true;
                flag44 = true;
            }
            else
                npc.noTileCollide = false;

            // Move to new location
            if (npc.ai[3] <= 0f)
            {
                npc.ai[3] = 300f;

                float maxDistance = 300f;

                // Four corners around target
                if (phase3 || turboEnrage)
                {
                    if (calamityGlobalNPC.newAI[1] == -maxDistance)
                    {
                        switch ((int)calamityGlobalNPC.newAI[0])
                        {
                            case 0:
                            case 300:
                                calamityGlobalNPC.newAI[0] = -maxDistance;
                                break;
                            case -300:
                                calamityGlobalNPC.newAI[1] = maxDistance;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch ((int)calamityGlobalNPC.newAI[0])
                        {
                            case 0:
                            case -300:
                                calamityGlobalNPC.newAI[0] = maxDistance;
                                break;
                            case 300:
                                calamityGlobalNPC.newAI[1] = -maxDistance;
                                break;
                            default:
                                break;
                        }
                    }
                }

                // Above target
                else if (phase2)
                {
                    switch ((int)calamityGlobalNPC.newAI[0])
                    {
                        case 0:
                            calamityGlobalNPC.newAI[0] = maxDistance;
                            break;
                        case 300:
                            calamityGlobalNPC.newAI[0] = -maxDistance;
                            break;
                        case -300:
                            calamityGlobalNPC.newAI[0] = 0f;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    calamityGlobalNPC.newAI[0] = 0f;
                    calamityGlobalNPC.newAI[1] = -maxDistance;
                }

                npc.netSpam = 5;
                npc.SyncExtraAI();
                npc.netUpdate = true;
            }

            npc.ai[3] -= 1f +
                ((phase2 || turboEnrage) ? 1f : 0f) +
                ((phase3 || turboEnrage) ? 1f : 0f) +
                ((phase4 || turboEnrage) ? 2f : 0f);

            float offsetX = calamityGlobalNPC.newAI[0];
            float offsetY = calamityGlobalNPC.newAI[1];
            Vector2 destination = Main.player[npc.target].Center + new Vector2(offsetX, offsetY);

            // Velocity and acceleration
            float velocity = 7f +
                ((phase2 || turboEnrage) ? 4f : 0f) +
                ((phase3 || turboEnrage) ? 4f : 0f);

            if (enrage)
                velocity = (phase3 || turboEnrage) ? 25f : 20f;

            float acceleration = enrage ? 0.4f : phase3 ? 0.2f : phase2 ? 0.15f : 0.1f;

            // How far Ceaseless Void is from where it's supposed to be
            Vector2 distanceFromDestination = destination - npc.Center;

            CalamityUtils.SmoothMovement(npc, 0f, distanceFromDestination, velocity, acceleration, true);

            if (death && calamityGlobalNPC.newAI[2] < 120f)
            {
                calamityGlobalNPC.newAI[2] += 1f;

                if (calamityGlobalNPC.newAI[2] % 15f == 0f)
                {
                    npc.netUpdate = true;
                    npc.SyncExtraAI();
                }

                return false;
            }

            // Fireballs
            float shootBoost = death ? 3f * (2f - (lifeRatio + golemLifeRatio)) : 2f * (2f - (lifeRatio + golemLifeRatio));
            npc.ai[1] += 1f + shootBoost;

            int num705 = 360;
            if (npc.ai[1] < 20f || npc.ai[1] > (num705 - 20))
                npc.localAI[0] = 1f;
            else
                npc.localAI[0] = 0f;

            if (flag44 && !phase3)
                npc.ai[1] = 20f;

            if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] >= num705 && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f)
            {
                npc.TargetClosest();

                npc.ai[1] = 0f;

                Vector2 vector88 = new Vector2(npc.Center.X, npc.Center.Y - 10f);
                float num706 = turboEnrage ? 8f : enrage ? 6.5f : 5f;
                float num709 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector88.X;
                float num710 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector88.Y;
                float num711 = (float)Math.Sqrt(num709 * num709 + num710 * num710);

                num711 = num706 / num711;
                num709 *= num711;
                num710 *= num711;

                int projectileType = phase3 ? ProjectileID.InfernoHostileBolt : ProjectileID.Fireball;
                int damage = npc.GetProjectileDamage(projectileType);
                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), vector88.X, vector88.Y, num709, num710, projectileType, damage, 0f, Main.myPlayer, 0f, 0f);
                if (projectileType == ProjectileID.InfernoHostileBolt)
                {
                    Main.projectile[proj].timeLeft = 300;
                    Main.projectile[proj].ai[0] = Main.player[npc.target].Center.X;
                    Main.projectile[proj].ai[1] = Main.player[npc.target].Center.Y;
                    Main.projectile[proj].netUpdate = true;
                }

                npc.netUpdate = true;
            }

            // Lasers
            npc.ai[2] += 1f + shootBoost;
            if (!Collision.CanHit(Main.npc[NPC.golemBoss].Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                npc.ai[2] += 8f;

            if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[2] >= 300f && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f)
            {
                npc.TargetClosest();

                npc.ai[2] = 0f;

                for (int num713 = 0; num713 < 2; num713++)
                {
                    Vector2 vector89 = new Vector2(npc.Center.X, npc.Center.Y - 50f);
                    if (Main.getGoodWorld)
                        vector89.Y += 30f;
                    if (num713 == 0)
                        vector89.X -= 14f;
                    else if (num713 == 1)
                        vector89.X += 14f;

                    float num714 = 5f + shootBoost;
                    float num717 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector89.X;
                    float num718 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector89.Y;
                    float num719 = (float)Math.Sqrt(num717 * num717 + num718 * num718);

                    num719 = num714 / num719;
                    num717 *= num719;
                    num718 *= num719;
                    vector89.X += num717 * 3f;
                    vector89.Y += num718 * 3f;

                    int type = ProjectileID.EyeBeam;
                    int damage = npc.GetProjectileDamage(type);
                    int num720 = Projectile.NewProjectile(npc.GetSource_FromAI(), vector89.X, vector89.Y, num717, num718, type, damage, 0f, Main.myPlayer, 0f, 0f);
                    Main.projectile[num720].timeLeft = enrage ? 480 : 300;
                    if (turboEnrage && Main.getGoodWorld)
                        Main.projectile[num720].extraUpdates += 1;
                }
            }

            if (!Main.getGoodWorld)
            {
                npc.position += npc.netOffset;
                int num765 = Main.rand.Next(2) * 2 - 1;
                Vector2 vector95 = npc.Bottom + new Vector2((float)(num765 * 22) * npc.scale, -22f * npc.scale);
                Dust dust5 = Dust.NewDustPerfect(vector95, 228, ((float)Math.PI / 2f + -(float)Math.PI / 2f * (float)num765 + Main.rand.NextFloatDirection() * ((float)Math.PI / 4f)).ToRotationVector2() * (2f + Main.rand.NextFloat()));
                Dust dust = dust5;
                dust.velocity += npc.velocity;
                dust5.noGravity = true;
                dust5 = Dust.NewDustPerfect(npc.Bottom + new Vector2(Main.rand.NextFloatDirection() * 6f * npc.scale, (Main.rand.NextFloat() * -4f - 8f) * npc.scale), 228, Vector2.UnitY * (2f + Main.rand.NextFloat()));
                dust5.fadeIn = 0f;
                dust5.scale = 0.7f + Main.rand.NextFloat() * 0.5f;
                dust5.noGravity = true;
                dust = dust5;
                dust.velocity += npc.velocity;
                npc.position -= npc.netOffset;
            }

            return false;
        }
    }
}
