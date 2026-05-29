using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SkillCircle : MonoBehaviour, ISkillObject
{
    [Header("스킬 고유 ID")]
    [SerializeField] private string _skillDataId = "skill_fire_01";

    [Header("스킬 기본 설정")]
    [SerializeField] private float _overlapRadius = 1.2f;
    [SerializeField] private Vector2 _offsetPosition = new Vector2(1.5f, 0f); // 플레이어 앞 여백

    private float _skillCoolTime;
    private float _skillDuration;
    private float _skillDamageInterval;
    private int _ownerInstanceId;
    private Vector3 _skillDirection;
    private Action<SkillCollisionInfo> _collisionCallback;

    private Player2D _localPlayer;

    // 인터페이스 멤버 ============================================
    public float GetSkillCoolTime()
    {
        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        if (skillData == null) return 1.0f;
        return skillData.SkillCoolTime;
    }

    public void InitSkillObject(int ownerInstanceId, Vector3 direction, string targetTag, Action<SkillCollisionInfo> collisionCallback)
    {
        _ownerInstanceId = ownerInstanceId;
        _skillDirection = direction;
        _collisionCallback = collisionCallback;

        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        if (skillData != null)
        {
            _skillCoolTime = skillData.SkillCoolTime;
            _skillDuration = skillData.SkillDuration;
            _skillDamageInterval = skillData.SkillDamageInterval;
            Debug.Log($"[SkillCircle] '{_skillDataId}' 데이터 연동 완료! (기획 데이터 쿨타임: {_skillCoolTime}s)");
        }
        else
        {
            _skillCoolTime = 3.0f;
            _skillDuration = 1.5f;
            _skillDamageInterval = 0.3f;
            Debug.LogWarning($"[SkillCircle] '{_skillDataId}' 데이터를 찾지 못해 기본 쿨타임으로 작동합니다.");
        }

        StartCoroutine(CoSkillDurationRoutine());
    }

    private IEnumerator CoSkillDurationRoutine()
    {
        float elapsed = 0f;

        // 다단히트 시 무한 연타로 몬스터가 1프레임만에 녹는 것을 방지하기 위한 타격 기록 필터
        Dictionary<GameObject, float> targetHitHistory = new Dictionary<GameObject, float>();

        var playerTarget = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        // 기획된 지속시간(_duration) 동안 루프 가동
        while (elapsed < _skillDuration)
        {
            _skillDirection = _localPlayer.GetLookDirection();
            Vector2 lookDir = new Vector2(_skillDirection.x, _skillDirection.y).normalized;
            Vector2 rightOffset = lookDir * _offsetPosition.x;
            Vector2 upOffset = new Vector2(-lookDir.y, lookDir.x) * _offsetPosition.y;

            // 부모(플레이어)의 위치를 따라 실시간 추적하며 중앙 좌표 갱신
            Vector2 originPos = playerTarget != null ? (Vector2)playerTarget.transform.position : (Vector2)transform.position;
            Vector2 center = (Vector2)transform.position + rightOffset + upOffset;

            if (playerTarget != null)
            {
                transform.position = playerTarget.transform.position;
            }

            // 지정한 범위 내 레이아웃의 모든 콜라이더 수집
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, _overlapRadius);

            foreach (Collider2D collision in hitColliders)
            {
                if (collision.CompareTag("Player")) continue; // 자기자신 혹은 플레이어 제외

                GameObject targetObj = collision.gameObject;

                // 타격 제한 시간 검사 (마지막 대미지 기록 후 주기 계산)
                if (targetHitHistory.TryGetValue(targetObj, out float lastHitTime))
                {
                    if (Time.time < lastHitTime + _skillDamageInterval) continue; // 쿨타임 안 지났으면 스킵
                }

                if (_collisionCallback != null)
                {
                    SkillCollisionInfo info = new SkillCollisionInfo(_skillDataId, collision);
                    _collisionCallback.Invoke(info);
                }

                // 타격 시간 갱신
                targetHitHistory[targetObj] = Time.time;
            }

            elapsed += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 지속시간이 다 끝났으므로 스스로 파괴(꺼짐)
        Destroy(this.gameObject);
    }

    // 범위 체크용 기즈모 에디터 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 currentDir = _localPlayer != null ? _localPlayer.GetLookDirection() : _skillDirection;
        Vector2 lookDir = currentDir == Vector3.zero ? Vector2.right : (Vector2)currentDir.normalized;

        Vector2 rightOffset = lookDir * _offsetPosition.x;
        Vector2 upOffset = new Vector2(-lookDir.y, lookDir.x) * _offsetPosition.y;

        Vector3 basePos = _localPlayer != null ? _localPlayer.transform.position : transform.position;
        Vector3 center = basePos + (Vector3)(rightOffset + upOffset);

        Gizmos.DrawWireSphere(center, _overlapRadius);
    }
}
