using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CurrentMagicSlotUI : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_SkillName;
    [SerializeField] private Text Text_Dmg;
    [SerializeField] private Text Text_CoolTime;
    [SerializeField] private Transform Transform_LevelSlotRoot;

    [Header("레벨 이미지 관련")]
    [SerializeField] private Image[] Image_LevelStars;
    [SerializeField] private Sprite Sprite_LevelStarOn;
    [SerializeField] private Sprite Sprite_LevelStarOff;
    [SerializeField] private Sprite Sprite_MaxLevel;

    private string _slotDataId;

    private Player2D _localPlayer;

    public string GetSlotDataId()
    {
        return _slotDataId;
    }

    public void InitSlot(string skillDataId)
    {
        _slotDataId = skillDataId;
        var skillData = DaniTechGameDataManager.Instance.GetSkill(skillDataId);
        if (skillDataId == null) return;

        int currentLevel = DaniTechGameObjectManager.Inst.GetSkillLevel(skillDataId);
        float currentSkillDamage = skillData.SkillDamage + (skillData.DamagePerLevel * (currentLevel - 1));
        float currentSkillCoolTime = DaniTechGameObjectManager.Inst.GetCurrentSkillCoolTime(skillDataId);
        if (currentSkillCoolTime == 0) { currentSkillCoolTime = skillData.SkillCoolTime; }
        string masterSkill = "";
        if (currentLevel >= 15)
        {
            masterSkill = "각성 ";
        }
        Text_SkillName.text = $"{masterSkill}{skillData.Name}";
        Text_Dmg.text = $"데미지: {currentSkillDamage}";
        Text_CoolTime.text = $"쿨타임: {currentSkillCoolTime}";

        string iconPath = skillData.IconPath;
        if (string.IsNullOrEmpty(iconPath) == true) return;

        DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconPath).Forget();


        _slotDataId = skillDataId;
        ShowSkillLevelToImg();
    }

    private void ShowSkillLevelToImg()
    {
        if (string.IsNullOrEmpty(_slotDataId)) return;

        int skillLevel = DaniTechGameObjectManager.Inst.GetSkillLevel(_slotDataId);

        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();
        int maxLevel = _localPlayer.GetMaxLevel();

        if (skillLevel >= 15)
        {
            for (int i = 0; i < Image_LevelStars.Length; i++)
            {
                Image_LevelStars[i].sprite = Sprite_MaxLevel;
                Image_LevelStars[i].color = Color.red;
            }
            return;
        }
        


        for (int i = 0; i < Image_LevelStars.Length; i++)
        {
            if (i < skillLevel)
            {
                Image_LevelStars[i].sprite = Sprite_LevelStarOn;
            }
            
            else
            {
                Image_LevelStars[i].sprite = Sprite_LevelStarOff;
            }
        }
    }
}
