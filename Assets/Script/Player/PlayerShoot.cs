//这个脚本挂在 GameScene 里的 Player 物体上（一般和 PlayerMove 挂在同一个物体上）
//作用是按住鼠标左键，朝鼠标的方向连续发射水弹
//需要组件：不需要额外组件；但要先把水弹预制体拖到下面 Inspector 的 bulletPrefab 上
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;//水弹预制体（必填！把 Assets/Prefabs/Bullets 里的水弹预制体拖到这里）
    public Transform firePoint;//枪口位置（可以不填，不填就从玩家中心发射）
    public float fireInterval = 0.15f;//射速：隔多少秒打一发，越小打得越快
    public float timer;//运行时的冷却计时，不用手动改

    void Update()
    {
        //计时器一直往下减，减到 0 以下就可以再打一发
        timer = timer - Time.deltaTime;

        //没按住鼠标左键就不打
        if (Input.GetMouseButton(0) == false) return;

        //还在冷却中就不打
        if (timer > 0f) return;

        timer = fireInterval;
        Fire();
    }

    //发射一发水弹
    void Fire()
    {
        //忘了拖预制体时给个提示，免得以为是脚本坏了
        if (bulletPrefab == null)
        {
            Debug.LogWarning("还没有把水弹预制体拖到 PlayerShoot 的 bulletPrefab 上！");
            return;
        }

        //1.算出"玩家 → 鼠标"的方向
        //  ScreenToWorldPoint 把鼠标的屏幕坐标换成游戏里的世界坐标
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mouseWorld - transform.position;
        dir = dir.normalized;

        //2.子弹从哪儿出生（没填枪口就用玩家自己的位置）
        Vector3 spawnPosition = transform.position;
        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        //3.生成一颗水弹
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        //4.把子弹转到"朝着鼠标"的角度
        //  子弹脚本是沿自己的右方向飞的，所以这里必须转过来
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Debug.Log("发射水弹，方向 = " + dir);
    }
}
