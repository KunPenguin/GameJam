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
