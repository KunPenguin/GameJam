//功能性组件，作为所有模块的基类无必须不用改，用途主要是让大家不用都去改PlayerShoot
using UnityEngine;

// 所有模块的"父类/基类"。
public abstract class WeaponModule
{
    public string displayName = "未命名模块";
    public string description = "（还没写说明）";
    public bool isFireModule = false; // 是否是开火模块，开火模块会触发 Fire 函数
    public System.Action<AttackContext> preFire = null;// "只有这一枪才生效"的改动，先放在这里面，等真的发射时执行
    // 这是"虚拟方法"，需要子类用 override 来重写它
    public abstract void Apply(AttackContext ctx);
}