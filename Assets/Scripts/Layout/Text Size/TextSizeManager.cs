using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]

public class TextSizeManager : MonoBehaviour
{
    public List<SameTextSize> allTextSizes;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ResetAllSizes());
    }

    // Resets all texts in the scene after a few frames
    IEnumerator ResetAllSizes() {
        TextSizeStatic.textSizeManager = this;
        
        yield return new WaitForEndOfFrame();
        TextSizeStatic.Reset();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        //Debug.Log("Resetting all " + allTextSizes.Count + " SameTextSizes");

        foreach (SameTextSize size in allTextSizes) {
            size.Reset();
        }
    }

    // Takes in a textScript and resets its size
    public void ResetSize(SameTextSize textScript) {
        StartCoroutine(ResetSizeEnum(textScript));
    }
    IEnumerator ResetSizeEnum(SameTextSize textScript) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        while (true) {
            // If text gets destroyed, stop doing the thing
            if (textScript == null) {
                //Object.Destroy(textScript);
                yield break;
            }

            // If the script is off, don't do anything
            if (!textScript.gameObject.activeInHierarchy) {
                textScript.isActive = false;
                //Debug.Log("Text is not active");
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            // Update text size if script says to
            if (textScript.UPDATE_TEXT) { }
            // Otherwise, wait a second before checking stuff again
            else if (textScript.isActive) {
                yield return new WaitForSeconds(1f);
                continue;
            }
            textScript.isActive = true;
            textScript.UPDATE_TEXT = false;

            // If the text sizes don't need to be updated, dont
            if (!textScript.NeedsToUpdate()) {
                //Debug.Log("Text does need an update -- Skipping");
                //continue;
            }


            // Set size to max
            foreach (SameTextSizeClass text in textScript.texts) {
                text.text.fontSize = 1000f;
                text.text.fontSizeMax = 1000f;
            }

            // Wait
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Get min
            float min = float.MaxValue;
            string minName = "";
            foreach (SameTextSizeClass text in textScript.texts) {
                // Non negative
                if (text.text.fontSize <= 0) {
                    continue;
                }
                if (text.text.fontSize < min) {
                    min = text.text.fontSize;
                    minName = (text.text.gameObject.transform.parent.name + " --> " + text.text.gameObject.name);
                }
            }

            //Debug.Log("Min came from " + minName);

            // Set min
            foreach (SameTextSizeClass text in textScript.texts) {
                float mult = 1f;
                if (text.theme != null) {
                    mult = text.theme.MaxTextRatio;
                }
                text.text.fontSizeMax = textScript.textSizeMultiplier * (min * mult);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    void ResetSize() {

    }
}
