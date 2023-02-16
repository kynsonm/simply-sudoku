using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class StatisticsTimer : MonoBehaviour
{
    // Update this text every <interval>
    TMP_Text timerText;
    TextTheme textTheme;

    // How often to update the timer, and by what amount
    float interval;

    // Starts the timer enumerator
    public void StartTimer(UnityAction<float> actionToCall, float intervalOfUpdate) {
        interval = (intervalOfUpdate <= 0.1f) ? 0.1f : intervalOfUpdate;
        StartCoroutine(Timer(actionToCall));
    }

    // Set the timerText variable
    public void SetTimerText(TMP_Text text) {
        timerText = text;
        textTheme = timerText.gameObject.GetComponent<TextTheme>();
    }

    // Is invoked every <interval>
    // Calls the action w/ argument <interval>
    // For what I'm doing, the action should just add time to the given statistics timer variable
    IEnumerator Timer(UnityAction<float> action) {
        while (true) {
            action.Invoke(interval);
            yield return new WaitForSeconds(interval);
        }
    }

    // Update the text of <timerText> to the new time, formatted
    public void UpdateText(float time) {
        if (timerText == null) { return; }
        timerText.text = FormatTime(time);
    }

    // Formats seconds into days, hours, minutes, and seconds
    string FormatTime(float seconds) {
        string s = "";

        TimeSpan time = TimeSpan.FromSeconds(seconds);
        bool onlySeconds = true;

        if (time.Days > 0) {
            s += time.Days.ToString() + " <i>days</i>, ";
            textTheme.MaxTextRatio = 1f;
            onlySeconds = false;
        }
        if (time.Hours > 0) {
            s += time.Hours.ToString() + " <i>hours</i>, ";
            textTheme.MaxTextRatio = 1f;
            onlySeconds = false;
        }
        if (time.Minutes > 0) {
            s += time.Minutes.ToString() + " <i>minutes</i>, ";
            textTheme.MaxTextRatio = 0.92f;
            onlySeconds = false;
        }

        s += time.Seconds + " <i>seconds</i>";
        if (onlySeconds) { textTheme.MaxTextRatio = 0.8f; }

        return s;
    }
}
