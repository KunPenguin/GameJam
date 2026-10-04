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
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
