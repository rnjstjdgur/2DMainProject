using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaniTechGameObjectManager : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_SkillProjectile;
    [SerializeField] private GameObject Prefab_SkillCircle;
    [SerializeField] private GameObject Prefab_Enemy;
    [SerializeField] private GameObject Prefab_Player;

    [Header("트랜스폼")]
    [SerializeField] private Transform Transform_EnemyRoot;
    [SerializeField] private Transform Transform_ManaBallRoot;
    [SerializeField] private Transform Transform_SkillObjectRoot;

    public static DaniTechGameObjectManager Inst { get; set; }

    // 생성된 오브젝트의 키가 됨
    private int _objectInstanceKeyGenerator = 0;

    private Vector2 _lastOverlapOffset = Vector2.zero;
    private float _lastOverlapRadius = 1f;

    // 생성된 오브젝트의 생명을 보관
    private Dictionary<int, GameObject> _createdGameObjectContainer = new Dictionary<int, GameObject>();
    private Dictionary<int, DaniTech_2DFieldObject> _fieldObjectContainer = new Dictionary<int, DaniTech_2DFieldObject>();
    private Dictionary<int, Monster2D> _monsterObjectContainer = new Dictionary<int, Monster2D>();
    private Dictionary<string, int> _skillList = new Dictionary<string, int>();

    private Player2D _localPlayer;
    private Monster2D _monster;

    private DNSkillData _circleSkillData;

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
        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(monsterDataId);
        if (monsterData == null)
        {
            Debug.LogError($"[스폰 실패] 데이터 매니저에 '{monsterDataId}'에 대한 기획 데이터가 존재하지 않습니다!");
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







    // 스킬 관련 =============================================================

    public void StartAutoProjectileSkillLoop()
    {
        AutoSkillLoop(Prefab_SkillProjectile, Transform_SkillObjectRoot, "skill_magicArrow_01").Forget();
    }

    public void StartAutoCircleSkillLoop()
    {
        AutoSkillLoop(Prefab_SkillCircle, Transform_SkillObjectRoot, "skill_fire_01").Forget();
    }

    private async UniTaskVoid AutoSkillLoop(GameObject Prefab_Skill, Transform Transform_Root, string skillDataId)
    {
        if (Prefab_Skill == null) return;

        ISkillObject sampleComponent = Prefab_Skill.GetComponent<ISkillObject>();
        if (sampleComponent == null) return;

        float coolTime = sampleComponent.GetSkillCoolTime();

        // [무한 루프] 게임이 동작하는 동안 무한 반복
        while (true)
        {
            // 1. 게임 매니저를 통해 현재 게임 상태가 '스타트'인지 매번 실시간으로 확인
            if (DaniTechGameManager.Inst != null && DaniTechGameManager.Inst.IsGameStart())
            {
                var player = GetLocalPlayer();
                if (player != null)
                {
                    if (GetSkillLevel(skillDataId) >= 1)
                    {
                        // 2. 주기마다 실제 발사할 투사체 오브젝트 동적 생성 (Instantiate)
                        var skillObj = Instantiate(Prefab_Skill, player.transform.position, Quaternion.identity, Transform_Root);

                        if (skillObj != null)
                        {
                            ISkillObject skillComponent = skillObj.GetComponent<ISkillObject>();
                            if (skillComponent != null)
                            {
                                Vector3 playerDir = player.GetLookDirection();
                                var playerId = player.GetPlayerInstanceId();

                                // 3. 생성된 투사체 날려보내기 초기화
                                skillComponent.InitSkillObject(playerId, playerDir, "Player", onSkillCollision);
                            }
                        }
                    }
                }
            }

            await UniTask.Delay(System.TimeSpan.FromSeconds(coolTime));
        }
    }

    public void onSkillCollision(SkillCollisionInfo info)
    {
        if (info.TargetCollider == null) return;

        int calculatedDamage = 0;
        var skillTableData = DaniTechGameDataManager.Instance.GetSkill(info.SkillDataId);

        if (skillTableData != null)
        {
            calculatedDamage = skillTableData.SkillDamage;
        }
        else
        {
            calculatedDamage = 0;
        }

        var player = info.TargetCollider.GetComponent<Player2D>();
        if (player != null)
        {
            player.TakeDamage(calculatedDamage);
            return;
        }

        var monster = info.TargetCollider.GetComponent<Monster2D>();
        if (monster != null)
        {
            monster.TakeDamage(calculatedDamage);
            Debug.Log($"[매니저] 몬스터 {monster.name}에게 스킬 {info.SkillDataId}로 대미지 {calculatedDamage} 전달!");
        }
    }

    public void UpgradeSkillLevel(string skillDataId)
    {
        var skillData = DaniTechGameDataManager.Instance.GetSkill(skillDataId);
        if (skillData == null)
        {
            Debug.LogWarning($"{skillDataId}의 스킬 데이터가 데이터 매니저에 없습니다.");
            return;
        }

        if (_skillList.ContainsKey(skillDataId))
        {
            _skillList[skillDataId]++;
            Debug.LogWarning($"스킬 {skillData.Name}의 레벨이 {_skillList[skillDataId]}로 올랐습니다");
        }
        else
        {
            _skillList.Add(skillDataId, 1);
            Debug.LogWarning($"새로운 스킬 {skillData.Name}을 배웠습니다. 스킬레벨: {_skillList[skillDataId]}");
        }

        // 원본 데이터 혹은 세션 데이터의 레벨 갱신
        skillData.SkillLevel = _skillList[skillDataId];
    }

    // 3. 기존의 GetSkillLevel 함수도 매니저의 딕셔너리를 기반으로 작동하도록 안전하게 변경
    public int GetSkillLevel(string skillDataId)
    {
        if (_skillList.TryGetValue(skillDataId, out int level))
        {
            return level;
        }

        // 딕셔너리에 없다면(레벨업 전) 데이터에서 레벨을 확인 (기초마법을 사용하기 위함)
        var skillData = DaniTechGameDataManager.Instance.GetSkill(skillDataId);
        if (skillData != null)
        {
            return skillData.SkillLevel;
        }
        return 0; // 아직 배우지 않은 스킬은 레벨 0 반환
    }


    //[필드 오브젝트] ====================================================================================================

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
