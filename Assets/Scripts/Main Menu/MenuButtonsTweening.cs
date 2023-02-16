using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonsTweening : MonoBehaviour
{
    [SerializeField] GameObject ParentOfChildren;
    [SerializeField] List<GameObject> MenuButtons;
    [SerializeField] List<GameObject> IconObjects;
    [SerializeField] List<GameObject> Backgrounds;

    [SerializeField] LeanTweenType easeInCurve;
    [SerializeField] LeanTweenType easeOutCurve;
    [SerializeField] LeanTweenType rotationCurve;

    [SerializeField] float iconTweenInterval;
    [SerializeField] float iconTweenSpeed;
    [SerializeField] float betweenIndividualsSpeed;

    [SerializeField] LeanTweenType scaleCurve;
    [SerializeField] float loopTime;
    [SerializeField] float minScale;
    [SerializeField] float maxScale;

    [SerializeField] LeanTweenType backTweenEaseIn;
    [SerializeField] LeanTweenType backTweenEaseOut;
    [SerializeField] float backTweenSpeed;
    float backTweenTime;
    float lastSettingSpeed;

    bool reverse = false;


    void Awake() {
        StopAllCoroutines();
        StartCoroutine(Start());
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        backTweenTime = Settings.AnimSpeedMultiplier * backTweenSpeed;
        lastSettingSpeed = Settings.AnimSpeedMultiplier;

        UpdateLeftHanded();
        GetMenuButtons();
        GetIconObjects();
        StartCoroutine(TweenIconEnum());
        StartCoroutine(TweenButtons());
        FadeInButtons();
    }


    // ----- Icon tweening -----

    // Tweens each icon after a certain interval
    IEnumerator TweenIconEnum() {
        yield return new WaitForSeconds(2.5f * backTweenTime);

        float wait = betweenIndividualsSpeed * Settings.AnimSpeedMultiplier;

        while (true) {
            TweenIcon(0);
            yield return new WaitForSeconds(wait);
            TweenIcon(1);
            yield return new WaitForSeconds(wait);
            TweenIcon(2);
            yield return new WaitForSeconds(wait);
            TweenIcon(3);
            yield return new WaitForSeconds(wait);

            yield return new WaitForSeconds(iconTweenInterval);
        }
    }

    // Tweens the icons occasionally over time
    // Bounce left/right
    // Rotate time trials and settings
    void TweenIcon(int index) {
        // Get objects necessary
        if (IconObjects == null) {return; }
        if (IconObjects.Count == 0) { return; }
        GameObject icon = IconObjects[index];
        if (icon == null) { return; }

        // Get start and ending position
        Vector3 start = icon.GetComponent<RectTransform>().localPosition;
        Vector3 end = getEndPos(start);

        // Create the initial tween
        var tween = LeanTween.moveLocal(icon, end, 0.5f * iconTweenSpeed).setEase(easeInCurve);

        // Add rotation to minute hand for "Time Trials"
        if (MenuButtons[index].name == "Time Trials") {
            GameObject hand = IconObjects[index].transform.parent.Find("Minute Hand").gameObject;
            Vector3 handEnd = getEndPos(hand.GetComponent<RectTransform>().localPosition);
            tween.setOnStart(() => {
                LeanTween.moveLocal(hand, handEnd, 0.5f * iconTweenSpeed).setEase(easeInCurve);
                LeanTween.rotateAroundLocal(hand, new Vector3(0, 0, 1), 360f, 1.3f * iconTweenSpeed)
                .setEase(rotationCurve);
            });
        }
        // Add rotation to settings logo for "Settings"
        else if (MenuButtons[index].name == "Settings") {
            float off = 25f;
            float speed = (1.3f * iconTweenSpeed) / 5f;
            tween.setOnStart(() => {
                LeanTween.rotateAroundLocal(icon, new Vector3(0, 0, 1), off, 1.2f*speed)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() => {
                    LeanTween.rotateAroundLocal(icon, new Vector3(0, 0, 1), -(off+360f), 3.8f*speed)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setOnComplete(() => {
                        icon.transform.rotation = new Quaternion(0, 0, 0, 0);
                    });
                });
            });
        }
        // Add wiggle rotation to awards
        else if (MenuButtons[index].name == "Awards") {
            float off = 25f;
            float next = 2f * (360f / 5f);
            float speed = (1.2f * iconTweenSpeed) / 5f;
            tween.setOnStart(() => {
                LeanTween.rotateAroundLocal(icon, new Vector3(0, 0, 1), off, 2f*speed)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() => {
                    LeanTween.rotateAroundLocal(icon, new Vector3(0, 0, 1), -(off+next), 3f*speed)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setOnComplete(() => {
                        icon.transform.rotation = new Quaternion(0, 0, 0, 0);
                    });
                });
            });
        }

        // Add initial tween to sequence
        var seq = LeanTween.sequence();
        seq.append( tween );

        // Add move tween to end position
        tween = LeanTween.moveLocal(icon, start, iconTweenSpeed).setEase(easeOutCurve);
        // Gotta move the minute hand with the timer
        if (MenuButtons[index].name == "Time Trials") {
            GameObject hand = IconObjects[index].transform.parent.Find("Minute Hand").gameObject;
            Vector3 handStart = hand.GetComponent<RectTransform>().localPosition;
            tween.setOnStart(() => {
                LeanTween.moveLocal(hand, handStart, iconTweenSpeed).setEase(easeOutCurve);
            });
        }
        seq.append( tween );
    }


    // ----- Button tweening -----

    // Tween buttons to "breathe" in and out over time
    IEnumerator TweenButtons() {
        yield return new WaitForSeconds(backTweenTime);

        foreach (GameObject obj in MenuButtons) {
            RectTransform buttRect = obj.transform.Find("Button").GetComponent<RectTransform>();
            RectTransform textRect = buttRect.transform.Find("Text (TMP)").GetComponent<RectTransform>();

            var tween1 = LeanTween.value(minScale, maxScale, loopTime/2f)
            .setOnUpdate((float value) => {
                buttRect.localScale = new Vector3(value, value, buttRect.localScale.z);
                textRect.localScale = new Vector3(1f/value, 1f/value, textRect.localScale.z);
            })
            .setEase(scaleCurve)
            .setLoopPingPong();

            yield return new WaitForSeconds(betweenIndividualsSpeed * Settings.AnimSpeedMultiplier);
        }
    }


    // ----- Background tweening -----

    // Methods to start the fade buttons enum w/ specific delay
    public void FadeInButtons() {
        StartCoroutine(FadeAndMove(0.25f));
    }
    public void FadeInButtons(float delay) {
        StartCoroutine(FadeAndMove(delay));
    }

    // Moves the background bar from screen left to screen right
    // Once it reaches the center, however, turn on the menu button
    IEnumerator FadeAndMove(float delay) {
        yield return new WaitForEndOfFrame();

        // First, fade out all of the menu buttons
        for (int i = 0; i < MenuButtons.Count; ++i) {
            GameObject obj = MenuButtons[i];
            CanvasGroup grp = obj.GetComponent<CanvasGroup>();
            if (grp == null) {
                grp = obj.AddComponent<CanvasGroup>();
            }
            grp.alpha = 0f;

            Backgrounds[i].SetActive(false);
            RectTransform rect = Backgrounds[i].GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(-(Screen.width + 50f), rect.offsetMin.y);
        }

        yield return new WaitForSeconds(delay);

        backTweenTime = Settings.AnimSpeedMultiplier * backTweenSpeed;

        // Do each tween at a specific interval
        for (int i = 0; i < Backgrounds.Count; ++i) {
            // Makes useful variables
            GameObject back = Backgrounds[i];
            RectTransform rect = back.GetComponent<RectTransform>();
            GameObject menu = MenuButtons[i];

            float start = -(Screen.width + 50f);
            float end1 = 0;
            float end2 = Screen.width + 50f;

            // Move background to the center
            LeanTween.value(back, start, end1, backTweenTime)
            .setEase(backTweenEaseIn)
            .setOnStart(() => {
                back.SetActive(true);
            })
            .setOnUpdate((float value) => {
                RectTransformOffset.Left(rect, value);
                RectTransformOffset.Right(rect, -value);
            })
            // Turn on the button
            .setOnComplete(() => {
                // Add canvas group
                CanvasGroup canv = menu.GetComponent<CanvasGroup>();
                if (canv == null) {
                    canv = menu.AddComponent<CanvasGroup>();
                }
                // Turn it on, set alpha
                menu.SetActive(true);
                canv.alpha = 1f;

                // Move background to the right
                LeanTween.value(back, end1, end2, backTweenTime)
                .setEase(backTweenEaseIn)
                .setOnUpdate((float value) => {
                    RectTransformOffset.Left(rect, value);
                    RectTransformOffset.Right(rect, -value);
                })
                .setOnComplete(() => {
                    back.SetActive(false);
                });
            });

            yield return new WaitForSeconds(betweenIndividualsSpeed * Settings.AnimSpeedMultiplier);
        }
    }


    // ----- Utilities -----

    // Changes layout depending on left handed mode
    void UpdateLeftHanded() {
        reverse = Settings.LeftHanded ? true : false;
        foreach (GameObject obj in MenuButtons) {
            HorizontalLayoutGroup hor = obj.GetComponent<HorizontalLayoutGroup>();
            if (reverse) { hor.reverseArrangement = true; }
            else         { hor.reverseArrangement = false; }
        }
    }

    // Gets children of ParentOfChildren if MenuButtons is not defined
    void GetMenuButtons() {
        // Get objects from parent
        if (MenuButtons == null) {
            MenuButtons = new List<GameObject>();
            foreach (Transform child in ParentOfChildren.transform) {
                MenuButtons.Add(child.gameObject);
            }
        }
        if (MenuButtons.Count == 0) {
            foreach (Transform child in ParentOfChildren.transform) {
                MenuButtons.Add(child.gameObject);
            }
        }

        // Get background objects
        if (Backgrounds == null) {
            Backgrounds = new List<GameObject>();
            foreach (Transform child in ParentOfChildren.transform) {
                Backgrounds.Add( child.Find("Back Bar").gameObject );
            }
        }
        if (Backgrounds.Count == 0) {
            foreach (Transform child in ParentOfChildren.transform) {
                Backgrounds.Add( child.Find("Back Bar").gameObject );
            }
        }
    }

    void GetIconObjects() {
        IconObjects = new List<GameObject>();
        IconObjects.Add(findObject("Play").transform.Find("Logos").GetComponentInChildren<Image>().gameObject);
        IconObjects.Add(findObject("Quickplay").transform.Find("Logos").GetComponentInChildren<Image>().gameObject);
        IconObjects.Add(findObject("Awards").transform.Find("Logos").GetComponentInChildren<Image>().gameObject);
        IconObjects.Add(findObject("Settings").transform.Find("Logos").GetComponentInChildren<Image>().gameObject);

        for (int i = 0; i < IconObjects.Count; ++i) {
            if (IconObjects[i] == null) {
                Debug.Log("Could not find object w/ index " + i);
            }
        }
    }

    // Finds object with a certain name in MenuButtons
    GameObject findObject(string objName) {
        foreach (GameObject obj in MenuButtons) {
            if (obj.name == objName) {
                return obj;
            }
        }
        Debug.Log("Could not find child with name " + objName);
        return null;
    }

    Vector3 getEndPos(Vector3 startPos) {
        float x = startPos.x, y = startPos.y, z = startPos.z;
        Vector3 end = new Vector3(x, y, z);
        if (reverse) {
            end.x += (float)Screen.width * 0.05f;
        } else {
            end.x -= (float)Screen.width * 0.05f;
        }
        return end;
    }
}
