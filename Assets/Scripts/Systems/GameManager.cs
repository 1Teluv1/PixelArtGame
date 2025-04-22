using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 게임 전체 흐름 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("게임 설정")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool showFPS = false;
    
    [Header("게임 상태")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private float gameTimer = 0f;
    
    [Header("난이도 설정")]
    [SerializeField] private float difficultyScaling = 0.1f; // 시간에 따른 난이도 증가 비율
    [SerializeField] private float maxDifficulty = 3f; // 최대 난이도
    
    // 게임 내 주요 시스템 참조
    private UIManager uiManager;
    private RankingSystem rankingSystem;
    
    // 싱글톤 인스턴스
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }
    
    // 현재 난이도
    private float currentDifficulty = 1f;
    
    // FPS 표시 관련
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer;
    private int frameCount;
    private float fps;
    
    // 게임 상태 열거형
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
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
        DontDestroyOnLoad(gameObject);
        
        // 프레임 레이트 설정
        Application.targetFrameRate = targetFrameRate;
    }
    
    private void Start()
    {
        // 주요 시스템 참조 찾기
        FindSystemReferences();
        
        // 초기 게임 상태 설정
        SetGameState(currentState);
        
        // 씬 관리 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void Update()
    {
        // 현재 게임 상태에 따른 로직 처리
        switch (currentState)
        {
            case GameState.Playing:
                UpdatePlayingState();
                break;
                
            case GameState.Paused:
                UpdatePausedState();
                break;
                
            case GameState.GameOver:
            case GameState.Victory:
                // 게임 종료 상태 처리
                HandleEndGameState();
                break;
        }
        
        // FPS 업데이트 및 표시
        if (showFPS)
        {
            UpdateFPS();
        }
    }
    
    /// <summary>
    /// 주요 시스템 레퍼런스 찾기
    /// </summary>
    private void FindSystemReferences()
    {
        uiManager = FindObjectOfType<UIManager>();
        rankingSystem = FindObjectOfType<RankingSystem>();
        
        // 없으면 생성
        if (uiManager == null)
        {
            GameObject uiManagerObj = new GameObject("UIManager");
            uiManager = uiManagerObj.AddComponent<UIManager>();
        }
        
        if (rankingSystem == null)
        {
            GameObject rankingSystemObj = new GameObject("RankingSystem");
            rankingSystem = rankingSystemObj.AddComponent<RankingSystem>();
        }
    }
    
    /// <summary>
    /// 씬 로드 완료 시 호출
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 변경 시 시스템 레퍼런스 다시 찾기
        FindSystemReferences();
        
        // 씬 타입에 따른 게임 상태 설정
        if (scene.name == "MainMenu")
        {
            SetGameState(GameState.MainMenu);
        }
        else if (scene.name.Contains("Level") || scene.name.Contains("Game"))
        {
            // 게임 씬이면 플레이 상태로 설정
            StartGame();
        }
    }
    
    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        // 게임 변수 초기화
        gameTimer = 0f;
        currentDifficulty = 1f;
        
        // 상태 변경
        SetGameState(GameState.Playing);
        
        // 필요한 경우 초기화 로직 호출
        if (rankingSystem != null)
        {
            // 점수 초기화
            rankingSystem.AddScore(0);
        }
    }
    
    /// <summary>
    /// 플레이 중 상태 업데이트
    /// </summary>
    private void UpdatePlayingState()
    {
        // 게임 시간 업데이트
        gameTimer += Time.deltaTime;
        
        // 난이도 조정
        UpdateDifficulty();
        
        // ESC 키로 일시 정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    /// <summary>
    /// 일시 정지 상태 업데이트
    /// </summary>
    private void UpdatePausedState()
    {
        // ESC 키로 일시 정지 해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    /// <summary>
    /// 게임 종료 상태 처리
    /// </summary>
    private void HandleEndGameState()
    {
        // R 키로 재시작
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        
        // ESC 키로 메인 메뉴
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }
    
    /// <summary>
    /// 일시 정지 토글
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
        }
        else if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
        }
    }
    
    /// <summary>
    /// 게임 상태 설정
    /// </summary>
    public void SetGameState(GameState newState)
    {
        // 이전 상태 처리
        switch (currentState)
        {
            case GameState.Paused:
                // 일시 정지 해제
                Time.timeScale = 1f;
                break;
        }
        
        // 새 상태 설정
        currentState = newState;
        
        // 새 상태 처리
        switch (newState)
        {
            case GameState.MainMenu:
                // 메인 메뉴 처리
                break;
                
            case GameState.Playing:
                // 플레이 시작 처리
                Time.timeScale = 1f;
                break;
                
            case GameState.Paused:
                // 일시 정지 처리
                Time.timeScale = 0f;
                break;
                
            case GameState.GameOver:
                // 게임 오버 처리
                if (uiManager != null)
                {
                    uiManager.ShowGameOverUI(false);
                }
                break;
                
            case GameState.Victory:
                // 승리 처리
                if (uiManager != null)
                {
                    uiManager.ShowGameOverUI(true);
                }
                break;
        }
    }
    
    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    public void GameOver(bool isVictory)
    {
        SetGameState(isVictory ? GameState.Victory : GameState.GameOver);
        
        // 랭킹 시스템이 있으면 점수 저장
        if (rankingSystem != null)
        {
            // 여기서 점수 구성 (시간, 처치 수 등 기반)
            // 실제 점수 저장은 UI에서 이름 입력 후 처리
        }
    }
    
    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// 메인 메뉴로 이동
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// 난이도 업데이트
    /// </summary>
    private void UpdateDifficulty()
    {
        // 게임 시간에 따라 난이도 증가
        float targetDifficulty = 1f + (gameTimer / 60f) * difficultyScaling;
        currentDifficulty = Mathf.Clamp(targetDifficulty, 1f, maxDifficulty);
        
        // 난이도에 따른 게임 조정 (적 스폰 속도, 적 체력 등)
        // TODO: 난이도에 따른 게임 요소 조정
    }
    
    /// <summary>
    /// FPS 계산 및 표시
    /// </summary>
    private void UpdateFPS()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;
        
        if (fpsTimer >= fpsUpdateInterval)
        {
            fps = frameCount / fpsTimer;
            frameCount = 0;
            fpsTimer = 0f;
        }
    }
    
    /// <summary>
    /// GUI로 FPS 표시
    /// </summary>
    private void OnGUI()
    {
        if (showFPS)
        {
            GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {fps:F1}");
        }
    }
    
    /// <summary>
    /// 게임 현재 난이도 반환
    /// </summary>
    public float GetCurrentDifficulty()
    {
        return currentDifficulty;
    }
    
    /// <summary>
    /// 게임 현재 타이머 반환
    /// </summary>
    public float GetGameTime()
    {
        return gameTimer;
    }
    
    /// <summary>
    /// 게임 현재 상태 반환
    /// </summary>
    public GameState GetGameState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 클린업
    /// </summary>
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
} 