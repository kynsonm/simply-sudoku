using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadSceneButton : MonoBehaviour
{
    [SerializeField] SceneLoader.Scene sceneToLoad;

    // Setup the button
    void OnEnable() { Start(); }
    void Start()
    {
        // Get the button
        Button butt = gameObject.GetComponent<Button>();
        if (butt == null) {
            Debug.Log("NO BUTTON ON LOAD SCENE BUTTON");
        }

        // Set its onclick
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(() => {
            SceneLoader.LoadScene(sceneToLoad);
        });
    }
}
