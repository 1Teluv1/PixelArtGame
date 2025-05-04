using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponAttackType { Ranged, Melee, Orbit, Area }

[System.Serializable]
public class WeaponSlot
{
    public string weaponKey; // 무기 이름 또는 타입 문자열로 구분
    public GameObject weaponPrefab;
    public GameObject equippedWeaponInstance;
    public WeaponData weaponData;
}

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Type")]
    public WeaponAttackType attackType;

    [Header("무기 슬롯 리스트 (무기별 WeaponMark 지정)")]
    public List<WeaponSlot> weaponSlots = new List<WeaponSlot>();

    // 무기 타입별 WeaponMark를 따로 선언
    [Header("무기 타입별 WeaponMark (에디터에서 할당)")]
    public Transform meleeMark;
    public Transform rangedMark;
    public Transform orbitMark;
    public Transform areaMark;

    // Mark별 좌/우 위치값을 하드코딩/에디터에서 조정할 수 있도록 public 필드로 선언
    [Header("Mark별 좌/우 위치값 (필요시 에디터에서 조정)")]
    public float meleeMarkLeftX = -1f;
    public float meleeMarkLeftY = 1f;
    public float meleeMarkRightX = 1f;
    public float meleeMarkRightY = 1f;
    public float rangedMarkLeftX = -2f;
    public float rangedMarkLeftY = 1f;
    public float rangedMarkRightX = 2f;
    public float rangedMarkRightY = 1f;
    public float orbitMarkLeftX = -1.5f;
    public float orbitMarkLeftY = 1f;
    public float orbitMarkRightX = 1.5f;
    public float orbitMarkRightY = 1f;
    public float areaMarkLeftX = -2.5f;
    public float areaMarkLeftY = 1f;
    public float areaMarkRightX = 2.5f;
    public float areaMarkRightY = 1f;

    private PlayerStats playerStats;
    private Dictionary<string, float> weaponCooldowns = new Dictionary<string, float>();
    
    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    
    private void Start()
    {
        // 더 이상 개별 무기 필드 초기화 없음
        // 무기 슬롯 및 데이터는 외부에서 할당/장착
        // Initialize cooldowns
        weaponCooldowns["Ranged"] = 0f;
        weaponCooldowns["Melee"] = 0f;
        weaponCooldowns["Orbit"] = 0f;
        weaponCooldowns["Area"] = 0f;

        // 게임 시작 시 Mark 위치 자동 설정
        if (meleeMark != null)
            meleeMark.localPosition = new Vector3(meleeMarkLeftX, meleeMarkLeftY, meleeMark.localPosition.z);
        if (rangedMark != null)
            rangedMark.localPosition = new Vector3(rangedMarkLeftX, rangedMarkLeftY, rangedMark.localPosition.z);
        if (orbitMark != null)
            orbitMark.localPosition = new Vector3(orbitMarkLeftX, orbitMarkLeftY, orbitMark.localPosition.z);
        if (areaMark != null)
            areaMark.localPosition = new Vector3(areaMarkLeftX, areaMarkLeftY, areaMark.localPosition.z);
    }
    
    private void Update()
    {
        UpdateWeaponCooldowns();
        FireActiveWeapons();
    }
    
    private void UpdateWeaponCooldowns()
    {
        weaponCooldowns["Ranged"] = Mathf.Max(0, weaponCooldowns["Ranged"] - Time.deltaTime);
        weaponCooldowns["Melee"] = Mathf.Max(0, weaponCooldowns["Melee"] - Time.deltaTime);
        weaponCooldowns["Orbit"] = Mathf.Max(0, weaponCooldowns["Orbit"] - Time.deltaTime);
        weaponCooldowns["Area"] = Mathf.Max(0, weaponCooldowns["Area"] - Time.deltaTime);
    }
    
    private void FireActiveWeapons()
    {
        foreach (var slot in weaponSlots)
        {
            if (slot.equippedWeaponInstance == null) continue;
            var behaviour = slot.equippedWeaponInstance.GetComponent<IWeaponBehaviour>();
            if (behaviour == null) continue;

            if (!weaponCooldowns.ContainsKey(slot.weaponKey))
                weaponCooldowns[slot.weaponKey] = 0f;

            if (weaponCooldowns[slot.weaponKey] <= 0f)
            {
                behaviour.Attack();
                float cooldown = (slot.weaponData != null) ? slot.weaponData.cooldown : 1f;
                weaponCooldowns[slot.weaponKey] = cooldown;
            }
        }
    }
    
    public void LevelUpWeapon(string weaponName)
    {
        // Implementation needed
    }
    
    public void UnlockWeapon(string weaponName)
    {
        // Implementation needed
    }
    
    public float GetWeaponCooldown(string weaponName)
    {
        return weaponCooldowns[weaponName];
    }

    // 무기별 방향 동기화 (string key 기반)
    public void SetWeaponMarkLeft(string weaponKey)
    {
        var slot = weaponSlots.Find(s => s.weaponKey == weaponKey);
        if (slot == null || meleeMark == null) return;
        Vector3 pos = meleeMark.localPosition;
        pos.x = -Mathf.Abs(pos.x);
        meleeMark.localPosition = pos;
    }
    public void SetWeaponMarkRight(string weaponKey)
    {
        var slot = weaponSlots.Find(s => s.weaponKey == weaponKey);
        if (slot == null || meleeMark == null) return;
        Vector3 pos = meleeMark.localPosition;
        pos.x = Mathf.Abs(pos.x);
        meleeMark.localPosition = pos;
    }

    // 무기 장착 (string key 기반)
    public void EquipWeapon(string weaponKey, GameObject prefab, WeaponData data, Transform mark = null)
    {
        Debug.Log($"[WeaponSystem] EquipWeapon 호출: key={weaponKey}, prefab={(prefab != null ? prefab.name : "null")}, mark={(mark != null ? mark.name : "null")}");
        // 이미 같은 무기(weaponId)가 장착되어 있으면 무시
        if (weaponSlots.Exists(s => s.weaponData != null && s.weaponData.weaponId == data.weaponId))
        {
            Debug.Log($"[WeaponSystem] 이미 장착된 무기: {data.name}");
            return;
        }

        // 새 슬롯 추가
        WeaponSlot slot = new WeaponSlot
        {
            weaponKey = weaponKey,
            weaponPrefab = prefab,
            weaponData = data
        };
        // mark가 null이면 자동 할당
        if (mark == null)
        {
            if (weaponKey == "Melee") mark = meleeMark;
            else if (weaponKey == "Ranged") mark = rangedMark;
            else if (weaponKey == "Orbit") mark = orbitMark;
            else if (weaponKey == "Area") mark = areaMark;
            Debug.Log($"[WeaponSystem] 자동 할당된 mark: {(mark != null ? mark.name : "null")}");
        }
        if (prefab != null && mark != null)
        {
            slot.equippedWeaponInstance = Instantiate(prefab, mark.position, mark.rotation, mark);
            Debug.Log($"[WeaponSystem] 무기 인스턴스 생성: {slot.equippedWeaponInstance.name}, 부모: {mark.name}");
            var behaviour = slot.equippedWeaponInstance.GetComponent<IWeaponBehaviour>();
            if (behaviour != null)
            {
                behaviour.InitFromData(data);
                Debug.Log($"[WeaponSystem] InitFromData 호출 완료: {data?.name}");
            }
            else
            {
                Debug.LogWarning($"[WeaponSystem] IWeaponBehaviour를 찾을 수 없음: {slot.equippedWeaponInstance.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[WeaponSystem] 무기 인스턴스 생성 실패: prefab={(prefab != null ? prefab.name : "null")}, mark={(mark != null ? mark.name : "null")}");
        }
        weaponSlots.Add(slot);
        Debug.Log($"[WeaponSystem] 무기 장착 완료: {data.name}");
    }

    public void SetWeaponMarkFlip(bool flip)
    {
        if (meleeMark != null)
        {
            Vector3 pos = meleeMark.localPosition;
            pos.x = flip ? meleeMarkLeftX : meleeMarkRightX;
            pos.y = flip ? meleeMarkLeftY : meleeMarkRightY;
            meleeMark.localPosition = pos;
        }
        if (rangedMark != null)
        {
            Vector3 pos = rangedMark.localPosition;
            pos.x = flip ? rangedMarkLeftX : rangedMarkRightX;
            pos.y = flip ? rangedMarkLeftY : rangedMarkRightY;
            rangedMark.localPosition = pos;
        }
    }
} 