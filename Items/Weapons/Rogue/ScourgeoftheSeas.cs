using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class ScourgeoftheSeas : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scourge of the Seas");
            Tooltip.SetDefault("Snaps apart into a venomous cloud upon striking an enemy\n" +
            "Stealth strikes are coated with vile toxins, afflicting enemies with a powerful debuff");
        }

        public override void SafeSetDefaults()
        {
            item.width = 82;
            item.damage = 45;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.useAnimation = 20;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 20;
            item.knockBack = 3.5f;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.height = 82;
            item.value = Item.buyPrice(0, 36, 0, 0);
            item.rare = 5;
            item.shoot = ModContent.ProjectileType<ScourgeoftheSeasProjectile>();
            item.shootSpeed = 8f;
            item.Calamity().rogue = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
            {
                int stealth = Projectile.NewProjectile(position, new Vector2(speedX, speedY), ModContent.ProjectileType<ScourgeoftheSeasProjectile>(), damage, knockBack, player.whoAmI, 0f, 1f);
                Main.projectile[stealth].Calamity().stealthStrike = true;
                return false;
            }
            return true;
        }
    }
}
