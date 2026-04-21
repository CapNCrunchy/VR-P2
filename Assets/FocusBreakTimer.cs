// Source: ChatGPT
using UnityEngine;
using TMPro;

public class FocusBreakTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI modeLabel;

    public float focusTime = 3000f; // 50 minutes
    public float breakTime = 600f;  // 10 minutes

    private float currentTime;
    private bool isRunning = true;
    private bool isFocusMode = true;

    void Start()
    {
        currentTime = focusTime;
        UpdateDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            if (isFocusMode)
            {
                isFocusMode = false;
                currentTime = breakTime;
                modeLabel.text = "Break Time";
            }
            else
            {
                isRunning = false;
                currentTime = 0;
            }
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (isFocusMode)
        {
            modeLabel.text = "Focus Time";
        }
    }
}
