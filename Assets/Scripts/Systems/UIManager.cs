using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 게임 UI 관리 시스템
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("플레이어 상태 UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Header("스킬 UI")]
    [SerializeField] private Image[] skillIcons;
    [SerializeField] private Image[] skillCooldowns;
    
    [Header("알림 UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2f;
    
    [Header("아이템 UI")]
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private GameObject itemSlotPrefab;
    
    // 게임 시간
    private float gameTime = 0f;
    private int killCount = 0;
    
    // 플레이어 레퍼런스
    private PlayerController playerController;
    
    // 싱글톤 인스턴스
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        // 싱글톤 설정
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
    }
    
    private void Start()
    {
        // 플레이어 찾기
        playerController = FindObjectOfType<PlayerController>();
        
        // 초기 UI 설정
        InitializeUI();
        
        // 알림창 초기 설정
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // 플레이어가 없으면 찾기 시도
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null) return;
        }
        
        // 플레이어 상태 UI 업데이트
        UpdatePlayerStatusUI();
        
        // 게임 시간 업데이트
        UpdateGameTime();
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        // 킬 카운트 초기화
        UpdateKillCount(0);
        
        // 스킬 UI 초기화
        InitializeSkillUI();
    }
    
    /// <summary>
    /// 플레이어 상태 UI 업데이트
    /// </summary>
    private void UpdatePlayerStatusUI()
    {
        if (playerController == null) return;
        
        // 체력바 업데이트
        if (healthBar != null)
        {
            healthBar.value = playerController.Health / playerController.MaxHealth;
        }
        
        // 스태미나바 업데이트
        if (staminaBar != null)
        {
            staminaBar.value = playerController.Stamina / playerController.MaxStamina;
        }
    }
    
    /// <summary>
    /// 게임 시간 업데이트
    /// </summary>
    private void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
        
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    /// <summary>
    /// 킬 카운트 업데이트
    /// </summary>
    public void UpdateKillCount(int amount)
    {
        killCount += amount;
        
        if (killCountText != null)
        {
            killCountText.text = string.Format("처치: {0}", killCount);
        }
    }
    
    /// <summary>
    /// 스킬 UI 초기화
    /// </summary>
    private void InitializeSkillUI()
    {
        if (skillCooldowns != null)
        {
            for (int i = 0; i < skillCooldowns.Length; i++)
            {
                if (skillCooldowns[i] != null)
                {
                    skillCooldowns[i].fillAmount = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// 스킬 쿨다운 업데이트
    /// </summary>
    public void UpdateSkillCooldown(int skillIndex, float cooldownRatio)
    {
        if (skillCooldowns != null && skillIndex >= 0 && skillIndex < skillCooldowns.Length)
        {
            if (skillCooldowns[skillIndex] != null)
            {
                skillCooldowns[skillIndex].fillAmount = cooldownRatio;
            }
        }
    }
    
    /// <summary>
    /// 알림 표시
    /// </summary>
    public void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            // 기존 알림 코루틴 중지
            StopAllCoroutines();
            
            // 알림 텍스트 설정
            notificationText.text = message;
            
            // 알림 패널 활성화
            notificationPanel.SetActive(true);
            
            // 자동 숨김 코루틴 시작
            StartCoroutine(HideNotificationAfterDelay());
        }
    }
    
    /// <summary>
    /// 일정 시간 후 알림 숨기기
    /// </summary>
    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 인벤토리에 아이템 추가
    /// </summary>
    public void AddItemToInventory(Sprite itemSprite, string itemName)
    {
        if (inventoryPanel != null && itemSlotPrefab != null)
        {
            // 아이템 슬롯 생성
            GameObject newItemSlot = Instantiate(itemSlotPrefab, inventoryPanel);
            
            // 아이템 이미지 설정
            Image itemImage = newItemSlot.transform.Find("ItemImage")?.GetComponent<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = itemSprite;
            }
            
            // 아이템 이름 설정
            TextMeshProUGUI itemNameText = newItemSlot.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
            if (itemNameText != null)
            {
                itemNameText.text = itemName;
            }
            
            // 아이템 획득 알림 표시
            ShowNotification(itemName + " 획득!");
        }
    }
    
    /// <summary>
    /// 게임 종료 UI 표시
    /// </summary>
    public void ShowGameOverUI(bool isVictory)
    {
        // TODO: 게임 오버 UI 구현
        Debug.Log(isVictory ? "게임 승리!" : "게임 오버!");
        
        // 일시 정지
        Time.timeScale = 0;
    }
    
    /// <summary>
    /// 랭킹 UI 표시
    /// </summary>
    public void ShowRankingUI()
    {
        // TODO: 점수 기반 랭킹 UI 구현
        Debug.Log("랭킹 UI 표시");
    }
    
    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        // 시간 스케일 복구
        Time.timeScale = 1;
        
        // 현재 씬 재로드
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    
    /// <summary>
    /// 메인 메뉴로 이동
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 시간 스케일 복구
        Time.timeScale = 1;
        
        // 메인 메뉴 씬 로드 (씬 인덱스 0이라고 가정)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}