#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MonsterSpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] public SpriteRenderer spriteRenderer; // 할당 필수
    [SerializeField] public List<Sprite> frames;           // PNG Sprite 리스트
    [SerializeField] public float frameRate = 12f;         // 초당 프레임
    [SerializeField] public string spriteSheetPath; // 예: "Assets/Sprites/Enemy/스켈레톤-변환.png" 또는 Resources 경로

    private int currentFrame;
    private float timer;

    // 런타임에서 Resources 폴더 사용 시 자동 할당
    public void LoadFramesFromResources(string resourcesPath)
    {
#if UNITY_EDITOR
        // 에디터에서는 기존 방식도 허용
        if (Application.isEditor && resourcesPath.StartsWith("Assets/"))
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(resourcesPath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToList();
            if (sprites == null || sprites.Count == 0)
            {
                Debug.LogError($"[MonsterSpriteFrameAnimator] 에디터에서 스프라이트를 찾을 수 없습니다: {resourcesPath}");
                return;
            }
            frames = new List<Sprite>(sprites);
            return;
        }
#endif
        // 런타임에서는 Resources.LoadAll<Sprite> 사용
        string path = resourcesPath;
        if (path.StartsWith("Assets/Resources/"))
            path = path.Substring("Assets/Resources/".Length);
        else if (path.StartsWith("Assets/"))
            path = path.Substring("Assets/".Length);
        if (path.EndsWith(".png"))
            path = path.Substring(0, path.Length - 4);
        var loaded = Resources.LoadAll<Sprite>(path);
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogError($"[MonsterSpriteFrameAnimator] 런타임에서 스프라이트를 찾을 수 없습니다: {path}");
            return;
        }
        frames = loaded.OrderBy(s => s.name).ToList();
    }
    
    void Update()
    {
        if (frames == null || frames.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
        }

        // --- 플레이어 방향에 따라 flipX 처리 ---
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && spriteRenderer != null)
        {
            float dir = player.transform.position.x - transform.position.x;
            spriteRenderer.flipX = dir < 0; // 플레이어가 왼쪽에 있으면 flipX=true(좌우반전)
        }
    }
}