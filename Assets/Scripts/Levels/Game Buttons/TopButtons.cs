using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static SoundClip;

public class TopButtons : MonoBehaviour
{
    [SerializeField] List<GameObject> uninteractableOnPause;
    [SerializeField] LeanTweenType easeInCurve, easeOutCurve;

    [Space(10f)]
    [SerializeField] GameObject MainMenu;
    [SerializeField] [TextArea(minLines:1, maxLines:5)]
    string mainMenuTitle, mainMenuText;
    [SerializeField] GameObject NextLevelButton;

    [Space(10f)]
    [SerializeField] GameObject SettingsButton;
    [SerializeField] GameObject SettingsMenu;

    [Space(10f)]
    [SerializeField] public GameObject Info;

    [Space(10f)]
    [SerializeField] GameObject Reset;
    [SerializeField] [TextArea(minLines:1, maxLines:5)]
    string resetTitle, resetText;

    [Space(10f)]
    [SerializeField] GameObject Pause;
    [SerializeField] GameObject PauseInfoMenu;
    CanvasGroup pauseInfoMenuCanvas;
    [SerializeField] Sprite playSprite, pauseSprite;
    Image pauseButtonImage;
    

    // Start is called before the first frame update
    void Start()
    {
        CheckButtons();
        SetOnClicks();

        CanvasGroup group = NextLevelButton.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        StartCoroutine(CheckGame());
    }

    // Replace Update to just be called every 0.25 seconds
    IEnumerator CheckGame() {
        while (true) {
            yield return new WaitForSeconds(0.25f);

            // Get pause button image
            if (pauseButtonImage == null) {
                pauseButtonImage = Pause.GetComponent<Button>().image;
            }
            // Get pause info canvas group
            if (pauseInfoMenuCanvas == null) {
                pauseInfoMenuCanvas = PauseInfoMenu.GetComponent<CanvasGroup>();
                if (pauseInfoMenuCanvas == null) {
                    pauseInfoMenuCanvas = PauseInfoMenu.AddComponent<CanvasGroup>();
                }
            }

            // Do stuff depending on the state of the game
            if (GameManager.IsPaused) {
                if (pauseButtonImage.sprite != playSprite) {
                    Pause.GetComponent<Button>().image.sprite = playSprite;
                }
                if (pauseInfoMenuCanvas.alpha <= 0.1f && !PauseInfoMenu.LeanIsTweening()) {
                    RectTransform rect = PauseInfoMenu.GetComponent<RectTransform>();
                    float width = rect.rect.width;
                    rect.position = new Vector3(-width, rect.position.y, rect.position.z);

                    LeanTween.moveX(PauseInfoMenu, 0.5f * Screen.width, Settings.AnimSpeedMultiplier)
                    .setOnStart(() => {
                        pauseInfoMenuCanvas.alpha = 1f;
                    })
                    .setEase(easeOutCurve);
                }
            }
            else {
                if (pauseButtonImage.sprite != pauseSprite) {
                    Pause.GetComponent<Button>().image.sprite = pauseSprite;
                }
                if (pauseInfoMenuCanvas.alpha >= 0.9f && !PauseInfoMenu.LeanIsTweening()) {
                    RectTransform rect = PauseInfoMenu.GetComponent<RectTransform>();
                    float end = Screen.width + rect.rect.width;

                    LeanTween.moveX(PauseInfoMenu, end, 0.65f * Settings.AnimSpeedMultiplier)
                    .setOnComplete(() => {
                        pauseInfoMenuCanvas.alpha = 0f;
                    })
                    .setEase(easeInCurve);
                }
            }
        }
    }

    // Do this in script because I'm lazy
    void SetOnClicks() {
        SetClick( MainMenu,        () => { MainMenuClick();       } );
        SetClick( NextLevelButton, () => { NextLevel();           } );
        SetClick( SettingsButton,  () => { SettingsButtonClick(); } );
        SetClick( Info,            () => { InfoButtonClick();     } );
        SetClick( Reset,           () => { ResetButtonClick();    } );
        SetClick( Pause,           () => { PauseButtonClick();    } );
    }
    void SetClick(GameObject obj, UnityEngine.Events.UnityAction action) {
        Button butt = obj.GetComponent<Button>();
        if (butt == null) { return; }
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(action);
    }

    // Turn off all but menu button
    public void CompleteLevel() {
        Pause.GetComponent<Button>().interactable = false;
        SettingsButton.GetComponent<Button>().interactable = false;
        Info.GetComponent<Button>().interactable = false;
        Reset.GetComponent<Button>().interactable = false;

        CanvasGroup group = NextLevelButton.GetComponent<CanvasGroup>();
        LeanTween.value(NextLevelButton, 0f, 1f, 0.5f)
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnStart(() => {
            group.interactable = true;
            group.blocksRaycasts = true;
        })
        .setOnUpdate((float value) => {
            NextLevelButton.GetComponent<CanvasGroup>().alpha = value;
        });
    }


    // ----- Button Presses -----

