﻿using CalamityMod.DataStructures;
using CalamityMod.World.Planets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace CalamityMod.World
{
    public class WorldEvilIsland
    {
        public static void PlaceEvilIsland()
        {
            int x = Main.maxTilesX;
            int xIslandGen;
            int yIslandGen;
            Rectangle potentialArea;

            if (WorldGen.getGoodWorldGen)
            {
                do
                {
                    xIslandGen = WorldGen.genRand.Next((int)(x * 0.1), (int)(x * 0.2));
                    yIslandGen = WorldGen.genRand.Next(95, 126);
                    yIslandGen = Math.Min(yIslandGen, (int)GenVars.worldSurfaceLow - 50);

                    int checkAreaX = 160;
                    int checkAreaY = 90;
                    potentialArea = Utils.CenteredRectangle(new Vector2(xIslandGen, yIslandGen), new Vector2(checkAreaX, checkAreaY));
                }
                while (!Planetoid.InvalidSkyPlacementArea(potentialArea));

                int tileXLookup = xIslandGen;
                while (Main.tile[tileXLookup, yIslandGen].HasTile)
                    tileXLookup++;

                xIslandGen = tileXLookup;
                EvilIsland(xIslandGen, yIslandGen, true);
                EvilIslandHouse(xIslandGen, yIslandGen, true);

                do
                {
                    xIslandGen = WorldGen.genRand.Next((int)(x * 0.8), (int)(x * 0.9));
                    yIslandGen = WorldGen.genRand.Next(95, 126);
                    yIslandGen = Math.Min(yIslandGen, (int)GenVars.worldSurfaceLow - 50);

                    int checkAreaX = 160;
                    int checkAreaY = 90;
                    potentialArea = Utils.CenteredRectangle(new Vector2(xIslandGen, yIslandGen), new Vector2(checkAreaX, checkAreaY));
                }
                while (!Planetoid.InvalidSkyPlacementArea(potentialArea));

                tileXLookup = xIslandGen;
                while (Main.tile[tileXLookup, yIslandGen].HasTile)
                    tileXLookup--;

                xIslandGen = tileXLookup;
                EvilIsland(xIslandGen, yIslandGen, false);
                EvilIslandHouse(xIslandGen, yIslandGen, false);
            }
            else
            {
                do
                {
                    xIslandGen = WorldGen.crimson ?
                        WorldGen.genRand.Next((int)(x * 0.1), (int)(x * 0.3)) :
                        WorldGen.genRand.Next((int)(x * 0.7), (int)(x * 0.9));
                    yIslandGen = WorldGen.genRand.Next(95, 126);
                    yIslandGen = Math.Min(yIslandGen, (int)GenVars.worldSurfaceLow - 50);

                    int checkAreaX = 160;
                    int checkAreaY = 90;
                    potentialArea = Utils.CenteredRectangle(new Vector2(xIslandGen, yIslandGen), new Vector2(checkAreaX, checkAreaY));
                }
                while (!Planetoid.InvalidSkyPlacementArea(potentialArea));

                int tileXLookup = xIslandGen;
                if (WorldGen.crimson)
                {
                    while (Main.tile[tileXLookup, yIslandGen].HasTile)
                        tileXLookup++;
                }
                else
                {
                    while (Main.tile[tileXLookup, yIslandGen].HasTile)
                        tileXLookup--;
                }

                xIslandGen = tileXLookup;
                EvilIsland(xIslandGen, yIslandGen, WorldGen.crimson);
                EvilIslandHouse(xIslandGen, yIslandGen, WorldGen.crimson);
            }
        }

        public static void EvilIsland(int i, int j, bool genCorruptIsland)
        {
            // Generate puffy clouds along the bottom.
            int leftOffset = 86;
            int rightOffset = 86;
            int maxVerticalOffset = 24;
            List<Point> cloudPositions = new List<Point>();
            for (int dx = -leftOffset; dx < rightOffset; dx += WorldGen.genRand.Next(21, 26))
            {
                float completionRatio = Utils.GetLerpValue(-leftOffset - 22f, rightOffset + 22f, dx, true);
                float completionRatio010 = CalamityUtils.Convert01To010(completionRatio);
                int verticalOffset = (int)(completionRatio010 * maxVerticalOffset);
                Point cloudPosition = new Point(i + dx, j + verticalOffset);

                int radius = WorldGen.genRand.Next(22, 26) - (int)((1f - completionRatio010) * 15);
                WorldUtils.Gen(cloudPosition, new CustomShapes.DistortedCircle(radius, 0.1f), Actions.Chain(
                    new Actions.SetTile(TileID.Cloud),
                    new Modifiers.Blotches(4, 1f)));

                cloudPositions.Add(cloudPosition);
            }

            // Clear out a hole.
            WorldUtils.Gen(new Point(i, j - 4), new Shapes.Circle((leftOffset + rightOffset - 30) / 2, 16), new Actions.ClearTile());

            // Create random smaller clouds with offsets to fill in the cut out area.
            foreach (Point cloudPosition in cloudPositions)
            {
                Vector2 offsetCloudPosition = cloudPosition.ToVector2();
                while (!CalamityUtils.ParanoidTileRetrieval((int)offsetCloudPosition.X, (int)offsetCloudPosition.Y).HasTile)
                    offsetCloudPosition.Y++;

                for (int k = 0; k < 3; k++)
                {
                    int radius = WorldGen.genRand.Next(6, 9);
                    Vector2 randomCloudPosition = offsetCloudPosition + WorldGen.genRand.NextVector2Circular(radius, radius) * 0.75f;

                    // Account for centering, somewhat.
                    randomCloudPosition.Y -= radius * 0.4f;
                    randomCloudPosition.Y += 5f;

                    WorldUtils.Gen(randomCloudPosition.ToPoint(), new Shapes.Circle(radius / 2), new Actions.SetTile(TileID.Cloud));
                }
            }

            // Fill the not cloud area with evil sandstone.
            for (int dx = -leftOffset + 10; dx < rightOffset - 10; dx++)
            {
                float completionRatio = Utils.GetLerpValue(-leftOffset - 22f, rightOffset + 22f, dx, true);
                float completionRatio010 = CalamityUtils.Convert01To010(completionRatio);
                int verticalOffset = (int)((1f - completionRatio010) * maxVerticalOffset);
                for (int dy = -5; dy < maxVerticalOffset + 13 - verticalOffset; dy++)
                {
                    if (!CalamityUtils.ParanoidTileRetrieval(i + dx, j + dy).HasTile)
                    {
                        Main.tile[i + dx, j + dy].TileType = genCorruptIsland ? TileID.CorruptSandstone : TileID.CrimsonSandstone;
                        Main.tile[i + dx, j + dy].Get<TileWallWireStateData>().HasTile = true;
                    }
                }
            }

            // Determine the lines at which clouds begin to form.
            List<Point> borderPoints = new List<Point>();
            for (int dx = -leftOffset + 14; dx < rightOffset - 14; dx++)
            {
                int verticalBorder = j - 10;
                while (CalamityUtils.ParanoidTileRetrieval(i + dx, verticalBorder).TileType != TileID.Cloud)
                {
                    verticalBorder++;
                    if (verticalBorder > j + 35)
                        break;
                }

                if (verticalBorder >= j + 35)
                    continue;

                borderPoints.Add(new Point(i + dx, verticalBorder));
            }

            // And generate blotches at those borders.
            for (int k = 0; k < 10; k++)
            {
                Point borderToGenerateAt = borderPoints[WorldGen.genRand.Next(borderPoints.Count)];
                Vector2 moveDirection = Vector2.UnitY.RotatedBy(WorldGen.genRand.NextFloat(-0.4f, 0.4f));
                for (int l = 0; l < 4; l++)
                {
                    Point blotchPosition = (borderToGenerateAt.ToVector2() + Main.rand.NextVector2Circular(5f, 5f) + moveDirection * l * 3f).ToPoint();
                    WorldUtils.Gen(blotchPosition, new CustomShapes.DistortedCircle(WorldGen.genRand.Next(8, 10) - l, 0.4f), Actions.Chain(
                        new Actions.SetTile(genCorruptIsland ? TileID.CorruptSandstone : TileID.CrimsonSandstone),
                        new Modifiers.IsSolid(),
                        new Modifiers.Blotches(5, 1f),
                        new Modifiers.OnlyTiles(TileID.CorruptSandstone, TileID.CrimsonSandstone)));
                }
            }

            // Create some blotches of hardened evil sand and evil ore.
            for (int k = 0; k < 12; k++)
            {
                int radius = WorldGen.genRand.Next(6, 8);
                ushort tileType = genCorruptIsland ? TileID.CorruptHardenedSand : TileID.CrimsonHardenedSand;

                Point blotchPosition = new Point(i + WorldGen.genRand.Next(-leftOffset + 20, rightOffset - 20), j + WorldGen.genRand.Next(-3, 12));
                if (k > 4)
                {
                    if (blotchPosition.Y > j + 3)
                    {
                        radius -= WorldGen.genRand.Next(3);
                        tileType = genCorruptIsland ? TileID.Demonite : TileID.Crimtane;
                    }
                    else
                        continue;
                }

                WorldUtils.Gen(blotchPosition, new CustomShapes.DistortedCircle(radius, 0.35f), Actions.Chain(
                    new Actions.SetTile(tileType),
                    new Modifiers.IsSolid(),
                    new Modifiers.Blotches(5, 1f),
                    new Modifiers.OnlyTiles(TileID.CorruptSandstone, TileID.CrimsonSandstone)));
            }

            // Determine the lines at which ground begins to appear. This only applies if the ground is an evil tile.
            List<Point> surfacePoints = new List<Point>();
            for (int dx = -leftOffset + 24; dx < rightOffset - 24; dx++)
            {
                // Ignore points near the center because bumps there could cause the house to offset in weird ways.
                if (Math.Abs(dx) < 15)
                    continue;

                int surface = j - 15;
                bool hitCloud = false;
                while (!CalamityUtils.ParanoidTileRetrieval(i + dx, surface).HasTile)
                {
                    surface++;
                    if (CalamityUtils.ParanoidTileRetrieval(i + dx, surface).TileType == TileID.Cloud)
                    {
                        hitCloud = true;
                        break;
                    }
                    if (surface > j + 35)
                        break;
                }

                if (hitCloud || surface >= j + 35)
                    continue;

                surfacePoints.Add(new Point(i + dx, surface));
            }

            // Create "bumps" on the top by creating blotches just a bit below the surface.
            for (int k = 0; k < 4; k++)
            {
                Point surfaceToGenerateAt = surfacePoints[WorldGen.genRand.Next(surfacePoints.Count)];
                Point blotchPosition = (surfaceToGenerateAt.ToVector2() + Vector2.UnitY * 5f).ToPoint();
                WorldUtils.Gen(blotchPosition, new Shapes.Circle(WorldGen.genRand.Next(8, 10)), Actions.Chain(
                    new Actions.SetTile(CalamityUtils.ParanoidTileRetrieval(surfaceToGenerateAt.X, surfaceToGenerateAt.Y).TileType),
                    new Modifiers.SkipTiles(TileID.Demonite, TileID.Crimtane),
                    new Modifiers.Blotches(5, 1f)));
            }

            // Generate one large chunk of ore somewhere, just in case there isn't much otherwise.
            Vector2 oreStartPosition = new Vector2(i + WorldGen.genRand.Next(-leftOffset + 28, rightOffset - 28), j + WorldGen.genRand.Next(8, 14));
            Vector2 oreEndPosition = oreStartPosition + Vector2.UnitX.RotatedBy(WorldGen.genRand.NextFloat(-0.36f, -0.14f)) * WorldGen.genRand.NextBool(2).ToDirectionInt() * WorldGen.genRand.NextFloat(18f, 25f);
            Vector2 oreMiddlePosition = (oreStartPosition + oreEndPosition) * 0.5f + WorldGen.genRand.NextVector2Circular(6f, 6f);
            List<Vector2> smoothOrePositions = new BezierCurve(oreStartPosition, oreMiddlePosition, oreStartPosition).GetPoints(25);
            for (int k = 0; k < smoothOrePositions.Count; k++)
            {
                float strength = MathHelper.Lerp(1f, 4f, CalamityUtils.Convert01To010(k / (float)smoothOrePositions.Count));
                Point orePosition = smoothOrePositions[k].ToPoint();

                WorldUtils.Gen(orePosition, new Shapes.Circle((int)strength), Actions.Chain(
                    new Actions.SetTile(genCorruptIsland ? TileID.Demonite : TileID.Crimtane)));
            }

            // Paint all clouds.
            for (int dx = -leftOffset - 30; dx < rightOffset + 30; dx++)
            {
                for (int dy = -8; dy < 55; dy++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval(i + dx, j + dy).TileType == TileID.Cloud)
                        WorldGen.paintTile(i + dx, j + dy, genCorruptIsland ? PaintID.PurplePaint : PaintID.RedPaint);
                }
            }
        }

        public static void EvilIslandHouse(int i, int j, bool genCorruptHouse)
        {
            ushort type = (ushort)(genCorruptHouse ? 152 : 347); //tile
            byte wall = (byte)(genCorruptHouse ? 35 : 174); //wall
            Vector2 vector = new Vector2((float)i, (float)j);
            int num = 1;
            if (WorldGen.genRand.Next(2) == 0)
            {
                num = -1;
            }
            int num2 = WorldGen.genRand.Next(7, 12);
            int num3 = WorldGen.genRand.Next(5, 7);
            vector.X = (float)(i + (num2 + 2) * num);
            for (int k = j - 15; k < j + 30; k++)
            {
                if (Main.tile[(int)vector.X, k].HasTile)
                {
                    vector.Y = (float)(k - 1);
                    break;
                }
            }
            vector.X = (float)i;
            int num4 = (int)(vector.X - (float)num2 - 1f);
            int num5 = (int)(vector.X + (float)num2 + 1f);
            int num6 = (int)(vector.Y - (float)num3 - 1f);
            int num7 = (int)(vector.Y + 2f);
            if (num4 < 0)
            {
                num4 = 0;
            }
            if (num5 > Main.maxTilesX)
            {
                num5 = Main.maxTilesX;
            }
            if (num6 < 0)
            {
                num6 = 0;
            }
            if (num7 > Main.maxTilesY)
            {
                num7 = Main.maxTilesY;
            }
            for (int l = num4; l <= num5; l++)
            {
                for (int m = num6 - 1; m < num7 + 1; m++)
                {
                    if (m != num6 - 1 || (l != num4 && l != num5))
                    {
                        Main.tile[l, m].Get<TileWallWireStateData>().HasTile = true;
                        Main.tile[l, m].LiquidAmount = 0;
                        Main.tile[l, m].TileType = type;
                        Main.tile[l, m].WallType = 0;
                        Main.tile[l, m].Get<TileWallWireStateData>().IsHalfBlock = false;
                        Main.tile[l, m].Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                    }
                }
            }
            num4 = (int)(vector.X - (float)num2);
            num5 = (int)(vector.X + (float)num2);
            num6 = (int)(vector.Y - (float)num3);
            num7 = (int)(vector.Y + 1f);
            if (num4 < 0)
            {
                num4 = 0;
            }
            if (num5 > Main.maxTilesX)
            {
                num5 = Main.maxTilesX;
            }
            if (num6 < 0)
            {
                num6 = 0;
            }
            if (num7 > Main.maxTilesY)
            {
                num7 = Main.maxTilesY;
            }
            for (int n = num4; n <= num5; n++)
            {
                for (int num8 = num6; num8 < num7; num8++)
                {
                    if ((num8 != num6 || (n != num4 && n != num5)) && Main.tile[n, num8].WallType == 0)
                    {
                        Main.tile[n, num8].Get<TileWallWireStateData>().HasTile = false;
                        Main.tile[n, num8].WallType = wall;
                    }
                }
            }
            int num9 = i + (num2 + 1) * num;
            int num10 = (int)vector.Y;
            for (int num11 = num9 - 2; num11 <= num9 + 2; num11++)
            {
                Main.tile[num11, num10].Get<TileWallWireStateData>().HasTile = false;
                Main.tile[num11, num10 - 1].Get<TileWallWireStateData>().HasTile = false;
                Main.tile[num11, num10 - 2].Get<TileWallWireStateData>().HasTile = false;
            }
            WorldGen.PlaceTile(num9, num10, 10, true, false, -1, genCorruptHouse ? 1 : 10); //door
            num9 = i + (num2 + 1) * -num - num;
            for (int num12 = num6; num12 <= num7 + 1; num12++)
            {
                Main.tile[num9, num12].Get<TileWallWireStateData>().HasTile = true;
                Main.tile[num9, num12].LiquidAmount = 0;
                Main.tile[num9, num12].TileType = type;
                Main.tile[num9, num12].WallType = 0;
                Main.tile[num9, num12].Get<TileWallWireStateData>().IsHalfBlock = false;
                Main.tile[num9, num12].Get<TileWallWireStateData>().Slope = SlopeType.Solid;
            }
            int contain;
            if (genCorruptHouse)
            {
                contain = 1571; //scourge
            }
            else
            {
                contain = 1569; //vampire
            }
            WorldGen.AddBuriedChest(i, num10 - 3, contain, false, genCorruptHouse ? 19 : 20); //chest
            int num14 = i - num2 / 2 + 1;
            int num15 = i + num2 / 2 - 1;
            int num16 = 1;
            if (num2 > 10)
            {
                num16 = 2;
            }
            int num17 = (num6 + num7) / 2 - 1;
            for (int num18 = num14 - num16; num18 <= num14 + num16; num18++)
            {
                for (int num19 = num17 - 1; num19 <= num17 + 1; num19++)
                {
                    Main.tile[num18, num19].WallType = 21; //glass
                }
            }
            for (int num20 = num15 - num16; num20 <= num15 + num16; num20++)
            {
                for (int num21 = num17 - 1; num21 <= num17 + 1; num21++)
                {
                    Main.tile[num20, num21].WallType = 21; //glass
                }
            }
            int num22 = i + (num2 / 2 + 1) * -num;
            WorldGen.PlaceTile(num22, num7 - 1, 14, true, false, -1, genCorruptHouse ? 1 : 8); //table
            WorldGen.PlaceTile(num22 - 2, num7 - 1, 15, true, false, 0, genCorruptHouse ? 2 : 11); //chair
            Tile tile = Main.tile[num22 - 2, num7 - 1];
            tile.TileFrameX += 18;
            Tile tile2 = Main.tile[num22 - 2, num7 - 2];
            tile2.TileFrameX += 18;
            WorldGen.PlaceTile(num22 + 2, num7 - 1, 15, true, false, 0, genCorruptHouse ? 2 : 11); //chair
        }
    }
}
