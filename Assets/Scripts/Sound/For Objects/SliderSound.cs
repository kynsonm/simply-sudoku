using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static SoundClip;

[RequireComponent(typeof(Slider))]
public class SliderSound : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    // Info
    new string name;

    // Objects
    Slider slider;
    public AudioSource audioSource;

    // Variables
    [SerializeField] float lastSliderValue;
    [SerializeField] float volume = 0f;
    [SerializeField] float time = 0f;

    [SerializeField] float speed = 0f;
    [SerializeField] float maxSpeed = 0f;

    bool fadingOut = false;


    // ----- METHODS -----

    // Set onValueChanged for ScrollRect
    void SetUpSlider() {
        if (!checkObjects()) { return; }

        UnityAction<float> a = (float value) => {
            OnDrag();
        };

        slider.onValueChanged.RemoveListener(a);
        slider.onValueChanged.AddListener(a);
    }

    public void OnBeginDrag(PointerEventData _EventData)
    {
        if (!checkObjects()) { return; }

        // Set volume and play it
        volume = Settings.SoundVolume * Settings.MasterVolume * Sound.GlobalVolumeMultiplier;
        audioSource.volume = volume;
        if (!audioSource.isPlaying) {
            audioSource.time = time;
            audioSource.Play();
        }

        lastSliderValue = slider.value;
    }
 
    public void OnDrag()
    {
        // v = d / t
        speed = Mathf.Abs(slider.value - lastSliderValue) / Time.deltaTime;
        if (speed > maxSpeed && speed <= 5f) { maxSpeed = speed; }
        
        float multiplier = Mathf.Min(1f, speed / (0.75f * maxSpeed));
        audioSource.volume = getVolume() * multiplier;

        lastSliderValue = slider.value;
    }
 
    public void OnEndDrag(PointerEventData _EventData)
    {
        FadeOut();
    }


    IEnumerator CheckFadeOut() {
        while (true) {
            if (!Input.anyKey && !fadingOut) {
                audioSource.Pause();
                yield break;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }
    void FadeOut() {
        gameObject.LeanCancel();

        LeanTween.value(gameObject, 1f, 0f, 0.5f)
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

    float lastSoundVol, lastMasterVol;
    float getVolume() {
        if (lastSoundVol == Settings.SoundVolume && lastMasterVol == Settings.MasterVolume) { return volume; }
        lastSoundVol = Settings.SoundVolume;
        lastMasterVol = Settings.MasterVolume;
        return Settings.SoundVolume * Settings.MasterVolume * Sound.GlobalVolumeMultiplier;
    }

    // Sets up all the objects needed
    bool checkObjects() {
        // Check for scroll rect on this object
        if (slider == null) {
            slider = gameObject.GetComponent<Slider>();
            if (slider == null) {
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

        SetUpSlider();
        StopAllCoroutines();
        StartCoroutine(CheckFadeOut());
    }
}
