using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    public class RadiatingCrystal : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiating Crystal");
            Tooltip.SetDefault("Summons a radiator light pet\n" +
                "The crystal contains traces of holothurin");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.WispinaBottle);
            item.shoot = ModContent.ProjectileType<RadiatorPet>();
            item.buffType = ModContent.BuffType<RadiatorBuff>();
            item.value = Item.sellPrice(gold: 1);
            item.rare = 3;
        }

        public override void UseStyle(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(item.buffType, 3600, true);
            }
        }
    }
}
