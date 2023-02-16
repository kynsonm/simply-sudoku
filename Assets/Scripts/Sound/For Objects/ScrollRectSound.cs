using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static SoundClip;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectSound : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    // Info
    new string name;

    // Objects
    ScrollRect scrollRect;
    public AudioSource audioSource;

    // Variables
    float volume = 0f;
    float time = 0f;
    float maxSpeed = 0f;

    float interval = 0.15f;
    bool fadingOut = false;


    // ----- METHODS -----

    // Set onValueChanged for ScrollRect
    void SetUpScrollRect() {
        if (!checkObjects()) { return; }
    }

    public void OnBeginDrag(PointerEventData _EventData)
    {
        Debug.Log("CALLING ON BEGIN DRAG");

        if (!checkObjects()) { return; }
        gameObject.LeanCancel();

        // Set volume and play it
        volume = Settings.SoundVolume * Settings.MasterVolume * Sound.GlobalVolumeMultiplier;
        audioSource.volume = volume;
        if (!audioSource.isPlaying && (scrollRect.horizontal || scrollRect.vertical)) {
            audioSource.time = time;
            audioSource.Play();
        }

        maxSpeed = 0f;
        StartCoroutine(OnDrag());
    }
 
    IEnumerator OnDrag()
    {
        yield return new WaitForSeconds(interval);
        while (true) {
            Debug.Log("CALLING ON DRAG");

            if (!(scrollRect.horizontal || scrollRect.vertical)) { yield break; }
            if (!audioSource.isPlaying) { audioSource.Play(); }
            if (scrollRect.velocity.magnitude > maxSpeed) { maxSpeed = scrollRect.velocity.magnitude; }

            float multiplier = Mathf.Min(1f, scrollRect.velocity.magnitude / (0.4f * maxSpeed));
            audioSource.volume = volume * multiplier;

            yield return new WaitForSeconds(interval);
        }
    }
 
    public void OnEndDrag(PointerEventData _EventData)
    {
        Debug.Log("CALLING ON END DRAG");
        maxSpeed = 0f;
        StopAllCoroutines();
        FadeOut();
    }


    IEnumerator CheckFadeOut() {
        while (true) {
            if (!Input.anyKey && !fadingOut) {
                audioSource.Pause();
                yield break;
            }
            yield return new WaitForSeconds(interval);
        }
    }
    void FadeOut() {
        gameObject.LeanCancel();
        StopAllCoroutines();

        LeanTween.value(gameObject, 1f, 0f, 1f)
        .setEase(LeanTweenType.easeInOutSine)
        .setOnStart(() => {
            fadingOut = true;
        })
        .setOnUpdate((float value) => {
            audioSource.volume = volume * value;
        })
        .setOnComplete(() => {
            time = audioSource.time;
            audioSource.Pause();
            fadingOut = false;
        });
    }


    // ----- UTILITIES -----

    // Sets up all the objects needed
    bool checkObjects() {
        // Check for scroll rect on this object
        if (scrollRect == null) {
            scrollRect = gameObject.GetComponent<ScrollRect>();
            if (scrollRect == null) {
                Debug.LogWarning("No scroll rect on this object");
                return false;
            }
        }
        // Check for AudioSource
        if (audioSource == null) {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null) {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = Sound.Clip(drag_menu);
            audioSource.loop = true;
        }
        return true;
    }


    // ----- Monobehaviour methods -----
    void Awake() { Start(); }
    void OnEnable() { Start(); }
    void Start()
    {
        name = "(" + transform.parent.parent.name + " --> " + transform.parent.name + " --> " + transform.name + ")";

        SetUpScrollRect();
        StopAllCoroutines();
        StartCoroutine(CheckFadeOut());
    }
}