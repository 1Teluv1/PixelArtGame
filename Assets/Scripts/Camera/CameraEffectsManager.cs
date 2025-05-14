using System.Collections;
using UnityEngine;

public class CameraEffectsManager : MonoBehaviour
{
    [Header("카메라 참조")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform playerTransform; // 플레이어 Transform 참조

    [Header("카메라 흔들림 설정")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDecay = 0.95f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField, Range(5, 30)] private int shakeVibrato = 10; // 초당 진동 횟수
    [SerializeField, Range(0f, 90f)] private float shakeRandomness = 70f; // 랜덤성 (0: 원형, 90: 완전 랜덤)
    [SerializeField] private bool shakeUseRandomSeed = true; // 매번 다른 패턴으로 흔들림

    [Header("카메라 슬로우 설정")]
    [SerializeField] private float slowdownFactor = 0.3f;
    [SerializeField] private float slowdownDuration = 1.0f;
    [SerializeField] private float slowdownRecoverySpeed = 2.0f;

    [Header("카메라 블러 설정")]
    [SerializeField] private float maxBlurAmount = 10f;
    [SerializeField] private Material blurMaterial; // 대체 블러 머티리얼

    [Header("카메라 모자이크 설정")]
    [SerializeField] private float maxMosaicAmount = 64f;
    [SerializeField] private Material mosaicMaterial;

    // 프로퍼티 추가
    public Material MosaicMaterial 
    { 
        get { return mosaicMaterial; } 
        set { mosaicMaterial = value; } 
    }
    
    public Material BlurMaterial
    {
        get { return blurMaterial; }
        set { blurMaterial = value; }
    }
    
    public Transform PlayerTransform
    {
        get { return playerTransform; }
        set { playerTransform = value; }
    }

    // 내부 변수
    private Vector3 originalOffset; // 카메라와 플레이어 사이의 기본 오프셋
    private float currentShakeIntensity = 0f;
    private float currentMosaicAmount = 0f;
    private float currentBlurAmount = 0f;
    private bool isMosaicActive = false;
    private bool isBlurActive = false;
    private bool isShaking = false;
    
    // 흔들림 관련 변수
    private Vector3[] shakeNoiseOffset; // 노이즈 오프셋 배열
    private int shakeSeed;
    private Coroutine shakeCoroutine;
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();
        
        InitShakeNoiseOffsets();
    }
    
