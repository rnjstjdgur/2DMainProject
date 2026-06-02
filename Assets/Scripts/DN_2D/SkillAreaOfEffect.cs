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
        return CalculateCoolTime(_skillDataId, _skillCoolTime, skillData.CoolDownPerLevel);
    }

    public void InitSkillObject(int ownerInstanceId, Vector3 direction, string targetTag, Action<SkillCollisionInfo> collisionCallback)
    {
        _collisionCallback = collisionCallback;
        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        if (skillData != null)
        {
            _skillCoolTime = skillData.SkillCoolTime;
            _skillDuration = skillData.SkillDuration;

            //string animPath = skillData.AnimControllerPath;
            //if (!string.IsNullOrEmpty(animPath))
            //{
            //    LoadAnimatorAddressable(animPath);
            //}

            //Debug.Log($"[SkillCircle] '{_skillDataId}' 데이터 연동 완료!");
        }
        else
        {
            _skillDuration = 1f;
            Debug.LogWarning($"[SkillCircle] 데이터를 찾지 못해 기본값으로 작동합니다.");
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
            var info = new SkillCollisionInfo(_skillDataId, collision);
            _collisionCallback.Invoke(info);

            Destroy(this.gameObject);
        }
    }



    // 코루틴 ===========================================================

    private IEnumerator CoSkillLifecycleRoutine()
    {
        yield return new WaitForSeconds(_skillDuration);

        // 지속시간 종료 시 삭제
        Destroy(this.gameObject);
    }
}
