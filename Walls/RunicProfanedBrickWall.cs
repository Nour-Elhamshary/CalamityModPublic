using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Walls
{
    public class RunicProfanedBrickWall : ModWall
    {
        public override void SetDefaults()
        {
            Main.wallHouse[Type] = true;
            dustType = ModContent.DustType<Sparkle>();
            drop = ModContent.ItemType<RunicProfanedBrickWall>();
            AddMapEntry(new Color(43, 22, 27));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, 205, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, ModContent.DustType<ProfanedTileRock>(), 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Texture2D sprite = mod.GetTexture("Walls/RunicProfanedBrickWall");
            Color lightColor = GetWallColour(i, j);
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            zero -= new Vector2(8, 8);
            Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;
            int[] sheetOffset = CreatePattern(i, j);
            spriteBatch.Draw
                (
                    sprite,
                    drawOffset,
                    new Rectangle(sheetOffset[0] + Main.tile[i, j].wallFrameX(), sheetOffset[1] + Main.tile[i, j].wallFrameY(), 32, 32),
                    lightColor,
                    0,
                    new Vector2(0f, 0f),
                    1,
                    SpriteEffects.None,
                    0f
                );
            return false;
        }

        private int[] CreatePattern(int i, int j)
        {
            int[] sheetOffset = new int[2] { i % 2, j % 2 };
            sheetOffset[0] = sheetOffset[0] * 468;
            sheetOffset[1] = sheetOffset[1] * 180;
            return sheetOffset;
        }

        private Color GetWallColour(int i, int j)
        {
            int colType = Main.tile[i, j].wallColor();
            Color paintCol = WorldGen.paintColor(colType);
            if (colType < 13)
            {
                paintCol.R = (byte)((paintCol.R / 2f) + 128);
                paintCol.G = (byte)((paintCol.G / 2f) + 128);
                paintCol.B = (byte)((paintCol.B / 2f) + 128);
            }
            if (colType == 29)
            {
                paintCol = Color.Black;
            }
            Color col = Lighting.GetColor(i, j);
            col.R = (byte)(paintCol.R / 255f * col.R);
            col.G = (byte)(paintCol.G / 255f * col.G);
            col.B = (byte)(paintCol.B / 255f * col.B);
            return col;
        }
    }
}
