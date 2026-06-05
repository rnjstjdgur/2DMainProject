using UnityEngine;
using UnityEngine.UI;

public class DaniTech_MainUI : DaniTechUIBase
{
    [Header("시간")]
    [SerializeField] private Text Text_GameTimer;

    private float _gameTimer;

    private void OnEnable()
    {
        Text_GameTimer.GetComponent<Text>();
    }

    private void Update()
    {
        _gameTimer = WaveSpawnManager.instance.GetGameTimer();
        int timeMInute = (int)(_gameTimer / 60);
        int timeSecond = (int)(_gameTimer % 60);
        Text_GameTimer.text = $"{timeMInute:D2} : {timeSecond:D2}";
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DaniTechUIManager.Instance.openBackContentUI(DaniTechUIType.EscUI);
        }
    }
}
