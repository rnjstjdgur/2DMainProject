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
    [SerializeField] private int _baseAtk;
    [SerializeField] private bool _isAlive = true;
    [SerializeField] private float damageInterval = 1.0f; // 데미지를 줄 주기

    private float currentDamageTimer = 0f;

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    private void Start()
    {
        currentDamageTimer = damageInterval;
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
        }
    }

    private void OnBattleUnitDie()
    {
        DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        Destroy(this.gameObject);
    }

    private int GetFinalNormalAtkDamage(int baseAtk, float normalAtkMultiple)
    {
        return GetFinalSkillDamage(baseAtk, normalAtkMultiple);
    }

    private int GetFinalSkillDamage(int baseAtk, float skillMultiple)
    {
        return (int)(baseAtk * skillMultiple);
    }
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

            if (currentDamageTimer >= damageInterval)
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            currentDamageTimer = damageInterval;
        }
    }
}
