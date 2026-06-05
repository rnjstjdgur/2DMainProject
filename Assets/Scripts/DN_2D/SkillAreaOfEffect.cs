using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SkillAreaOfEffect : DaniTech_SkillBase, ISkillObject
{
    [Header("스킬 고유 ID")]
    [SerializeField] private string _skillDataId = "skill_ice_01";

    [Header("스킬 기본 설정")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CircleCollider2D _circleCollider;

    private float _skillCoolTime;
    private float _skillDuration;

    private Action<SkillCollisionInfo> _collisionCallback;

    private HashSet<int> _hitEnemyIds = new HashSet<int>();

    private Player2D _localPlayer;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _circleCollider = GetComponentInChildren<CircleCollider2D>();
    }

    // 인터페이스 멤버 ============================================

    public float GetSkillCoolTime()
    {
        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        if (skillData == null) return 1.0f;
        return CalculateCoolTime(_skillDataId, skillData.SkillCoolTime, skillData.CoolDownPerLevel);
    }

    public void InitSkillObject(int ownerInstanceId, Vector3 direction, string targetTag, Action<SkillCollisionInfo> collisionCallback)
    {
        this.transform.position = this.transform.position + new Vector3(0, -1f, 0);

        _collisionCallback = collisionCallback;
        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        int currentLevel = DaniTechGameObjectManager.Inst.GetSkillLevel(_skillDataId);
        bool isSkillMaxLevel = (currentLevel >= 15);
        if (currentLevel < 1) currentLevel = 1;

        if (skillData != null)
        {
            _skillCoolTime = skillData.SkillCoolTime;
            _skillDuration = skillData.SkillDuration/* + ((currentLevel - 1) * skillData.SkillDurationPerLevel)*/;  // 일단 바로 시전되는 스킬로 하고 나중에 지속시간 추가
        }

        if (isSkillMaxLevel)
        {
            ApplyAwakeningEffect();
        }

        else
        {
            Debug.LogWarning($"[SkillCircle] 데이터를 찾지 못했습니다.");
        }

        // 스킬 지속시간 및 방향 동기화를 제어하는 코루틴 시작
        StartCoroutine(CoSkillLifecycleRoutine());
    }

    // 스킬 기믹 관련 ===================================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }

    private void CheckCollision(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int enemyId = collision.GetInstanceID();

            // 아직 타격하지 않은 몬스터라면 데미지 + 빙결 효과 부여
            if (!_hitEnemyIds.Contains(enemyId))
            {
                _hitEnemyIds.Add(enemyId);

                // 1. 데미지 전달
                _collisionCallback?.Invoke(new SkillCollisionInfo(_skillDataId, collision));

                // 2. 빙결 로직 (몬스터 컴포넌트에 접근)
                var enemy = collision.GetComponent<Monster2D>();
                if (enemy != null)
                {
                    enemy.ApplyFreezeEffect(_skillDuration);
                }
            }
        }
    }

    private void ApplyAwakeningEffect()
    {
        var collider = GetComponentInChildren<CircleCollider2D>();
        if (collider != null) collider.radius *= 1.5f;

        var sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.transform.localScale *= 1.5f;
            sprite.color = Color.red;
        }

        _skillCoolTime
    }



    // 코루틴 ===========================================================

    private IEnumerator CoSkillLifecycleRoutine()
    {
        yield return new WaitForSeconds(_skillDuration);

        // 지속시간 종료 시 삭제
        Destroy(this.gameObject);
    }
}
