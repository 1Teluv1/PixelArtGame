using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// 점수 기반 랭킹 시스템
/// </summary>
public class RankingSystem : MonoBehaviour
{
    // 저장 파일 이름
    private const string RANKING_FILE_NAME = "rankings.json";
    
    // 최대 랭킹 수
    [SerializeField] private int maxRankingEntries = 10;
    
    // 현재 점수
    private int currentScore = 0;
    
    // 랭킹 데이터
    private List<RankingEntry> rankings = new List<RankingEntry>();
    
    // 싱글톤 인스턴스
    private static RankingSystem _instance;
    public static RankingSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RankingSystem>();
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
        DontDestroyOnLoad(gameObject);
        
        // 랭킹 데이터 로드
        LoadRankings();
    }
    
    /// <summary>
    /// 점수 추가
    /// </summary>
    public void AddScore(int score)
    {
        currentScore += score;
    }
    
    /// <summary>
    /// 현재 점수 반환
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// 게임 종료 시 점수 저장
    /// </summary>
    public void SubmitScore(string playerName)
    {
        // 새 랭킹 엔트리 생성
        RankingEntry newEntry = new RankingEntry
        {
            playerName = playerName,
            score = currentScore,
            date = System.DateTime.Now.ToString("yyyy-MM-dd")
        };
        
        // 랭킹에 추가
        rankings.Add(newEntry);
        
        // 점수 순으로 정렬
        rankings = rankings.OrderByDescending(entry => entry.score).ToList();
        
        // 최대 개수 제한
        if (rankings.Count > maxRankingEntries)
        {
            rankings = rankings.Take(maxRankingEntries).ToList();
        }
        
        // 랭킹 저장
        SaveRankings();
        
        // 점수 초기화
        currentScore = 0;
    }
    
    /// <summary>
    /// 랭킹 데이터 로드
    /// </summary>
    private void LoadRankings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, RANKING_FILE_NAME);
        
        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                RankingData data = JsonUtility.FromJson<RankingData>(jsonData);
                
                if (data != null && data.entries != null)
                {
                    rankings = data.entries;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("랭킹 데이터 로드 실패: " + e.Message);
                rankings = new List<RankingEntry>();
            }
        }
        else
        {
            // 파일이 없으면 빈 랭킹으로 초기화
            rankings = new List<RankingEntry>();
        }
    }
    
    /// <summary>
    /// 랭킹 데이터 저장
    /// </summary>
    private void SaveRankings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, RANKING_FILE_NAME);
        
        try
        {
            RankingData data = new RankingData
            {
                entries = rankings
            };
            
            string jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("랭킹 데이터 저장 실패: " + e.Message);
        }
    }
    
    /// <summary>
    /// 랭킹 데이터 가져오기
    /// </summary>
    public List<RankingEntry> GetRankings()
    {
        return rankings;
    }
    
    /// <summary>
    /// 플레이어의 순위 확인 (순위가 없으면 -1 반환)
    /// </summary>
    public int GetPlayerRank(string playerName)
    {
        for (int i = 0; i < rankings.Count; i++)
        {
            if (rankings[i].playerName == playerName)
            {
                return i + 1; // 1부터 시작하는 순위
            }
        }
        
        return -1; // 순위에 없음
    }
    
    /// <summary>
    /// 현재 점수의 예상 랭킹 위치
    /// </summary>
    public int GetEstimatedRank()
    {
        int rank = 1;
        
        foreach (var entry in rankings)
        {
            if (currentScore < entry.score)
            {
                rank++;
            }
            else
            {
                break;
            }
        }
        
        return rank;
    }
    
    /// <summary>
    /// 랭킹 초기화
    /// </summary>
    public void ClearRankings()
    {
        rankings.Clear();
        SaveRankings();
    }
}

/// <summary>
/// 랭킹 항목 클래스
/// </summary>
[System.Serializable]
public class RankingEntry
{
    public string playerName;
    public int score;
    public string date;
}

/// <summary>
/// 랭킹 데이터 직렬화를 위한 클래스
/// </summary>
[System.Serializable]
public class RankingData
{
    public List<RankingEntry> entries;
} 