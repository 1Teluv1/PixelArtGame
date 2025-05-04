using UnityEngine;
using System.Collections;

public class WeaponSelectUIAnimator : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public float duration = 0.5f;
    public Vector2 showFrom = new Vector2(0, 800); // 위에서 시작
    public Vector2 showTo = new Vector2(0, 0);     // 중앙
    public Vector2 hideTo = new Vector2(0, -800);  // 아래로 퇴장

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // 등장 애니메이션
        rectTransform.anchoredPosition = showFrom;
        StartCoroutine(AnimatePosition(showFrom, showTo, duration));
    }

    public void HideAndDisable()
    {
        StopAllCoroutines();
        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        yield return AnimatePosition(rectTransform.anchoredPosition, hideTo, duration);
        gameObject.SetActive(false);
    }

    private IEnumerator AnimatePosition(Vector2 from, Vector2 to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(from, to, elapsed / time);
            elapsed += Time.unscaledDeltaTime; // TimeScale 0에서도 동작
            yield return null;
        }
        rectTransform.anchoredPosition = to;
    }
}