using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaniTech_2DFieldObject : MonoBehaviour
{
    [SerializeField] private int _fieldObjectInstanceId;
    [SerializeField] private string _fieldObjectDataId;
    [SerializeField] private string _fieldObjectName;

    private static int _manaEvent = 1;

    public void InitFieldObjectInfoOnCreated(int instanceId, string fieldObjectDataId)
    {
        var fieldObjectData = DaniTechGameDataManager.Instance.GetDNFieldObjectData(fieldObjectDataId);
        if (fieldObjectData == null) 
        {
            Debug.LogWarning($"유효하지 않은 필드 오브젝트 데이터 입니다! {fieldObjectDataId}");
            return;
        }

        _fieldObjectInstanceId = instanceId;
        _fieldObjectDataId = fieldObjectDataId;
    }

    public string GetFieldObjectDataId()
    {
        return _fieldObjectDataId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        if(collision.CompareTag("Player") == true)
        {
            // 플레이어와 충돌했을때 이제 GameManager에 아이템을 저장해준다던가 등등 처리
            // 필요에 따라 역할을 다하면 지우거나 비활성화 해주자 = 아래 둘 중 하나 사용
            // this.gameObject.SetActive(false);
            // Destroy(this.gameObject);

            // 채집과 드랍 1-0) 내가 상호작용한 필드 오브젝트의 타입에 따라 처리를 추가해봅시다
            var fieldObjectData = DaniTechGameDataManager.Instance.GetDNFieldObjectData(_fieldObjectDataId);
            if (fieldObjectData == null)
            {
                Debug.LogWarning($"유효하지 않은 필드 오브젝트 데이터 입니다! {_fieldObjectDataId}");
                return;
            }
            if (fieldObjectData.FieldObjectType == "Exp")
            {
                DaniTechGameManager.Inst.IncreasePlayerExp(fieldObjectData.ManaPoints * _manaEvent);

                DaniTechGameObjectManager.Inst.RequestDespawnFieldObject(_fieldObjectInstanceId, _fieldObjectDataId);
            }

            if (fieldObjectData.FieldObjectType == "DropItem")
            {
                DaniTechGameObjectManager.Inst.RequestDespawnFieldObject(_fieldObjectInstanceId, _fieldObjectDataId);

                if (fieldObjectData.Id == "dropItem_heart_1")
                {
                    DaniTechGameManager.Inst.HealingPlayerHp(fieldObjectData.HealAmount);
                    DaniTechUIManager.Instance.OpenSimplePopup("체력회복!");
                }
                else if (fieldObjectData.Id == "dropItem_chest_1")
                {
                    DaniTechUIManager.Instance.OpenSimplePopup("랜덤 이벤트 발생!");
                    int randomNum = Random.Range(0, 4);
                    randomNum = 0;
                    switch (randomNum)
                    {
                        case 0:
                            DaniTechUIManager.Instance.OpenSimplePopup("이벤트 웨이브 발생!");
                            WaveSpawnManager.instance.TriggerEventWave("mob_event_1", 30, 10);
                            break;
                        case 1:
                            DaniTechUIManager.Instance.OpenSimplePopup("마나 획득량 2배 증가!");
                            StartCoroutine(CoManaEvent());
                            break;
                        case 2:
                            DaniTechUIManager.Instance.OpenSimplePopup("체력 모두 회복!");
                            DaniTechGameManager.Inst.HealingPlayerHp((int)player.GetMaxHp());
                            break;
                        case 3:
                            DaniTechUIManager.Instance.OpenSimplePopup("매직미사일 일시적 각성!");
                            StartCoroutine(CoAwakenEvent());
                            break;
                    }

                }
                else if (fieldObjectData.Id == "dropItem_wand_1")
                {
                    DaniTechUIManager.Instance.OpenSimplePopup("공격력, 이동속도 소폭증가!");
                    DaniTechGameManager.Inst.IncreasePlayerStat(fieldObjectData.DmgAmount, fieldObjectData.SpeedAmount);
                }
                else if (fieldObjectData.Id == "dropItem_wand_dmgup_1")
                {
                    DaniTechUIManager.Instance.OpenSimplePopup("공격력, 이동속도 대폭증가!");
                    DaniTechGameManager.Inst.IncreasePlayerStat(fieldObjectData.DmgAmount, fieldObjectData.SpeedAmount);
                }
            }
        }
    }



    // 코루틴 =========================================
    
    private IEnumerator CoManaEvent()
    {
        _manaEvent = 2;

        yield return new WaitForSeconds(30);

        _manaEvent = 1;
        DaniTechUIManager.Instance.OpenSimplePopup("마나 획득량 원상복구!");
    }

    private IEnumerator CoAwakenEvent()
    {
        var skillData = DaniTechGameDataManager.Instance.GetSkill("skill_magicArrow_01");
        if (skillData == null) yield return null;

        var skillLevel = DaniTechGameObjectManager.Inst.GetSkillLevel("skill_magicArrow_01");
        int eventSkillLevel = 15;
        skillData.SkillLevel = eventSkillLevel;

        yield return new WaitForSeconds(15);

        skillData.SkillLevel = skillLevel;
    }
}
