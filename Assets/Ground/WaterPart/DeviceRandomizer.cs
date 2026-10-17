using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 开局随机器：把场景里现有的设备"位置"收集起来，随机决定哪几台是【正常】、哪几台是【损坏】。
///
/// · 场景里不用手动摆 6 个空的槽位 —— 它会读取现有的设备位置（挂 DeviceRepair 的 + 挂 DeviceMarker 的）；
/// · 只重建"状态不对"的那几台，本来就对的保持原样不动；
/// · 执行顺序调到 -200：保证它在 DeviceManager（自动配图标）之前跑完。
///
/// 用法：把这脚本挂到场景里任意一个物体上（比如你的 GameManager / DeviceChaos 所在的物体），
///       然后拖 3 个字段：正常设备预制体、损坏设备预制体、（可选）水。
/// </summary>
[DefaultExecutionOrder(-200)]
public class DeviceRandomizer : MonoBehaviour
{
    [Header("正常设备预制体（CompleteDevice）")]
    public GameObject normalPrefab;

    [Header("损坏设备预制体（Damaged Device）")]
    public GameObject damagedPrefab;

    [Header("开局要随机几台是【正常】的")]
    public int normalCount = 3;

    [Header("（可选）水预制体：水如果是设备的子物体，填这里，给新生成的损坏设备补一份水")]
    public GameObject waterPrefab;

    [Header("固定随机种子（0 = 每局都不同；填 12345 之类 = 每局都一样，方便调试）")]
    public int seed = 0;

    void Start()
    {
        Randomize();
    }

    /// <summary>重新随机一次（也可以在编辑器里用右键菜单调）。</summary>
    [ContextMenu("重新随机")]
    public void Randomize()
    {
        // ---------- 1) 收集场景里现有的设备位置 ----------
        List<Transform> slots = new List<Transform>();

        // 损坏的设备：DeviceRepair 挂在子物体（Trident）上，所以要往上找根物体
        DeviceRepair[] repairs = FindObjectsOfType<DeviceRepair>(true);
        for (int i = 0; i < repairs.Length; i++)
        {
            if (repairs[i] == null) continue;

            Transform root = GetDeviceRoot(repairs[i].transform);
            if (root != null && !slots.Contains(root)) slots.Add(root);
        }

        // 正常的设备：身上有 DeviceMarker
        DeviceMarker[] markers = FindObjectsOfType<DeviceMarker>(true);
        for (int i = 0; i < markers.Length; i++)
        {
            if (markers[i] == null) continue;

            Transform t = markers[i].transform;
            if (!slots.Contains(t)) slots.Add(t);
        }

        if (slots.Count == 0)
        {
            Debug.LogWarning("[随机开局] 场景里一个设备都没找到（既没有 DeviceRepair 也没有 DeviceMarker）。", this);
            return;
        }

        int normalTarget = Mathf.Clamp(normalCount, 0, slots.Count);

        // ---------- 2) 打乱顺序 ----------
        if (seed != 0) Random.InitState(seed);

        for (int i = slots.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform tmp = slots[i];
            slots[i] = slots[j];
            slots[j] = tmp;
        }

        // ---------- 3) 前 normalTarget 个 → 正常，其余 → 损坏 ----------
        int changed = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            Transform slot = slots[i];
            bool wantNormal = i < normalTarget;

            bool isDamagedNow = slot.GetComponentInChildren<DeviceRepair>(true) != null;
            bool isNormalNow = !isDamagedNow;

            if (wantNormal == isNormalNow) continue;   // 已经是想要的状态，不动它

            BuildAt(slot, wantNormal);
            changed++;
        }

        int normalNow = 0;
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] != null && slots[i].GetComponentInChildren<DeviceRepair>(true) == null) normalNow++;

        Debug.Log("[随机开局] 共 " + slots.Count + " 台设备：正常 " + normalTarget
                  + " 台 / 损坏 " + (slots.Count - normalTarget) + " 台；本次重建了 " + changed + " 台。", this);
    }

    /// <summary>在 slot 的位置重建一台设备（先销毁原件，再生成目标预制体）。</summary>
    void BuildAt(Transform slot, bool wantNormal)
    {
        Vector3 pos = slot.position;
        Quaternion rot = slot.rotation;
        Transform parent = slot.parent;
        string nm = slot.name;

        Destroy(slot.gameObject);          // 连同它的子物体（水 / Trident）一起消失

        GameObject go = null;

        if (wantNormal)
        {
            if (normalPrefab == null)
            {
                Debug.LogWarning("[随机开局] 没填 Normal Prefab，跳过：" + nm, this);
                return;
            }

            go = Instantiate(normalPrefab, pos, rot, parent);
            go.name = "Normal_" + nm;
        }
        else
        {
            if (damagedPrefab == null)
            {
                Debug.LogWarning("[随机开局] 没填 Damaged Prefab，跳过：" + nm, this);
                return;
            }

            go = Instantiate(damagedPrefab, pos, rot, parent);
            go.name = "Damaged_" + nm;

            // 给新生成的损坏设备补一份水（如果水是设备子物体的话）
            if (waterPrefab != null)
            {
                GameObject w = Instantiate(waterPrefab, pos, rot, go.transform);
                w.name = "Water";
            }
        }

        Debug.Log("[随机开局] " + nm + " → " + (wantNormal ? "正常" : "损坏"), go);
    }

    /// <summary>往上找"画着贴图的那一层"（设备根物体）。</summary>
    Transform GetDeviceRoot(Transform t)
    {
        while (t.parent != null && t.parent.GetComponent<SpriteRenderer>() != null)
            t = t.parent;

        return t;
    }
}
