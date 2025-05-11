using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카메라 효과 데모 스크립트
/// </summary>
public class CameraEffectsDemo : MonoBehaviour
{
    [Header("플레이어 참조")]
    [SerializeField] private PlayerController playerController;
    
    [Header("UI 버튼")]
    [SerializeField] private Button shakeButton;
    [SerializeField] private Button slowMotionButton;
    [SerializeField] private Button blurButton;
    [SerializeField] private Button mosaicButton;
    [SerializeField] private Button allEffectsButton;
    
    private CameraEffectsManager cameraEffects;
    
    private void Start()
    {
        // 플레이어 참조가 없으면 자동으로 찾음
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
            
        if (playerController != null)
            cameraEffects = playerController.GetCameraEffects();
            
        // 버튼 동작 설정
        if (shakeButton != null)
            shakeButton.onClick.AddListener(DemoShake);
            
        if (slowMotionButton != null)
            slowMotionButton.onClick.AddListener(DemoSlowMotion);
            
        if (blurButton != null)
            blurButton.onClick.AddListener(DemoBlur);
            
        if (mosaicButton != null)
            mosaicButton.onClick.AddListener(DemoMosaic);
            
        if (allEffectsButton != null)
            allEffectsButton.onClick.AddListener(DemoAllEffects);
    }
    
    private void DemoShake()
    {
        if (cameraEffects != null)
        {
            Debug.Log("[CameraEffectsDemo] 카메라 흔들림 효과 데모");
            cameraEffects.ShakeCamera(0.15f, 0.5f);
        }
    }
    
    private void DemoSlowMotion()
    {
        if (cameraEffects != null)
        {
            Debug.Log("[CameraEffectsDemo] 슬로우모션 효과 데모");
            cameraEffects.SlowMotion(0.3f, 1.0f);
        }
    }
    
    private void DemoBlur()
    {
        if (cameraEffects != null)
        {
            Debug.Log("[CameraEffectsDemo] 블러 효과 데모");
            cameraEffects.ApplyBlur(8f, 0.8f);
        }
    }
    
    private void DemoMosaic()
    {
        if (cameraEffects != null)
        {
            Debug.Log("[CameraEffectsDemo] 모자이크 효과 데모");
            cameraEffects.ApplyMosaic(24f, 0.8f);
        }
    }
    
    private void DemoAllEffects()
    {
        if (cameraEffects != null)
        {
            Debug.Log("[CameraEffectsDemo] 모든 카메라 효과 데모");
            cameraEffects.ShakeCamera(0.2f, 0.6f);
            cameraEffects.SlowMotion(0.2f, 1.0f);
            cameraEffects.ApplyBlur(10f, 0.8f);
            
            // 약간의 딜레이 후 모자이크 효과 적용
            StartCoroutine(DelayedEffect(() => 
            {
                cameraEffects.ApplyMosaic(32f, 0.5f);
            }, 0.2f));
        }
    }
    
    private System.Collections.IEnumerator DelayedEffect(System.Action effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.Invoke();
    }
    
    // 테스트용 단축키
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            DemoShake();
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            DemoSlowMotion();
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            DemoBlur();
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            DemoMosaic();
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            DemoAllEffects();
    }
} 