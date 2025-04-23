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
    [SerializeField] private SpriteFrameAnimator spriteFrameAnimator;

    private Transform playerTransform;
    private float attackCooldown = 0f;
    private bool isDead = false;

    // 상태 관리
    private IMonsterState currentState;

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
        transform.position += direction * moveSpeed * Time.deltaTime;
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
        StartCoroutine(FlashEffect());
        if (currentHealth <= 0)
        {
            ChangeState(new MonsterState_Dead());
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