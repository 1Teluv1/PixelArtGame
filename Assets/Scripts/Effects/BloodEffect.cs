using UnityEngine;
using System.Collections.Generic;
using System.Linq; // OrderBy 사용을 위해 추가

namespace GameEffects
{
    // 블러드 이펙트의 소스를 나타내는 enum
    public enum BloodSource { Monster, Player }

    public class BloodEffect : MonoBehaviour
    {
        [Header("Settings")]
        public float duration = 1.0f; // 블러드 이펙트 지속 시간 (초)

        [Header("Sprite Sheet Paths (Resources Folder)")]
        // Resources 폴더 내 스프라이트 시트의 경로 (예: "Effects/Blood/Monster", "Effects/Blood/Player")
        public string monsterSpriteSheetPath; 
        public string playerSpriteSheetPath;

        [Header("Animation Settings")]
        public SpriteRenderer spriteRenderer; // 이 이펙트의 SpriteRenderer (할당 필수)
        private List<Sprite> frames;   // 사용될 스프라이트 시트에서 로드된 프레임
        public float frameRate = 12f;  // 초당 프레임

        [Header("Scale Settings")]
        public Vector3 monsterScale = Vector3.one; // 몬스터 블러드 이펙트의 스케일
        public Vector3 playerScale = Vector3.one;  // 플레이어 블러드 이펙트의 스케일

        private int currentFrame;
        private float timer;
        private bool isInitialized = false;

        void Awake()
        {
            // 랜덤 회전 설정
            float randomAngle = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, randomAngle);

            // 파괴는 Awake에서 바로 호출해도 무방합니다.
            Destroy(gameObject, duration);
        }

        /// <summary>
        /// 블러드 이펙트를 초기화하고 사용할 스프라이트 시트 경로를 설정합니다.
        /// 이펙트 생성 직후 호출되어야 합니다.
        /// </summary>
        /// <param name="source"></param>
        public void Initialize(BloodSource source)
        {
            if (isInitialized) return; // 이미 초기화되었으면 중복 실행 방지

            string targetSpriteSheetPath = null;
            Vector3 targetScale = Vector3.one; // 사용할 스케일 변수

            if (source == BloodSource.Monster)
            {
                targetSpriteSheetPath = monsterSpriteSheetPath;
                targetScale = monsterScale; // 몬스터 스케일 설정
            }
            else if (source == BloodSource.Player)
            {
                targetSpriteSheetPath = playerSpriteSheetPath;
                targetScale = playerScale; // 플레이어 스케일 설정
            }

            // 스케일 적용 (랜덤 회전 후 적용되어야 함)
            transform.localScale = targetScale;

            if (string.IsNullOrEmpty(targetSpriteSheetPath))
            {
                Debug.LogError($"[BloodEffect] {source} Sprite sheet path is not assigned!");
                frames = new List<Sprite>(); // 빈 리스트 할당하여 에러 방지
                enabled = false; // 애니메이션 업데이트 비활성화
                return;
            }

            // Resources.LoadAll<Sprite>를 사용하여 경로에서 모든 스프라이트 로드
             var loaded = Resources.LoadAll<Sprite>(targetSpriteSheetPath);
            if (loaded == null || loaded.Length == 0)
            {
                Debug.LogError($"[BloodEffect] Failed to load sprites from path {targetSpriteSheetPath}");
                frames = new List<Sprite>(); // 빈 리스트 할당
                enabled = false; // 애니메이션 업데이트 비활성화
                return;
            }
            // 이름 순으로 정렬하여 애니메이션 순서 보장
            frames = loaded.OrderBy(s => s.name).ToList();


            // 첫 프레임 설정
            if (frames.Count > 0 && spriteRenderer != null)
            {
                 spriteRenderer.sprite = frames[0];
            }
            else
            {
                 // 프레임 로드 실패 시 업데이트 비활성화
                 enabled = false;
                 return;
            }
            
            isInitialized = true;
        }

        // Update is called once per frame
        void Update()
        {
            // 초기화되지 않았거나 프레임이 없으면 업데이트 중지
            if (!isInitialized || frames == null || frames.Count == 0 || spriteRenderer == null) return;

            timer += Time.deltaTime;
            if (timer >= 1f / frameRate)
            {
                timer -= 1f / frameRate;
                currentFrame = (currentFrame + 1);

                // 애니메이션이 끝났는지 확인 (마지막 프레임)
                if (currentFrame >= frames.Count)
                {
                    // 애니메이션이 끝나면 더 이상 업데이트하지 않음
                    enabled = false; // 이 스크립트의 Update를 비활성화
                    return; 
                }

                spriteRenderer.sprite = frames[currentFrame];
            }
        }
    }
}