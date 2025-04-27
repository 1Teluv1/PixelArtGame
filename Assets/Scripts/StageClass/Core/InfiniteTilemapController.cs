using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

namespace PixelArtGame.Assets.Scripts.Stage.Core
{
    public class InfiniteTilemapController : MonoBehaviour
    {
        [Header("필수 참조")]
        [SerializeField] private Tilemap tilemap; // 단일 Tilemap
        [SerializeField] private Transform player; // 플레이어 Transform
        [Header("타일맵 설정")]
        [SerializeField] private int visibleRange = 50; // 가시 범위 (10x10)
        [Header("타일셋 (Sprite)")]
        [SerializeField] private Sprite[] tileSprites; // PNG/Sprite 배열
        [Header("노이즈 설정")]
        [SerializeField] private float noiseScale = 0.1f;

        private TileBase[] tileTypes; // 런타임에 Sprite -> Tile 변환
        private Vector2Int lastPlayerTilePos;
        private HashSet<Vector3Int> lastVisibleTiles = new HashSet<Vector3Int>();
        private Coroutine gradualUpdateCoroutine;

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            if (player != null)
            {
                lastPlayerTilePos = GetPlayerTilePosition();
            }
            if (tileTypes != null && tileTypes.Length > 0)
                UpdateVisibleTiles();
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }
            Vector2Int currentTilePos = GetPlayerTilePosition();
            if (currentTilePos != lastPlayerTilePos)
            {
                lastPlayerTilePos = currentTilePos;
                UpdateVisibleTiles();
            }
        }

        public void AssignPlayer(Transform playerTransform)
        {
            player = playerTransform;
            lastPlayerTilePos = GetPlayerTilePosition();
            UpdateVisibleTiles();
        }

        private Vector2Int GetPlayerTilePosition()
        {
            if (player == null) return Vector2Int.zero;
            Vector3 worldPos = player.position;
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);
            return new Vector2Int(cellPos.x, cellPos.y);
        }

        private void UpdateVisibleTiles()
        {
            if (tilemap == null || tileTypes == null || tileTypes.Length == 0) return;
            Vector2Int center = lastPlayerTilePos;
            int half = visibleRange / 2;
            HashSet<Vector3Int> newVisibleTiles = new HashSet<Vector3Int>();

            // 1. 새로 보이게 되는 타일만 생성
            for (int x = -half; x <= half; x++)
            {
                for (int y = -half; y <= half; y++)
                {
                    Vector2Int worldTilePos = new Vector2Int(center.x + x, center.y + y);
                    Vector3Int cellPos = new Vector3Int(worldTilePos.x, worldTilePos.y, 0);
                    newVisibleTiles.Add(cellPos);

                    if (!lastVisibleTiles.Contains(cellPos))
                    {
                        // 새로 보이게 되는 타일만 생성
                        TileBase tile = GetTileTypeByNoise(worldTilePos);
                        tilemap.SetTile(cellPos, tile);
                    }
                }
            }

            // 2. visibleRange 밖 타일만 제거
            foreach (var prev in lastVisibleTiles)
            {
                if (!newVisibleTiles.Contains(prev))
                {
                    tilemap.SetTile(prev, null);
                }
            }
            lastVisibleTiles = newVisibleTiles;
        }

        private TileBase GetTileTypeByNoise(Vector2Int pos)
        {
            float noise = Mathf.PerlinNoise(pos.x * noiseScale, pos.y * noiseScale);
            int idx = Mathf.FloorToInt(noise * tileTypes.Length);
            idx = Mathf.Clamp(idx, 0, tileTypes.Length - 1);
            return tileTypes[idx];
        }

        private void ConvertSpritesToTiles()
        {
            if (tileSprites == null || tileSprites.Length == 0)
            {
                tileTypes = new TileBase[0];
                return;
            }
            tileTypes = new TileBase[tileSprites.Length];
            for (int i = 0; i < tileSprites.Length; i++)
            {
                if (tileSprites[i] == null)
                {
                    continue;
                }
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = tileSprites[i];
                tileTypes[i] = tile;
            }
        }

        private void ValidateReferences()
        {
            if (tilemap == null)
                return;
            if (player == null)
                return;
        }

        public void LoadTileSpritesFromPath(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
            {
                return;
            }
            // Resources 폴더 기준 경로로 변환
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
                return;
            }
            tileSprites = loaded;
            ConvertSpritesToTiles();
            GradualUpdateVisibleTiles(0.01f); // 0.01초 간격으로 점진적 변경
        }

        public void GradualUpdateVisibleTiles(float delayPerTile = 0.01f)
        {
            if (gradualUpdateCoroutine != null)
                StopCoroutine(gradualUpdateCoroutine);
            gradualUpdateCoroutine = StartCoroutine(GradualUpdateCoroutine(delayPerTile));
        }

        private IEnumerator GradualUpdateCoroutine(float delayPerTile)
        {
            if (tilemap == null || tileTypes == null || tileTypes.Length == 0) yield break;
            Vector2Int center = lastPlayerTilePos;
            int half = visibleRange / 2;
            List<Vector3Int> tilePositions = new List<Vector3Int>();

            // 모든 타일 좌표 수집
            for (int x = -half; x <= half; x++)
            {
                for (int y = -half; y <= half; y++)
                {
                    Vector2Int worldTilePos = new Vector2Int(center.x + x, center.y + y);
                    Vector3Int cellPos = new Vector3Int(worldTilePos.x, worldTilePos.y, 0);
                    tilePositions.Add(cellPos);
                }
            }

            // 노이즈 값 기준 정렬(자연스러운 퍼짐 효과)
            tilePositions.Sort((a, b) => {
                float na = Mathf.PerlinNoise(a.x * noiseScale, a.y * noiseScale);
                float nb = Mathf.PerlinNoise(b.x * noiseScale, b.y * noiseScale);
                return na.CompareTo(nb);
            });

            foreach (var cellPos in tilePositions)
            {
                Vector2Int worldTilePos = new Vector2Int(cellPos.x, cellPos.y);
                TileBase tile = GetTileTypeByNoise(worldTilePos);
                tilemap.SetTile(cellPos, tile);
                yield return new WaitForSeconds(delayPerTile);
            }
        }
    }
}
