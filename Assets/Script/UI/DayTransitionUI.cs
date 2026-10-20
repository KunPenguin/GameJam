using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DayTransitionUI : MonoBehaviour
{

    [Header("随天数变化的那张图")]
    public Image dayImage;

    [Header("按顺序排：0=第1天结束(显示二)，1=第2天结束(显示三)，2=四，3=五")]
    public Sprite[] daySprites;

    void Start()
    {
        int day = GameData.Instance.currentDay;

        // 换图
        if (dayImage != null && daySprites != null && daySprites.Length > 0)
        {
            int index = day - 1;

            // 防止越界：第1天以前 → 用第1张；超出数组 → 用最后一张
            if (index < 0) index = 0;
            if (index >= daySprites.Length) index = daySprites.Length - 1;

            dayImage.sprite = daySprites[index];
        }
    }

    public void Continue()
    {
        GameData.Instance.currentDay++;
        SceneManager.LoadScene("GameScene");
    }
}