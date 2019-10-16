using Microsoft.Xna.Framework;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class Butcher : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Butcher");
            Tooltip.SetDefault("Fires faster and more accurately the longer you hold the trigger");
        }

        public override void SetDefaults()
        {
            item.damage = 17;
            item.width = 20;
            item.height = 12;
            item.useTime = 40;
            item.useAnimation = 40;
            item.useStyle = 5;
            item.rare = 5;
            item.knockBack = 1f;
            item.value = Item.buyPrice(0, 36, 0, 0);
            item.UseSound = SoundID.Item38;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.ranged = true;
            item.channel = true;
            item.autoReuse = true;
            item.shoot = ModContent.ProjectileType<Butcher>();
            item.shootSpeed = 12f;
            item.useAmmo = 97;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, ModContent.ProjectileType<Butcher>(), damage, knockBack, player.whoAmI, 0.0f, 0.0f);
            return false;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Shotgun);
            recipe.AddIngredient(null, "EssenceofChaos", 4);
            recipe.AddIngredient(null, "EssenceofEleum", 4);
            recipe.AddIngredient(ItemID.IllegalGunParts);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
