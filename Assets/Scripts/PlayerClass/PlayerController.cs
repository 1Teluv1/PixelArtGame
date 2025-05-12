using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    
    [Header("플레이어 카메라")]
    [SerializeField] private Camera playerCamera; // 플레이어를 따라다니는 카메라
    [SerializeField] private CameraEffectsManager cameraEffects; // 카메라 효과 관리 시스템
    public Camera GetPlayerCamera() => playerCamera;
    public CameraEffectsManager GetCameraEffects() => cameraEffects;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private float currentHealth;
    [Header("애니메이션 설정")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    // Events
    public delegate void PlayerHealthChanged(float currentHealth, float maxHealth);
    public static event PlayerHealthChanged OnHealthChanged;
    
    public delegate void PlayerDied();
    public static event PlayerDied OnPlayerDied;

    private bool isDead = false;
    [SerializeField] private Player player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        Debug.Log($"[PlayerController] Awake: rb={(rb != null)}, player={(player != null)}");
        
        // 카메라 효과 컴포넌트 찾기
        if (playerCamera != null && cameraEffects == null)
        {
            cameraEffects = playerCamera.GetComponent<CameraEffectsManager>();
            if (cameraEffects == null && playerCamera.gameObject != null)
            {
                cameraEffects = playerCamera.gameObject.AddComponent<CameraEffectsManager>();
                Debug.Log("[PlayerController] 카메라에 CameraEffectsManager 자동 추가");
            }
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        InvokeHealthChangedEvent();
        Debug.Log($"[PlayerController] Start: currentHealth={currentHealth}, maxHealth={maxHealth}");
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void LateUpdate()
    {
        FollowCamera();
    }

    private void Move()
    {
        rb.linearVelocity = moveInput * moveSpeed;
        // 이동 입력 로그
        if (moveInput != Vector2.zero)
            Debug.Log($"[PlayerController] Move: moveInput={moveInput}, speed={moveSpeed}");
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        { 
            bool isRunning = rb.linearVelocity.magnitude > 0.1f;
            animator.SetBool("IsRunning", isRunning);
            Debug.Log($"[PlayerController] UpdateAnimation: isRunning={isRunning}");
            // 좌우 이동에 따라 스프라이트 Flip
            if (rb.linearVelocity.x != 0 && spriteRenderer != null)
            {
                bool flip = rb.linearVelocity.x < 0;
                spriteRenderer.flipX = flip;
                Debug.Log($"[PlayerController] SpriteRenderer.flipX={flip}");

                // WeaponSystem의 Mark도 반전
                if (player != null && player.GetWeaponSystem() != null)
                {
                    player.GetWeaponSystem().SetWeaponMarkFlip(flip);
                }
            }
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log($"[PlayerController] OnMove: moveInput={moveInput}");
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"[PlayerController] TakeDamage: damage={damage}, currentHealth={currentHealth}");
        
        // 데미지 받을 때 카메라 효과 적용
        if (cameraEffects != null && damage > 0)
        {
            float intensity = Mathf.Clamp01(damage / maxHealth) * 2.0f;
            cameraEffects.ShakeCamera(intensity, 0.5f);
            Debug.Log($"[CameraEffectsManager] ShakeCamera: intensity={intensity}, duration=0.5f");
        }
        
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
        Debug.Log($"[PlayerController] Heal: amount={amount}, currentHealth={currentHealth}");
        InvokeHealthChangedEvent();
    }

    private void InvokeHealthChangedEvent()
    {
        Debug.Log($"[PlayerController] InvokeHealthChangedEvent: currentHealth={currentHealth}, maxHealth={maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[PlayerController] Die: 플레이어 사망");
        
        // 사망 시 카메라 효과
        if (cameraEffects != null)
        {
            cameraEffects.ShakeCamera(0.2f, 0.5f);
            cameraEffects.SlowMotion(0.2f, 1.0f);
            cameraEffects.ApplyBlur(15f, 0.8f);
        }
        
        // Invoke death event
        OnPlayerDied?.Invoke();
        // Disable player controls
        rb.linearVelocity = Vector2.zero;
        enabled = false;
        // Play death animation if available
        if (animator != null)
        {
            animator.SetTrigger("Die"); // 트리거 방식으로 변경
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

    private void FollowCamera()
    {
        if (playerCamera != null)
        {
            // 카메라 효과 시스템에 의해 이미 처리되고 있지 않은 경우에만 위치 조정
            if (cameraEffects == null || !cameraEffects.IsShaking())
            {
                Vector3 playerPos = transform.position;
                playerPos.z = playerCamera.transform.position.z; // 카메라의 z값 유지(2D)
                playerCamera.transform.position = playerPos;
            }
        }
    }
} 