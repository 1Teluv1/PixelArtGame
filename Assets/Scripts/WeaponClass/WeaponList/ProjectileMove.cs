using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    public Vector2 direction;
    public float speed = 10f;
    public float damage = 10f;

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // BoxCollider2D 없이 Rigidbody2D가 없으므로 직접 충돌 체크
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size, 0f);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Monster"))
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