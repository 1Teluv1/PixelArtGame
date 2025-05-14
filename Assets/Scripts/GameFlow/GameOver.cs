using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace GameFlow
{
    public class GameOver : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject gameOverPanel;
        public Image gameOverImage;
        public TextMeshProUGUI gameOverText;
        public Button restartButton;

        [Header("Animation Settings")]
        public float fadeDuration = 1.0f; // 페이드 애니메이션 지속 시간

        // 깜빡임 애니메이션 설정
        [Header("Blink Settings - Image")]
        public float imageBlinkDuration = 0.5f; // 이미지 깜빡임 애니메이션 지속 시간 (한 번 왕복)
        public float imageBlinkTargetAlpha = 0.2f; // 이미지 깜빡일 때의 목표 투명도
        // public LoopType imageBlinkLoopType = LoopType.Yoyo; // 이미지 깜빡임 반복 타입 (DOTween 제거로 주석 처리)

        [Header("Blink Settings - Text")]
        public float textBlinkDuration = 0.5f; // 텍스트 깜빡임 애니메이션 지속 시간 (한 번 왕복)
        public float textBlinkTargetAlpha = 0.2f; // 텍스트 깜빡일 때의 목표 투명도
        // public LoopType textBlinkLoopType = LoopType.Yoyo; // 텍스트 깜빡임 반복 타입 (DOTween 제거로 주석 처리)

        [Header("Blur Settings")]
        public float blurInDuration = 0.3f; // 블러에서 원본으로 복귀하는 시간
        public float blurStartValue = 1.0f; // 블러 시작값 (강하게)
        public float blurEndValue = 0.0f;   // 블러 종료값 (원본)
        private Coroutine blinkCoroutine;
        private Coroutine blurAndAlphaCoroutine;

        void Start()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            // 버튼 클릭 시 씬 재시작 연결
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartScene);
            }

            // 블러+알파 트윈 시작 (완료 후 깜빡임 트윈 시작)
            blurAndAlphaCoroutine = StartCoroutine(BlurAndAlphaCoroutine());
            Time.timeScale = 0;
        }

        public void SetIntroText(string text)
        {
            if (gameOverText != null)
            {
                gameOverText.text = text;
            }
        }

        private IEnumerator BlurAndAlphaCoroutine()
        {
            if (gameOverImage != null && gameOverImage.material != null)
            {
                float elapsed = 0f;
                float startBlur = blurStartValue;
                float endBlur = blurEndValue;
                float duration = blurInDuration;
                float startAlpha = 0f;
                float endAlpha = 1f;
                // 초기값 세팅
                gameOverImage.material.SetFloat("_BlurAmount", startBlur);
                Color c = gameOverImage.color;
                c.a = startAlpha;
                gameOverImage.color = c;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    // 블러 보간
                    float blur = Mathf.Lerp(startBlur, endBlur, t);
                    gameOverImage.material.SetFloat("_BlurAmount", blur);
                    // 알파 보간
                    c.a = Mathf.Lerp(startAlpha, endAlpha, t);
                    gameOverImage.color = c;
                    yield return null;
                }
                // 최종값 보정
                gameOverImage.material.SetFloat("_BlurAmount", endBlur);
                c.a = endAlpha;
                gameOverImage.color = c;
                // 블러+알파 트윈 끝난 뒤에만 깜빡임 트윈 시작
                blinkCoroutine = StartCoroutine(BlinkCoroutine());
            }
            else
            {
                if (gameOverImage != null)
                {
                    Color c = gameOverImage.color;
                    c.a = 1f;
                    gameOverImage.color = c;
                }
                blinkCoroutine = StartCoroutine(BlinkCoroutine());
            }
        }

        private IEnumerator BlinkCoroutine()
        {
            // 이미지/텍스트 깜빡임
            Color imgColor = gameOverImage != null ? gameOverImage.color : Color.white;
            Color txtColor = gameOverText != null ? gameOverText.color : Color.white;
            float imgBaseAlpha = 1f;
            float imgBlinkAlpha = imageBlinkTargetAlpha;
            float imgDuration = imageBlinkDuration;
            float txtBaseAlpha = 1f;
            float txtBlinkAlpha = textBlinkTargetAlpha;
            float txtDuration = textBlinkDuration;
            while (true)
            {
                // 알파 감소 (이미지)
                float elapsedImg = 0f;
                float elapsedTxt = 0f;
                // 동기화: 더 짧은 쪽에 맞춰 반복
                float maxDown = Mathf.Max(imgDuration, txtDuration);
                while (elapsedImg < imgDuration || elapsedTxt < txtDuration)
                {
                    if (gameOverImage != null && elapsedImg < imgDuration)
                    {
                        elapsedImg += Time.unscaledDeltaTime;
                        float tImg = Mathf.Clamp01(elapsedImg / imgDuration);
                        imgColor.a = Mathf.Lerp(imgBaseAlpha, imgBlinkAlpha, tImg);
                        gameOverImage.color = imgColor;
                    }
                    if (gameOverText != null && elapsedTxt < txtDuration)
                    {
                        elapsedTxt += Time.unscaledDeltaTime;
                        float tTxt = Mathf.Clamp01(elapsedTxt / txtDuration);
                        txtColor.a = Mathf.Lerp(txtBaseAlpha, txtBlinkAlpha, tTxt);
                        gameOverText.color = txtColor;
                    }
                    yield return null;
                }
                // 알파 증가 (이미지)
                elapsedImg = 0f;
                elapsedTxt = 0f;
                while (elapsedImg < imgDuration || elapsedTxt < txtDuration)
                {
                    if (gameOverImage != null && elapsedImg < imgDuration)
                    {
                        elapsedImg += Time.unscaledDeltaTime;
                        float tImg = Mathf.Clamp01(elapsedImg / imgDuration);
                        imgColor.a = Mathf.Lerp(imgBlinkAlpha, imgBaseAlpha, tImg);
                        gameOverImage.color = imgColor;
                    }
                    if (gameOverText != null && elapsedTxt < txtDuration)
                    {
                        elapsedTxt += Time.unscaledDeltaTime;
                        float tTxt = Mathf.Clamp01(elapsedTxt / txtDuration);
                        txtColor.a = Mathf.Lerp(txtBlinkAlpha, txtBaseAlpha, tTxt);
                        gameOverText.color = txtColor;
                    }
                    yield return null;
                }
            }
        }

        private IEnumerator FadeOutCoroutine()
        {
            float elapsed = 0f;
            float duration = fadeDuration;
            float startAlpha = gameOverImage != null ? gameOverImage.color.a : 1f;
            float endAlpha = 0f;
            Color c = gameOverImage != null ? gameOverImage.color : Color.white;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (gameOverImage != null)
                {
                    c.a = Mathf.Lerp(startAlpha, endAlpha, t);
                    gameOverImage.color = c;
                }
                if (gameOverText != null)
                {
                    Color tc = gameOverText.color;
                    tc.a = Mathf.Lerp(1f, 0f, t);
                    gameOverText.color = tc;
                }
                yield return null;
            }
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            Time.timeScale = 1;
        }

        // 씬 재시작 함수 (SRP)
        private void RestartScene()
        {

            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex < 0)
            {
                Debug.LogError("[GameOver] 현재 씬 인덱스가 유효하지 않습니다. 씬을 재시작할 수 없습니다.");
                return;
            }
            SceneManager.LoadScene(sceneIndex);
        }

        // Removed Animator related code
        // Removed StartClosingAnimation method
        // Removed OnCloseAnimationFinished method
    }
}