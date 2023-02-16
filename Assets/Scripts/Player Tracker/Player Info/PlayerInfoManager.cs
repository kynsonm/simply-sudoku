using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        PlayerInfo.Load();
        enumRunning = false;
        StartCoroutine(Save());
        StartCoroutine(UpdatePlayerInfoManager());
    }

    // Update is called once per frame
    IEnumerator UpdatePlayerInfoManager()
    {
        while (!enumRunning) {
            StartCoroutine(Save());
            yield return new WaitForSeconds(0.1f);
        }
    }

    bool enumRunning = false;
    IEnumerator Save() {
        enumRunning = true;
        while (true) {
            PlayerInfo.Save();
            yield return new WaitForSeconds(3f);
        }
    }
}
