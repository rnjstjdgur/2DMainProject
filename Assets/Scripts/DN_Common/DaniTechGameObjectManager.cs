using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaniTechGameObjectManager : MonoBehaviour
{
    // 생성할 몬스터의 프리팹
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_SkillProjectile;
    [SerializeField] private GameObject Prefab_Enemy;
    [SerializeField] private GameObject Prefab_Player;

    [Header("트랜스폼")]
    [SerializeField] private Transform Transform_EnemyRoot;
    [SerializeField] private Transform Transform_ManaBallRoot;
    [SerializeField] private Transform Tranform_ProjectileSkillRoot;

    public static DaniTechGameObjectManager Inst { get; set; }

    // 생성된 오브젝트의 키가 됨
    private int _objectInstanceKeyGenerator = 0;

    // 생성된 오브젝트의 생명을 보관
    private Dictionary<int, GameObject> _createdGameObjectContainer = new Dictionary<int, GameObject>();
    private Dictionary<int, DaniTech_2DFieldObject> _fieldObjectContainer = new Dictionary<int, DaniTech_2DFieldObject>();
    private Dictionary<int, Monster2D> _monsterObjectContainer = new Dictionary<int, Monster2D>();

    private Player2D _localPlayer;
    private Monster2D _monster;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        GameObject spawnPlayer = Instantiate(Prefab_Player, Vector3.zero, Quaternion.identity);
        _localPlayer = spawnPlayer.GetComponent<Player2D>();
    }

    public void RegisterLocalPlayer(Player2D localPlayer)
    {
        _localPlayer = localPlayer;
    }

    public Player2D GetLocalPlayer()
    {
        if (_localPlayer == null)
        {
            return null;
        }

        // 우리가 배웠던 원시적인 Get함수입니다. -> 원시적이지만 유용함
        return _localPlayer;
    }

    public void RequestSpawnEnemy()
    {
        if (Prefab_Enemy == null)
        {
            Debug.LogWarning("프리팹이 등록되지 않은 오브젝트 입니다.");
            return;
        }

        var gObj = Instantiate(Prefab_Enemy, Transform_EnemyRoot);
        if (gObj == null)
        {
            Debug.LogWarning("생성에 실패한 게임 오브젝트 입니다.");
            return;
        }

        // 1-1 생성에 성공했다면, 미리 Key를 발급한다.
        _objectInstanceKeyGenerator++;

        // 1-2 Dictionary에 추가하기 전에 미리 키 검사한다
        if (_createdGameObjectContainer.ContainsKey(_objectInstanceKeyGenerator) == true)
        {
            Debug.LogWarning("이미 동일한 키가 발급된 게임 오브젝트가 존재합니다");
            return;
        }

        // 1-3 동적생성(실체화)된 오브젝트를 게임 오브젝트 매니저의 자료구조(Dictionary)에 보관하자!
        _createdGameObjectContainer.Add(_objectInstanceKeyGenerator, gObj);
        InitGeneratedEntityObject(_objectInstanceKeyGenerator, gObj);

        Debug.Log($"키: {_objectInstanceKeyGenerator}의 객체 {gObj.name}이 호출되었습니다.");
    }

    private void InitGeneratedEntityObject(int generatedId, GameObject gObj)
    {
        // 4-1 지금은 Enemy지만, 나중에 IGameEntity 같은 인터페이스로 개선하면 더 좋다
        DaniTech_2DEnemy gameEntity = gObj.GetComponent<DaniTech_2DEnemy>();
        if (gameEntity == null)
        {
            Debug.LogWarning($"생성된 {gObj.name}의 InstanceId를 대입할 수 있는 컴포넌트를 가져올 수 없습니다!");
            return;
        }

        // 4-2 생성된 객체에 정보를 부여한다!
        gameEntity.InitEnemyInfo(generatedId);
    }


    public GameObject GetEntityObjectCanBeNull(int instanceId)
    {
        if (_createdGameObjectContainer.ContainsKey(instanceId) == false)
        {
            Debug.LogWarning($"{instanceId}는 존재하지 않습니다.");
            return null;
        }

        // 2-1 실체화하면서 등록된 게임 오브젝트가 있다면 반환
        return _createdGameObjectContainer[instanceId];
    }

    public void RequestDestroyEntityObject(int instanceId)
    {
        var gObj = GetEntityObjectCanBeNull(instanceId);
        if (gObj == null)
        {
            return;
        }

        // 3-1 요청된 객체를 제거함
        _createdGameObjectContainer.Remove(instanceId);
        Destroy(gObj);
    }


    //[몬스터] ====================================================================================================

    public async UniTaskVoid CreateMonsterObject(string monsterDataId, Transform spawnSpot)
    {
        Debug.Log($"[스폰 요청] {monsterDataId} 생성 시도 시작");
        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(monsterDataId);
        if (monsterData == null)
        {
            Debug.LogWarning($"[스폰 실패] 데이터 매니저에 '{monsterDataId}'에 대한 기획 데이터가 존재하지 않습니다!");
            return;
        }

        var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(monsterData.PrefabPath, Transform_EnemyRoot, true);
        if (createdObj == null)
        {
            Debug.LogError($"[스폰 실패] 프리팹 경로가 잘못되었거나 생성에 실패했습니다: {monsterData.PrefabPath}");
            return;
        }
        createdObj.transform.position = spawnSpot.position;

        AddMonsterObjectOnCreate(createdObj, monsterDataId);
    }

    private void AddMonsterObjectOnCreate(GameObject createdObject, string monsterDataId)
    {
        _objectInstanceKeyGenerator++;
        var generatedInstanceId = _objectInstanceKeyGenerator;

        var monsterComponent = createdObject.GetComponent<Monster2D>();
        if (monsterComponent == null) return;

        _monsterObjectContainer.Add(generatedInstanceId, monsterComponent);

        monsterComponent.InitMonster(generatedInstanceId, monsterDataId);
    }








    //[필드 오브젝트] ====================================================================================================

    public void CreateProjectileSkillObjectByPlayer()
    {
        var player = GetLocalPlayer();
        if (player == null) return;

        bool isGameStart = DaniTechGameManager.Inst.IsGameStart();

        var skillObj = Instantiate(Prefab_SkillProjectile, player.transform.position, Quaternion.identity, Tranform_ProjectileSkillRoot);
        if (skillObj == null) return;

        SkillProjectile skillProjectileComponent = skillObj.GetComponent<SkillProjectile>();
        if (skillProjectileComponent == null) return;

        Vector3 playerDir = player.GetLookDirection();
        var playerId = player.GetPlayerInstanceId();

        var skillCoolTime = skillProjectileComponent.SkillCoolTime();

        skillProjectileComponent.InitSkillObject(playerId, playerDir, "Player", onSkillCollision);
            
    }

    public void StartAutoProjectileSkillLoop()
    {
        AutoSkillLoop().Forget();
    }

    private async UniTaskVoid AutoSkillLoop()
    {
        // 💡 팁: 최초 실행 시 프리팹에서 미리 컴포넌트를 한 번만 읽어와 쿨타임을 체크합니다.
        if (Prefab_SkillProjectile == null) return;
        SkillProjectile sampleComponent = Prefab_SkillProjectile.GetComponent<SkillProjectile>();
        if (sampleComponent == null) return;

        // 프리팹에 설정된 오리지널 쿨타임 가져오기 (예: 1.5초)
        float coolTime = sampleComponent.SkillCoolTime();

        // [무한 루프] 게임이 동작하는 동안 무한 반복
        while (true)
        {
            // 1. 게임 매니저를 통해 현재 게임 상태가 '스타트'인지 매번 실시간으로 확인
            if (DaniTechGameManager.Inst != null && DaniTechGameManager.Inst.IsGameStart())
            {
                var player = GetLocalPlayer();
                if (player != null)
                {
                    // 2. 주기마다 실제 발사할 투사체 오브젝트 동적 생성 (Instantiate)
                    var skillObj = Instantiate(Prefab_SkillProjectile, player.transform.position, Quaternion.identity, Tranform_ProjectileSkillRoot);

                    if (skillObj != null)
                    {
                        SkillProjectile skillProjectileComponent = skillObj.GetComponent<SkillProjectile>();
                        if (skillProjectileComponent != null)
                        {
                            Vector3 playerDir = player.GetLookDirection();
                            var playerId = player.GetPlayerInstanceId();

                            // 3. 생성된 투사체 날려보내기 초기화
                            skillProjectileComponent.InitSkillObject(playerId, playerDir, "Player", onSkillCollision);
                        }
                    }
                }
            }

            // 4. ⭐️ 핵심: 쿨타임(초)만큼 비동기로 정확히 대기합니다! (유니티 멈춤 현상 완전 해결)
            await UniTask.Delay(System.TimeSpan.FromSeconds(coolTime));
        }
    }

    public void onSkillCollision(int colliedObjectInstanceId, int damage)
    {
        if (colliedObjectInstanceId == 0)
        {
            var player = GetLocalPlayer();
            player.TakeDamage(damage);
        }
    }

    public async UniTaskVoid CreateFieldObject(string fieldObjectDataId, Transform spawnSpot)
    {
        var fieldObject = DaniTechGameDataManager.Instance.GetDNFieldObjectData(fieldObjectDataId);
        if (fieldObject != null)
        {
            var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(fieldObject.PrefabPath, Transform_ManaBallRoot, true);
            createdObj.transform.position = spawnSpot.position;
            AddFieldObjectOnCreate(createdObj, fieldObjectDataId);
        }

    }

    private void AddFieldObjectOnCreate(GameObject createdObject, string fieldObjectDataId)
    {
        _objectInstanceKeyGenerator++;
        var generatedInstanceId = _objectInstanceKeyGenerator;
        var fieldObject = createdObject.GetComponent<DaniTech_2DFieldObject>();

        if (fieldObject != null)
        {
            _fieldObjectContainer.Add(generatedInstanceId, fieldObject);
            fieldObject.InitFieldObjectInfoOnCreated(generatedInstanceId, fieldObjectDataId);
        }
    }

    public void RequestDestroyFieldObject(int instanceId)
    {
        var fieldObjectComponent = GetFieldObjectByInstanceId(instanceId);
        if (fieldObjectComponent == null)
        {
            return;
        }

        // 요청된 필드 오브젝트를 제거함
        _fieldObjectContainer.Remove(instanceId);
        Destroy(fieldObjectComponent.gameObject);
    }

    public DaniTech_2DFieldObject GetFieldObjectByInstanceId(int fieldObjectInstanceId)
    {
        if (_fieldObjectContainer.ContainsKey(fieldObjectInstanceId) == false)
        {
            Debug.LogError($"{fieldObjectInstanceId} 찾으려는 필드 오브젝트가 유효하지 않습니다");
            return null;
        }

        return _fieldObjectContainer[fieldObjectInstanceId];
    }

    //코루틴 ====================================================================
    IEnumerator CoWaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
