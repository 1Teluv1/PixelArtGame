using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("레벨 설정")] // Level Settings -> 레벨 설정
    [SerializeField] private int level = 1;
    [SerializeField] private float experience = 0;
    [SerializeField] private float experienceToNextLevel = 100;
    [SerializeField] private float experienceMultiplier = 1.2f;
    
    [Header("능력치 설정")] // Stat Settings -> 능력치 설정
    [SerializeField] private float damageMultiplier = 1.0f;
    [SerializeField] private float attackSpeed = 1.0f;
    [SerializeField] private float pickupRange = 1.5f;
    
    // 이벤트
    public delegate void PlayerLeveledUp(int newLevel);
    public static event PlayerLeveledUp OnPlayerLeveledUp;
    
    public delegate void ExperienceChanged(float currentExp, float maxExp);
    public static event ExperienceChanged OnExperienceChanged;

    private void Start()
    {
        InvokeExperienceChangedEvent();
    }

    public void AddExperience(float amount)
    {
        experience += amount;
        
        // 레벨업 확인
        while (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
        
        InvokeExperienceChangedEvent();
    }
    
    private void LevelUp()
    {
        level++;
        experience -= experienceToNextLevel;
        experienceToNextLevel *= experienceMultiplier;
        
        // 능력치 증가
        damageMultiplier += 0.1f;
        attackSpeed += 0.05f;
        
        // 레벨업 이벤트 호출
        OnPlayerLeveledUp?.Invoke(level);
    }
    
    private void InvokeExperienceChangedEvent()
    {
        OnExperienceChanged?.Invoke(experience, experienceToNextLevel);
    }
    
    // Getters
    public int GetLevel() { return level; }
    public float GetExperience() { return experience; }
    public float GetExperienceToNextLevel() { return experienceToNextLevel; }
    public float GetDamageMultiplier() { return damageMultiplier; }
    public float GetAttackSpeed() { return attackSpeed; }
    public float GetPickupRange() { return pickupRange; }
} 