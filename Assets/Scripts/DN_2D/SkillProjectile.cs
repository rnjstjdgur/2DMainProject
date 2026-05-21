using System.Collections;
using UnityEngine;

public class SkillProjectile : DaniTech_SkillBase
{
    private Vector3 _moveDirection = Vector3.right;

    public void InitSkillObject(Vector3 launchDirection)
    {
        _moveDirection = launchDirection.normalized;

        float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        // [ToDo] 날아가는 속도를 일단 5.0으로 하고 나중에 데이터에서 받아와 스킬별로 다른 속도로 날아가도록 하자
        StartCoroutine(ShootProjectileSkill());
    }

    IEnumerator ShootProjectileSkill()
    {
        transform.position += _moveDirection * 10.0f * Time.deltaTime;
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }
}
