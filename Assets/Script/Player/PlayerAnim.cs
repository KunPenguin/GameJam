using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        anim.SetBool("IsMoving", h != 0 || v != 0);
        anim.SetBool("IsAttacking", Input.GetMouseButton(0));

        // 修正：按右(D)取消翻转(面朝左)，按左(A)进行翻转(面朝右)
        if (h > 0)
        {
            spriteRenderer.flipX = true;  // 往右走，翻转图片（变成朝右）
        }
        else if (h < 0)
        {
            spriteRenderer.flipX = false; // 往左走，保持原图（本身就是朝左）
        }
    }
}