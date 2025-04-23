namespace PixelArtGame.Assets.Scripts.Stage.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;

    public class StageManager : MonoBehaviour
    {
        // 싱글톤 인스턴스
        public static StageManager Instance { get; private set; }
        [SerializeField] private int currentStageIndex;
        [SerializeField] private float stageTimer;

        // 몬스터 스포너 할당
        [SerializeField] private MonsterSpawner monsterSpawner;

        // JSON에서 파싱한 스테이지 데이터 (딕셔너리 리스트)
        private List<Dictionary<string, object>> loadedStageDataList;

        // 몬스터 데이터 딕셔너리 추가
        private Dictionary<int, MonsterData> monsterDataDict;

        // 주요 이벤트
        public event Action OnStageStart;
        public event Action OnStageEnd;
        public event Action OnBossAppear;
        public event Action OnStageFail;
        public event Action OnStageClear;

        private void Start()
        {
            // 씬 시작 시 자동으로 스테이지 시작 및 몬스터 스폰
            StartStage(currentStageIndex);
        }

        // 스테이지 관리 메서드 시그니처
        public void StartStage(int index)
        {
            OnStageStart?.Invoke();
            Debug.Log($"[StageManager] StartStage({index}) 호출됨");
            if (monsterSpawner != null && loadedStageDataList != null && loadedStageDataList.Count > index)
            {
                var stage = loadedStageDataList[index];
                if (stage.ContainsKey("enemyWaves"))
                {
                    var enemyWaves = stage["enemyWaves"] as JArray;
                    if (enemyWaves != null && enemyWaves.Count > 0)
                    {
                        StartCoroutine(SpawnWavesCoroutine(enemyWaves));
                    }
                    else
                    {
                        Debug.LogWarning("[StageManager] enemyWaves가 비어있음");
                    }
                }
                else
                {
                    Debug.LogWarning("[StageManager] stage에 enemyWaves 키가 없음");
                }
            }
            else
            {
                Debug.LogWarning("[StageManager] monsterSpawner가 할당되지 않았거나 stage 데이터가 없음");
            }
        }

        private System.Collections.IEnumerator SpawnWavesCoroutine(JArray enemyWaves)
        {
            // spawnTime 순서대로 정렬
            var sortedWaves = enemyWaves.OrderBy(w => w["spawnTime"] != null ? ((JObject)w)["spawnTime"].Value<float>() : 0f).ToList();
            float lastTime = 0f;
            foreach (JObject waveObj in sortedWaves)
            {
                float spawnTime = waveObj["spawnTime"]?.Value<float>() ?? 0f;
                int monsterId = waveObj["monsterId"]?.Value<int>() ?? -1;
                int count = waveObj["count"]?.Value<int>() ?? 1;
                float waitTime = spawnTime - lastTime;
                if (waitTime > 0f)
                    yield return new UnityEngine.WaitForSeconds(waitTime);
                lastTime = spawnTime;
                if (monsterId == -1)
                {
                    Debug.LogWarning("[StageManager] enemyWaves에 monsterId가 없음");
                    continue;
                }
                if (!monsterDataDict.ContainsKey(monsterId))
                {
                    Debug.LogWarning($"[StageManager] monster_data.json에 monsterId {monsterId}가 없음");
                    continue;
                }
                MonsterData baseData = monsterDataDict[monsterId];
                for (int i = 0; i < Mathf.Max(1, count); i++)
                {
                    var spawned = monsterSpawner.SpawnMonster(baseData);
                    Debug.Log(spawned != null ? $"[StageManager] 몬스터 스폰 성공 (monsterId={monsterId})" : $"[StageManager] 몬스터 스폰 실패 (monsterId={monsterId})");
                }
            }
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

            // JSON 데이터 로드
            LoadStageDataFromJson();
            LoadMonsterDataFromJson();
        }

        private void LoadStageDataFromJson()
        {
            string path = Path.Combine(Application.dataPath, "DataBase/stage_data.json");
            if (!File.Exists(path))
            {
                Debug.LogError("stage_data.json 파일을 찾을 수 없습니다: " + path);
                loadedStageDataList = new List<Dictionary<string, object>>();
                return;
            }
            string json = File.ReadAllText(path);
            loadedStageDataList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
        }

        private void LoadMonsterDataFromJson()
        {
            monsterDataDict = new Dictionary<int, MonsterData>();
            string path = Path.Combine(Application.dataPath, "DataBase/monster_data.json");
            if (!File.Exists(path))
            {
                Debug.LogError("monster_data.json 파일을 찾을 수 없습니다: " + path);
                return;
            }
            string json = File.ReadAllText(path);
            var monsterList = JsonConvert.DeserializeObject<List<MonsterData>>(json);
            foreach (var monster in monsterList)
            {
                if (!monsterDataDict.ContainsKey(monster.monsterId))
                    monsterDataDict.Add(monster.monsterId, monster);
            }
        }
    }
}