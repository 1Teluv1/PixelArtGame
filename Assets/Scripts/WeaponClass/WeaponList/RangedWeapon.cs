using UnityEngine;

public class RangedWeapon : MonoBehaviour, IWeaponBehaviour
{
    public GameObject projectileObject; // 씬에 비활성화 상태로 존재
    public float damage = 10f;
    public float speed = 10f;
    public int count = 1;
    public float cooldown = 1f;
    private float lastAttackTime = -999f;
    public WeaponData weaponData;

    public void Attack()
    {
        // projectileObject(원본)가 활성화되어 있으면 비활성화
        if (projectileObject != null && projectileObject.activeSelf)
            projectileObject.SetActive(false);

        if (Time.time < lastAttackTime + cooldown)
            return;
        lastAttackTime = Time.time;

        GameObject nearestMonster = FindNearestMonster();
        if (nearestMonster == null || projectileObject == null)
            return;
        Vector2 dir = ((Vector2)nearestMonster.transform.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        for (int i = 0; i < count; i++)
        {
            GameObject proj = Instantiate(projectileObject, transform.position, rot);
            proj.SetActive(true);

            // Projectile의 WeaponSpriteFrameAnimator에 프레임 할당 (데이터 기반)
            var animators = proj.GetComponentsInChildren<WeaponSpriteFrameAnimator>(true);
            foreach (var animator in animators)
            {
                Debug.Log($"[RangedWeapon] Projectile animator found: {animator.gameObject.name}");
                if (weaponData != null && !string.IsNullOrEmpty(weaponData.texturePath))
                {
                    Debug.Log($"[RangedWeapon] texturePath: {weaponData.texturePath}");
                    animator.LoadFramesFromResources(weaponData.texturePath);
                    animator.PlayOnce();
                }
                else
                {
                    Debug.LogWarning($"[RangedWeapon] weaponData or texturePath is null/empty. weaponData: {weaponData}, texturePath: {(weaponData != null ? weaponData.texturePath : "null")}");
                }
            }

            // ProjectileMove 등 이동 처리
            var move = proj.GetComponent<ProjectileMove>();
            if (move != null)
            {
                move.direction = dir;
                move.speed = speed;
                move.damage = damage;
            }

            // 발사체의 Scale을 무기 데이터에 맞게 조정
            if (weaponData != null)
            {
                float scaleValueX = (weaponData.scale_x > 0f) ? weaponData.scale_x : 1f;
                float scaleValueY = (weaponData.scale_y > 0f) ? weaponData.scale_y : 1f;
                proj.transform.localScale = new Vector3(scaleValueX, scaleValueY, 1f);
            }

            Destroy(proj, 2f);
        }
    }

    private GameObject FindNearestMonster()
    {
        float minDist = float.MaxValue;
        GameObject nearest = null;
        var monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (var m in monsters)
        {
            float dist = Vector2.Distance(transform.position, m.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = m;
            }
        }
        return nearest;
    }

    public void InitFromData(WeaponData data)
    {
        if (data == null)
            return;
        damage = data.damage;
        cooldown = data.cooldown;
        float scaleValueX = (data.scale_x > 0f) ? data.scale_x : 1f;
        float scaleValueY = (data.scale_y > 0f) ? data.scale_y : 1f;
        transform.localScale = new Vector3(scaleValueX, scaleValueY, 1f);
        var animator = GetComponent<WeaponSpriteFrameAnimator>();
        if (animator != null && !string.IsNullOrEmpty(data.texturePath))
        {
            animator.LoadFramesFromResources(data.texturePath);
        }
        weaponData = data;
    }

    private void Update()
    {
        if (Time.time >= lastAttackTime + cooldown)
        {
            Attack();
        }
    }
}