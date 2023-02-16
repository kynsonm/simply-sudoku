using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.UI;

// Used to search all IAPs for matching ones
[System.Serializable]
public class IAPSearch {
    public string name;
    public string ID;

    public override string ToString() {
        return name + ": [" + ID + "]--\"" + name + "\"";
    }
}

// All things that the IAP pack purchases
[System.Serializable]
public class IAPPack {
    // For displaying the packs
    public string name;
    [TextArea(minLines:3, maxLines:10)]
    public string info;
    public bool makePackInShop = false;

    // Actual things this pack purchases
    [Space(10f)]
    public string ID;
    public IAPTier cost;
    public List<IAPSearch> IAPs;
    public bool isUnlocked;

    public override string ToString() {
        string str = "IAPs in search:\n";
        foreach (IAPSearch iap in IAPs) { str += "--- " + iap.ToString() + "\n"; }
        return str;
    }

    public int Contains(string ID_search) {
        for (int i = 0; i < IAPs.Count; ++i) {
            IAPSearch iap = IAPs[i];
            if (iap.ID == ID_search) { return i; }
        }
        return -1;
    }
}

// Buyable pack of coins
[System.Serializable]
public class CoinPack {
    // For displaying the packs
    public string name;
    [TextArea(minLines:3, maxLines:10)]
    public string info;

    // Main information
    [Space(10f)]
    public string ID;
    public IAPTier cost;
    public int numCoins;
    public bool isUnlocked;

    public override string ToString() {
        return name + ": [Tier " + cost.ToString() + "]--\"" + numCoins + " coins\"";
    }
}

public class ShopMenuItem {
    // Main vars
    public GameObject itemObj;
    public bool isUnlocked;

    // Whuch unlock type it is
    public Unlockable unlock = null;
    public IAPPack iapPack = null;
    public CoinPack coinPack = null;

    // For coins text
    public TMP_Text youHaveCoinsText = null;
    public int lastCoinAmount;

    // Basic constructor
    public ShopMenuItem(GameObject itemObj_in, bool isUnlocked_in) {
        itemObj = itemObj_in;
        isUnlocked = isUnlocked_in;
        lastCoinAmount = PlayerInfo.Coins;
    }
}

public class ShopMenu : MonoBehaviour
{
    // Main vars
    [SerializeField] Transform ShopCanvas;
    [SerializeField] Transform ShopContent;
    [SerializeField] GameObject BuyablePrefab;
    List<ShopMenuItem> shopMenuItems = new List<ShopMenuItem>();
    RectTransform shopMenu;
    CanvasGroup shopMenuCanvas;
    RectTransform confirmMenu;
    Blur.BlurMaterial blurMaterial;
    Purchaser purchaserObject;

    // Buyable vars
    [Space(10f)]
    [SerializeField] List<IAPPack> buyablePacks;
    [SerializeField] List<CoinPack> coinPacks;
    [SerializeField] IAPPack comingSoonPack;
    [SerializeField] float widthMultiplier;
    float width;
    float lastAnchoredPosition = 0f;

    // For setting menu rect size
    [Space(10f)]
    [SerializeField] GameObject TopReference;
    [SerializeField] GameObject BottomReference;

    // For avoiding any unnecessary button clicks
    [SerializeField] List<GameObject> MakeUninteractable;

    // For tweening menu in and out
    [Space(10f)]
    [SerializeField] float animationMultiplier;
    [SerializeField] LeanTweenType easeInCurve;
    [SerializeField] LeanTweenType easeOutCurve;

    // Just {get;} for buyable packs
    public List<IAPPack> getBuyablePacks() {
        List<IAPPack> packs = new List<IAPPack>();
        foreach (var pack in buyablePacks) { packs.Add(pack); }
        return packs;
    }


