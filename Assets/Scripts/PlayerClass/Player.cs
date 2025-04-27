using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Player : MonoBehaviour
{
    // 플레이어 컴포넌트 참조
    private PlayerController controller;
    private PlayerStats stats;
    [SerializeField] private WeaponSystem weaponSystem;
    
    // 이벤트
    public delegate void PlayerExperienceCollected(float amount);
    public static event PlayerExperienceCollected OnExperienceCollected;
    
    [Header("기본 무기 설정")]
    public int defaultWeaponId; // 데이터로 할당할 무기 아이디
    public GameObject defaultWeaponPrefab;
    [SerializeField] private GameObject meleeWeaponPrefab;
    [SerializeField] private GameObject rangedWeaponPrefab;
    [SerializeField] private GameObject orbitWeaponPrefab;
    
    private void Awake()
    {
        // 컴포넌트 참조 가져오기
        controller = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
        Debug.Log($"[Player] Awake: controller={(controller != null)}, stats={(stats != null)}, weaponSystem={(weaponSystem != null)}");
        // 필요한 모든 컴포넌트가 있는지 확인
        if (controller == null)
        {
            controller = gameObject.AddComponent<PlayerController>();
            Debug.Log("[Player] PlayerController가 없어서 새로 추가함");
        }
        if (stats == null)
        {
            stats = gameObject.AddComponent<PlayerStats>();
            Debug.Log("[Player] PlayerStats가 없어서 새로 추가함");
        }
    }
    
    private void OnEnable()
    {
        // 이벤트 구독
        PlayerStats.OnPlayerLeveledUp += HandlePlayerLevelUp;
    }
    
    private void OnDisable()
    {
        // 이벤트 구독 취소
        PlayerStats.OnPlayerLeveledUp -= HandlePlayerLevelUp;
    }
    
    private void Start()
    {
        StartCoroutine(WeaponDataLoader.LoadWeaponData((weaponDataList) => {
            Debug.Log($"[Player] 전체 무기 데이터: {JsonConvert.SerializeObject(weaponDataList)}");
            // 기본 무기 프리팹이 할당되어 있으면 자동 장착 (무기 타입 제한 없음)
            if (defaultWeaponPrefab != null)
            {
                var weaponData = weaponDataList.Find(w => w.weaponId == defaultWeaponId);
                Debug.Log($"[Player] Start: defaultWeaponId={defaultWeaponId}, weaponData={(weaponData != null ? weaponData.name : "null")}");
                if (weaponData != null)
                {
                    EquipWeapon(weaponData);
                }
                else
                {
                    Debug.LogWarning($"[Player] Start: 기본 무기 데이터가 없습니다. id={defaultWeaponId}");
                }
            }
            else
            {
                Debug.LogWarning("[Player] Start: defaultWeaponPrefab이 할당되어 있지 않습니다.");
            }
        }));
    }
    
    // 경험치 수집 메서드
    public void CollectExperience(float amount)
    {
        stats.AddExperience(amount);
        OnExperienceCollected?.Invoke(amount);
    }
    
    // 레벨업 처리 메서드
    private void HandlePlayerLevelUp(int newLevel)
    {
        Debug.Log("플레이어가 레벨 " + newLevel + "(으)로 레벨업했습니다!");
        
        // 예시: 3 레벨마다 픽업 범위 증가
        if (newLevel % 3 == 0)
        {
            // 픽업 범위를 증가시키는 로직이 여기에 들어갑니다.
        }
    }

    
    // 체력 픽업 수집 메서드
    public void CollectHealthPickup(float amount)
    {
        controller.Heal(amount);
    }
    
    // 컴포넌트에 대한 공개 getter
    public PlayerController GetController()
    {
        return controller;
    }
    
    public PlayerStats GetStats()
    {
        return stats;
    }
    
    public WeaponSystem GetWeaponSystem()
    {
        return weaponSystem;
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null) return;
        GameObject prefab = null;
        switch (weaponData.type)
        {
            case "Melee":
                prefab = meleeWeaponPrefab;
                break;
            case "Ranged":
                prefab = rangedWeaponPrefab;
                break;
            case "Orbit":
                prefab = orbitWeaponPrefab;
                break;
            // 필요시 추가
            default:
                prefab = defaultWeaponPrefab;
                break;
        }
        string weaponKey = weaponData.type;
        weaponSystem.EquipWeapon(weaponKey, prefab, weaponData);
    }
} 