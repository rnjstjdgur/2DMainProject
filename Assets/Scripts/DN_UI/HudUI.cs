using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class HudUI : DaniTechUIBase
{
    [SerializeField] private GameObject Prefab_HudSlot;
    [SerializeField] private GameObject Prefab_MPHudSlot;
    [SerializeField] private Transform Transform_SlotRoot;

    private Dictionary<int, List<GameObject>> _hudSlotMap = new Dictionary<int, List<GameObject>>();

    public void AddHudSlot(int instanceId, Transform targetTransform)
    {
        CreateHudSlot(instanceId, targetTransform);
        
    }

    private void CreateHudSlot(int instanceId, Transform targetTransform)
    {
        //foreach (var prefab in Prefab_List)
        //{
        //    var gObj = Instantiate(prefab, Transform_SlotRoot);
        //    if (gObj == null) return;

        //    var slotComponent = gObj.GetComponent<HudSlotUI>();
        //    if (slotComponent == null) return;

        //    slotComponent.InitSlot(instanceId, targetTransform);
        //    _hudSlotList.Add(instanceId, slotComponent);
        //}
        if (targetTransform == null) return;

        // 해당 ID의 리스트 보관함이 없다면 새로 생성
        if (!_hudSlotMap.ContainsKey(instanceId))
        {
            _hudSlotMap[instanceId] = new List<GameObject>();
        }

        // 1. 캐릭터 추적용 HP 슬롯 생성
        var hpObj = Instantiate(Prefab_HudSlot, Transform_SlotRoot);
        if (hpObj != null)
        {
            var hpSlot = hpObj.GetComponent<HudSlotUI>();
            if (hpSlot != null) hpSlot.InitSlot(instanceId, targetTransform);

            _hudSlotMap[instanceId].Add(hpObj); // 관리 리스트에 추가
        }

        // 2. 플레이어일 때만 화면 고정용 MP 슬롯 생성 (몬스터는 MP바가 필요 없으므로 체크)
        var playerCheck = targetTransform.GetComponent<Player2D>();
        if (playerCheck != null)
        {
            var mpObj = Instantiate(Prefab_MPHudSlot, Transform_SlotRoot);
            if (mpObj != null)
            {
                var mpSlot = mpObj.GetComponent<MPHudSlotUI>();
                if (mpSlot != null) mpSlot.InitSlot(targetTransform.gameObject);

                // 💡 팁: MP바의 화면 고정 위치(Canvas 기준)를 여기서 잡아주거나 프리팹 자체의 앵커를 세팅해두면 됩니다.
                var rect = mpObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // 예: 화면 정중앙 상단에 고정
                    rect.anchoredPosition = new Vector2(0, -50);
                }

                _hudSlotMap[instanceId].Add(mpObj); // 관리 리스트에 추가
            }
        }
    }

    public void RemoveHudSlot(int instanceId)
    {
        if (_hudSlotMap.TryGetValue(instanceId, out List<GameObject> createdObjects))
        {
            foreach (var gObj in createdObjects)
            {
                if (gObj != null)
                {
                    Destroy(gObj);
                }
            }

            // 3. 찌꺼기가 남지 않도록 딕셔너리에서도 완전히 키값을 제거
            _hudSlotMap.Remove(instanceId);

            Debug.Log($"[HudUI] {instanceId}번 HUD 슬롯 파괴 완료 (오브젝트 및 맵 데이터 제거)");
        }
    }
}
