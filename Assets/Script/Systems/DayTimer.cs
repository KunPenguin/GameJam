// 每天的限时计时器。
// 挂在 GameScene 里新建的 DayManager 物体上。
//
// 作用：每天倒计时 dayLength 秒，时间到就自动进入「天数过渡场景」，
//       过渡场景里点「继续」会 currentDay+1 并回到 GameScene，开始新的一天。
//
// 设计要点：
//   1. 用 Time.deltaTime —— 所以模块面板打开时（Time.timeScale = 0）计时会自动暂停。
//      这是有意为之：整理模块不该算进 3 分钟里。
//   2. 第 10 天是 Boss 战，不限时，所以到第 10 天计时器不启动（Boss 以后再做）。
//   3. 到点加载的是字符串场景名 "DayTransitionScence"
//      ⚠️ 注意 Scene 被拼成了 Scence，这是项目里一直沿用的名字，别改。
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DayTimer : MonoBehaviour
{
    [Header("每天多少秒（180 = 3 分钟）")]
    public float dayLength = 180f;

    [Header("倒计时文字（暂时留空，以后换成进度条）")]
    public TextMeshProUGUI timerText;

    [Header("多少天之后开始 Boss 战（第 10 天及以后不计时）")]
    public int bossDay = 10;

    [Header("过渡场景的名字（别改，项目里就是拼成 Scence 的）")]
    public string transitionSceneName = "DayTransitionScence";

    // 剩余时间
    private float timeLeft;

    // 是否已经开始计时（用来区分「还没开始」和「时间到了」）
    private bool running = false;

    // 防止一帧里触发两次跳转
    private bool ended = false;

    private void Start()
    {
        int day = (GameData.Instance != null) ? GameData.Instance.currentDay : 1;

        // 第 10 天及以后是 Boss 战，不限时
        if (day >= bossDay)
        {
            running = false;
            Debug.Log("第 " + day + " 天是 Boss 战，不计时");
            if (timerText != null) timerText.text = "";
            return;
        }

        timeLeft = dayLength;
        running = true;
        Debug.Log("第 " + day + " 天开始，限时 " + dayLength + " 秒");

        UpdateTimerText();
    }

    private void Update()
    {
        if (running == false) return;
        if (ended) return;

        // ⚠️ 用 deltaTime（不是 unscaledDeltaTime）：
        //    模块面板打开时 timeScale = 0，这里会变成 0，计时就自动暂停了。
        timeLeft = timeLeft - Time.deltaTime;

        UpdateTimerText();

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
            ended = true;
            UpdateTimerText();
            OnDayEnd();
        }
    }

    // 时间到：进入天数过渡场景
    private void OnDayEnd()
    {
        int day = (GameData.Instance != null) ? GameData.Instance.currentDay : 1;
        Debug.Log("第 " + day + " 天时间到，进入过渡界面");

        SceneManager.LoadScene(transitionSceneName);
    }

    // 刷新倒计时文字（timerText 没拖就什么都不做 —— 现在就是这种情况）
    private void UpdateTimerText()
    {
        if (timerText == null) return;

        float t = Mathf.Max(0f, timeLeft);
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    // 给别的脚本读剩余时间用（以后做进度条时会用到）
    public float GetTimeLeft()
    {
        return timeLeft;
    }

    // 给别的脚本读「本天总时长」用（进度条要拿它算比例）
    public float GetDayLength()
    {
        return dayLength;
    }

    // 给别的脚本读「现在是不是在计时」（暂停时可以用来判断）
    public bool IsRunning()
    {
        return running;
    }
}
