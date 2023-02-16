using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using static SoundClip;

public class GameHint : MonoBehaviour
{
    [SerializeField] GameObject HintMenu;
    [SerializeField] GameObject HintButton;
    [SerializeField] LeanTweenType easeCurve;

    [Space(10f)]
    [SerializeField] GameObject BlurCanvas;
    [SerializeField] List<GameObject> makeUninteractable;

    [Space(10f)]
    [SerializeField] TMP_Text coinText;
    [SerializeField] Image coinImage;
    [SerializeField] List<Sprite> coinSequence;

    [Space(10f)]
    [SerializeField] GameObject DoHintMenu;
    [SerializeField] GameObject BottomSizeReference;
    [SerializeField] GameObject TopCloseReference;
    [SerializeField] List<GameObject> DoHintUninteractable;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        HintMenu.transform.parent.gameObject.SetActive(true);
        DoHintMenu.transform.parent.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        SetOnClicks();
        SetDoHintOnClicks();
        SetDoHintObjects();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        HintMenu.transform.parent.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        DoHintMenu.transform.parent.gameObject.SetActive(false);
    }

    // ------------------------------------
    // ------ HINT BUTTON CLICK MENU ------
    // ------------------------------------

    // Starts the hint menu if possible
    public void HintButtonPress() {
        if (HintMenu.activeInHierarchy) {
            Close();
        } else {
            Open();
        }
    }

    // Open or close the menu, and tween it in or out
    public void Open() {
        if (HintMenu.LeanIsTweening())   { Sound.Play(tap_nothing); return; }
        if (HintMenu.activeInHierarchy)  { Sound.Play(tap_nothing); return; }
        if (GameManager.IsPaused)        { Sound.Play(tap_nothing); return; }
        Sound.Play(hint_button);
        Move(true);
    }
    public void Close() {
        if (HintMenu.LeanIsTweening())   { Sound.Play(tap_nothing); return; }
        if (!HintMenu.activeInHierarchy) { Sound.Play(tap_nothing); return; }
        Sound.Play(close);
        Move(false);
    }
    void Move(bool isOpening) {
        // Set initial dimensions and turn it on
        if (isOpening) {
            HintMenu.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 1f);
        }
        HintMenu.transform.parent.gameObject.SetActive(true);

        if (isOpening) { GameManager.GameBoard.Pause(); }
        else { GameManager.GameBoard.Resume(); }

        // Turn off layout group while moving the menu, then turn it back on
        VerticalLayoutGroup vert = HintMenu.GetComponent<VerticalLayoutGroup>();
        vert.enabled = false;

        // Get rects
        RectTransform rect = HintMenu.GetComponent<RectTransform>();
        RectTransform butt = HintButton.GetComponent<RectTransform>();

        // Get start and end position
        Vector2 start = isOpening ? butt.position : rect.position;
        Vector2 end = isOpening ? rect.position : butt.position;
        Vector3 posOnEnd = new Vector3(rect.position.x, rect.position.y, rect.position.z);
        rect.position = start;

        // Get canvas group
        CanvasGroup canv = HintMenu.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = HintMenu.AddComponent<CanvasGroup>();
        }
        float alphaStart = canv.alpha;

        float scaleStart = rect.localScale.x;

        // Tween blur
        Blur.BlurMaterial blurMaterial = Blur.FindMaterial(Blur.BlurType.UI);
        CanvasGroup blurCanvasGroup = BlurCanvas.GetComponent<CanvasGroup>();

        // Move from start to end
        // Fade & scale in/out
        LeanTween.value(HintMenu, 0f, 1f, Settings.AnimSpeedMultiplier)
        .setEase(easeCurve)
        .setOnStart(() => {
            BlurCanvas.SetActive(true);
            blurMaterial.SetRadius( isOpening ? 0f : 1f );
        })
        .setOnUpdate((float value) => {
            // Set position
            rect.position = start + value * (end - start);

            // Set scale and alpha
            float scale = isOpening ? value : (1f - value);
            if (isOpening) { canv.alpha = Mathf.Pow(scale, 0.75f); }
            else { canv.alpha = alphaStart * Mathf.Pow(scale, 0.75f); }

            Vector3 newScale = new Vector3(scale, scale, 1f);
            if (!isOpening) {
                float temp = scaleStart * scale;
                newScale = new Vector3(temp, temp, 1f);
            }
            rect.localScale = newScale;

            blurCanvasGroup.alpha = isOpening ? Mathf.Sqrt(value) : 1-value;
            blurMaterial.SetRadius(isOpening ? value : 1-value);
        })
        .setOnComplete(() => {
            rect.position = posOnEnd;
            vert.enabled = true;
            if (!isOpening) {
                HintMenu.transform.parent.gameObject.SetActive(false);
                if (GameManager.IsPaused) {
                    GameManager.GameBoard.Resume();
                }
                BlurCanvas.SetActive(false);
                blurMaterial.SetRadius(1f);
            }
        });

        // Turn objects on/off
        foreach (GameObject obj in makeUninteractable) {
            CanvasGroup off = obj.GetComponent<CanvasGroup>();
            if (off == null) {
                off = obj.AddComponent<CanvasGroup>();
            }
            off.interactable = !isOpening;
            off.blocksRaycasts = !isOpening;
        }

        // Set coin amount text
        coinText.text = PlayerInfo.Coins.ToString();
        Button yes = HintMenu.transform.Find("Buttons").Find("Yes").GetComponent<Button>();
        if (PlayerInfo.Coins < 50) {
            yes.interactable = false;
        } else {
            yes.interactable = true;
        }
    }


    // Set onClicks for close, yes, and no buttons
    void SetOnClicks() {
        // Setup close button
        Button close = HintMenu.transform.parent.Find("Close Button").GetComponent<Button>();
        close.onClick.RemoveAllListeners();
        close.onClick.AddListener(() => {
            Close();
            GameManager.GameBoard.Resume();
        });

        // Get yes and no buttons
        Button yes = HintMenu.transform.Find("Buttons").Find("Yes").GetComponent<Button>();
        Button no = HintMenu.transform.Find("Buttons").Find("No").GetComponent<Button>();
        Button ad = HintMenu.transform.Find("Ad Button").GetComponentInChildren<Button>();

        // Set no onClick
        no.onClick.RemoveAllListeners();
        no.onClick.AddListener(() => {
            Close();
            GameManager.GameBoard.Resume();
        });

        // Set ad onClick
        ad.onClick.RemoveAllListeners();
        ad.onClick.AddListener(() => {
            Ads.RewardedAd(() => {
                PlayerInfo.Coins += 50;
                OpenDoHint(HintMenu);
            });
        });

        // Set yes onClick
        yes.onClick.RemoveAllListeners();
        yes.onClick.AddListener(() => {
            if (PlayerInfo.Coins < 50) { return; }
            OpenDoHint(HintMenu);
        });
    }


    // -------------------------------------
    // -------- HINT SELECTION MENU --------
    // -------------------------------------

    void OpenDoHint(GameObject posReference) {
        if (DoHintMenu.activeInHierarchy) { return; }
        else { DoHintMenu.transform.parent.gameObject.SetActive(true); }
        MoveDoHint(true, posReference);
    }
    void CloseDoHint() {
        Debug.Log("Closing do hint menu");
        if (!DoHintMenu.activeInHierarchy) { return; }
        MoveDoHint(false, HintButton);
    }
    void MoveDoHint(bool isOpening, GameObject posReference) {
        // Get rects / vars
        RectTransform rect = DoHintMenu.GetComponent<RectTransform>();
        RectTransform refRect = posReference.GetComponent<RectTransform>();
        float start = refRect.position.y;
        float end = rect.position.y;

        float startX = rect.position.x;
        float endX = refRect.position.x;

        // Get canvas groups
        CanvasGroup canv = rect.gameObject.GetComponent<CanvasGroup>();
        if (canv == null) { canv = rect.gameObject.AddComponent<CanvasGroup>(); }
        CanvasGroup refCanv = posReference.GetComponent<CanvasGroup>();
        if (refCanv == null) { refCanv = posReference.AddComponent<CanvasGroup>(); }

        // Tween it
        LeanTween.value(DoHintMenu, 0f, 1f, Settings.AnimSpeedMultiplier)
        .setEase(easeCurve)
        .setOnUpdate((float value) => {
            // Set new y
            float pos = start + value * (end - start);
            // Move both doHint and reference down
            if (isOpening) {
                rect.position = new Vector3(rect.position.x, pos, rect.position.z);
                refRect.position = new Vector3(refRect.position.x, pos, refRect.position.z);
                canv.alpha = value;
                refCanv.alpha = 1f - value;
            }
            // Just move doHint to reference
            else {
                pos = end + value * (start - end);
                float x = startX + value * (endX - startX);
                rect.position = new Vector3(x, pos, rect.position.z);
                canv.alpha = 1f - value;
                rect.localScale = new Vector3(1f - value, 1f - value, 1f);
            }
        })
        .setOnComplete(() => {
            // Reset positions and scale
            rect.position = new Vector3(startX, end, rect.position.z);
            rect.localScale = new Vector3(1f, 1f, 1f);
            refRect.position = new Vector3(refRect.position.x, start, refRect.position.z);
            Close();

            // Turn off
            if (!isOpening) { DoHintMenu.transform.parent.gameObject.SetActive(false); }

            // Turn uninteractables on or off
            LeanTween.value(0f, 1f, 0.1f * Settings.AnimSpeedMultiplier)
            .setOnComplete(() => {
                foreach (GameObject obj in DoHintUninteractable) {
                    CanvasGroup off = obj.GetComponent<CanvasGroup>();
                    if (off == null) {
                        off = obj.AddComponent<CanvasGroup>();
                    }
                    off.interactable = !isOpening;
                    off.blocksRaycasts = !isOpening;
                }
            });
        });
    }

    // Sets position of the DoHint menu and its top close button
    void SetDoHintObjects() {
        DoHintMenu.transform.parent.gameObject.SetActive(true);

        Debug.Log("Setting doHint dimensions");

        // Menu size / pos
        RectTransform menu = DoHintMenu.GetComponent<RectTransform>();
        float pad = (float)Screen.height * 0.01f;
        float y = BottomSizeReference.GetComponent<RectTransform>().position.y + pad;
        float height = BottomSizeReference.GetComponent<RectTransform>().rect.height - pad;
        menu.sizeDelta = new Vector2(0f, height);
        menu.anchoredPosition = new Vector2(0f, y);

        // Close size/pos
        RectTransform close = DoHintMenu.transform.parent.Find("Cancel Button").GetComponent<RectTransform>();
        float height2 = Screen.height - TopCloseReference.GetComponent<RectTransform>().position.y;
        close.sizeDelta = new Vector2(0f, height2);
        close.anchoredPosition = new Vector2(0f, 0f);

        StartCoroutine(SetDoHintSizesEnum());
    }
    IEnumerator SetDoHintSizesEnum() {
        yield return new WaitForEndOfFrame();

        // Back image size / pos
        RectTransform info = DoHintMenu.transform.Find("Info Text").GetComponent<RectTransform>();
        RectTransform buttons = DoHintMenu.transform.Find("Buttons").GetComponent<RectTransform>();
        RectTransform back = DoHintMenu.transform.Find("Back Image").GetComponent<RectTransform>();

        // average of info text and buttons position
        float y = 0.475f * info.localPosition.y + 0.525f * buttons.localPosition.y;
        Debug.LogWarning($"Local posiions == {info.localPosition.y} and {buttons.localPosition.y}");
        //back.localPosition = new Vector3(back.localPosition.x, y, back.localPosition.z);

        // size delta of their combined heights + some multiplier of average screen size
        float sizeY = info.position.y - buttons.position.y;
        float sizeX = info.sizeDelta.x;
        float add = 0.03f * ((Screen.height + Screen.width) / 2f);
        //back.sizeDelta = new Vector2(sizeX, sizeY) + new Vector2(2.5f * add, add);

        // Turn do hint menu off again
        DoHintMenu.transform.parent.gameObject.SetActive(false);

        // Reset PPU (cuz it gets messed up ig)
        ImageTheme accept = DoHintMenu.transform.Find("Buttons").Find("Accept").GetComponent<ImageTheme>();
        accept.PPUMultiplier += 0.01f;
        ImageTheme cancel = DoHintMenu.transform.Find("Buttons").Find("Cancel").GetComponent<ImageTheme>();
        cancel.PPUMultiplier += 0.01f;
    }

    // Sets the onClicks for each cancel, accept, and cancel buttons
    void SetDoHintOnClicks() {
        // Close button
        Button close = DoHintMenu.transform.GetComponentInChildren<Button>();
        close.onClick.RemoveAllListeners();
        close.onClick.AddListener(() => {
            CloseDoHint();
        });

        // Get buttons
        Button accept = DoHintMenu.transform.Find("Buttons").Find("Accept").GetComponent<Button>();
        Button cancel = DoHintMenu.transform.Find("Buttons").Find("Cancel").GetComponent<Button>();

        // Cancel button
        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(() => {
            CloseDoHint();
        });

        // Accept button
        accept.onClick.RemoveAllListeners();
        accept.onClick.AddListener(() => {
            DoHint();
        });
    }

    void DoHint() {
        // If no button selected, return
        if (!GameManager.GameBoard.buttonIsSelected) { return; }
        // If this spot is a solution, return
        if (GameManager.GameBoard.SelectedIsSolution()) { return; }
        
        // Otherwise, set it as solution
        GameManager.GameBoard.MakeSelectedSolution();
        // Remove 50 coins
        PlayerInfo.Coins -= 50;
        // Close do hint
        ++Statistics.hintsUsed;
        CloseDoHint();
    }
}
