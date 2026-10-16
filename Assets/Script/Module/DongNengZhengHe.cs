// 动能整合：伤害+80%，穿透-1，每波子弹个数-1，弹射-1，吸血+1%

public class DongNengZhengHe : WeaponModule
{
    public DongNengZhengHe()
    {
        displayName = "动能整合";
        description = "伤害+80%，穿透-1，每波子弹个数-1，弹射-1，吸血+1%";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.damage = ctx.damage + 0.8f;
        ctx.pierce = ctx.pierce - 1;
        ctx.bulletsPerWave = ctx.bulletsPerWave - 1;
        ctx.bounce = ctx.bounce - 1;
        ctx.lifesteal = ctx.lifesteal + 0.01f;
    }
}