//防护水幕：个数 + 1，伤害 - 50 %，吸血 + 1 %

public class FangHuShuiMu : WeaponModule
{
    public FangHuShuiMu()
    {
        displayName = "防护水幕";
        description = "每波子弹个数 + 1，伤害-50%，吸血+1%";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.bulletsPerWave = ctx.bulletsPerWave + 1;
        ctx.damage = ctx.damage - 0.5f;
        ctx.lifesteal = ctx.lifesteal + 0.01f;
    }
}
