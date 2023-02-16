using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LogToFile : MonoBehaviour
{
    // Comment out whichever you want
    bool makeLogFile = 
    false
    //true
    ;

    new string name = "LogToFile Object";

    string path = "";
    string fullLog = "";
    int count = 0;

    string prefix =   " ===============  LOG # : T =============== \n";
    string info =     " -------------  Scene: #  ------------- \n";
    string suffix = "\n ============================================ \n\n";

    public void Awake()
    {
        // Dont make log file, and delete it if its already there
        if (!makeLogFile) {
            if (File.Exists(path)) { File.Delete(path); }
            return;
        }

        // Create a LogToFile object
        if (gameObject.name != name) {
            if (GameObject.Find(name) != null) { return; }

            GameObject obj = new GameObject();
            GameObject logToFileObj = Instantiate(obj);
            logToFileObj.name = name;
            LogToFile logToFile = logToFileObj.AddComponent<LogToFile>();

            DontDestroyOnLoad(logToFileObj);

            GameObject.Destroy(obj);
        }
        else {
            path = Application.persistentDataPath + "/DebugLog.txt";
            fullLog = "";
            count = 0;

            Application.logMessageReceivedThreaded += HandleLog;
        }
    }

    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!makeLogFile) { return; }

        if (count > 2000) { return; }
        if (logString.Contains("[Adaptive Performance]")) { return; }

        ++count;
        fullLog += prefix.Replace("T", type.ToString()).Replace("#", count.ToString());
        fullLog += info.Replace("#", SceneLoader.CurrentScene.ToString());
        fullLog += logString;
        if (type == LogType.Error || type == LogType.Exception) {
            fullLog += stackTrace;
        }
        fullLog += suffix;

        File.WriteAllText(path, fullLog);
    }
}
