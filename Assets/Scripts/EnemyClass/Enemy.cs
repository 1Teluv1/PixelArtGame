using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float experienceValue = 10f;
    
    private Transform playerTransform;
    private float attackCooldown = 0f;
    private bool isDead = false;
    
    private void Start()
    {
        currentHealth = maxHealth;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        if (isDead) return;
        
        if (playerTransform != null)
        {
            // 플레이어에게 이동
            MoveTowardsPlayer();
            
            // 공격 쿨다운 업데이트
            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }
        }
    }
    
    private void MoveTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        
        // 플레이어와 충돌하는지 확인
        if (collision.gameObject.CompareTag("Player") && attackCooldown <= 0)
        {
            // 플레이어 공격
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                attackCooldown = 1f / attackRate;
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // 깜빡이거나 피격 애니메이션 재생
        StartCoroutine(FlashEffect());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        // 경험치 드롭
        DropExperience();
        
        // 죽음 애니메이션 재생
        // ...
        
        // 컴포넌트 비활성화
        GetComponent<Collider2D>().enabled = false;
        
        // 지연 후 파괴
        Destroy(gameObject, 1f);
    }
    
    private void DropExperience()
    {
        // 경험치를 줄 플레이어 찾기
        Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player != null)
        {
            player.CollectExperience(experienceValue);
        }
        
        // 대안: 수집 가능한 픽업 오브젝트 생성
        // GameObject expPickup = Instantiate(experiencePickupPrefab, transform.position, Quaternion.identity);
        // expPickup.GetComponent<ExperiencePickup>().SetValue(experienceValue);
    }
    
    // 선택 사항: 체력 또는 무기 픽업 드롭 확률 추가
    private void DropPickups()
    {
        // Die()에서 호출될 수 있는 예제 구현
        float randomValue = Random.value;
        
        if (randomValue < 0.1f)
        {
            // 10% 확률로 체력 드롭
            // 체력 픽업 생성
        }
        else if (randomValue < 0.15f)
        {
            // 5% 확률로 무기 드롭
            // 무기 픽업 생성
        }
    }
} 