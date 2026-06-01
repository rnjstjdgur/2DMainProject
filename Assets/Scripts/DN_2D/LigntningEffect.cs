using System;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;

    private Action<SkillCollisionInfo> _collisionCallback;

    // 외부에서 호출하여 번개를 즉시 생성
    public void Play(Vector3 start, Vector3 end, int segments, float duration, Action<SkillCollisionInfo> callback, string skillDataId)
    {
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        DrawZigzag(start, end, segments);

        // [수정] 직선상의 모든 적을 찾음
        Vector2 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, dir, dist, LayerMask.GetMask("Enemy"));

        foreach (var hit in hits)
        {
            callback?.Invoke(new SkillCollisionInfo(skillDataId, hit.collider));
        }

        Destroy(gameObject, duration);
    }

    private void DrawZigzag(Vector3 start, Vector3 end, int segments)
    {
        _lineRenderer.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            if (i > 0 && i < segments - 1)
            {
                point += new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f), 0);
            }
            _lineRenderer.SetPosition(i, point);
        }
    }
}
