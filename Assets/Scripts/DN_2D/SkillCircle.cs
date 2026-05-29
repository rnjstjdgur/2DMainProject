using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SkillCircle : MonoBehaviour, ISkillObject
{
    [Header("스킬 고유 ID")]
    [SerializeField] private string _skillDataId = "skill_fire_01";

    [Header("스킬 기본 설정")]

    private float _skillCoolTime;
    private float _skillDuration;
    private float _skillDamageInterval;
    private int _ownerInstanceId;
    private Vector3 _skillDirection;
    private Action<SkillCollisionInfo> _collisionCallback;

    private Player2D _localPlayer;
    private Animator _animator;
    private BoxCollider2D _boxCollider;

    private Dictionary<GameObject, float> _targetHitHistory = new Dictionary<GameObject, float>();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>();

        // 물리 연산을 트리거 형태로 사용하기 위해 설정 강제
        _boxCollider.isTrigger = true;
    }

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

        // 히트 기록 초기화
        _targetHitHistory.Clear();

        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        if (skillData != null)
        {
            _skillCoolTime = skillData.SkillCoolTime;
            _skillDuration = skillData.SkillDuration;
            _skillDamageInterval = skillData.SkillDamageInterval;

            // [데이터 드리븐] 애니메이터 경로를 받아와서 동적 로드 및 변경
            // 예시: skillData.AnimatorPath 변수명이 다를 경우 해당 변수명으로 변경해주세요.
            string animPath = skillData.AnimControllerPath;
            if (!string.IsNullOrEmpty(animPath))
            {
                RuntimeAnimatorController newController = Resources.Load<RuntimeAnimatorController>(animPath);
                if (newController != null)
                {
                    _animator.runtimeAnimatorController = newController;
                    Debug.Log($"[SkillCircle] 애니메이터 변경 완료: {animPath}");
                }
                else
                {
                    Debug.LogError($"[SkillCircle] Resources 폴더에서 애니메이터를 찾을 수 없습니다: {animPath}");
                }
            }

            Debug.Log($"[SkillCircle] '{_skillDataId}' 데이터 연동 완료!");
        }
        else
        {
            _skillCoolTime = 3.0f;
            _skillDuration = 1.5f;
            _skillDamageInterval = 0.3f;
            Debug.LogWarning($"[SkillCircle] 데이터를 찾지 못해 기본값으로 작동합니다.");
        }

        // 스킬 지속시간 및 방향 동기화를 제어하는 코루틴 시작
        StartCoroutine(CoSkillLifecycleRoutine());
    }

    // ==============================================================



    private IEnumerator CoSkillLifecycleRoutine()
    {
        float elapsed = 0f;

        while (elapsed < _skillDuration)
        {
            if (_localPlayer != null)
            {
                // 플레이어 위치 실시간 추적
                transform.position = _localPlayer.transform.position;
                _skillDirection = _localPlayer.GetLookDirection();

                // 플레이어가 바라보는 방향에 맞춰 스킬 오브젝트(콜라이더+이펙트) 전체를 회전시킴
                Vector2 lookDir = new Vector2(_skillDirection.x, _skillDirection.y).normalized;
                if (lookDir != Vector2.zero)
                {
                    float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 지속시간 종료 시 삭제
        Destroy(this.gameObject);
    }

    // ★ 핵심: 콜라이더 범위 안에 적이 들어와 있는 동안 매 프레임/물리 주기마다 체크
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) return; // 플레이어 제외

        GameObject targetObj = collision.gameObject;

        // 타격 제한 시간(인터벌) 검사
        if (_targetHitHistory.TryGetValue(targetObj, out float lastHitTime))
        {
            if (Time.time < lastHitTime + _skillDamageInterval) return; // 주기 안 지났으면 대미지 안 줌
        }

        // 데미지 콜백 실행
        if (_collisionCallback != null)
        {
            SkillCollisionInfo info = new SkillCollisionInfo(_skillDataId, collision);
            _collisionCallback.Invoke(info);
        }

        // 타격 시간 갱신
        _targetHitHistory[targetObj] = Time.time;
    }
}
