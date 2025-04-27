using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageData", menuName = "Stage/StageData", order = 1)]
public class StageData : ScriptableObject
{
    public string stageName;
    public float duration; // 스테이지 제한 시간(초)
    public List<EnemyWaveData> enemyWaves;
    public BossData bossData;
    public EnvironmentData environmentData;
    public string tileSet;
} 