using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelButton : MonoBehaviour
{
    public bool LoadRandomLevel = false;

    Button ThisButton;

    public BoardSize board;
    public BoxSize box;
    public Difficulty difficulty;
    public int number;

    bool running = false;

    void Start() {
        running = false;
        StartCoroutine(SetButton());
        StartCoroutine(UpdateSelectLevelButton());
    }

    IEnumerator UpdateSelectLevelButton() {
        while (ThisButton == null && !running) {
            StartCoroutine(SetButton());
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SelectLevel() {
        if (LoadRandomLevel) {
            board = (BoardSize)Random.Range(0, System.Enum.GetValues(typeof(BoardSize)).Length);
            box = (BoxSize)Random.Range(0, System.Enum.GetValues(typeof(BoxSize)).Length);
            difficulty = (Difficulty)Random.Range(0, System.Enum.GetValues(typeof(Difficulty)).Length);
            number = Random.Range(0, 100);
        }
        LevelInfo.SetLevel(board, box, difficulty, number);

        if (LoadRandomLevel && LevelInfo.LevelFile == null) {
            Debug.Log("Redoing find random level");
            SelectLevel();
        }
    }

    IEnumerator SetButton() {
        running = true;

        ThisButton = gameObject.GetComponent<Button>();
        if (ThisButton == null) {
            Debug.Log("Unable to find level button");
        }

        ThisButton.onClick.AddListener(SelectLevel);

        yield return new WaitForSeconds(1f);

        running = false;
    }
}
