using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class RancorMagicCircle : ModProjectile
    {
        public Player Owner => Main.player[projectile.owner];
        public ref float Time => ref projectile.ai[0];
        public ref float PulseLoopSoundSlot => ref projectile.localAI[0];
        public ActiveSound PulseLoopSound => Main.GetActiveSound(SlotId.FromFloat(PulseLoopSoundSlot));
        public float ChargeupCompletion => MathHelper.Clamp(Time / ChargeupTime, 0f, 1f);
        public const int ChargeupTime = 240;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Magic Circle");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 114;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.magic = true;
            projectile.ignoreWater = true;
            projectile.timeLeft = 90000;
        }

        public override void AI()
        {
            // If the owner is no longer able to cast the circle, kill it.
            if (!Owner.channel || Owner.noItems || Owner.CCed)
            {
                projectile.Kill();
                return;
            }

            if (Time >= 1f && Owner.ownedProjectileCounts[ModContent.ProjectileType<RancorHoldout>()] <= 0)
            {
                projectile.Kill();
                return;
            }

            // Adjust visual values such as scale and opacity when charging.
            AdjustVisualValues();

            // Update aim.
            UpdateAim();

            // Decide where to position the magic circle.
            Vector2 circlePointDirection = projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction);
            projectile.Center = Owner.Center + Vector2.UnitX * Owner.direction * 30f + circlePointDirection * projectile.scale * 56f;

            // Adjust the owner's direction.
            Owner.ChangeDir(projectile.direction);

            // Do animation stuff.
            DoPrettyDustEffects();

            // Handle charge stuff.
            if (Time < ChargeupTime)
                HandleChargeEffects();

            // Create an idle ominous sound once the laser has appeared.
            else if (Main.GetActiveSound(SlotId.FromFloat(PulseLoopSoundSlot)) is null)
                PulseLoopSoundSlot = Main.PlayTrackedSound(SoundID.DD2_EtherianPortalIdleLoop, projectile.Center).ToFloat();

            // Make a cast sound effect soon after the circle appears.
            if (Time == 15f)
                Main.PlaySound(SoundID.Item117, projectile.Center);

            Time++;
        }

        public void AdjustVisualValues()
        {
            projectile.scale = Utils.InverseLerp(0f, 35f, Time, true) * 1.4f;
            projectile.Opacity = (float)Math.Pow(projectile.scale / 1.4f, 2D);
            projectile.rotation -= MathHelper.ToRadians(projectile.scale * 4f);
        }

        public void UpdateAim()
        {
            // Only execute the aiming code for the owner since Main.MouseWorld is a client-side variable.
            if (Main.myPlayer != projectile.owner)
                return;

            Vector2 idealDirection = Owner.SafeDirectionTo(Main.MouseWorld, Vector2.UnitX * Owner.direction);
            Vector2 newAimDirection = projectile.velocity.MoveTowards(idealDirection, 0.05f);

            // Sync if the direction is different from the old one.
            // Spam caps are ignored due to the frequency of this happening.
            if (newAimDirection != projectile.velocity)
            {
                projectile.netUpdate = true;
                projectile.netSpam = 0;
            }

            projectile.velocity = newAimDirection;
            projectile.direction = (projectile.velocity.X > 0f).ToDirectionInt();
        }

        public void DoPrettyDustEffects()
        {
            int dustSpawnChance = (int)MathHelper.SmoothStep(16f, 4f, ChargeupCompletion);
            for (int i = 0; i < 2; i++)
            {
                if (!Main.rand.NextBool(dustSpawnChance))
                    continue;

                float dustSpawnOffsetFactor = Main.rand.NextFloat(projectile.width * 0.375f, projectile.width * 0.485f);
                Vector2 dustSpawnOffsetDirection = Main.rand.NextVector2CircularEdge(0.5f, 1f).RotatedBy(projectile.velocity.ToRotation());
                Vector2 dustSpawnOffset = dustSpawnOffsetDirection * dustSpawnOffsetFactor;
                Vector2 dustVelocity = (-dustSpawnOffset.SafeNormalize(Vector2.UnitY)).RotatedBy(MathHelper.PiOver2 * Main.rand.NextFloatDirection());
                dustVelocity *= Main.rand.NextFloat(2f, 6f);

                Dust magic = Dust.NewDustPerfect(projectile.Center + dustSpawnOffset, 264);
                magic.color = Color.Lerp(Color.Red, Color.Blue, Main.rand.NextFloat());
                magic.velocity = dustVelocity;
                magic.scale *= Main.rand.NextFloat(1f, 1.4f);
                magic.noLight = true;
                magic.noGravity = true;
            }
        }

        public void HandleChargeEffects()
        {
            // Create dust that fires parallel to the direction of the circle.
            if (Main.rand.NextBool(3))
            {
                float dustSpeed = MathHelper.Lerp(3.5f, 8f, ChargeupCompletion) * Main.rand.NextFloat(0.65f, 1f);
                float dustSpawnOffsetFactor = Main.rand.NextFloat(projectile.width * 0.375f, projectile.width * 0.485f);
                Vector2 dustVelocity = projectile.velocity * dustSpeed;
                Vector2 dustSpawnOffsetDirection = Main.rand.NextVector2CircularEdge(0.5f, 1f).RotatedBy(projectile.velocity.ToRotation());
                Vector2 dustSpawnOffset = dustSpawnOffsetDirection * dustSpawnOffsetFactor;

                Dust magic = Dust.NewDustPerfect(projectile.Center + dustSpawnOffset, 264);
                magic.color = Color.Lerp(Color.Red, Color.Blue, Main.rand.NextFloat());
                magic.velocity = dustVelocity;
                magic.scale *= Main.rand.NextFloat(1f, 1.05f + ChargeupCompletion * 0.55f);
                magic.noLight = true;
                magic.noGravity = true;
            }

            // Create the laser once the charge animation is complete.
            if (Time == ChargeupTime - 1f)
            {
                // Play a laserbeam deathray sound. Should probably be replaced some day
                Main.PlaySound(SoundID.Zombie, projectile.Center, 104);

                if (Main.myPlayer == projectile.owner)
                    Projectile.NewProjectile(projectile.Center, projectile.velocity, ModContent.ProjectileType<RancorLaserbeam>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.identity);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D outerCircleTexture = ModContent.GetTexture(Texture);
            Texture2D outerCircleGlowmask = ModContent.GetTexture(Texture + "Glowmask");
            Texture2D innerCircleTexture = ModContent.GetTexture(Texture + "Inner");
            Texture2D innerCircleGlowmask = ModContent.GetTexture(Texture + "InnerGlowmask");
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            float directionRotation = projectile.velocity.ToRotation();
            Color startingColor = Color.Red;
            Color endingColor = Color.Blue;

            void restartShader(Texture2D texture, float opacity, float circularRotation, BlendState blendMode)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, blendMode, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                CalamityUtils.CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix);

                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseColor(startingColor);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSecondaryColor(endingColor);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSaturation(directionRotation);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseOpacity(opacity);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uDirection"].SetValue((float)projectile.direction);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uCircularRotation"].SetValue(circularRotation);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uImageSize0"].SetValue(texture.Size());
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["overallImageSize"].SetValue(outerCircleTexture.Size());
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Apply();
            }

            restartShader(outerCircleGlowmask, projectile.Opacity, projectile.rotation, BlendState.Additive);
            spriteBatch.Draw(outerCircleGlowmask, drawPosition, null, Color.White, 0f, outerCircleGlowmask.Size() * 0.5f, projectile.scale * 1.075f, SpriteEffects.None, 0f);

            restartShader(outerCircleTexture, projectile.Opacity * 0.7f, projectile.rotation, BlendState.AlphaBlend);
            spriteBatch.Draw(outerCircleTexture, drawPosition, null, Color.White, 0f, outerCircleTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);

            restartShader(innerCircleGlowmask, projectile.Opacity * 0.5f, 0f, BlendState.Additive);
            spriteBatch.Draw(innerCircleGlowmask, drawPosition, null, Color.White, 0f, innerCircleGlowmask.Size() * 0.5f, projectile.scale * 1.075f, SpriteEffects.None, 0f);

            restartShader(innerCircleTexture, projectile.Opacity * 0.7f, 0f, BlendState.AlphaBlend);
            spriteBatch.Draw(innerCircleTexture, drawPosition, null, Color.White, 0f, innerCircleTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.ExitShaderRegion();

            return false;
        }

        public override void Kill(int timeLeft) => PulseLoopSound?.Stop();

        public override bool ShouldUpdatePosition() => false;

        public override bool CanDamage() => false;
    }
}