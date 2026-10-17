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