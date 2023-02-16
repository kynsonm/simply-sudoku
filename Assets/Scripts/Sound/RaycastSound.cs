using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaycastSound : MonoBehaviour
{
    enum MouseButton {
        primary = 0, secondary = 1, middle = 2
    }

    void Start()
    {
        Canvas[] canvs = GameObject.FindObjectsOfType<Canvas>(true);
        if (canvases != null) {
            canvases.Clear();
        }
        foreach (Canvas canv in canvs) {
            canvases.Add(canv);
        }
        UpdateRaycastSound();
    }

    // Update is called once per frame
    void UpdateRaycastSound()
    {
        StartCoroutine(UpdateRaycastSoundEnum());
    }
    IEnumerator UpdateRaycastSoundEnum() {
        while (true) {
            // Wait while nothings happening
            if (enumRunning) { 
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            // Check for inputs
            if (Input.touchCount > 0) {
                StartCoroutine(Cast(true));
            }
            if (Input.GetMouseButtonDown((int)MouseButton.primary)) {
                StartCoroutine(Cast(false));
            }
            yield return new WaitForEndOfFrame();
        }
    }

    [SerializeField] List<string> canvasNamesToSkip;
    EventSystem eventSystem;
    List<Canvas> canvases = new List<Canvas>();
    bool enumRunning = false;

    IEnumerator Cast(bool isTouch) {
        enumRunning = true;

        bool makeTap = true;
        bool makeDrag = false;

        List<RaycastResult> hits = new List<RaycastResult>();
        hits = RaycastAllCanvases(isTouch);

        string log = "Raycast found " + hits.Count + " objects:\n";
        foreach (var hit in hits) {
            log += hit.gameObject.name + ", ";
        }

        foreach (RaycastResult hit in hits) {
            GameObject obj = hit.gameObject;

            // Check for anything that makes its own sound
            if (obj.GetComponent<Button>() != null) {
                makeTap = false;
            }
            if (obj.GetComponent<ScrollRect>() != null) {
                makeDrag = true;
            }
        }

        // If no important collisions, make tap sound
        if (makeTap) {
            Sound.Play(SoundClip.tap_nothing);
        }

        // While inputs are still detected
        while (Input.touchCount > 0) {
            if (makeDrag) {
                //Sound.Play(SoundClip.drag_menu);
            }
            yield return new WaitForEndOfFrame();
        }

        enumRunning = false;
    }

    // Returns results of raycast against all canvses
    List<RaycastResult> RaycastAllCanvases(bool isTouch) {
        // Find an event system
        if (eventSystem == null) {
            eventSystem = GameObject.FindObjectOfType<EventSystem>();
            if (eventSystem == null) {
                Debug.LogError("No event system in the scene!!");
                return null;
            }
        }

        // Make pointer event data... for some reason...
        PointerEventData pointerData = new PointerEventData(eventSystem);
        if (isTouch) {
            pointerData.position = Input.touches[0].position;
        } else {
            pointerData.position = Input.mousePosition;
        }

        List<RaycastResult> hits = new List<RaycastResult>();

        // Raycast each canvas
        foreach (Canvas can in canvases) {
            // Do nothing if the canvas is off or destroyed
            if (can == null) { continue; }
            if (!can.gameObject.activeInHierarchy) { continue; }
            if (canvasNamesToSkip.Contains(can.gameObject.name)) { continue; }

            // Get raycaster component, or add one
            GraphicRaycaster raycaster = can.gameObject.GetComponent<GraphicRaycaster>();
            if (raycaster == null) {
                raycaster = can.gameObject.AddComponent<GraphicRaycaster>();
            }

            // Raycast
            List<RaycastResult> canvHits = new List<RaycastResult>();
            raycaster.Raycast(pointerData, canvHits);

            // Add each to <hits>
            foreach (RaycastResult hit in canvHits) {
                hits.Add(hit);
            }
        }

        return hits;
    }
}
