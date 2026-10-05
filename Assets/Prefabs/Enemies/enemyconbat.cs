using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class enemyconbat : MonoBehaviour
{
    public float damage = 1f;
    void OnTriggerEnter2D(Collider2D other)  //这一段是碰撞
    {
        if (other.CompareTag("Player"))     //撞到的tag是不是player
        {
            Debug.Log("玩家被击中！");
            //新增：真的扣玩家血（调用玩家身上 PlayerHealth 脚本的 TakeDamage）
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
     
        }
    }

}
