using System;
using UnityEngine;

public interface ISkillObject
{
    float GetSkillCoolTime();

    void InitSkillObject(int ownerInstanceId, Vector3 direction, string targetTag, Action<int, int> collisionCallback);
}
