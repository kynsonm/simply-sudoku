using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
    [SerializeField] GameObject AchievementPopupPrefab;
    [SerializeField] Transform PopUpCanvas;

    [Space(10f)]
    [SerializeField] float widthDivider;
    [SerializeField] float heightDivider;
    [SerializeField] float yPosDivider;
    [SerializeField] float expandBlurMultiplier;

    [Space(10f)]
    [SerializeField] float moveTime;
    [SerializeField] float holdTime;
    [SerializeField] LeanTweenType easeCurve;

    class PopupObjects {
        public RectTransform popupRect;

        public RectTransform titleRect;
        public RectTransform achievementRect;
        public RectTransform achievedTextRect;

        public RectTransform achTextAreaRect;
        public RectTransform achTitleRect;
        public RectTransform achDescRect;
        public RectTransform achRewRect;
    }

    // Creates a popup
    public void Popup(Achievement ach) {
        // Turn on the canvas and turn off blur image if it has one
        PopUpCanvas.gameObject.SetActive(true);
        PopUpCanvas.GetComponent<CanvasGroup>().alpha = 1f;
        ActivateBlurImage(true);

        GameObject obj = Instantiate(AchievementPopupPrefab, PopUpCanvas);
        RectTransform rect = obj.GetComponent<RectTransform>();

        PopupObjects popupObjects = new PopupObjects();
        popupObjects.popupRect = rect;
        popupObjects.titleRect = obj.transform.Find("Title").GetComponent<RectTransform>();

        // Set position and size
        RectTransformOffset.Sides(rect, (float)Screen.width / widthDivider);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, (float)Screen.height / heightDivider);
        rect.position = new Vector3(Screen.width/2f, 0f, rect.position.z);

        // Expand back image
        RectTransform backRect = obj.transform.Find("Back Image").GetComponent<RectTransform>();
        float off = -((float)(Screen.height + Screen.width) / 2f) / 50f;
        RectTransformOffset.Sides(backRect, off);
        RectTransformOffset.Vertical(backRect, 0.5f * off);

        // Blur image
        float blurOff = off * expandBlurMultiplier;
        RectTransform blur = obj.transform.Find("Blur Image").GetComponent<RectTransform>();
        RectTransformOffset.Sides(blur, blurOff);
        RectTransformOffset.Vertical(blur, 0.75f * blurOff);

        // Set achievement info
        Transform achObj = obj.transform.Find("Achievement");
        popupObjects.achievementRect = achObj.GetComponent<RectTransform>();

        // Title text and size
        Transform achText = achObj.Find("Text");
        popupObjects.achTextAreaRect = achText.GetComponent<RectTransform>();
        TMP_Text titleText = achText.Find("Title").GetComponent<TMP_Text>();
        titleText.text = ach.name;
        popupObjects.achTitleRect = titleText.gameObject.GetComponent<RectTransform>();

        // Description text and size
        TMP_Text descText = achText.Find("Description").GetComponent<TMP_Text>();
        descText.text = ach.info;
        popupObjects.achDescRect = descText.gameObject.GetComponent<RectTransform>();

        // Reward text and size
        Transform rewArea = achText.Find("Reward");
        rewArea.Find("XP").GetComponent<TMP_Text>().text = ach.xp + " XP";
        rewArea.Find("Coins").GetComponent<TMP_Text>().text = ach.reward + " Coins";
        popupObjects.achRewRect = rewArea.gameObject.GetComponent<RectTransform>();

        // Icon
        Transform icon = achObj.Find("Image").Find("Icon");
        icon.GetComponentInChildren<TMP_Text>().text = ach.iconText;

        // Achieved text
        TMP_Text achievedText = obj.transform.Find("Achieved Text").GetComponent<TMP_Text>();
        achievedText.text = ach.achievedText;
        popupObjects.achievedTextRect = achievedText.gameObject.GetComponent<RectTransform>();

        StartCoroutine(SetPopupSizes(popupObjects));
        TweenPopup(obj);
    }

    IEnumerator SetPopupSizes(PopupObjects popupObjects) {
        yield return new WaitForEndOfFrame();

        // Find total achievement height w/out spacing stuff
        VerticalLayoutGroup vert = popupObjects.popupRect.gameObject.GetComponent<VerticalLayoutGroup>();
        float achHeight = popupObjects.popupRect.rect.height;
        achHeight -= (vert.padding.top + vert.padding.bottom);
        achHeight -= 2 * vert.spacing;

        // Set heights of the 3 popup areas
        float h1 = 0.2f * achHeight;
        float h2 = 0.4f * achHeight;
        float h3 = achHeight - h1 - h2;
        popupObjects.titleRect.sizeDelta = new Vector2(0f, h1);
        popupObjects.achievementRect.sizeDelta = new Vector2(0f, h2);
        popupObjects.achievedTextRect.sizeDelta = new Vector2(0f, h3);

        yield return new WaitForEndOfFrame();

        // Set heights of achievement info
        float height = popupObjects.achTextAreaRect.parent.GetComponent<RectTransform>().rect.height;
        float h4 = (1f / 3.75f) * height;
        float h5 = (1f / 5.00f) * height;
        popupObjects.achTitleRect.sizeDelta = new Vector2(0f, h4);
        popupObjects.achDescRect.sizeDelta = new Vector2(0f, 2f * h4);
        popupObjects.achRewRect.sizeDelta = new Vector2(0f, h5);

        Debug.Log($"Achievement height == {height}, h4 == {h4}, h5 == {h5}");
    }

    // Tweens it
    public void TweenPopup(GameObject popup) {
        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.position = new Vector3(rect.position.x, -50f, rect.position.z);

        CanvasGroup popupCanvasGroup = PopUpCanvas.GetComponent<CanvasGroup>();
        CanvasGroup group = popup.GetComponent<CanvasGroup>();
        group.alpha = 0.05f;

        // Position for achievement
        LeanTween.moveY(popup, Screen.height / yPosDivider, moveTime)
        .setEase(easeCurve)
        .setOnStart(() => {
            popupCanvasGroup.interactable = false;
            popupCanvasGroup.blocksRaycasts = false;
            LeanTween.value(popup, 0.05f, 1f, moveTime/5f)
            .setEase(easeCurve)
            .setOnUpdate((float value) => { group.alpha = value; });
        })
        .setOnComplete(() => {
            LeanTween.moveY(popup, -(rect.rect.height + 50f), moveTime)
            .setDelay(holdTime)
            //.setDelay(20*holdTime)
            .setEase(easeCurve)
            .setOnStart(() => {
                LeanTween.value(popup, 1f, 0f, 0.2f * moveTime)
                .setEase(easeCurve)
                .setDelay(0.8f * moveTime)
                .setOnUpdate((float value) => { group.alpha = value; });
            })
            .setOnComplete(() => {
                GameObject.Destroy(popup);
                popupCanvasGroup.interactable = true;
                popupCanvasGroup.blocksRaycasts = true;
                PopUpCanvas.gameObject.SetActive(false);

                ActivateBlurImage(false);
            });
        });
    }

    // Turn on/off blur image
    public void ActivateBlurImage(bool turnOff) {
        if (PopUpCanvas.GetComponentInChildren<Image>() != null) {
            Image img = PopUpCanvas.GetComponentInChildren<Image>();
            if (img.material != null && img.material.name.Contains("Blur")) {
                img.gameObject.SetActive(!turnOff);
            }
        }
    }

    // Tests it
    public void TestPopup() {
        Achievement ach = new Achievement();
        Popup(ach);
    }
}
