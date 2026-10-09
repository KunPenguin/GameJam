//功能性组件，作为所有模块的基类无必须不用改，用途主要是让大家不用都去改PlayerShoot
using UnityEngine;

// 所有模块的"父类/基类"。
public abstract class WeaponModule
{
    // 卡片上显示的名字和说明，由子类在构造函数里填
    public string displayName = "未命名模块";
    public string description = "（还没写说明）";

    // 【第一件事】我这一轮要不要开火？
    // 普通属性模块保持 false；开火模块在 Apply 里把它设成 true。
    // 少了这个开关，枪不会响，而且不会报错。
    public bool isFireModule = false;

    // 【第二件事】"只有这一枪才生效"的改动，先贴在便条上，等真的发射时再执行。
    // System.Action<AttackContext> 的意思是：一段"接收一个数据包、没有返回值"的代码。
    // 默认 null = 这张便条是空的（属性模块不写它，所以是空的）。
    public System.Action<AttackContext> preFire = null;

    // 这是"虚拟方法"，需要子类用 override 来重写它
    public abstract void Apply(AttackContext ctx);
}
