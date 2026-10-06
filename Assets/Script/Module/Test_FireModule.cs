using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个是测试用的开火节点
public class Test_FireModule : WeaponModule
{
    public override void Apply(AttackContext ctx)
    {
        ctx.isFireTrigger = true;
    }
}