using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterState;

public class Monster : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField] public float maxHealth = 50f;
    [SerializeField] public float currentHealth;
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] public float damage = 10f;
    [SerializeField] public float attackRate = 1f;
    [SerializeField] public float experienceValue = 10f;

    [Header("애니메이션")]
    [SerializeField] private MonsterSpriteFrameAnimator spriteFrameAnimator;

    [Header("스프라이트 및 이펙트")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material hitMaterial;

    private Transform playerTransform;
    private float attackCooldown = 0f;
    private bool isDead = false;

    // 상태 관리
    private IMonsterState currentState;

    private float lastHitEffectTime = -999f;
    private float hitEffectCooldown = 0.2f; // 피격 이펙트 최소 간격(초)

    private MonsterHitEffectController hitEffectController;

    // MonsterData로부터 능력치 자동 할당
    public void InitFromData(MonsterData data)
    {
        maxHealth = data.maxHealth;
        currentHealth = data.maxHealth;
        moveSpeed = data.moveSpeed;
        damage = data.damage;
        attackRate = data.attackRate;
        experienceValue = data.experienceValue;
        if (spriteFrameAnimator != null)
        {
            spriteFrameAnimator.LoadFramesFromResources(data.texturePath);
        }
        // scale 값이 0 이하일 경우 기본값 1 적용 (가드 조건)
        float scaleValue = (data.scale > 0f) ? data.scale : 1f;
        transform.localScale = new Vector3(scaleValue, scaleValue, 1f);
    }

    private void Awake()
    {
        hitEffectController = GetComponent<MonsterHitEffectController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        Debug.Log($"[Monster] moveSpeed: {moveSpeed}");
        ChangeState(new MonsterState_Chase());
    }

    private void Update()
    {
        currentState?.Update(this);
    }

    public void ChangeState(IMonsterState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    public bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        return dist < 8f;
    }

    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null) return false;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        return dist < 1.5f;
    }

    public void MoveTowardsPlayer()
    {
        if (isDead || playerTransform == null) return;
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.MovePosition(rb.position + (Vector2)direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    public void AttackPlayer()
    {
        if (isDead || playerTransform == null) return;
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            return;
        }
        // 플레이어와 충돌 체크
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            attackCooldown = 1f / attackRate;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log($"[Monster] {gameObject.name} 피격! 받은 데미지: {damage}, 남은 체력: {currentHealth}");

        // 피격 이펙트 분리
        hitEffectController?.PlayHitEffect();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        DropExperience();
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1f);
    }

    private void DropExperience()
    {
        Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player != null)
        {
            player.CollectExperience(experienceValue);
        }
    }
} 