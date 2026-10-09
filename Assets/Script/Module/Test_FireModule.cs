// 这个是测试用的开火节点
public class Test_FireModule : WeaponModule
{
    // 构造函数：出生时就把名片填好
    public Test_FireModule()
    {
        displayName = "开火";
        description = "击发一次。没有它，枪不响";
    }

    public override void Apply(AttackContext ctx)
    {
        //此处放置会继承给后方组件的属性修改代码，例如：
        //ctx.damage += 10f;就会给这一发子弹与之后的模块发射的子弹都加上10点伤害

        //此处放置不会继承给后方组件只作用于自己的属性修改代码，例如：
        preFire = delegate (AttackContext oneShot)
        {
            //如果这么写的话
            //oneShot.damage += 10f; // 只有自己伤害加10，后续模块不会继承这个加成
        };

        // 【必须】通知管道：我是开火模块，请发射
        // 少了这一行，枪不会响，而且不会报任何错
        isFireModule = true;
    }
}
