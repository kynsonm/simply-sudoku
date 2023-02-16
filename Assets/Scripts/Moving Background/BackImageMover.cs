using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BackImageMover : MonoBehaviour
{
    // ----- VARIABLES -----

    [SerializeField] public Transform BackgroundImagesHolder;
    [SerializeField] GameObject StaticImage;
    [SerializeField] GameObject BackgroundImagePrefab;

    [Space(10f)]
    [SerializeField] float LargestScreenDimensionMultiplier;
    [SerializeField] int   NumberOfImagesPerDimension;
    [SerializeField] float TweenTimeEachDimension;


    // Start is called before the first frame update
    void Awake() { Start(); }
    void Start()
    {
        ResetBackground(false);
    }

    // ----- TURN ON AND OFF -----

    public void Pause() {
        BackgroundImagesHolder.gameObject.LeanPause();
        BackgroundImagesHolder.gameObject.SetActive(false);
        foreach (Transform child in BackgroundImagesHolder) {
            child.gameObject.LeanPause();
            child.gameObject.SetActive(false);
        }
    }
    public void Resume() {
        BackgroundImagesHolder.gameObject.LeanResume();
        BackgroundImagesHolder.gameObject.SetActive(true);
        foreach (Transform child in BackgroundImagesHolder) {
            child.gameObject.LeanPause();
            child.gameObject.SetActive(true);
        }
    }


    // ----- METHODS -----

    // Do the thing(s)
    public void ResetBackground(bool tweenThem) {
        float time = Settings.AnimSpeedMultiplier;

        // Destroy what's already holder
        UnityAction a = () => {
            foreach (Transform child in BackgroundImagesHolder) {
                GameObject.Destroy(child.gameObject);
            }
        };
        // Add canvas group
        CanvasGroup canv = BackgroundImagesHolder.GetComponent<CanvasGroup>();
        if (canv == null) { canv = BackgroundImagesHolder.gameObject.AddComponent<CanvasGroup>(); }
        if (tweenThem) {
            a.Invoke();

            // Fade it out
            LeanTween.value(BackgroundImagesHolder.gameObject, 1f, 0f, time)
            .setEase(LeanTweenType.easeInOutCubic)
            .setOnStart(() => {
                //Debug.Log("Starting tween");
            })
            .setOnUpdate((float value) => {
                canv.alpha = value;
            })
            .setOnComplete(() => {
                //Debug.Log("ENDING TWEEN");
                foreach (Transform child in BackgroundImagesHolder) {
                    //Debug.Log("Deleing child " + child.name);
                    GameObject.Destroy(child.gameObject);
                }
            });
        } else {
            canv.alpha = 1f;
            foreach (Transform child in BackgroundImagesHolder) {
                //Debug.Log("Deleing child " + child.name);
                GameObject.Destroy(child.gameObject);
            }
        }

        // Set static background
        Image staticImg = StaticImage.GetComponent<Image>();
        if (tweenThem) {

        }
        staticImg.sprite = Background.backgroundSprite;
        staticImg.gameObject.SetActive( !Background.moveBackgroundImage );

        // Set moving background
        if (Background.moveBackgroundImage) {
            LeanTween.cancel(BackgroundImagesHolder.gameObject);
            CreateBackgroundImages(tweenThem);
            MoveBackgroundImages();
        }
    }

    // Create grid of backgrounds in a rough square shape
    void CreateBackgroundImages(bool tweenThem) {
        // Get some vars
        float size = LargestScreenDimensionMultiplier * Mathf.Max(Screen.height, Screen.width);

        // Get number of copies of the image needed in each dimension
        float cellSize = size / (float)NumberOfImagesPerDimension;

        // Get objects
        RectTransform rect = BackgroundImagesHolder.GetComponent<RectTransform>();
        GridLayoutGroup grid = BackgroundImagesHolder.GetComponent<GridLayoutGroup>();

        // Set up the grid
        grid.enabled = true;
        grid.cellSize = new Vector2(cellSize, cellSize);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = NumberOfImagesPerDimension;

        float rotation = 0f;

        // Create all the backgrounds
        int numToMake = NumberOfImagesPerDimension * NumberOfImagesPerDimension;
        for (int i = 0; i < numToMake; ++i) {
            // Create the new background images
            GameObject obj = Instantiate(BackgroundImagePrefab, BackgroundImagesHolder);
            obj.name = "Background Img " + (i+1);
            obj.GetComponentInChildren<Image>().sprite = Background.backgroundSprite;

            // Rotate it to be move diverse
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.localEulerAngles = new Vector3(0f, 0f, rotation);
            rotation = (rotation >= 270f) ? 0f : (rotation + 90f);
            /*if (Random.Range(0, 100) <= 33) {
                objRect.localScale = new Vector3(-1f, objRect.localScale.y, objRect.localScale.z);
            }
            if (Random.Range(0, 100) <= 33) {
                objRect.localScale = new Vector3(objRect.localScale.x, -1f, objRect.localScale.z);
            }*/
        }

        // Fade in the holder, or just turn it on
        CanvasGroup canv = BackgroundImagesHolder.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = BackgroundImagesHolder.gameObject.AddComponent<CanvasGroup>();
        }
        if (tweenThem) {
            LeanTween.value(BackgroundImagesHolder.gameObject, 0f, 1f, Settings.AnimSpeedMultiplier)
            .setEase(LeanTweenType.easeInOutCubic)
            .setOnUpdate((float value) => {
                canv.alpha = value;
            });
        } else {
            canv.alpha = 1f;
        }

        // Resize the holder
        rect.sizeDelta = new Vector2(size, size);

        GameObject.FindObjectOfType<CheckOnScreen>().Reset();
    }

    // Move the background images
    void MoveBackgroundImages() {
        GameObject obj = BackgroundImagesHolder.gameObject;
        RectTransform rect = BackgroundImagesHolder.GetComponent<RectTransform>();
        float halfX = rect.sizeDelta.x/2f, halfY = rect.sizeDelta.y/2f;
        float halfScreenX = Screen.width/2f, halfScreenY = Screen.height/2f;

        float startX = -halfX + halfScreenX;
        float endX   =  halfX - halfScreenX;
        float startY = -halfY + halfScreenY;
        float endY   =  halfY - halfScreenY;

        // Set initial position
        Vector2 pos = new Vector2(startX, startY);
        rect.anchoredPosition = pos;

        // Tween x
        LeanTween.value(obj, startX, endX, TweenTimeEachDimension)
        .setEase(LeanTweenType.easeInOutSine)
        .setLoopPingPong(int.MaxValue)
        .setOnUpdate((float value) => {
            pos.x = value;
            rect.anchoredPosition = pos;
        });

        // Tween y
        UnityAction tween = () =>
        LeanTween.value(obj, 0f, startY, 0.5f * TweenTimeEachDimension)
        .setEase(LeanTweenType.easeOutSine)
        .setOnUpdate((float value) => {
            pos.y = value;
        })
        .setOnComplete(() => {
            LeanTween.value(obj, startY, endY, TweenTimeEachDimension)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float value) => {
                pos.y = value;
            })
            .setOnComplete(() => {
                LeanTween.value(obj, endY, 0f, 0.5f * TweenTimeEachDimension)
                .setEase(LeanTweenType.easeInSine)
                .setOnUpdate((float value) => {
                    pos.y = value;
                });
            });
        });
        // And repeat the y tween
        StartCoroutine(RepeatAction(tween, 2f * TweenTimeEachDimension));
    }

    // Invokes <action> every <interval> seconds
    IEnumerator RepeatAction(UnityAction action, float interval) {
        while (true) {
            action.Invoke();
            yield return new WaitForSeconds(interval);
        }
    }
}
