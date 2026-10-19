// 文字版新手教程 v2。
// 关键改动：
//   1. "修好装置"改用 DeviceRepair.Repaired 事件判断（准确，不扫描场景）
//   2. 模块界面改成 3 步"操作指引"（选卡 → 拖进模块链 → 关闭）
//   3. 计时用 unscaledDeltaTime —— 因为模块面板会 Time.timeScale = 0
using UnityEngine;
using TMPro;

public class TutorialText : MonoBehaviour
{
    [Header("屏幕顶部的提示文字（拖一个 TMP 进去）")]
    public TextMeshProUGUI tipText;

    [Header("玩家（留空会自动按 Player 标签找）")]
    public Transform player;

    [Header("每条提示至少显示几秒（即使已经做到）")]
    public float minTimePerStep = 2f;

    [Header("调试：勾上就无视天数，任何天都播")]
    public bool alwaysPlay = false;

    private string[] tips =
    {
        "WASD 移动",
        "按住左键，朝鼠标方向发射水弹",
        "灰白色的浅水会让你变慢，蓝紫色的深水不能通过，水地块会周期性涌出敌人",
        "跟随屏幕上的指引走近坏掉的水泵，站在范围里就会自动修理，坏掉的水泵会不断产生越来越多的浅水",
        "【模块界面】点上面三张卡里的任意一张",
        "选好后奖励区会出现一个模块，通过点击把它移入【模块链】，只有没有模块时才能退出此界面",
        "模块链上的模块会从上到下依次生效。不需要的模块可以移入左侧加号处的背包区，或者移入左下角废弃区，调整好后点左上角退出此界面",
        "右上角进度条归零会进入下一天，请撑过 5 天",
    };

    // 每步最多等几秒；<= 0 表示不超时（模块界面那几步靠玩家操作推进）
    private float[] timeouts = { 8f, 10f, 12f, 30f, 0f, 0f, 0f, 6f };

    private int step = 0;
    private float stepTimer = 0f;
    private Vector3 startPos;
    private bool fired = false;
    private bool inWater = false;
    private bool repairedInTutorial = false;
    private bool finished = false;

    // ===== 订阅"装置被修好"事件（比扫描场景准确）=====
    private void OnEnable() { DeviceRepair.Repaired += OnDeviceRepaired; }
    private void OnDisable() { DeviceRepair.Repaired -= OnDeviceRepaired; }
    private void OnDeviceRepaired(DeviceRepair d) { repairedInTutorial = true; }

    private void Start()
    {
        if (player == null) player = PlayerMove.T;    // 玩家在 Awake 里已登记，不用遍历场景
        if (player != null) startPos = player.position;

        // ===== 什么时候播 =====
        // 只在第 1 天播（第 2 天起重载场景不该再教一遍）。
        // 不做"看过就记住"—— 教程不产生额外决策负担，每局都看一遍反而更保险。
        // alwaysPlay 勾上 = 无视天数，任何天都播（调试用）。
        if (alwaysPlay == false)
        {
            int day = (GameData.Instance != null) ? GameData.Instance.currentDay : 1;
            if (day > 1)
            {
                SkipSilently();
                return;
            }
        }

        ShowStep();
    }

    private void Update()
    {
        if (finished) return;

        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null && ph.dead) { EndTutorial(false); return; }

            WaterSlowdown ws = player.GetComponent<WaterSlowdown>();
            if (ws != null && ws.InWater) inWater = true;
        }

        if (Input.GetMouseButton(0)) fired = true;

        if (Input.GetKeyDown(KeyCode.Escape)) { EndTutorial(true); return; }

        //  用 unscaledDeltaTime：模块面板打开时 timeScale = 0，
        //    如果用 deltaTime，计时会停住，教程就卡在那里不动了
        stepTimer += Time.unscaledDeltaTime;

        if (stepTimer >= minTimePerStep && IsStepDone()) { NextStep(); return; }

        if (timeouts[step] > 0f && stepTimer >= timeouts[step]) NextStep();
    }

    private bool IsStepDone()
    {
        switch (step)
        {
            case 0: return player != null && Vector2.Distance(player.position, startPos) > 2f;
            case 1: return fired;
            case 2: return inWater;
            case 3: return repairedInTutorial;                     // ← 用事件，准确
            case 4: return string.IsNullOrEmpty(ModulePanelData.Read(ModuleArea.Reward, 0)) == false;
            case 5: return ModulePutIntoChain();
            case 6: return PanelClosed();
            default: return false;
        }
    }

    // 模块链里出现了"开局自带的 Test_FireModule 之外"的模块 = 玩家装上了新模块
    private bool ModulePutIntoChain()
    {
        if (GameData.Instance == null) return false;

        for (int i = 0; i < GameData.Instance.equipped.Length; i++)
        {
            string n = GameData.Instance.equipped[i];
            if (string.IsNullOrEmpty(n)) continue;
            if (n == "Test_FireModule") continue;                  // 开局自带的，跳过
            return true;
        }
        return false;
    }

    private bool PanelClosed()
    {
        ModulePanelUI ui = ModulePanelUI.Instance;
        if (ui == null) return true;                               // 场景里没有面板，算它关了
        if (ui.panel == null) return true;
        return ui.panel.activeSelf == false;
    }

    private void ShowStep()
    {
        stepTimer = 0f;
        if (tipText != null) tipText.text = tips[step];
        Debug.Log("[教程] 第 " + (step + 1) + "/" + tips.Length + " 步：" + tips[step]);
    }

    private void NextStep()
    {
        step++;
        if (step >= tips.Length) { EndTutorial(false); return; }
        ShowStep();
    }

    // 教程正常结束（走完 或 玩家按 Esc 跳过）
    // 注意：这里不写任何"已看过"的记录 —— 每次开新局都该再看一遍
    private void EndTutorial(bool bySkip)
    {
        finished = true;
        if (tipText != null) tipText.text = "";
        Debug.Log("[教程] 结束" + (bySkip ? "（玩家跳过）" : ""));
    }

    // 不该播的时候安静退出：不显示、也不写存档
    // （和第 1 天无关的情况不该被记成"看过了"，否则以后第 1 天也不会播）
    private void SkipSilently()
    {
        finished = true;
        if (tipText != null) tipText.text = "";
    }
}