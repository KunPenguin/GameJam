// 二次充能：子弹波数波数+1，伤害+80%，弹速+20%

public class ErCiChongNeng : WeaponModule
{
    public ErCiChongNeng()
    {
        displayName = "“二次充能”";
        description = "子弹波数+1，伤害+80%，弹速+20%";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.waveCount = ctx.waveCount + 1;
        ctx.damage = ctx.damage + 0.8f;
        ctx.bulletSpeed = ctx.bulletSpeed + 0.2f;
    }
}