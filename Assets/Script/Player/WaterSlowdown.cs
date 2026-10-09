using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSlowdown : MonoBehaviour
{
    [Header("放Water图层")]
    public LayerMask waterLayers;

    [Header("在水里的速度倍率：0.5 = 半速，0.3 = 三成速")]
    [Range(0.05f, 1f)]
    public float waterSpeedMultiplier = 0.5f;

    [Header("速度变化的平滑度：0 = 瞬间切换，8 = 大约 0.3 秒过渡")]
    public float smooth = 8f;

    [Header("检测半径：小一点，免得站在岸边就被判定进水")]
    public float checkRadius = 0.15f;

    /// <summary>当前速度倍率（1 = 正常，在水里 = waterSpeedMultiplier）</summary>
    public float SpeedMultiplier { get; private set; } = 1f;

    /// <summary>现在是不是在水里（其他脚本想用可以直接读）</summary>
    public bool InWater { get; private set; }

    private void Update()
    {
        // ① 以自己为中心画一个小圆，看有没有碰到「水」层的东西
        InWater = Physics2D.OverlapCircle(transform.position, checkRadius, waterLayers) != null;

        // ② 算出目标倍率，再用 Lerp 平滑过去（避免速度"啪"一下变掉）
        float target = InWater ? waterSpeedMultiplier : 1f;
        SpeedMultiplier = Mathf.Lerp(SpeedMultiplier, target, smooth * Time.deltaTime);
    }
}
