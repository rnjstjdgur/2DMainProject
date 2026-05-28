using System;
using System.Collections;
using System.Xml;
using UnityEngine;

public class Player2D : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float Move_Speed = 5.0f;
    [SerializeField] private Vector3 Move_Direction;

    [Header("스킬")]
    [SerializeField] private Collider2D Collider_PlayerNormalAttack;

    [Header("전투관련 정보")]     // 초기값 나중에 데이터로 받아오거나 에디터에서 수정하자
    [SerializeField] private int _maxHp;
    [SerializeField] private int _playerHp = 1000;
    [SerializeField] private int _playerMp = 0;
    [SerializeField] private int _playerLevel = 1;
    [SerializeField] private int _maxMp;
    [SerializeField] private int _playerBaseAtk = 100;
    // 스킬 관련 =======================================================
    public enum ViewType { Isometric, SideView, TopDown}
    public ViewType _currentView = ViewType.TopDown;
    private Vector2 _lookDirection = Vector2.right;

    private DNCharacterData _playerData;

    private Vector2 _lastOverlapOffset = Vector2.zero;
    private float _lastOverlapRadius = 1f;
    private int _instanceId = 0;
    private bool _isLookRight = true;
    private bool _isPlayerLevelUp = false;

    private readonly int[] _expTable = { 0, 100, 200, 400, 800, 1600, 2400, 3200, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    private void Awake()
    {
        Collider_PlayerNormalAttack.gameObject.SetActive(false);

        _playerHp = 1000;
        _maxHp = _playerHp;
    }

    private void Start()
    {
        _playerData = DaniTechGameDataManager.Instance.GetCharacterData("character_basic_01");
        _playerLevel = _playerData.PlayerLevel;

        DaniTechGameObjectManager.Inst.RegisterLocalPlayer(this);
        DaniTechGameObjectManager.Inst.StartAutoProjectileSkillLoop();
        DaniTechUIManager.Instance.AddHudSlot(_instanceId, this.gameObject.transform);
    }

    private void OnDisable()
    {
        ResetStatChangedEvent();
    }

    private void Update()
    {
        bool isGameStart = DaniTechGameManager.Inst.IsGameStart();
        if (isGameStart == false) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Move_Direction = new Vector2(moveX, moveY).normalized;

        if (Move_Direction != Vector3.zero)
        {
            _lookDirection = Move_Direction.normalized;
        }

        if (moveX > 0 && !_isLookRight)
        {
            Flip();
        }
        else if (moveX < 0 && _isLookRight)
        {
            Flip();
        }

        transform.Translate(Move_Direction * Move_Speed * Time.deltaTime);
    }

    // 이동 관련 ========================================================

    void Flip()
    {
        _isLookRight = !_isLookRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // 레벨 관련 ========================================================

    public int GetPlayerLevel()
    {
        return _playerLevel;
    }

    public int GetPlayerMp()
    {
        return _playerMp;
    }

    private void CheckPlayerLevelUp()
    {
        _isPlayerLevelUp = false;

        while (_playerLevel < _expTable.Length - 1)
        {
            int nextLevelExpRequired = _expTable[_playerLevel];
            if (_playerMp >= nextLevelExpRequired)
            {
                _playerMp -= _expTable[_playerLevel]; // 사용한 경험치만큼 차감
                _playerLevel++;                       // 레벨 상승
                _isPlayerLevelUp = true;

                Debug.LogWarning($"★ LEVEL UP! ★ 현재 레벨: {_playerLevel}");
            }
            else
            {
                break;
            }
        }

        if (_isPlayerLevelUp)
        {
            //_playerHp = _maxHp;
            InvokeStatChangedEvent(); // UI 갱신 신호
        }
    }

    public int IncreasePlayerMp(int exp)
    {
        if (_playerLevel >= _expTable.Length - 1)
        {
            Debug.Log("[만렙] 이미 최고 레벨에 도달하여 경험치를 획득할 수 없습니다.");
            return _playerMp;
        }

        _playerMp += exp;
        Debug.Log($"[경험치 획득] +{exp} | 현재 경험치: {_playerMp} / 다음 레벨 필요 경험치: {_expTable[_playerLevel]}");

        CheckPlayerLevelUp();

        InvokeStatChangedEvent();

        return _playerMp;
    }

    public int GetRequiredExpForCurrentLevel()
    {
        if (_playerLevel < _expTable.Length)
        {
            return _expTable[_playerLevel];
        }
        return 1; // 만렙 시 에러 방지용 기본값
    }

    // 스킬 ====================================================

    public void UseNormalAttack()
    {
        //changePlayerState(Atk) Danitech2DPlayer에 있음
        Collider_PlayerNormalAttack.gameObject.SetActive(true);
        StartCoroutine(CoStartNormalAttack());
    }

    public void UseCircleSkill(float skillRange, float skillRadius)
    {
        UseOverlapSkill(new Vector2(skillRange, 0.0f), skillRadius);
    }

    public void UseProjectileSkill()
    {
        DaniTechGameObjectManager.Inst.CreateProjectileSkillObjectByPlayer();
    }

    // 기믹 관련 ====================================================================

    IEnumerator CoStartNormalAttack()
    {
        yield return new WaitForSeconds(1.0f);
        Collider_PlayerNormalAttack.gameObject.SetActive(false);
    }
    

    private Vector2 GetAdjusedDirection(Vector2 rawDir)
    {
        switch (_currentView)
        {
            case ViewType.Isometric:
                return new Vector2(rawDir.x - rawDir.y, (rawDir.x + rawDir.y) * 0.5f).normalized;

            case ViewType.SideView:
                return new Vector2(rawDir.x, 0).normalized;

            case ViewType.TopDown:
            default:
                return rawDir.normalized;
        }
    }

    public Vector3 GetLookDirection()
    {
        return new Vector3(_lookDirection.x, _lookDirection.y, 0f);
    }

    public void UseOverlapSkill(Vector2 offsetPosition, float radius)
    {
        _lastOverlapOffset = offsetPosition;
        _lastOverlapRadius = radius;

        Vector2 lookDir = GetAdjusedDirection(_lookDirection);

        Vector2 rightOffset = lookDir * offsetPosition.x;
        Vector2 upOffset = new Vector2(-lookDir.y, lookDir.x) * offsetPosition.y;

        Vector2 center = (Vector2)transform.position + rightOffset + upOffset;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);

        foreach (Collider2D col in hitColliders)
        {
            if (col != null && col.gameObject != this.gameObject)
            {
                Debug.Log($"오버랩 스킬 적중: {col.name}");
            }
        }
    }

    // 전투 관련 =================================================

    public int Damage()
    {
        return _playerBaseAtk;
    }

    public int GetPlayerInstanceId()
    {
        return _instanceId;
    }

    public void TakeDamage(int damage)
    {
        _playerHp -= damage;
        Debug.LogError($"플레이어가 {damage} 데미지를 입었습니다. 현재체력: {_playerHp}");
        InvokeStatChangedEvent();

        if (_playerHp < 0)
        {
            // 죽음 처리 하기
            PlayerDie();
        }
    }

    public void PlayerDie()
    {
        // bool _isAlive = false;
    }
    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback, Action<int, int> mpChangeCallback)
    {
        _onHpChanged += hpChangeCallback;
        _onMpChanged += mpChangeCallback;
    }

    public void ResetStatChangedEvent()
    {
        _onHpChanged = null;
        _onMpChanged = null;
    }

    private void InvokeStatChangedEvent()
    {
        // 우선 HP든 MP든 하나라도 바뀌면 다 호출해준다
        _onHpChanged?.Invoke(_playerHp, _maxHp);
        int currentLevelExpRequired = (_playerLevel < _expTable.Length) ? _expTable[_playerLevel] : 1;
        _onMpChanged?.Invoke(_playerMp, currentLevelExpRequired);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Vector2 lookDir = GetAdjusedDirection(_lookDirection);
        Vector2 rightOffset = lookDir * _lastOverlapOffset.x;
        Vector2 upOffset = new Vector2(-lookDir.y, lookDir.x) * _lastOverlapOffset.y;

        Vector3 center = transform.position + (Vector3)(rightOffset + upOffset);
        Gizmos.DrawWireSphere(center, _lastOverlapRadius);
    }
}
