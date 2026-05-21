using UnityEngine;

public class Monster2D : DaniTech_MonsterBase
{
    [Header("데이터 확인용")]
    [SerializeField] private int _instanceId;    // 게임에서 태어날때 부여된 ID                 [오브젝트매니저에서 찾는 용도]
    [SerializeField] private string _dataId;     // 내가 누구인지 나중에 찾을 수 있는 고유 ID    [ 데이터 드리븐 용도]

    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        _dataId = dataId;
    }
}
