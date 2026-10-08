//这个脚本挂在 GameScene 里的 Player 物体上（这个物体的 Tag 必须是 Player）
//作用是让玩家用 W A S D 上下左右移动
//需要组件：同一个物体上要有 Rigidbody2D（Gravity Scale 必须改成 0）
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;//移动速度（单位/秒），在 Inspector 里改
    public Rigidbody2D rb;//刚体组件（Inspector 里留空的话，会自动找自己身上的）
    private WaterSlowdown waterSlow;
    void Start()
    {
        //忘了拖的话就自动拿自己身上的 Rigidbody2D
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        } 
        waterSlow = GetComponent<WaterSlowdown>();
        //2D 俯视角不需要重力，不然角色会自己往下掉
        rb.gravityScale = 0f;
        //撞到东西不要原地打转
        rb.freezeRotation = true;
    }

    void Update()
    {
        //1.读键盘：按住哪个键就是 -1、0 或 1
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A)) x = -1f;//左
        if (Input.GetKey(KeyCode.D)) x = 1f;//右
        if (Input.GetKey(KeyCode.S)) y = -1f;//下
        if (Input.GetKey(KeyCode.W)) y = 1f;//上

        //2.拼成一个方向
        Vector2 dir = new Vector2(x, y);

        //3.同时按两个键（比如 W + D）斜着走时，速度不要变快
        if (dir.magnitude > 1f)
        {
            dir = dir.normalized;     
        }

        //4.把速度交给刚体，由物理系统负责真正移动（撞墙不会穿过去）
        //  注意：不能写 transform.Translate，那样会穿过墙和敌人
        float multiplier = (waterSlow != null) ? waterSlow.SpeedMultiplier : 1f;
        rb.velocity = dir * moveSpeed * multiplier;
    }
}
