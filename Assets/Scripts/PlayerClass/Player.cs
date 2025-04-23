using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 플레이어 컴포넌트 참조
    private PlayerController controller;
    private PlayerStats stats;
    private WeaponSystem weaponSystem;
    
    // 이벤트
    public delegate void PlayerExperienceCollected(float amount);
    public static event PlayerExperienceCollected OnExperienceCollected;
    
    private void Awake()
    {
        // 컴포넌트 참조 가져오기
        controller = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
        weaponSystem = GetComponent<WeaponSystem>();
        
        // 필요한 모든 컴포넌트가 있는지 확인
        if (controller == null)
        {
            controller = gameObject.AddComponent<PlayerController>();
        }
        
        if (stats == null)
        {
            stats = gameObject.AddComponent<PlayerStats>();
        }
        
        if (weaponSystem == null)
        {
            weaponSystem = gameObject.AddComponent<WeaponSystem>();
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
    
    // 무기 픽업 수집 메서드
    public void CollectWeaponPickup(string weaponName)
    {
        // 무기가 이미 잠금 해제되었는지 확인
        foreach (var weapon in weaponSystem.GetAvailableWeapons())
        {
            if (weapon.weaponName == weaponName)
            {
                if (weapon.isUnlocked)
                {
                    // 이미 잠금 해제된 경우 레벨업
                    weaponSystem.LevelUpWeapon(weaponName);
                }
                else
                {
                    // 그렇지 않으면 잠금 해제
                    weaponSystem.UnlockWeapon(weaponName);
                }
                break;
            }
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
} 