using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SkillRayCast : DaniTech_SkillBase, ISkillObject
{
    [Header("스킬 고유 ID")]
    [SerializeField] private string _skillDataId = "skill_lightning_01";

    [Header("번개 설정")]
    [SerializeField] private AssetReference _lightningEffectRef;
    [SerializeField] private float _boltLifeTime = 0.1f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private int _segments = 10;

    private float _skillCoolTime;
    private float _skillDuration;
    private float _skillDamageInterval;
    private float _skillDistance;
    private float _skillDamage;

    private int _ownerInstanceId;
    private Action<SkillCollisionInfo> _collisionCallback;

    // 인터페이스 멤버 =====================================================

    public float GetSkillCoolTime()
    {
        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        if (skillData == null) return 1.0f;
        // 부모(DaniTech_SkillBase)가 제공하는 CalculateCoolTime 메서드 사용
        return CalculateCoolTime(_skillDataId, skillData.SkillCoolTime, skillData.CoolDownPerLevel);
    }

    public void InitSkillObject(int ownerInstanceId, Vector3 direction, string targetTag, Action<SkillCollisionInfo> collisionCallback)
    {
        _collisionCallback = collisionCallback;

        // 1. 데이터 로드
        DNSkillData skillData = DaniTechGameDataManager.Instance.GetSkill(_skillDataId);
        int currentLevel = DaniTechGameObjectManager.Inst.GetSkillLevel(_skillDataId);
        if (currentLevel < 1) currentLevel = 1;
        if (skillData != null)
        {
            _ownerInstanceId = ownerInstanceId;
            _skillDistance = skillData.SkillDistance;
            _skillDuration = skillData.SkillDuration;
            _skillDamage = skillData.SkillDamage;
            _skillCoolTime = skillData.SkillCoolTime;
            _skillDamageInterval = skillData.SkillDamageInterval;
        }

        int boltCount = Mathf.Min(currentLevel + 1, 7);

        for (int i = 0; i < boltCount; i++)
        {
            Vector2 shootDir;

            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            shootDir = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;

            Vector3 endPos = this.transform.position + (Vector3)shootDir * _skillDistance;

            Vector2 boxSize = new Vector2(1f, _skillDistance);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(this.transform.position, boxSize, Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg, shootDir, 0f, _enemyLayer);
            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    _collisionCallback?.Invoke(new SkillCollisionInfo(_skillDataId, hit.collider));
                }
            }

            // 4. 각 번개마다 이펙트 생성
            _lightningEffectRef.InstantiateAsync(this.transform.position, Quaternion.identity).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    LightningEffect effect = handle.Result.GetComponent<LightningEffect>();
                    effect.Play(this.transform.position, endPos, _segments, _boltLifeTime, null, _skillDataId);
                }
            };
        }

        Destroy(gameObject, 0.5f);
    }
}
