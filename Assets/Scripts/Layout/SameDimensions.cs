using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameDimensions : MonoBehaviour
{
    [SerializeField] GameObject reference;

    [SerializeField] bool controlWidth, controlHeight;
    bool lastControlWidth, lastControlHeight;

    [SerializeField] float widthMultipllier, heightMultiplier;
    float lastWidthMultiplier, lastHeightMultiplier;


    // Start is called before the first frame update
    void Start()
    {
        SetSize();
        UpdateSameDimansions();
    }

    // Update is called once per frame
    void UpdateSameDimansions()
    {
        StartCoroutine(UpdateSameDimensionsEnum());
    }
    IEnumerator UpdateSameDimensionsEnum() {
        while (true) {
            yield return new WaitForSeconds(1f);
            if (controlHeight != lastControlHeight || controlWidth != lastControlWidth
                    || widthMultipllier != lastWidthMultiplier || heightMultiplier != lastHeightMultiplier) {
                SetSize();
                lastControlHeight = controlHeight;
                lastControlWidth = controlWidth;
                lastWidthMultiplier = widthMultipllier;
                lastHeightMultiplier = heightMultiplier;
            }
        }
    }


    void SetSize() {
        widthMultipllier = (widthMultipllier == 0f) ? 1f : widthMultipllier;
        heightMultiplier = (heightMultiplier == 0f) ? 1f : heightMultiplier;

        RectTransform sizeRef = reference.GetComponent<RectTransform>();
        RectTransform rect = gameObject.GetComponent<RectTransform>();

        if (controlWidth) {
            StartCoroutine(FindCorrectWidth(rect, sizeRef.rect.width * widthMultipllier));
        }

        if (controlHeight) {
            StartCoroutine(FindCorrectHeight(rect, sizeRef.rect.height * heightMultiplier));
        }
    }


    IEnumerator FindCorrectHeight(RectTransform rect, float targetHeight) {
        yield return new WaitForEndOfFrame();

        if (rect.rect.height < targetHeight) {
            while (rect.rect.height < targetHeight) {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + 1f);
                yield return new WaitForEndOfFrame();
            }
        }
        else {
            while (rect.rect.height > targetHeight) {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y - 1f);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator FindCorrectWidth(RectTransform rect, float targetWidth) {
        yield return new WaitForEndOfFrame();

        if (rect.rect.width < targetWidth) {
            while (rect.rect.width < targetWidth) {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x + 1f, rect.sizeDelta.y);
                yield return new WaitForEndOfFrame();
            }
        }
        else {
            while (rect.rect.width > targetWidth) {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x - 1f, rect.sizeDelta.y);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
