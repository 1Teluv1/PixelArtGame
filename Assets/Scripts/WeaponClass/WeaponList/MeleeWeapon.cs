using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IWeaponBehaviour
{
    public float damage = 10f;
    public float range = 1.5f;
    public float angle = 60f;
    public float cooldown = 1f;

    private float lastAttackTime = -999f;

    public void Attack()
    {
        if (Time.time < lastAttackTime + cooldown)
            return;
        lastAttackTime = Time.time;

        var animator = GetComponent<WeaponSpriteFrameAnimator>();
        if (animator != null && !animator.IsPlaying())
        {
            animator.PlayOnce();
        }
        // BoxCollider2D를 자동으로 찾음
        var box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            return;
        }
        // 콜라이더의 월드 영역 계산
        Vector2 center = (Vector2)transform.position + box.offset;
        Vector2 size = box.size;
        LayerMask mask = LayerMask.GetMask("Monster", "Boss");
        var hits = Physics2D.OverlapBoxAll(center, size, transform.eulerAngles.z, mask);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                var monster = hit.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damage);
                    Debug.Log("[Attack] 몬스터 공격 진행중");
                }
            }
        }
    }

    public void InitFromData(WeaponData data)
    {
        if (data == null)
        {
            return;
        }
        damage = data.damage;
        cooldown = data.cooldown;
        float scaleValueX = (data.scale_x > 0f) ? data.scale_x : 1f;
        float scaleValueY = (data.scale_y > 0f) ? data.scale_y : 1f;
        transform.localScale = new Vector3(scaleValueX, scaleValueY, 1f);
        var animator = GetComponent<WeaponSpriteFrameAnimator>();
        if (animator != null && !string.IsNullOrEmpty(data.texturePath))
        {
            animator.LoadFramesFromResources(data.texturePath);
        }
    }

    private void Update()
    {
        Attack();
    }

    private SpriteRenderer GetSpriteRenderer()
    {
        return GetComponent<SpriteRenderer>();
    }
}