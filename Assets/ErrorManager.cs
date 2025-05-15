using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorManager : MonoBehaviour
{
    public TMP_Text DisplayText;
    public GameObject panel;
    private float DisplayTime = 0f;

    private Queue<(string, float)> ErrorQueue = new();
    private (string, float) currentError;

    private bool isDisplaying = false;
    
    private float t = 0f;
    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (isDisplaying) {
            panel.SetActive(true);
            if (t >= DisplayTime && ErrorQueue.Count > 0) {
                currentError = ErrorQueue.Dequeue();
                t = 0f;
                DisplayText.text = currentError.Item1;
                DisplayTime = currentError.Item2;

            } else if (t >= DisplayTime && ErrorQueue.Count == 0) {
                isDisplaying = false;
            } else {
                t += .01f;
            }
        } else {
            panel.SetActive(false);
        }
    }

    public void DisplayError(string message, float time) {
        ErrorQueue.Enqueue((message, time));
        isDisplaying = true;
    }
}
