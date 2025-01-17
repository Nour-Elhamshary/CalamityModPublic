﻿using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
namespace CalamityMod.Projectiles.Magic
{
    public class InfernalBlade2 : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/Magic/InfernalBlade";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.penetrate = 2;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            DelegateMethods.v3_1 = new Vector3(0.6f, 1f, 1f) * 0.2f;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * 10f, 8f, DelegateMethods.CastLightOpen);
            if (Projectile.alpha > 0)
            {
                SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
                Projectile.alpha = 0;
                Projectile.scale = 1.1f;
                Projectile.frame = Main.rand.Next(14);
                float num98 = 16f;
                int num99 = 0;
                while ((float)num99 < num98)
                {
                    Vector2 vector11 = Vector2.UnitX * 0f;
                    vector11 += -Vector2.UnitY.RotatedBy((double)((float)num99 * (6.28318548f / num98)), default) * new Vector2(1f, 4f);
                    vector11 = vector11.RotatedBy((double)Projectile.velocity.ToRotation(), default);
                    int num100 = Dust.NewDust(Projectile.Center, 0, 0, 182, 0f, 0f, 0, default, 1f);
                    Main.dust[num100].scale = 1.5f;
                    Main.dust[num100].noGravity = true;
                    Main.dust[num100].position = Projectile.Center + vector11;
                    Main.dust[num100].velocity = Projectile.velocity * 0f + vector11.SafeNormalize(Vector2.UnitY) * 1f;
                    num99++;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + 0.7853982f;
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            int num487 = Main.rand.Next(4, 10);
            for (int num488 = 0; num488 < num487; num488++)
            {
                int num489 = Dust.NewDust(Projectile.Center, 0, 0, 182, 0f, 0f, 100, default, 1f);
                Main.dust[num489].velocity *= 1.6f;
                Dust expr_FEDF_cp_0 = Main.dust[num489];
                expr_FEDF_cp_0.velocity.Y -= 1f;
                Main.dust[num489].velocity += -Projectile.velocity * (Main.rand.NextFloat() * 2f - 1f) * 0.5f;
                Main.dust[num489].scale = 2f;
                Main.dust[num489].fadeIn = 0.5f;
                Main.dust[num489].noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 50, 50, 0);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Color color25 = Lighting.GetColor((int)((double)Projectile.position.X + (double)Projectile.width * 0.5) / 16, (int)(((double)Projectile.position.Y + (double)Projectile.height * 0.5) / 16.0));
            Texture2D texture2D3 = ModContent.Request<Texture2D>(Texture).Value;
            int num155 = ModContent.Request<Texture2D>(Texture).Value.Height / Main.projFrames[Projectile.type];
            int y3 = num155 * Projectile.frame;
            Rectangle rectangle = new Rectangle(0, y3, texture2D3.Width, num155);
            Vector2 origin2 = rectangle.Size() / 2f;
            float num158 = 0f;
            int num156 = 3;
            int num157 = 1;
            float value4 = 8f;
            rectangle = new Rectangle(38 * Projectile.frame, 0, 38, 38);
            origin2 = rectangle.Size() / 2f;
            for (int num159 = 1; num159 < num156; num159 += num157)
            {
                Color color26 = color25;
                color26 = Projectile.GetAlpha(color26);
                color26 *= (float)(num156 - num159) / ((float)ProjectileID.Sets.TrailCacheLength[Projectile.type] * 1.5f);
                Vector2 value5 = Projectile.oldPos[num159];
                float num160 = Projectile.rotation;
                SpriteEffects effects = spriteEffects;
                if (ProjectileID.Sets.TrailingMode[Projectile.type] == 2)
                {
                    num160 = Projectile.oldRot[num159];
                    effects = (Projectile.oldSpriteDirection[num159] == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                }
                Main.spriteBatch.Draw(texture2D3, value5 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, num160 + Projectile.rotation * num158 * (float)(num159 - 1) * (float)-(float)spriteEffects.HasFlag(SpriteEffects.FlipHorizontally).ToDirectionInt(), origin2, MathHelper.Lerp(Projectile.scale, value4, (float)num159 / 15f), effects, 0f);
            }
            Color color28 = Projectile.GetAlpha(color25);
            Main.spriteBatch.Draw(texture2D3, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color28, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
        }
    }
}
