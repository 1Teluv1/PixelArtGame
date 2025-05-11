using UnityEngine;

/// <summary>
/// 블러 효과를 위한 머티리얼을 생성하는 유틸리티 스크립트
/// </summary>
public class BlurMaterialCreator : MonoBehaviour
{
    [Header("블러 셰이더 참조")]
    [SerializeField] private Shader blurShader;
    
    [Header("생성된 머티리얼(자동 생성)")]
    [SerializeField] private Material blurMaterial;
    
    private void Awake()
    {
        if (blurShader == null)
        {
            blurShader = Shader.Find("Custom/BlurEffect");
            if (blurShader == null)
            {
                Debug.LogError("[BlurMaterialCreator] 'Custom/BlurEffect' 셰이더를 찾을 수 없습니다!");
                return;
            }
        }
        
        // 머티리얼이 없으면 생성
        if (blurMaterial == null)
        {
            blurMaterial = new Material(blurShader);
            blurMaterial.hideFlags = HideFlags.DontSave;
            Debug.Log("[BlurMaterialCreator] 블러 머티리얼이 생성되었습니다.");
        }
        
        // 카메라 이펙트 매니저에 자동 할당
        var cameraEffects = GetComponent<CameraEffectsManager>();
        if (cameraEffects != null)
        {
            // 프로퍼티를 통해 머티리얼 설정
            cameraEffects.BlurMaterial = blurMaterial;
            Debug.Log("[BlurMaterialCreator] 블러 머티리얼이 CameraEffectsManager에 할당되었습니다.");
        }
    }
    
    // 외부에서 머티리얼을 얻기 위한 게터
    public Material GetBlurMaterial()
    {
        return blurMaterial;
    }
} 