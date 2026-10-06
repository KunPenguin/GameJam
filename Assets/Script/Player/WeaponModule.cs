//功能性组件，无必须不用改，用途主要是让大家不用都去改PlayerShoot
using UnityEngine;

// 所有模块的"父类/基类"。
public abstract class WeaponModule
{
    // 这是"虚拟方法"，需要子类用 override 来重写它
    public abstract void Apply(AttackContext ctx);
}