using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class Player2D : MonoBehaviour
{
    [SerializeField] private float Move_Speed = 5f;
    [SerializeField] private Vector3 Move_Direction;

    [Header("스킬")]
    [SerializeField] private Collider2D Collider_PlayerNormalAttack;

    private Vector2 _lookDirection = Vector2.right;

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

        Move_Direction = new Vector2(moveX, moveY); //.normalized

        transform.Translate(Move_Direction * Move_Speed * Time.deltaTime);
    }






    //public bool CheckSkillUseable()
    //{
    //    //if (_isSkillUsing == true)
    //    //{
    //    //    DaniTechUIManager.Instance.OpenSimplePopup("스킬이 이미 사용중입니다");
    //    //}
    //    //return false;
    //}
    

    // 스킬
    public void UseNormalAttack()
    {
        //changePlayerState(Atk) Danitech2DPlayer에 있음
        Collider_PlayerNormalAttack.gameObject.SetActive(true);
        StartCoroutine(CoStartNormalAttack());
    }

    public void UseCircleSkill()
    {
        //Vector2 adjustedDir = GetAdjustedDirection(_lookDirection);
    }

    public void UseRaySkill()
    {

    }

    public void UseProjectileSkill()
    {

    }

    IEnumerator CoStartNormalAttack()
    {
        yield return new WaitForSeconds(1.0f);
        Collider_PlayerNormalAttack.gameObject.SetActive(false);
    }
}
