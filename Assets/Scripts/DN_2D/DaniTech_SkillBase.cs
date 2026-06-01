using UnityEngine;

public class DaniTech_SkillBase : MonoBehaviour
{
    protected float CalculateCoolTime(string skillDataId, float baseCool, float coolDownPerLevel)
    {
        int currentLevel = DaniTechGameObjectManager.Inst.GetSkillLevel(skillDataId);
        if (currentLevel < 1) currentLevel = 1;

        float reducedCool = baseCool - (coolDownPerLevel * (currentLevel - 1));

        return Mathf.Max(0.5f, reducedCool);
    }
}
