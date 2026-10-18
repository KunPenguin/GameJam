// 文字版新手教程。挂在 GameScene 里新建的空物体 "Tutorial" 上。
// 设计原则：不暂停游戏、不改任何现有脚本、条件达标就前进、超时也前进（不会卡住人）。
using UnityEngine;
using TMPro;

public class TutorialText : MonoBehaviour
{
    [Header("屏幕下方的提示文字（拖一个 TMP 进去）")]
    public TextMeshProUGUI tipText;

    [Header("玩家（留空会自动按 Player 标签找）")]
    public Transform player;

    [Header("每条提示至少显示几秒（即使已经做到，也让玩家看完）")]
    public float minTimePerStep = 2f;

    [Header("调试：勾上就忽略存档，每次进游戏都播")]
    public bool alwaysPlay = false;

    private string[] tips =
    {
        "WASD 移动",
        "按住左键，朝鼠标方向发射水弹",
        "水会让你变慢 —— 蓝色区域就是水",
        "走近坏掉的装置，站在范围里就会自动修理",
        "修好了！选一张模块卡",
        "把模块拖进模块链。顺序决定效果，放好点关闭",
        "撑过 5 天。第 5 天，它会来。",
    };

    // 每步最多等几秒（超出就强制前进，避免卡住玩家）
    private float[] timeouts = { 8f, 10f, 12f, 25f, 30f, 35f, 5f };

    private int step = 0;
    private float stepTimer = 0f;
    private Vector3 startPos;
    private bool fired = false;
    private bool inWater = false;
    private bool finished = false;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player != null) startPos = player.position;

        // 看过就不再播（alwaysPlay 勾上则强制播）
        if (alwaysPlay == false && PlayerPrefs.GetInt("TutorialDone", 0) == 1)
        {
            EndTutorial(false);
            return;
        }

        ShowStep();
    }

    private void Update()
    {
        if (finished) return;

        // 玩家死了就停（不拦着结束面板）
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null && ph.dead) { EndTutorial(false); return; }

            WaterSlowdown ws = player.GetComponent<WaterSlowdown>();
            if (ws != null && ws.InWater) inWater = true;
        }

        if (Input.GetMouseButton(0)) fired = true;

        // 按 Esc 跳过
        if (Input.GetKeyDown(KeyCode.Escape)) { EndTutorial(true); return; }

        stepTimer += Time.deltaTime;

        // 至少显示 minTimePerStep，再检查条件
        if (stepTimer >= minTimePerStep && IsStepDone()) { NextStep(); return; }

        // 超时也前进
        if (stepTimer >= timeouts[step]) NextStep();
    }

    // 这一步玩家做到了吗
    private bool IsStepDone()
    {
        switch (step)
        {
            case 0:   // 移动
                return player != null && Vector2.Distance(player.position, startPos) > 2f;

            case 1:   // 射击
                return fired;

            case 2:   // 进水
                return inWater;

            case 3:   // 修好任意一台装置
                return AnyDeviceRepaired();

            case 4:   // 奖励区有模块了（= 玩家选了一张卡）
                return string.IsNullOrEmpty(ModulePanelData.Read(ModuleArea.Reward, 0)) == false;

            case 5:   // 面板关掉了
                return ModulePanelUI.Instance == null || ModulePanelUI.Instance.panel.activeSelf == false;

            default:  // 最后一条纯文字，靠超时结束
                return false;
        }
    }

    private bool AnyDeviceRepaired()
    {
        DeviceRepair[] all = FindObjectsOfType<DeviceRepair>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].IsRepaired) return true;
        }
        return false;
    }

    private void ShowStep()
    {
        stepTimer = 0f;
        if (tipText != null) tipText.text = tips[step];
        Debug.Log("[教程] 第 " + (step + 1) + " 步：" + tips[step]);
    }

    private void NextStep()
    {
        step++;
        if (step >= tips.Length) { EndTutorial(false); return; }
        ShowStep();
    }

    private void EndTutorial(bool bySkip)
    {
        finished = true;
        if (tipText != null) tipText.text = "";
        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.Save();
        Debug.Log("[教程] 结束" + (bySkip ? "（玩家跳过）" : ""));
    }
}