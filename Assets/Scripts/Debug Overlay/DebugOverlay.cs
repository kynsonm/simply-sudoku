using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    public GameObject FPSText;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetFPS());
    }

    IEnumerator GetFPS() {
        while (true) {
            float fps = (int)(10f / Time.unscaledDeltaTime);
            fps /= 10f;

            FPSText.GetComponent<TMP_Text>().text = (fps + " fps");

            yield return new WaitForSeconds(0.25f);
        }
    }
}
