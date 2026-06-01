using UnityEngine;
using UnityEngine.UI;

public class MPHudSlotUI : MonoBehaviour
{
    [SerializeField] private Slider Slider_Mp;

    private Player2D _localPlayer;

    public void InitSlot(GameObject playerObj)
    {
        var player = playerObj.GetComponent<Player2D>();
        if (player != null)
        {
            // HP는 안 받고 MP 변경 이벤트만 구독
            player.BindOnStatChangedEvent(null, OnTargetEntityMpChanged);

            int currentExp = player.GetPlayerMp();
            int maxExp = player.GetRequiredExpForCurrentLevel();

            Debug.Log($"[UI 초기화] 시작 값 반영 - 현재: {currentExp}, 최대: {maxExp}");
            UpdateMpSlider(currentExp, maxExp);
        }
    }

    private void OnTargetEntityMpChanged(int curMp, int maxMp)
    {
        UpdateMpSlider(curMp, maxMp);
    }

    private void UpdateMpSlider(int curMp, int maxMp)
    {
        if (Slider_Mp == null) return;
        if (maxMp <= 0) return;

        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();
        float ratio = (curMp / (float)maxMp);

        if (_localPlayer.GetPlayerLevel() == 15)
        {
            ratio = 1;
        }
        Slider_Mp.value = ratio;
    }
}
