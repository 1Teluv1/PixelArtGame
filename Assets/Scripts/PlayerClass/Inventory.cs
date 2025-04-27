using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxWeaponSlots = 10;
    [SerializeField] private Image[] weaponSlotImages = new Image[10]; // 인스펙터에서 10개 할당
    private List<WeaponData> equippedWeapons = new List<WeaponData>();
    [SerializeField] private Player player; // 인스펙터에서 할당 또는 자동 할당

    public IReadOnlyList<WeaponData> EquippedWeapons => equippedWeapons;

    private void Start()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
            if (player == null)
                Debug.LogWarning("[Inventory] Player를 씬에서 찾을 수 없습니다.");
            else
                Debug.Log("[Inventory] Player를 자동으로 할당했습니다.");
        }
        UpdateWeaponSlotImages();
    }

    // 무기 장착 시도
    public bool TryEquipWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("장착할 무기 데이터가 없습니다.");
            return false;
        }

        // 이미 장착된 무기라면 무시
        if (equippedWeapons.Exists(w => w.weaponId == weapon.weaponId))
        {
            Debug.Log($"{weapon.name}은 이미 장착 중입니다.");
            return false;
        }

        // 슬롯이 가득 찼는지 확인
        if (equippedWeapons.Count >= maxWeaponSlots)
        {
            Debug.Log("무기 슬롯이 가득 찼습니다.");
            return false;
        }

        equippedWeapons.Add(weapon);
        OnWeaponEquipped(weapon);
        UpdateWeaponSlotImages();

        // 플레이어에게 실제 무기 장착
        if (player != null)
        {
            player.EquipWeapon(weapon);
        }
        else
        {
            Debug.LogWarning("[Inventory] Player 참조가 없습니다. 무기 오브젝트는 장착되지 않습니다.");
        }
        return true;
    }

    // 무기 장착 시 호출 (이벤트 등)
    private void OnWeaponEquipped(WeaponData weapon)
    {
        Debug.Log($"{weapon.name} 장착 완료!");
        // 무기 오브젝트 생성, UI 갱신 등
    }

    // 슬롯 이미지 갱신 (구조만, 실제 아이콘 세팅은 추후 구현)
    private void UpdateWeaponSlotImages()
    {
        for (int i = 0; i < weaponSlotImages.Length; i++)
        {
            if (weaponSlotImages[i] != null)
            {
                if (i < equippedWeapons.Count && equippedWeapons[i] != null)
                {
                    string imagePath = equippedWeapons[i].imagePath;
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        Sprite icon = Resources.Load<Sprite>(imagePath.Replace(".png", ""));
                        weaponSlotImages[i].sprite = icon;
                        weaponSlotImages[i].gameObject.SetActive(true);
                        if (icon == null)
                        {
                            Debug.LogWarning($"[Inventory] 슬롯 {i} 무기 아이콘 로드 실패: {imagePath}");
                        }
                    }
                    else
                    {
                        weaponSlotImages[i].sprite = null;
                        weaponSlotImages[i].gameObject.SetActive(false);
                        Debug.LogWarning($"[Inventory] 슬롯 {i} imagePath가 비어있음");
                    }
                }
                else
                {
                    weaponSlotImages[i].sprite = null; // 빈 슬롯
                    weaponSlotImages[i].gameObject.SetActive(false);
                }
            }
        }
    }
}