using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameNumbers : MonoBehaviour
{
    public GameObject NumberButtonHolder;
    public GameObject NumberButtonPrefab;
    int maxNumber;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MakeNumbers());
    }

    public void CompleteLevel() {
        foreach (Transform trans in NumberButtonHolder.transform) {
            Button butt = trans.gameObject.GetComponent<Button>();
            butt.interactable = false;
        }
    }

    IEnumerator MakeNumbers() {
        // Get the number of buttons to make
        maxNumber = 0;
        int count = 0;
        while (maxNumber == 0 && count < 100) {
            maxNumber = Mathf.Max(LevelInfo.BoardSizeToHeight(), LevelInfo.BoardSizeToWidth());
            ++count;
            yield return new WaitForEndOfFrame();
        }

        // Get their size
        float height = NumberButtonHolder.GetComponent<RectTransform>().sizeDelta.y;

        // Create the numbers
        for (int num = 1; num <= maxNumber; ++num) {
            // Make button and name it
            GameObject obj = Instantiate(NumberButtonPrefab, NumberButtonHolder.transform);
            obj.name = (num + " Button");

            // Set its text
            TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
            text.text = num.ToString();

            // Set its button stuff
            Button but = obj.GetComponent<Button>();
            int temp = num;
            but.onClick.AddListener(delegate { GameManager.NumberButtonPress(temp); } );
        }
    }
}
