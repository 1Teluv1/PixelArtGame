namespace PixelArtGame.Assets.Scripts.Stage.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using UnityEngine.Networking;

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

        private InfiniteTilemapController infiniteTilemapController; // Inspector에서 할당

        [SerializeField] private GameObject weaponSelectUIPrefab;
        private GameObject weaponSelectUIInstance;

        private void Start()
        {
            // 씬이 바뀔 때마다 새로 할당
            if (infiniteTilemapController == null)
                infiniteTilemapController = FindAnyObjectByType<InfiniteTilemapController>();

            // 씬 시작 시 자동으로 스테이지 시작 및 몬스터 스폰
            StartStage(currentStageIndex);
        }

        // 스테이지 관리 메서드 시그니처
        public void StartStage(int index)
        {
            OnStageStart?.Invoke();
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
                }
            }

            // 타일셋 연동
            if (infiniteTilemapController != null && loadedStageDataList != null && loadedStageDataList.Count > index)
            {
                var stage = loadedStageDataList[index];
                if (stage.ContainsKey("tileSet"))
                {
                    string tileSetPath = stage["tileSet"] as string;
                    infiniteTilemapController.LoadTileSpritesFromPath(tileSetPath);
                }
            }
            else
            {
                FindAnyObjectByType<InfiniteTilemapController>().LoadTileSpritesFromPath("Assets/Resources/TileSet/TileSet.png");
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
                    continue;
                }
                if (!monsterDataDict.ContainsKey(monsterId))
                {
                    continue;
                }
                MonsterData baseData = monsterDataDict[monsterId];
                for (int i = 0; i < Mathf.Max(1, count); i++)
                {
                    var spawned = monsterSpawner.SpawnMonster(baseData);
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

            // JSON 데이터 로드 (코루틴)
            StartCoroutine(LoadStageDataFromJsonCoroutine((stageList) => {
                loadedStageDataList = stageList;
                // stage 데이터가 로드된 후에 monster 데이터도 로드
                StartCoroutine(LoadMonsterDataFromJsonCoroutine((monsterDict) => {
                    monsterDataDict = monsterDict;
                    // 데이터가 모두 준비된 후에 스테이지 시작
                    StartStage(currentStageIndex);
                }));
            }));
        }

        private System.Collections.IEnumerator LoadStageDataFromJsonCoroutine(System.Action<List<Dictionary<string, object>>> onLoaded)
        {
            string path = Application.streamingAssetsPath + "/DataBase/stage_data.json";
            string uri = path;
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL에서는 file:// 필요 없음
#else
            if (!uri.StartsWith("file://"))
                uri = "file://" + uri;
#endif

            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"스테이지 데이터 파일을 읽을 수 없습니다: {uri}, 에러: {www.error}");
                    onLoaded?.Invoke(new List<Dictionary<string, object>>());
                    yield break;
                }

                try
                {
                    var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(www.downloadHandler.text);
                    onLoaded?.Invoke(list);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"스테이지 데이터 파싱 실패: {e.Message}");
                    onLoaded?.Invoke(new List<Dictionary<string, object>>());
                }
            }
        }

        private System.Collections.IEnumerator LoadMonsterDataFromJsonCoroutine(System.Action<Dictionary<int, MonsterData>> onLoaded)
        {
            string path = Application.streamingAssetsPath + "/DataBase/monster_data.json";
            string uri = path;
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL에서는 file:// 필요 없음
#else
            if (!uri.StartsWith("file://"))
                uri = "file://" + uri;
#endif

            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"몬스터 데이터 파일을 읽을 수 없습니다: {uri}, 에러: {www.error}");
                    onLoaded?.Invoke(new Dictionary<int, MonsterData>());
                    yield break;
                }

                try
                {
                    var monsterList = JsonConvert.DeserializeObject<List<MonsterData>>(www.downloadHandler.text);
                    var dict = new Dictionary<int, MonsterData>();
                    foreach (var monster in monsterList)
                    {
                        if (!dict.ContainsKey(monster.monsterId))
                            dict.Add(monster.monsterId, monster);
                    }
                    onLoaded?.Invoke(dict);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"몬스터 데이터 파싱 실패: {e.Message}");
                    onLoaded?.Invoke(new Dictionary<int, MonsterData>());
                }
            }
        }

        private void OnEnable()
        {
            PlayerStats.OnPlayerLeveledUp += HandlePlayerLevelUp;
        }

        private void OnDisable()
        {
            PlayerStats.OnPlayerLeveledUp -= HandlePlayerLevelUp;
        }

        private void HandlePlayerLevelUp(int newLevel)
        {
            SetStageLevel(newLevel);
            ShowWeaponSelectUI();
        }

        private void SetStageLevel(int level)
        {
            if (loadedStageDataList == null || loadedStageDataList.Count == 0) return;
            currentStageIndex = Mathf.Clamp(level - 1, 0, loadedStageDataList.Count - 1);
            StartStage(currentStageIndex);
        }

        private void ShowWeaponSelectUI()
        {
            if (weaponSelectUIPrefab == null)
            {
                Debug.LogWarning("[StageManager] WeaponSelect UI 프리팹이 할당되어 있지 않습니다.");
                return;
            }
            if (weaponSelectUIInstance != null)
            {
                Destroy(weaponSelectUIInstance);
            }
            weaponSelectUIInstance = Instantiate(weaponSelectUIPrefab, FindCanvasTransform());
        }

        private Transform FindCanvasTransform()
        {
            var canvasObj = GameObject.FindGameObjectWithTag("MainCanvas");
            if (canvasObj == null)
            {
                Debug.LogWarning("[StageManager] 'MainCanvas' 태그를 가진 Canvas를 찾을 수 없습니다.");
                return null;
            }
            var canvas = canvasObj.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[StageManager] 'MainCanvas' 태그가 있지만 Canvas 컴포넌트가 없습니다.");
                return null;
            }
            return canvas.transform;
        }
    }
}