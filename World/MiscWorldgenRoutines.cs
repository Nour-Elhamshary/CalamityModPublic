﻿using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Tiles;
using CalamityMod.Tiles.Abyss;
using CalamityMod.Tiles.Astral;
using CalamityMod.Tiles.Crags;
using CalamityMod.Tiles.FurnitureAncient;
using CalamityMod.Tiles.Ores;
using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;

namespace CalamityMod.World
{
    public class MiscWorldgenRoutines
    {
        #region Place Chests
        internal static Chest AddChestWithLoot(int i, int j, ushort type = TileID.Containers, uint startingSlot = 1, int tileStyle = 0)
        {
            int chestIndex = -1;

            // Slide downwards on the Y axis trying to find the floor beneath the empty position initially picked
            while (j < Main.maxTilesY - 210)
            {
                if (!WorldGen.SolidTile(i, j))
                {
                    j++;
                    continue;
                }

                // If there are already 1,000 chests in the world and this one fails to place, just give up.
                chestIndex = WorldGen.PlaceChest(i - 1, j - 1, type, false, tileStyle);
                break;
            }

            if (chestIndex < 0)
                return null;

            Chest chest = Main.chest[chestIndex];
            PlaceLootInChest(ref chest, type, startingSlot);
            return chest;
        }

        internal static void PlaceLootInChest(ref Chest chest, ushort type, uint startingSlot)
        {
            uint itemIndex = startingSlot;

            void PutItemInChest(ref Chest c, int id, int minQuantity = 0, int maxQuantity = 0, bool condition = true)
            {
                if (!condition)
                    return;

                c.item[itemIndex].SetDefaults(id, false);

                // Don't set quantity unless quantity is specified
                if (minQuantity > 0)
                {
                    // Max quantity cannot be less than min quantity. It's zero if not specified, meaning you get exactly minQuantity.
                    if (maxQuantity < minQuantity)
                        maxQuantity = minQuantity;
                    c.item[itemIndex].stack = WorldGen.genRand.Next(minQuantity, maxQuantity + 1);
                }
                itemIndex++;
            }

            // Astral Chest has completely different loot in it
            if (type == ModContent.TileType<AstralChestLocked>())
            {
                PutItemInChest(ref chest, ModContent.ItemType<Stardust>(), 30, 80);
                PutItemInChest(ref chest, ModContent.ItemType<AureusCell>(), 10, 14);
                PutItemInChest(ref chest, ModContent.ItemType<ZergPotion>(), 8);
                PutItemInChest(ref chest, ModContent.ItemType<ZenPotion>(), 3, 5);
                PutItemInChest(ref chest, ItemID.FallenStar, 12, 30);

                // Gold Coins don't stack above 100, so this efficiently lets you stuff over a platinum into a chest
                int goldCoins = WorldGen.genRand.Next(30, 120);
                if (goldCoins > 100)
                {
                    PutItemInChest(ref chest, ItemID.PlatinumCoin);
                    goldCoins -= 100;
                }
                PutItemInChest(ref chest, ItemID.GoldCoin, goldCoins);
            }
            else if (type == ModContent.TileType<RustyChestTile>())
            {
                // 15-29 torches (in accordence with vanilla)
                PutItemInChest(ref chest, ItemID.Torch, 15, 29);

                // 50% chance of 1 or 2 of the following potions
                int[] potions = new int[]
                {
                    ModContent.ItemType<HadalStew>(), ItemID.WaterWalkingPotion, ItemID.ShinePotion, ItemID.GillsPotion, ItemID.FlipperPotion
                };
                PutItemInChest(ref chest, ModContent.ItemType<SulphurskinPotion>(), 4, 7, WorldGen.genRand.NextBool());
                PutItemInChest(ref chest, WorldGen.genRand.Next(potions), 1, 2, WorldGen.genRand.NextBool());
                PutItemInChest(ref chest, WorldGen.genRand.Next(potions), 1, 2, WorldGen.genRand.NextBool());

                // 33% chance of flippers
                PutItemInChest(ref chest, ItemID.Flipper, condition: WorldGen.genRand.NextBool(3));

                // Typical coins
                PutItemInChest(ref chest, ItemID.GoldCoin, 2, 4);
            }
            // Default loot
            else
            {
                // Silver, Tungsten, Gold or Platinum bars (following worldgen choice)
                int barID = WorldGen.genRand.NextBool() ? GenVars.goldBar : GenVars.silverBar;
                PutItemInChest(ref chest, barID, 3, 10);

                // 50% chance of 25-50 Holy Arrows
                PutItemInChest(ref chest, ItemID.HolyArrow, 25, 50, WorldGen.genRand.NextBool());

                // 50% chance of 1 or 2 of the following potions
                int[] potions = new int[] {
                    ItemID.SpelunkerPotion, ItemID.FeatherfallPotion, ItemID.NightOwlPotion,
                    ItemID.WaterWalkingPotion, ItemID.ArcheryPotion, ItemID.GravitationPotion
                };
                PutItemInChest(ref chest, WorldGen.genRand.Next(potions), 1, 2, WorldGen.genRand.NextBool());

                // 50% chance of 1 or 2 of the following potions
                // Yes, in vanilla, Dangersense Potions have double the chance to appear.
                potions = new int[] {
                    ItemID.ThornsPotion, ItemID.WaterWalkingPotion, ItemID.InvisibilityPotion,
                    ItemID.ManaRegenerationPotion, ItemID.TeleportationPotion, ItemID.TrapsightPotion, ItemID.TrapsightPotion
                };

                PutItemInChest(ref chest, WorldGen.genRand.Next(potions), 1, 2, WorldGen.genRand.NextBool());
                PutItemInChest(ref chest, ItemID.RecallPotion, 1, 2, WorldGen.genRand.NextBool());
                PutItemInChest(ref chest, ItemID.GoldCoin, 1, 2);
            }
        }
        #endregion

