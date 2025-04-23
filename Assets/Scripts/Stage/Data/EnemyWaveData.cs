using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class EnemyWaveData
{
    public float spawnTime; // 웨이브 시작 시간(초)
    public GameObject enemyPrefab;
    public int count;
    public List<Transform> spawnPositions;
} 