    // Main Menu
    public void MainMenuClick() {
        if (GameManager.isClear || GameManager.isCompleted) {
            MainMenuAccept();
            Sound.Play(tap_something);
            return;
        }

        // Open confimarion dialogue
        GameObject popUp = PopUp.CreatePopUp(mainMenuTitle, mainMenuText, false);
        if (popUp == null) {
            Debug.Log("Pop up is null");
            Sound.Play(tap_nothing);
            return;
        }

        CheckCloseSettings();

        GameManager.GameBoard.Pause();
        GameManager.GameBoard.Save();

        Button butt = popUp.transform.Find("Bottom Buttons").Find("Yes").GetComponent<Button>();
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(() => {
            MainMenuAccept(butt.gameObject);
        });

        butt = popUp.transform.Find("Bottom Buttons").Find("No").GetComponent<Button>();
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(() => {
            MainMenuDecline();
        });
    }
    public void MainMenuAccept() {
        Debug.Log("Accepting main menu");
        GameManager.isClear = true;
        SceneLoader.LoadScene(SceneLoader.Scene.MainMenu);
        Sound.Play(confirm);
    }
    public void MainMenuAccept(GameObject obj) {
        Debug.Log("Accepting main menu");
        GameManager.isClear = true;
        SceneLoader.LoadScene(SceneLoader.Scene.MainMenu, obj);
        Sound.Play(confirm);
    }
    public void MainMenuDecline() {
        Debug.Log("Closing main menu");
        PopUp.ClosePopUp(() => GameManager.GameBoard.Resume());
        Sound.Play(decline);
    }


    // Next level
    void NextLevel() {
        Sound.Play(tap_something);

        // Action of calling the next level
        UnityAction a = () => {
            if (!LevelInfo.NextLevel()) {
                GameManager.isClear = true;
                SceneLoader.LoadScene(SceneLoader.Scene.MainMenu, NextLevelButton);
            } else {
                GameManager.isClear = true;
                SceneLoader.LoadScene(SceneLoader.Scene.Game, NextLevelButton);
            }
        };

        // Do success screen's "next level" fxn if possible
        SuccessScreen success = GameObject.FindObjectOfType<SuccessScreen>();
        if (success != null) {
            success.NextLevel(a);
        }
        // Otherwise, just change the scene without doing the ad
        else {
            a.Invoke();
        }
    }


    // Settings
    public void SettingsButtonClick() {
        GameSettings settings = GameObject.FindObjectOfType<GameSettings>();
        settings.Activate();

        Sound.Play(tap_something);
    }
    void CheckCloseSettings() {
        GameSettings settings = GameObject.FindObjectOfType<GameSettings>();
        if (settings.IsOpened()) {
            settings.Close(true);
        }
    }


    // Info
    public void InfoButtonClick() {
        GameObject.FindObjectOfType<GameInfo>().Activate();
        Sound.Play(info_button);
    }


    // Reset
    public void ResetButtonClick() {
        if (GameManager.isClear) {
            Sound.Play(tap_nothing);
            return;
        }
        CheckCloseSettings();

        Sound.Play(reset_button);

        if (!GameManager.IsPaused) {
            GameManager.GameBoard.Pause();
            Uninteractable(true);
        }
        GameObject obj = PopUp.CreatePopUp(resetTitle, resetText, false);
        
        Transform butts = obj.transform.Find("Bottom Buttons");
        GameObject accept = butts.Find("Yes").gameObject;
        butts.Find("Yes").GetComponent<Button>().onClick.AddListener(() => {
            ResetAccept(accept);
        });
        butts.Find("No").GetComponent<Button>().onClick.AddListener(() => {
            ResetDecline();
        });
    }
    public void ResetAccept(GameObject acceptButton) {
        Sound.Play(reset_button);
        GameManager.GameBoard.Reset();
        SceneLoader.LoadScene(SceneLoader.Scene.Game, acceptButton, 0.65f);
    }
    public void ResetDecline() {
        Sound.Play(decline);
        PopUp.ClosePopUp();
        Uninteractable(false);
        GameManager.GameBoard.Resume();
    }


    // Pause
    public void PauseButtonClick() {
        Button butt = Pause.GetComponent<Button>();

        GameSettings settings = GameObject.FindObjectOfType<GameSettings>(true);
        if (settings.IsOpened()) {
            settings.Close(true);
        }

        GameInfo info = GameObject.FindObjectOfType<GameInfo>(true);
        if (info.isOpen()) {
            info.TurnOff(true);
        }

        if (!GameManager.IsPaused) {
            GameManager.GameBoard.Pause();
            butt.image.sprite = playSprite;
        }
        else {
            GameManager.GameBoard.Resume();
            butt.image.sprite = pauseSprite;
        }

        Uninteractable(GameManager.IsPaused);
    }

    void Uninteractable(bool pausing) {
        foreach (GameObject obj in uninteractableOnPause) {
            // Get canvas group
            CanvasGroup canv = obj.GetComponent<CanvasGroup>();
            if (canv == null) {
                canv = obj.AddComponent<CanvasGroup>();
            }

            // Turn on/off
            canv.interactable = !pausing;
            canv.blocksRaycasts = !pausing;
        }
    }


    // ----- Utilities -----

    bool CheckButtons() {
        bool allGood = true;
        allGood = checkObject(MainMenu, nameof(MainMenu) );
        allGood = checkObject(SettingsButton, nameof(SettingsButton) );
        allGood = checkObject(Info,     nameof(Info)     );
        allGood = checkObject(Reset,    nameof(Reset)    );
        allGood = checkObject(Pause,    nameof(Pause)    );
        return allGood;
    }

    bool checkObject(GameObject obj, string name) {
        if (obj == null) {
            Debug.Log("No " + name + " object");
            return false;
        } else {
            if (obj.GetComponent<Button>() == null) {
                Debug.Log("No " + name + " button");
                return false;
            }
        }
        return true;
    }
}
