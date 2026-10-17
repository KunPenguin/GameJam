// 挂在表盘根物体（TimerDial）上。
// 职责：每帧把 DayTimer 的剩余时间比例，喂给那个扇形 Image 的 fillAmount。
using UnityEngine;
using UnityEngine.UI;

public class DayTimerDial : MonoBehaviour
{
    [Header("那个扇形（Image Type 必须是 Filled）")]
    public Image fillImage;

    [Header("场景里的计时器（拖 DayManager 那个物体）")]
    public DayTimer timer;

    private void Update()
    {
        if (fillImage == null || timer == null) return;

        float total = Mathf.Max(0.01f, timer.GetDayLength());   // 防除零
        float ratio = timer.GetTimeLeft() / total;

        fillImage.fillAmount = Mathf.Clamp01(ratio);            // 1 → 0
    }

    // 如果当前天数 >= bossDay，就把这个表盘隐藏掉
    private void Start()
    {
        if (timer == null) return;
        int day = (GameData.Instance != null) ? GameData.Instance.currentDay : 1;
        if (day >= timer.bossDay)
        {
            gameObject.SetActive(false);
        }
    }
}