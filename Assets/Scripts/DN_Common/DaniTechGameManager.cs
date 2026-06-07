using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaniTechGameManager : MonoBehaviour
{
    public static DaniTechGameManager Inst { get; set; }

    // 플레이 중에 저장되어야 하는 정보들이 있는 위치
    private DaniTechPlayerModel _playerModel = new DaniTechPlayerModel();
    private bool _IsGameStart = false;
    private Player2D _localPlayer;
    private float _gameTimer;
    private int _gameClearTime = 720;
    private bool _IsGameClear = false;
    private WaveSpawnManager _waveManager;
    public int _manaMultiplier { get; private set; }


    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        _manaMultiplier = 1;
        LoadSaveData();
        _waveManager = WaveSpawnManager.instance;
    }

    private void Update()
    {
        var gameTimer = _waveManager.GetGameTimer();
        if (gameTimer >= _gameClearTime && _IsGameClear == false)
        {
            GameClear();
        }
    }
    // 게임 흐름 관련 =======================================================

    public bool IsGameStart()
    {
        return _IsGameStart;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        _IsGameStart = false;

        if (DaniTechUIManager.Instance != null)
        {
            DaniTechUIManager.Instance.ClearAllHudSlot();
        }

        if (DaniTechGameObjectManager.Inst != null)
        {
            DaniTechGameObjectManager.Inst.ResetObjectOnNewGame();
            DaniTechGameObjectManager.Inst.ResetSkillList();

            var newPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();
            if (newPlayer != null)
            {
                DaniTechGameObjectManager.Inst.RegisterLocalPlayer(newPlayer);
                _localPlayer = newPlayer;
            }
        }

        if (DaniTechGameDataManager.Instance != null)
        {
        }

        if (WaveSpawnManager.instance != null)
        {
            WaveSpawnManager.instance.ResetWaveManagerOnRestart();
        }

        _IsGameClear = false;
        _IsGameStart = true;
    }

    public void SaveAndEndGame()
    {
        _IsGameStart = false;
        SaveData();
        Application.Quit();
    }

    private void GameClear()
    {
        _IsGameClear = true;
        Debug.LogError("게임클리어!");
        DaniTechUIManager.Instance.OpenPopupUI(DaniTechUIType.GameClearUI);
        TimeManager.instance.TimeStop();
    }

    // 데이터 관련 ==========================================================

    public void SaveData()
    {
        DaniTechNetworkManager.Inst.RequstSaveData(_playerModel);
    }
    

    private void LoadSaveData()
    {
        _playerModel = DaniTechNetworkManager.Inst.RequstLoadSaveData();
    }

    // 플레이어 관련 ===========================================================

    public void IncreasePlayerExp(int exp)
    {
        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        // 추후에 한곳에서 관리할 수 있게 익스텐션으로 빼도 된다
        //_playerModel.PlayerTotalExp += exp;
        if (_localPlayer == null) return;
        _localPlayer.IncreasePlayerMp(exp);
    }

    public void HealingPlayerHp(int healAmount)
    {
        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        if (_localPlayer == null) return;
        _localPlayer.HealingHp(healAmount);
    }

    public void IncreasePlayerStat(float dmgAmount, float speedAmount)
    {
        _localPlayer = DaniTechGameObjectManager.Inst.GetLocalPlayer();

        if (_localPlayer == null) return;
        _localPlayer.ApplyWandBuff(dmgAmount, speedAmount);
    }

    public Transform GetPlayerTransform()
    {
        var player = DaniTechGameObjectManager.Inst.GetLocalPlayer();
        return player.transform;
    }

    // 아이템 관련 =================================================================

    public void AddItem(string itemDataId, int addItemCount)
    {
        // 저장할때 고유값 ID를 부여하기 위해 사용
        long uniqueId = DaniTechGameUtil.GenerateUniqueId();

        // TODO : 우선 쉽게 사용할 수 있도록 중복 처리는 빼두었다. 습득할때마다 아이템이 하나씩 추가되도록 해두고
        // 추후에 중복값은 StackCount가 다 찰때까지 누적해줄 수 있도록 로직을 추가하자
        var newItem = new DaniTechItemModel();
        newItem.ItemUniqueId = uniqueId;
        newItem.ItemDataId = itemDataId;
        newItem.ItemStackCount = addItemCount;

        _playerModel.ItemList.Add(newItem);
    }

    public List<DaniTechItemModel> GetPlayerItemList()
    {
        // _playerModel이 Private이므로 외부에서 ItemList를 받아올 수 있게 Get함수를 사용한다
        return _playerModel.ItemList;
    }

    public void StartManaEvent()
    {
        StartCoroutine(CoManaEvent());
    }

    public void StartAwakenEvent()
    {
        StartCoroutine(CoAwakenEvent());
    }

    // 스킬 관련 ======================================================

    public Transform GetClosestEnemy(Vector3 thisPosition, float scanRadius, LayerMask enemyLayer, float skillDistance)
    {
        // 1. 플레이어 주변의 일정 반경 내에 있는 모든 몬스터의 Collider를 가져옵니다.
        Collider2D[] enemies = Physics2D.OverlapCircleAll(thisPosition, scanRadius, enemyLayer);

        Transform closestEnemy = null;
        float minDistance = skillDistance;

        // 2. 찾은 몬스터들을 하나씩 순회하며 거리를 비교합니다.
        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector3.Distance(thisPosition, enemy.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }


    // 코루틴 =================================================

    private IEnumerator CoManaEvent()
    {
        _manaMultiplier = 2;

        yield return new WaitForSeconds(30);

        _manaMultiplier = 1;
        DaniTechUIManager.Instance.OpenSimplePopup("마나 획득량 원상복구!");
    }

    private IEnumerator CoAwakenEvent()
    {
        string skillDataId = "skill_magicArrow_01";
        var skillData = DaniTechGameDataManager.Instance.GetSkill(skillDataId);
        if (skillData == null) yield return null;

        int originalLevel = DaniTechGameObjectManager.Inst.GetSkillLevel(skillDataId);
        int eventSkillLevel = 15;

        DaniTechGameObjectManager.Inst.SetSkillLevel(skillDataId, eventSkillLevel);

        yield return new WaitForSeconds(15);

        DaniTechGameObjectManager.Inst.SetSkillLevel(skillDataId, originalLevel);
    }
}
