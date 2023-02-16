using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class EmailCooldown : MonoBehaviour
{
    string prefix;
    DateTime lastEmailSent = DateTime.MinValue;
    int remainingSeconds;

    private void Awake() { OnEnable(); }
    private void Start() { OnEnable(); }
    private void OnEnable() {
        getLastEmailTime();
        StartTimer(false);
    }

    public void StartTimer(bool setLastEmailSent) {
        StartCoroutine(StartTimerEnum(setLastEmailSent));
    }
    IEnumerator StartTimerEnum(bool setLastEmailSent) {
        if (setLastEmailSent) {
            setLastEmailTime();
        }
        prefix = "To avoid spam, please wait a few minutes before sending another email!\nTime remaining: ";

        transform.Find("Wait").Find("Darken Image").GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);

        yield return new WaitForEndOfFrame();
        startEmailTimer();
    }

    void setLastEmailTime() {
        lastEmailSent = DateTime.Now;
        PlayerPrefs.SetString(nameof(lastEmailSent), lastEmailSent.ToBinary().ToString());
    }
    void getLastEmailTime() {
        // No pref saved for lastEmailSent
        if (!PlayerPrefs.HasKey(nameof(lastEmailSent))) {
            lastEmailSent = DateTime.MinValue;
            return;
        }

        // Interpret from string
        string prefString = PlayerPrefs.GetString(nameof(lastEmailSent));
        long dateLong;

        // Can't parse, set to min value
        if (!long.TryParse(prefString, out dateLong)) {
            lastEmailSent = DateTime.MinValue;
            return;
        }

        lastEmailSent = DateTime.FromBinary(dateLong);
    }

    // Turns on and starts email timer, etc
    void startEmailTimer() {
        Transform waitArea = transform.Find("Wait");
        TMP_Text timerText = waitArea.Find("Wait Text").GetComponent<TMP_Text>();

        // Set image dimensions
        RectTransform imageRect = waitArea.Find("Darken Image").GetComponent<RectTransform>();
        float pad = -0.3f * imageRect.rect.height;
        RectTransformOffset.Sides(imageRect, 1.25f * pad);
        RectTransformOffset.Vertical(imageRect, pad);

        if (waitHasPassed()) {
            waitArea.gameObject.SetActive(false);
            return;
        }

        waitArea.gameObject.SetActive(true);
        StopCoroutine(updateTimer(timerText));
        StartCoroutine(updateTimer(timerText));
    }
    IEnumerator updateTimer(TMP_Text timerText) {
        TimeSpan toWait = new TimeSpan(0, 10, 0);
        TimeSpan hourFromLast = new TimeSpan(lastEmailSent.Ticks + toWait.Ticks);
        int remainingSeconds = (int)(new TimeSpan(lastEmailSent.Ticks + toWait.Ticks - DateTime.Now.Ticks).TotalSeconds);
        string time = "";
        while (true) {
            // Stop when an hour is up
            if (remainingSeconds <= 0) {
                lastEmailSent = DateTime.MinValue;
                PlayerPrefs.SetString(nameof(lastEmailSent), lastEmailSent.ToBinary().ToString());
                transform.Find("Wait").gameObject.SetActive(false);
                yield break;
            }

            // Get timespan of remaining seconds
            TimeSpan remainingTime = TimeSpan.FromSeconds(remainingSeconds);
            time = format(remainingTime);

            // Set timer text
            timerText.text = prefix + time;
            --remainingSeconds;

            yield return new WaitForSeconds(1f);
        }
    }

    string format(TimeSpan time) {
        string str = time.Minutes + ":";
        if (time.Seconds < 10) {
            str += "0";
        }
        str += time.Seconds;
        return str;
    }

    // Check if an hour has passed since last email sent
    bool waitHasPassed() {
        // It hasn't been set yet
        if (lastEmailSent == DateTime.MinValue) { return true; }

        // Check if hour has passed, reset it if so
        TimeSpan span = new TimeSpan(DateTime.Now.Ticks - lastEmailSent.Ticks);
        if (span.Minutes >= 10) {
            lastEmailSent = DateTime.MinValue;
            return true;
        }
        return false;
    }
}
