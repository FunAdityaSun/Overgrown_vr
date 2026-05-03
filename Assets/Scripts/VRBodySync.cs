using UnityEngine;

public class VRBodySync : MonoBehaviour
{
    public Transform vrCamera;

    void LateUpdate()
    {
        // Sync body rotation with camera's y rotation
        if (vrCamera != null)
        {
            transform.rotation = Quaternion.Euler(0, vrCamera.eulerAngles.y, 0);
        }
    }
}
