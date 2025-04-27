using UnityEngine;
using System;

[Serializable]
public class BossData
{
    public float spawnTime; // 보스 등장 시간(초)
    public GameObject bossPrefab;
    public Transform spawnPosition;
} 