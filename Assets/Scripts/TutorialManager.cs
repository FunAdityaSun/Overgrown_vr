using Fusion;
using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;

public class TutorialManager : NetworkBehaviour
{
    private bool _isTransitioning = false;
    public TMP_Text countdownText;

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsSharedModeMasterClient || _isTransitioning) return;

        //Make sure that at least 2 players are in the room before checking
        //if (Runner.ActivePlayers.Count() < 2) return;

        // Check if all players are ready, if so, switch to main scene
        PlayerReadyState[] allPlayers = FindObjectsOfType<PlayerReadyState>();
        if (allPlayers.Length != Runner.ActivePlayers.Count()) return;

        bool everyoneReady = true;
        foreach (var player in allPlayers)
        {
            if (!player.IsPlayerReady)
            {
                everyoneReady = false;
                break;
            }
        }

        if (everyoneReady)
        {
            _isTransitioning = true;
            StartCoroutine(TransitionToMainGameSequence());
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UpdateCountdown(string message)
    {
        if (countdownText != null)
        {
            countdownText.text = message;
        }
    }

    // Countdown sequence before transitioning to the main game scene
    private IEnumerator TransitionToMainGameSequence()
    {
        RPC_UpdateCountdown("Joining in 3...");
        yield return new WaitForSeconds(1f);
        RPC_UpdateCountdown("Joining in 2...");
        yield return new WaitForSeconds(1f);
        RPC_UpdateCountdown("Joining in 1...");
        yield return new WaitForSeconds(1f);

        Runner.LoadScene(SceneRef.FromIndex(2), UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}