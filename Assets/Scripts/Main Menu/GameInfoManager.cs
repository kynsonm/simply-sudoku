using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoManager : MonoBehaviour
{
    // Buttons
    [SerializeField] GameObject StartTourButton;
    [SerializeField] GameObject EmailMeButton;
    [SerializeField] GameObject SupportMeButton;

    // Whether the manager is on or not
    void Awake() {
        SetOnClicks();
    }

    // Gives each button their click
    void SetOnClicks() {
        SequenceManager seq = GameObject.FindObjectOfType<SequenceManager>(true);

        Button tour = StartTourButton.transform.Find("Button").GetComponent<Button>();
        tour.onClick.RemoveAllListeners();
        tour.onClick.AddListener(() => {
            seq.StartSequence();
        });

        Button email = EmailMeButton.transform.Find("Button").GetComponent<Button>();
        email.onClick.RemoveAllListeners();
        email.onClick.AddListener(() => {
            // TODO: Set up email stuff
        });

        Button support = SupportMeButton.transform.Find("Button").GetComponent<Button>();
        support.onClick.RemoveAllListeners();
        support.onClick.AddListener(() => {
            Application.OpenURL("https://linktr.ee/kynson");
        });
    }
}
