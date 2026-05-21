using System.Collections;
using UnityEngine;

public class SkillProjectile : DaniTech_SkillBase
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer_Effect;

    private Vector3 _moveDirection = Vector3.right;
    private float _skillMoveSpeed = 10.0f;  // [ToDo] 나중에 데이터로 받아와서 스킬의 속도를 대입하자

    public void InitSkillObject(Vector3 launchDirection)
    {
        _moveDirection = launchDirection.normalized;

        // [ToDo] 데이터로 스킬 스프라이트를 받아와서 스킬별로 다른 이미지가 생성됨
        //var skillData = DaniTechGameDataManager.Instance.GetSkill();
        //Sprite skillSprite = DaniTechResourceManager.Inst.LoadSprite(skillData.spritePath);
        //spriteRenderer_Effect.sprite = skillSprite;

        float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        StartCoroutine(DestroySkillAfterDelay());
    }

    void Update()
    {
        transform.position += _moveDirection * _skillMoveSpeed * Time.deltaTime;
    }

    IEnumerator DestroySkillAfterDelay()
    {
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }
}
