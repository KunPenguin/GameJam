using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;   
    public TMP_Text hpText;

    private PlayerHealth player;

    void Start()
    {
        // 自动在场景里找挂着 PlayerHealth 的主角
        player = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (player == null) return;

        // 1. 控制红条填充多少2. 把红条设成剩余血量比例，但不许超过满，也不许低于空
        float ratio = player.currentHp / player.maxHp;
        fillImage.fillAmount = Mathf.Clamp01(ratio);

        // 2. 刷新数字（RoundToInt 是为了不显示小数）
        hpText.text = Mathf.RoundToInt(player.currentHp) + " / " + Mathf.RoundToInt(player.maxHp);
    }
}
