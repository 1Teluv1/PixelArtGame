using UnityEngine;

public class OrbitWeapon : MonoBehaviour, IWeaponBehaviour
{
    [Header("Orbit Settings")]
    public GameObject orbitObject; // 회전체 프리팹
    public float radius = 2f;
    public float speed = 180f;
    public int orbitCount = 1;
    public float damage = 10f;
    public float cooldown = 1f;
    private float lastAttackTime = -999f;

    public void Attack()
    {
        if (orbitObject == null) return;
        var box = orbitObject.GetComponent<BoxCollider2D>();
        if (box == null) return;
        var animator = GetComponent<WeaponSpriteFrameAnimator>();
        if (animator != null && !animator.IsPlaying())
        {
            animator.PlayOnce();
        }
        Vector2 center = (Vector2)orbitObject.transform.position + box.offset;
        Vector2 size = box.size;
        LayerMask mask = LayerMask.GetMask("Monster", "Boss");
        var hits = Physics2D.OverlapBoxAll(center, size, orbitObject.transform.eulerAngles.z, mask);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                var monster = hit.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damage);
                }
            }
        }
    }

    public void InitFromData(WeaponData data)
    {
        if (data == null)
            return;
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
        if (orbitObject == null)
        {
            Debug.Log("[OrbitWeapon] orbitObject가 null입니다.");
            return;
        }
        Vector3 center = transform.position; // 플레이어(혹은 OrbitWeapon 오브젝트) 위치
        float angle = speed * Time.time;
        Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
        orbitObject.transform.position = center + offset; // 월드 좌표로 배치
        if (Time.time >= lastAttackTime + cooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }
}