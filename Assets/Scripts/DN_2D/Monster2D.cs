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

    private void OnDisable()
    {
        _isAlive = false;
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
        StartCoroutine(CheckAndUseSkill());
    }

    private int GetFinalNormalAtkDamage(int baseAtk, float normalAtkMultiple)
    {
        return GetFinalSkillDamage(baseAtk, normalAtkMultiple);
    }

    private int GetFinalSkillDamage(int baseAtk, float skillMultiple)
    {
        return (int)(baseAtk * skillMultiple);
    }

    IEnumerator CheckAndUseSkill()
    {
        while (_isAlive)
        {
            yield return new WaitForSeconds(2.0f);

            if (_isAlive == false)
            {
                break;
            }

            UseSkill();
        }
    }

    private void UseSkill()
    {
        var gObj = Instantiate(Prefab_ThisMonsterSkillObject, this.transform.position, Quaternion.identity, DaniTechGameObjectManager.Inst.transform);
        if (gObj == null) return;

        var skillProjectileComponent = gObj.GetComponent<SkillProjectile>();
        if (skillProjectileComponent == null) return;

        Vector3 ShootDirection = this.transform.right;

        skillProjectileComponent.InitSkillObject(ShootDirection, "Enemy");
    }

    //private void OnSkillCollision(int colliedObjecInstanceId)
    //{
    //    if (colliedObjecInstanceId == 0)
    //    {
    //        var player = DaniTechGameObjectManager.Inst.GetLocalPlayer();
    //        player.TakeDamage(_damage);
    //    }
    //}
}
