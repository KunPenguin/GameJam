//这个脚本被挂载在GameScene里的GameManager上
//这个脚本是用来控制玩家死亡后停止时间并弹出的结束面板的，并且可以重新开始。
//目前包含：
//1.Show()函数，用来显示结束面板，并停止时间。
//2.Restart()函数，用来重置游戏并继续时间流动
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    public GameObject panel;//将其与GameOverPanel关联

    public void Show()
    {
        panel.SetActive(true);//显示结束面板
        Time.timeScale = 0f;//停止时间
    }
    public void Restart()
    {
        Time.timeScale = 1f;//恢复时间流动
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);//重置其所在场景

    }
}
