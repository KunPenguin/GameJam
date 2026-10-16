// “泡泡机”：个数+2，伤害-50%，弹射+2，弹速-30%，散射角+60度

public class PaoPaoJi : WeaponModule
{
    public PaoPaoJi()
    {
        displayName = "“泡泡机”";
        description = "每波子弹个数+2，弹射次数+2，但是伤害-50%，弹速-30%，散射角+60度";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.bulletsPerWave = ctx.bulletsPerWave + 2;
        ctx.damage = ctx.damage - 0.5f;
        ctx.bounce = ctx.bounce + 2;
        ctx.bulletSpeed = ctx.bulletSpeed  - 0.3f;
        ctx.spreadAngle = ctx.spreadAngle + 60f;
    }
}