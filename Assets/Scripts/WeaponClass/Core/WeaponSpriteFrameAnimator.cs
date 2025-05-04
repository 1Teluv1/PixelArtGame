#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WeaponSpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] public SpriteRenderer spriteRenderer; // 할당 필수
    [SerializeField] public List<Sprite> frames;           // PNG Sprite 리스트
    [SerializeField] public float frameRate = 12f;         // 초당 프레임
    [SerializeField] public string spriteSheetPath;
    private int currentFrame;
    private float timer;
    private bool isPlaying = false;
    private int playFrameCount = 0;
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
                Debug.LogError($"[WeaponSpriteFrameAnimator] 에디터에서 스프라이트를 찾을 수 없습니다: {resourcesPath}");
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
        Debug.Log($"로드된 스프라이트 개수: {loaded?.Length ?? 0}, 경로: {path}");
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogError($"[WeaponSpriteFrameAnimator] 런타임에서 스프라이트를 찾을 수 없습니다: {path}");
            return;
        }
        frames = loaded.OrderBy(s => s.name).ToList();
    }
    
    void Update()
    {
        if (!isPlaying)
        {
            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                spriteRenderer.sprite = null;
            }
            return;
        }

        if (frames == null || frames.Count == 0)
        {
            Debug.LogWarning($"[WeaponSpriteFrameAnimator] frames가 비어있음 (gameObject: {gameObject.name})");
            return;
        }
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[WeaponSpriteFrameAnimator] spriteRenderer가 할당되지 않음 (gameObject: {gameObject.name})");
            return;
        }

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame++;
            if (currentFrame >= playFrameCount)
            {
                isPlaying = false;
                currentFrame = 0; // Idle 프레임(첫 프레임)으로 고정
            }
            spriteRenderer.sprite = frames[Mathf.Min(currentFrame, frames.Count - 1)];
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && spriteRenderer != null)
        {
            float dir = player.transform.position.x - transform.position.x;
            spriteRenderer.flipX = dir > 0; // 플레이어가 오른쪽에 있으면 flipX=true(좌우반전)
        }
    }

    public void InitFromData(WeaponData data)
    {
        if (data == null) return;
        if (!string.IsNullOrEmpty(data.texturePath))
        {
            LoadFramesFromResources(data.texturePath);
        }}

    public void PlayOnce()
    {
        isPlaying = true;
        currentFrame = 0;
        playFrameCount = frames != null ? frames.Count : 0;
        timer = 0f;
        if (frames != null && frames.Count > 0 && spriteRenderer != null)
            spriteRenderer.sprite = frames[0]; // 첫 프레임으로 초기화
    }

    public bool IsPlaying() => isPlaying;
}