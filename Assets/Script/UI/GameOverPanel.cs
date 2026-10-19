//这个脚本被挂载在GameScene里的GameManager上
//这个脚本是用来控制玩家死亡后停止时间并弹出的结束面板的，并且可以重新开始。
//目前包含：
//1.Show()      —— 失败：显示面板、暂停、标题"游戏结束"
//2.ShowWin()   —— 胜利：显示面板、暂停、标题"你撑过了5天"
//3.Restart()   —— 重开一局
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;                                      // ★ 改动1：新增这一行

public class GameOverPanel : MonoBehaviour
{

    /// <summary>回到开始界面（死亡/胜利界面的按钮用）</summary>
    public void BackToStart()
    {
        // ⚠️ 必须先恢复时间：面板把 timeScale 设成了 0，
        //    不恢复的话，进新场景后整个游戏是"暂停"的（UI 能点，但什么都不动）。
        Time.timeScale = 1f;

        // 清空这一局的数据（天数、模块链、背包）
        if (GameData.Instance != null)
        {
            GameData.Instance.ResetForNewGame();
        }

        SceneManager.LoadScene("StartScene");
    }
    public GameObject panel;//将其与GameOverPanel关联

    [Header("标题文字（拖面板里那个 Text (TMP)）")]   // ★ 改动2：新增这4行
    public TextMeshProUGUI titleText;
    public string loseTitle = "游戏结束";
    public string winTitle = "你撑过了 5 天！";

    public void Show()
    {
        panel.SetActive(true);//显示结束面板
        if (titleText != null) titleText.text = loseTitle;   // ★ 改动3：新增
        Time.timeScale = 0f;//停止时间
    }
    public void Restart()
    {
        Time.timeScale = 1f;//恢复时间流动
        if (GameData.Instance != null)
        {
            GameData.Instance.ResetForNewGame();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);//重置其所在场景

    }
    public void ShowWin()
    {
        panel.SetActive(true);
        if (titleText != null) titleText.text = winTitle;    // ★ 改动4：新增
        Time.timeScale = 0f;
    }
}