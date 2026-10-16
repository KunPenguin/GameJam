// “水锤效应”：每波子弹个数+1，伤害+80%，散射角+30%


public class ShuiChuiXiaoYing : WeaponModule
{
    public ShuiChuiXiaoYing()
    {
        displayName = "“水锤效应”";
        description = "每波子弹个数+1，伤害+80%，散射角+30%";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.bulletsPerWave = ctx.bulletsPerWave + 1;
        ctx.damage = ctx.damage + 0.8f;
        ctx.spreadAngle = ctx.spreadAngle + 30f;
    }
}