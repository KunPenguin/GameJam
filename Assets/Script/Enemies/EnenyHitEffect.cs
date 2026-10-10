using UnityEngine;
using System.Collections;

public class EnemyHitEffect : MonoBehaviour                         //敌人受击时的闪白效果，我会尽量给注释，不难懂。欢迎来捉虫
{
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock mpb;                             // 性能优化：避免每次受击实例化新材质

    void Start()
    {                                                              // 找精灵的
        spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// 触发闪白效果
    /// </summary>
    /// <param name="duration">闪白持续时间</param>
    public void FlashWhite(float duration = 0.1f)
    {
        StopAllCoroutines();                                     // 防止连续受击导致闪烁异常（毕竟是三连发嘛）
        StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        // 开启闪白
        spriteRenderer.GetPropertyBlock(mpb);                   //整个的逻辑是先获取当前渲染器的属性，然后再把它变白，持续设定好的时间后，再变回正常的颜色
        mpb.SetFloat("_FlashAmount", 1f);
        spriteRenderer.SetPropertyBlock(mpb);

        // 等待指定时间
        yield return new WaitForSeconds(duration);

        // 恢复原状
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FlashAmount", 0f);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}