// 模块界面的数据读写入口。
// 模块链/背包/奖励区三个区读写的写法几乎一样，
// 统一在这里做成两个方法，UI 代码就不用写三套分支。
using UnityEngine;

public static class ModulePanelData
{
    //奖励区：只在这个界面出现，不跨场景
    public static string[] reward = new string[1];

    /// <summary>
    /// 读：读取这个区的这一格放的模块名字，如果没有放模块，返回""
    /// </summary>
    /// <param name="area"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string Read(ModuleArea area, int index)
    {
        if(area==ModuleArea.Equip)
        {
            if(!CheckLength(GameData.Instance!=null?GameData.Instance.equipped.Length:0,index,area))return "";
            return GameData.Instance.equipped[index];
        }
        if(area == ModuleArea.Backpack)
        {
            if (!CheckLength(GameData.Instance != null ? GameData.Instance.backpack.Length : 0, index, area)) return "";
            return GameData.Instance.backpack[index];
        }
        if(area == ModuleArea.Reward)
        {
            if (!CheckLength(reward.Length, index, area)) return "";
            return reward[index];
        }

        //丢弃区不存储数据，直接返回空
        return "";
    }

    /// <summary>
    /// 写：把模块名字写入这个区的这一格，传入""表示清空这一格
    /// </summary>
    /// <param name="area"></param>
    /// <param name="index"></param>
    /// <param name="moduleName"></param>
    public static void Write(ModuleArea area, int index, string moduleName)
    {
        if (area == ModuleArea.Equip)
        {
            if (!CheckLength(GameData.Instance != null ? GameData.Instance.equipped.Length : 0, index, area)) return;
            GameData.Instance.equipped[index] = moduleName;
            return;
        }
        if (area == ModuleArea.Backpack)
        {
            if (!CheckLength(GameData.Instance != null ? GameData.Instance.backpack.Length : 0, index, area)) return;
            GameData.Instance.backpack[index] = moduleName;
            return;
        }
        if (area == ModuleArea.Reward)
        {
            if (!CheckLength(reward.Length, index, area)) return;
            reward[index] = moduleName;
            return;
        }
        //丢弃区不存储数据，直接返回
    }

    //长度检查：格子的 index 超出数组长度时报错，而不是让 Unity 抛越界异常
    private static bool CheckLength(int arrayLength, int index, ModuleArea area)
    {
        if (index < 0 || index >= arrayLength)
        {
            Debug.LogError($"ModulePanelData: {area} 区的格子 index 越界，index={index}，该区总格子数只有={arrayLength}格");
            return false;
        }
        return true;
    }
}