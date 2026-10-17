using UnityEngine;
using UnityEditor;

/// <summary>
/// 一键把物体对齐到 Tilemap 格子中心（格子边界在整数，中心在 x.5）。
/// 菜单位置：顶部菜单 → 工具 → 对齐格子。
/// </summary>
public static class GridSnapTool
{
    [MenuItem("工具/对齐格子：选中的物体")]
    static void SnapSelected()
    {
        GameObject[] sel = Selection.gameObjects;
        if (sel == null || sel.Length == 0)
        {
            Debug.LogWarning("[对齐] 先在 Hierarchy 里选中要对的物体。");
            return;
        }

        int n = Snap(sel);
        Debug.Log("[对齐] 已把 " + n + " 个物体对齐到格子中心。");
    }

    [MenuItem("工具/对齐格子：场景里所有设备")]
    static void SnapAllDevices()
    {
        DeviceRepair[] all = Object.FindObjectsOfType<DeviceRepair>(true);
        if (all == null || all.Length == 0)
        {
            Debug.LogWarning("[对齐] 场景里没找到挂 DeviceRepair 的设备。");
            return;
        }

        int n = 0;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;

            Transform root = GetDeviceRoot(all[i].transform);
            if (root == null) continue;

            n += Snap(new GameObject[] { root.gameObject });
        }

        Debug.Log("[对齐] 已把 " + n + " 台设备对齐到格子中心。");
    }

    /// <summary>往上找"画着贴图的那一层"（设备根物体）。</summary>
    static Transform GetDeviceRoot(Transform t)
    {
        while (t.parent != null && t.parent.GetComponent<SpriteRenderer>() != null)
            t = t.parent;

        return t;
    }

    static int Snap(GameObject[] gos)
    {
        int n = 0;

        for (int i = 0; i < gos.Length; i++)
        {
            if (gos[i] == null) continue;

            Transform t = gos[i].transform;

            Undo.RecordObject(t, "Snap to grid center");

            Vector3 p = t.position;
            t.position = new Vector3(Mathf.Floor(p.x) + 0.5f,
                                     Mathf.Floor(p.y) + 0.5f,
                                     p.z);

            // 预制体实例要额外记录修改，否则可能不写回场景
            if (PrefabUtility.IsPartOfPrefabInstance(t))
                PrefabUtility.RecordPrefabInstancePropertyModifications(t);

            EditorUtility.SetDirty(t);
            n++;
        }

        return n;
    }
}
