﻿using CalamityMod.BiomeManagers;
using CalamityMod.Items.Critters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.DraedonLabThings
{
    public class Androomba : ModNPC
    {
        public SoundStyle HurrySound = new("CalamityMod/Sounds/Custom/WulfrumDroidHurry", 2) { PitchVariance = 0.3f };
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Androomba");
            Main.npcFrameCount[NPC.type] = 22;
            NPCID.Sets.CountsAsCritter[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 40;
            NPC.height = 16;
            NPC.lifeMax = 80;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.chaseable = false;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath44;
            NPC.catchItem = (short)ModContent.ItemType<RepairUnitItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<ArsenalLabBiome>().Type };
            DrawOffsetY = -17;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {

				// Will move to localization whenever that is cleaned up.
				new FlavorTextBestiaryInfoElement("Slowly moving back and forth, these contraptions endlessly operate with the fruitless goal of cleaning the long abandoned labs.")
            });
        }

        public override void AI()
        {
            // Gravity
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y + 0.4f, -15f, 15f);
            NPC.spriteDirection = (int)NPC.ai[2];
            switch (NPC.ai[0])
            {
                // Idle
                case 0:
                    {
                        if (NPC.ai[1] == 0)
                        {
                            NPC.ai[2] = Main.rand.NextBool(2) ? -1 : 1;
                        }
                        NPC.ai[1]++;
                        NPC.velocity.X = NPC.ai[2] * 2;
                        //if (NPC.collideX)
                        if (!Collision.CanHit(NPC.Center - Vector2.UnitX * NPC.ai[2] * 8f, 2, 2, NPC.Center + Vector2.UnitX * NPC.ai[2] * 32f, 8, 8))
                        {
                            NPC.ai[2] *= -1;
                        }
                        // If she sees someone, start running
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[i].position, Main.player[i].width, Main.player[i].height) && NPC.ai[1] > 300)
                            {
                                ChangeAI(1);
                            }
                        }
                    }
                    break;
                // Surprise and explode animations
                case 1:
                case 3:
                    {
                        NPC.velocity = Vector2.Zero;
                        if (NPC.ai[1] == 0)
                        {
                            if (NPC.ai[0] == 1)
                            {
                                SoundEngine.PlaySound(HurrySound, NPC.Center);
                            }
                            if (NPC.ai[0] == 3)
                            {
                                SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);
                            }
                        }
                        NPC.ai[1]++;
                    }
                    break;
                // Run!!!
                case 2:
                    {
                        NPC.ai[1]++;
                        NPC.velocity.X = NPC.ai[2] * 4;
                        if (NPC.collideX)
                        {
                            ChangeAI(3);
                        }
                        else if (NPC.ai[1] > 480) // If the roomba is somehow still alive after 8 seconds and there are no players, calm down
                        {
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                if (!Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[i].position, Main.player[i].width, Main.player[i].height))
                                {
                                    ChangeAI(0);
                                }
                            }
                        }
                    }
                    break;
                // Fatal eror (corpse)
                case 4:
                    {
                        NPC.velocity = Vector2.Zero;
                        NPC.ai[1]++;
                        if (NPC.ai[1] > 1200)
                        {
                            NPC.active = false;
                        }
                        NPC.damage = 10; // Stay aware from fire, kids
                    }
                    break;
            }
        }

        public void ChangeAI(int phase)
        {
            NPC.ai[0] = phase;
            NPC.ai[1] = 0;
        }

        public override bool? CanBeHitByItem(Player player, Item item) => null;

        public override bool? CanBeHitByProjectile(Projectile projectile) => null;

        public override void FindFrame(int frameHeight)
        {
            /*
            -Frame 0: Idle
		    -Frames 1-4: Moving
		    -Frames 5-6: Surprised animation
		    -Frames 7-12: Escaping loop
		    -Frames 13-17: Exploding animation
		    -Frames 18-21: Debris loop 
            */
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter > 6.0)
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y += frameHeight;
            }

            // Idle
            if (NPC.ai[0] == 0 || NPC.IsABestiaryIconDummy)
            {
                if (NPC.frame.Y > frameHeight * 4)
                {
                    NPC.frameCounter = 0.0;
                    NPC.frame.Y = 1;
                }
            }
            // Suprise!
            else if (NPC.ai[0] == 1)
            {
                if (NPC.frame.Y < frameHeight * 5)
                {
                    NPC.frameCounter = 0.0;
                    NPC.frame.Y = frameHeight * 5;
                }
                if (NPC.frame.Y > frameHeight * 6)
                {
                    ChangeAI(2);
                }
            }
            // Run
            else if (NPC.ai[0] == 2)
            {
                if (NPC.frame.Y < frameHeight * 7 || NPC.frame.Y > frameHeight * 12)
                {
                    NPC.frameCounter = 0.0;
                    NPC.frame.Y = frameHeight * 7;
                }
            }
            // Explode
            else if (NPC.ai[0] == 3)
            {
                if (NPC.frame.Y < frameHeight * 13)
                {
                    NPC.frameCounter = 0.0;
                    NPC.frame.Y = frameHeight * 13;
                }
                if (NPC.frame.Y > frameHeight * 17)
                {
                    ChangeAI(4);
                }
            }
            // Fire
            else
            {
                if (NPC.frame.Y < frameHeight * 18 || NPC.frame.Y > frameHeight * 21)
                {
                    NPC.frameCounter = 0.0;
                    NPC.frame.Y = frameHeight * 18;
                }
            }
            if (NPC.frame.Y >= frameHeight * 15 && NPC.active)
            {
                Lighting.AddLight(NPC.Center, 0.8f, 0.03f, 0.1f);
            }
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int i = 0; i < 6; i++)
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 226);
        }

        public override void OnCaughtBy(Player player, Item item, bool failed)
        {
            // You can't catch what's dead
            if (NPC.ai[0] >= 3)
            {
                failed = true;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D critterTexture = TextureAssets.Npc[NPC.type].Value;
            Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/NPCs/DraedonLabThings/Androomba_Glow").Value;
            Vector2 drawPosition = NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY;
            drawPosition.Y += DrawOffsetY;
            SpriteEffects direction = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(critterTexture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, direction, 0f);
            spriteBatch.Draw(glowmask, drawPosition, NPC.frame, NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, direction, 0f);
            return false;
        }
    }
}
