//这是一个管理天数切换的UI的脚本，由刘堃制作维护，因大概率由我负责到底故不详细说明，如果你有涉及其的问题可以直接来问
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayTransitionUI : MonoBehaviour
{
    public TextMeshProUGUI dayText;

    void Start()
    {
        //从全局数据中读出天数
        int day = GameData.Instance.currentDay;
        dayText.text = "第" + day + "天结束";
    }
    public void Continue()
    {
        GameData.Instance.currentDay++;//天数加一
        SceneManager.LoadScene("GameScene");//回到游戏场景
    }
}


