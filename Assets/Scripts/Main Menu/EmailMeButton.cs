using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Networking;
using System;
using TMPro;

public class EmailMeButton : MonoBehaviour
{
    // Email menu stuff
    [SerializeField] GameObject EmailInfoCanvas;

    [Space(5f)]
    [SerializeField] string email;

    [Space(5f)]
    [SerializeField] float tweenSpeedMultiplier;
    [SerializeField] LeanTweenType easeCurveOpen, easeCurveClose;

    Transform emailInfoMenu;
    Blur.BlurMaterial emailInfoMenuBlur;

    // Timer stuff
    [SerializeField] GameObject openEmailButton;
    DateTime lastEmailSent = DateTime.MinValue;

    [Space(5f)]
    [SerializeField] List<GameObject> InactivateOnOpen;


    void Awake() {
        startCooldownTimer(false);
        EmailInfoCanvas.SetActive(true);
        StartCoroutine(Start());
    }
    IEnumerator Start()
    {
        EmailInfoCanvas.SetActive(true);
        GetObjects();
        yield return new WaitForEndOfFrame();
        EmailInfoCanvas.SetActive(true);
        yield return new WaitForEndOfFrame();
        EmailInfoCanvas.SetActive(true);
        yield return new WaitForEndOfFrame();
        EmailInfoCanvas.SetActive(false);
    }
    void OnEnable() {
        if (!CheckObjects()) { GetObjects(); }
    }


    // ----- METHODS -----

    // Copy the email to clipboard
    public void CopyToClipboard() {
        // Idk if either WONT work, so I'm doing both ig

        // Copy via TextEditor
        TextEditor te = new TextEditor();
        te.text = email;
        te.SelectAll();
        te.Copy();

        // Copy via GUIUtility
        GUIUtility.systemCopyBuffer = email;

        CloseEmailInfo();
        startCooldownTimer();
    }

    // Open email app and set it up
    // I THINK???
    public void OpenEmailApp() {
        string subject = UnityWebRequest.EscapeURL("Support Email - Simply Sudoku!").Replace("+","%20");
        string body = UnityWebRequest.EscapeURL("");
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        CloseEmailInfo();
        startCooldownTimer();
    }


    // ----- TWEENING IN AND OUT -----

    // Opening and closing the menu
    // Tweening it in/out
    public void OpenEmailInfo()  { TweenCanvas(true); }
    public void CloseEmailInfo() { TweenCanvas(false); }
    void TweenCanvas(bool turnOn) {
        if (!CheckObjects()) { return; }
        if (emailInfoMenu.gameObject.LeanIsTweening()) {
            return;
        }

        ActivateObjects(!turnOn);

        // Get objects
        GameObject obj = emailInfoMenu.gameObject;
        RectTransform rect = emailInfoMenu.GetComponent<RectTransform>();
        CanvasGroup canv = emailInfoMenu.GetComponent<CanvasGroup>();
        LeanTweenType easeCurve = (turnOn) ? easeCurveOpen : easeCurveClose;
        if (canv == null) {
            canv = emailInfoMenu.gameObject.AddComponent<CanvasGroup>();
        }

        // Get vars
        float start = (turnOn) ? 0f : 1f;
        float end = (turnOn) ? 1f : 0f;
        tweenSpeedMultiplier = (tweenSpeedMultiplier <= 0.1f) ? 0.1f : tweenSpeedMultiplier;
        float time = tweenSpeedMultiplier * Settings.AnimSpeedMultiplier;

        // Initial state
        EmailInfoCanvas.SetActive(true);
        canv.alpha = start;
        rect.localScale = new Vector3(start, start, rect.localScale.z);

        // Tween scale and opacity
        LeanTween.value(obj, start, end, time)
        .setEase(easeCurve)
        .setOnUpdate((float value) => {
            canv.alpha = value;
            rect.localScale = new Vector3(value, value, rect.localScale.z);
        })
        .setOnComplete(() => {
            EmailInfoCanvas.SetActive(turnOn);
        });

        // Tween blur canvas
        float delay = (turnOn) ? 0f : (0.25f * time);
        float blurTime = (turnOn) ? time : (0.75f * time);
        LeanTween.value(start, end, blurTime)
        .setEase(easeCurve)
        .setDelay(delay)
        .setOnStart(() => {
            EmailInfoCanvas.transform.Find("Blur Material").gameObject.SetActive(true);
            emailInfoMenuBlur.SetRadius(start);
        })
        .setOnUpdate((float value) => {
            emailInfoMenuBlur.SetRadius(value);
        })
        .setOnComplete(() => {
            EmailInfoCanvas.transform.Find("Blur Material").gameObject.SetActive(turnOn);
            emailInfoMenuBlur.SetRadius(end);
        });
    }


    // ----- UTILITIES -----

    // Turn on or off the canvas groups of each object in <InactivateOnOpen>
    void ActivateObjects(bool turnOn) {
        foreach (GameObject obj in InactivateOnOpen) {
            CanvasGroup canv = obj.GetComponent<CanvasGroup>();
            if (canv == null) {
                canv = obj.AddComponent<CanvasGroup>();
            }

            canv.blocksRaycasts = turnOn;
            canv.interactable = turnOn;
        }
    }

    // Gets objects attatched to this gameObject
    void GetObjects() {
        emailInfoMenu = EmailInfoCanvas.transform.Find("Menu");
        emailInfoMenuBlur = Blur.FindMaterial(Blur.BlurType.UI);
    }

    // Return true if objects are good and menu is ready to tween
    bool CheckObjects() {
        // Check if email menu is good
        if (emailInfoMenu == null) {
            GetObjects();
            if (emailInfoMenu == null) {
                Debug.Log("EMAIL INFO MENU IS NULL");
                return false;
            }
        }
        // Check if blur is good
        if (emailInfoMenuBlur == null) {
            GetObjects();
            if (emailInfoMenuBlur == null) {
                Debug.Log("NO BLUR ON THE EMAIL CANVAS");
                return false;
            }
        }
        // Check if its tweening
        if (emailInfoMenu.gameObject.LeanIsTweening()) {
            return false;
        }
        return true;
    }


    // ----- CHECKING TIME SINCE LAST EMAIL -----

    void startCooldownTimer(bool resetLastEmailTime) {
        Debug.Log("Starting cooldown timer");
        EmailCooldown cooldown = openEmailButton.GetComponent<EmailCooldown>();
        if (cooldown == null) {
            cooldown = openEmailButton.AddComponent<EmailCooldown>();
        }
        cooldown.StartTimer(resetLastEmailTime);
    }
    void startCooldownTimer() {
        startCooldownTimer(true);
    }
}
