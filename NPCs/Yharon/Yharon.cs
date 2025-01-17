﻿using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Wings;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Placeables.Furniture.BossRelics;
using CalamityMod.Items.Placeables.Furniture.DevPaintings;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.Potions;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Tiles.Ores;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ReLogic.Utilities;
using CalamityMod.NPCs.Bumblebirb;

namespace CalamityMod.NPCs.Yharon
{
    [AutoloadBossHead]
    public class Yharon : ModNPC
    {
        private Rectangle safeBox = default;

        private bool enraged = false;
        private bool protectionBoost = false;
        private bool moveCloser = false;
        private bool useTornado = true;
        private int secondPhasePhase = 1;
        private int teleportLocation = 0;
        private bool startSecondAI = false;
        private bool spawnArena = false;
        private int invincibilityCounter = 0;
        private int fastChargeTelegraphTime = 120;

        private const float ai2GateValue = 0.55f;
        public const int Phase2InvincibilityTime = 300;

        public static float normalDR = 0.22f;
        public static float EnragedDR = 0.9f;

        public static readonly SoundStyle RoarSound = new("CalamityMod/Sounds/Custom/Yharon/YharonRoar");
        public static readonly SoundStyle ShortRoarSound = new("CalamityMod/Sounds/Custom/Yharon/YharonRoarShort");
        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Custom/Yharon/YharonFire");
        public static readonly SoundStyle OrbSound = new("CalamityMod/Sounds/Custom/Yharon/YharonFireOrb");
        public static readonly SoundStyle HitSound = new("CalamityMod/Sounds/NPCHit/YharonHurt");
        public static readonly SoundStyle DeathSound = new("CalamityMod/Sounds/NPCKilled/YharonDeath");

        public SlotId RoarSoundSlot;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Scale = 0.3f,
                PortraitScale = 0.4f,
                PortraitPositionYOverride = -16f,
                SpriteDirection = 1
            };
            value.Position.X += 26f;
            value.Position.Y -= 14f;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.npcSlots = 50f;
            NPC.GetNPCDamage();
            NPC.width = 200;
            NPC.height = 200;
            NPC.defense = 90;
            NPC.LifeMaxNERB(1300000, 1560000, 740000);
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(10, 0, 0, 0);
            NPC.boss = true;
            NPC.DR_NERD(normalDR);

            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;

