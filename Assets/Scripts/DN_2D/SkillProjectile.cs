using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillProjectile : DaniTech_SkillBase
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer_Effect;

    [Header("스킬 이동 관련")]
    [SerializeField] private float _skillMoveSpeed = 10.0f;  // [ToDo] 나중에 데이터로 받아와서 스킬의 속도를 대입하자
    [SerializeField] private float _scanRadius = 20.0f; // 몬스터를 탐색할 범위 (반지름)
    [SerializeField] private LayerMask _enemyLayer;

    [Header("스킬 전투 관련")]
    [SerializeField] private float _skillDistance = 10.0f;  // 스킬범위

    private int _damage = 100;

    private int _ownerInstanceId;

    private Vector3 _fireDirection;
    //private Vector3 _moveDirection = Vector3.right;   // 이동방향으로 나가는 스킬
    private float _skillDurationTime = 3.0f;    // [ToDo] 나중에 데이터로 받아와서 스킬의 유지시간을 대입하자 (스킬이 강화되면 유지시간이 늘어나는식)


    private event Action<int, int> _onSkillCollision;

    private void OnDisable()
    {
        _onSkillCollision = null;
    }

    public void InitSkillObject(int ownerInstanceId, Vector3 launchDirection, string parentTag, Action<int, int> onSkillCollision = null)
    {
        //_moveDirection = launchDirection.normalized;   // 이동방향으로 나가는 스킬

        // [ToDo] 데이터로 스킬 스프라이트를 받아와서 스킬별로 다른 이미지가 생성됨
        //var skillData = DaniTechGameDataManager.Instance.GetSkill();
        //Sprite skillSprite = DaniTechResourceManager.Inst.LoadSprite(skillData.spritePath);
        //spriteRenderer_Effect.sprite = skillSprite;

        _ownerInstanceId = ownerInstanceId;
        Transform targetEnemy = GetClosestEnemy();

        if (targetEnemy != null)
        {
            // 가장 가까운 몬스터가 있으면 ➡️ 몬스터 방향 벡터 계산 (목적지 - 출발지)
            _fireDirection = (targetEnemy.position - transform.position).normalized;
        }
        else
        {
            // 주변에 몬스터가 한 마리도 없다면 ➡️ 기본적으로 플레이어가 바라보는 방향(혹은 기본 앞방향)으로 발사
            _fireDirection = transform.right;
        }

        this.gameObject.tag = parentTag;

        _onSkillCollision = onSkillCollision;

        float angle = Mathf.Atan2(_fireDirection.y, _fireDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        //float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;   // 이동방향으로 나가는 스킬
        //transform.rotation = Quaternion.Euler(0, 0, angle);
        StartCoroutine(DestroySkillAfterDelay());
    }

    void Update()
    {
        transform.position += _fireDirection * _skillMoveSpeed * Time.deltaTime;   // 이동방향으로 나가는 스킬
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
            var gObj = collision.gameObject;
            if (gObj == null) return;

            var monsterComponent = gObj.GetComponent<Monster2D>();
            if (monsterComponent == null) return;

            monsterComponent.TakeDamage(_damage);

            int instId = monsterComponent.GetMonsterInstanceId();

            Destroy(this.gameObject);
        }
    }


    // 스킬 이동 관련 ============================================
    private Transform GetClosestEnemy()
    {
        // 1. 플레이어 주변의 일정 반경 내에 있는 모든 몬스터의 Collider를 가져옵니다.
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, _scanRadius, _enemyLayer);

        Transform closestEnemy = null;
        float minDistance = _skillDistance;

        // 2. 찾은 몬스터들을 하나씩 순회하며 거리를 비교합니다.
        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }





    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }
}
