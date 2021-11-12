using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class CindersOfLament : ModItem
    {
        public const string PoeticTooltipLine = "The Witch, a sinner of her own making,\n" +
            "Within her mind her demon lies, ever patient, until the end of time.";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cinders of Lament");
            Tooltip.SetDefault("Summons either Cataclysm or Catastrophe at the mouse position\n" +
                "They will look at you for a moment before charging at you\n" +
                "They can do damage to both you and enemies\n" +
               CalamityUtils.ColorMessage(PoeticTooltipLine, CalamityGlobalItem.ExhumedTooltipColor));
        }

        public override void SetDefaults()
        {
            item.mana = 10;
            item.damage = 1666;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.width = 58;
            item.height = 64;
            item.useTime = item.useAnimation = 30; // 29 because of useStyle 1
            item.autoReuse = true;
            item.noMelee = true;
            item.knockBack = 0f;
            item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            item.rare = ItemRarityID.Red;
            item.Calamity().customRarity = CalamityRarity.Violet;
            item.UseSound = SoundID.DD2_BetsySummon;
            item.shoot = ModContent.ProjectileType<CataclysmSummon>();
            item.shootSpeed = 10f;
            item.summon = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse != 2)
            {
                if (Main.rand.NextBool(2))
                    type = ModContent.ProjectileType<CatastropheSummon>();
                Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, type, damage, knockBack, player.whoAmI);
            }
            return false;
        }
    }
}