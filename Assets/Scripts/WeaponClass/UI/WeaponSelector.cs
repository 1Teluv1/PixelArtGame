using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class WeaponSelector : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private Image[] weaponImages = new Image[3]; // 인스펙터에서 1,2,3 순서로 할당
    [SerializeField] private TextMeshProUGUI[] weaponNames = new TextMeshProUGUI[3]; // 인스펙터에서 1,2,3 순서로 할당
    [SerializeField] private int candidateCount = 3; // 고정 3개

    private List<WeaponData> allWeapons;
    private WeaponData[] currentCandidates = new WeaponData[3];

    private void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogError("[WeaponSelector] Inventory를 씬에서 찾을 수 없습니다. 무기 장착이 불가합니다.");
            }
            else
            {
                Debug.Log("[WeaponSelector] Inventory를 자동으로 할당했습니다.");
            }
        }
        // WeaponDataLoader를 통한 비동기 로드
        StartCoroutine(WeaponDataLoader.LoadWeaponData((weaponDataList) => {
            allWeapons = weaponDataList;
            ShowRandomWeaponCandidates();
        }));
    }

    private void ShowRandomWeaponCandidates()
    {
        if (allWeapons == null || allWeapons.Count < candidateCount)
        {
            Debug.LogWarning($"[WeaponSelector] 무기 데이터 부족: allWeapons.Count={allWeapons?.Count ?? -1}");
            return;
        }

        // 랜덤 후보 추출
        List<WeaponData> pool = new List<WeaponData>(allWeapons);
        for (int i = 0; i < candidateCount; i++)
        {
            int idx = Random.Range(0, pool.Count);
            currentCandidates[i] = pool[idx];
            Debug.Log($"[WeaponSelector] {i}번 후보: id={pool[idx].weaponId}, name={pool[idx].name}, imagePath={pool[idx].imagePath}");
            pool.RemoveAt(idx);
        }

        // 이미지/텍스트 세팅 및 클릭 이벤트 등록
        for (int i = 0; i < candidateCount; i++)
        {
            var weapon = currentCandidates[i];
            Debug.Log($"[WeaponSelector] {i}번 무기 UI 세팅: id={weapon.weaponId}, name={weapon.name}, imagePath={weapon.imagePath}");

            // 이미지 세팅
            if (weaponImages[i] != null && !string.IsNullOrEmpty(weapon.imagePath))
            {
                Sprite sprite = Resources.Load<Sprite>(weapon.imagePath.Replace(".png", ""));
                weaponImages[i].sprite = sprite;
                weaponImages[i].raycastTarget = true;
                Debug.Log($"[WeaponSelector] {i}번 무기 이미지 sprite: {(sprite != null ? sprite.name : "null")}");
            }
            else
            {
                Debug.LogWarning($"[WeaponSelector] {i}번 무기 이미지 세팅 실패: imagePath={weapon.imagePath}");
            }

            // 텍스트 세팅
            if (weaponNames[i] != null)
            {
                weaponNames[i].text = weapon.name;
            }
            else
            {
                Debug.LogWarning($"[WeaponSelector] {i}번 무기 텍스트 세팅 실패");
            }

            // 클릭 이벤트 등록 (중복 방지 위해 기존 리스너 제거)
            int capturedIndex = i;
            EventTrigger trigger = weaponImages[i].GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = weaponImages[i].gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { OnWeaponImageClicked(capturedIndex); });
            trigger.triggers.Add(entry);
        }
    }

    private void OnWeaponImageClicked(int index)
    {
        if (index < 0 || index >= currentCandidates.Length) return;
        SelectWeapon(currentCandidates[index]);
    }

    public void SelectWeapon(WeaponData weapon)
    {
        if (playerInventory == null)
        {
            Debug.LogError("Inventory가 할당되지 않았습니다.");
            return;
        }
        if (weapon == null)
        {
            Debug.LogWarning("선택된 무기 데이터가 null입니다.");
            return;
        }
        bool equipped = playerInventory.TryEquipWeapon(weapon);
        if (!equipped)
        {
            Debug.Log($"{weapon.name} 장착 실패 또는 이미 장착됨.");
        }
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
} 