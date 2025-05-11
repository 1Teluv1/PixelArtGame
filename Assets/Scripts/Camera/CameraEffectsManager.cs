using System.Collections;
using UnityEngine;

public class CameraEffectsManager : MonoBehaviour
{
    [Header("카메라 참조")]
    [SerializeField] private Camera mainCamera;

    [Header("카메라 흔들림 설정")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDecay = 0.95f;
    [SerializeField] private float shakeDuration = 0.5f;

    [Header("카메라 슬로우 설정")]
    [SerializeField] private float slowdownFactor = 0.3f;
    [SerializeField] private float slowdownDuration = 1.0f;
    [SerializeField] private float slowdownRecoverySpeed = 2.0f;

    [Header("카메라 블러 설정")]
    [SerializeField] private float maxBlurAmount = 10f;
    [SerializeField] private float blurFadeSpeed = 2f;
    [SerializeField] private Material blurMaterial; // 대체 블러 머티리얼

    [Header("카메라 모자이크 설정")]
    [SerializeField] private float maxMosaicAmount = 64f;
    [SerializeField] private float mosaicFadeSpeed = 2f;
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

    // 내부 변수
    private Vector3 originalPosition;
    private float currentShakeIntensity = 0f;
    private float currentMosaicAmount = 0f;
    private float currentBlurAmount = 0f;
    private float timeScale = 1f;
    private bool isMosaicActive = false;
    private bool isBlurActive = false;
    private bool isShaking = false;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();
            
        originalPosition = transform.localPosition;
    }

    #region 카메라 흔들림
    public void ShakeCamera(float intensity = -1, float duration = -1)
    {
        StopCoroutine(nameof(ShakeCameraCoroutine));
        currentShakeIntensity = intensity > 0 ? intensity : shakeIntensity;
        float shakeDur = duration > 0 ? duration : shakeDuration;
        isShaking = true;
        StartCoroutine(ShakeCameraCoroutine(shakeDur));
    }

    private IEnumerator ShakeCameraCoroutine(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && currentShakeIntensity > 0.01f)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeIntensity;
            transform.localPosition = originalPosition + shakeOffset;
            
            currentShakeIntensity *= shakeDecay;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPosition;
        currentShakeIntensity = 0f;
        isShaking = false;
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