using UnityEngine;
using UnityEngine.UI;

public class HudSlotUI : MonoBehaviour
{
    [SerializeField] private int SlotOffsetY;

    [SerializeField] private Slider Slider_Hp;
    [SerializeField] private Slider Slider_Mp;


    private int _instanceId;

    // 참조형을 기록(캐싱)
    private Transform _targetTransform;

    public void InitSlot(int instanceId, Transform targetTransform)
    {
        _instanceId = instanceId;
        _targetTransform = targetTransform;
        SlotOffsetY = 250;

        TryBindStatChangedEvent(targetTransform.gameObject);
    }

    private void TryBindStatChangedEvent(GameObject gObj)
    {
        // gObj가 몬스터거나, 플레이어라면 GetComponent를 시도해보고, 잘 되면 그곳에 있는 이벤트를 구독하자!
        var player = gObj.GetComponent<Player2D>();
        if (player != null)
        {
            player.BindOnStatChangedEvent(OnTargetEntityHpChanged, OnTargetEntityMpChanged);
            return;
        }

        var monster = gObj.GetComponent<Monster2D>();
        if (monster != null)
        {
            monster.BindOnStatChangedEvent(OnTargetEntityHpChanged, OnTargetEntityMpChanged);
            return;
        }
    }
    private void OnTargetEntityHpChanged(int curHp, int maxHp)
    {
        Slider_Hp.value = (curHp / (float)maxHp);
    }

    private void OnTargetEntityMpChanged(int curMp, int maxMp)
    {
        Slider_Mp.value = (curMp / (float)maxMp);
    }

    private void Update()
    {
        if (_targetTransform != null)
        {
            //this.gameObject.transform.position = _targetTransform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(_targetTransform.position);

            var rectTransform = this.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 finalScreenPos = new Vector2(screenPos.x, screenPos.y - SlotOffsetY);
                rectTransform.anchoredPosition = finalScreenPos;
            }
        }
    }
}
