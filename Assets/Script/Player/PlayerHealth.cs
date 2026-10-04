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
    public bool dead = false;//血量归零后就不再扣血了（死亡之后要做什么，等以后再加）

    void Start()
    {
        //开始游戏时把血回满，并打印一次，方便确认脚本挂上了
        currentHp = maxHp;
        Debug.Log("玩家血量：" + currentHp + " / " + maxHp);
    }

    //被怪打的时候调用（怪物基础伤害是 1）
    public void TakeDamage(float damage)
    {
        if (dead) return;

        currentHp = currentHp - damage;
        if (currentHp < 0f)
        {
            currentHp = 0f;
        }

        Debug.Log("玩家受到 " + damage + " 点伤害，当前血量 " + currentHp + " / " + maxHp);

        if (currentHp <= 0f)
        {
            dead = true;
            Debug.Log("玩家血量归零");
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
}
