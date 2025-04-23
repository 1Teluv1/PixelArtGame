using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private float currentHealth;
    [Header("애니메이션 설정")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // CombatSystem 참조 추가
    [Header("전투 시스템")]
    [SerializeField] private CombatSystem combatSystem; // Inspector에서 할당하거나 GetComponent로 찾기

    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    // Events
    public delegate void PlayerHealthChanged(float currentHealth, float maxHealth);
    public static event PlayerHealthChanged OnHealthChanged;
    
    public delegate void PlayerDied();
    public static event PlayerDied OnPlayerDied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        InvokeHealthChangedEvent();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        UpdateAnimation();
    }
    

    private void Move()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateAnimation()
    {
        // 이동 애니메이션을 bool 파라미터로 제어
        if (animator != null)
        { 
            // 이동 중인지 확인 (속도가 0보다 큰지 체크, 작은 임계값 사용)
            bool isRunning = rb.linearVelocity.magnitude > 0.1f;
            animator.SetBool("IsRunning", isRunning); // "IsRunning" bool 파라미터 설정

            // 스프라이트 뒤집기 로직은 유지
            if (rb.linearVelocity.x != 0)
            {
                spriteRenderer.flipX = rb.linearVelocity.x < 0;
            }
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Z 키 입력 (기존 Attack)
    public void OnAttack1(InputValue value)
    {
        if (combatSystem != null && value.isPressed)
        { 
            combatSystem.PerformAttack1(); // CombatSystem의 공격 메서드 호출
        }
    }

    // X 키 입력
    public void OnAttack2(InputValue value)
    {
        if (combatSystem != null && value.isPressed)
        {
            combatSystem.PerformAttack2();
        }
    }

    // C 키 입력
    public void OnAttack3(InputValue value)
    {
        if (combatSystem != null && value.isPressed)
        {
            combatSystem.PerformAttack3();
        }
    }

    // V 키 입력
    public void OnAttack4(InputValue value)
    {
        if (combatSystem != null && value.isPressed)
        {
            combatSystem.PerformAttack4();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        InvokeHealthChangedEvent();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        InvokeHealthChangedEvent();
    }

    private void InvokeHealthChangedEvent()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // Invoke death event
        OnPlayerDied?.Invoke();
        
        // Disable player controls
        rb.linearVelocity = Vector2.zero;
        enabled = false;
        
        // Play death animation if available
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
} 