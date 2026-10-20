//这个脚本挂在 GameScene 里的 Player 物体上（和 PlayerMove、PlayerShoot 挂在同一个物体上）
//作用：管理玩家血量：被怪打会扣血，吸血会回血，每次血量变化都在 Console 打印出来
//需要组件：无
//怎么被别人调用：
//    怪打玩家：player.SendMessage("TakeDamage", 1f);   //1f 就是怪物基础伤害 1
//    子弹吸血：bulletScript 里直接调用 playerHealth.Heal(回复量);
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHp = 20f;//最大血量（基础血量 20）
    public float currentHp = 20f;//当前血量
    public bool dead = false;//血量归零后就不再扣血了（死亡之后跳出死亡画面）
    public GameOverPanel gameOverPanel;

    // ✨【新增这两行】
    private SpriteRenderer sr;
    private Coroutine flashCoroutine;
    void Start()
    {
        //开始游戏时把血回满，并打印一次，方便确认脚本挂上了
        currentHp = maxHp;
        Debug.Log("玩家血量：" + currentHp + " / " + maxHp);

        // ✨【新增这一行】获取玩家身上的图片渲染器
        sr = GetComponent<SpriteRenderer>();

        // 【新增】自动找死亡面板...
        if (gameOverPanel == null)
            

            // 【新增】自动找死亡面板。
            // 为什么必须自动找：PlayerHealth 挂在 Player 预制体上，
            // 而 GameOverPanel 是场景里的物体 —— 预制体没法引用场景物体，
            // 所以在 Inspector 里是拖不上的，只能运行时找。
            if (gameOverPanel == null)
        {
            gameOverPanel = FindObjectOfType<GameOverPanel>();
        }

        if (gameOverPanel == null)
        {
            Debug.LogWarning("场景里找不到 GameOverPanel，玩家死亡时不会弹面板！");
        }
    }

    //被怪打的时候调用（怪物基础伤害是 1）
    public void TakeDamage(float damage)
    {
        if (dead) return;

        currentHp = currentHp - damage;
        StartFlashRed(); //闪红
        DeviceRepair.NotifyPlayerDamaged();
        //新增：播放"受伤"音效
        if (SfxPlayer.instance != null) SfxPlayer.instance.PlayHurt();
        if (currentHp < 0f)
        {
            currentHp = 0f;
        }

        Debug.Log("玩家受到 " + damage + " 点伤害，当前血量 " + currentHp + " / " + maxHp);

        if (currentHp <= 0f)
        {
            dead = true;
            Debug.Log("玩家血量归零");
            if (gameOverPanel != null)
            {
                gameOverPanel.Show(); // 弹出结束面板并暂停时间
            }
            else
            {
                Debug.LogWarning("PlayerHealth 没有绑定 GameOverPanel！");
            }
        }
    }

    //吸血回血时调用（回复量 = 子弹伤害 × 吸血比例）
    public void Heal(float amount)
    {
        if (dead) return;
        if (amount <= 0f) return;

        currentHp = currentHp + amount;
        if (currentHp > maxHp)
        {
            currentHp = maxHp;//回血不能超过上限
        }

        Debug.Log("吸血回复 " + amount + " 点，当前血量 " + currentHp + " / " + maxHp);
    }
    // ✨【新增】纯代码实现闪红逻辑
    private void StartFlashRed()
    {
        if (sr == null) return;

        // 如果之前正在闪，先停掉，防止颜色卡住
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        // 开始新的闪红
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        // 将图片颜色设为红色
        sr.color = Color.red;

        // 等待 0.15 秒
        yield return new WaitForSeconds(0.15f);

        // 恢复原色（白色代表不改变原图颜色）
        sr.color = Color.white;
    }
}
