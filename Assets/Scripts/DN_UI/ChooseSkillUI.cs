using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChooseSkillUI : DaniTechUIBase
{
    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private Transform Transform_UISlotRoot;
    [SerializeField] private DaniTechUIButton Button_CloseSelfAllArea;

    private int _generatedKey = 0;
    private Dictionary<int, ChooseSkillSlotUI> _skillSlotList = new Dictionary<int, ChooseSkillSlotUI>();
    private Dictionary<int, string> _skillDataIdList = new Dictionary<int, string>();
    private Dictionary<string, int> _selectedSkillList = new Dictionary<string, int>();
    private void OnEnable()
    {
        TimeManager.instance.TimeStop();
        Button_CloseSelfAllArea.BindOnClickButtonEvent(OnClick_ClosePopup);
        SetSkillSlotOnEnable();
    }

    private void SetSkillSlotOnEnable()
    {
        // 슬롯 정리 - 혹시 오픈 시점에 다른 슬롯들이 있다면 제거하자
        if (_skillSlotList.Count > 0)
        {
            foreach (Transform child in Transform_UISlotRoot)
            {
                Destroy(child.gameObject);
            }
            _skillSlotList.Clear();
        }

        var myHero = DaniTechGameDataManager.Instance.GetCharacterData("character_basic_01");
        if (myHero == null) return;


        // 스킬 정보가 있다면
        if (myHero.SkillList != string.Empty)
        {
            string[] fullSkillList = myHero.SkillList.Split(',');

            var randomThreeSkills = fullSkillList.OrderBy(x => Random.value).Take(3);

            foreach (string skillName in randomThreeSkills)
            {
                var skillData = DaniTechGameDataManager.Instance.GetSkill(skillName);
                if (skillData != null)
                {
                    CreateSlot(skillData.Id, skillData.Name);
                }
            }
        }
    }

    public void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.ChooseSkillUI);
        TimeManager.instance.TimeStart();
    }

    private void CreateSlot(string skillDataId, string skillName)
    {
        // 1-1 수동 SetParant가 뒤에 지금은 자동으로 해주고 있다
        var gObj = Instantiate(Prefab_Slot, Transform_UISlotRoot);
        if (gObj == null) return;

        // 1-2 자식 슬롯의 컴포넌트를 가져온다 -> 위에 게임오브젝트는 스크립트가 아직 아니므로
        var slotComponent = gObj.GetComponent<ChooseSkillSlotUI>();
        if (slotComponent == null) return;

        _generatedKey++;

        // 1-3 여기서 slotComponent가지고 뭔가를 하는 겁니다!
        slotComponent.InitSlot(_generatedKey, skillDataId, skillName);
        slotComponent.gameObject.name = $"SkillSlot : {slotComponent.SlotInstanceId}";

        // 1-4 중복체크 해주면 좋긴 하지만, 일단 쉽게 컴포넌트(컴포넌트로 게임오브젝트는 받을 수 있으므로)를 보관해보자
        _skillSlotList.Add(slotComponent.SlotInstanceId, slotComponent);
        _skillDataIdList.Add(slotComponent.SlotInstanceId, skillDataId);

        slotComponent.BindSlotSelectEvent(OnChildSlotSelected);
    }


    private void OnChildSlotSelected(int selectedSlotInstanceId)
    {
        if (_skillDataIdList.TryGetValue(selectedSlotInstanceId, out var selectedSkillDataId))
        {
            string skillDataId = selectedSkillDataId;

            var skillData = DaniTechGameDataManager.Instance.GetSkill(skillDataId);
            if (skillData == null)
            {
                Debug.LogWarning($"{skillDataId}의 스킬 데이터가 없습니다.");
            }

            if (_selectedSkillList.ContainsKey(skillDataId))
            {
                _selectedSkillList[skillDataId]++;
                skillData.SkillLevel = _selectedSkillList[skillDataId];
            }
            else
            {
                _selectedSkillList.Add(skillDataId, 1);
            }
        }
        OnClick_ClosePopup();
    }
}
