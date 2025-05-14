using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("플레이어 카메라")]
    [SerializeField] private Camera playerCamera; // 플레이어(메인) 카메라
    [SerializeField] private float spawnDistance = 20f; // 카메라 밖 스폰 거리
    [Header("몬스터 프리팹")]
    [SerializeField] private GameObject monsterPrefab; // 단일 프리팹

    // MonsterData를 받아 몬스터 스폰
    public GameObject SpawnMonster(MonsterData data)
    {
        if (monsterPrefab == null) {
            Debug.LogWarning("[MonsterSpawner] monsterPrefab이 할당되지 않음");
            return null;
        }
        if (playerCamera == null) {
            playerCamera = FindAnyObjectByType<Camera>();
            Debug.LogWarning("[MonsterSpawner] playerCamera가 할당되지 않음");
            return null;
        }
        if (data == null) {
            Debug.LogWarning("[MonsterSpawner] MonsterData가 null임");
            return null;
        }

        Vector3 spawnPos = GetRandomPositionOutsideCamera();
        GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);

        var monsterComp = monster.GetComponent<Monster>();
        if (monsterComp != null)
            monsterComp.InitFromData(data);

        return monster;
    }

    // 카메라 밖 랜덤 위치 계산
    private Vector3 GetRandomPositionOutsideCamera()
    {
        // 카메라 뷰포트의 4방향 중 랜덤 방향 선택
        int edge = Random.Range(0, 4); // 0:상, 1:하, 2:좌, 3:우
        Vector3 spawnViewport = Vector3.zero;
        switch (edge)
        {
            case 0: spawnViewport = new Vector3(Random.value, 1.1f, 0); break; // 위
            case 1: spawnViewport = new Vector3(Random.value, -0.1f, 0); break; // 아래
            case 2: spawnViewport = new Vector3(-0.1f, Random.value, 0); break; // 왼쪽
            case 3: spawnViewport = new Vector3(1.1f, Random.value, 0); break; // 오른쪽
        }
        // 카메라의 현재 위치와 크기를 반영하여 항상 카메라 밖에서 스폰
        float camZ = Mathf.Abs(playerCamera.transform.position.z);
        float camOrthoSize = playerCamera.orthographic ? playerCamera.orthographicSize : 5f;
        float camAspect = playerCamera.aspect;
        float camWidth = camOrthoSize * 2f * camAspect;
        float camHeight = camOrthoSize * 2f;
        Vector3 camCenter = playerCamera.transform.position;
        // ViewportToWorldPoint로 변환 (spawnDistance는 카메라 nearClipPlane이 아니라, 카메라 중심에서의 거리로 사용)
        Vector3 worldPos = playerCamera.ViewportToWorldPoint(new Vector3(spawnViewport.x, spawnViewport.y, camZ));
        // 카메라 밖으로 충분히 떨어뜨리기 위해 방향 벡터로 spawnDistance만큼 이동
        Vector3 dir = (worldPos - camCenter).normalized;
        worldPos = camCenter + dir * (Mathf.Max(camWidth, camHeight) * 0.5f + spawnDistance);
        worldPos.z = 0; // 2D 게임 기준
        return worldPos;
    }
} 