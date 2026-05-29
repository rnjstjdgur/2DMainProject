using System;
using UnityEngine;

public struct SkillCollisionInfo
{
    public string SkillDataId;
    public Collider2D TargetCollider;

    public SkillCollisionInfo(string skillId, Collider2D targetCollider)
    {
        SkillDataId = skillId;
        TargetCollider = targetCollider;
    }
}

public interface ISkillObject
{
    float GetSkillCoolTime();

    void InitSkillObject(int ownerInstanceId, Vector3 direction, string targetTag, Action<int, int> collisionCallback);
}
