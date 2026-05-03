using UnityEngine;
using Fusion;

public class VRHeadSync : NetworkBehaviour
{
    public Transform vrCamera;

    [Networked]
    public Quaternion NetworkedRotation { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority && vrCamera != null)
        {
            NetworkedRotation = vrCamera.localRotation;
        }
    }

    public override void Render()
    {
        // Sync head rotation across the network
        if (HasInputAuthority == false && vrCamera != null)
        {
            vrCamera.localRotation = NetworkedRotation;
        }
    }
}
