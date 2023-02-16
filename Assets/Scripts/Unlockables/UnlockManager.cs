using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Events;
using TMPro;

public class UnlockManager : MonoBehaviour
{
    // Objects needed
    [SerializeField] GameObject purchaseDialogueCanvas;
    public GameObject lockedButtonPrefab;
    
    [Space(10f)][SerializeField] List<GameObject> MakeUninteractable;

    // Dialogue vars
    [Space(10f)]
    [SerializeField] string infoPrefix, infoSuffix;
    [SerializeField] LookType infoUnlockNameColor;

    // Tween vars
    [Space(10f)]
    [SerializeField] float animationSpeedMultiplier;
    [SerializeField] LeanTweenType tweenOnCurve, tweenOffCurve;
    [HideInInspector] public bool menuIsOpen;

    // Debug vars
    [Space(10f)]
    [SerializeField] Button DEBUG_PurchaseButton;

    
    // Turn canvas off at start
    IEnumerator Start() {
        Unlocks.Load();
        Unlocks.Save();

        animationSpeedMultiplier = (animationSpeedMultiplier <= 0.1f) ? 0.1f : animationSpeedMultiplier;

        yield return new WaitForEndOfFrame();

        purchaseDialogueCanvas.SetActive(false);
        menuIsOpen = false;

        StartCoroutine(UnlockSaverEnum());
    }


    // ----- METHODS -----

    // Start dialogue menu
    public void StartPurchase(Unlockable unlockable) {
        if (purchaseDialogueCanvas.transform.Find("Menu").gameObject.LeanIsTweening()) {
            return;
        }
        MakeInteractable(false);
        TweenMenu(true);
        StartCoroutine(SetObjects(unlockable));
    }
    // End dialogue menu
    public void EndPurchase() {
        if (purchaseDialogueCanvas.transform.Find("Menu").gameObject.LeanIsTweening()) {
            return;
        }
        TweenMenu(false);
        MakeInteractable(true);
    }


    // Save unlockables every second
    IEnumerator UnlockSaverEnum() {
        yield return new WaitForSeconds(2.5f);
        while (true) {
            Unlocks.Save();
            yield return new WaitForSeconds(5f);
        }
    }
    // Reset all unlocks to false
    public void DEBUG_ResetAllUnlocks() {
        Unlocks.DEBUG_ResetAllUnlocks();
    }


    // ----- SETUP OPTION BUTTONS -----

