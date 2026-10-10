// 模块名册：负责按名字造模块和发牌
// 加新模块时要来三个地方登记：
//   1. 新建模块类
//   2. types 数组加一行
//   3. Create 里加一段 if
using System.Collections.Generic;
using UnityEngine;

public static class ModuleBook
{
    //目前共有哪几种模块
    public static string[] types = new string[]
    {
        "Test_FireModule",
        "Test_PierceModule",
        "Test_ShotgunModule",
    };

    //按名字造模块
    public static WeaponModule Create(string typeName)
    {
        WeaponModule m = null;

        if (typeName == "Test_FireModule")
        {
            m = new Test_FireModule();
        }
        else if (typeName == "Test_PierceModule")
        {
            m = new Test_PierceModule();
        }
        else if (typeName == "Test_ShotgunModule")
        {
            m = new Test_ShotgunModule();
        }
        else
        {
            Debug.LogError("ModuleBook里没有登记这个模块" + typeName);
            return null;
        }
        return m;
    }

    //随机发放模块
    public static WeaponModule CreateRandom()
    {
        int i = Random.Range(0, types.Length);
        return Create(types[i]);
    }

    //随机抽取count个模块
    public static List<string> CreateRandomList(int count)
    {
        //先把所有模块名放进一个池子里
        List<string> pool = new List<string>();
        for (int i = 0; i < types.Length; i++)
        {
            pool.Add(types[i]);
        }

        //一个一个抽，每抽一个就从池子里删掉，保证不重复
        List<string> result = new List<string>();
        for (int i = 0; i < count; i++)
        {
            //池子空了会停
            if (pool.Count == 0)
            {
                break;
            }
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return result;
    }
    //取模块的名字和说明
    public static bool GetInfo(string typeName, out string displayName, out string description)
    {
        WeaponModule m = Create(typeName);
        if (m == null)
        {
            displayName = "未知模块";
            description = "（没有登记在ModuleBook里）";
            return false;
        }

        displayName = m.displayName;
        description = m.description;
        return true;
    }
    //自检模块，保证没有漏填名字或说明
    public static void CheckAll()
    {
        for (int i = 0; i < types.Length; i++)
        {
            WeaponModule m = Create(types[i]);
            if (m == null) continue;

            if (string.IsNullOrEmpty(m.displayName) || m.displayName == "未命名模块")
            {
                Debug.LogError("模块漏填名字：" + types[i]);
            }
            Debug.Log("【" + m.displayName + "】" + m.description);
        }
    }
}   
