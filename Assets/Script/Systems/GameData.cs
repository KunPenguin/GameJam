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

    [Header("模块链：8个插槽（空的用空字符串）")]
    public string[] equipped = new string[]
    {
        "Test_FireModule",   // 第1格：开局自带的基础开火模块
        "",                  // 第2格：空
        "", "", "", "", "", ""   // 第3~8格：空
    };


    [Header("背包：3个格子（空的用空字符串）")]
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
}
