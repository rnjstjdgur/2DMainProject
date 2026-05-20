using System;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_Player;
    [SerializeField] private GameObject Prefab_TilemapChunk;

    [Header("세팅")]
    [SerializeField] private float _chunkSize = 200.0f;

    private Transform _playerTransform;
    private Transform[] _chunkTransforms = new Transform[9];

    private float _halfChunkSize;
    private float _teleportDistance;
    private int _index = 0;

    private void Start()
    {
        _halfChunkSize = _chunkSize * 1.5f;
        _teleportDistance = _chunkSize * 3f;

        GameObject PlayerObj = Instantiate(Prefab_Player, Vector3.zero, Quaternion.identity);
        _playerTransform = PlayerObj.transform;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 spawnPos = new Vector3(x * _chunkSize, y * _chunkSize, 0f);

                GameObject chunkObj = Instantiate(Prefab_TilemapChunk, spawnPos, Quaternion.identity, this.transform);
                _chunkTransforms[_index] = chunkObj.transform;
                _index++;
            }
        }
    }

    private void LateUpdate()
    {
        if (_playerTransform == null) return;

        Vector3 playerPos = _playerTransform.position;

        for (int i = 0; i <  _chunkTransforms.Length; i++)
        {
            Transform chunk = _chunkTransforms[i];
            if (chunk == null) continue;

            float diffX = playerPos.x - chunk.position.x;
            float diffY = playerPos.y - chunk.position.y;

            if (Math.Abs(diffX) > _halfChunkSize)
            {
                float directionX = diffX > 0 ? 1 : -1;
                chunk.Translate(Vector3.right* directionX * _teleportDistance);
            }

            if (Math.Abs(diffY) > _halfChunkSize)
            {
                float directionY = diffY > 0 ? 1 : -1;
                chunk.Translate(Vector3.up * directionY * _teleportDistance);
            }
        }
    }
}
