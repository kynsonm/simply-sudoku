using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public static class RewardClass
{
    // Action to invoke when reward is completed
    static UnityAction RewardAction;
    // False if the rewarded action is stale
    static bool ready = false;

    // Returns whether reward is ready
    public static bool isReady() {
        return ready;
    }

    // Invokes the reward action and turns <ready> off
    public static void Invoke() {
        if (!ready) {
            Debug.Log("Reward is not ready to give!");
            return;
        }
        RewardAction.Invoke();
        ready = false;
        Unlocks.Save();
    }

    // Sets the reward action
    public static void SetAction(UnityAction action) {
        RewardAction = action;
        ready = true;
    }
}