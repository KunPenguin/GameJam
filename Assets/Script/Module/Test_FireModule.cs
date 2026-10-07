// 这个是测试用的开火节点
public class Test_FireModule : WeaponModule
{
    public override void Apply(AttackContext ctx)
    {
        displayName = "测试开火模块";
        description = "发射一发子弹";
    //此处放置会继承给后方组件的属性修改代码，例如：
    //ctx.damage += 10f;就会给这一发子弹与之后的模块发射的子弹都加上10点伤害

    //此处放置不会继承给后方组件只作用于自己的属性修改代码，例如：
    preFire=delegate (AttackContext oneShot)
        {
            //如果这么写的话
            //oneShot.damage += 10f; // 只有自己伤害加10，后续模块不会继承这个加成
        };
        isFireModule = true;
    }
}