    // ----- MONOBEHAVIOUR STUFF -----
    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (!GetObjects()) {
            Debug.Log("ShopMenu: GET OBJECTS IS BAD");
            yield break;
        }
        ShopCanvas.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        SetDimensions();
        yield return new WaitForEndOfFrame();
        StartCoroutine(UpdateLastPosition());
        StartCoroutine(CheckUnlocks());
        SetWidthAndLayoutGroup();
        ShopCanvas.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        CreateObjects();
    }
    // ----- end monobehaviour stuff -----


    // ----- SHOP MENU CONTENT DIMENSIONS AND SIZING -----

    public void SetupShopMenu() {
        CheckForUnlocks();

        string log = "RESETTING SHOP MENU\n";
        foreach (ShopMenuItem shopItem in shopMenuItems) {
            // Reset IAP Pack
            if (shopItem.iapPack != null && shopItem.itemObj != null) {
                SetupIAPPack(shopItem.itemObj, shopItem.iapPack);

                log += $" -- Setting up IAP Pack: {shopItem.iapPack.name}\n";
                foreach (var unl in shopItem.iapPack.IAPs) {
                    log += $" ---- Unlock: {unl.name}, unlocked? {Unlocks.isUnlocked(unl.ID).ToString()}\n";
                }
            }
            // Reset Unlockables
            else if (shopItem.unlock != null && shopItem.itemObj != null) {
                SetupUnlockable(shopItem.itemObj, shopItem.unlock);

                log += $" -- Setting up Unlockable: {shopItem.unlock.name} -- Unlocked? ${shopItem.unlock.isUnlocked.ToString()}\n";
            }
        }
        Debug.Log(log);
    }

    // Sets the dimensions of the menu
    void SetDimensions() {
        // Set shop menu sizing
        RectTransform topRect = TopReference.GetComponent<RectTransform>();
        RectTransform botRect = BottomReference.GetComponent<RectTransform>();
        float y = botRect.position.y;
        float height = topRect.position.y - y;
        shopMenu.anchoredPosition = new Vector2(0f, y);
        shopMenu.sizeDelta = new Vector2(0f, height);

        // DEBUG STUFF
        string log = "ShopMenu: Setting up menu dimensions\n";
        log += $"topRect position: {topRect.position.ToString()}\n";
        log += $"botRect position: {botRect.position.ToString()}\n";
        log += $"Setting shop menu to y={y} and height={height}";
        Debug.Log(log);
    }
    void SetContentSize() {
        RectTransform rect = ShopContent.GetComponent<RectTransform>();
        HorizontalLayoutGroup hor = ShopContent.GetComponent<HorizontalLayoutGroup>();

        float size = hor.padding.left + hor.padding.right;
        size += (float)(ShopContent.childCount-1) * hor.spacing;
        size += (float)(ShopContent.childCount) * width;

        rect.sizeDelta = new Vector2(size, 0f);
        rect.anchoredPosition = new Vector2(lastAnchoredPosition, 0f);
    }
    IEnumerator UpdateLastPosition() {
        RectTransform rect = ShopContent.GetComponent<RectTransform>();
        while (true) {
            yield return new WaitForSeconds(0.5f);
            if (!ShopCanvas.gameObject.activeInHierarchy) { continue; }
            lastAnchoredPosition = rect.anchoredPosition.x;
        }
    }
    void SetWidthAndLayoutGroup() {
        float parWidth = ShopContent.parent.GetComponent<RectTransform>().rect.width;

        // Set buyable prefab width
        widthMultiplier = (widthMultiplier <= 0.05f) ? 0.05f : widthMultiplier;
        widthMultiplier = (widthMultiplier >= 1f) ? 1f : widthMultiplier;
        width = parWidth * widthMultiplier;

        // Set layout group
        HorizontalLayoutGroup hor = ShopContent.GetComponent<HorizontalLayoutGroup>();
        hor.padding.left = (int)((parWidth - width) / 2f);
        hor.padding.right = hor.padding.left;
        hor.spacing = hor.padding.left;
    }

    IEnumerator CheckUnlocks() {
        Unlockable[] unlockables = GameObject.FindObjectsOfType<Unlockable>();
        yield return new WaitForSeconds(1f);
        while (true) {
            CheckIAPPacks();
            yield return new WaitForSeconds(1f);
        }
    }
    void CheckIAPPacks() {
        foreach (IAPPack pack in buyablePacks) {
            if (pack.IAPs == null || pack.IAPs.Count == 0) {
                continue;
            }

            bool allUnlocked = true;
            foreach (var iap in pack.IAPs) {
                if (!Unlocks.isUnlocked(iap.ID)) { allUnlocked = false; }
            }
            pack.isUnlocked = allUnlocked;
        }
    }

    // Create buyable objects
    public void CreateObjects() {
        // Delete whats there already
        foreach (Transform child in ShopContent) {
            if (child.name == "Background") { continue; }
            GameObject.Destroy(child.gameObject);
        }
        shopMenuItems = new List<ShopMenuItem>();

        // Create the objects
        string log = "All unlocks in the scene:\n";
        log += CreateIAPPacks();
        log += CreateUnlockables();
        CreateComingSoon();
        log += CreateCoinPacks();
        SetContentSize();
        //Debug.Log(log);
    }

    string CreateUnlockables() {
        string log = "";
        foreach (Unlockable unlock in GameObject.FindObjectsOfType<Unlockable>(true)) {
            if (!unlock.isIAP) { continue; }
            log += unlock.unlockName + " " + unlock.ID + "\n";
            GameObject obj = CreateUnlockable(unlock);

            ShopMenuItem item = new ShopMenuItem(obj, unlock.isUnlocked);
            item.unlock = unlock;
            if (unlock.isCoin) {
                item.youHaveCoinsText = obj.transform.Find("Cost Area").Find("Coins").Find("You have").Find("Coin Text").GetComponent<TMP_Text>();
                if (item.youHaveCoinsText == null) { Debug.LogWarning("UMMM IDK"); }
            }
            shopMenuItems.Add(item);
        }
        return log;
    }
    string CreateIAPPacks() {
        string log = "";
        foreach (IAPPack pack in buyablePacks) {
            if (!pack.makePackInShop) { continue; }

            log += pack.ToString() + "\n";
            GameObject obj = CreateIAPPack(pack);

            ShopMenuItem item = new ShopMenuItem(obj, pack.isUnlocked);
            item.iapPack = pack;
            shopMenuItems.Add(item);
        }
        return log;
    }
    string CreateCoinPacks() {
        string log = "";
        foreach (CoinPack pack in coinPacks) {
            log += pack.ToString() + "\n";
            GameObject obj = CreateCoinPack(pack);

            ShopMenuItem item = new ShopMenuItem(obj, false);
            item.coinPack = pack;
            shopMenuItems.Add(item);
        }
        return log;
    }
    void CreateComingSoon() {
        GameObject obj = CreateIAPPack(comingSoonPack);
    }

    // Check each object if it has been unlocked
    void CheckForUnlocks() {
        foreach (ShopMenuItem item in shopMenuItems) {
            if (item.unlock != null) {
                if (item.isUnlocked != item.unlock.isUnlocked) {
                    item.isUnlocked = item.unlock.isUnlocked;
                    TweenAlpha(item.itemObj.transform.Find("Unlocked").gameObject);
                }
                if (PlayerInfo.Coins != item.lastCoinAmount && item.youHaveCoinsText != null) {
                    item.lastCoinAmount = PlayerInfo.Coins;
                    item.youHaveCoinsText.text = PlayerInfo.Coins.ToString();
                }
            }
            if (item.iapPack != null && item.isUnlocked != item.iapPack.isUnlocked) {
                item.isUnlocked = item.iapPack.isUnlocked;
                TweenAlpha(item.itemObj.transform.Find("Unlocked").gameObject);
            }
        }
    }
    void TweenAlpha(GameObject obj) {
        CanvasGroup canv = obj.AddComponent<CanvasGroup>();
        LeanTween.value(obj, 0f, 1f, Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnStart(() => {
            canv.alpha = 0f;
            obj.SetActive(true);
        })
        .setOnUpdate((float value) => {
            canv.alpha = value;
        })
        .setOnComplete(() => {
            canv.alpha = 1f;
            Object.Destroy(canv);
        });
    }


    // Handle a purchase
    public void Purchase(IAPPack iapPack) {
        string log = "Purchasing IAP Pack: " + iapPack.name + "\n";

        iapPack.isUnlocked = true;

        // Check all unlocks in the scene for matching ID
        Unlockable[] allUnlocks = GameObject.FindObjectsOfType<Unlockable>(true);
        foreach (Unlockable unlock in allUnlocks) {
            // Check matching ID w/ all IAPs in the pack
            foreach (IAPSearch iap in iapPack.IAPs) {
                // If match, unlock it and set it up
                if (unlock.ID == iap.ID) {
                    log += $"-- Unlockable: [{unlock.ID}] - {unlock.unlockName}\n";
                    unlock.Unlock();
                    unlock.Setup();
                    break;
                }
            }
        }
        Unlocks.Save();
        SetupShopMenu();
        Debug.Log(log);
    }
    public void Purchase(CoinPack coins) {
        PlayerInfo.Coins += coins.numCoins;

        Debug.Log($"Purchasing CoinPack: Adding {coins.numCoins} to PlayerInfo");
    }
    public void Purchase(Unlockable unlock) {
        Unlocks.Save();
        SetupShopMenu();
    }


    // ----- INDIVIDUAL PURCHASER CREATOR -----

    GameObject CreateUnlockable(Unlockable unlockable) {
        GameObject obj = Instantiate(BuyablePrefab, ShopContent);
        SetupUnlockable(obj, unlockable);
        return obj;
    }
    void SetupUnlockable(GameObject unlockableObject, Unlockable unlockable) {
        Transform trans = unlockableObject.transform;
        trans.gameObject.name = unlockable.unlockName + " Buyable";
        RectTransform rect = trans.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 0f);

        // Title text
        TMP_Text title = trans.Find("Title").GetComponent<TMP_Text>();
        title.text = unlockable.unlockName;

        // Info text
        TMP_Text info = trans.Find("Info Text").GetComponent<TMP_Text>();
        info.text = unlockable.unlockDescription;

        // Coin texts
        Transform coinArea = trans.Find("Cost Area").Find("Coins");
        TMP_Text coinCost = coinArea.Find("Cost").Find("Coin Text").GetComponent<TMP_Text>();
        coinCost.text = unlockable.coinsAmount.ToString();
        TMP_Text coinHave = coinArea.Find("You have").Find("Coin Text").GetComponent<TMP_Text>();
        coinHave.text = PlayerInfo.Coins.ToString();

        // Money text
        TMP_Text moneyText = trans.Find("Cost Area").Find("Money Text").GetComponent<TMP_Text>();
        moneyText.text = "$" + CostOfIAPTier.CostOf(unlockable.IAPCost);

        // Unlocked background
        if (!unlockable.isUnlocked) {
            trans.Find("Unlocked").gameObject.SetActive(false);
        } else {
            CanvasGroup canvGroup = trans.gameObject.GetComponent<CanvasGroup>();
            if (canvGroup == null) {
                canvGroup = trans.gameObject.AddComponent<CanvasGroup>();
            }
            canvGroup.interactable = false;
            return;
        }

        // Button onClicks
        Transform buttonArea = trans.Find("Buttons");
        Button coinButt = buttonArea.Find("Coins Button").GetComponent<Button>();
        if (!unlockable.isCoin) {
            coinButt.gameObject.SetActive(false);
        } 
        else if (PlayerInfo.Coins < unlockable.coinsAmount) {
            coinButt.interactable = false;
        }
        else {
            coinButt.onClick.RemoveAllListeners();
            coinButt.onClick.AddListener(() => {
                if (PlayerInfo.Coins >= unlockable.coinsAmount) {
                    unlockable.Unlock();
                    PlayerInfo.Coins -= unlockable.coinsAmount;
                    CheckForUnlocks();
                }
            });
        }
        Button buyButton = buttonArea.Find("Buy Button").GetComponent<Button>();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => {
            purchaserObject.BuyProductID(unlockable.ID);
        });
    }

    GameObject CreateIAPPack(IAPPack iapPack) {
        GameObject obj = Instantiate(BuyablePrefab, ShopContent);
        SetupIAPPack(obj, iapPack);
        return obj;
    }
    void SetupIAPPack(GameObject packObject, IAPPack iapPack) {
        Transform trans = packObject.transform;
        trans.gameObject.name = iapPack.name + " IAPPack";
        RectTransform rect = trans.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 0f);

        // Title text
        TMP_Text title = trans.Find("Title").GetComponent<TMP_Text>();
        title.text = iapPack.name;

        // Info text
        TMP_Text info = trans.Find("Info Text").GetComponent<TMP_Text>();
        Unlockable[] unlockables = GameObject.FindObjectsOfType<Unlockable>(true);
        string infoStr = iapPack.info + "<align=\"left\">";
        bool firstOwnCheck = true;
        foreach (IAPSearch iap in iapPack.IAPs) {
            if (firstOwnCheck) {
                infoStr += "<line-height=130%>\n<line-height=100%><i>This pack includes:";
                firstOwnCheck = false;
            }
            foreach (Unlockable unl in unlockables) {
                if (unl.ID == iap.ID) {
                    infoStr += "\n \U00002022 " + unl.unlockName;
                }
            }
        }
        if (!firstOwnCheck) { infoStr += "</i>"; }
        firstOwnCheck = true;
        foreach (IAPSearch iap in iapPack.IAPs) {
            if (iapPack.isUnlocked) { break; }
            if (Unlocks.isUnlocked(iap.ID)) {
                if (firstOwnCheck) {
                    infoStr += "<line-height=130%>\n<line-height=100%><b>Note: You already own part of this pack!";
                    firstOwnCheck = false;
                }
                foreach (Unlockable unl in unlockables) {
                    if (unl.ID == iap.ID) {
                        infoStr += "\n \U00002022 " + unl.unlockName;
                    }
                }
            }
        }
        if (!firstOwnCheck) {
            infoStr += "\nUnfortunately, you would still have to pay full price for this</b>";
        }
        info.text = infoStr;

        // Turn off coin area
        GameObject coinArea = trans.Find("Cost Area").Find("Coins").gameObject;
        coinArea.SetActive(false);

        // Money text
        TMP_Text moneyText = trans.Find("Cost Area").Find("Money Text").GetComponent<TMP_Text>();
        moneyText.text = "$" + CostOfIAPTier.CostOf(iapPack.cost);
        if (iapPack.IAPs == null || iapPack.IAPs.Count == 0) {
            moneyText.gameObject.SetActive(false);
        }

        // Unlocked background
        if (!iapPack.isUnlocked) {
            trans.Find("Unlocked").gameObject.SetActive(false);
        } else {
            CanvasGroup canvGroup = trans.gameObject.GetComponent<CanvasGroup>();
            if (canvGroup == null) {
                canvGroup = trans.gameObject.AddComponent<CanvasGroup>();
            }
            canvGroup.interactable = false;
            return;
        }

        // Button stuffs
        Transform buttonArea = trans.Find("Buttons");
        buttonArea.Find("Coins Button").gameObject.SetActive(false);
        Button buyButton = buttonArea.Find("Buy Button").GetComponent<Button>();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => {
            purchaserObject.BuyProductID(iapPack.ID);
        });
        if (iapPack.IAPs == null || iapPack.IAPs.Count == 0) {
            buyButton.gameObject.SetActive(false);
        }
    }

    GameObject CreateCoinPack(CoinPack coinPack) {
        GameObject obj = Instantiate(BuyablePrefab, ShopContent);
        SetupCoinPack(obj, coinPack);
        return obj;
    }
    void SetupCoinPack(GameObject coinPackObject, CoinPack coinPack) {
        string name = coinPack.name;
        if (name == "") { name = coinPack.numCoins + " Coin Pack"; }

        Transform trans = coinPackObject.transform;
        trans.gameObject.name = name + " CoinPack";
        RectTransform rect = trans.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 0f);

        // Title text
        TMP_Text title = trans.Find("Title").GetComponent<TMP_Text>();
        title.text = name;

        // Info text
        TMP_Text info = trans.Find("Info Text").GetComponent<TMP_Text>();
        string color = "<#" + ColorUtility.ToHtmlStringRGB(Theme.text_accent) + ">";
        string text = "This will give you " + coinPack.numCoins + " coins\n";
        text += coinPack.info;
        info.text = text;

        // Turn off coin area
        GameObject coinArea = trans.Find("Cost Area").Find("Coins").gameObject;
        coinArea.SetActive(false);

        // Money text
        TMP_Text moneyText = trans.Find("Cost Area").Find("Money Text").GetComponent<TMP_Text>();
        moneyText.text = "$" + CostOfIAPTier.CostOf(coinPack.cost);

        // Unlocked background
        trans.Find("Unlocked").gameObject.SetActive(false);

        // Button stuffs
        Transform buttonArea = trans.Find("Buttons");
        buttonArea.Find("Coins Button").gameObject.SetActive(false);
        Button buyButton = buttonArea.Find("Buy Button").GetComponent<Button>();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => {
            purchaserObject.BuyProductID(coinPack.ID);
        });
    }


    // ----- TWEENING IN AND OUT -----

    // Turn menu on and off
    public void TurnOn() {
        TweenShopMenu(true);
    }
    public void TurnOff() {
        TweenShopMenu(false);
    }
    void TweenShopMenu(bool turnOn) {
        if (!GetObjects()) { return; }
        if (shopMenu.gameObject.LeanIsTweening()) { return; }

        GameObject menu = shopMenu.gameObject;
        float start = turnOn ? 0f : 1f;
        float end = turnOn ? 1f : 0f;
        LeanTweenType easeCurve = turnOn ? easeInCurve : easeOutCurve;

        Vector2 startPos = new Vector2(shopMenu.anchoredPosition.x, shopMenu.anchoredPosition.y);
        Vector2 endPos = new Vector2(shopMenu.anchoredPosition.x, shopMenu.rect.height/2f + startPos.y);

        LeanTween.value(menu, start, end, animationMultiplier * Settings.AnimSpeedMultiplier)
        .setEase(easeCurve)
        .setOnStart(() => {
            ShopCanvas.gameObject.SetActive(true);
            shopMenu.localScale = new Vector3(start, start, 1f);
            shopMenuCanvas.alpha = (turnOn) ? 0f : 1f;
            if (turnOn) {
                CheckIAPPacks();
                CheckForUnlocks();
                ShopContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(lastAnchoredPosition, 0f);
                blurMaterial.SetRadius(start);
            }
        })
        .setOnUpdate((float value) => {
            shopMenu.localScale = new Vector3(value, value, 1f);

            shopMenuCanvas.alpha = Mathf.Pow(value, 0.75f);
            blurMaterial.SetRadius(value);

            float p = 1f - value;
            shopMenu.anchoredPosition = startPos + p * endPos;
        })
        .setOnComplete(() => {
            shopMenu.anchoredPosition = startPos;
            shopMenuCanvas.alpha = 1f;
            ShopCanvas.gameObject.SetActive(turnOn);
        });

        TurnInteractables(!turnOn);
    }


    // ----- UTILITIES -----

    // Returns each unlockable type
    public List<CoinPack> CoinPacks() {
        List<CoinPack> packs = new List<CoinPack>();
        foreach (CoinPack pack in coinPacks) {
            packs.Add(pack);
        }
        return packs;
    }
    public List<IAPPack> IAPPacks() {
        List<IAPPack> packs = new List<IAPPack>();
        foreach (IAPPack pack in buyablePacks) {
            packs.Add(pack);
        }
        return packs;
    }

    // Returns index of ID if it is coin pack or IAP pack
    //   returns -1 otherwise
    public CoinPack FindCoinPack(string ID) {
        for (int i = 0; i < coinPacks.Count; ++i) {
            if (coinPacks[i].ID == ID) { return coinPacks[i]; }
        }
        return null;
    }
    public IAPPack FindIAPPack(string ID) {
        for (int i = 0; i < buyablePacks.Count; ++i) {
            if (buyablePacks[i].ID == ID) { return buyablePacks[i]; }
        }
        return null;
    }

    // Turn interactables on or off
    void TurnInteractables(bool on) {
        foreach (GameObject obj in MakeUninteractable) {
            CanvasGroup canv = obj.GetComponent<CanvasGroup>();
            if (canv == null) {
                canv = obj.AddComponent<CanvasGroup>();
            }
            canv.interactable = on;
            canv.blocksRaycasts = on;
        }
    }

    // Get shop menu, blur, etc
    bool GetObjects() {
        if (ShopCanvas == null) {
            Debug.LogWarning("ShopMenu: No Shop Canvas!");
            return false;
        }

        bool allGood = true;
        if (TopReference == null) {
            Debug.Log("ShopMenu: No top reference");
            allGood = false;
        }
        if (BottomReference == null) {
            Debug.LogWarning("ShopMenu: No bottom reference");
            allGood = false;
        }
        if (purchaserObject == null) {
            purchaserObject = GameObject.FindObjectOfType<Purchaser>(true);
            if (purchaserObject == null) {
                Debug.LogWarning("ShopMenu: No Purcahser object!");
                allGood = false;
            }
        }

        if (shopMenu == null) {
            shopMenu = ShopCanvas.Find("Menu").GetComponent<RectTransform>();
        }
        if (shopMenuCanvas == null) {
            shopMenuCanvas = shopMenu.gameObject.AddComponent<CanvasGroup>();
        }
        if (confirmMenu == null) {
            confirmMenu = ShopCanvas.Find("Confirmation").GetComponent<RectTransform>();
        }
        if (blurMaterial == null) {
            blurMaterial = Blur.FindMaterial(Blur.BlurType.UI);
        }

        animationMultiplier = (animationMultiplier <= 0.1f) ? 0.1f : animationMultiplier;
        return allGood;
    }
}
