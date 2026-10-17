//这是一个掌控刷怪的脚本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemycreate : MonoBehaviour
{
    public float rangedCheckRadius = 2f;   // 多大范围内算"这块地上有远程怪"
    public float playerSafeRadius = 10f;   // 玩家周围这么远以内不刷怪
    private Transform playerTransform;    // Start 里找一次就存着
    public GameObject closeenemy;
    public GameObject Midcloseenemy;
    public GameObject Highcloseenemy;
    public GameObject farenemy;
    public GameObject Midfarenemy;
    public GameObject Highfarenemy;
    public float spawnRate = 2;
    private DayWeights myRow;   // 当天的配置行，Start 里查一次
    private float currentDay;
    private float nextCommandTime;          // 下一次"发指令"的时刻
    private float pendingSpawnTime = -1f;   // 已收到指令、等待刷出的时刻；-1 表示没有
    [Header("生成近战怪物的概率，要小于等于1哦QAQ")]
    [Header("刷怪量")]
    public int number=0;
    // 这个刷怪点是不是离玩家太近
    bool TooCloseToPlayer()
    {
        if (playerTransform == null) return false;    // 没玩家（测试场景）就不拦
        float d = Vector2.Distance(transform.position, playerTransform.position);
        return d < playerSafeRadius;
    }
    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
        // 查当天配置（currentDay 是 Awake 里读的）
        if (EnemySpawnTable.Instance != null)
        {
            myRow = EnemySpawnTable.Instance.GetRow((int)currentDay);
        }
        float interval = (myRow != null) ? myRow.spawnInterval : spawnRate;
        nextCommandTime = Time.time;      // 第一道指令不等待周期
    }
    void Awake()
    {
       currentDay= (GameData.Instance != null) ? GameData.Instance.currentDay : 1; 
    }
    // Update is called once per frame
    void Update()
    {
        // 每帧读参数（Play 模式下改表立刻生效）
        float interval = (myRow != null) ? myRow.spawnInterval : spawnRate;
        interval = Mathf.Max(0.05f, interval);       // 防手滑填 0 → 每帧刷一群

        float dMin = (myRow != null) ? myRow.delayMin : 0f;
        float dMax = (myRow != null) ? myRow.delayMax : 1.5f;

        // 到点 → 发一道刷怪指令
        if (Time.time >= nextCommandTime)
        {
            nextCommandTime += interval;             // 排下一道

            // 自动夹住：延迟必须 <= 周期，否则下一道指令来了上一只还没刷，会积压
            float lo = Mathf.Clamp(dMin, 0f, interval);
            float hi = Mathf.Clamp(dMax, lo, interval);

            pendingSpawnTime = Time.time + Random.Range(lo, hi);
        }

        // 指令的随机延迟到了 → 真的刷
        if (pendingSpawnTime >= 0f && Time.time >= pendingSpawnTime)
        {
            pendingSpawnTime = -1f;
            shengcheng();
            number++;
        }
    }

    // 这块地附近是不是已经有远程怪了
    bool HasRangedNearby()
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;

            float d = Vector2.Distance(transform.position, all[i].transform.position);
            if (d > rangedCheckRadius) continue;             // 太远，不算这块地的

            if (all[i].GetComponent<RangedEnemy>() != null) return true;
        }
        return false;
    }

    void shengcheng()
    {
        // 玩家脚边不刷（放在最前面，省得白抽一次权重、白遍历一次敌人）
        if (TooCloseToPlayer()) return;
        int day = (GameData.Instance != null) ? GameData.Instance.currentDay : 1;

        bool allowRanged = (HasRangedNearby() == false);     // 附近有远程 → 不许再刷远程
        // 查表拿槽位；表没挂就返回 -1
        int slot = (EnemySpawnTable.Instance != null)? EnemySpawnTable.Instance.PickSlot(day, allowRanged): -1;

        // 槽位 → 预制体（用的是你已经拖好的那 6 个字段，不用重拖）
        GameObject prefab = null;
        switch (slot)
        {
            case 0: prefab = closeenemy; break;
            case 1: prefab = Midcloseenemy; break;
            case 2: prefab = Highcloseenemy; break;
            case 3: prefab = farenemy; break;
            case 4: prefab = Midfarenemy; break;
            case 5: prefab = Highfarenemy; break;
            default: prefab = closeenemy; break;   // 没表/没配 → 退回近战低档
        }

        if (prefab == null) return;
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}

