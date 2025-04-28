using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    public Vector2 direction;
    public float speed = 10f;
    public float damage = 10f;

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        var box = GetComponent<BoxCollider2D>();
        if (box == null) return; // Collider 없으면 충돌 체크 생략

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, box.size, 0f);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue; // 자기 자신 무시
            if (hit.CompareTag("Monster"))
            {
                var monster = hit.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damage);
                }
                Destroy(gameObject);
                break;
            }
        }
    }
} 