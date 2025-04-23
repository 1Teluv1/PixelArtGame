namespace PixelArtGame.Assets.Scripts.Stage.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class StageManager : MonoBehaviour
    {
        // 싱글톤 인스턴스
        public static StageManager Instance { get; private set; }

        // 스테이지 데이터 목록
        [SerializeField] private List<StageData> stageDataList;
        [SerializeField] private int currentStageIndex;
        [SerializeField] private float stageTimer;

        // 주요 이벤트
        public event Action OnStageStart;
        public event Action OnStageEnd;
        public event Action OnBossAppear;
        public event Action OnStageFail;
        public event Action OnStageClear;

        // 스테이지 관리 메서드 시그니처
        public void StartStage(int index)
        {
            OnStageStart?.Invoke();
        }
        public void EndStage(bool isClear)
        {
            // ... (스테이지 종료 로직)
            OnStageEnd?.Invoke();
            if (isClear)
                OnStageClear?.Invoke();
            else
                OnStageFail?.Invoke();
        }
        public void UpdateStageTimer() { }
        public void LoadNextStage() { }
        public void RestartStage() { }
        public void PauseStage() { }
        public void ResumeStage() { }

        // 예시: 보스 등장 시 호출
        public void TriggerBossAppear()
        {
            OnBossAppear?.Invoke();
        }

        private void Awake()
        {
            // 싱글톤 패턴 구현
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}