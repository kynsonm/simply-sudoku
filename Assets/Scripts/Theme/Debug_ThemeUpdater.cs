using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class Debug_ThemeUpdater : MonoBehaviour
{
    [SerializeField] bool resetThemes;
    bool enumRunning = false;

    void Awake() {
        Start();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ResetAllThemes());
    }

    // Update is called once per frame
    void Update()
    {
        if (!enumRunning && resetThemes) {
            StopAllCoroutines();
            StartCoroutine(ResetAllThemes());
        }
    }

    // Resets every theme in the game every second
    IEnumerator ResetAllThemes() {
        enumRunning = true;

        while (true) {
            // Dont update if its turned off
            if (!resetThemes) {
                enumRunning = false;
                yield break;
            }

            // Reset all the themes
            Theme.Reset();
            Debug.Log("Resetting all themes");
            yield return new WaitForSeconds(1f);
        }
    }
}
