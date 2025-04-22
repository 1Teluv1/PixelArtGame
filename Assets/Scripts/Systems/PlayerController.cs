using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 움직임과 입력을 처리하는 컨트롤러
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("상태")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f;
    
    // 컴포넌트 참조
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    // 상태 변수
    private bool isFacingRight = true;
    private bool isGrounded = false;
    private Vector2 moveInput;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 입력 액션 초기화
    }
    
    private void Update()
    {
        // 이동 입력 처리
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // 애니메이션 상태 업데이트
        UpdateAnimationState();
        
        // 방향 처리
        CheckDirectionToFace();
        
        // 스태미나 재생
        RegenerateStamina();
    }
    
    private void FixedUpdate()
    {
        // 실제 이동 적용
        Move();
    }
    
    /// <summary>
    /// 플레이어 이동 처리
    /// </summary>
    private void Move()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
    
    /// <summary>
    /// 점프 입력 처리
    /// </summary>
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            
            // 점프 애니메이션 실행
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }
    
    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (stamina >= 10f) // 공격에 필요한 스태미나 체크
        {
            stamina -= 10f; // 스태미나 소모
            
            // 공격 애니메이션 실행
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // 여기에 공격 로직 추가
            // TODO: 히트박스 활성화
        }
    }
    
    /// <summary>
    /// 애니메이션 상태 업데이트
    /// </summary>
    private void UpdateAnimationState()
    {
        if (animator != null)
        {
            animator.SetFloat("HorizontalSpeed", Mathf.Abs(moveInput.x));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
    
    /// <summary>
    /// 이동 방향에 따른 스프라이트 방향 전환
    /// </summary>
    private void CheckDirectionToFace()
    {
        if (moveInput.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && isFacingRight)
        {
            Flip();
        }
    }
    
    /// <summary>
    /// 캐릭터 방향 전환
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
    
    /// <summary>
    /// 스태미나 재생
    /// </summary>
    private void RegenerateStamina()
    {
        if (stamina < maxStamina)
        {
            stamina += staminaRegenRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }
    }
    
    /// <summary>
    /// 지면 충돌 감지
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    /// <summary>
    /// 플레이어가 데미지를 받음
    /// </summary>
    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        
        // 피격 애니메이션 실행
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // 체력이 0이면 사망 처리
        if (health <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void Die()
    {
        // 사망 애니메이션 실행
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 움직임 비활성화
        enabled = false;
        rb.linearVelocity = Vector2.zero;
        
        // TODO: 게임오버 처리
    }
    
    // 공개 속성 (UI 접근용)
    public float Health => health;
    public float MaxHealth => maxHealth;
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
} 