            NPC.DeathSound = DeathSound;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] 
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Yharon")
            });
        }

        public override void ModifyTypeName(ref string typeName)
        {
            if (startSecondAI)
                typeName = CalamityUtils.GetTextValue("NPCs.YharonPhase2"); // phase 2 name
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            BitsByte bb = new BitsByte();
            bb[0] = enraged;
            bb[1] = protectionBoost;
            bb[2] = moveCloser;
            bb[3] = useTornado;
            bb[4] = startSecondAI;
            bb[5] = NPC.dontTakeDamage;
            writer.Write(bb);
            writer.Write(secondPhasePhase);
            writer.Write(teleportLocation);
            writer.Write(invincibilityCounter);
            writer.Write(fastChargeTelegraphTime);
            writer.Write(safeBox.X);
            writer.Write(safeBox.Y);
            writer.Write(safeBox.Width);
            writer.Write(safeBox.Height);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            BitsByte bb = reader.ReadByte();
            enraged = bb[0];
            protectionBoost = bb[1];
            moveCloser = bb[2];
            useTornado = bb[3];
            startSecondAI = bb[4];
            NPC.dontTakeDamage = bb[5];
            secondPhasePhase = reader.ReadInt32();
            teleportLocation = reader.ReadInt32();
            invincibilityCounter = reader.ReadInt32();
            fastChargeTelegraphTime = reader.ReadInt32();
            safeBox.X = reader.ReadInt32();
            safeBox.Y = reader.ReadInt32();
            safeBox.Width = reader.ReadInt32();
            safeBox.Height = reader.ReadInt32();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
        }

        public override void AI()
        {
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            // Percent life remaining
            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            // Stop rain
            if (CalamityConfig.Instance.BossesStopWeather)
                CalamityMod.StopRain();

            // Variables
            bool bossRush = BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || bossRush;
            bool revenge = CalamityWorld.revenge || bossRush;
            bool death = CalamityWorld.death || bossRush;

            float pie = (float)Math.PI;

            CalamityGlobalNPC.yharon = NPC.whoAmI;
            CalamityGlobalNPC.yharonP2 = -1;

            // Start phase 2 or not
            if (startSecondAI)
            {
                Yharon_AI2(expertMode, revenge, death, bossRush, pie, lifeRatio, calamityGlobalNPC);
                return;
            }

            // Phase booleans
            float phase2GateValue = revenge ? 0.9f : expertMode ? 0.85f : 0.75f;
            bool phase2Check = death || lifeRatio <= phase2GateValue;
            bool phase3Check = lifeRatio <= (death ? 0.8f : revenge ? 0.75f : expertMode ? 0.7f : 0.625f);
            bool phase4Check = lifeRatio <= ai2GateValue;
            bool phase1Change = NPC.ai[0] > -1f;
            bool phase2Change = NPC.ai[0] > 5f;
            bool phase3Change = NPC.ai[0] > 12f;

            // Timer, velocity and acceleration for idle phase before phase switch
            int phaseSwitchTimer = expertMode ? 36 : 40;
            float acceleration = expertMode ? 0.75f : 0.7f;
            float velocity = expertMode ? 12f : 11f;

            if (phase3Change)
            {
                acceleration = expertMode ? 0.85f : 0.8f;
                velocity = expertMode ? 14f : 13f;
                phaseSwitchTimer = expertMode ? 25 : 28;
                fastChargeTelegraphTime = 100;
            }
            else if (phase2Change)
            {
                acceleration = expertMode ? 0.8f : 0.75f;
                velocity = expertMode ? 13f : 12f;
                phaseSwitchTimer = expertMode ? 32 : 36;
                fastChargeTelegraphTime = 110;
            }
            else
                phaseSwitchTimer = 25;

            // Timers and velocity for charging
            float reduceSpeedChargeDistance = 540f;
            int chargeTime = expertMode ? 40 : 45;
            float chargeSpeed = expertMode ? 28f : 26f;
            float fastChargeVelocityMultiplier = bossRush ? 2f : 1.5f;
            bool playFastChargeRoarSound = NPC.localAI[1] == fastChargeTelegraphTime * 0.5f;
            bool doFastCharge = NPC.localAI[1] > fastChargeTelegraphTime;

            if (phase3Change)
            {
                chargeTime = 35;
                chargeSpeed = 30f;
            }
            else if (phase2Change)
            {
                chargeTime = expertMode ? 38 : 43;

                if (expertMode)
                    chargeSpeed = 28.5f;
            }

            if (revenge)
            {
                int chargeTimeDecrease = bossRush ? 6 : death ? 4 : 2;
                float velocityMult = bossRush ? 1.15f : death ? 1.1f : 1.05f;
                phaseSwitchTimer -= chargeTimeDecrease;
                acceleration *= velocityMult;
                velocity *= velocityMult;
                chargeTime -= chargeTimeDecrease;
                chargeSpeed *= velocityMult;
            }

            float reduceSpeedFlareBombDistance = 570f;
            int flareBombPhaseTimer = bossRush ? 30 : death ? 40 : 60;
            int flareBombSpawnDivisor = flareBombPhaseTimer / 20;
            float flareBombPhaseAcceleration = bossRush ? 1f : death ? 0.92f : 0.8f;
            float flareBombPhaseVelocity = bossRush ? 16f : death ? 14f : 12f;

            int fireTornadoPhaseTimer = 90;

            int newPhaseTimer = 180;

            int flareDustPhaseTimer = bossRush ? 160 : death ? 200 : 240;
            int flareDustPhaseTimer2 = bossRush ? 80 : death ? 100 : 120;

            float spinTime = flareDustPhaseTimer / 2;

            int flareDustSpawnDivisor = flareDustPhaseTimer / 10;
            int flareDustSpawnDivisor2 = flareDustPhaseTimer2 / 30;
            int flareDustSpawnDivisor3 = flareDustPhaseTimer / 25;

            float spinPhaseVelocity = 25f;
            float spinPhaseRotation = MathHelper.TwoPi * 3 / spinTime;

            float increasedIdleTimeAfterBulletHell = 120f;
            bool moveSlowerAfterBulletHell = NPC.ai[2] < 0f;
            if (moveSlowerAfterBulletHell)
            {
                float reducedMovementMultiplier = MathHelper.Lerp(0.1f, 1f, (NPC.ai[2] + increasedIdleTimeAfterBulletHell) / increasedIdleTimeAfterBulletHell);
                acceleration *= reducedMovementMultiplier;
                velocity *= reducedMovementMultiplier;
            }

            float teleportPhaseTimer = 30f;

            int spawnPhaseTimer = 75;

            // Target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            // Despawn
            if (player.dead || !player.active)
            {
                NPC.TargetClosest();
                player = Main.player[NPC.target];
                if (player.dead || !player.active)
                {
                    NPC.velocity.Y -= 0.4f;

                    if (NPC.timeLeft > 60)
                        NPC.timeLeft = 60;

                    if (NPC.ai[0] > 12f)
                        NPC.ai[0] = 13f;
                    else if (NPC.ai[0] > 5f)
                        NPC.ai[0] = 6f;
                    else
                        NPC.ai[0] = 0f;

                    NPC.ai[2] = 0f;
                }
            }
            else if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;

            int xPos = 60 * NPC.direction;
            Vector2 vector = Vector2.Normalize(player.Center - NPC.Center) * (NPC.width + 20) / 2f + NPC.Center;
            Vector2 fromMouth = new Vector2((int)vector.X + xPos, (int)vector.Y - 15);

            // Create the arena, but not as a multiplayer client.
            // In single player, the arena gets created and never gets synced because it's single player.
            // In multiplayer, only the server/host creates the arena, and everyone else receives it on the next frame via SendExtraAI.
            // Everyone however sets spawnArena to true to confirm that the fight has started.
            if (!spawnArena)
            {
                spawnArena = true;
                enraged = false;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    safeBox.X = (int)(player.Center.X - (Main.zenithWorld ? 1500f : Main.getGoodWorld ? 1000f : bossRush ? 2000f : revenge ? 3000f : 3500f));
                    safeBox.Y = (int)(player.Center.Y - 10500f);
                    safeBox.Width = Main.zenithWorld ? 3000 : Main.getGoodWorld ? 2000 : bossRush ? 4000 : revenge ? 6000 : 7000;
                    safeBox.Height = 21000;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center.X + (Main.zenithWorld ? 1500f : Main.getGoodWorld ? 1000f : bossRush ? 2000f : revenge ? 3000f : 3500f), player.Center.Y + 100f, 0f, 0f, ModContent.ProjectileType<SkyFlareRevenge>(), 0, 0f, Main.myPlayer);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center.X - (Main.zenithWorld ? 1500f : Main.getGoodWorld ? 1000f : bossRush ? 2000f : revenge ? 3000f : 3500f), player.Center.Y + 100f, 0f, 0f, ModContent.ProjectileType<SkyFlareRevenge>(), 0, 0f, Main.myPlayer);
                }

                // Force Yharon to send a sync packet so that the arena gets sent immediately
                NPC.netUpdate = true;
            }
            // Enrage code doesn't run on frame 1 so that Yharon won't be enraged for 1 frame in multiplayer
            else
            {
                enraged = !player.Hitbox.Intersects(safeBox);
                NPC.Calamity().CurrentlyEnraged = enraged;
                if (enraged)
                {
                    phaseSwitchTimer = 15;
                    protectionBoost = true;
                    NPC.damage = NPC.defDamage * 5;
                    chargeSpeed += 25f;
                }
                else
                {
                    NPC.damage = NPC.defDamage;
                    protectionBoost = false;
                }
            }

            if (Main.getGoodWorld)
                phaseSwitchTimer /= 2;

            // Set DR based on protection boost (aka enrage)
            bool chargeTelegraph = (NPC.ai[0] == 0f || NPC.ai[0] == 6f || NPC.ai[0] == 13f) && NPC.localAI[1] > 0f;
            bool bulletHell = NPC.ai[0] == 8f || NPC.ai[0] == 15f;
            calamityGlobalNPC.DR = protectionBoost ? EnragedDR : normalDR;
            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = protectionBoost;

            // Increased DR during phase transitions
            if (!protectionBoost)
            {
                if (phase3Change)
                {
                    calamityGlobalNPC.DR = phase4Check ? (bossRush ? 0.99f : 0.7f) : normalDR;
                    calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = phase4Check;
                }
                else if (phase2Change)
                {
                    calamityGlobalNPC.DR = phase3Check ? (bossRush ? 0.99f : 0.7f) : normalDR;
                    calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = phase3Check;
                }
                else if (phase1Change)
                {
                    calamityGlobalNPC.DR = phase2Check ? (bossRush ? 0.99f : 0.7f) : normalDR;
                    calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = phase2Check;
                }
            }

            if (bulletHell)
                NPC.damage = 0;

            NPC.dontTakeDamage = bulletHell;

            // Trigger spawn effects
            if (NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;
                NPC.Opacity = 0f;
                NPC.rotation = 0f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.ai[0] = -1f;
                    NPC.netUpdate = true;
                }
            }

            // Rotation
            float npcRotation = (float)Math.Atan2(player.Center.Y - NPC.Center.Y, player.Center.X - NPC.Center.X);
            if (NPC.spriteDirection == 1)
                npcRotation += pie;
            if (npcRotation < 0f)
                npcRotation += MathHelper.TwoPi;
            if (npcRotation > MathHelper.TwoPi)
                npcRotation -= MathHelper.TwoPi;
            if (NPC.ai[0] == -1f || NPC.ai[0] == 3f || NPC.ai[0] == 4f || NPC.ai[0] == 9f || NPC.ai[0] == 10f || NPC.ai[0] == 16f)
                npcRotation = 0f;

            float npcRotationSpeed = 0.04f;
            if (NPC.ai[0] == 1f || NPC.ai[0] == 5f || NPC.ai[0] == 7f || NPC.ai[0] == 11f || NPC.ai[0] == 12f || NPC.ai[0] == 14f || NPC.ai[0] == 18f || NPC.ai[0] == 19f)
                npcRotationSpeed = 0f;
            if (NPC.ai[0] == 3f || NPC.ai[0] == 4f || NPC.ai[0] == 9f || NPC.ai[0] == 16f)
                npcRotationSpeed = 0.01f;

            if (npcRotationSpeed != 0f)
                NPC.rotation = NPC.rotation.AngleTowards(npcRotation, npcRotationSpeed);

            // Alpha effects
            if (NPC.ai[0] != -1f && !bulletHell && ((NPC.ai[0] != 6f && NPC.ai[0] != 13f) || NPC.ai[2] <= phaseSwitchTimer))
            {
                bool colliding = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);

                if (colliding)
                    NPC.Opacity -= 0.1f;
                else
                    NPC.Opacity += 0.1f;

                if (NPC.Opacity > 1f)
                    NPC.Opacity = 1f;

                if (NPC.Opacity < 0.6f)
                    NPC.Opacity = 0.6f;
            }

            // Spawn effects
            if (NPC.ai[0] == -1f)
            {
                NPC.velocity *= 0.98f;

                int num1467 = Math.Sign(player.Center.X - NPC.Center.X);
                if (num1467 != 0)
                {
                    NPC.direction = num1467;
                    NPC.spriteDirection = -NPC.direction;
                }

                if (NPC.ai[2] > 20f)
                {
                    NPC.velocity.Y = -2f;
                    NPC.Opacity += 0.1f;

                    bool colliding = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);

                    if (colliding)
                        NPC.Opacity -= 0.1f;

                    if (NPC.Opacity > 1f)
                        NPC.Opacity = 1f;

                    if (NPC.Opacity < 0.6f)
                        NPC.Opacity = 0.6f;
                }

                if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
                {
                    int num1468 = 72;
                    for (int num1469 = 0; num1469 < num1468; num1469++)
                    {
                        Vector2 vector169 = Vector2.Normalize(NPC.velocity) * new Vector2(NPC.width / 2f, NPC.height) * 0.75f * 0.5f;
                        vector169 = vector169.RotatedBy((num1469 - (num1468 / 2 - 1)) * MathHelper.TwoPi / num1468) + NPC.Center;
                        Vector2 value16 = vector169 - NPC.Center;
                        int num1470 = Dust.NewDust(vector169 + value16, 0, 0, 244, value16.X * 2f, value16.Y * 2f, 100, default, 1.4f);
                        Main.dust[num1470].noGravity = true;
                        Main.dust[num1470].noLight = true;
                        Main.dust[num1470].velocity = Vector2.Normalize(value16) * 3f;
                    }

                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= spawnPhaseTimer)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = Main.rand.Next(4);
                    NPC.netUpdate = true;
                }
            }

            #region Phase1
            // Phase switch
            else if (NPC.ai[0] == 0f && !player.dead)
            {
                if (NPC.ai[1] == 0f)
                    NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

                Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
                Vector2 distanceFromDestination = destination - NPC.Center;
                Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

                if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                    NPC.SimpleFlyMovement(desiredVelocity, acceleration);
                else
                    NPC.velocity *= 0.98f;

                int num1471 = Math.Sign(player.Center.X - NPC.Center.X);
                if (num1471 != 0)
                {
                    if (NPC.ai[2] == 0f && num1471 != NPC.direction)
                        NPC.rotation += pie;

                    NPC.direction = num1471;

                    if (NPC.spriteDirection != -NPC.direction)
                        NPC.rotation += pie;

                    NPC.spriteDirection = -NPC.direction;
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= phaseSwitchTimer)
                {
                    int aiState = 0;
                    switch ((int)NPC.ai[3])
                    {
                        case 0:
                        case 1:
                        case 2:
                            aiState = 1;
                            break;
                        case 3:
                            aiState = 5;
                            break;
                        case 4:
                            NPC.ai[3] = 1f;
                            aiState = 2;
                            break;
                        case 5:
                            NPC.ai[3] = 0f;
                            aiState = 3;
                            break;
                    }

                    if (phase2Check)
                        aiState = 4;

                    if (aiState == 1)
                    {
                        NPC.ai[0] = 1f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;

                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (num1471 != 0)
                        {
                            NPC.direction = num1471;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += pie;

                            NPC.spriteDirection = -NPC.direction;
                        }
                    }
                    else if (aiState == 2)
                    {
                        NPC.ai[0] = 2f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 3)
                    {
                        NPC.ai[0] = 3f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 4)
                    {
                        NPC.ai[0] = 4f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 5)
                    {
                        if (playFastChargeRoarSound)
                            RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                        if (doFastCharge)
                        {
                            NPC.ai[0] = 5f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.localAI[1] = 0f;
                            NPC.netUpdate = true;

                            NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed * fastChargeVelocityMultiplier;
                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                            if (num1471 != 0)
                            {
                                NPC.direction = num1471;

                                if (NPC.spriteDirection == 1)
                                    NPC.rotation += pie;

                                NPC.spriteDirection = -NPC.direction;
                            }
                        }
                        else
                            NPC.localAI[1] += 1f;
                    }
                }
            }

            // Charge
            else if (NPC.ai[0] == 1f)
            {
                ChargeDust(7, pie);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= chargeTime)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 2f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Fireball breath
            else if (NPC.ai[0] == 2f)
            {
                if (NPC.ai[1] == 0f)
                    NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

                Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
                Vector2 value17 = destination - NPC.Center;
                Vector2 vector170 = Vector2.Normalize(value17 - NPC.velocity) * flareBombPhaseVelocity;

                if (Vector2.Distance(NPC.Center, destination) > reduceSpeedFlareBombDistance)
                    NPC.SimpleFlyMovement(vector170, flareBombPhaseAcceleration);
                else
                    NPC.velocity *= 0.98f;

                if (NPC.ai[2] == 0f)
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                if (NPC.ai[2] % flareBombSpawnDivisor == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int type = ModContent.ProjectileType<FlareBomb>();
                        int damage = NPC.GetProjectileDamage(type);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), fromMouth, Vector2.Zero, type, damage, 0f, Main.myPlayer, NPC.target, 1f);
                    }
                }

                int num1476 = Math.Sign(player.Center.X - NPC.Center.X);
                if (num1476 != 0)
                {
                    NPC.direction = num1476;

                    if (NPC.spriteDirection != -NPC.direction)
                        NPC.rotation += pie;

                    NPC.spriteDirection = -NPC.direction;
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= flareBombPhaseTimer)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Fire tornadoes
            else if (NPC.ai[0] == 3f)
            {
                NPC.velocity *= 0.98f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

                if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] == fireTornadoPhaseTimer - 30)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, NPC.direction * 4, 8f, ModContent.ProjectileType<Flare>(), 0, 0f, Main.myPlayer, 0f, 0f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, -(float)NPC.direction * 4, 8f, ModContent.ProjectileType<Flare>(), 0, 0f, Main.myPlayer, 0f, 0f);
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= fireTornadoPhaseTimer)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Enter new phase
            else if (NPC.ai[0] == 4f)
            {
                NPC.velocity *= 0.9f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

                if (NPC.ai[2] == newPhaseTimer - 60)
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= newPhaseTimer)
                {
                    NPC.ai[0] = 6f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = Main.rand.Next(5);
                    NPC.localAI[1] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Fast charge
            else if (NPC.ai[0] == 5f)
            {
                ChargeDust(14, pie);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= chargeTime)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 2f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }
            #endregion

            #region Phase2
            // Phase switch
            else if (NPC.ai[0] == 6f && !player.dead)
            {
                if (NPC.ai[1] == 0f)
                    NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

                Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
                Vector2 distanceFromDestination = destination - NPC.Center;
                Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

                if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                    NPC.SimpleFlyMovement(desiredVelocity, acceleration);
                else
                    NPC.velocity *= 0.98f;

                int num1477 = Math.Sign(player.Center.X - NPC.Center.X);
                if (num1477 != 0)
                {
                    if (NPC.ai[2] == 0f && num1477 != NPC.direction)
                        NPC.rotation += pie;

                    NPC.direction = num1477;

                    if (NPC.spriteDirection != -NPC.direction)
                        NPC.rotation += pie;

                    NPC.spriteDirection = -NPC.direction;
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= phaseSwitchTimer)
                {
                    int aiState = 0;
                    switch ((int)NPC.ai[3])
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            aiState = 1;
                            break;
                        case 4:
                            aiState = 5;
                            break;
                        case 5:
                            aiState = 6;
                            break;
                        case 6:
                            aiState = 2;
                            break;
                        case 7:
                            NPC.ai[3] = 0f;
                            aiState = 3;
                            break;
                    }

                    if (phase3Check)
                        aiState = 4;

                    if (aiState == 1)
                    {
                        NPC.ai[0] = 7f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;

                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (num1477 != 0)
                        {
                            NPC.direction = num1477;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += pie;

                            NPC.spriteDirection = -NPC.direction;
                        }
                    }
                    else if (aiState == 2)
                    {
                        NPC.damage = 0;

                        if (NPC.Opacity > 0f)
                        {
                            NPC.Opacity -= 0.2f;
                            if (NPC.Opacity < 0f)
                                NPC.Opacity = 0f;
                        }

                        bool spawnBulletHellVortex = NPC.ai[2] == phaseSwitchTimer + 15f;
                        if (spawnBulletHellVortex)
                        {
                            SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 center = player.Center + new Vector2(0f, -540f);
                                NPC.Center = center;

                                int type = ModContent.ProjectileType<YharonBulletHellVortex>();
                                int damage = Main.zenithWorld ? NPC.GetProjectileDamage(type) : 0;
                                float bulletHellVortexDuration = flareDustPhaseTimer + teleportPhaseTimer - 15f;
                                int extraTime = Main.zenithWorld ? 300 : 0;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, damage, 0f, Main.myPlayer, bulletHellVortexDuration + extraTime, NPC.whoAmI);

                                // Yharon takes a small amount of damage in order to summon the bullet hell. This is to compensate for him being invulnerable during it.
                                int damageAmt = (int)(NPC.lifeMax * (bulletHellVortexDuration / calamityGlobalNPC.KillTime));
                                NPC.life -= damageAmt;
                                if (NPC.life < 1)
                                    NPC.life = 1;

                                NPC.HealEffect(-damageAmt, true);
                                NPC.netUpdate = true;
                            }
                        }

                        if (NPC.ai[2] >= phaseSwitchTimer + 15f)
                        {
                            NPC.dontTakeDamage = true;
                            NPC.velocity = Vector2.Zero;
                        }

                        if (NPC.ai[2] < phaseSwitchTimer + teleportPhaseTimer)
                            return;

                        NPC.ai[0] = 8f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 1f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 3)
                    {
                        NPC.ai[0] = 9f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 4)
                    {
                        NPC.ai[0] = 10f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 5)
                    {
                        if (playFastChargeRoarSound)
                            RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                        if (doFastCharge)
                        {
                            NPC.ai[0] = 11f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.localAI[1] = 0f;
                            NPC.netUpdate = true;

                            NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed * fastChargeVelocityMultiplier;
                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                            if (num1477 != 0)
                            {
                                NPC.direction = num1477;

                                if (NPC.spriteDirection == 1)
                                    NPC.rotation += pie;

                                NPC.spriteDirection = -NPC.direction;
                            }
                        }
                        else
                            NPC.localAI[1] += 1f;
                    }
                    else if (aiState == 6)
                    {
                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * spinPhaseVelocity;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (num1477 != 0)
                        {
                            NPC.direction = num1477;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += pie;

                            NPC.spriteDirection = -NPC.direction;
                        }

                        NPC.ai[0] = 12f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                }
            }

            // Charge
            else if (NPC.ai[0] == 7f)
            {
                ChargeDust(7, pie);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= chargeTime)
                {
                    NPC.ai[0] = 6f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 2f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Flare Dust bullet hell
            else if (NPC.ai[0] == 8f)
            {
                if (NPC.ai[2] == 0f)
                {
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                    SoundEngine.PlaySound(OrbSound, NPC.Center);
                } 

                NPC.ai[2] += 1f;

                if (NPC.ai[2] % flareDustSpawnDivisor == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int ringReduction = (int)MathHelper.Lerp(0f, 14f, NPC.ai[2] / flareDustPhaseTimer);
                        int totalProjectiles = 38 - ringReduction; // 36 for first ring, 22 for last ring
                        DoFlareDustBulletHell(0, flareDustSpawnDivisor, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles, 0f, 0f, false);
                    }
                }

                if (NPC.ai[2] >= flareDustPhaseTimer)
                {
                    NPC.ai[0] = 6f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = -increasedIdleTimeAfterBulletHell;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Infernado
            else if (NPC.ai[0] == 9f)
            {
                NPC.velocity *= 0.98f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

                if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] == fireTornadoPhaseTimer - 30)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, 0f, 0f, ModContent.ProjectileType<BigFlare>(), 0, 0f, Main.myPlayer, 1f, NPC.target + 1);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= fireTornadoPhaseTimer)
                {
                    NPC.ai[0] = 6f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Enter new phase
            else if (NPC.ai[0] == 10f)
            {
                NPC.velocity *= 0.9f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

                if (NPC.ai[2] == newPhaseTimer - 60)
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= newPhaseTimer)
                {
                    NPC.ai[0] = 13f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = Main.rand.Next(5);
                    NPC.localAI[1] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Fast charge
            else if (NPC.ai[0] == 11f)
            {
                ChargeDust(14, pie);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= chargeTime)
                {
                    NPC.ai[0] = 6f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 2f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Flare Dust circle
            else if (NPC.ai[0] == 12f)
            {
                if (NPC.ai[2] == 0f)
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                NPC.ai[2] += 1f;

                if (NPC.ai[2] % flareDustSpawnDivisor2 == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 projectileVelocity = NPC.velocity;
                        projectileVelocity.Normalize();
                        int type = ModContent.ProjectileType<FlareDust2>();
                        int damage = NPC.GetProjectileDamage(type);
                        float finalVelocity = 12f;
                        float projectileAcceleration = 1.1f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), fromMouth, projectileVelocity, type, damage, 0f, Main.myPlayer, finalVelocity, projectileAcceleration);
                    }
                }

                NPC.velocity = NPC.velocity.RotatedBy(-(double)spinPhaseRotation * (float)NPC.direction);
                NPC.rotation -= spinPhaseRotation * NPC.direction;

                if (NPC.ai[2] >= flareDustPhaseTimer2)
                {
                    NPC.ai[0] = 6f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 2f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }
            #endregion

            #region Phase3
            // Phase switch
            else if (NPC.ai[0] == 13f && !player.dead)
            {
                if (NPC.ai[1] == 0f)
                    NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

                Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
                Vector2 distanceFromDestination = destination - NPC.Center;
                Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

                if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                    NPC.SimpleFlyMovement(desiredVelocity, acceleration);
                else
                    NPC.velocity *= 0.98f;

                int num1477 = Math.Sign(player.Center.X - NPC.Center.X);
                if (num1477 != 0)
                {
                    if (NPC.ai[2] == 0f && num1477 != NPC.direction)
                        NPC.rotation += pie;

                    NPC.direction = num1477;

                    if (NPC.spriteDirection != -NPC.direction)
                        NPC.rotation += pie;

                    NPC.spriteDirection = -NPC.direction;
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= phaseSwitchTimer)
                {
                    int aiState = 0;
                    switch ((int)NPC.ai[3])
                    {
                        case 0:
                        case 1:
                            aiState = 1;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            aiState = 5;
                            break;
                        case 5:
                            aiState = 3;
                            break;
                        case 6:
                            aiState = 6;
                            break;
                        case 7:
                            NPC.ai[3] = 1f;
                            aiState = 7;
                            break;
                        case 8:
                            aiState = 2;
                            break;
                    }

                    if (phase4Check)
                        aiState = 4;

                    if (aiState == 1)
                    {
                        NPC.ai[0] = 14f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;

                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (num1477 != 0)
                        {
                            NPC.direction = num1477;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += pie;

                            NPC.spriteDirection = -NPC.direction;
                        }
                    }
                    else if (aiState == 2)
                    {
                        NPC.damage = 0;

                        if (NPC.Opacity > 0f)
                        {
                            NPC.Opacity -= 0.2f;
                            if (NPC.Opacity < 0f)
                                NPC.Opacity = 0f;
                        }

                        bool spawnBulletHellVortex = NPC.ai[2] == phaseSwitchTimer + 15f;
                        if (spawnBulletHellVortex)
                        {
                            SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 center = player.Center + new Vector2(0f, -540f);
                                NPC.Center = center;

                                int type = ModContent.ProjectileType<YharonBulletHellVortex>();
                                int damage = Main.zenithWorld ? NPC.GetProjectileDamage(type) : 0;
                                float bulletHellVortexDuration = flareDustPhaseTimer + teleportPhaseTimer - 15f;
                                int extraTime = Main.zenithWorld ? 300 : 0;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, damage, 0f, Main.myPlayer, bulletHellVortexDuration + extraTime, NPC.whoAmI);

                                // Yharon takes a small amount of damage in order to summon the bullet hell. This is to compensate for him being invulnerable during it.
                                int damageAmt = (int)(NPC.lifeMax * (bulletHellVortexDuration / calamityGlobalNPC.KillTime));
                                NPC.life -= damageAmt;
                                if (NPC.life < 1)
                                    NPC.life = 1;

                                NPC.HealEffect(-damageAmt, true);
                                NPC.netUpdate = true;
                            }
                        }

                        if (NPC.ai[2] >= phaseSwitchTimer + 15f)
                        {
                            NPC.dontTakeDamage = true;
                            NPC.velocity = Vector2.Zero;
                        }

                        if (NPC.ai[2] < phaseSwitchTimer + teleportPhaseTimer)
                            return;

                        NPC.ai[0] = 15f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 3)
                    {
                        NPC.ai[0] = 16f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 4)
                    {
                        NPC.ai[0] = 17f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 5)
                    {
                        if (playFastChargeRoarSound)
                            RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                        if (doFastCharge)
                        {
                            NPC.ai[0] = 18f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.localAI[1] = 0f;
                            NPC.netUpdate = true;

                            NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed * fastChargeVelocityMultiplier;
                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                            if (num1477 != 0)
                            {
                                NPC.direction = num1477;

                                if (NPC.spriteDirection == 1)
                                    NPC.rotation += pie;

                                NPC.spriteDirection = -NPC.direction;
                            }
                        }
                        else
                            NPC.localAI[1] += 1f;
                    }
                    else if (aiState == 6)
                    {
                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * spinPhaseVelocity;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (num1477 != 0)
                        {
                            NPC.direction = num1477;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += pie;

                            NPC.spriteDirection = -NPC.direction;
                        }

                        NPC.ai[0] = 19f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (aiState == 7)
                    {
                        NPC.ai[0] = 20f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                }
            }

            // Charge
            else if (NPC.ai[0] == 14f)
            {
                ChargeDust(7, pie);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= chargeTime)
                {
                    NPC.ai[0] = 13f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 2f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Flare Dust bullet hell
            else if (NPC.ai[0] == 15f)
            {
                if (NPC.ai[2] == 0f)
                {
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                    SoundEngine.PlaySound(OrbSound, NPC.Center);
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] % flareDustSpawnDivisor3 == 0f)
                {
                    // Rotate spiral by 7.2 * (300 / 12) = +90 degrees and then back -90 degrees

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        DoFlareDustBulletHell(1, flareDustPhaseTimer, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), 8, 12f, 3.6f, false);
                    }
                }

                if (NPC.ai[2] >= flareDustPhaseTimer)
                {
                    NPC.ai[0] = 13f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = -increasedIdleTimeAfterBulletHell;
                    NPC.localAI[2] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Infernado
            else if (NPC.ai[0] == 16f)
            {
                NPC.velocity *= 0.98f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

                if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] == fireTornadoPhaseTimer - 30)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, 0f, 0f, ModContent.ProjectileType<BigFlare>(), 0, 0f, Main.myPlayer, 1f, NPC.target + 1);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= fireTornadoPhaseTimer)
                {
                    NPC.ai[0] = 13f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 3f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Enter new phase
            else if (NPC.ai[0] == 17f)
            {
                NPC.velocity *= 0.9f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

                if (NPC.ai[2] == newPhaseTimer - 60)
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= newPhaseTimer)
                {
                    startSecondAI = true;
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.localAI[1] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Fast charge
            else if (NPC.ai[0] == 18f)
            {
                ChargeDust(14, pie);

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= chargeTime)
                {
                    NPC.ai[0] = 13f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 2f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Fireball ring
            else if (NPC.ai[0] == 19f)
            {
                if (NPC.ai[2] == 0f)
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                NPC.ai[2] += 1f;

                if (NPC.ai[2] % flareDustSpawnDivisor2 == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 projectileVelocity = NPC.velocity;
                        projectileVelocity.Normalize();
                        int type = ModContent.ProjectileType<FlareDust2>();
                        int damage = NPC.GetProjectileDamage(type);
                        float finalVelocity = 15f;
                        float projectileAcceleration = 1.11f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), fromMouth, projectileVelocity, type, damage, 0f, Main.myPlayer, finalVelocity, projectileAcceleration);
                    }
                }

                NPC.velocity = NPC.velocity.RotatedBy(-(double)spinPhaseRotation * (float)NPC.direction);
                NPC.rotation -= spinPhaseRotation * NPC.direction;

                if (NPC.ai[2] >= flareDustPhaseTimer2 - 50)
                {
                    NPC.ai[0] = 13f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            // Fireball breath
            else if (NPC.ai[0] == 20f)
            {
                if (NPC.ai[1] == 0f)
                    NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

                Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
                Vector2 value17 = destination - NPC.Center;
                Vector2 vector170 = Vector2.Normalize(value17 - NPC.velocity) * flareBombPhaseVelocity;

                if (Vector2.Distance(NPC.Center, destination) > reduceSpeedFlareBombDistance)
                    NPC.SimpleFlyMovement(vector170, flareBombPhaseAcceleration);
                else
                    NPC.velocity *= 0.98f;

                if (NPC.ai[2] == 0f)
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                if (NPC.ai[2] % flareBombSpawnDivisor == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int type = ModContent.ProjectileType<FlareBomb>();
                        int damage = NPC.GetProjectileDamage(type);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), fromMouth, Vector2.Zero, type, damage, 0f, Main.myPlayer, NPC.target, 1f);
                    }
                }

                int num1476 = Math.Sign(player.Center.X - NPC.Center.X);
                if (num1476 != 0)
                {
                    NPC.direction = num1476;

                    if (NPC.spriteDirection != -NPC.direction)
                        NPC.rotation += pie;

                    NPC.spriteDirection = -NPC.direction;
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= flareBombPhaseTimer - 15)
                {
                    NPC.ai[0] = 13f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }
            #endregion
        }

        #region AI2
        public void Yharon_AI2(bool expertMode, bool revenge, bool death, bool bossRush, float pie, float lifeRatio, CalamityGlobalNPC calamityGlobalNPC)
        {
            CalamityGlobalNPC.yharonP2 = NPC.whoAmI;

            float phase2GateValue = revenge ? 0.44f : expertMode ? 0.385f : 0.275f;
            bool phase2 = death || lifeRatio <= phase2GateValue;
            float phase3GateValue = death ? 0.358f : revenge ? 0.275f : expertMode ? 0.22f : 0.138f;
            bool phase3 = lifeRatio <= phase3GateValue;
            float phase4GateValue = death ? 0.165f : 0.11f;
            bool phase4 = lifeRatio <= phase4GateValue && revenge;

            if (NPC.ai[0] != 5f && NPC.ai[0] != 8f)
            {
                NPC.Opacity += 0.1f;
                if (NPC.Opacity > 1f)
                    NPC.Opacity = 1f;
            }

            if (!moveCloser)
            {
                moveCloser = true;

                string key = "Mods.CalamityMod.Status.Boss.FlameText";
                Color messageColor = Color.Orange;

                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }

            NPC.dontTakeDamage = false;
            bool invincible = invincibilityCounter < Phase2InvincibilityTime;
            if (invincible)
            {
                if (Main.zenithWorld)
                {
                    if (NPC.life < NPC.lifeMax)
                    {
                        NPC.life += (int)(NPC.lifeMax * 0.01f);
                        NPC.HealEffect((int)(NPC.lifeMax * 0.01f), true);
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        NPC.life = NPC.lifeMax;
                    }
                }
                NPC.dontTakeDamage = true;
                phase2 = phase3 = phase4 = false;
                invincibilityCounter++;
            }

            // Acquire target and determine enrage state
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                NPC.TargetClosest();

            Player targetData = Main.player[NPC.target];

            // Despawn
            bool targetDead = false;
            if (targetData.dead || !targetData.active)
            {
                NPC.TargetClosest();
                targetData = Main.player[NPC.target];
                if (targetData.dead || !targetData.active)
                {
                    targetDead = true;

                    NPC.velocity.Y -= 0.4f;

                    if (NPC.timeLeft > 60)
                        NPC.timeLeft = 60;

                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                }
            }
            else if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;

            enraged = !targetData.Hitbox.Intersects(safeBox);
            if (enraged)
            {
                protectionBoost = true;
                NPC.damage = NPC.defDamage * 5;
            }
            else
            {
                protectionBoost = false;
                NPC.damage = NPC.defDamage;
            }

            // Set DR based on protection boost (aka enrage)
            bool bulletHell = NPC.ai[0] == 5f;
            NPC.dontTakeDamage = bulletHell;
            calamityGlobalNPC.DR = protectionBoost ? EnragedDR : normalDR;
            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = protectionBoost;

            // Increased DR during phase transitions
            if (!protectionBoost)
            {
                switch (secondPhasePhase)
                {
                    case 1:
                        calamityGlobalNPC.DR = phase2 ? (bossRush ? 0.99f : 0.7f) : normalDR;
                        calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = phase2;
                        break;
                    case 2:
                        calamityGlobalNPC.DR = phase3 ? (bossRush ? 0.99f : 0.7f) : normalDR;
                        calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = phase3;
                        break;
                    case 3:
                        calamityGlobalNPC.DR = phase4 ? (bossRush ? 0.99f : 0.7f) : normalDR;
                        calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = phase4;
                        break;
                }

                if (NPC.ai[0] == 9f)
                {
                    calamityGlobalNPC.DR = bossRush ? 0.99f : 0.7f;
                    calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = true;
                }
            }

            if (bulletHell)
                NPC.damage = 0;

            float reduceSpeedChargeDistance = 500f;
            float reduceSpeedFireballSpitChargeDistance = 800f;
            float phaseSwitchTimer = bossRush ? 28f : expertMode ? 30f : 32f;
            float acceleration = expertMode ? 0.92f : 0.9f;
            float velocity = expertMode ? 14.5f : 14f;
            float chargeTime = expertMode ? 32f : 35f;
            float chargeSpeed = expertMode ? 32f : 30f;

            float fastChargeVelocityMultiplier = bossRush ? 2f : 1.5f;
            fastChargeTelegraphTime = protectionBoost ? 60 : (100 - secondPhasePhase * 10);
            bool playFastChargeRoarSound = NPC.localAI[1] == fastChargeTelegraphTime * 0.5f;
            bool doFastChargeTelegraph = NPC.localAI[1] <= fastChargeTelegraphTime;

            float fireballBreathTimer = 60f;
            float fireballBreathPhaseTimer = fireballBreathTimer + 80f;
            float fireballBreathPhaseVelocity = expertMode ? 32f : 30f;

            float splittingFireballBreathTimer = 40f;
            float splittingFireballBreathPhaseVelocity = 22f;
            int splittingFireballBreathDivisor = 10;
            int splittingFireballs = 10;
            int splittingFireballBreathTimer2 = splittingFireballs * splittingFireballBreathDivisor;
            float splittingFireballBreathYVelocityTimer = 40f;
            float splittingFireballBreathPhaseTimer = splittingFireballBreathTimer + splittingFireballBreathTimer2 + splittingFireballBreathYVelocityTimer;

            int spinPhaseTimer = secondPhasePhase == 4 ? (bossRush ? 80 : death ? 100 : 120) : (bossRush ? 120 : death ? 150 : 180);
            int flareDustSpawnDivisor = spinPhaseTimer / 10;
            int flareDustSpawnDivisor2 = spinPhaseTimer / 20 + (secondPhasePhase == 4 ? spinPhaseTimer / 60 : 0);

            float increasedIdleTimeAfterBulletHell = 120f;
            bool moveSlowerAfterBulletHell = NPC.ai[1] < 0f;
            if (moveSlowerAfterBulletHell)
            {
                float reducedMovementMultiplier = MathHelper.Lerp(0.1f, 1f, (NPC.ai[1] + increasedIdleTimeAfterBulletHell) / increasedIdleTimeAfterBulletHell);
                acceleration *= reducedMovementMultiplier;
                velocity *= reducedMovementMultiplier;
            }

            float flareSpawnDecelerationTimer = bossRush ? 60f : death ? 75f : 90f;
            int flareSpawnPhaseTimerReduction = revenge ? (int)(flareSpawnDecelerationTimer * (ai2GateValue - lifeRatio)) : 0;
            float flareSpawnPhaseTimer = (bossRush ? 120f : death ? 150f : 180f) - flareSpawnPhaseTimerReduction;

            float teleportPhaseTimer = 45f;

            if (revenge)
            {
                float chargeTimeDecrease = bossRush ? 6f : death ? 4f : 2f;
                float velocityMult = bossRush ? 1.15f : death ? 1.1f : 1.05f;
                acceleration *= velocityMult;
                velocity *= velocityMult;
                chargeTime -= chargeTimeDecrease;
                chargeSpeed *= velocityMult;
            }

            if (Main.getGoodWorld)
                phaseSwitchTimer *= 0.5f;

            if (NPC.ai[0] == 0f)
            {
                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= 10f)
                {
                    NPC.ai[1] = 0f;
                    NPC.ai[0] = 1f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 1f)
            {
                if (NPC.ai[2] == 0f)
                    NPC.ai[2] = (NPC.Center.X < targetData.Center.X) ? 1 : -1;

                Vector2 destination = targetData.Center + new Vector2(-NPC.ai[2], 0f);
                Vector2 desiredVelocity = NPC.SafeDirectionTo(destination) * velocity;

                if (!targetDead)
                {
                    if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                    NPC.SimpleFlyMovement(desiredVelocity, acceleration);
                    else
                        NPC.velocity *= 0.98f;
                }

                int num27 = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
                NPC.direction = NPC.spriteDirection = num27;

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= phaseSwitchTimer)
                {
                    int num28 = 1;
                    if (phase4)
                    {
                        switch ((int)NPC.ai[3])
                        {
                            case 0:
                                num28 = 8; //teleport
                                break;
                            case 1:
                            case 2:
                                num28 = 7; //fast charge
                                break;
                            case 3:
                                num28 = 5; //fire circle + tornado (only once) + fireballs
                                break;
                        }
                    }
                    else if (phase3)
                    {
                        switch ((int)NPC.ai[3])
                        {
                            case 0:
                                num28 = 6; //tornado
                                break;
                            case 1:
                                num28 = 7; //fast charge
                                break;
                            case 2:
                                num28 = 8; //teleport
                                break;
                            case 3:
                                num28 = 7; //fast charge
                                break;
                            case 4:
                                num28 = 5; //fire circle
                                break;
                            case 5:
                                num28 = Main.rand.NextBool() ? 3 : 4; //fireballs
                                break;
                            case 6:
                                num28 = 7; //fast charge
                                break;
                            case 7:
                                num28 = 8; //teleport
                                break;
                            case 8:
                                num28 = 7; //fast charge
                                break;
                            case 9:
                                num28 = Main.rand.NextBool() ? 4 : 3; //fireballs
                                break;
                            case 10:
                                num28 = 6; //tornado
                                break;
                            case 11:
                                num28 = 7; //fast charge
                                break;
                            case 12:
                                num28 = 8; //teleport
                                break;
                            case 13:
                                num28 = 7; //fast charge
                                break;
                            case 14:
                                num28 = 5; //fire circle
                                break;
                            case 15:
                                num28 = Main.rand.NextBool() ? 3 : 4; //fireballs
                                break;
                        }
                    }
                    else if (phase2)
                    {
                        switch ((int)NPC.ai[3])
                        {
                            case 0:
                                num28 = 6; //tornado
                                break;
                            case 1:
                                num28 = 7; //fast charge
                                break;
                            case 2:
                                num28 = 2; //charge
                                break;
                            case 3:
                                num28 = 5; //fire circle
                                break;
                            case 4:
                                num28 = Main.rand.NextBool() ? 3 : 4; //fireballs
                                break;
                            case 5:
                                num28 = 7; //fast charge
                                break;
                            case 6:
                                num28 = 2; //charge
                                break;
                            case 7:
                                num28 = Main.rand.NextBool() ? 4 : 3; //fireballs
                                break;
                            case 8:
                                num28 = 7; //fast charge
                                break;
                            case 9:
                                num28 = 2; //charge
                                break;
                            case 10:
                                num28 = 5; //fire circle
                                break;
                        }
                    }
                    else
                    {
                        switch ((int)NPC.ai[3])
                        {
                            case 0:
                                num28 = 6; //tornado
                                break;
                            case 1:
                            case 2:
                                num28 = 2; //charge
                                break;
                            case 3:
                                num28 = Main.rand.NextBool() ? 3 : 4; //fireballs
                                break;
                            case 4:
                            case 5:
                                num28 = 7; //fast charge
                                break;
                            case 6:
                                num28 = Main.rand.NextBool() ? 4 : 3; //fireballs
                                break;
                            case 7:
                            case 8:
                                num28 = 2; //charge
                                break;
                            case 9:
                                num28 = 5; //fire circle
                                break;
                        }
                    }

                    if (num28 == 5 && NPC.ai[1] < phaseSwitchTimer + teleportPhaseTimer)
                    {
                        NPC.damage = 0;

                        float newRotation = NPC.AngleTo(targetData.Center);
                        float amount = 0.04f;

                        if (NPC.spriteDirection == -1)
                            newRotation += pie;

                        if (amount != 0f)
                            NPC.rotation = NPC.rotation.AngleTowards(newRotation, amount);

                        if (NPC.Opacity > 0f)
                        {
                            NPC.Opacity -= 0.2f;
                            if (NPC.Opacity < 0f)
                                NPC.Opacity = 0f;
                        }

                        float timeBeforeTeleport = teleportPhaseTimer - 15f;
                        bool spawnBulletHellVortex = NPC.ai[1] == phaseSwitchTimer + timeBeforeTeleport;
                        if (spawnBulletHellVortex)
                        {
                            SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (CalamityWorld.LegendaryMode && revenge)
                                {
                                    if (!NPC.AnyNPCs(ModContent.NPCType<Bumblefuck>()))
                                        NPC.SpawnOnPlayer(NPC.FindClosestPlayer(), ModContent.NPCType<Bumblefuck>());
                                }

                                Vector2 center = targetData.Center + new Vector2(0f, -540f);
                                NPC.Center = center;

                                int type = ModContent.ProjectileType<YharonBulletHellVortex>();
                                int damage = Main.zenithWorld ? NPC.GetProjectileDamage(type) : 0;
                                float bulletHellVortexDuration = spinPhaseTimer + 15f;
                                int extraTime = Main.zenithWorld ? 300 : 0;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, damage, 0f, Main.myPlayer, bulletHellVortexDuration + extraTime, NPC.whoAmI);

                                // Yharon takes a small amount of damage in order to summon the bullet hell. This is to compensate for him being invulnerable during it.
                                int damageAmt = (int)(NPC.lifeMax * (bulletHellVortexDuration / calamityGlobalNPC.KillTime));
                                NPC.life -= damageAmt;
                                if (NPC.life < 1)
                                    NPC.life = 1;

                                NPC.HealEffect(-damageAmt, true);
                                NPC.netUpdate = true;
                            }
                        }

                        if (NPC.ai[1] >= phaseSwitchTimer + timeBeforeTeleport)
                        {
                            NPC.dontTakeDamage = true;
                            NPC.velocity = Vector2.Zero;
                        }

                        return;
                    }

                    if (num28 == 7 && doFastChargeTelegraph)
                    {
                        float newRotation = NPC.AngleTo(targetData.Center);
                        float amount = 0.04f;

                        if (NPC.spriteDirection == -1)
                            newRotation += pie;

                        if (amount != 0f)
                            NPC.rotation = NPC.rotation.AngleTowards(newRotation, amount);

                        if (playFastChargeRoarSound)
                            RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                        NPC.localAI[1] += 1f;

                        return;
                    }

                    NPC.ai[0] = num28;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.localAI[1] = 0f;

                    switch (secondPhasePhase)
                    {
                        case 1:
                            if (phase2)
                            {
                                secondPhasePhase = 2;
                                NPC.ai[0] = 9f;
                                NPC.ai[1] = 0f;
                                NPC.ai[2] = 0f;
                                NPC.ai[3] = Main.rand.Next(11);
                            }
                            break;
                        case 2:
                            if (phase3)
                            {
                                secondPhasePhase = 3;
                                NPC.ai[0] = 9f;
                                NPC.ai[1] = 0f;
                                NPC.ai[2] = 0f;
                                NPC.ai[3] = Main.rand.Next(16);
                            }
                            break;
                        case 3:
                            if (phase4)
                            {
                                secondPhasePhase = 4;
                                NPC.ai[0] = 9f;
                                NPC.ai[1] = 0f;
                                NPC.ai[2] = 0f;
                                NPC.ai[3] = 0f;
                            }
                            break;
                    }

                    NPC.netUpdate = true;

                    float aiLimit = 10f;
                    if (phase4)
                        aiLimit = 4f;
                    else if (phase3)
                        aiLimit = 16f;
                    else if (phase2)
                        aiLimit = 11f;

                    if (NPC.ai[3] >= aiLimit)
                        NPC.ai[3] = 0f;

                    switch (num28)
                    {
                        case 2: //charge
                        {
                            Vector2 vector = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                            NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
                            NPC.rotation = vector.ToRotation();

                            if (NPC.spriteDirection == -1)
                                NPC.rotation += pie;

                            NPC.velocity = vector * chargeSpeed;

                            break;
                        }
                        case 3: //fireballs
                        {
                            Vector2 vector2 = new Vector2((targetData.Center.X > NPC.Center.X) ? 1 : -1, 0f);
                            NPC.spriteDirection = (vector2.X > 0f) ? 1 : -1;
                            NPC.velocity = vector2 * -2f;

                            break;
                        }
                        case 5: //spin move
                        {
                            NPC.damage = 0;
                            NPC.dontTakeDamage = true;
                            NPC.localAI[3] = Main.rand.Next(2);
                            NPC.velocity = Vector2.Zero;

                            break;
                        }
                        case 7: //fast charge
                        {
                            Vector2 vector = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                            NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
                            NPC.rotation = vector.ToRotation();

                            if (NPC.spriteDirection == -1)
                                NPC.rotation += pie;

                            NPC.velocity = vector * chargeSpeed * fastChargeVelocityMultiplier;

                            break;
                        }
                    }
                }
            }

            // Charge
            else if (NPC.ai[0] == 2f)
            {
                if (NPC.ai[1] == 1f)
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                ChargeDust(7, pie);

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= chargeTime)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                }
            }

            // Fireball spit charge
            else if (NPC.ai[0] == 3f)
            {
                int num29 = (NPC.Center.X < targetData.Center.X) ? 1 : -1;

                NPC.ai[1] += 1f;
                if (NPC.ai[1] < fireballBreathTimer)
                {
                    Vector2 destination = targetData.Center + new Vector2(num29, 0);
                    Vector2 distanceFromDestination = destination - NPC.Center;
                    Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

                    if (!targetDead)
                    {
                        if (Vector2.Distance(NPC.Center, destination) > reduceSpeedFireballSpitChargeDistance)
                            NPC.SimpleFlyMovement(desiredVelocity, acceleration);
                        else
                            NPC.velocity *= 0.98f;
                    }

                    NPC.direction = NPC.spriteDirection = num29;

                    if (Vector2.Distance(destination, NPC.Center) < 32f)
                        NPC.ai[1] = fireballBreathTimer - 1f;
                }

                if (NPC.ai[1] == fireballBreathTimer)
                {
                    Vector2 vector = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                    NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
                    NPC.rotation = vector.ToRotation();

                    if (NPC.spriteDirection == -1)
                        NPC.rotation += pie;

                    NPC.velocity = vector * fireballBreathPhaseVelocity;

                    SoundEngine.PlaySound(FireSound, NPC.Center);
                }

                if (NPC.ai[1] >= fireballBreathTimer)
                {
                    if (NPC.ai[1] % (expertMode ? 6f : 8f) == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float xOffset = 30f;
                        Vector2 position = NPC.Center + new Vector2((110f + xOffset) * NPC.direction, -20f).RotatedBy(NPC.rotation);
                        Vector2 projectileVelocity = NPC.velocity;
                        projectileVelocity.Normalize();
                        int type = ModContent.ProjectileType<FlareDust2>();
                        int damage = NPC.GetProjectileDamage(type);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), position, projectileVelocity, type, damage, 0f, Main.myPlayer);
                    }

                    if (Math.Abs(targetData.Center.X - NPC.Center.X) > 700f && Math.Abs(NPC.velocity.X) < chargeSpeed)
                        NPC.velocity.X += Math.Sign(NPC.velocity.X) * 0.5f;
                }

                if (NPC.ai[1] >= fireballBreathPhaseTimer)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.TargetClosest();
                }
            }

            // Splitting fireball breath
            else if (NPC.ai[0] == 4f)
            {
                int num31 = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
                NPC.ai[2] = num31;

                if (NPC.ai[1] < splittingFireballBreathTimer)
                {
                    Vector2 vector5 = targetData.Center + new Vector2(num31 * -750f, -300f);
                    Vector2 value2 = NPC.SafeDirectionTo(vector5) * splittingFireballBreathPhaseVelocity;

                    NPC.velocity = Vector2.Lerp(NPC.velocity, value2, 0.0333333351f);

                    int direction = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
                    NPC.direction = NPC.spriteDirection = direction;

                    if (Vector2.Distance(vector5, NPC.Center) < 32f)
                        NPC.ai[1] = splittingFireballBreathTimer - 1f;
                }
                else if (NPC.ai[1] == splittingFireballBreathTimer)
                {
                    Vector2 vector6 = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                    vector6.Y *= 0.15f;
                    vector6 = vector6.SafeNormalize(Vector2.UnitX * NPC.direction);

                    NPC.spriteDirection = (vector6.X > 0f) ? 1 : -1;
                    NPC.rotation = vector6.ToRotation();

                    if (NPC.spriteDirection == -1)
                        NPC.rotation += pie;

                    NPC.velocity = vector6 * splittingFireballBreathPhaseVelocity;
                    SoundEngine.PlaySound(FireSound, NPC.Center);
                }
                else
                {
                    NPC.position.X += NPC.SafeDirectionTo(targetData.Center).X * 7f;
                    NPC.position.Y += NPC.SafeDirectionTo(targetData.Center + new Vector2(0f, -400f)).Y * 6f;

                    float xOffset = 30f;
                    Vector2 position = NPC.Center + new Vector2((110f + xOffset) * NPC.direction, -20f).RotatedBy(NPC.rotation);
                    int num34 = (int)(NPC.ai[1] - splittingFireballBreathTimer + 1f);

                    int type = ModContent.ProjectileType<YharonFireball>();
                    int damage = NPC.GetProjectileDamage(type);
                    if (num34 <= splittingFireballBreathTimer2 && num34 % splittingFireballBreathDivisor == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), position, NPC.velocity, type, damage, 0f, Main.myPlayer, 0f, 0f);
                }

                if (NPC.ai[1] > splittingFireballBreathPhaseTimer - splittingFireballBreathYVelocityTimer)
                    NPC.velocity.Y -= 0.1f;

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= splittingFireballBreathPhaseTimer)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                }
            }

            // Fireball spin
            else if (NPC.ai[0] == 5f)
            {
                if (NPC.ai[1] == 1f)
                {
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                    SoundEngine.PlaySound(OrbSound, NPC.Center);
                }

                NPC.ai[1] += 1f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (secondPhasePhase >= 3)
                    {
                        // Rotate spiral by 9 * (240 / 12) = +90 degrees and then back -90 degrees

                        // For phase 4: Rotate spiral by 18 * (240 / 16) = +135 degrees and then back -135 degrees

                        if (NPC.ai[1] % flareDustSpawnDivisor2 == 0f)
                        {
                            int totalProjectiles = secondPhasePhase == 4 ? 12 : 10;
                            float projectileVelocity = secondPhasePhase == 4 ? 16f : 12f;
                            float radialOffset = secondPhasePhase == 4 ? 2.8f : 3.2f;
                            if (NPC.localAI[3] == 0f)
                            {
                                DoFlareDustBulletHell(1, spinPhaseTimer, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles, projectileVelocity, radialOffset, true);
                            }
                            else
                            {
                                int ringReduction = (int)MathHelper.Lerp(0f, 12f, NPC.ai[1] / spinPhaseTimer);
                                int totalProjectiles2 = 38 - ringReduction; // 36 for first ring, 24 for last ring
                                DoFlareDustBulletHell(0, flareDustSpawnDivisor2, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles2, 0f, 0f, true);
                            }
                        }
                    }
                    else
                    {
                        if (NPC.ai[1] % flareDustSpawnDivisor == 0f)
                        {
                            int ringReduction = (int)MathHelper.Lerp(0f, 12f, NPC.ai[1] / spinPhaseTimer);
                            int totalProjectiles = 38 - ringReduction; // 36 for first ring, 24 for last ring
                            DoFlareDustBulletHell(0, flareDustSpawnDivisor, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles, 0f, 0f, true);
                        }
                    }

                    if (NPC.ai[1] == 210f && secondPhasePhase == 4 && useTornado)
                    {
                        useTornado = false;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BigFlare2>(), 0, 0f, Main.myPlayer, 1f, NPC.target + 1);
                    }
                }

                if (NPC.ai[1] >= spinPhaseTimer)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = -increasedIdleTimeAfterBulletHell;
                    NPC.ai[2] = 0f;
                    NPC.localAI[2] = 0f;
                    NPC.TargetClosest();
                    NPC.velocity /= 2f;
                }
            }

            // Fire ring
            else if (NPC.ai[0] == 6f)
            {
                if (NPC.ai[1] == 0f)
                {
                    Vector2 destination2 = targetData.Center + new Vector2(0f, -200f);
                    Vector2 desiredVelocity2 = NPC.SafeDirectionTo(destination2) * velocity * 1.5f;
                    NPC.SimpleFlyMovement(desiredVelocity2, acceleration * 1.5f);

                    int num35 = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
                    NPC.direction = NPC.spriteDirection = num35;

                    NPC.ai[2] += 1f;
                    if (NPC.Distance(targetData.Center) < 600f || NPC.ai[2] >= 180f)
                    {
                        NPC.ai[1] = 1f;
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    if (NPC.ai[1] < flareSpawnDecelerationTimer)
                        NPC.velocity *= 0.95f;
                    else
                        NPC.velocity *= 0.98f;

                    if (NPC.ai[1] == flareSpawnDecelerationTimer)
                    {
                        if (NPC.velocity.Y > 0f)
                            NPC.velocity.Y /= 3f;

                        NPC.velocity.Y -= 3f;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (NPC.ai[1] == 20f || NPC.ai[1] == 80f || NPC.ai[1] == 140f)
                        {
                            SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                            DoFireRing(expertMode ? 300 : 180, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareBomb>()), NPC.target, 1f);
                        }
                    }

                    NPC.ai[1] += 1f;
                }

                if (NPC.ai[1] >= flareSpawnPhaseTimer)
                {
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BigFlare2>(), 0, 0f, Main.myPlayer, 1f, NPC.target + 1);

                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                }
            }

            // Fast charge
            else if (NPC.ai[0] == 7f)
            {
                if (NPC.ai[1] == 1f)
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                ChargeDust(14, pie);

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= chargeTime)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.TargetClosest();
                }
            }

            // Teleport
            else if (NPC.ai[0] == 8f)
            {
                if (NPC.Opacity > 0f)
                {
                    NPC.Opacity -= 0.1f;
                    if (NPC.Opacity < 0f)
                        NPC.Opacity = 0f;
                }

                NPC.velocity *= 0.98f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

                if (NPC.ai[2] == 15f)
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] == 15f)
                {
                    if (NPC.ai[1] == 0f)
                        NPC.ai[1] = 450 * Math.Sign((NPC.Center - targetData.Center).X);

                    teleportLocation = Main.rand.NextBool() ? (revenge ? 500 : 600) : (revenge ? -500 : -600);
                    Vector2 center = targetData.Center + new Vector2(-NPC.ai[1], teleportLocation);
                    NPC.Center = center;
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= teleportPhaseTimer)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.localAI[1] = fastChargeTelegraphTime + 1f;
                    NPC.netUpdate = true;
                }
            }

            // Enter new phase
            else if (NPC.ai[0] == 9f)
            {
                NPC.velocity *= 0.9f;

                Vector2 vector = NPC.SafeDirectionTo(targetData.Center, -Vector2.UnitY);
                NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
                NPC.rotation = vector.ToRotation();

                if (NPC.spriteDirection == -1)
                    NPC.rotation += pie;

                if (NPC.ai[2] == 120f)
                {
                    if (secondPhasePhase == 4)
                    {
                        for (int x = 0; x < Main.maxProjectiles; x++)
                        {
                            Projectile projectile = Main.projectile[x];
                            if (projectile.active)
                            {
                                if (projectile.type == ModContent.ProjectileType<Infernado2>())
                                {
                                    if (projectile.timeLeft >= 300)
                                        projectile.active = false;
                                    else if (projectile.timeLeft > 5)
                                        projectile.timeLeft = (int)(5f * projectile.ai[1]);
                                }
                                else if (projectile.type == ModContent.ProjectileType<BigFlare2>())
                                    projectile.active = false;
                            }
                        }
                    }

                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= 180f)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }
            }

            float num42 = NPC.AngleTo(targetData.Center);
            float num43 = 0.04f;

            switch ((int)NPC.ai[0])
            {
                case 2:
                case 7:
                case 8:
                case 9:
                    num43 = 0f;
                    break;

                case 3:
                    if (NPC.ai[1] >= fireballBreathTimer)
                        num43 = 0f;

                    break;

                case 4:
                    num43 = 0.01f;
                    num42 = pie;

                    if (NPC.spriteDirection == 1)
                        num42 += pie;

                    break;
                case 6:
                    num43 = 0.02f;
                    num42 = 0f;

                    if (NPC.spriteDirection == -1)
                        num42 -= pie;

                    break;
            }

            if (NPC.spriteDirection == -1)
                num42 += pie;

            if (num43 != 0f)
                NPC.rotation = NPC.rotation.AngleTowards(num42, num43);
        }
        #endregion

        #region Charge Dust
        private void ChargeDust(int dustAmt, float pie)
        {
            for (int num1474 = 0; num1474 < dustAmt; num1474++)
            {
                Vector2 vector171 = Vector2.Normalize(NPC.velocity) * new Vector2((NPC.width + 50) / 2f, NPC.height) * 0.75f;
                vector171 = vector171.RotatedBy((num1474 - (dustAmt / 2 - 1)) * (double)pie / (float)dustAmt) + NPC.Center;
                Vector2 value18 = ((float)(Main.rand.NextDouble() * pie) - MathHelper.PiOver2).ToRotationVector2() * Main.rand.Next(3, 8);
                int num1475 = Dust.NewDust(vector171 + value18, 0, 0, 244, value18.X * 2f, value18.Y * 2f, 100, default, 1.4f);
                Main.dust[num1475].noGravity = true;
                Main.dust[num1475].noLight = true;
                Main.dust[num1475].velocity /= 4f;
                Main.dust[num1475].velocity -= NPC.velocity;
            }
        }
        #endregion

        #region Flare Dust Bullet Hell
        private void DoFlareDustBulletHell(int attackType, int timer, int projectileDamage, int totalProjectiles, float projectileVelocity, float radialOffset, bool phase2)
        {
            SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
            float aiVariableUsed = phase2 ? NPC.ai[1] : NPC.ai[2];
            switch (attackType)
            {
                case 0:
                    float offsetAngle = 360 / totalProjectiles;
                    int totalSpaces = totalProjectiles / 5;
                    int spaceStart = Main.rand.Next(totalProjectiles - totalSpaces);
                    float ai0 = aiVariableUsed % (timer * 2) == 0f ? 1f : 0f;

                    int spacesMade = 0;
                    for (int i = 0; i < totalProjectiles; i++)
                    {
                        if (i >= spaceStart && spacesMade < totalSpaces)
                            spacesMade++;
                        else
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<FlareDust>(), projectileDamage, 0f, Main.myPlayer, ai0, i * offsetAngle);
                    }
                    break;

                case 1:
                    double radians = MathHelper.TwoPi / totalProjectiles;
                    Vector2 spinningPoint = Vector2.Normalize(new Vector2(-NPC.localAI[2], -projectileVelocity));

                    for (int i = 0; i < totalProjectiles; i++)
                    {
                        Vector2 vector2 = spinningPoint.RotatedBy(radians * i) * projectileVelocity;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vector2, ModContent.ProjectileType<FlareDust>(), projectileDamage, 0f, Main.myPlayer, 2f, 0f);
                    }

                    float newRadialOffset = (int)aiVariableUsed / (timer / 4) % 2f == 0f ? radialOffset : -radialOffset;
                    NPC.localAI[2] += newRadialOffset;
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Fire Ring
        public void DoFireRing(int timeLeft, int damage, float ai0, float ai1)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float velocity = ai1 == 0f ? 10f : 5f;
                int totalProjectiles = 50;
                float radians = MathHelper.TwoPi / totalProjectiles;
                for (int i = 0; i < totalProjectiles; i++)
                {
                    Vector2 vector255 = new Vector2(0f, -velocity).RotatedBy(radians * i);
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vector255, ModContent.ProjectileType<FlareBomb>(), damage, 0f, Main.myPlayer, ai0, ai1);
                    Main.projectile[proj].timeLeft = timeLeft;
                }
            }
        }
        #endregion

        #region Drawing
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool idlePhases = (!startSecondAI && (NPC.ai[0] == 0f || NPC.ai[0] == 6f || NPC.ai[0] == 13f)) || (startSecondAI && (NPC.ai[0] == 5f || NPC.ai[0] < 2f));

            bool chargingOrSpawnPhases = (!startSecondAI && (NPC.ai[0] == 1f || NPC.ai[0] == 5f || NPC.ai[0] == 7f || NPC.ai[0] == 11f || NPC.ai[0] == 14f || NPC.ai[0] == 18f)) ||
                (startSecondAI && (NPC.ai[0] == 6f || NPC.ai[0] == 2f || NPC.ai[0] == 3f || NPC.ai[0] == 7f));

            bool tornadoPhase = !startSecondAI && (NPC.ai[0] == 3f || NPC.ai[0] == 9f || NPC.ai[0] == -1f || NPC.ai[0] == 16f);

            bool newPhasePhase = (!startSecondAI && (NPC.ai[0] == 4f || NPC.ai[0] == 10f || NPC.ai[0] == 17f)) || (startSecondAI && NPC.ai[0] == 9f);

            bool pauseAfterTeleportPhase = startSecondAI && NPC.ai[0] == 8f;

            bool ai2 = startSecondAI;

            SpriteEffects spriteEffects = ai2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = ai2 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 vector11 = new Vector2(texture.Width / 2, texture.Height / Main.npcFrameCount[NPC.type] / 2);
            Color color = drawColor;
            Color invincibleColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, 0);
            Color color36 = Color.White;

            float amount9 = 0f;
            bool invincible = ai2 && invincibilityCounter < Phase2InvincibilityTime;
            bool flag8 = NPC.ai[0] > 5f;
            bool flag9 = NPC.ai[0] > 12f;
            bool flag10 = startSecondAI;
            int num150 = 120;
            int num151 = 60;

            if (flag10)
                color = CalamityGlobalNPC.buffColor(color, 0.9f, 0.7f, 0.3f, 1f);
            else if (flag9)
                color = CalamityGlobalNPC.buffColor(color, 0.8f, 0.7f, 0.4f, 1f);
            else if (flag8)
                color = CalamityGlobalNPC.buffColor(color, 0.7f, 0.7f, 0.5f, 1f);
            else if (NPC.ai[0] == 4f && NPC.ai[2] > num150)
            {
                float num152 = NPC.ai[2] - num150;
                num152 /= num151;
                color = CalamityGlobalNPC.buffColor(color, 1f - 0.3f * num152, 1f - 0.3f * num152, 1f - 0.5f * num152, 1f);
            }

            int num153 = 10;
            int num154 = 2;
            if (NPC.ai[0] == -1f)
                num153 = 0;
            if (idlePhases)
                num153 = 7;

            if (invincible)
                color36 = invincibleColor;
            else if (chargingOrSpawnPhases)
            {
                color36 = Color.Red;
                amount9 = 0.5f;
            }
            else
                color = drawColor;

            if (CalamityConfig.Instance.Afterimages)
            {
                for (int num155 = 1; num155 < num153; num155 += num154)
                {
                    Color color38 = color;
                    color38 = Color.Lerp(color38, color36, amount9);
                    color38 = NPC.GetAlpha(color38);
                    color38 *= (num153 - num155) / 15f;
                    Vector2 vector41 = NPC.oldPos[num155] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
                    vector41 -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
                    vector41 += vector11 * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                    spriteBatch.Draw(texture, vector41, NPC.frame, color38, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);
                }
            }

            int num156 = 0;
            float num157 = 0f;
            float scaleFactor9 = 0f;

            if (NPC.ai[0] == -1f)
                num156 = 0;

            if (tornadoPhase)
            {
                int num158 = 60;
                int num159 = 30;
                if (NPC.ai[2] > num158)
                {
                    num156 = 6;
                    num157 = 1f - (float)Math.Cos((NPC.ai[2] - num158) / num159 * MathHelper.TwoPi);
                    num157 /= 3f;
                    scaleFactor9 = 40f;
                }
            }

            if (newPhasePhase && NPC.ai[2] > num150)
            {
                num156 = 6;
                num157 = 1f - (float)Math.Cos((NPC.ai[2] - num150) / num151 * MathHelper.TwoPi);
                num157 /= 3f;
                scaleFactor9 = 60f;
            }

            if (pauseAfterTeleportPhase)
            {
                num156 = 6;
                num157 = 1f - (float)Math.Cos(NPC.ai[2] / 30f * MathHelper.TwoPi);
                num157 /= 3f;
                scaleFactor9 = 20f;
            }

            if (CalamityConfig.Instance.Afterimages)
            {
                for (int num160 = 0; num160 < num156; num160++)
                {
                    Color color39 = drawColor;
                    color39 = Color.Lerp(color39, color36, amount9);
                    color39 = NPC.GetAlpha(color39);
                    color39 *= 1f - num157;
                    Vector2 vector42 = NPC.Center + (num160 / (float)num156 * MathHelper.TwoPi + NPC.rotation).ToRotationVector2() * scaleFactor9 * num157 - screenPos;
                    vector42 -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
                    vector42 += vector11 * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                    spriteBatch.Draw(texture, vector42, NPC.frame, color39, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);
                }
            }

            Vector2 vector43 = NPC.Center - screenPos;
            vector43 -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
            vector43 += vector11 * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture, vector43, NPC.frame, invincible ? invincibleColor : NPC.GetAlpha(drawColor), NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);

            if (flag8 || NPC.ai[0] == 4f || startSecondAI)
            {
                texture = ModContent.Request<Texture2D>("CalamityMod/NPCs/Yharon/YharonGlowOrange").Value;
                Color color40 = Color.Lerp(Color.White, invincible ? invincibleColor : Color.Orange, 0.5f) * NPC.Opacity;
                color36 = invincible ? invincibleColor : Color.Orange;

                Texture2D texture2 = ModContent.Request<Texture2D>("CalamityMod/NPCs/Yharon/YharonGlowGreen").Value;
                Color color43 = Color.Lerp(Color.White, invincible ? invincibleColor : Color.Chartreuse, 0.5f) * NPC.Opacity;
                Color color44 = invincible ? invincibleColor : Color.Chartreuse;

                Texture2D texture3 = ModContent.Request<Texture2D>("CalamityMod/NPCs/Yharon/YharonGlowPurple").Value;
                Color color45 = Color.Lerp(Color.White, invincible ? invincibleColor : Color.BlueViolet, 0.5f) * NPC.Opacity;
                Color color46 = invincible ? invincibleColor : Color.BlueViolet;

                amount9 = 1f;
                num157 = 0.5f;
                scaleFactor9 = 10f;
                num154 = 1;

                if (newPhasePhase)
                {
                    float num161 = NPC.ai[2] - num150;
                    num161 /= num151;
                    color36 *= num161;
                    color40 *= num161;

                    if (flag9 || NPC.ai[0] == 10f || startSecondAI)
                    {
                        color43 *= num161;
                        color44 *= num161;
                    }

                    if (flag10 || NPC.ai[0] == 17f)
                    {
                        color45 *= num161;
                        color46 *= num161;
                    }
                }

                if (pauseAfterTeleportPhase)
                {
                    float num162 = NPC.ai[2];
                    num162 /= 30f;

                    if (num162 > 0.5f)
                        num162 = 1f - num162;

                    num162 *= 2f;
                    num162 = 1f - num162;
                    color36 *= num162;
                    color40 *= num162;
                    color43 *= num162;
                    color44 *= num162;
                    color45 *= num162;
                    color46 *= num162;
                }

                if (CalamityConfig.Instance.Afterimages)
                {
                    for (int num163 = 1; num163 < num153; num163 += num154)
                    {
                        Color color41 = color40;
                        color41 = Color.Lerp(color41, color36, amount9);
                        color41 *= (num153 - num163) / 15f;
                        Vector2 vector44 = NPC.oldPos[num163] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
                        vector44 -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
                        vector44 += vector11 * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                        spriteBatch.Draw(texture, vector44, NPC.frame, color41, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);

                        if (flag9 || NPC.ai[0] == 10f || startSecondAI)
                        {
                            Color color47 = color43;
                            color47 = Color.Lerp(color47, color44, amount9);
                            color47 *= (num153 - num163) / 15f;
                            spriteBatch.Draw(texture2, vector44, NPC.frame, color47, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);
                        }

                        if (flag10 || NPC.ai[0] == 17f)
                        {
                            Color color48 = color45;
                            color48 = Color.Lerp(color48, color46, amount9);
                            color48 *= (num153 - num163) / 15f;
                            spriteBatch.Draw(texture3, vector44, NPC.frame, color48, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);
                        }
                    }

                    for (int num164 = 1; num164 < num156; num164++)
                    {
                        Color color42 = color40;
                        color42 = Color.Lerp(color42, color36, amount9);
                        color42 = NPC.GetAlpha(color42);
                        color42 *= 1f - num157;
                        Vector2 vector45 = NPC.Center + (num164 / (float)num156 * MathHelper.TwoPi + NPC.rotation).ToRotationVector2() * scaleFactor9 * num157 - screenPos;
                        vector45 -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
                        vector45 += vector11 * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                        spriteBatch.Draw(texture, vector45, NPC.frame, color42, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);

                        if (flag9 || NPC.ai[0] == 10f || startSecondAI)
                        {
                            Color color49 = color43;
                            color49 = Color.Lerp(color49, color44, amount9);
                            color49 = NPC.GetAlpha(color49);
                            color49 *= 1f - num157;
                            spriteBatch.Draw(texture2, vector45, NPC.frame, color49, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);
                        }

                        if (flag10 || NPC.ai[0] == 17f)
                        {
                            Color color50 = color45;
                            color50 = Color.Lerp(color50, color46, amount9);
                            color50 = NPC.GetAlpha(color50);
                            color50 *= 1f - num157;
                            spriteBatch.Draw(texture3, vector45, NPC.frame, color50, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);
                        }
                    }
                }

                spriteBatch.Draw(texture, vector43, NPC.frame, color40, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);

                if (flag9 || NPC.ai[0] == 10f || startSecondAI)
                    spriteBatch.Draw(texture2, vector43, NPC.frame, color43, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);

                if (flag10 || NPC.ai[0] == 17f)
                    spriteBatch.Draw(texture3, vector43, NPC.frame, color45, NPC.rotation, vector11, NPC.scale, spriteEffects, 0f);
            }

            return false;
        }
        #endregion

        #region Loot
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Boss bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<YharonBag>()));

            // Normal drops: Everything that would otherwise be in the bag
            var normalOnly = npcLoot.DefineNormalOnlyDropSet();
            {
                // Weapons
                int[] weapons = new int[]
                {
                    ModContent.ItemType<DragonRage>(),
                    ModContent.ItemType<TheBurningSky>(),
                    ModContent.ItemType<DragonsBreath>(),
                    ModContent.ItemType<ChickenCannon>(),
                    ModContent.ItemType<PhoenixFlameBarrage>(),
                    ModContent.ItemType<YharonsKindleStaff>(), // Yharon Kindle Staff
                    ModContent.ItemType<Wrathwing>(), // Infernal Spear
                    ModContent.ItemType<FinalDawn>(),
                };
                normalOnly.Add(DropHelper.CalamityStyle(DropHelper.NormalWeaponDropRateFraction, weapons));
                normalOnly.Add(ModContent.ItemType<YharimsCrystal>(), 10);

                // Vanity
                normalOnly.Add(ModContent.ItemType<YharonMask>(), 7);
                normalOnly.Add(ModContent.ItemType<ForgottenDragonEgg>(), 10);
                normalOnly.Add(ModContent.ItemType<McNuggets>(), 10);
                normalOnly.Add(ModContent.ItemType<ThankYouPainting>(), ThankYouPainting.DropInt);

                // Materials
                normalOnly.Add(DropHelper.PerPlayer(ModContent.ItemType<YharonSoulFragment>(), 1, 25, 30));

                // Equipment
                normalOnly.Add(DropHelper.PerPlayer(ModContent.ItemType<YharimsGift>()));
                normalOnly.Add(DropHelper.PerPlayer(ModContent.ItemType<DrewsWings>()));
            }

            // Trophy (always directly from boss, never in bag)
            npcLoot.Add(ModContent.ItemType<YharonTrophy>(), 10);

            // Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ModContent.ItemType<YharonRelic>());

            // GFB Egg drop
            // He is the dragon of rebirth afterall
            var GFBOnly = npcLoot.DefineConditionalDropSet(DropHelper.GFB);
            {
                GFBOnly.Add(ModContent.ItemType<YharonEgg>());
            }

            // Lore
            npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedYharon, ModContent.ItemType<LoreYharon>(), desc: DropHelper.FirstKillText);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ModContent.ItemType<OmegaHealingPotion>();
        }
        #endregion

        #region On Kill
        public override void OnKill()
        {
            // Things that happen on killing a boss BESIDES DROPPING ITEMS go in OnKill.
            // This function is essentially equivalent to good old NPCLoot -- minus the loot, of course.
            CalamityGlobalNPC.SetNewShopVariable(new int[] { ModContent.NPCType<THIEF>() }, DownedBossSystem.downedYharon);
            CalamityGlobalNPC.SetNewBossJustDowned(NPC);

            // If Yharon has not been killed yet, notify players of Auric Ore
            if (!DownedBossSystem.downedYharon)
            {
                CalamityUtils.SpawnOre(ModContent.TileType<AuricOre>(), 2E-05, 0.75f, 0.9f, 10, 20);

                string key = "Mods.CalamityMod.Status.Progression.AuricOreText";
                Color messageColor = Color.Gold;
                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }

            // Mark Yharon as dead
            DownedBossSystem.downedYharon = true;
            CalamityNetcode.SyncWorld();

            if (Main.netMode != NetmodeID.MultiplayerClient && Main.zenithWorld)
            {
                for (int i = 0; i < 4; i++)
                {
                    int type = ModContent.ProjectileType<YharonBulletHellVortex>();
                    int damage = NPC.GetProjectileDamage(type);
                    Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero, type, damage, 0f, Main.myPlayer, 360, NPC.whoAmI);
                }
            }
        }
        #endregion

        #region On Hit Player
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<Dragonfire>(), 120, true);
        }
        #endregion

        #region HP Bar Cooldown Slot and Stats
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 2f;
            return null;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance);
            NPC.damage = (int)(NPC.damage * NPC.GetExpertDamageMultiplier());
        }
        #endregion

        #region Find Frame
        public override void FindFrame(int frameHeight)
        {
            bool idlePhases = (!startSecondAI && (NPC.ai[0] == 0f || NPC.ai[0] == 6f || NPC.ai[0] == 8f || NPC.ai[0] == 13f || NPC.ai[0] == 15f)) || (startSecondAI && (NPC.ai[0] == 5f || NPC.ai[0] < 2f));

            bool chargingOrSpawnPhases = (!startSecondAI && (NPC.ai[0] == 1f || NPC.ai[0] == 5f || NPC.ai[0] == 7f || NPC.ai[0] == 11f || NPC.ai[0] == 14f || NPC.ai[0] == 18f)) ||
                (startSecondAI && (NPC.ai[0] == 6f || NPC.ai[0] == 2f || NPC.ai[0] == 7f));

            bool projectileOrCirclePhases = (!startSecondAI && (NPC.ai[0] == 2f || NPC.ai[0] == 12f || NPC.ai[0] == 19f || NPC.ai[0] == 20f)) ||
                (startSecondAI && (NPC.ai[0] == 4f || NPC.ai[0] == 3f || NPC.ai[0] == 8f));

            bool tornadoPhase = !startSecondAI && (NPC.ai[0] == 3f || NPC.ai[0] == 9f || NPC.ai[0] == -1f || NPC.ai[0] == 16f);

            bool newPhasePhase = (!startSecondAI && (NPC.ai[0] == 4f || NPC.ai[0] == 10f || NPC.ai[0] == 17f)) || (startSecondAI && NPC.ai[0] == 9f);

            bool chargeTelegraph = (!startSecondAI && (NPC.ai[0] == 0f || NPC.ai[0] == 6f || NPC.ai[0] == 13f) && NPC.localAI[1] > 0f) ||
                (startSecondAI && NPC.ai[0] < 2f && NPC.localAI[1] > 0f);

            if (chargeTelegraph)
            {
                // Percent life remaining
                float lifeRatio = NPC.life / (float)NPC.lifeMax;

                bool doTelegraphFlightAnimation = NPC.localAI[1] < fastChargeTelegraphTime * 0.5f || NPC.localAI[1] > fastChargeTelegraphTime - (fastChargeTelegraphTime / 6f);
                bool doTelegraphRoarAnimation = NPC.localAI[1] > fastChargeTelegraphTime - fastChargeTelegraphTime * 0.4f && NPC.localAI[1] < fastChargeTelegraphTime - fastChargeTelegraphTime * 0.2f;
                bool phase4 = startSecondAI && lifeRatio <= ((CalamityWorld.death || BossRushEvent.BossRushActive) ? 0.165f : 0.11f) && (CalamityWorld.revenge || BossRushEvent.BossRushActive);
                if (doTelegraphFlightAnimation)
                {
                    NPC.frameCounter += phase4 ? 2D : 1D;
                    if (NPC.frameCounter > 5D)
                    {
                        NPC.frameCounter = 0D;
                        NPC.frame.Y += frameHeight;
                    }
                    if (NPC.frame.Y >= frameHeight * 5)
                        NPC.frame.Y = 0;
                    }
                else
                {
                    NPC.frame.Y = frameHeight * 5;
                    if (doTelegraphRoarAnimation)
                        NPC.frame.Y = frameHeight * 6;
                    }
                return;
            }

            if (idlePhases)
            {
                int num84 = 5;
                if (!startSecondAI && (NPC.ai[0] == 6f || NPC.ai[0] == 13f))
                {
                    num84 = 4;
                }
                NPC.frameCounter += 1D;
                if (NPC.frameCounter > num84)
                {
                    NPC.frameCounter = 0D;
                    NPC.frame.Y += frameHeight;
                }
                if (NPC.frame.Y >= frameHeight * 5)
                {
                    NPC.frame.Y = 0;
                }
            }

            if (chargingOrSpawnPhases)
                NPC.frame.Y = frameHeight * 5;

            if (projectileOrCirclePhases)
                NPC.frame.Y = frameHeight * 5;

            if (tornadoPhase)
            {
                int num85 = 90;
                if (NPC.ai[2] < num85 - 30 || NPC.ai[2] > num85 - 10)
                {
                    NPC.frameCounter += 1D;
                    if (NPC.frameCounter > 5D)
                    {
                        NPC.frameCounter = 0D;
                        NPC.frame.Y += frameHeight;
                    }
                    if (NPC.frame.Y >= frameHeight * 5)
                    {
                        NPC.frame.Y = 0;
                    }
                }
                else
                {
                    NPC.frame.Y = frameHeight * 5;
                    if (NPC.ai[2] > num85 - 20 && NPC.ai[2] < num85 - 15)
                    {
                        NPC.frame.Y = frameHeight * 6;
                    }
                }
            }

            if (newPhasePhase)
            {
                int num86 = 180;
                if (NPC.ai[2] < num86 - 60 || NPC.ai[2] > num86 - 20)
                {
                    NPC.frameCounter += 1D;
                    if (NPC.frameCounter > 5D)
                    {
                        NPC.frameCounter = 0D;
                        NPC.frame.Y += frameHeight;
                    }
                    if (NPC.frame.Y >= frameHeight * 5)
                    {
                        NPC.frame.Y = 0;
                    }
                }
                else
                {
                    NPC.frame.Y = frameHeight * 5;
                    if (NPC.ai[2] > num86 - 50 && NPC.ai[2] < num86 - 25)
                    {
                        NPC.frame.Y = frameHeight * 6;
                    }
                }
            }
        }
        #endregion

        #region Hit Effect
        public override void HitEffect(NPC.HitInfo hit)
        {
            // hit sound
            if (NPC.soundDelay == 0)
            {
                NPC.soundDelay = Main.rand.Next(16, 20);
                SoundEngine.PlaySound(HitSound, NPC.Center);
            }

            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                DoFireRing(300, (Main.expertMode || BossRushEvent.BossRushActive) ? 125 : 150, -1f, 0f);
                NPC.position.X = NPC.position.X + (NPC.width / 2);
                NPC.position.Y = NPC.position.Y + (NPC.height / 2);
                NPC.width = 300;
                NPC.height = 280;
                NPC.position.X = NPC.position.X - (NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (NPC.height / 2);
                for (int num621 = 0; num621 < 40; num621++)
                {
                    int num622 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 244, 0f, 0f, 100, default, 2f);
                    Main.dust[num622].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[num622].scale = 0.5f;
                        Main.dust[num622].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int num623 = 0; num623 < 70; num623++)
                {
                    int num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 244, 0f, 0f, 100, default, 3f);
                    Main.dust[num624].noGravity = true;
                    Main.dust[num624].velocity *= 5f;
                    num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 244, 0f, 0f, 100, default, 2f);
                    Main.dust[num624].velocity *= 2f;
                }

                // Turn into dust on death.
                if (NPC.life <= 0)
                    DeathAshParticle.CreateAshesFromNPC(NPC);
            }
        }
        #endregion

        #region Sound tracking
        public override void PostAI()
        {
            //Update the roar sound if it's being done.
            if (SoundEngine.TryGetActiveSound(RoarSoundSlot, out var roarSound) && roarSound.IsPlaying)
            {
                roarSound.Position = NPC.Center;
            }
        }
        #endregion
    }
}
