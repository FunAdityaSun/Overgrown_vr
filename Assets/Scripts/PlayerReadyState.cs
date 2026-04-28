using UnityEngine;
using Fusion;

public class PlayerReadyState : NetworkBehaviour
{
    [Networked] public NetworkBool IsPlayerReady { get; set; }
    
    // This method is called when the player clicks the "Ready" button in the lobby
    public void SetReady()
    {
        if (HasStateAuthority)
        {
            IsPlayerReady = true;
            Debug.Log("I am ready!");

            // Update ready text for local player
            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager != null && tutorialManager.countdownText != null)
            {
                tutorialManager.countdownText.text = "Waiting for other players...";
            }
        }
    }
}
