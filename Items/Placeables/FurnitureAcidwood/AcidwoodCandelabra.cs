using CalamityMod.Tiles.FurnitureAcidwood;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAcidwood
{
    public class AcidwoodCandelabra : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            item.SetNameOverride("Acidwood Candelabra");
            item.width = 28;
            item.height = 20;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createTile = ModContent.TileType<AcidwoodCandelabraTile>();
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Acidwood>(), 5);
            recipe.AddIngredient(ItemID.Torch, 3);
            recipe.SetResult(this);
            recipe.AddTile(TileID.WorkBenches);
            recipe.AddRecipe();
        }
    }
}