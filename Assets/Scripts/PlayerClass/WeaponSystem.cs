using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public class WeaponData
    {
        public string weaponName;
        public GameObject projectilePrefab;
        public float damage = 10f;
        public float attackRate = 1f;
        public int projectileCount = 1;
        public float projectileSpeed = 10f;
        public float projectileLifetime = 2f;
        public bool piercing = false;
        public bool isUnlocked = false;
        public int level = 0;
        public int maxLevel = 5;
    }
    
    [Header("Weapons")]
    [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();
    [SerializeField] private List<WeaponData> activeWeapons = new List<WeaponData>();
    [SerializeField] private int maxActiveWeapons = 3;
    
    private PlayerStats playerStats;
    private Dictionary<string, float> weaponCooldowns = new Dictionary<string, float>();
    
    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    
    private void Start()
    {
        // Unlock the first weapon by default
        if (availableWeapons.Count > 0)
        {
            availableWeapons[0].isUnlocked = true;
            availableWeapons[0].level = 1;
            activeWeapons.Add(availableWeapons[0]);
        }
        
        // Initialize cooldowns
        foreach (var weapon in availableWeapons)
        {
            weaponCooldowns[weapon.weaponName] = 0f;
        }
    }
    
    private void Update()
    {
        UpdateWeaponCooldowns();
        FireActiveWeapons();
    }
    
    private void UpdateWeaponCooldowns()
    {
        foreach (var weapon in activeWeapons)
        {
            if (weaponCooldowns[weapon.weaponName] > 0)
            {
                weaponCooldowns[weapon.weaponName] -= Time.deltaTime;
            }
        }
    }
    
    private void FireActiveWeapons()
    {
        foreach (var weapon in activeWeapons)
        {
            if (weapon.level > 0 && weaponCooldowns[weapon.weaponName] <= 0)
            {
                FireWeapon(weapon);
                // Set cooldown based on attack rate and player's attack speed stat
                weaponCooldowns[weapon.weaponName] = 1f / (weapon.attackRate * playerStats.GetAttackSpeed());
            }
        }
    }
    
    private void FireWeapon(WeaponData weapon)
    {
        if (weapon.projectilePrefab == null) return;
        
        float angleStep = 360f / weapon.projectileCount;
        float startAngle = 0;
        
        // Calculate base damage with player's damage multiplier
        float finalDamage = weapon.damage * playerStats.GetDamageMultiplier();
        
        for (int i = 0; i < weapon.projectileCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
            
            GameObject projectile = Instantiate(weapon.projectilePrefab, transform.position, Quaternion.identity);
            
            // Set projectile properties
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.Initialize(finalDamage, direction, weapon.projectileSpeed, weapon.projectileLifetime, weapon.piercing);
            }
            else
            {
                // If no Projectile component, just add velocity to the rigidbody
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * weapon.projectileSpeed;
                }
                Destroy(projectile, weapon.projectileLifetime);
            }
        }
    }
    
    public void LevelUpWeapon(string weaponName)
    {
        foreach (var weapon in availableWeapons)
        {
            if (weapon.weaponName == weaponName && weapon.level < weapon.maxLevel)
            {
                weapon.level++;
                
                // If not already active and we have room, add to active weapons
                if (!activeWeapons.Contains(weapon) && activeWeapons.Count < maxActiveWeapons)
                {
                    activeWeapons.Add(weapon);
                }
                
                break;
            }
        }
    }
    
    public void UnlockWeapon(string weaponName)
    {
        foreach (var weapon in availableWeapons)
        {
            if (weapon.weaponName == weaponName && !weapon.isUnlocked)
            {
                weapon.isUnlocked = true;
                weapon.level = 1;
                
                // If we have room, add to active weapons
                if (activeWeapons.Count < maxActiveWeapons)
                {
                    activeWeapons.Add(weapon);
                }
                
                break;
            }
        }
    }
    
    public List<WeaponData> GetAvailableWeapons()
    {
        return availableWeapons;
    }
    
    public List<WeaponData> GetActiveWeapons()
    {
        return activeWeapons;
    }
} 