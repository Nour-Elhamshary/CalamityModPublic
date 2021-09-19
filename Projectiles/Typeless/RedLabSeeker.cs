using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class RedLabSeeker : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public override string Texture => "CalamityMod/Items/LabFinders/RedSeekingMechanism";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Seeking Mechanism");
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 300;
        }

        public static void Behavior(Projectile projectile, Vector2 destination, Color dustColor, ref float time)
        {
            projectile.ai[1] = 0f;

            if (time < 45f)
                projectile.velocity = Vector2.UnitY * Utils.InverseLerp(0f, 25f, time, true) * Utils.InverseLerp(30f, 25f, time, true) * -4.5f;
            else if (time < 80f)
                projectile.rotation = (projectile.AngleTo(destination) + MathHelper.PiOver2) * Utils.InverseLerp(45f, 70f, time, true);
            else
            {
                if (projectile.WithinRange(destination, 420f))
                {
                    projectile.ai[1] = 1f;
                    projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.UnitY * (float)Math.Sin((time - 80f) / 24f) * 2.5f, 0.15f);

                    if (!projectile.WithinRange(destination, 30f))
                        projectile.Center = projectile.Center.MoveTowards(destination, 10f);
                    projectile.rotation *= 0.95f;
                    projectile.Center = Vector2.Lerp(projectile.Center, destination, 0.1f);
                }
                // Release lingering dust.
                else
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 267);
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                    dust.noGravity = true;
                    dust.color = dustColor;
                    dust.scale = 1.6f;
                    dust.fadeIn = 1.45f;
                }
            }

            if (time > 80f && time < 120f)
                projectile.velocity = (projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Utils.InverseLerp(80f, 105f, time, true) * 25f;

            // Reset afterimages before the charge.
            if (time < 80f)
                projectile.oldPos = new Vector2[projectile.oldPos.Length];

            Lighting.AddLight(projectile.Center, Vector3.One * 0.8f);
            time++;
        }

        public override void AI() => Behavior(projectile, CalamityWorld.HellLabCenter, Color.Red, ref Time);

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(16f, 16f), 267);
                dust.color = Color.Red;
                dust.scale = Main.rand.NextFloat(0.95f, 1.25f);
                dust.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
                dust.velocity.Y -= 1.5f;
                dust.fadeIn = 1.2f;
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            if (Time > 80f && projectile.ai[1] == 0f)
            {
                for (int i = 0; i < 24; i += 2)
                {
                    Vector2 drawPosition = projectile.Center - projectile.velocity.SafeNormalize(Vector2.Zero) * i * Utils.InverseLerp(80f, 125f, Time, true) * 25f - Main.screenPosition;
                    spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(lightColor) * (1f - i / 24f), projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }
    }
}