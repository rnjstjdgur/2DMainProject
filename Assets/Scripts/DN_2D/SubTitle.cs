using UnityEngine;
using UnityEngine.UI;

public class SubTitle : MonoBehaviour
{
    [Header("자막")]
    [SerializeField] private Text Text_Subtitle;

    public void SetSubtitle(string text)
    {
        Text_Subtitle.text = text;
    }
}
