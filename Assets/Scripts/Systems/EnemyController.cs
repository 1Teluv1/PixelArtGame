using UnityEngine;
using System.Collections;

/// <summary>
/// 적 캐릭터의 기본 컨트롤러
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float health = 50f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackCooldown = 1f;
    
    [Header("패트롤 설정")]
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected float patrolWaitTime = 1f;
    
    // 컴포넌트 참조
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    
    // 상태 변수
    protected Transform playerTransform;
    protected bool isFacingRight = true;
    protected bool isAttacking = false;
    protected bool isDead = false;
    protected bool canAttack = true;
    protected int currentPatrolIndex = 0;
    
    // 상태 머신
    protected enum EnemyState { Patrol, Chase, Attack, Hurt, Die }
    protected EnemyState currentState;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    protected virtual void Start()
    {
        // 플레이어 찾기
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 초기 상태 설정
        currentState = EnemyState.Patrol;
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        // 플레이어가 존재하는지 확인
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }
        
        // 상태에 따른 행동 처리
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                // 플레이어가 감지 범위 내에 있는지 체크
                if (IsPlayerInRange(detectionRange))
                {
                    currentState = EnemyState.Chase;
                }
                break;
                
            case EnemyState.Chase:
                ChasePlayer();
                // 플레이어가 공격 범위 내에 있는지 체크
                if (IsPlayerInRange(attackRange))
                {
                    currentState = EnemyState.Attack;
                }
                // 플레이어가 감지 범위를 벗어났는지 체크
                else if (!IsPlayerInRange(detectionRange))
                {
                    currentState = EnemyState.Patrol;
                }
                break;
                
            case EnemyState.Attack:
                if (canAttack)
                {
                    Attack();
                }
                
                // 플레이어가 공격 범위를 벗어났는지 체크
                if (!IsPlayerInRange(attackRange))
                {
                    currentState = EnemyState.Chase;
                }
                break;
        }
        
        // 이동 방향에 따른 스프라이트 방향 전환
        UpdateDirection();
        
        // 애니메이션 상태 업데이트
        UpdateAnimationState();
    }
    
    /// <summary>
    /// 패트롤 기능
    /// </summary>
    protected virtual void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        
        // 현재 패트롤 포인트로 이동
        Transform target = patrolPoints[currentPatrolIndex];
        if (target != null)
        {
            // 목표 지점으로 이동
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            
            // 도착 체크
            float distance = Vector2.Distance(transform.position, target.position);
            if (distance < 0.1f)
            {
                // 다음 패트롤 포인트로 변경
                StartCoroutine(WaitAtPatrolPoint());
            }
        }
    }
    
    /// <summary>
    /// 패트롤 포인트에서 대기
    /// </summary>
    protected virtual IEnumerator WaitAtPatrolPoint()
    {
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(patrolWaitTime);
        
        // 다음 패트롤 포인트로 인덱스 변경
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
    
    /// <summary>
    /// 플레이어 추적
    /// </summary>
    protected virtual void ChasePlayer()
    {
        if (playerTransform == null) return;
        
        // 플레이어 방향으로 이동
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }
    
    /// <summary>
    /// 공격 기능
    /// </summary>
    protected virtual void Attack()
    {
        // 공격 중 상태로 변경
        isAttacking = true;
        canAttack = false;
        
        // 이동 멈춤
        rb.linearVelocity = Vector2.zero;
        
        // 공격 애니메이션 실행
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 공격 쿨다운 시작
        StartCoroutine(AttackCooldown());
    }
    
    /// <summary>
    /// 공격 쿨다운
    /// </summary>
    protected virtual IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
    }
    
    /// <summary>
    /// 플레이어가 범위 내에 있는지 체크
    /// </summary>
    protected virtual bool IsPlayerInRange(float range)
    {
        if (playerTransform == null) return false;
        
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= range;
    }
    
    /// <summary>
    /// 이동 방향에 따른 스프라이트 방향 전환
    /// </summary>
    protected virtual void UpdateDirection()
    {
        if (rb.linearVelocity.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < -0.1f && isFacingRight)
        {
            Flip();
        }
    }
    
    /// <summary>
    /// 스프라이트 방향 전환
    /// </summary>
    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    /// <summary>
    /// 애니메이션 상태 업데이트
    /// </summary>
    protected virtual void UpdateAnimationState()
    {
        if (animator == null) return;
        
        // 이동 속도 파라미터 설정
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", speed);
        
        // 기타 상태 파라미터 설정
        animator.SetBool("IsAttacking", isAttacking);
    }
    
    /// <summary>
    /// 데미지를 받음
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        
        health -= damage;
        
        // 피격 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // 체력이 0 이하면 사망 처리
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // 피격 상태로 변경
            currentState = EnemyState.Hurt;
            StartCoroutine(ReturnToChaseState());
        }
    }
    
    /// <summary>
    /// 피격 이후 추적 상태로 복귀
    /// </summary>
    protected virtual IEnumerator ReturnToChaseState()
    {
        // 잠시 기절 효과
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        
        // 플레이어가 범위 내에 있으면 추적 상태로, 없으면 패트롤 상태로
        currentState = IsPlayerInRange(detectionRange) ? EnemyState.Chase : EnemyState.Patrol;
    }
    
    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void Die()
    {
        isDead = true;
        currentState = EnemyState.Die;
        
        // 물리 효과 비활성화
        rb.linearVelocity = Vector2.zero;
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }
        
        // 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 일정 시간 후 오브젝트 제거
        Destroy(gameObject, 2f);
    }
    
    /// <summary>
    /// 실제 플레이어에게 데미지를 가하는 메서드 (애니메이션 이벤트에서 호출)
    /// </summary>
    public virtual void DealDamageToPlayer()
    {
        if (isDead || playerTransform == null) return;
        
        // 플레이어가 공격 범위 내에 있는지 확인
        if (IsPlayerInRange(attackRange))
        {
            // 플레이어에게 데미지 전달
            PlayerController player = playerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
            }
        }
    }
    
    /// <summary>
    /// 기즈모를 통한 시각화
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        // 감지 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 공격 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
} 