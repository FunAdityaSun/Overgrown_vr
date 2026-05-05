using UnityEngine;
using Fusion;

public class FinishedPlant : NetworkBehaviour
{
    [Networked] public NetworkString<_32> CurrentPotId { get; set; }
    [Networked] public NetworkString<_32> CurrentFlowerId { get; set; }
}
