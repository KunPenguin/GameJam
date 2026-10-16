// 6.聚焦模块：每波子弹个数-1，伤害+120%，子弹大小+100%，散射角-30

public class JuJiaoMoKuai : WeaponModule
{
    public JuJiaoMoKuai()
    {
        displayName = "“聚焦模块”";
        description = "每波子弹个数-1，伤害+120%，子弹大小+100%，散射角-30度";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.bulletsPerWave = ctx.bulletsPerWave - 1;
        ctx.damage = ctx.damage +1.2f;
        ctx.bulletSize = ctx.bulletSize + 1f;
        ctx.spreadAngle = ctx.spreadAngle - 30f;
    }
}