    // Sets up onClick for each button
    void SetAD(Button button, Unlockable unlockable) {
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            Ads.RewardedAd(() => {
                unlockable.Unlock();
                Unlocks.Save();
            });
        });
    }
    void SetCoins(Button button, Unlockable unlockable) {
        // Grey out button if its not affordable
        if (unlockable.coinsAmount >= PlayerInfo.Coins) {
            button.interactable = false;
            return;
        }

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            if (unlockable.coinsAmount <= PlayerInfo.Coins) {
                PlayerInfo.Coins -= unlockable.coinsAmount;
                unlockable.Unlock();
                Unlocks.Save();
            }
            this.EndPurchase();
        });
    }
    void SetIAP(Button button, Unlockable unlockable) {
        ShopMenu shop = GameObject.FindObjectOfType<ShopMenu>(true);
        Purchaser purchaser = GameObject.FindObjectOfType<Purchaser>(true);
        /*if (shop != null) {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                shop.TurnOn();
            });
        }
        else*/
        if (purchaser != null) {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                purchaser.BuyProductID(unlockable.ID);
            });
        } else {
            button.interactable = false;
        }
    }


    // Unlocks unlockable without checking conditions
    public void DEBUG_PURCHASE(Unlockable unlockable) {
        if (DEBUG_PurchaseButton == null) { return; }
        DEBUG_PurchaseButton.onClick.RemoveAllListeners();
        DEBUG_PurchaseButton.onClick.AddListener(() => {
            unlockable.Unlock();
            this.EndPurchase();
        });
    }


    // ----- SETUP DIALOGUE OBJECTS/TEXTS -----

    // Set texts of everything in the menu
    IEnumerator SetObjects(Unlockable unlockable) {
        // DEBUG ONLY
        DEBUG_PURCHASE(unlockable);

        // Get objects
        Transform menu = purchaseDialogueCanvas.transform.Find("Menu");
        TurnAllChildrenOn(menu);

        // Set set info text
        Color col = ThemeController.GetColorFromLookType(infoUnlockNameColor);
        string colStr = "<color=#" + ColorUtility.ToHtmlStringRGBA(col) + ">";
        string endCol = "</color>";
        TMP_Text info = menu.Find("Text").Find("Info").GetComponent<TMP_Text>();
        string text = infoPrefix + "\n" + colStr + unlockable.unlockName + endCol + "\n";
        text += /* infoSuffix + " " + */ unlockable.unlockDescription;
        info.text = text;

        // Get the other vars
        Transform options = menu.Find("Options").Find("Option Buttons");
        TMP_Text op1 = options.Find("Option 1").GetComponentInChildren<TMP_Text>();
        Button op1Button = op1.transform.parent.GetComponent<Button>();
        TMP_Text op2 = options.Find("Option 2").GetComponentInChildren<TMP_Text>();
        Button op2Button = op2.transform.parent.GetComponent<Button>();
        //
        Transform costMoney = menu.Find("Cost money");
        TMP_Text costMoneyAmount = costMoney.Find("Will cost").Find("Amount").GetComponent<TMP_Text>();
        Transform costArea = menu.Find("Cost coins");
        //
        Transform willCost = costArea.Find("Will cost");
        TMP_Text willCostAmount = willCost.Find("Amount").GetComponent<TMP_Text>();
        //
        Transform youHave = costArea.Find("You have");
        TMP_Text youHaveAmount = youHave.Find("Amount").GetComponent<TMP_Text>();

        VerticalEditor vert = menu.GetComponent<VerticalEditor>();
        string color = "<color=#" + ColorUtility.ToHtmlStringRGB( ThemeController.Half(Theme.color4, Theme.text_main, 0.6f) ) + ">";

        // Set cost text (text, coin?, amount)
        // Set option 1 (money or coins or ad)
        // Set option 2 (coins or ad or empty)
        switch (unlockable.GetUnlockableType()) {
            // Just ad:  Turn off cost area and option 2
            case Unlockable.Type.Ad: {
                costArea.gameObject.SetActive(false);
                costMoney.gameObject.SetActive(false);

                op1.text = "Ad";
                SetAD(op1Button, unlockable);
                op2.transform.parent.gameObject.SetActive(false);
                break;
            }
            // Ad + Coin:  Turn off <Cost money> area
            case Unlockable.Type.Ad_Coin: {
                willCostAmount.text = unlockable.coinsAmount.ToString();
                youHaveAmount.text = PlayerInfo.Coins.ToString();
                costMoney.gameObject.SetActive(false);

                text += color + "<line-height=110%>\n<line-height=100%>";
                text += "You can either watch an ad or pay with coins!</color>";
                info.text = text;

                op1.text = "Coins";
                SetCoins(op1Button, unlockable);
                op2.text = "Ad";
                SetAD(op2Button, unlockable);
                break;
            }
            // Just coin:  Turn off option 2 and <Cost money> area
            case Unlockable.Type.Coin: {
                willCostAmount.text = unlockable.coinsAmount.ToString();
                youHaveAmount.text = PlayerInfo.Coins.ToString();
                costMoney.gameObject.SetActive(false);

                op1.text = "Coins";
                SetCoins(op1Button, unlockable);
                op2.transform.parent.gameObject.SetActive(false);
                break;
            }
            // IAP + Coin:
            case Unlockable.Type.IAP_Coin: {
                willCostAmount.text = unlockable.coinsAmount.ToString();
                youHaveAmount.text = PlayerInfo.Coins.ToString();
                costMoneyAmount.text = "$" + CostOfIAPTier.CostOf(unlockable.IAPCost).ToString();

                text += color + "<line-height=110%>\n<line-height=100%>";
                text += "You can either pay with coins or with <i>real</i> money!</color>";
                info.text = text;

                op1.text = "Pay";
                SetIAP(op1Button, unlockable);
                op2.text = "Coins";
                SetCoins(op2Button, unlockable);
                break;
            }
            // IAP:  Turn off op2 and <You have> area
            case Unlockable.Type.IAP: {
                costArea.gameObject.SetActive(false);
                costMoneyAmount.text = CostOfIAPTier.CostOf(unlockable.IAPCost).ToString();

                op1.text = "Pay";
                SetIAP(op1Button, unlockable);
                op2.transform.parent.gameObject.SetActive(false);
                break;
            }
        }

        // Reset all their looks
        yield return new WaitForEndOfFrame();
        ResetAllLooks(menu);
    }


    // ----- TWEENING -----

    // Tween it in or out
    void TweenMenu(bool tweenIn) {
        menuIsOpen = tweenIn;

        // Get vars
        RectTransform rect = purchaseDialogueCanvas.transform.Find("Menu").GetComponent<RectTransform>();
        GameObject menu = rect.gameObject;
        if (menu.LeanIsTweening()) { return; }
        Blur.BlurMaterial blurMaterial = Blur.FindMaterial(Blur.BlurType.UI);

        float time = animationSpeedMultiplier * Settings.AnimSpeedMultiplier;
        float start = (tweenIn) ? 0f : 1f;
        float end = 1f - start;

        LeanTweenType tweenCurve = tweenIn ? tweenOnCurve : tweenOffCurve;

        // Tween scale
        LeanTween.value(menu, start, end, time)
        .setEase(tweenCurve)
        .setOnStart(() => {
            purchaseDialogueCanvas.SetActive(true);
            rect.localScale = new Vector3(start, start, 1f);
            blurMaterial.SetRadius(start);
        })
        .setOnUpdate((float value) => {
            rect.localScale = new Vector3(value, value, 1f);
            blurMaterial.SetRadius(value);
        })
        .setOnComplete(() => {
            purchaseDialogueCanvas.SetActive(tweenIn);
            rect.localScale = new Vector3(1f, 1f, 1f);
        });

        // Alpha vars
        float alphaTimeRatio = 0.65f;
        float delay = (tweenIn) ? 0f : ((1f-alphaTimeRatio) * time);
        CanvasGroup canv = menu.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = menu.AddComponent<CanvasGroup>();
        }

        // Tween alpha
        LeanTween.value(menu, start, end, alphaTimeRatio * time)
        .setEase(tweenCurve)
        .setDelay(delay)
        .setOnUpdate((float value) => {
            canv.alpha = value;
        });
    }


    // ----- UTILITY -----


    // Turns every object in <MakeUninteractable> on or off
    void MakeInteractable(bool turnOn) {
        foreach (GameObject obj in MakeUninteractable) {
            // Add canvas group
            CanvasGroup canv = obj.GetComponent<CanvasGroup>();
            if (canv == null) {
                canv = obj.AddComponent<CanvasGroup>();
            }

            // Turn it on or off
            canv.blocksRaycasts = turnOn;
            canv.interactable = turnOn;
        }
    }

    void ActivateAllLayoutGroups(bool turnOn) {
        ActLayoutRecursion(turnOn, purchaseDialogueCanvas.transform.Find("Menu"));
    }
    void ActLayoutRecursion(bool turnOn, Transform parent) {
        // Turn them off
        VerticalLayoutGroup vert = parent.GetComponent<VerticalLayoutGroup>();
        if (vert != null) {
            vert.childControlHeight = turnOn;
            vert.childControlWidth = turnOn;
        }
        HorizontalLayoutGroup hor = parent.GetComponent<HorizontalLayoutGroup>();
        if (hor != null) {
            hor.childControlHeight = turnOn;
            hor.childControlWidth = turnOn;
        }

        // Call this fxn again
        foreach (Transform child in parent) {
            ActLayoutRecursion(turnOn, child);
        }
    }

    // Turns every child of parent on
    void TurnAllChildrenOn(Transform parent) {
        parent.gameObject.SetActive(true);
        // Call this fxn again
        foreach (Transform child in parent) {
            TurnAllChildrenOn(child);
        }
    }

    // Resets the text and image themes of every object
    void ResetAllLooks(Transform parent) {
        // Reset text
        TextTheme text = parent.GetComponent<TextTheme>();
        if (text != null) { text.Reset(); }
        // Reset images
        ImageTheme img = parent.GetComponent<ImageTheme>();
        if (img != null) { img.Reset(); }
        // Reset children
        foreach (Transform child in parent) {
            ResetAllLooks(child);
        }
    }
}
