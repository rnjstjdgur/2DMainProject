using System.Collections;
using UnityEngine;

public class Player2D : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float Move_Speed = 5f;
    [SerializeField] private Vector3 Move_Direction;

    [Header("스킬")]
    [SerializeField] private Collider2D Collider_PlayerNormalAttack;

    // 스킬 관련 =======================================================
    public enum ViewType { Isometric, SideView, TopDown}
    public ViewType _currentView = ViewType.TopDown;
    private Vector2 _lookDirection = Vector2.right;

    private Vector2 _lastOverlapOffset = Vector2.zero;
    private float _lastOverlapRadius = 1f;

    private void Awake()
    {
        Collider_PlayerNormalAttack.gameObject.SetActive(false);
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

        transform.Translate(Move_Direction * Move_Speed * Time.deltaTime);
    }

    // 스킬 ====================================================

    public void UseNormalAttack()
    {
        //changePlayerState(Atk) Danitech2DPlayer에 있음
        Collider_PlayerNormalAttack.gameObject.SetActive(true);
        StartCoroutine(CoStartNormalAttack());
    }

    public void UseCircleSkill()
    {
        UseOverlapSkill(new Vector2(1.0f, 0.0f), 3f);
    }

    public void UseRaySkill()
    {

    }

    public void UseProjectileSkill()
    {

    }

    // 기믹 관련 ====================================================================

    IEnumerator CoStartNormalAttack()
    {
        yield return new WaitForSeconds(1.0f);
        Collider_PlayerNormalAttack.gameObject.SetActive(false);
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
}
