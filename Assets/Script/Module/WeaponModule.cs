//功能性组件，作为所有模块的基类无必须不用改，用途主要是让大家不用都去改PlayerShoot
using UnityEngine;

// 所有模块的"父类/基类"。
public abstract class WeaponModule
{
    public string displayName = "未命名模块";
    public string description = "（还没写说明）";
    // 这是"虚拟方法"，需要子类用 override 来重写它
    public abstract void Apply(AttackContext ctx);
}