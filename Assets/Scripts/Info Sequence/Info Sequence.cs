using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SequenceCommand
{

}

public static class InfoSequence
{
    // Yeah
    static SequenceManager sequenceManager;

    // Constructor just gets sequence manager var
    static InfoSequence() {
        sequenceManager = GameObject.FindObjectOfType<SequenceManager>();
        if (sequenceManager == null) {
            Debug.LogError("Sequence manager is null!!");
        }
    }

    // Starts the sequence defined by SequenceManager
    // In a static class so that it can called from any script
    public static void StartSequence() {
        // Don't do it!!
        if (sequenceManager == null) {
            Debug.LogError("Sequence manager is null!!");
            return;
        }

        // Starts the sequence
        sequenceManager.StartSequence();
    }
}
