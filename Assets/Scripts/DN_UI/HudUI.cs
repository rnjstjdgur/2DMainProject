using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class HudUI : DaniTechUIBase
{
    [SerializeField] private GameObject Prefab_HudSlot;
    [SerializeField] private Transform Transform_SlotRoot;

    private Dictionary<int, HudSlotUI> _hudSlotList = new Dictionary<int, HudSlotUI>();

    public void AddHudSlot(int instanceId)
    {
        CreateHudSlot(instanceId);
    }

    private void CreateHudSlot(int instanceId)
    {
        var gObj = Instantiate(Prefab_HudSlot, Transform_SlotRoot);
        if (gObj == null) return;

        var slotComponent = gObj.GetComponent<HudSlotUI>();
        if (slotComponent == null) return;

        //slotComponent.InitSlot(dataId, onClickChildSlotSelected);
        _hudSlotList.Add(instanceId, slotComponent);
    }

    public void RemoveHudSlot()
    {

    }
}
