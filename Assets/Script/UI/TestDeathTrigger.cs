//这个脚本被挂载在GameScene里的TestDeathTrigger上
//这是一个测试脚本，按下K键会自动死亡
using UnityEngine;

public class TestDeathTrigger : MonoBehaviour
{
    public GameOverPanel gameOverPanel; // 获取面板脚本

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))//如果按下k
        {
            gameOverPanel.Show(); // 调用显示方法
        }
    }
}
