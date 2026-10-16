using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    [Header("目标列表")]
    public List<Transform> targets = new List<Transform>(); // 所有需要追踪的目的地

    [Header("UI设置")]
    public GameObject indicatorPrefab; // 刚才做好的图标 Prefab
    public Transform canvasParent;     // 场景里的 Canvas

    void Start()
    {
        // 1. 遍历所有目标
        foreach (Transform target in targets)
        {
            if (target == null) continue;

            // 2. 实例化图标，并放到 Canvas 下
            GameObject indicatorObj = Instantiate(indicatorPrefab, canvasParent);

            // 3. 获取图标脚本，把目标塞进去
            DeviceManager indicatorScript = indicatorObj.GetComponent<DeviceManager>();
            indicatorScript.targets = targets;
        }
    }
}
