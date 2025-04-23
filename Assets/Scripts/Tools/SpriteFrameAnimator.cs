#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] public SpriteRenderer spriteRenderer; // 할당 필수
    [SerializeField] public List<Sprite> frames;           // PNG Sprite 리스트
    [SerializeField] public float frameRate = 12f;         // 초당 프레임
    [SerializeField] public string spriteSheetPath; // 예: "Assets/Sprites/Enemy/스켈레톤-변환.png" 또는 Resources 경로

    private int currentFrame;
    private float timer;

#if UNITY_EDITOR
    // 에디터에서 스프라이트 시트 경로로 프레임 자동 할당
    [ContextMenu("Load Frames From Sprite Sheet (Editor)")]
    public void LoadFramesFromSpriteSheet()
    {
        if (string.IsNullOrEmpty(spriteSheetPath))
        {
            Debug.LogError("spriteSheetPath를 입력하세요.");
            return;
        }
        var sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToList();
        if (sprites.Count == 0)
        {
            Debug.LogError($"스프라이트 시트에서 프레임을 찾을 수 없습니다: {spriteSheetPath}");
            return;
        }
        frames = sprites;
        Debug.Log($"{sprites.Count}개의 프레임이 자동으로 할당되었습니다.");
    }
#endif

    // 런타임에서 Resources 폴더 사용 시 자동 할당
    public void LoadFramesFromResources(string resourcesPath)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(resourcesPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToList();
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogError($"Resources에서 스프라이트를 찾을 수 없습니다: {resourcesPath}");
            return;
        }
        frames = new List<Sprite>(sprites);
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