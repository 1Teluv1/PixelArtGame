using System.Collections;
using UnityEngine;

public class MonsterHitEffectController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material hitMaterial;
    [SerializeField] private float effectDuration = 0.05f;
    [SerializeField] private float maxStrength = 0.1f;
    [SerializeField] private bool isEffectEnabled = false; // 기본값 false(비활성화)
    public bool IsEffectEnabled
    {
        get => isEffectEnabled;
        set
        {
            isEffectEnabled = value;
            // 머티리얼에도 즉시 반영
            if (spriteRenderer != null && spriteRenderer.material != null)
                spriteRenderer.material.SetFloat("_EffectEnabled", isEffectEnabled ? 1f : 0f);
        }
    }

    private Material originalMaterial;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.material;
        // 머티리얼에 이펙트 활성화 상태 반영
        if (spriteRenderer != null && spriteRenderer.material != null)
            spriteRenderer.material.SetFloat("_EffectEnabled", isEffectEnabled ? 1f : 0f);
    }

    public void PlayHitEffect()
    {
        if (!isEffectEnabled) return;
        if (spriteRenderer == null || hitMaterial == null)
            return;
        StartCoroutine(HitWaveEffect());
        Debug.Log("[HitEffect] 히트 이펙트 진행중");
    }

    private IEnumerator HitWaveEffect()
    {
        Material tempMat = new Material(hitMaterial);
        tempMat.SetFloat("_EffectEnabled", isEffectEnabled ? 1f : 0f);
        spriteRenderer.material = tempMat;
        tempMat.SetFloat("_HitBlend", 1f);
        tempMat.SetFloat("_WaveStrength", maxStrength);

        float timer = 0f;
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float t = timer / effectDuration;
            tempMat.SetFloat("_WaveStrength", Mathf.Lerp(maxStrength, 0, t));
            yield return null;
        }
        tempMat.SetFloat("_WaveStrength", 0);
        tempMat.SetFloat("_HitBlend", 0f);
        spriteRenderer.material = originalMaterial;
        Destroy(tempMat);
    }
}