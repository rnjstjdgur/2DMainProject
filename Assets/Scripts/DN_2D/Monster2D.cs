using System;
using System.Collections;
using UnityEngine;

public class Monster2D : DaniTech_MonsterBase
{
    [Header("몬스터 프리팹에서 미리 세팅할 데이터")]
    [SerializeField] private float Skill_CoolTime;
    [SerializeField] private SpriteRenderer SpriteRenderer_MonsterSprite;
    [SerializeField] private GameObject Prefab_ThisMonsterSkillObject;

    [Header("데이터 확인용")]
    [SerializeField] private int _instanceId;    // 게임에서 태어날때 부여된 ID                 [오브젝트매니저에서 찾는 용도]
    [SerializeField] private string _dataId;     // 내가 누구인지 나중에 찾을 수 있는 고유 ID    [ 데이터 드리븐 용도]

    [Header("전투에서 필요한 데이터")]
    private DNMonsterData _thisMonsterData;
    [SerializeField] private float _baseHp;
    [SerializeField] private float _maxHp;
    [SerializeField] private int _baseAtk;
    [SerializeField] private bool _isAlive = true;
    [SerializeField] private float _damageInterval = 1.0f; // 데미지를 줄 주기
    [SerializeField] private string _monsterType;

    [Header("이동 관련 정보")]
    [SerializeField] private float _moveSpeed = 1.0f;
    [SerializeField] private Vector2 _direction;

    private float currentDamageTimer = 0f;

    private Transform _playerTransform;

    private event Action<float, float> _onHpChanged;

    private void Start()
    {
        currentDamageTimer = _damageInterval;
        
        _playerTransform = DaniTechGameManager.Inst.GetPlayerTransform();
        SpriteRenderer_MonsterSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        bool isGameStart = DaniTechGameManager.Inst.IsGameStart();
        if (isGameStart == false) return;

        _direction = (_playerTransform.position - this.transform.position).normalized;
        transform.Translate(_direction * _moveSpeed * Time.deltaTime);
        Flip();
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
        Debug.Log($"[Monster2D] 고유번호: {instanceId} / 데이터ID: {dataId} 스폰 완료! (재사용 여부: {gameObject.activeSelf})");
        _instanceId = instanceId;
        _dataId = dataId;
        _isAlive = true;

        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if (monsterData != null)
        {
            _thisMonsterData = monsterData;
            _baseHp = _thisMonsterData.BaseHp;
            _baseAtk = _thisMonsterData.BaseAtk;
            _monsterType = _thisMonsterData.MonsterType;
        }

        _maxHp = _baseHp;

        if (_monsterType == "Elite")
        {
            DaniTechUIManager.Instance.AddHudSlot(_instanceId, this.gameObject.transform);
        }

        InvokeStatChangedEvent();
    }

    private void Flip()
    {
        
        if (_direction.x != 0)
        {
            if (_monsterType == "Normal")
            {
                SpriteRenderer_MonsterSprite.flipX = _direction.x > 0;
            }
            else
            {
                // SpriteRenderer의 flipX를 이용해 이미지를 뒤집습니다.
                // 기본 이미지가 오른쪽을 바라보고 있다면 direction.x < 0 일 때 flipX를 true로 만듭니다.
                SpriteRenderer_MonsterSprite.flipX = _direction.x < 0;
            }
        }
    }

    private void OnBattleUnitDie()
    {
        _isAlive = false;
        DaniTechGameObjectManager.Inst.RequestDespawnMonster(_instanceId, _dataId);
    }

    //private int GetFinalNormalAtkDamage(int baseAtk, float normalAtkMultiple)
    //{
    //    return GetFinalSkillDamage(baseAtk, normalAtkMultiple);
    //}

    //private int GetFinalSkillDamage(int baseAtk, float skillMultiple)
    //{
    //    return (int)(baseAtk * skillMultiple);
    //}
    public void BindOnStatChangedEvent(Action<float, float> hpChangeCallback)
    {
        _onHpChanged += hpChangeCallback;
    }

    public void ResetStatChangedEvent()
    {
        _onHpChanged = null;
    }
    
    private void InvokeStatChangedEvent()
    {
        // 우선 HP든 MP든 하나라도 바뀌면 다 호출해준다
        _onHpChanged?.Invoke(_baseHp, _maxHp);
        // _onMpChanged?.Invoke(_playerMp);
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

    public void TakeDamage(float playerDamage)
    {
        _baseHp -= playerDamage;
        Debug.LogWarning($"몬스터 {_instanceId}가 플레이어의 공격을 받아 체력이 {_baseHp} / {_maxHp}가 되었습니다.");
        InvokeStatChangedEvent();

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
