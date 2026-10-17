using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多目标指引管理器：只给【还没修好】的设备生成屏幕边缘图标。
///   · 自动发现：每隔 scanInterval 秒扫一次 DeviceRepair，新出现的设备自动配图标；
///   · 已经修好的设备（IsRepaired = true）不配图标；修好之后它的图标也会自动销毁，不会残留；
///   · 设备被销毁 → 对应图标自动销毁。
///
/// 图标本身的行为（贴边、隐藏、朝向）在 ObjectivePointer 里。
/// </summary>
public class DeviceManager : MonoBehaviour
{
    [Header("图标预制体（带 Image + ObjectivePointer）")]
    public GameObject pointerPrefab;

    [Header("图标挂在哪个 UI 父物体下（拖 Canvas）")]
    public Transform uiParent;

    [Header("角色（建议留空 = 每个图标自己按 Player 标签找）")]
    public Transform player;

    [Header("一开始就要指引的设备（可选，从场景 Hierarchy 拖）")]
    public List<Transform> targets = new List<Transform>();

    [Header("自动发现新设备（设备身上要有 DeviceRepair）")]
    public bool autoDetect = true;

    [Tooltip("每隔几秒扫描一次场景里有没有新设备")]
    public float scanInterval = 0.5f;

    [Header("最多同时显示几个（0 = 不限）")]
    public int maxIcons = 0;

    [Header("图标挤在一起时沿边缘错开多少像素（0 = 不错开）")]
    public float stackOffset = 0f;

    // 三个列表下标一一对应：目的地 / 图标 / 目的地的 DeviceRepair（可能为 null）
    readonly List<Transform> _targets = new List<Transform>();
    readonly List<ObjectivePointer> _icons = new List<ObjectivePointer>();
    readonly List<DeviceRepair> _repairs = new List<DeviceRepair>();

    float _scanTimer;

    // ------------------------------------------------------------------
    // 启动
    // ------------------------------------------------------------------
    void Start()
    {
        if (pointerPrefab == null) Debug.LogWarning("[指引] DeviceManager 没填 Pointer Prefab，生成不了图标。", this);
        if (uiParent == null) Debug.LogWarning("[指引] DeviceManager 没填 UI Parent（应该拖 Canvas），生成不了图标。", this);

        for (int i = 0; i < targets.Count; i++)
        {
            if (maxIcons > 0 && _icons.Count >= maxIcons) break;

            if (targets[i] == null)
            {
                Debug.LogWarning("[指引] Targets 第 " + (i + 1) + " 个槽是空的（None），已跳过。", this);
                continue;
            }

            Add(targets[i]);
        }

        int nullSlots = 0;
        for (int i = 0; i < targets.Count; i++) if (targets[i] == null) nullSlots++;

        Debug.Log("[指引] DeviceManager 启动：Targets 共 " + targets.Count + " 个（空槽 " + nullSlots
                  + " 个），已生成图标 " + _icons.Count + " 个；自动发现 = " + autoDetect + "。", this);

        if (autoDetect) ScanNewDevices();
    }

    // ------------------------------------------------------------------
    // 每帧
    // ------------------------------------------------------------------
    void Update()
    {
        // 1) 目的地被销毁 / 已经修好 → 图标没用了，销毁并剔除
        for (int i = _targets.Count - 1; i >= 0; i--)      // 倒着遍历，删元素才安全
        {
            if (IsStillWorthGuiding(i)) continue;

            if (_icons[i] != null) Destroy(_icons[i].gameObject);

            _targets.RemoveAt(i);
            _icons.RemoveAt(i);
            _repairs.RemoveAt(i);
        }

        // 2) 定时扫描新出现的设备
        if (!autoDetect) return;

        _scanTimer -= Time.deltaTime;
        if (_scanTimer > 0f) return;

        _scanTimer = Mathf.Max(0.1f, scanInterval);
        ScanNewDevices();
    }

    /// <summary>这个目的地还需要指引吗？（存在 + 还没修好）</summary>
    bool IsStillWorthGuiding(int i)
    {
        if (_targets[i] == null) return false;

        DeviceRepair r = _repairs[i];
        if (r != null && r.IsRepaired) return false;      // 已经修好了 → 不用再指

        return true;
    }

    // ------------------------------------------------------------------
    // 自动发现
    // ------------------------------------------------------------------
    /// <summary>扫一遍场景里的所有 DeviceRepair，没配过图标的、还没修好的，配一个图标。</summary>
    void ScanNewDevices()
    {
        if (pointerPrefab == null || uiParent == null) return;   // 没配好就别扫，免得刷警告

        DeviceRepair[] all = FindObjectsOfType<DeviceRepair>(true);   // true = 包含未激活的

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            if (all[i].IsRepaired) continue;                       // ★ 已修好的设备不配图标

            Transform t = all[i].transform;
            if (t == null || _targets.Contains(t)) continue;       // 已经配过就跳过

            int before = _icons.Count;
            Add(t);

            if (_icons.Count > before)
            {
                Debug.Log("[指引] 自动发现新设备：" + t.name + "，已配图标（现在共 " + _icons.Count + " 个）。", this);
            }
        }
    }

    // ------------------------------------------------------------------
    // 给一个目的地配一个图标
    // ------------------------------------------------------------------
    public void Add(Transform t)
    {
        if (t == null) return;
        if (pointerPrefab == null || uiParent == null) return;
        if (_targets.Contains(t)) return;                  // 同一个目的地只配一次
        if (maxIcons > 0 && _icons.Count >= maxIcons) return;

        GameObject go = Instantiate(pointerPrefab, uiParent);
        go.name = "Pointer_" + t.name;

        ObjectivePointer pointer = go.GetComponent<ObjectivePointer>();
        if (pointer == null)
        {
            Debug.LogWarning("[指引] 图标预制体上没有 ObjectivePointer 组件，已跳过：" + pointerPrefab.name, this);
            Destroy(go);
            return;
        }

        pointer.target = t;                                // ★ 一个图标 → 一个目的地
        pointer.player = player;                           // 留空也没关系，图标会自己按标签找
        pointer.stackIndex = _icons.Count;
        pointer.stackOffset = stackOffset;

        _targets.Add(t);
        _icons.Add(pointer);
        _repairs.Add(t.GetComponent<DeviceRepair>());      // 记下来，后面判断"修好了没"
    }
}
