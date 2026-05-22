using System;
using System.Collections;
using UnityEngine;

public class SkillProjectile : DaniTech_SkillBase
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer_Effect;

    private int _damage = 100;
    private int _ownerInstanceId;

    private Vector3 _moveDirection = Vector3.right;
    private float _skillMoveSpeed = 10.0f;  // [ToDo] 나중에 데이터로 받아와서 스킬의 속도를 대입하자
    private float _skillDurationTime = 3.0f;    // [ToDo] 나중에 데이터로 받아와서 스킬의 유지시간을 대입하자 (스킬이 강화되면 유지시간이 늘어나는식)


    private event Action<int, int> _onSkillCollision;

    private void OnDisable()
    {
        _onSkillCollision = null;
    }

    public void InitSkillObject(Vector3 launchDirection, int ownerInstanceId, string parentTag, Action<int, int> onSkillCollision = null)
    {
        _moveDirection = launchDirection.normalized;

        // [ToDo] 데이터로 스킬 스프라이트를 받아와서 스킬별로 다른 이미지가 생성됨
        //var skillData = DaniTechGameDataManager.Instance.GetSkill();
        //Sprite skillSprite = DaniTechResourceManager.Inst.LoadSprite(skillData.spritePath);
        //spriteRenderer_Effect.sprite = skillSprite;

        var tag = this.gameObject.tag;
        tag = parentTag;

        //_damage = damage;
        _ownerInstanceId = ownerInstanceId;
        _onSkillCollision = onSkillCollision;

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
        yield return new WaitForSeconds(_skillDurationTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckCollision(collision.collider);
    }

    private void CheckCollision(Collider2D collision)
    {
        var player = DaniTechGameManager.Inst.GetLocalPlayer();

        bool isOwnerPlayer = (_ownerInstanceId == 0);
        if (collision.CompareTag("Player") && isOwnerPlayer)
        {
            // 일단 투사체가 직접 데미지를 부여해보자
            player.TakeDamage(_damage);
            var instanceId = player.GetPlayerInstanceId();
            Debug.Log($"{instanceId}");

            Destroy(this.gameObject);
        }

        _onSkillCollision?.Invoke(0, player.Damage());
    }
}
