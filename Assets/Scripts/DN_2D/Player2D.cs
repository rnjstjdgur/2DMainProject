using System;
using System.Collections;
using System.Xml;
using UnityEngine;

public class Player2D : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float Move_Speed = 5f;
    [SerializeField] private Vector3 Move_Direction;

    [Header("스킬")]
    [SerializeField] private Collider2D Collider_PlayerNormalAttack;

    [Header("전투관련 정보")]     // 초기값 나중에 데이터로 받아오거나 에디터에서 수정하자
    [SerializeField] private int _maxHp;
    [SerializeField] private int _playerHp = 1000;
    [SerializeField] private int _playerBaseAtk = 100;
    // 스킬 관련 =======================================================
    public enum ViewType { Isometric, SideView, TopDown}
    public ViewType _currentView = ViewType.TopDown;
    private Vector2 _lookDirection = Vector2.right;

    private Vector2 _lastOverlapOffset = Vector2.zero;
    private float _lastOverlapRadius = 1f;
    private int _instanceId = 0;
    private bool _lookRight = true;

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
        DaniTechGameObjectManager.Inst.RegisterLocalPlayer(this);
        DaniTechGameObjectManager.Inst.RegisterLocalPlayer(DaniTechGameObjectManager.Inst.GetLocalPlayer());
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

        if (moveX > 0 && !_lookRight)
        {
            Flip();
        }
        else if (moveX < 0 && _lookRight)
        {
            Flip();
        }

        transform.Translate(Move_Direction * Move_Speed * Time.deltaTime);
    }
    void Flip()
    {
        _lookRight = !_lookRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
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
