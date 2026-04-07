using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovement : NetworkBehaviour
{
    CharacterController charCntrl;
    [Tooltip("The speed at which the character will move.")]
    public float speed = 5f;
    [Tooltip("The camera representing where the character is looking.")]
    public GameObject cameraObj;
    [Tooltip("Should be checked if using the Bluetooth Controller to move. If using keyboard, leave this unchecked.")]
    public bool joyStickMode;

    // Start is called before the first frame update
    public override void Spawned()
    {
        charCntrl = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority==false)
        {
            return;
        }
        //Get horizontal and Vertical movements
        float horComp = Input.GetAxis("Horizontal");
        float vertComp = Input.GetAxis("Vertical");

        if (joyStickMode)
        {
            horComp = Input.GetAxis("Vertical");
            vertComp = Input.GetAxis("Horizontal") * -1;
        }

        Vector3 moveVect = Vector3.zero;

        //Get look Direction
        Vector3 cameraLook = cameraObj.transform.forward;
        cameraLook.y = 0f;
        cameraLook = cameraLook.normalized;

        Vector3 forwardVect = cameraLook;
        Vector3 rightVect = Vector3.Cross(forwardVect, Vector3.up).normalized * -1;

        moveVect += rightVect * horComp;
        moveVect += forwardVect * vertComp;
        //Since not simplemove need manual gravity
        moveVect += new Vector3(0f,-9.8f,0f);

        moveVect *= speed;
     

        charCntrl.Move(moveVect*Runner.DeltaTime);


    }
}
