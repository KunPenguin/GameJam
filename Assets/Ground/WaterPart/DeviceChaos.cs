using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceChaos : MonoBehaviour
{
    [Header("损坏设备的预制体（挂 DeviceRepair 的那个：Damaged Device）")]
    public GameObject damagedDevicePrefab;

    [Header("冷却（秒）：防止同一帧连锁触发")]
    public float cooldown = 0.3f;

    [Header("至少保留几台正常设备（0 = 不保底）")]
    public int keepWorking = 1;

    [Header("打印日志")]
    public bool logEnabled = true;

    float _lastTime = -999f;
    // Start is called before the first frame update
    void OnEnable()
    {
        DeviceRepair.Repaired += OnAnyRepaired;      // 订阅
    }
    void OnDisable()
    {
        DeviceRepair.Repaired -= OnAnyRepaired;      // 必须退订，否则场景重载后会重复触发
    }
    void OnAnyRepaired(DeviceRepair justRepaired)
    {
        if (damagedDevicePrefab == null) return;
        if (Time.time - _lastTime < cooldown) return;    // 冷却中
        _lastTime = Time.time;

        // ---------- 1) 收集所有"正常设备" ----------
        DeviceMarker[] all = FindObjectsOfType<DeviceMarker>(true);   // true = 含未激活
        List<DeviceMarker> candidates = new List<DeviceMarker>();

        Vector3 fixedPos = justRepaired != null ? justRepaired.transform.position : Vector3.zero;

        for (int i = 0; i < all.Length; i++)
        {
            DeviceMarker m = all[i];
            if (m == null) continue;

            // 刚修好的那台：就在这个位置新生成的，排除掉（别修好立刻又坏）
            if (Vector3.Distance(m.transform.position, fixedPos) < 0.5f) continue;

            candidates.Add(m);
        }

        if (candidates.Count <= keepWorking)
        {
            if (logEnabled) Debug.Log("[设备] 没有可以弄坏的设备了（剩余 " + candidates.Count + " 台正常）。");
            return;
        }

        // ---------- 3) 随机挑一台 ----------
        DeviceMarker victim = candidates[Random.Range(0, candidates.Count)];

        Vector3 pos = victim.transform.position;
        Quaternion rot = victim.transform.rotation;
        Transform parent = victim.transform.parent;
        string victimName = victim.name;

        // ---------- 4) 换成损坏设备 ----------
        Destroy(victim.gameObject);                                  // 先移除正常外观
        GameObject dmg = Instantiate(damagedDevicePrefab, pos, rot, parent);
        dmg.name = "Damaged_" + victimName;

        if (logEnabled) Debug.Log("[设备] " + victimName + " 坏掉了 → " + dmg.name, dmg);
    }
}

