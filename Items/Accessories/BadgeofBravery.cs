﻿using CalamityMod.CalPlayer;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class BadgeofBravery : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Badge of Bravery");
            Tooltip.SetDefault("15% increased melee speed");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 30;
            item.value = Item.buyPrice(0, 21, 0, 0);
            item.accessory = true;
            item.Calamity().postMoonLordRarity = 12;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.badgeOfBravery = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "UeliaceBar", 2);
            recipe.AddIngredient(ItemID.FeralClaws);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void OnCraft(Recipe recipe)
        {
            if (Main.rand.NextBool(40))
                recipe.createItem.type = ModContent.ItemType<SamuraiBadge>();
            else
                recipe.createItem.type = item.type;
        }
    }
}
