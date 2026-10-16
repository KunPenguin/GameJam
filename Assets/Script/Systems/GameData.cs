//这是一个被用来存放游戏内一部分无关角色怪物地图的数据的脚本
//目前有：
//1.当前日期
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;
    [Header("当前天数（非测试别手动改）")]
    public int currentDay = 1;//当前天数  
    [Header("模块链：8个插槽（千万别手动改）")]
    public string[] equipped = new string[]
    {
        "Test_FireModule",   // 第1格：开局自带的基础开火模块
        "",                  // 第2格：空
        "", "", "", "", "", ""   // 第3~8格：空
    };


    [Header("背包：3个格子（千万别手动改）")]
    public string[] backpack = new string[] { "", "", "" };


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else Destroy(gameObject);
    }
    /// <summary>
    /// 把这一局的所有数据清空，准备开始新的一局。
    /// 为什么要有这个方法：死亡重开 / 从主菜单开始新游戏，
    /// 都必须把模块链和背包清回初始状态 —— 否则会"天数回到第1天，但枪还是上一局的"。
    /// 以后 GameData 里再加新数据，也要在这里清一遍。
    /// </summary>
    public void ResetForNewGame()
    {
        // 天数回到第 1 天
        currentDay = 1;

        // 模块链：只留开局自带的基础开火模块，其余清空
        for (int i = 0; i < equipped.Length; i++)
        {
            equipped[i] = (i == 0) ? "Test_FireModule" : "";
        }

        // 背包清空
        for (int i = 0; i < backpack.Length; i++)
        {
            backpack[i] = "";
        }

        Debug.Log("新的一局：天数和模块已重置");
    }
}
