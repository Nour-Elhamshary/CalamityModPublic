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
    public static class WallOfFleshAI
    {
        public static bool BuffedWallofFleshAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Despawn
            if (npc.position.X < 160f || npc.position.X > ((Main.maxTilesX - 10) * 16))
                npc.active = false;

            // Set Wall of Flesh variables
            if (npc.localAI[0] == 0f)
            {
                npc.localAI[0] = 1f;
                Main.wofDrawAreaBottom = -1;
                Main.wofDrawAreaTop = -1;
            }

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Clamp life ratio to prevent bad velocity math.
            lifeRatio = MathHelper.Clamp(lifeRatio, 0f, 1f);

            // Phases based on HP
            bool phase2 = lifeRatio < 0.66f;
            bool phase3 = lifeRatio < 0.33f;

            if (Main.getGoodWorld && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(180))
            {
                if (NPC.CountNPCS(NPCID.FireImp) < 4)
                {
                    int num351 = 1000;
                    for (int num352 = 0; num352 < num351; num352++)
                    {
                        int num353 = (int)(npc.Center.X / 16f);
                        int num354 = (int)(npc.Center.Y / 16f);
                        if (npc.target >= 0)
                        {
                            num353 = (int)(Main.player[npc.target].Center.X / 16f);
                            num354 = (int)(Main.player[npc.target].Center.Y / 16f);
                        }

                        num353 += Main.rand.Next(-50, 51);
                        for (num354 += Main.rand.Next(-50, 51); num354 < Main.maxTilesY - 10 && !WorldGen.SolidTile(num353, num354); num354++)
                        {
                        }

                        num354--;
                        if (!WorldGen.SolidTile(num353, num354))
                        {
                            int num355 = NPC.NewNPC(npc.GetSource_FromAI(), num353 * 16 + 8, num354 * 16, 24);
                            if (Main.netMode == NetmodeID.Server && num355 < Main.maxNPCs)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num355);

                            break;
                        }
                    }
                }
            }

            // Start leech spawning based on HP
            npc.ai[1] += 1f;
            if (npc.ai[2] == 0f)
            {
                if (phase2)
                    npc.ai[1] += 1f;
                if (phase3)
                    npc.ai[1] += 1f;
                if (bossRush)
                    npc.ai[1] += 3f;
                if (CalamityWorld.LegendaryMode)
                    npc.ai[1] += 9f;

                if (npc.ai[1] > 2700f)
                    npc.ai[2] = 1f;
            }

            // Leech spawn
            if (npc.ai[2] > 0f && npc.ai[1] > 60f)
            {
                int num330 = phase3 ? 3 : 2;

                npc.ai[2] += 1f;
                npc.ai[1] = 0f;
                if (npc.ai[2] > num330)
                    npc.ai[2] = 0f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int num331 = NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.position.X + (npc.width / 2)), (int)(npc.position.Y + (npc.height / 2) + 20f), NPCID.LeechHead, 1);
                    Main.npc[num331].velocity.X = npc.direction * 9;

                    if (phase2)
                    {
                        // Get target vector
                        Vector2 projectileVelocity = Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * npc.velocity.Length();
                        Vector2 projectileSpawn = npc.Center + projectileVelocity * 5f;

                        int damage = npc.GetProjectileDamage(ProjectileID.DemonSickle);
                        int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileSpawn, projectileVelocity, ProjectileID.DemonSickle, damage, 0f, Main.myPlayer, 0f, projectileVelocity.Length() * 3f);
                        Main.projectile[proj].timeLeft = 600;
                        Main.projectile[proj].tileCollide = false;
                    }
                }
            }

            // Play sound
            npc.localAI[3] += 1f;
            if (npc.localAI[3] >= (600 + Main.rand.Next(1000)))
            {
                npc.localAI[3] = -Main.rand.Next(200);
                SoundEngine.PlaySound(SoundID.NPCDeath10, npc.position);
            }

            // Set whoAmI variable
            Main.wofNPCIndex = npc.whoAmI;

            // Set eye positions
            int num332 = (int)(npc.position.X / 16f);
            int num333 = (int)((npc.position.X + npc.width) / 16f);
            int num334 = (int)((npc.position.Y + (npc.height / 2)) / 16f);
            int num335 = 0;
            int num336 = num334 + 7;
            while (num335 < 15 && num336 > Main.UnderworldLayer)
            {
                num336++;
                for (int num337 = num332; num337 <= num333; num337++)
                {
                    try
                    {
                        if (WorldGen.SolidTile(num337, num336) || Main.tile[num337, num336].LiquidAmount > 0)
                            num335++;
                    }
                    catch
                    { num335 += 15; }
                }
            }
            num336 += 4;
            if (Main.wofDrawAreaBottom == -1)
                Main.wofDrawAreaBottom = num336 * 16;
            else if (Main.wofDrawAreaBottom > num336 * 16)
            {
                Main.wofDrawAreaBottom--;
                if (Main.wofDrawAreaBottom < num336 * 16)
                    Main.wofDrawAreaBottom = num336 * 16;
            }
            else if (Main.wofDrawAreaBottom < num336 * 16)
            {
                Main.wofDrawAreaBottom++;
                if (Main.wofDrawAreaBottom > num336 * 16)
                    Main.wofDrawAreaBottom = num336 * 16;
            }

            num335 = 0;
            num336 = num334 - 7;
            while (num335 < 15 && num336 < Main.maxTilesY - 10)
            {
                num336--;
                for (int num338 = num332; num338 <= num333; num338++)
                {
                    try
                    {
                        if (WorldGen.SolidTile(num338, num336) || Main.tile[num338, num336].LiquidAmount > 0)
                            num335++;
                    }
                    catch
                    { num335 += 15; }
                }
            }
            num336 -= 4;
            if (Main.wofDrawAreaTop == -1)
                Main.wofDrawAreaTop = num336 * 16;
            else if (Main.wofDrawAreaTop > num336 * 16)
            {
                Main.wofDrawAreaTop--;
                if (Main.wofDrawAreaTop < num336 * 16)
                    Main.wofDrawAreaTop = num336 * 16;
            }
            else if (Main.wofDrawAreaTop < num336 * 16)
            {
                Main.wofDrawAreaTop++;
                if (Main.wofDrawAreaTop > num336 * 16)
                    Main.wofDrawAreaTop = num336 * 16;
            }

            // Set Y position
            float num339 = (Main.wofDrawAreaBottom + Main.wofDrawAreaTop) / 2 - npc.height / 2;
            int num340 = (Main.maxTilesY - 180) * 16;
            if (num339 < num340)
                num339 = num340;
            npc.position.Y = num339;

            float targetPosition = Main.player[npc.target].position.X + (Main.player[npc.target].width / 2);
            float npcPosition = npc.position.X + (npc.width / 2);

            // Speed up if target is too far or if they're hiding behind tiles, slow down if too close
            float distanceFromTarget;
            if (npc.velocity.X < 0f)
                distanceFromTarget = npcPosition - targetPosition;
            else
                distanceFromTarget = targetPosition - npcPosition;

            float halfAverageScreenWidth = 960f;
            float distanceBeforeSlowingDown = 400f;
            float timeBeforeEnrage = 600f - (death ? 390f * (1f - lifeRatio) : 0f);
            float speedMult = 1f;

            if (bossRush)
                timeBeforeEnrage *= 0.25f;

            if (calamityGlobalNPC.newAI[0] < timeBeforeEnrage)
            {
                if (distanceFromTarget > halfAverageScreenWidth ||
                    !Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    speedMult += (distanceFromTarget - halfAverageScreenWidth) * 0.001f;
                    calamityGlobalNPC.newAI[0] += 1f;

                    // Enrage after 10 seconds of target being off screen
                    if (calamityGlobalNPC.newAI[0] >= timeBeforeEnrage)
                    {
                        calamityGlobalNPC.newAI[1] = 1f;

                        // Tell eyes to fire different lasers
                        npc.ai[3] = 1f;

                        // Play roar sound on players nearby
                        if (Main.player[Main.myPlayer].active && !Main.player[Main.myPlayer].dead && Vector2.Distance(Main.player[Main.myPlayer].Center, npc.Center) < 2800f)
                            SoundEngine.PlaySound(SoundID.NPCDeath10 with { Pitch = SoundID.NPCDeath10.Pitch - 0.25f}, Main.player[Main.myPlayer].position);
                    }
                }
                else if (distanceFromTarget < distanceBeforeSlowingDown)
                    speedMult += (distanceFromTarget - distanceBeforeSlowingDown) * 0.002f;

                if (distanceFromTarget < halfAverageScreenWidth &&
                    Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    if (calamityGlobalNPC.newAI[0] > 0f)
                        calamityGlobalNPC.newAI[0] -= 1f;
                }

                speedMult = MathHelper.Clamp(speedMult, 0.75f, 2f);
            }

            // Enrage if target is off screen for too long
            if (calamityGlobalNPC.newAI[1] == 1f)
            {
                // Triple speed
                speedMult = 3.25f;

                // Return to normal if very close to target
                if (distanceFromTarget < distanceBeforeSlowingDown)
                {
                    calamityGlobalNPC.newAI[0] = 0f;
                    calamityGlobalNPC.newAI[1] = 0f;
                    npc.ai[3] = 0f;
                }
            }

            if (bossRush)
                speedMult += 0.2f;

            // NOTE: Max velocity is 8 in Expert Mode
            // NOTE: Max velocity is 9 in For The Worthy

            float velocityBoost = 4f * (1f - lifeRatio);
            float velocityX = (bossRush ? 7f : death ? 3.5f : 2f) + velocityBoost;
            velocityX *= speedMult;

            if (Main.getGoodWorld)
            {
                velocityX *= 1.1f;
                velocityX += 0.2f;
            }

            // NOTE: Values below are based on Rev Mode only!
            // Max velocity without enrage is 12
            // Min velocity is 1.5
            // Max velocity with enrage is 18

            // Set X velocity
            if (npc.velocity.X == 0f)
            {
                npc.TargetClosest();
                if (Main.player[npc.target].dead)
                {
                    float num359 = float.PositiveInfinity;
                    int direction2 = 0;
                    for (int num360 = 0; num360 < Main.maxPlayers; num360++)
                    {
                        Player player = Main.player[npc.target];
                        if (player.active)
                        {
                            float num361 = npc.Distance(player.Center);
                            if (num359 > num361)
                            {
                                num359 = num361;
                                direction2 = (npc.Center.X < player.Center.X) ? 1 : -1;
                            }
                        }
                    }

                    npc.direction = direction2;
                }

                npc.velocity.X = npc.direction;
            }

            if (npc.velocity.X < 0f)
            {
                npc.velocity.X = -velocityX;
                npc.direction = -1;
            }
            else
            {
                npc.velocity.X = velocityX;
                npc.direction = 1;
            }

            if (Main.player[npc.target].dead || !Main.player[npc.target].gross)
                npc.TargetClosest_WOF();

            if (Main.player[npc.target].dead)
            {
                npc.localAI[1] += 0.0055555557f;
                if (npc.localAI[1] >= 1f)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath10, npc.position);
                    npc.life = 0;
                    npc.active = false;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, npc.whoAmI, -1f);

                    return false;
                }
            }
            else
            {
                npc.localAI[1] = MathHelper.Clamp(npc.localAI[1] - 1f / 30f, 0f, 1f);
            }

            // Direction
            npc.spriteDirection = npc.direction;
            Vector2 vector37 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
            float num342 = Main.player[npc.target].position.X + (Main.player[npc.target].width / 2) - vector37.X;
            float num343 = Main.player[npc.target].position.Y + (Main.player[npc.target].height / 2) - vector37.Y;
            float num344 = (float)Math.Sqrt(num342 * num342 + num343 * num343);
            num342 *= num344;
            num343 *= num344;

            // Rotation based on direction
            if (npc.direction > 0)
            {
                if (Main.player[npc.target].position.X + (Main.player[npc.target].width / 2) > npc.position.X + (npc.width / 2))
                    npc.rotation = (float)Math.Atan2(-num343, -num342) + MathHelper.Pi;
                else
                    npc.rotation = 0f;
            }
            else if (Main.player[npc.target].position.X + (Main.player[npc.target].width / 2) < npc.position.X + (npc.width / 2))
                npc.rotation = (float)Math.Atan2(num343, num342) + MathHelper.Pi;
            else
                npc.rotation = 0f;

            // Expert hungry respawn over time
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Range of 2 to 11
                float spawnBoost = death ? 1f : (float)Math.Ceiling(lifeRatio * 10f);
                int chance = (int)(1f + spawnBoost);

                // Range of 4 to 121
                chance *= chance;

                // Range of 23 to 134
                chance = (chance * 19 + 400) / 20;

                // Range of 32 to 59
                if (chance < 60)
                    chance = (chance * 3 + 60) / 4;

                // Range of 64 to 268
                chance *= 2;

                if (bossRush)
                    chance /= 4;

                if (Main.rand.NextBool(chance))
                {
                    int num346 = 0;
                    float[] array = new float[10];
                    for (int num347 = 0; num347 < Main.maxNPCs; num347++)
                    {
                        if (num346 < 10 && Main.npc[num347].active && Main.npc[num347].type == NPCID.TheHungry)
                        {
                            array[num346] = Main.npc[num347].ai[0];
                            num346++;
                        }
                    }

                    int maxValue = 1 + num346 * 2;
                    if (num346 < 10 && Main.rand.Next(maxValue) <= 1)
                    {
                        int num348 = -1;
                        for (int num349 = 0; num349 < 1000; num349++)
                        {
                            int num350 = Main.rand.Next(10);
                            float num351 = num350 * 0.1f - 0.05f;
                            bool flag29 = true;
                            for (int num352 = 0; num352 < num346; num352++)
                            {
                                if (num351 == array[num352])
                                {
                                    flag29 = false;
                                    break;
                                }
                            }
                            if (flag29)
                            {
                                num348 = num350;
                                break;
                            }
                        }
                        if (num348 >= 0)
                        {
                            int num353 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)num339, NPCID.TheHungry, npc.whoAmI);
                            Main.npc[num353].ai[0] = num348 * 0.1f - 0.05f;
                        }
                    }
                }
            }

            // Spawn eyes and hungries
            if (npc.localAI[0] == 1f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.localAI[0] = 2f;

                num339 = (Main.wofDrawAreaBottom + Main.wofDrawAreaTop) / 2;
                num339 = (num339 + Main.wofDrawAreaTop) / 2f;
                int num354 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)num339, NPCID.WallofFleshEye, npc.whoAmI, 0f, 0f, 0f, 0f, 255);
                Main.npc[num354].ai[0] = 1f;

                num339 = (Main.wofDrawAreaBottom + Main.wofDrawAreaTop) / 2;
                num339 = (num339 + Main.wofDrawAreaBottom) / 2f;
                num354 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)num339, NPCID.WallofFleshEye, npc.whoAmI, 0f, 0f, 0f, 0f, 255);
                Main.npc[num354].ai[0] = -1f;

                num339 = (Main.wofDrawAreaBottom + Main.wofDrawAreaTop) / 2;
                num339 = (num339 + Main.wofDrawAreaBottom) / 2f;

                int num;
                for (int num355 = 0; num355 < 11; num355 = num + 1)
                {
                    num354 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)num339, NPCID.TheHungry, npc.whoAmI, 0f, 0f, 0f, 0f, 255);
                    Main.npc[num354].ai[0] = num355 * 0.1f - 0.05f;
                    num = num355;
                }
            }

            return false;
        }

        public static bool BuffedWallofFleshEyeAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Despawn
            if (Main.wofNPCIndex < 0)
            {
                npc.active = false;
                return false;
            }

            npc.realLife = Main.wofNPCIndex;

            if (Main.npc[Main.wofNPCIndex].life > 0)
                npc.life = Main.npc[Main.wofNPCIndex].life;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Velocity, direction, and position
            npc.position.X = Main.npc[Main.wofNPCIndex].position.X;
            npc.direction = Main.npc[Main.wofNPCIndex].direction;
            npc.spriteDirection = npc.direction;

            float expectedPosition = (Main.wofDrawAreaBottom + Main.wofDrawAreaTop) / 2;
            if (npc.ai[0] > 0f)
                expectedPosition = (expectedPosition + Main.wofDrawAreaTop) / 2f;
            else
                expectedPosition = (expectedPosition + Main.wofDrawAreaBottom) / 2f;
            expectedPosition -= npc.height / 2;

            bool belowExpectedPosition = npc.position.Y > expectedPosition + 1f;
            bool aboveExpectedPosition = npc.position.Y < expectedPosition - 1f;
            if (belowExpectedPosition)
            {
                float distanceBelowExpectedPosition = npc.position.Y - expectedPosition + 1f;
                float movementVelocity = MathHelper.Clamp(distanceBelowExpectedPosition * 0.03125f, 1f, 5f);
                npc.velocity.Y = -movementVelocity;
            }
            else if (aboveExpectedPosition)
            {
                float distanceAboveExpectedPosition = expectedPosition - 1f - npc.position.Y;
                float movementVelocity = MathHelper.Clamp(distanceAboveExpectedPosition * 0.03125f, 1f, 5f);
                npc.velocity.Y = movementVelocity;
            }
            else
            {
                npc.velocity.Y = 0f;
                npc.position.Y = expectedPosition;
            }

            Vector2 vector38 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
            float num357 = Main.player[npc.target].position.X + (Main.player[npc.target].width / 2) - vector38.X;
            float num358 = Main.player[npc.target].position.Y + (Main.player[npc.target].height / 2) - vector38.Y;
            float num359 = (float)Math.Sqrt(num357 * num357 + num358 * num358);
            num357 *= num359;
            num358 *= num359;

            // Rotation based on direction and whether to fire lasers or not
            bool flag30 = true;
            if (npc.direction > 0)
            {
                if (Main.player[npc.target].position.X + (Main.player[npc.target].width / 2) > npc.position.X + (npc.width / 2))
                {
                    npc.rotation = (float)Math.Atan2(-num358, -num357) + MathHelper.Pi;
                }
                else
                {
                    npc.rotation = 0f;
                    flag30 = false;
                }
            }
            else if (Main.player[npc.target].position.X + (Main.player[npc.target].width / 2) < npc.position.X + (npc.width / 2))
            {
                npc.rotation = (float)Math.Atan2(num358, num357) + MathHelper.Pi;
            }
            else
            {
                npc.rotation = 0f;
                flag30 = false;
            }

            // Fire lasers
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Percent life remaining
                float lifeRatio = Main.npc[Main.wofNPCIndex].life / (float)Main.npc[Main.wofNPCIndex].lifeMax;

                bool charging = Main.npc[Main.wofNPCIndex].ai[3] == 1f;

                // Set up enraged laser firing timer
                float enragedLaserTimer = 300f;
                if (charging)
                    npc.localAI[3] = enragedLaserTimer;

                bool fireEnragedLasers = npc.localAI[3] > 0f && npc.localAI[3] < enragedLaserTimer;

                // Decrement the enraged laser timer
                if (npc.localAI[3] > 0f)
                {
                    npc.localAI[3] -= 1f;

                    // Stop firing normal lasers when enrage ends
                    if (npc.localAI[3] == 0f)
                        npc.localAI[1] = 0f;
                }

                float shootBoost = fireEnragedLasers ? (death ? 1.5f : 1.5f * (1f - lifeRatio)) : (death ? 3f : 3f * (1f - lifeRatio));
                npc.localAI[1] += 1f + shootBoost;

                bool canHit = Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);

                if (npc.localAI[2] == 0f)
                {
                    if (npc.localAI[1] > 400f || fireEnragedLasers)
                    {
                        npc.localAI[2] = 1f;
                        npc.localAI[1] = 0f;
                        npc.TargetClosest();
                    }
                }
                else if (npc.localAI[1] > 45f && (canHit || fireEnragedLasers) && !charging)
                {
                    npc.localAI[1] = 0f;
                    npc.localAI[2] += 1f;
                    if (npc.localAI[2] >= 4f)
                        npc.localAI[2] = 0f;

                    if (flag30)
                    {
                        bool phase2 = lifeRatio < 0.5 || bossRush;
                        float velocity = (fireEnragedLasers ? 3f : 4f) + shootBoost;

                        int projectileType = phase2 ? ProjectileID.DeathLaser : ProjectileID.EyeLaser;
                        int damage = npc.GetProjectileDamage(projectileType);

                        float laserSpawnDistance = fireEnragedLasers ? 30f : 22.5f;
                        Vector2 projectileVelocity = Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * velocity;
                        Vector2 projectileSpawn = npc.Center + projectileVelocity * laserSpawnDistance;

                        int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileSpawn, projectileVelocity, projectileType, damage, 0f, Main.myPlayer, 1f, 0f);
                        Main.projectile[proj].timeLeft = 900;

                        if (!canHit)
                            Main.projectile[proj].tileCollide = false;

                    }
                }
            }

            return false;
        }
    }
}
