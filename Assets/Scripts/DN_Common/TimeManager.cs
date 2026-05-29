using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void TimeStop()
    {
        Time.timeScale = 0f;
    }

    public void TimeStart()
    {
        Time.timeScale = 1f;
    }
}
