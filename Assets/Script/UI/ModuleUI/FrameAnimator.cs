using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Image))]
public class FrameAnimator : MonoBehaviour
{
    [Header("所有帧，按播放顺序摆放")]
    public Sprite[] frames;

    [Header("每帧停留时间")]
    public float frameInterval = 0.067f;
    [Header("每轮间隔")]
    public float loopDelay = 0f;

    private Image img;
    private float timer=0f;
    private int current=0;
    private float delayTimer = 0f;

    private void Awake()
    {
        img = GetComponent<Image>();

        if(img==null)
        {
            Debug.LogError("FrameAnimator: 这个物体上没有Image组件");
            return;
        }

        //开始先显示第一帧
        if (frames!=null&&frames.Length > 0)
        {
            img.sprite = frames[0];
        }
        else
        {
            Debug.LogWarning("FrameAnimator：" + gameObject.name + " 的 frames 数组是空的，动图不会播放");
        }
    }
    void Update()
    {
        if (frames == null || frames.Length <= 1 || img == null) return;
        timer = timer + Time.unscaledDeltaTime;

        if (timer < frameInterval) return;

        timer = timer - frameInterval;
        
        if (loopDelay > 0f && current == frames.Length - 1)
        {
            delayTimer = delayTimer + frameInterval;
            if (delayTimer < loopDelay)
            {
                return;      // 还没停够，这一帧多留一会
            }
            delayTimer = 0f;
        }

        // 切换到下一帧
        current = (current + 1) % frames.Length;
        img.sprite = frames[current];

    }
}
