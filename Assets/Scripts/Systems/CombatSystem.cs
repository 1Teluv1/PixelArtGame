using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 시스템 관리 및 처리
/// </summary>
public class CombatSystem : MonoBehaviour
{
    [Header("전투 설정")]
    [SerializeField] private float comboTimeWindow = 0.5f; // 콤보 입력 가능 시간
    [SerializeField] private int maxComboCount = 3; // 최대 콤보 수
    
    [Header("히트 이펙트")]
    [SerializeField] private GameObject hitEffectPrefab; // 히트 이펙트 프리팹
    [SerializeField] private GameObject damageTextPrefab; // 데미지 텍스트 프리팹
    
    // 콤보 관련 변수
    private int currentComboCount = 0;
    private float lastAttackTime = 0f;
    
    // 공격 대상 캐싱
    private List<GameObject> alreadyHitEnemies = new List<GameObject>();
    
    // 싱글톤 인스턴스
    private static CombatSystem _instance;
    public static CombatSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CombatSystem>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CombatSystem");
                    _instance = obj.AddComponent<CombatSystem>();
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        // 싱글톤 설정
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 공격 시작 처리
    /// </summary>
    public int StartAttack(GameObject attacker)
    {
        // 콤보 시스템 처리
        float currentTime = Time.time;
        if (currentTime - lastAttackTime > comboTimeWindow)
        {
            // 콤보 타임 윈도우가 지나면 콤보 초기화
            currentComboCount = 0;
        }
        
        // 콤보 증가 (최대치 제한)
        currentComboCount = Mathf.Min(currentComboCount + 1, maxComboCount);
        lastAttackTime = currentTime;
        
        // 공격 대상 캐싱 초기화
        alreadyHitEnemies.Clear();
        
        // 현재 콤보 카운트 반환
        return currentComboCount;
    }
    
    /// <summary>
    /// 공격 히트 처리
    /// </summary>
    public void ProcessHit(GameObject attacker, GameObject target, float damage, float knockbackMagnitude)
    {
        // 이미 히트한 대상이라면 처리 안함 (한 번의 공격에 여러 번 히트 방지)
        if (alreadyHitEnemies.Contains(target))
        {
            return;
        }
        
        // 히트한 대상 추가
        alreadyHitEnemies.Add(target);
        
        // 데미지 처리
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        
        // 데미지 텍스트 생성
        if (damageTextPrefab != null)
        {
            GameObject damageText = Instantiate(damageTextPrefab, target.transform.position + Vector3.up, Quaternion.identity);
            DamageText textComponent = damageText.GetComponent<DamageText>();
            if (textComponent != null)
            {
                textComponent.SetDamage(damage);
            }
        }
        
        // 히트 이펙트 생성
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, target.transform.position, Quaternion.identity);
        }
        
        // 넉백 적용
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null && knockbackMagnitude > 0) // knockbackMagnitude가 0보다 클 때만 적용
        {
            Vector2 direction = (target.transform.position - attacker.transform.position).normalized;
            // 방향 벡터가 NaN이 되는 경우 방지 (공격자와 피격자 위치가 같을 때)
            if (direction == Vector2.zero)
            {
                direction = Vector2.right; // 기본 방향 설정 (혹은 랜덤 방향)
            }
            targetRb.AddForce(direction * knockbackMagnitude, ForceMode2D.Impulse); // knockbackMagnitude 사용
        }
    }
    
    /// <summary>
    /// 특정 위치에서 원형 공격 판정
    /// </summary>
    public void CircleAttack(GameObject attacker, Vector2 center, float radius, float damage, float knockbackForce, LayerMask targetLayers)
    {
        // 공격 시작
        int comboCount = StartAttack(attacker);
        
        // 원형 범위 내 충돌체 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, targetLayers);
        
        // 각 충돌체에 대한 히트 처리
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != attacker)
            {
                // 데미지 계산 (콤보에 따라 증가)
                float finalDamage = damage * (1f + (comboCount - 1) * 0.2f);
                
                // 히트 처리 (knockbackForce를 float 타입 그대로 전달)
                ProcessHit(attacker, hit.gameObject, finalDamage, knockbackForce);
            }
        }
    }
    
    /// <summary>
    /// 특정 방향의 사각형 공격 판정
    /// </summary>
    public void BoxAttack(GameObject attacker, Vector2 center, Vector2 size, float angle, float damage, float knockbackForce, LayerMask targetLayers)
    {
        // 공격 시작
        int comboCount = StartAttack(attacker);
        
        // 사각형 범위 내 충돌체 감지
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle, targetLayers);
        
        // 각 충돌체에 대한 히트 처리
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != attacker)
            {
                // 데미지 계산 (콤보에 따라 증가)
                float finalDamage = damage * (1f + (comboCount - 1) * 0.2f);
                
                // 히트 처리 (knockbackForce를 float 타입 그대로 전달)
                ProcessHit(attacker, hit.gameObject, finalDamage, knockbackForce);
            }
        }
    }
    
    /// <summary>
    /// 레이캐스트 공격 판정
    /// </summary>
    public void RaycastAttack(GameObject attacker, Vector2 origin, Vector2 direction, float distance, float damage, float knockbackForce, LayerMask targetLayers)
    {
        // 공격 시작
        int comboCount = StartAttack(attacker);
        
        // 레이캐스트 수행
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, targetLayers);
        
        // 충돌 발생한 경우 히트 처리
        if (hit.collider != null && hit.collider.gameObject != attacker)
        {
            // 데미지 계산 (콤보에 따라 증가)
            float finalDamage = damage * (1f + (comboCount - 1) * 0.2f);
            
            // 히트 처리 (knockbackForce를 float 타입 그대로 전달)
            ProcessHit(attacker, hit.collider.gameObject, finalDamage, knockbackForce);
        }
    }
    
    /// <summary>
    /// 시각화를 위한 기즈모 설정
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 필요한 경우 디버깅용 기즈모 그리기
    }
}

/// <summary>
/// 데미지를 받을 수 있는 객체를 위한 인터페이스
/// </summary>
public interface IDamageable
{
    void TakeDamage(float damage);
}

/// <summary>
/// 데미지 텍스트 컴포넌트
/// </summary>
public class DamageText : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshPro textMesh;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeSpeed = 1f;
    
    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TMPro.TextMeshPro>();
        }
        
        // 수명 제한
        Destroy(gameObject, lifetime);
    }
    
    /// <summary>
    /// 데미지 텍스트 설정
    /// </summary>
    public void SetDamage(float damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString("0");
        }
    }
    
    private void Update()
    {
        // 위로 올라가는 움직임
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        // 페이드 아웃
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = color;
        }
    }
} 