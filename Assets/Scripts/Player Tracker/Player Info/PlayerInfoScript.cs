using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerInfoScript : MonoBehaviour
{
    [SerializeField] int DB_playerLevel;
    [SerializeField] int DB_playerXP;
    [SerializeField] int DB_xpTotalLevel;
    [SerializeField] int DB_xpNextLevel;
    [Space(10f)]

    // Variables
    [SerializeField] float GameObjectSizeDivider;
    [Space(10f)]
    [SerializeField] float coinAnimationTime;
    [SerializeField] float coinScaleAmount;
    [SerializeField] LeanTweenType easeCurve;
    [Space(5f)]
    [SerializeField] List<Sprite> coinSprites;

    // Player info
    TMP_Text playerName;
    TMP_Text placeholderText;
    TMP_Text playerLevel;
    TMP_InputField playerNameInput;
    int lastPlayerLevel;

    // Level info
    Slider progress;
    TMP_Text progressText;
    int lastXP;

    // Coins
    Image coinIcon;
    TMP_Text coinText;

    bool waitingToStart = false;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        GetObjects();
        CoinScale();

        playerName.text = PlayerInfo.PlayerName;
        playerLevel.text = LevelCurve.CurrentLevel().ToString();

        progress.wholeNumbers = true;
        SetTexts();

        StartCoroutine(DB_Vars_Update());

        yield return new WaitForSeconds(0.25f);
        GameObjectSizeDivider = (GameObjectSizeDivider <= 2f) ? 2f : GameObjectSizeDivider;
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, Screen.height / GameObjectSizeDivider);
        Debug.LogWarning("Setting playerinfo area height to " + (Screen.height / GameObjectSizeDivider));
    }

    void OnEnable() {
        SetTexts();

        if (PlayerInfo.Level == -1 || PlayerInfo.XP == -1) {
            PlayerInfo.Load();
            SetTexts();
            lastPlayerLevel = PlayerInfo.Level;
            lastXP = PlayerInfo.XP;
        }

        if (!CheckObjects()) {
            GetObjects();
        }

        if (waitingToStart) {
            StartCoroutine(CoinAnimation());
        }

        if (lastPlayerLevel != PlayerInfo.Level || lastXP != PlayerInfo.XP) {
            SetTexts();
            lastPlayerLevel = PlayerInfo.Level;
            lastXP = PlayerInfo.XP;
        }

        StartCoroutine(UpdateTextsRegularly());

        GameObjectSizeDivider = (GameObjectSizeDivider <= 2f) ? 2f : GameObjectSizeDivider;
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, Screen.height / GameObjectSizeDivider);
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
        Debug.LogWarning("Setting playerinfo area height to " + (Screen.height / GameObjectSizeDivider));
    }
    IEnumerator UpdateTextsRegularly() {
        while (true) {
            yield return new WaitForSeconds(0.5f);
            if (lastPlayerLevel != PlayerInfo.Level || lastXP != PlayerInfo.XP) {
                SetTexts();
                lastPlayerLevel = PlayerInfo.Level;
                lastXP = PlayerInfo.XP;
            }
        }
    }

    IEnumerator DB_Vars_Update() {
        DB_playerLevel = PlayerInfo.Level;
        DB_playerXP = PlayerInfo.XP;
        DB_xpTotalLevel = LevelCurve.TotalNextLevelXP();
        DB_xpNextLevel = LevelCurve.NextLevelXP();
        yield return new WaitForSeconds(1f);
        StartCoroutine(DB_Vars_Update());
    }


    public void SetTexts() {
        if (!CheckObjects()) {
            GetObjects();
            if (!CheckObjects()) { return; }
        }

        // Player name
        if (PlayerInfo.PlayerName != "") {
            placeholderText.text = PlayerInfo.PlayerName;
            playerName.text = PlayerInfo.PlayerName;
        } else {
            placeholderText.text = "Player name here...";
            playerName.text = "";
        }
        playerNameInput.onEndEdit.RemoveAllListeners();
        playerNameInput.onEndEdit.AddListener(delegate {
            PlayerInfo.PlayerName = playerName.text;
            PlayerInfo.Save();
        });

        // Level
        playerLevel.text = "lvl. " + PlayerInfo.Level.ToString();
        lastPlayerLevel = PlayerInfo.Level;

        // Coins
        coinText.text = PlayerInfo.Coins.ToString();

        // Progress
        progress.maxValue = LevelCurve.NextLevelXP();
        progress.value = LevelCurve.CurrentXP();
        if (progress.value < 0.07f * progress.maxValue) {
            progress.value += (int)(0.07f * progress.maxValue);
        }

        // Progress
        string total = "Total XP:  " + LevelCurve.TotalCurrentXP() + " / " + LevelCurve.TotalNextLevelXP();
        string current = "To next level:  " + LevelCurve.CurrentXP() + " / " + LevelCurve.NextLevelXP();

        Debug.Log("Total == " + total + "\nCurrent == " + current);

        LeanTween.cancel(progressText.gameObject);
        StopAllCoroutines();

        StartCoroutine(CoinAnimation());
        StartCoroutine(SetTextSizes());

        // Get canvas group
        CanvasGroup group = progressText.gameObject.GetComponent<CanvasGroup>();
        if (group == null) {
            group = progressText.gameObject.AddComponent<CanvasGroup>();
        }
        group.alpha = 1f;
        progressText.text = total;
        StartCoroutine(TweenText(total, current, group));
    }
    IEnumerator TweenText(string total, string current, CanvasGroup group) {
        LeanTweenType curve = LeanTweenType.easeInOutCubic;

        yield return new WaitForSeconds(5f);

        LeanTween.cancel(progressText.gameObject);
        LeanTween.value(progressText.gameObject, 1f, 0f, 0.5f)
        .setEase(curve)
        .setDelay(3f)
        .setOnUpdate((float value) => {
            group.alpha = value;
        })
        .setOnComplete(() => {
            progressText.text = (progressText.text == total) ? current : total;

            LeanTween.value(progressText.gameObject, 0f, 1f, 0.5f)
            .setEase(curve)
            .setOnUpdate((float value) => {
                group.alpha = value;
            })
            .setOnComplete(() => {
                StartCoroutine(TweenText(total, current, group));
            });
        });
    }
    IEnumerator SetTextSizes() {
        // Parent rects
        RectTransform coinParRect = coinText.transform.parent.GetComponent<RectTransform>();
        // Size ref rects
        RectTransform coinIconRect = coinIcon.gameObject.GetComponent<RectTransform>();
        // Rects
        RectTransform coinRect = coinText.transform.GetComponent<RectTransform>();

        // Set size ref sizes
        coinIconRect.sizeDelta = new Vector2(coinParRect.rect.height / 2.15f, coinIconRect.sizeDelta.y);

        // Reset their sizes
        float width = coinText.text.Length * (coinParRect.rect.height / 2.5f);
        width = Mathf.Min(width, 0.95f * coinParRect.rect.width - coinIconRect.sizeDelta.x);
        coinRect.sizeDelta = new Vector2(width, coinRect.sizeDelta.y);
        coinText.fontSizeMax = 1000f;

        // Wait
        yield return new WaitForEndOfFrame();

        // Set new positions
        float pos = 0.05f * coinParRect.rect.width + coinRect.rect.width;
        pos *= -1f;
        coinIconRect.anchoredPosition = new Vector2(pos, coinIconRect.anchoredPosition.y);
    }


    // Animate the coin moving
    IEnumerator CoinAnimation() {
        waitingToStart = false;

        float time = coinAnimationTime / (float)(2 * coinSprites.Count - 1);
        for (int i = 0; i < coinSprites.Count; ++i) {
            coinIcon.sprite = coinSprites[i];
            yield return new WaitForSeconds(time);
        }
        for (int i = coinSprites.Count-2; i >= 0; --i) {
            coinIcon.sprite = coinSprites[i];
            yield return new WaitForSeconds(time);
        }

        CoinScale();
    }
    void CoinScale() {
        LeanTween.scaleZ(coinIcon.gameObject, coinScaleAmount, coinAnimationTime)
        .setEase(easeCurve)
        .setOnComplete(() => {
            LeanTween.scaleZ(coinIcon.gameObject, 1f, coinAnimationTime/2f)
            .setEase(easeCurve)
            .setOnComplete(() => {
                coinIcon.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                waitingToStart = true;
            });
        });
    }


    // Get objects
    void GetObjects() {
        Transform trans = gameObject.transform;

        Transform info = trans.Find("Player").Find("Info");
        playerName = info.Find("Player Name").Find("Text").GetComponent<TMP_Text>();
        placeholderText = info.Find("Player Name").Find("Placeholder").GetComponent<TMP_Text>();
        playerLevel = info.Find("Level Text").GetComponent<TMP_Text>();
        playerNameInput = info.Find("Player Name").GetComponent<TMP_InputField>();

        Transform level = trans.Find("Player").Find("Level Progress");
        progress = level.GetComponent<Slider>();
        progressText = level.Find("Text").GetComponent<TMP_Text>();

        Transform coin = trans.Find("Currency");
        coinIcon = coin.GetComponentInChildren<Image>();
        coinText = coin.GetComponentInChildren<TMP_Text>();
    }
    // Makes sure everything is good
    bool CheckObjects() {
        if (playerName == null)   { return false; }
        if (placeholderText == null) { return false; }
        if (playerLevel == null)  { return false; }
        if (playerNameInput == null) { return false; }

        if (progress == null)     { return false; }
        if (progressText == null) { return false; }

        if (coinIcon == null)     { return false; }
        if (coinText == null)     { return false; }

        return true;
    }
}