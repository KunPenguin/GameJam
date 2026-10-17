// 每天的刷怪权重表。挂在 GameScene 的 GameManager 上。
// 只存权重数字；预制体引用还留在 Enemycreate 上，靠"槽位"对应。
using UnityEngine;

[System.Serializable]
public class DayWeights
{
    [Tooltip("第几天（1 = 第1天）")]
    public int day = 1;

    [Header("刷怪节奏")]
    [Tooltip("每隔多少秒给水域发一次刷怪指令")]
    public float spawnInterval = 2f;

    [Tooltip("收到指令后，最少等多少秒才真的刷出来")]
    public float delayMin = 0f;

    [Tooltip("收到指令后，最多等多少秒才真的刷出来（超过周期会被自动夹到周期内）")]
    public float delayMax = 1.5f;

    [Header("近战三档")]
    public float close;      // 近战 低档
    public float midClose;   // 近战 中档
    public float highClose;  // 近战 高档

    [Header("远程三档")]
    public float far;        // 远程 低档
    public float midFar;     // 远程 中档
    public float highFar;    // 远程 高档
}

public class EnemySpawnTable : MonoBehaviour
{
    public static EnemySpawnTable Instance;

    [Header("按天数从小到大排；没配到的天沿用最近的前一天")]
    public DayWeights[] days;

    private void Awake() { Instance = this; }

    /// <summary>抽一个槽位：0=近战低 1=近战中 2=近战高 3=远程低 4=远程中 5=远程高；-1=抽不出来</summary>
    public int PickSlot(int day, bool allowRanged = true)
    {
        DayWeights row = FindDay(day);
        if (row == null)
        {
            Debug.LogWarning("[刷怪表] 第 " + day + " 天没有配置");
            return -1;
        }

        // 把 6 个字段临时装进数组，后面就能用一套循环处理
        float[] w = { row.close, row.midClose, row.highClose,
                      row.far,   row.midFar,   row.highFar };
        // 不允许远程 → 把远程三个权重清零
        if (allowRanged == false)
        {
            w[3] = 0f;
            w[4] = 0f;
            w[5] = 0f;

            // 如果这天根本没配近战，只能兜底刷近战低档
            // （不然会一直返回 -1，整块地就不刷怪了）
            if (w[0] + w[1] + w[2] <= 0f) return 0;
        }


        // 第一步：算总长
        float total = 0f;
        for (int i = 0; i < w.Length; i++)
        {
            if (w[i] > 0f) total += w[i];      // 0 和负数都当"不刷"
        }
        if (total <= 0f)
        {
            Debug.LogWarning("[刷怪表] 第 " + day + " 天权重全是 0");
            return -1;
        }

        // 第二步：掷 0 ~ total
        float roll = Random.Range(0f, total);

        // 第三步：一段一段减，减到 ≤0 就是它
        for (int i = 0; i < w.Length; i++)
        {
            if (w[i] <= 0f) continue;
            roll -= w[i];
            if (roll <= 0f) return i;
        }

        // 浮点误差兜底
        for (int i = w.Length - 1; i >= 0; i--)
        {
            if (w[i] > 0f) return i;
        }
        return -1;
    }

    // 找这一天；找不到就用"天数 ≤ 目标"里最大的那一天
    private DayWeights FindDay(int day)
    {
        DayWeights best = null;
        for (int i = 0; i < days.Length; i++)
        {
            if (days[i] == null) continue;
            if (days[i].day == day) return days[i];
            if (days[i].day < day && (best == null || days[i].day > best.day))
                best = days[i];
        }
        return best;
    }
    [ContextMenu("自测：抽 1000 次看比例")]
    private void DebugDistribution()
    {
        int day = (GameData.Instance != null) ? GameData.Instance.currentDay : 1;
        int[] count = new int[6];
        string[] names = { "近战低", "近战中", "近战高", "远程低", "远程中", "远程高" };

        for (int i = 0; i < 1000; i++)
        {
            int slot = PickSlot(day);
            if (slot >= 0) count[slot]++;
        }

        string report = "[刷怪表] 第 " + day + " 天抽 1000 次：";
        for (int i = 0; i < 6; i++) report += "\n  " + names[i] + " = " + count[i];
        Debug.Log(report);
    }
    /// <summary>给刷怪器用：拿到当天的完整配置</summary>
    public DayWeights GetRow(int day)
    {
        return FindDay(day);
    }
}