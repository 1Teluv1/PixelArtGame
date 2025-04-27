using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [Header("체력바 (슬라이더)")]
    [SerializeField] private Slider hpSlider;

    // 체력바에 슬라이더 할당 (에디터에서 할당 권장)
    private void Awake()
    {
        if (hpSlider == null)
        {
            Debug.LogWarning("[PlayerHP] hpSlider가 할당되지 않았습니다. 에디터에서 할당하세요.");
        }
    }

    private void OnEnable()
    {
        PlayerController.OnHealthChanged += OnPlayerHealthChanged;
    }

    private void OnDisable()
    {
        PlayerController.OnHealthChanged -= OnPlayerHealthChanged;
    }

    private void OnPlayerHealthChanged(float current, float max)
    {
        SetHP(current, max);
    }

    // 체력바 갱신 함수
    public void SetHP(float current, float max)
    {
        if (hpSlider == null)
        {
            Debug.LogWarning("[PlayerHP] hpSlider가 할당되지 않아 체력 갱신 불가");
            return;
        }
        if (max <= 0f)
        {
            Debug.LogWarning("[PlayerHP] max 값이 0 이하입니다.");
            hpSlider.value = 0f;
            return;
        }
        hpSlider.value = Mathf.Clamp01(current / max);
    }

    // 외부에서 슬라이더를 동적으로 할당할 수 있게 하는 함수 (선택)
    public void SetSlider(Slider slider)
    {
        hpSlider = slider;
    }
} 