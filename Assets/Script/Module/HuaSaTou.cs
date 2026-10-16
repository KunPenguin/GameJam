// “花洒头”：波数+1，个数+1，散射角+60度

public class HuaSaTou : WeaponModule
{
    public HuaSaTou()
    {
        displayName = "“花洒头”";
        description = "每波子弹个数+1，弹射次数+1，散射角+60度";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.bulletsPerWave = ctx.bulletsPerWave + 1;
        ctx.bounce = ctx.bounce + 1;
        ctx.spreadAngle = ctx.spreadAngle + 60f;
    }
}