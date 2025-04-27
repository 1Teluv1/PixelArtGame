using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PixelArtGame.Assets.Scripts.Inventory
{
    public class PlayerExp : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider expSlider;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Exp Settings")]
        [SerializeField] private int maxLevel = 99;
        [SerializeField] private int baseExpToLevelUp = 100;
        [SerializeField] private float expGrowth = 1.2f;

        private int currentLevel = 1;
        private int currentExp = 0;
        private int expToNextLevel;

        private PlayerStats playerStats;

        public int CurrentLevel => currentLevel;
        public int CurrentExp => currentExp;
        public int ExpToNextLevel => expToNextLevel;

        public event Action<int> OnLevelUp;
        public event Action<int, int> OnExpChanged;

        private void Awake()
        {
            Debug.Log("[PlayerExp] Awake 호출");
            FindAndAssignUIComponents();
            CalculateExpToNextLevel();
            UpdateUI();
        }

        private void FindAndAssignUIComponents()
        {
            if (expSlider == null)
                expSlider = FindAnyObjectByType<Slider>();
            if (levelText == null)
                levelText = FindAnyObjectByType<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (playerStats == null)
                playerStats = FindAnyObjectByType<PlayerStats>();
            Debug.Log($"[PlayerExp] OnEnable: playerStats={(playerStats != null)}");
            if (playerStats != null)
            {
                PlayerStats.OnExperienceChanged += HandleExperienceChanged;
                PlayerStats.OnPlayerLeveledUp += HandlePlayerLeveledUp;
            }
        }

        private void OnDisable()
        {
            Debug.Log("[PlayerExp] OnDisable: 이벤트 해제");
            PlayerStats.OnExperienceChanged -= HandleExperienceChanged;
            PlayerStats.OnPlayerLeveledUp -= HandlePlayerLeveledUp;
        }

        private void HandleExperienceChanged(float exp, float expToNext)
        {
            Debug.Log($"[PlayerExp] HandleExperienceChanged: exp={exp}, expToNext={expToNext}");
            if (expSlider != null)
            {
                expSlider.maxValue = expToNext;
                expSlider.value = exp;
                Debug.Log($"[PlayerExp] Slider 갱신: value={expSlider.value}, maxValue={expSlider.maxValue}");
            }
        }

        private void HandlePlayerLeveledUp(int newLevel)
        {
            Debug.Log($"[PlayerExp] HandlePlayerLeveledUp: newLevel={newLevel}");
            if (levelText != null)
            {
                levelText.text = $"Lv. {newLevel}";
            }
        }

        public void AddExp(int amount)
        {
            Debug.Log($"[PlayerExp] AddExp 호출: amount={amount}");
            if (amount <= 0)
                return;
            if (currentLevel >= maxLevel)
                return;

            int prevExp = currentExp;
            currentExp += amount;
            while (currentExp >= expToNextLevel && currentLevel < maxLevel)
            {
                currentExp -= expToNextLevel;
                LevelUp();
            }
            UpdateUI();
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }

        private void LevelUp()
        {
            currentLevel++;
            CalculateExpToNextLevel();
            OnLevelUp?.Invoke(currentLevel);
        }

        private void CalculateExpToNextLevel()
        {
            expToNextLevel = Mathf.RoundToInt(baseExpToLevelUp * Mathf.Pow(expGrowth, currentLevel - 1));
        }

        private void UpdateUI()
        {
            if (expSlider != null)
            {
                expSlider.maxValue = expToNextLevel;
                expSlider.value = currentExp;
            }
            if (levelText != null)
            {
                levelText.text = $"Lv. {currentLevel}";
            }
        }

        public void SetExpDirect(int exp, int level)
        {
            if (level < 1 || level > maxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), "레벨은 1 이상, maxLevel 이하만 허용됩니다.");
            if (exp < 0)
                throw new ArgumentOutOfRangeException(nameof(exp), "경험치는 0 이상이어야 합니다.");
            currentLevel = level;
            CalculateExpToNextLevel();
            currentExp = Mathf.Clamp(exp, 0, expToNextLevel);
            UpdateUI();
        }
    }
} 