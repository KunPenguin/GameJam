//这段代码被挂载在StartScene里的StartMenuManager上
//这段代码的作用是控制开始界面的UI，以后开始界面的ui也写在这，目前有：1.StartGame函数被绑定在“开始游戏按钮上”用于进入游戏界面
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");//跳转到游戏场景
    }
}
