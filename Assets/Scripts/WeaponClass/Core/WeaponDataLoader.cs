using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

public static class WeaponDataLoader
{
    // 기본 파일명: newtoon.json
    public static IEnumerator LoadWeaponData(System.Action<List<WeaponData>> onLoaded, string jsonPath = null)
    {
        if (string.IsNullOrEmpty(jsonPath))
        {
            jsonPath = Application.streamingAssetsPath + "/DataBase/weapon_data.json";
        }

        string uri = jsonPath;
#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL에서는 file:// 프로토콜이 필요 없음
#else
    if (!uri.StartsWith("file://"))
        uri = "file://" + uri;
#endif

        using (UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"무기 데이터 파일을 읽을 수 없습니다: {uri}, 에러: {www.error}");
                onLoaded?.Invoke(new List<WeaponData>());
                yield break;
            }

            try
            {
                var list = JsonConvert.DeserializeObject<List<WeaponData>>(www.downloadHandler.text);
                onLoaded?.Invoke(list);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"무기 데이터 파싱 실패: {e.Message}");
                onLoaded?.Invoke(new List<WeaponData>());
            }
        }
    }
} 