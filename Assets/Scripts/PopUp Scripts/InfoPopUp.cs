using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPopUp : MonoBehaviour
{
    [SerializeField] bool useLocalDimensions;
    [SerializeField] bool closable;
    
    public string Message;
    string lastMessage;

    public float VerticalMultiplier;
    float lastVertMultiplier;

    public float HorizontalMultiplier;
    float lastHorMultiplier;


    // Do everything
    void SetUp() {
        SetText();
        SetDimensions();
        lastMessage = Message;
        lastHorMultiplier = HorizontalMultiplier;
        lastVertMultiplier = VerticalMultiplier;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
        StartCoroutine(UpdateInfoPopUpEnum());
    }

    // Update is called once per frame
    IEnumerator UpdateInfoPopUpEnum()
    {
        while (!CheckVars()) {
            SetUp();
            yield return new WaitForSeconds(0.1f);
        }
    }


    void SetText() {
        TMP_Text text = gameObject.GetComponentInChildren<TMP_Text>();
        text.text = Message;
        text.fontSizeMax = 1000f;
    }

    void SetDimensions() {
        HorizontalMultiplier = (HorizontalMultiplier <= 0f) ? 1f : HorizontalMultiplier;
        HorizontalMultiplier = (HorizontalMultiplier >= 1f) ? 1f : HorizontalMultiplier;
        VerticalMultiplier   = (VerticalMultiplier <= 1f)   ? 1f : VerticalMultiplier;

        StartCoroutine(SetTextDimensions());
    }

    IEnumerator SetTextDimensions() {
        yield return new WaitForEndOfFrame();

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        TMP_Text text = gameObject.GetComponentInChildren<TMP_Text>();

        float size;
        if (useLocalDimensions) { size = ((float)rect.rect.width / 2f) * HorizontalMultiplier; }
        else                    { size = ((float)Screen.width / 2f)    * HorizontalMultiplier; }
        RectTransformOffset.Sides(rect, size);

        yield return new WaitForEndOfFrame();

        size = VerticalMultiplier * text.bounds.size.y;
        text.fontSizeMax = text.fontSize;
        RectTransformOffset.SetToHeight(rect, size);
        RectTransformOffset.SetToWidth(text.rectTransform, 0.9f * rect.rect.width);

        yield return new WaitForEndOfFrame();

        gameObject.GetComponent<ImageTheme>().UpdatePPU();
    }


    // ----- UTILITIES -----

    bool CheckVars() {
        if (lastMessage != Message)                    { return false; }
        if (lastHorMultiplier != HorizontalMultiplier) { return false; }
        if (lastVertMultiplier != VerticalMultiplier)  { return false; }
        return true;
    }
}
