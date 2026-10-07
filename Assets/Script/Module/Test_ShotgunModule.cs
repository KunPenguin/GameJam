
public class Test_ShotgunModule : WeaponModule
{
    public Test_ShotgunModule()
    {
        displayName = "测试霰弹枪";
        description = "用于测试临时改变功能";
    }

    public override void Apply(AttackContext ctx)
    {
        // 这里可以修改 ctx 的属性，例如：
        ctx.bulletsPerWave+=2;
        ctx.spreadAngle = ctx.spreadAngle += 60;
        // 这里可以设置 preFire 委托来在开火前修改一次性属性，例如：
        preFire = delegate (AttackContext oneShot)
        {
            oneShot.damage = oneShot.damage * 0.6f;
            // oneShot.damage += 10f; // 只有自己伤害加10，后续模块不会继承这个加成
        };
        isFireModule = true;
    }
}
