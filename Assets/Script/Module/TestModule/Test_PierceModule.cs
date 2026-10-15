// 穿透：子弹能多穿一个敌人
public class Test_PierceModule : WeaponModule
{
    public Test_PierceModule()
    {
        displayName = "穿透";
        description = "子弹能多穿一个敌人";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.pierce = ctx.pierce + 1f;   // 加上去，不是直接赋值
    }
}