using System;
using System.Collections;
using UnityEngine;

public class Monster2D : DaniTech_MonsterBase
{
    [Header("몬스터 프리팹에서 미리 세팅할 데이터")]
    [SerializeField] private float Skill_CoolTime;
    [SerializeField] private GameObject Prefab_ThisMonsterSkillObject;

    [Header("데이터 확인용")]
    [SerializeField] private int _instanceId;    // 게임에서 태어날때 부여된 ID                 [오브젝트매니저에서 찾는 용도]
    [SerializeField] private string _dataId;     // 내가 누구인지 나중에 찾을 수 있는 고유 ID    [ 데이터 드리븐 용도]

    [Header("전투에서 필요한 데이터")]
    private DNMonsterData _thisMonsterData;
    [SerializeField] private int _baseHp;
    [SerializeField] private int _maxHp;
    [SerializeField] private int _baseAtk;
    [SerializeField] private bool _isAlive = true;
    [SerializeField] private float _damageInterval = 1.0f; // 데미지를 줄 주기
    [SerializeField] private float _moveSpeed = 1.0f;
    [SerializeField] private string _monsterType;

    private float currentDamageTimer = 0f;

    private Transform _playerTransform;

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    private void Start()
    {
        currentDamageTimer = _damageInterval;
        DaniTechUIManager.Instance.AddHudSlot(_instanceId, this.gameObject.transform);
        _playerTransform = DaniTechGameManager.Inst.GetPlayerTransform();
    }

    private void Update()
    {
        bool isGameStart = DaniTechGameManager.Inst.IsGameStart();
        if (isGameStart == false) return;

        Vector2 direction = (_playerTransform.position - this.transform.position).normalized;
        transform.Translate(direction * _moveSpeed * Time.deltaTime);
    }

    private void OnDisable()
    {
        _isAlive = false;
        ResetStatChangedEvent();
    }

    public int GetMonsterInstanceId()
    {
        return _instanceId;
    }

    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        _dataId = dataId;

        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if (monsterData != null)
        {
            _thisMonsterData = monsterData;
            _baseHp = _thisMonsterData.BaseHp;
            _baseAtk = _thisMonsterData.BaseAtk;
            _monsterType = _thisMonsterData.MonsterType;
        }

        _maxHp = _baseHp;
    }

    private void OnBattleUnitDie()
    {
        DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        Destroy(this.gameObject);
    }

    //private int GetFinalNormalAtkDamage(int baseAtk, float normalAtkMultiple)
    //{
    //    return GetFinalSkillDamage(baseAtk, normalAtkMultiple);
    //}

    //private int GetFinalSkillDamage(int baseAtk, float skillMultiple)
    //{
    //    return (int)(baseAtk * skillMultiple);
    //}
    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback, Action<int, int> mpChangeCallback)
    {
        _onHpChanged += hpChangeCallback;
        _onMpChanged += mpChangeCallback;
    }

    public void ResetStatChangedEvent()
    {
        _onHpChanged = null;
        _onMpChanged = null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckCollision(collision);
    }

    private void CheckCollision(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            currentDamageTimer += Time.deltaTime;

            if (currentDamageTimer >= _damageInterval)
            {
                var player = collision.gameObject.GetComponent<Player2D>();
                if (player != null)
                {
                    player.TakeDamage(_baseAtk);
                }

                currentDamageTimer = 0f;
            }
        }
    }

    public void TakeDamage(int playerDamage)
    {
        _baseHp -= playerDamage;
        Debug.LogWarning($"몬스터가 플레이어의 공격을 받아 체력이 {_baseHp} / {_maxHp}가 되었습니다.");

        if (_baseHp <= 0)
        {
            OnBattleUnitDie();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            currentDamageTimer = _damageInterval;
        }
    }
}
