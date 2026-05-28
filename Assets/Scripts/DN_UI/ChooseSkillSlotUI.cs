using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ChooseSkillSlotUI : MonoBehaviour
{
    [SerializeField] private Text Text_SkillLevel;
    [SerializeField] private DaniTechUIButton Button_Slot;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Image Image_Frame;

    private event Action<int> OnSelectEvent;

    public int SlotInstanceId { get; private set; }

    private void OnEnable()
    {
        Button_Slot.BindOnClickButtonEvent(OnClick_SelectItem);
    }

    public void SetIcon(string skillDataId, int skillLevel)
    {
        var skillData = DaniTechGameDataManager.Instance.GetSkill(skillDataId);
        if (skillData == null)
        {
            Debug.LogWarning($"skill 데이터를 불러올 수 없습니다! 경로:{skillDataId}");
            return;
        }

        string iconPath = skillData.IconPath;
        if (string.IsNullOrEmpty(iconPath) == true)
        {
            Debug.LogWarning($"Item 데이터에 아이콘 경로가 존재하지 않습니다.");
            return;
        }

        // + Addressable을 적용하면서 비동기로 바뀌었다
        //DaniTechResourceManager.Inst.LoadSprite(iconPath, (sprite) => {
        //    Image_Icon.sprite = sprite;
        //});

        DaniTechGameUtil.LoadAndSetSpriteImage(Image_Icon, iconPath).Forget();

        //var sprite = GameUtil.LoadSpriteCanBeNull(iconPath);
        //if(sprite == null)
        //{
        //    Debug.LogWarning($"Sprite를 불러올 수 없습니다! 경로:{iconPath}");
        //    return;
        //}
        //Image_Icon.sprite = sprite;

        Text_SkillLevel.text = $"{skillLevel}";
    }

    private void OnDisable()
    {
        OnSelectEvent = null;
    }

    public void InitSlot(int slotInstanceId, string skillDataId, int skillLevel)
    {
        SlotInstanceId = slotInstanceId;
        SetIcon(skillDataId, skillLevel);
        // Text_StackCount.text = slotInstanceId.ToString();
    }

    public void OnClick_SelectItem()
    {
        // 부모한테 알려주자
        OnSelectEvent?.Invoke(SlotInstanceId);


        Debug.Log($"{SlotInstanceId}눌러졌다");
        // 나중에 툴팁, 팝업 다 여기서 띄워주면 된다
    }

    public void BindSlotSelectEvent(Action<int> onSelectEvent)
    {
        // 얘는 부모 하나만 콜백이벤트 등록하면 된다.
        OnSelectEvent = onSelectEvent;
    }
}