        #region Place Rox Shrine
        public static void PlaceRoxShrine()
        {
            while (!CalamityWorld.roxShrinePlaced)
            {
                CalamityWorld.roxShrinePlaced = true;
                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        if (Main.tile[x, y] != null && Main.tile[x, y].TileType == TileID.LargePiles)
                        {
                            if ((Main.tile[x, y].TileFrameX == 18 && Main.tile[x, y].TileFrameY == 0) || (Main.tile[x, y].TileFrameX == 45 && Main.tile[x, y].TileFrameY == 0))
                            {
                                if (WorldGen.genRand.NextBool(3))
                                {
                                    for (int dx = -1; dx < 2; dx++)
                                    {
                                        for (int dy = -1; dy < 2; dy++)
                                            Main.tile[x + dx, y + dy].Get<TileWallWireStateData>().HasTile = false;
                                    }

                                    WorldGen.PlaceTile(x, y + 1, ModContent.TileType<RoxTile>());
                                    return;
                                }
                                else
                                    CalamityWorld.roxShrinePlaced = false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Chasm Generator
        public static void ChasmGenerator(int i, int j, int steps, bool ocean = false)
        {
            float num = steps; //850 small 1450 medium 2050 large
            int limitIncrease = Main.remixWorld ? 110 : 0;
            if (ocean)
            {
                int tileYLookup = j;
                if (Abyss.AtLeftSideOfWorld)
                {
                    while (!Main.tile[i + 125, tileYLookup].HasTile)
                    {
                        tileYLookup++;
                    }
                }
                else
                {
                    while (!Main.tile[i - 125, tileYLookup].HasTile)
                    {
                        tileYLookup++;
                    }
                }
                j = tileYLookup;
            }
            Vector2 vector;
            vector.X = i;
            vector.Y = j;
            Vector2 vector2;
            vector2.X = WorldGen.genRand.Next(-1, 2) * 0.1f;
            vector2.Y = WorldGen.genRand.Next(3, 8) * 0.2f + 0.5f;
            int num2 = 5;
            double num3 = WorldGen.genRand.Next(5, 7) + 20; //start width
            while (num3 > 0.0)
            {
                if (num > 0f)
                {
                    num3 += WorldGen.genRand.Next(10);
                    num3 -= WorldGen.genRand.Next(10);
                    float smallHoleLimit = 790f; //small
                    
                    if (Main.maxTilesY > 1500)
                    { 
                        smallHoleLimit = 1360f; 
                        
                        if (Main.maxTilesY > 2100) 
                        { 
                            smallHoleLimit = 1950f; 
                        } 
                    }
                    
                    if (ocean && num > smallHoleLimit)
                    {
                        if (num3 < 7.0) //min width
                        {
                            num3 = 7.0; //min width
                        }
                        if (num3 > 45.0) //max width
                        {
                            num3 = 45.0; //max width
                        }
                    }
                    else //dig large hole
                    {
                        if (num3 < (ocean ? 30.0 : 8.0)) //min width
                        {
                            num3 = ocean ? 30.0 : 8.0; //min width
                        }
                        if (num3 > (ocean ? 70.0 : 20.0)) //max width
                        {
                            num3 = ocean ? 70.0 : 20.0; //max width
                        }
                        if (num == 1f && num3 < (ocean ? 50.0 : 15.0))
                        {
                            num3 = ocean ? 50.0 : 15.0;
                        }
                    }
                }
                else
                {
                    if ((double)vector.Y > (Abyss.AbyssChasmBottom + limitIncrease))
                    {
                        num3 -= WorldGen.genRand.Next(5) + 8;
                    }
                }
                if (Main.maxTilesY > 2100)
                {
                    if (((double)vector.Y > (Abyss.AbyssChasmBottom + limitIncrease) && num > 0f && ocean) ||
                    (vector.Y >= Main.maxTilesY && num > 0f && !ocean))
                    {
                        num = 0f;
                    }
                }
                else if (Main.maxTilesY > 1500)
                {
                    if (((double)vector.Y > (Abyss.AbyssChasmBottom + limitIncrease) && num > 0f && ocean) ||
                    (vector.Y > Main.maxTilesY && num > 0f && !ocean))
                    {
                        num = 0f;
                    }
                }
                else
                {
                    if (((double)vector.Y > (Abyss.AbyssChasmBottom + limitIncrease) && num > 0f && ocean) ||
                    (vector.Y > Main.maxTilesY && num > 0f && !ocean))
                    {
                        num = 0f;
                    }
                }

                num -= 1f;
                int num4;
                int num5;
                int num6;
                int num7;
                if (num > num2)
                {
                    num4 = (int)(vector.X - num3 * 0.5);
                    num5 = (int)(vector.X + num3 * 0.5);
                    num6 = (int)(vector.Y - num3 * 0.5);
                    num7 = (int)(vector.Y + num3 * 0.5);
                    if (num4 < 0)
                    {
                        num4 = 0;
                    }
                    if (num5 > Main.maxTilesX - 1)
                    {
                        num5 = Main.maxTilesX - 1;
                    }
                    if (num6 < 0)
                    {
                        num6 = 0;
                    }
                    if (num7 > Main.maxTilesY)
                    {
                        num7 = Main.maxTilesY;
                    }
                    for (int k = num4; k < num5; k++)
                    {
                        for (int l = num6; l < num7; l++)
                        {
                            if ((Math.Abs(k - vector.X) + Math.Abs(l - vector.Y)) < num3 * 0.5 * (1.0 + WorldGen.genRand.Next(-5, 6) * 0.015))
                            {
                                if (ocean)
                                {
                                    Main.tile[k, l].Get<TileWallWireStateData>().HasTile = false;
                                    Main.tile[k, l].LiquidAmount = 255;
                                    Main.tile[k, l].Get<LiquidData>().LiquidType = LiquidID.Water;
                                }
                                else
                                {
                                    Main.tile[k, l].Get<TileWallWireStateData>().HasTile = false;
                                    Main.tile[k, l].LiquidAmount = 255;
                                    Main.tile[k, l].Get<LiquidData>().LiquidType = LiquidID.Lava;
                                }
                            }
                        }
                    }
                }
                /*if (num <= 2f && vector.Y < (Main.rockLayer + Main.maxTilesY * 0.3))
                {
                    num = 2f;
                }*/
                vector += vector2;
                vector2.X += WorldGen.genRand.Next(-1, 2) * 0.01f;
                if (vector2.X > 0.02)
                {
                    vector2.X = 0.02f;
                }
                if (vector2.X < -0.02)
                {
                    vector2.X = -0.02f;
                }
                num4 = (int)(vector.X - num3 * 1.1);
                num5 = (int)(vector.X + num3 * 1.1);
                num6 = (int)(vector.Y - num3 * 1.1);
                num7 = (int)(vector.Y + num3 * 1.1);
                if (num4 < 1)
                {
                    num4 = 1;
                }
                if (num5 > Main.maxTilesX - 1)
                {
                    num5 = Main.maxTilesX - 1;
                }
                if (num6 < 0)
                {
                    num6 = 0;
                }
                if (num7 > Main.maxTilesY)
                {
                    num7 = Main.maxTilesY;
                }
                for (int m = num4; m < num5; m++)
                {
                    for (int n = num6; n < num7; n++)
                    {
                        if ((Math.Abs(m - vector.X) + Math.Abs(n - vector.Y)) < num3 * 1.1 * (1.0 + WorldGen.genRand.Next(-5, 6) * 0.015))
                        {
                            if (n > j + WorldGen.genRand.Next(7, 16))
                            {
                                Main.tile[m, n].Get<TileWallWireStateData>().HasTile = false;
                            }
                            if (steps <= num2)
                            {
                                Main.tile[m, n].Get<TileWallWireStateData>().HasTile = false;
                            }
                            if (ocean)
                            {
                                Main.tile[m, n].LiquidAmount = 255;
                                Main.tile[m, n].Get<LiquidData>().LiquidType = LiquidID.Water;
                            }
                            else
                            {
                                Main.tile[m, n].LiquidAmount = 255;
                                Main.tile[m, n].Get<LiquidData>().LiquidType = LiquidID.Lava;
                            }
                        }
                    }
                }
                for (int num11 = num4; num11 < num5; num11++)
                {
                    for (int num12 = num6; num12 < num7; num12++)
                    {
                        if ((Math.Abs(num11 - vector.X) + Math.Abs(num12 - vector.Y)) < num3 * 1.1 * (1.0 + WorldGen.genRand.Next(-5, 6) * 0.015))
                        {
                            if (ocean)
                            {
                                Main.tile[num11, num12].LiquidAmount = 255;
                                Main.tile[num11, num12].Get<LiquidData>().LiquidType = LiquidID.Water;
                            }
                            else
                            {
                                Main.tile[num11, num12].LiquidAmount = 255;
                                Main.tile[num11, num12].Get<LiquidData>().LiquidType = LiquidID.Lava;
                            }
                            if (steps <= num2)
                            {
                                Main.tile[num11, num12].Get<TileWallWireStateData>().HasTile = false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Smart Gem Gen
        public static void SmartGemGen()
        {
            double oneThirdOfUnderground = (Main.maxTilesY - 200 - Main.worldSurface) / 3D;
            double verticalStartFactor_Layer1 = Main.worldSurface;
            double verticalStartFactor_Layer2 = verticalStartFactor_Layer1 + oneThirdOfUnderground;
            double verticalStartFactor_Layer3 = verticalStartFactor_Layer1 + oneThirdOfUnderground * 2D;
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    if (y > verticalStartFactor_Layer3)
                    {
                        if (Main.tile[x, y] != null)
                        {
                            if (Main.remixWorld)
                            {
                                if (Main.tile[x, y].TileType == TileID.Diamond || Main.tile[x, y].TileType == TileID.Ruby)
                                    Main.tile[x, y].TileType = TileID.Topaz;
                                else if (Main.tile[x, y].TileType == TileID.Emerald || Main.tile[x, y].TileType == TileID.Sapphire)
                                    Main.tile[x, y].TileType = TileID.Amethyst;
                            }
                            else
                            {
                                if (Main.tile[x, y].TileType == TileID.Emerald || Main.tile[x, y].TileType == TileID.Sapphire)
                                    Main.tile[x, y].TileType = TileID.Diamond;
                                else if (Main.tile[x, y].TileType == TileID.Topaz || Main.tile[x, y].TileType == TileID.Amethyst)
                                    Main.tile[x, y].TileType = TileID.Ruby;
                            }
                        }
                    }
                    else if (y > verticalStartFactor_Layer2)
                    {
                        if (Main.tile[x, y] != null)
                        {
                            if (Main.tile[x, y].TileType == TileID.Diamond || Main.tile[x, y].TileType == TileID.Ruby)
                                Main.tile[x, y].TileType = TileID.Emerald;
                            else if (Main.tile[x, y].TileType == TileID.Topaz || Main.tile[x, y].TileType == TileID.Amethyst)
                                Main.tile[x, y].TileType = TileID.Sapphire;
                        }
                    }
                    else if (y > verticalStartFactor_Layer1)
                    {
                        if (Main.tile[x, y] != null)
                        {
                            if (Main.remixWorld)
                            {
                                if (Main.tile[x, y].TileType == TileID.Emerald || Main.tile[x, y].TileType == TileID.Sapphire)
                                    Main.tile[x, y].TileType = TileID.Diamond;
                                else if (Main.tile[x, y].TileType == TileID.Topaz || Main.tile[x, y].TileType == TileID.Amethyst)
                                    Main.tile[x, y].TileType = TileID.Ruby;
                            }
                            else
                            {
                                if (Main.tile[x, y].TileType == TileID.Diamond || Main.tile[x, y].TileType == TileID.Ruby)
                                    Main.tile[x, y].TileType = TileID.Topaz;
                                else if (Main.tile[x, y].TileType == TileID.Emerald || Main.tile[x, y].TileType == TileID.Sapphire)
                                    Main.tile[x, y].TileType = TileID.Amethyst;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
