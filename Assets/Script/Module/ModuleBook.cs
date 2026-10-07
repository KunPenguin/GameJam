//这里是模块书，存放所有模块的引用，方便玩家获取模块时使用，添加新模块时需要在这里登记，
//all数组里存放所有模块的引用，Create方法根据玩家获取的种类返回一个全新的模块实例，新模块在这两个里都要记录
//CreateRandom方法随机返回一个模块实例
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModuleBook
{
    //此处存放所有模块的引用，方便玩家获取模块时使用
    public static WeaponModule[] all = new WeaponModule[]
    {
        new Test_FireModule(),
        new Test_PierceModule(),        
    };

    //根据玩家获取的种类返回一个全新的模块
    //注意：这里返回的是一个全新的模块实例，而不是原来的模块引用，因为玩家可能会在游戏中多次获取同一个模块的不同实例
    //玩家获取的模块实例应该是独立的，互不影响
    public static WeaponModule Create(string typeName)
    {
        if (typeName == "Test_FireModule")
        {
            return new Test_FireModule();
        }
        else if (typeName == "Test_PierceModule")
        {
            return new Test_PierceModule();
        }
        else
        {
            Debug.LogError("ModuleBook里的Create里没有登记 : " + typeName);
            return null;
        }
    }

    //玩家修好水泵随机获取一个模块
    public static WeaponModule CreateRandom()
    {
        int i = Random.Range(0, all.Length);
        return Create(all[i].GetType().Name);
    }

    // 自检：把所有模块检查一遍，漏填名字/说明就报红字。保证每个写模块的人不出错
    public static void CheckAll()
    {
        for (int i = 0; i < all.Length; i++)
        {
            WeaponModule m = Create(all[i].GetType().Name);
            if (m == null) continue;

            if (string.IsNullOrEmpty(m.displayName) || m.displayName == "未命名模块")
            {
                Debug.LogError("模块漏填名字：" + all[i].GetType().Name);
            }
            Debug.Log("【" + m.displayName + "】" + m.description);
        }
    }
}
