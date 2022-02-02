﻿using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.DataStructures;
using CalamityMod.Particles;
using CalamityMod.Items.Weapons.Melee;
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static CalamityMod.CalamityUtils;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Melee
{
    public class SwordsmithsPride : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Melee/TrueBiomeBlade_SwordsmithsPride";
        private bool initialized = false;
        Vector2 direction = Vector2.Zero;
        public ref float CurrentState => ref projectile.ai[0];
        public Player Owner => Main.player[projectile.owner];
        private bool OwnerCanShoot => Owner.channel && !Owner.noItems && !Owner.CCed;
        public const float throwOutTime = 90f;
        public const float throwOutDistance = 440f;

        public static float snapPoint = 0.45f;
        public float snapTimer => (throwTimer / throwOutTime) < snapPoint ? 0 : ((throwTimer / throwOutTime) - snapPoint) / (1f - snapPoint);
        public static float retractionPoint = 0.6f;
        public float retractionTimer => (throwTimer / throwOutTime) < retractionPoint ? 0 : ((throwTimer / throwOutTime) - retractionPoint) / (1f - retractionPoint);
        public ref float Empowerment => ref projectile.ai[1];
        public float OverEmpowerment = 0f; //Used to keep cooldowns working when the spin is full
        public ref float hasMadeSound => ref projectile.localAI[0];
        public ref float hasMadeChargeSound => ref projectile.localAI[1];

        public const float maxEmpowerment = 600f;
        public float throwTimer => throwOutTime - projectile.timeLeft;


        public float AngleReset = 0f;
        public bool CanDirectFire = true;
        public Particle smear;
        public Particle sightLine;
        public NPC lastTarget;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Swordsmith's Pride");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            projectile.melee = true;
            projectile.width = projectile.height = 74;
            projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.extraUpdates = 1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = OmegaBiomeBlade.WhirlwindAttunement_LocalIFrames;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            float bladeLenght = 140 * projectile.scale;
            float bladeWidth = 25 * projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + (direction * bladeLenght), bladeWidth, ref collisionPoint);
        }


        public CurveSegment launch = new CurveSegment(EasingType.CircOut, 0f, 0f, 1f, 4);
        public CurveSegment hold = new CurveSegment(EasingType.Linear, snapPoint, 1f, 0f);
        public CurveSegment retract = new CurveSegment(EasingType.PolyInOut, retractionPoint, 1f, -1.05f, 3);
        internal float ThrowCurve() => PiecewiseAnimation((throwOutTime - projectile.timeLeft) / throwOutTime, new CurveSegment[] { launch, hold, retract });

        public override void AI()
        {
            if (!initialized) //Initialization. Here its litterally just playing a sound tho lmfao
            {
                Main.PlaySound(SoundID.Item90, projectile.Center);
                projectile.velocity = Vector2.Zero;
                direction = Owner.DirectionTo(Main.MouseWorld);
                direction.Normalize();
                initialized = true;
            }

            projectile.rotation = direction.ToRotation(); //Only done for afterimages

            if (!OwnerCanShoot)
            {
                if (CurrentState == 2f || (CurrentState == 0f && Empowerment / maxEmpowerment < 0.5))
                {
                    Main.PlaySound(SoundID.Item77, projectile.Center);
                    projectile.Kill();
                    return;
                }

                else if (CurrentState == 0f)
                {
                    CurrentState = 1f;
                    Main.PlaySound(SoundID.Item80, projectile.Center);
                    direction = Owner.DirectionTo(Main.MouseWorld);
                    //PARTICLES LOTS OF PARTICLES LOTS OF SPARKLES YES YES MH YES YES 
                    for (int i = 0; i <= 8; i++)
                    {
                        float variation = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                        float strength = (float)Math.Sin(variation * 2f + MathHelper.PiOver2);
                        Particle Sparkle = new CritSpark(projectile.Center, Owner.velocity + direction.RotatedBy(variation) * (1 + strength) * 2f * Main.rand.NextFloat(7.5f, 20f), Color.White, Color.HotPink, 2f + Main.rand.NextFloat(0f, 1.5f), 20 + Main.rand.Next(30), 1, 2f);
                        GeneralParticleHandler.SpawnParticle(Sparkle);
                    }
                    for (int i = 0; i <= 8; i++)
                    {
                        float variation = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                        float strength = (float)Math.Sin(variation * 2f + MathHelper.PiOver2);
                        Particle Sparkle = new CritSpark(projectile.Center, Owner.velocity + direction.RotatedBy(variation) * (1 + strength) * Main.rand.NextFloat(7.5f, 20f), Color.White, Color.GreenYellow, 2f + Main.rand.NextFloat(0f, 1.5f), 20 + Main.rand.Next(30), 1, 2f);
                        GeneralParticleHandler.SpawnParticle(Sparkle);
                    }
                }
            }

            if (CurrentState == 0f)
            {

                if (hasMadeChargeSound == 0f && Empowerment / maxEmpowerment >= 0.5)
                {
                    hasMadeChargeSound = 1f;
                    Main.PlaySound(SoundID.Item76, projectile.Center);
                }

                float rotation = direction.ToRotation();
                if (rotation > -MathHelper.PiOver2 - MathHelper.PiOver4 && rotation < -MathHelper.PiOver2 + MathHelper.PiOver4 && hasMadeSound == 1f)
                    hasMadeSound = 0f;

                else if (rotation > MathHelper.PiOver2 - MathHelper.PiOver4 && rotation < MathHelper.PiOver2 + MathHelper.PiOver4 && hasMadeSound == 0f)
                {
                    CanDirectFire = true;
                    hasMadeSound = 1f;
                    Main.PlaySound(SoundID.Item71, projectile.Center);
                }


                if (Empowerment / maxEmpowerment >= 0.5 && (Empowerment + OverEmpowerment) % 30 == 29 && Owner.whoAmI == Main.myPlayer)
                {
                    Vector2 shotDirection = Main.rand.NextVector2CircularEdge(15f, 15f);
                    if (lastTarget != null && lastTarget.active) //If you've got an actual target, angle your shots towards them
                    {
                        shotDirection = (shotDirection.ToRotation().AngleTowards(Owner.AngleTo(lastTarget.Center), MathHelper.PiOver2)).ToRotationVector2() * 15f;
                    }
                    Projectile.NewProjectile(Owner.Center, shotDirection, ProjectileType<SwordsmithsPrideBeam>(), (int)(projectile.damage * OmegaBiomeBlade.WhirlwindAttunement_BeamDamageReduction), 0f, Owner.whoAmI);

                }


                if (Empowerment / maxEmpowerment >= 0.75)
                {
                    Color currentColor = Color.Lerp(Color.HotPink, Color.GreenYellow, (float)Math.Sin(Main.GlobalTime * 2f)) * (((Empowerment / maxEmpowerment) - 0.75f) / 0.25f * 0.8f);
                    if (smear == null)
                    {
                        smear = new CircularSmearVFX(Owner.Center, Color.HotPink, direction.ToRotation(), projectile.scale * 1.5f);
                        GeneralParticleHandler.SpawnParticle(smear);
                    }
                    //Update the variables of the smear
                    else
                    {
                        smear.Rotation = direction.ToRotation() + MathHelper.PiOver2;
                        smear.Time = 0;
                        smear.Position = Owner.Center;
                        smear.Scale = projectile.scale * 1.9f;
                        smear.Color = currentColor;
                    }

                    if (sightLine == null)
                    {
                        sightLine = new LineVFX(Owner.Center, Owner.DirectionTo(Main.MouseWorld), 0.2f, Color.HotPink, false);
                        GeneralParticleHandler.SpawnParticle(sightLine);
                    }
                    else
                    {
                        sightLine.Position = Owner.Center + Owner.DirectionTo(Main.MouseWorld) * projectile.scale * 1.88f * 40;
                        (sightLine as LineVFX).LineVector = Owner.DirectionTo(Main.MouseWorld) * projectile.scale * 1.88f * 38f;
                        sightLine.Scale = 0.2f;
                        sightLine.Time = 0;
                        sightLine.Color = currentColor * 0.7f;
                    }

                    float rotationAdjusted = MathHelper.WrapAngle(projectile.rotation) + MathHelper.Pi;
                    float mouseAngleAdjusted = MathHelper.WrapAngle(Owner.DirectionTo(Main.MouseWorld).ToRotation()) + MathHelper.Pi;
                    float deltaAngleShoot = Math.Abs(MathHelper.WrapAngle(rotationAdjusted - mouseAngleAdjusted));

                    if (CanDirectFire && deltaAngleShoot < 0.1f)
                    {
                        Particle Blink = new GenericSparkle(Owner.Center + Owner.DirectionTo(Main.MouseWorld) * projectile.scale * 1.88f * 78f, Owner.velocity, Color.White, currentColor, 1.5f, 10, 0.1f, 3f);
                        GeneralParticleHandler.SpawnParticle(Blink);

                        if (Owner.whoAmI == Main.myPlayer)
                        {
                            Projectile.NewProjectile(Owner.Center, Owner.DirectionTo(Main.MouseWorld) * 15f, ProjectileType<SwordsmithsPrideBeam>(), (int)(projectile.damage * OmegaBiomeBlade.WhirlwindAttunement_BeamDamageReduction), 0f, Owner.whoAmI);
                        }
                        CanDirectFire = false;
                        AngleReset = Owner.DirectionTo(Main.MouseWorld).ToRotation();
                    }


                    if (Main.rand.NextBool())
                    {
                        float maxDistance = projectile.scale * 1.9f * 78f;
                        Vector2 distance = Main.rand.NextVector2Circular(maxDistance, maxDistance);
                        Vector2 angularVelocity = Vector2.Normalize(distance.RotatedBy(MathHelper.PiOver2)) * 2f * (1f + distance.Length() / 15f);
                        Particle glitter = new CritSpark(Owner.Center + distance, Owner.velocity + angularVelocity, Color.White, currentColor, 1f + 1 * (distance.Length() / maxDistance), 10, 0.05f, 3f);
                        GeneralParticleHandler.SpawnParticle(glitter);
                    }
                }


                //Manage position and rotation
                projectile.scale = 1 + Empowerment / maxEmpowerment * 1.5f;

                direction = direction.RotatedBy(MathHelper.Clamp(Empowerment / maxEmpowerment, 0.4f, 1f) * MathHelper.PiOver4 * 0.20f);
                direction.Normalize();
                projectile.rotation = direction.ToRotation();
                projectile.Center = Owner.Center + (direction * projectile.scale * 10);
                projectile.timeLeft = (int)throwOutTime + 1;
                Empowerment++;
                if (Empowerment > maxEmpowerment)
                {
                    Empowerment = maxEmpowerment;
                    OverEmpowerment++;
                }
            }

            if (CurrentState == 1f)
            {
                projectile.Center = Owner.Center + (direction * projectile.scale * 10) + (direction * throwOutDistance * ThrowCurve());
                projectile.scale = (1 + Empowerment / maxEmpowerment * 1.5f) * MathHelper.Clamp(1 - retractionTimer, 0.3f, 1f);
            }

            //Make the owner look like theyre holding the sword bla bla
            Owner.heldProj = projectile.whoAmI;
            Owner.direction = Math.Sign(direction.X);
            Owner.itemRotation = direction.ToRotation();
            if (Owner.direction != 1)
            {
                Owner.itemRotation -= 3.14f;
            }
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int)(damage * (OmegaBiomeBlade.WhirlwindAttunement_BaseDamageReduction + (OmegaBiomeBlade.WhirlwindAttunement_FullChargeDamageBoost * Empowerment / maxEmpowerment)));

            if (CurrentState != 1)
            {
                if (Owner.HeldItem.modItem is OmegaBiomeBlade sword && Main.rand.NextFloat() <= OmegaBiomeBlade.WhirlwindAttunement_WhirlwindProc)
                    sword.OnHitProc = true;
                return;
            }

            if (Owner.HeldItem.modItem is OmegaBiomeBlade blade && Main.rand.NextFloat() <= OmegaBiomeBlade.WhirlwindAttunement_SwordThrowProc)
                blade.OnHitProc = true;

            lastTarget = target;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ProjectileType<PurityProjectionSigil>() && proj.owner == Owner.whoAmI)
                {
                    //Reset the timeleft on the sigil & give it its new target (or the same, it doesnt matter really.
                    proj.ai[0] = target.whoAmI;
                    proj.timeLeft = OmegaBiomeBlade.WhirlwindAttunement_SigilTime;
                    return;
                }
            }
            Projectile sigil = Projectile.NewProjectileDirect(target.Center, Vector2.Zero, ProjectileType<PurityProjectionSigil>(), 0, 0, Owner.whoAmI, target.whoAmI);
            sigil.timeLeft = OmegaBiomeBlade.WhirlwindAttunement_SigilTime;
        }

        public override void Kill(int timeLeft)
        {
            if (smear != null)
                smear.Kill();
            if (sightLine != null)
                sightLine.Kill();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D handle = GetTexture("CalamityMod/Items/Weapons/Melee/OmegaBiomeBlade");
            Texture2D blade = GetTexture("CalamityMod/Projectiles/Melee/TrueBiomeBlade_SwordsmithsPride");

            float drawAngle = direction.ToRotation();
            float drawRotation = drawAngle + MathHelper.PiOver4;

            Vector2 drawOrigin = new Vector2(0f, handle.Height);
            Vector2 drawOffset = projectile.Center - Main.screenPosition;

            spriteBatch.Draw(handle, drawOffset, null, lightColor, drawRotation, drawOrigin, projectile.scale, 0f, 0f);

            //Turn on additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            //Update the parameters
            drawOrigin = new Vector2(0f, blade.Height);

            //Afterimages
            if (CalamityConfig.Instance.Afterimages && CurrentState == 0f && Empowerment / maxEmpowerment > 0.4f)
            {
                for (int i = 0; i < projectile.oldRot.Length; ++i)
                {
                    Color color = projectile.GetAlpha(lightColor) * (1f - (i / (float)projectile.oldRot.Length));
                    float afterimageRotation = projectile.oldRot[i] + MathHelper.PiOver4;
                    Main.spriteBatch.Draw(blade, drawOffset, null, color * MathHelper.Lerp(0f, 0.5f, MathHelper.Clamp((Empowerment / maxEmpowerment - 0.4f) / 0.1f, 0f, 1f)), afterimageRotation, drawOrigin, projectile.scale - 0.2f * ((i / (float)projectile.oldRot.Length)), 0f, 0f);
                }
            }

            //Don't draw the glowing blade after the retraction
            float opacityFade = CurrentState == 0f ? 1f : 1f - retractionTimer;
            //Add basic tint (my beloved) during the snap
            if (snapTimer > 0 && retractionTimer <= 0)
            {
                GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(MathHelper.Clamp(0.8f - snapTimer, 0f, 1f));
                GameShaders.Misc["CalamityMod:BasicTint"].UseColor(Color.White);
                GameShaders.Misc["CalamityMod:BasicTint"].Apply();
            }

            spriteBatch.Draw(blade, drawOffset, null, Color.Lerp(Color.White, lightColor, 0.5f) * 0.9f * opacityFade, drawRotation, drawOrigin, projectile.scale, 0f, 0f);

            if (CurrentState == 1f && snapTimer > 0)
            {
                drawChain(spriteBatch, snapTimer, retractionTimer);
            }

            //Back to normal
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public void drawChain(SpriteBatch spriteBatch, float snapProgress, float retractProgress)
        {
            Texture2D chainTex = GetTexture("CalamityMod/Projectiles/Melee/MendedBiomeBlade_HeavensMightChain");

            float opacity = retractProgress < 0.5 ? 1 : (retractProgress - 0.5f) / 0.5f;

            Vector2 Shake = retractProgress > 0 ? Vector2.Zero : Vector2.One.RotatedByRandom(MathHelper.TwoPi) * (1f - snapProgress) * 10f;

            int dist = (int)Vector2.Distance(Owner.Center, projectile.Center) / 16;
            Vector2[] Nodes = new Vector2[dist + 1];
            Nodes[0] = Owner.Center;
            Nodes[dist] = projectile.Center;

            for (int i = 1; i < dist + 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0 + 18 * (i % 2), 12, 18);
                Vector2 positionAlongLine = Vector2.Lerp(Owner.Center, projectile.Center, i / (float)dist); //Get the position of the segment along the line, as if it were a flat line
                Nodes[i] = positionAlongLine + Shake * (float)Math.Sin(i / (float)dist * MathHelper.Pi);

                float rotation = (Nodes[i] - Nodes[i - 1]).ToRotation() - MathHelper.PiOver2; //Calculate rotation based on direction from last point
                float yScale = Vector2.Distance(Nodes[i], Nodes[i - 1]) / frame.Height; //Calculate how much to squash/stretch for smooth chain based on distance between points
                Vector2 scale = new Vector2(1, yScale);

                Color chainLightColor = Lighting.GetColor((int)Nodes[i].X / 16, (int)Nodes[i].Y / 16); //Lighting of the position of the chain segment

                Vector2 origin = new Vector2(frame.Width / 2, frame.Height); //Draw from center bottom of texture
                spriteBatch.Draw(chainTex, Nodes[i] - Main.screenPosition, frame, chainLightColor * opacity * 0.7f, rotation, origin, scale, SpriteEffects.None, 0);
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(initialized);
            writer.WriteVector2(direction);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initialized = reader.ReadBoolean();
            direction = reader.ReadVector2();
        }
    }
}