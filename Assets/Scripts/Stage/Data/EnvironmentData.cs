using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class EnvironmentData
{
    public GameObject backgroundPrefab;
    public List<GameObject> obstaclePrefabs;
    public List<EnvironmentEventData> specialEvents;
} 