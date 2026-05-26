using UnityEngine;

public class HudSlotUI : MonoBehaviour
{

    private int _instanceId;
    private Transform _targetTransform;

    public void InitSlot(int instanceId, Transform targetTransform)
    {
        _instanceId = instanceId;
        _targetTransform = targetTransform;
    }

    private void Update()
    {
        if (_targetTransform != null)
        {
            //this.gameObject.transform.position = _targetTransform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(_targetTransform.position);

            var rectTransform = this.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = screenPos;
            }
        }
    }
}
