using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillProjectile : DaniTech_SkillBase, ISkillObject
{
    [Header("스킬 고유 ID")]
    [SerializeField] private string _skillDataId = "skill_magicArrow_01";

    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer_Effect;

    [Header("스킬 이동 관련")]
    [SerializeField] private float _skillMoveSpeed = 10.0f;  // [ToDo] 나중에 데이터로 받아와서 스킬의 속도를 대입하자
    [SerializeField] private float _scanRadius = 20.0f; // 몬스터를 탐색할 범위 (반지름)
    [SerializeField] private LayerMask _enemyLayer;

    [Header("스킬 전투 관련")]
    [SerializeField] private float _skillDistance = 10.0f;  // 스킬범위
    [SerializeField] private float _skillCoolTime = 1.0f;

    private int _damage = 100;
    private int _ownerInstanceId;

    private Action<SkillCollisionInfo> _collisionCallback;

    private Vector3 _moveDirection;
    private float _skillDurationTime = 3.0f;    // [ToDo] 나중에 데이터로 받아와서 스킬의 유지시간을 대입하자 (스킬이 강화되면 유지시간이 늘어나는식)


    private event Action<int, int> _onSkillCollision;


    // 인터페이스 멤버 ============================================
    public float GetSkillCoolTime()
    {
        return _skillCoolTime;
    }

    public void InitSkillObject(int ownerInstanceId, Vector3 direction, string targetTag, Action<SkillCollisionInfo> collisionCallback)
    {
        _ownerInstanceId = ownerInstanceId;
        _collisionCallback = collisionCallback;
        this.gameObject.tag = targetTag; // 필요시 태그 부여

        // 1. 기획 데이터 테이블 연동
        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        if (skillData != null)
        {
            _skillCoolTime = skillData.SkillCoolTime;
            _damage = skillData.SkillDamage;
            // _skillMoveSpeed = skillData.SkillSpeed; 
        }
        else
        {
            _skillCoolTime = 1.0f;
            Debug.LogWarning($"[SkillProjectile] '{_skillDataId}' 데이터를 찾지 못해 인스펙터 기본값으로 작동합니다.");
        }

        // 2. 타겟팅 유도 시스템 가동 (주변에 가장 가까운 적 찾기)
        Transform targetEnemy = DaniTechGameManager.Inst.GetClosestEnemy(this.transform.position, _scanRadius, _enemyLayer, _skillDistance);

        if (targetEnemy != null)
        {
            // 가장 가까운 몬스터 방향 벡터 계산 (목적지 - 출발지)
            _moveDirection = (targetEnemy.position - transform.position).normalized;
        }
        else
        {
            // 주변에 적이 없다면 매니저가 넘겨준 플레이어가 바라보는 방향 사용
            _moveDirection = direction.normalized;
        }

        // 3. 만약 어떤 방향도 구하지 못했다면 (플레이어가 멈춰있고 적도 없다면) 우측 발사 방어코드
        if (_moveDirection == Vector3.zero)
        {
            _moveDirection = Vector3.right;
        }

        // 4. 날아갈 방향에 맞게 투사체 회전 세팅
        float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(DestroySkillAfterDelay());
    }

    // 생성, 물리 관련 ==============================================================

    private void OnDisable()
    {
        _onSkillCollision = null;
    }

    void Update()
    {
        this.transform.position += _moveDirection * _skillMoveSpeed * Time.deltaTime;   // 이동방향으로 나가는 스킬
    }

    IEnumerator DestroySkillAfterDelay()
    {
        yield return new WaitForSeconds(_skillDurationTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckCollision(collision.collider);
    }

    private void CheckCollision(Collider2D collision)
    {
        bool isOwnerPlayer = (_ownerInstanceId == 0);

        // 투사체가 충돌한 오브젝트의 Tag가 플레이어라면?
        if (collision.CompareTag("Player") && (isOwnerPlayer == false))
        {
            _onSkillCollision?.Invoke(0, _damage);

            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("Enemy") && (isOwnerPlayer))
        {
            var info = new SkillCollisionInfo(_skillDataId, collision);
            _collisionCallback.Invoke(info);

            Destroy(this.gameObject);
        }
    }








    // 기즈모 ============================================
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, _scanRadius);
    }
}
