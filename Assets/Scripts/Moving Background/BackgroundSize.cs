using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class BackgroundSize : MonoBehaviour
{
    public GameObject BackgroundObject;
    RectTransform thisRect;

    // Start is called before the first frame update
    void Start()
    {
        if (BackgroundObject == null) {
            BackgroundObject = gameObject;
        }

        thisRect = BackgroundObject.GetComponent<RectTransform>();

        Vector2 size = new Vector2(Screen.width, Screen.height);
        thisRect.sizeDelta = size;
    }
}
