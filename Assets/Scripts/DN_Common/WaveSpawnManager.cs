using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;

[System.Serializable]
public class WaveData
{
    public string waveName;
    public float startTime;
    public float endTime;
    public string monsterDataId;
    public float spawnInterval;
    public int spawnCountPerTime;
}

[System.Serializable]
public class LevelData
{
    public string manaBallDataId;
    public string manaSpawnWaveName;
    public int startLevel;
    public int endLevel;
    public float manaSpawnInterval;
    public int manaSpawnCountPerTime;
}

public class WaveSpawnManager : MonoBehaviour
{
    [Header("몬스터 웨이브 타임라인 설정")]
    [SerializeField] private List<WaveData> _waveTimeline;

    [Header("레벨별 마나볼 스폰 설정")]
    [SerializeField] private List<LevelData> _manaTimeLine;

    [Header("스폰 반경 세팅")]
    [SerializeField] private float _spawnRadius = 12f;
    [SerializeField] private float _manaSpawnRadius = 10f;

    public static WaveSpawnManager instance;

    private int _playerLevel;
    private float _gameTimer = 0f;

    private bool _isGameActive = true;

    private Dictionary<WaveData, float> _waveTimers = new Dictionary<WaveData, float>();
    private Dictionary<LevelData, float> _manaTimers = new Dictionary<LevelData, float>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 각 웨이브마다 독립적으로 작동할 주기 타이머 공간 초기화
        foreach (var wave in _waveTimeline)
        {
            _waveTimers.Add(wave, 0f);
        }
        
        foreach (var levelData in _manaTimeLine)
        {
            _manaTimers.Add(levelData, 0f);
        }
    }

    void Update()
    {
        if (DaniTechGameManager.Inst.IsGameStart() == false) return;

        if (!_isGameActive) return;

        // 1. 전체 게임 시간 흘러감
        _gameTimer += Time.deltaTime;

        var player = DaniTechGameObjectManager.Inst.GetLocalPlayer();
        if (player != null)
        {
            _playerLevel = player.GetPlayerLevel();
        }
        else
        {
            _playerLevel = 1; // 플레이어가 로드되기 전이라면 기본 1레벨로 가차 처리
        }

        HandleMonsterSpawn();
        HandleManaSpawn();
    }

    private void HandleMonsterSpawn()
    {
        // 2. 타임라인에 등록된 모든 웨이브 조건을 검사
        foreach (var wave in _waveTimeline)
        {
            // 현재 게임 시간이 이 웨이브의 활성화 시간 범위 안에 있다면?
            if (_gameTimer >= wave.startTime && _gameTimer <= wave.endTime)
            {
                // 해당 웨이브 고유의 개별 타이머 누적
                _waveTimers[wave] += Time.deltaTime;

                // 설정한 스폰 주기에 도달했는지 확인
                if (_waveTimers[wave] >= wave.spawnInterval)
                {
                    _waveTimers[wave] = 0f; // 타이머 리셋

                    // 설정된 마릿수만큼 화면 밖 랜덤 소환 실행
                    SpawnWaveGroup(wave.monsterDataId, wave.spawnCountPerTime, _spawnRadius, false);
                }
            }
        }
    }

    private void HandleManaSpawn()
    {
        foreach (var manaData in _manaTimeLine)
        {
            if (_playerLevel >= manaData.startLevel && _playerLevel <= manaData.endLevel)
            {
                _manaTimers[manaData] += Time.deltaTime;

                if (_manaTimers[manaData] >= manaData.manaSpawnInterval)
                {
                    _manaTimers[manaData] = 0f;

                    SpawnWaveGroup(manaData.manaBallDataId, manaData.manaSpawnCountPerTime, _manaSpawnRadius, true);
                }
            }
        }
    }

    private void SpawnWaveGroup(string dataId, int count, float radius, bool isMana = false)
    {
        var player = DaniTechGameObjectManager.Inst.GetLocalPlayer();
        // 1. 이미 검증된 게임오브젝트 매니저에서 플레이어 위치 가져오기
        if (player == null) return;

        Vector3 playerPos = player.transform.position;

        for (int i = 0; i < count; i++)
        {
            // 2. 플레이어 중심의 화면 밖 무작위 원형 좌표 계산 (삼각함수 활용)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 spawnOffset = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0f) * radius;
            Vector3 finalSpawnPosition = playerPos + spawnOffset;

            // 3. 기존 DaniTechGameObjectManager 구조가 'Transform'을 요구하므로
            // 이 매니저 스크립트가 붙은 오브젝트의 위치를 소환 위치로 잠시 순간이동 시켜서 넘겨줌
            this.transform.position = finalSpawnPosition;

            if (isMana)
            {
                DaniTechGameObjectManager.Inst.CreateFieldObject(dataId, this.transform).Forget();
            }
            else
            {
                DaniTechGameObjectManager.Inst.CreateMonsterObject(dataId, this.transform).Forget();
            }
        }
    }

    // 이벤트 웨이브
    public void TriggerEventWave(string monsterDataId, int totalCount, float duration = 0f)
    {

        // 1. duration이 0이면? 아이템을 먹은 순간 '즉시 동시에' 한꺼번에 스폰 (폭발형 이벤트)
        if (duration <= 0f)
        {
            Debug.Log($"[이벤트 웨이브]: {monsterDataId} 몬스터 {totalCount}마리 즉시 스폰");
            SpawnWaveGroup(monsterDataId, totalCount, _spawnRadius, false);
        }
        // 2. duration이 지정되어 있다면? 그 시간 동안 쪼개서 스폰 (지속형 이벤트)
        else
        {
            Debug.Log($"[이벤트 웨이브] {monsterDataId} 몬스터 총 {totalCount}마리가 {duration}초 동안 나누어 스폰됩니다.");
            RunEventWaveOverTime(monsterDataId, totalCount, duration).Forget();
        }
    }

    private async UniTaskVoid RunEventWaveOverTime(string monsterDataId, int totalCount, float duration)
    {
        // 예: 10마리를 5초 동안 뽑아야 한다면 0.5초마다 1마리씩
        float interval = duration / totalCount;

        for (int i = 0; i < totalCount; i++)
        {
            SpawnWaveGroup(monsterDataId, 1, _spawnRadius, false);

            // 다음 마리 소환 전까지 대기
            await UniTask.Delay(System.TimeSpan.FromSeconds(interval));
        }
    }

    public void ResetWaveManagerOnRestart()
    {
        Debug.LogWarning("[웨이브 매니저] 전체 시간 및 웨이브 타이머 리셋");

        _gameTimer = 0f;
        _isGameActive = true;

        List<WaveData> waves = new List<WaveData>(_waveTimers.Keys);
        foreach (var wave in waves)
        {
            _waveTimers[wave] = 0f;
        }

        List<LevelData> manas = new List<LevelData>(_manaTimers.Keys);
        foreach (var mana in manas)
        {
            _manaTimers[mana] = 0f;
        }
    }






    // 기즈모 (시각적으로 확인 위함) =============================================

    // 에디터 뷰에서 스폰 반경을 시각적으로 조율하기 위한 기즈모
    private void OnDrawGizmosSelected()
    {
        var player = DaniTechGameObjectManager.Inst?.GetLocalPlayer();
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.transform.position, _spawnRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, _manaSpawnRadius);
        }
    }
}
