using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    [Header("图标预制体（带 Image + ObjectivePointer）")]
    public GameObject pointerPrefab;

    [Header("图标挂在哪个 UI 父物体下（拖 Canvas）")]
    public Transform uiParent;

    [Header("角色（可选）。留空 = 每个图标自己按标签找")]
    public Transform player;

    [Header("目的地列表（手动摆的设备，从 Hierarchy 拖进来）")]
    public List<Transform> targets = new List<Transform>();

    [Header("最多同时显示几个（0 = 不限）")]
    public int maxIcons = 0;

    [Header("图标挤在一起时沿边缘错开多少像素（0 = 不错开）")]
    public float stackOffset = 0f;

    readonly List<Transform> _targets = new List<Transform>();
    readonly List<ObjectivePointer> _icons = new List<ObjectivePointer>();

    void Start()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (maxIcons > 0 && _icons.Count >= maxIcons) break;
            if (targets[i] == null) continue;

            Add(targets[i]);
        }
    }

    void Update()
    {
        // 目的地被销毁（设备修好了）→ 销毁对应图标，并从列表剔除
        for (int i = _targets.Count - 1; i >= 0; i--)      // 倒着遍历，删元素才安全
        {
            if (_targets[i] != null) continue;

            if (_icons[i] != null) Destroy(_icons[i].gameObject);

            _targets.RemoveAt(i);
            _icons.RemoveAt(i);
        }
    }

    /// <summary>给一个新目的地配一个图标。</summary>
    public void Add(Transform t)
    {
        if (t == null || pointerPrefab == null || uiParent == null) return;
        if (_targets.Contains(t)) return;                  // 别重复配
        if (maxIcons > 0 && _icons.Count >= maxIcons) return;

        GameObject go = Instantiate(pointerPrefab, uiParent);
        go.name = "Pointer_" + t.name;

        ObjectivePointer pointer = go.GetComponent<ObjectivePointer>();
        if (pointer == null) { Destroy(go); return; }      // 预制体上没挂脚本 → 别留垃圾

        pointer.target = t;                                // ★ 一个图标 → 一个目的地
        pointer.player = player;
        pointer.stackIndex = _icons.Count;
        pointer.stackOffset = stackOffset;

        _targets.Add(t);
        _icons.Add(pointer);
    }
}
