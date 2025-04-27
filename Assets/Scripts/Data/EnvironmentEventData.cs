using UnityEngine;
using System;

[Serializable]
public class EnvironmentEventData
{
    public float triggerTime; // 이벤트 발생 시간(초)
    public string eventType; // 예: "Storm", "MapShrink", "HealZone"
    public string parameters; // JSON 또는 간단한 파라미터 문자열
} 