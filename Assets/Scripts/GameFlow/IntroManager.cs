using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GameFlow
{
    public class IntroManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject introPanel;
        public Image introImage;
        public TextMeshProUGUI introText;
        // public CanvasGroup introCanvasGroup;

        [Header("Animation Settings")]
        public float fadeDuration = 1.0f;

        [Header("Blink Settings - Image")]
        public float imageBlinkDuration = 0.5f;
        public float imageBlinkTargetAlpha = 0.2f;
        // public LoopType imageBlinkLoopType = LoopType.Yoyo; // DOTween 제거로 주석 처리

        [Header("Blink Settings - Text")]
        public float textBlinkDuration = 0.5f;
        public float textBlinkTargetAlpha = 0.2f;
        // public LoopType textBlinkLoopType = LoopType.Yoyo; // DOTween 제거로 주석 처리

        private bool clickHandled = false;
        private Coroutine blinkCoroutine;
        private Coroutine fadeOutCoroutine;

        void Start()
        {
            if (introPanel != null)
                introPanel.SetActive(true);
            blinkCoroutine = StartCoroutine(BlinkCoroutine());
            Time.timeScale = 0;
        }

        void Update()
        {
            if (!clickHandled && Input.GetMouseButtonDown(0))
            {
                clickHandled = true;
                StartClosingTween();
            }
        }

        public void SetIntroText(string text)
        {
            if (introText != null)
            {
                introText.text = text;
            }
        }

        private void StartClosingTween()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            fadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
        }

        private IEnumerator BlinkCoroutine()
        {
            Color imgColor = introImage != null ? introImage.color : Color.white;
            Color txtColor = introText != null ? introText.color : Color.white;
            float imgBaseAlpha = 1f;
            float imgBlinkAlpha = imageBlinkTargetAlpha;
            float imgDuration = imageBlinkDuration;
            float txtBaseAlpha = 1f;
            float txtBlinkAlpha = textBlinkTargetAlpha;
            float txtDuration = textBlinkDuration;
            while (true)
            {
                float elapsedImg = 0f;
                float elapsedTxt = 0f;
                while (elapsedImg < imgDuration || elapsedTxt < txtDuration)
                {
                    if (introImage != null && elapsedImg < imgDuration)
                    {
                        elapsedImg += Time.unscaledDeltaTime;
                        float tImg = Mathf.Clamp01(elapsedImg / imgDuration);
                        imgColor.a = Mathf.Lerp(imgBaseAlpha, imgBlinkAlpha, tImg);
                        introImage.color = imgColor;
                    }
                    if (introText != null && elapsedTxt < txtDuration)
                    {
                        elapsedTxt += Time.unscaledDeltaTime;
                        float tTxt = Mathf.Clamp01(elapsedTxt / txtDuration);
                        txtColor.a = Mathf.Lerp(txtBaseAlpha, txtBlinkAlpha, tTxt);
                        introText.color = txtColor;
                    }
                    yield return null;
                }
                elapsedImg = 0f;
                elapsedTxt = 0f;
                while (elapsedImg < imgDuration || elapsedTxt < txtDuration)
                {
                    if (introImage != null && elapsedImg < imgDuration)
                    {
                        elapsedImg += Time.unscaledDeltaTime;
                        float tImg = Mathf.Clamp01(elapsedImg / imgDuration);
                        imgColor.a = Mathf.Lerp(imgBlinkAlpha, imgBaseAlpha, tImg);
                        introImage.color = imgColor;
                    }
                    if (introText != null && elapsedTxt < txtDuration)
                    {
                        elapsedTxt += Time.unscaledDeltaTime;
                        float tTxt = Mathf.Clamp01(elapsedTxt / txtDuration);
                        txtColor.a = Mathf.Lerp(txtBlinkAlpha, txtBaseAlpha, tTxt);
                        introText.color = txtColor;
                    }
                    yield return null;
                }
            }
        }

        private IEnumerator FadeOutCoroutine()
        {
            float elapsed = 0f;
            float duration = fadeDuration;
            float imgStartAlpha = introImage != null ? introImage.color.a : 1f;
            float imgEndAlpha = 0f;
            float txtStartAlpha = introText != null ? introText.color.a : 1f;
            float txtEndAlpha = 0f;
            Color imgColor = introImage != null ? introImage.color : Color.white;
            Color txtColor = introText != null ? introText.color : Color.white;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (introImage != null)
                {
                    imgColor.a = Mathf.Lerp(imgStartAlpha, imgEndAlpha, t);
                    introImage.color = imgColor;
                }
                if (introText != null)
                {
                    txtColor.a = Mathf.Lerp(txtStartAlpha, txtEndAlpha, t);
                    introText.color = txtColor;
                }
                yield return null;
            }
            if (introPanel != null) introPanel.SetActive(false);
            Time.timeScale = 1;
        }
    }
}