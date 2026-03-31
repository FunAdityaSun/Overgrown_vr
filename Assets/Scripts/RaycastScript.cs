using UnityEngine;

public class RaycastScript : MonoBehaviour
{
    public float rayDistance = 20f;
    private Outline lastOutline;
    public Transform player;

    void Start()
    {
        lastOutline = null;
    }


    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        UnityEngine.Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.red);

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Outline currentOutline = hit.collider.GetComponent<Outline>();

            if (currentOutline != null)
            {
                if (lastOutline != currentOutline)
                {
                    ClearOutline();
                    currentOutline.enabled = true;
                    lastOutline = currentOutline;
                }                
            }
            else
            {
                ClearOutline();
            }


            if (Input.GetButton("js10"))
            {
                if (hit.collider.name.Contains("Plane"))
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        cc.enabled = false; // Disable CharacterController to avoid collision issues
                        player.position = new Vector3(hit.point.x, player.position.y + 1f, hit.point.z);
                        cc.enabled = true; // Re-enable CharacterController after moving
                    }
                    //player.position = new Vector3(hit.point.x, player.position.y + 1f, hit.point.z);
                }
            }
        }
        else
        {
            ClearOutline();
        }
    }

    void ClearOutline()
    {
        if (lastOutline != null)
        {
            lastOutline.enabled = false;
            lastOutline = null;
        }
    }
}
