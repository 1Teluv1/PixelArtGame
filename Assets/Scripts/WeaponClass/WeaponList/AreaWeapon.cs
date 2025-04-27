using UnityEngine;

public class AreaWeapon : MonoBehaviour, IWeaponBehaviour
{
    public float radius = 2f;
    public float duration = 1f;
    public GameObject effectPrefab;

    public void Attack()
    {
        // 범위 이펙트 생성 및 데미지 판정
    }
    public void InitFromData(WeaponData data)
    {
        if (data == null)
            return;
        float scaleValueX = (data.scale_x > 0f) ? data.scale_x : 1f;
        float scaleValueY = (data.scale_y > 0f) ? data.scale_y : 1f;
        transform.localScale = new Vector3(scaleValueX, scaleValueY, 1f);
        var animator = GetComponent<WeaponSpriteFrameAnimator>();
        if (animator != null && !string.IsNullOrEmpty(data.texturePath))
        {
            animator.LoadFramesFromResources(data.texturePath);
        }
    }
    
}