    private void Start()
    {
        // 자동으로 플레이어 찾기
        if (playerTransform == null)
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
                Debug.Log("[CameraEffectsManager] 플레이어 자동 할당 완료");
            }
            else
            {
                Debug.LogWarning("[CameraEffectsManager] 플레이어를 찾을 수 없습니다. 수동으로 할당하세요.");
            }
        }
        
        // 카메라와 플레이어 사이의 초기 오프셋 계산
        CalculateOriginalOffset();
    }
    
    private void CalculateOriginalOffset()
    {
        if (playerTransform != null)
        {
            originalOffset = transform.position - playerTransform.position;
            originalOffset.z = transform.position.z; // z 값 유지 (2D 게임용)
            Debug.Log($"[CameraEffectsManager] 카메라-플레이어 기본 오프셋: {originalOffset}");
        }
    }
    
    private void LateUpdate()
    {
        if (playerTransform != null && !isShaking)
        {
            // 카메라 효과가 없을 때는 플레이어를 따라다님
            UpdateCameraPositionToPlayer();
        }
    }
    
    private void UpdateCameraPositionToPlayer()
    {
        Vector3 targetPosition = playerTransform.position + originalOffset;
        transform.position = targetPosition;
    }
    
    private void InitShakeNoiseOffsets()
    {
        // 여러 방향으로의 랜덤 움직임을 표현하기 위한 노이즈 옵셋 생성
        shakeNoiseOffset = new Vector3[3];
        RefreshShakeNoiseOffsets();
    }
    
    private void RefreshShakeNoiseOffsets()
    {
        // 매번 다른 흔들림 패턴 생성
        shakeSeed = shakeUseRandomSeed ? Random.Range(0, 1000) : 0;
        
        for (int i = 0; i < shakeNoiseOffset.Length; i++)
        {
            // 각 축마다 다른 속도로 움직이는 노이즈 오프셋
            shakeNoiseOffset[i] = new Vector3(
                Random.Range(0f, 100f) + shakeSeed,
                Random.Range(0f, 100f) + shakeSeed,
                Random.Range(0f, 100f) + shakeSeed
            );
        }
    }

    #region 카메라 흔들림
    public void ShakeCamera(float intensity = -1, float duration = -1)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        
        currentShakeIntensity = intensity > 0 ? intensity : shakeIntensity;
        float shakeDur = duration > 0 ? duration : shakeDuration;
        
        // 새로운 흔들림 패턴 생성
        if (shakeUseRandomSeed)
            RefreshShakeNoiseOffsets();
            
        isShaking = true;
        shakeCoroutine = StartCoroutine(ImprovedShakeCameraCoroutine(shakeDur));
    }

    private IEnumerator ImprovedShakeCameraCoroutine(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && currentShakeIntensity > 0.01f)
        {
            // 경과 시간에 따른 진동 계산
            float strength = currentShakeIntensity;
            float vibration = elapsed * shakeVibrato;
            
            // 여러 노이즈 값을 조합하여 자연스러운 흔들림 생성
            Vector3 shakeOffset = CalculateShakeOffset(strength, vibration);
            
            // 플레이어 위치를 기준으로 카메라 위치 업데이트
            if (playerTransform != null)
            {
                Vector3 targetPosition = playerTransform.position + originalOffset + shakeOffset;
                transform.position = targetPosition;
            }
            else
            {
                // 플레이어가 없는 경우 기존 방식 사용
                transform.position = transform.position + shakeOffset;
            }
            
            // 감쇠 적용
            currentShakeIntensity *= shakeDecay;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 효과 종료 후 카메라를 플레이어 위치로 복원
        if (playerTransform != null)
        {
            UpdateCameraPositionToPlayer();
        }
        
        currentShakeIntensity = 0f;
        isShaking = false;
        shakeCoroutine = null;
    }
    
    private Vector3 CalculateShakeOffset(float strength, float vibration)
    {
        // 기본 진동값
        float sin = Mathf.Sin(vibration);
        float cos = Mathf.Cos(vibration);
        
        // 원형 흔들림 기준
        Vector3 circularShake = new Vector3(cos, sin, 0) * strength;
        
        // 랜덤 흔들림 생성
        Vector3 randomShake = new Vector3(
            PerlinNoise(shakeNoiseOffset[0].x, vibration),
            PerlinNoise(shakeNoiseOffset[0].y, vibration),
            0
        ) * strength;
        
        // 흔들림 방향의 랜덤성 (0: 원형, 1: 완전 랜덤)
        float randomFactor = shakeRandomness / 90f;
        
        // 원형과 랜덤 흔들림을 혼합
        return Vector3.Lerp(circularShake, randomShake, randomFactor);
    }
    
    // 퍼린 노이즈를 -1~1 범위로 변환
    private float PerlinNoise(float x, float y)
    {
        return (Mathf.PerlinNoise(x + y * 0.1f, y) * 2f) - 1f;
    }
    
    public bool IsShaking()
    {
        return isShaking;
    }
    #endregion

    #region 카메라 슬로우
    public void SlowMotion(float factor = -1, float duration = -1)
    {
        StopCoroutine(nameof(SlowMotionCoroutine));
        float slowFactor = factor > 0 ? factor : slowdownFactor;
        float slowDuration = duration > 0 ? duration : slowdownDuration;
        StartCoroutine(SlowMotionCoroutine(slowFactor, slowDuration));
    }
    
    private IEnumerator SlowMotionCoroutine(float factor, float duration)
    {
        // 원래 타임스케일 저장
        float originalTimeScale = Time.timeScale;
        float originalFixedDeltaTime = Time.fixedDeltaTime;
        
        // 슬로우 모션 적용
        Time.timeScale = factor;
        Time.fixedDeltaTime = Time.fixedDeltaTime * factor;
        
        yield return new WaitForSecondsRealtime(duration);
        
        // 슬로우 모션 회복
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime * slowdownRecoverySpeed;
            float t = Mathf.Clamp01(elapsed);
            Time.timeScale = Mathf.Lerp(factor, originalTimeScale, t);
            Time.fixedDeltaTime = Mathf.Lerp(Time.fixedDeltaTime, originalFixedDeltaTime, t);
            yield return null;
        }
        
        // 원래 값으로 완전히 복구
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
    #endregion

    #region 카메라 블러
    public void ApplyBlur(float intensity = -1, float duration = -1)
    {
        if (blurMaterial == null)
            return;
            
        StopCoroutine(nameof(BlurCoroutine));
        float blurIntensity = intensity > 0 ? intensity : maxBlurAmount;
        float blurDuration = duration > 0 ? duration : 0.5f;
        StartCoroutine(BlurCoroutine(blurIntensity, blurDuration));
    }
    
    private IEnumerator BlurCoroutine(float intensity, float duration)
    {
        isBlurActive = true;
        
        // 블러 적용
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.2f);
            currentBlurAmount = Mathf.Lerp(0, intensity, t);
            UpdateBlurEffect();
            yield return null;
        }
        
        // 유지
        yield return new WaitForSeconds(duration);
        
        // 블러 제거
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.5f);
            currentBlurAmount = Mathf.Lerp(intensity, 0, t);
            UpdateBlurEffect();
            yield return null;
        }
        
        currentBlurAmount = 0f;
        UpdateBlurEffect();
        isBlurActive = false;
    }
    
    private void UpdateBlurEffect()
    {
        if (blurMaterial != null)
        {
            // 커스텀 블러 셰이더의 블러 강도 파라미터 설정
            blurMaterial.SetFloat("_BlurSize", currentBlurAmount);
        }
    }
    #endregion

    #region 카메라 모자이크
    public void ApplyMosaic(float intensity = -1, float duration = -1)
    {
        if (mosaicMaterial == null)
            return;
            
        StopCoroutine(nameof(MosaicCoroutine));
        float mosaicIntensity = intensity > 0 ? intensity : maxMosaicAmount;
        float mosaicDuration = duration > 0 ? duration : 0.5f;
        StartCoroutine(MosaicCoroutine(mosaicIntensity, mosaicDuration));
    }
    
    private IEnumerator MosaicCoroutine(float intensity, float duration)
    {
        isMosaicActive = true;
        
        // 모자이크 적용
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.2f);
            currentMosaicAmount = Mathf.Lerp(0, intensity, t);
            UpdateMosaicEffect();
            yield return null;
        }
        
        // 유지
        yield return new WaitForSeconds(duration);
        
        // 모자이크 제거
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.5f);
            currentMosaicAmount = Mathf.Lerp(intensity, 0, t);
            UpdateMosaicEffect();
            yield return null;
        }
        
        currentMosaicAmount = 0f;
        UpdateMosaicEffect();
        isMosaicActive = false;
    }
    
    private void UpdateMosaicEffect()
    {
        if (mosaicMaterial != null)
        {
            mosaicMaterial.SetFloat("_PixelSize", currentMosaicAmount);
        }
    }
    #endregion

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (isMosaicActive && mosaicMaterial != null)
        {
            Graphics.Blit(source, destination, mosaicMaterial);
        }
        else if (isBlurActive && blurMaterial != null)
        {
            Graphics.Blit(source, destination, blurMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
} 