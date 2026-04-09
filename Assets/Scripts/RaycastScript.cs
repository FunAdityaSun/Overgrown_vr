using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections;

public class RaycastScript : MonoBehaviour
{
    private enum GameState
    {
        Normal,
        SystemControl
    }
    private GameState currentState = GameState.Normal;

    // Player parameters
    public Transform player;
    private float mvmtSpeed = 10f;
    
    private Outline lastOutline;

    // Raycast parameters
    public float rayDistance = 20f;
    private RaycastHit hit;
    private LineRenderer lightSaber;

    // System control parameters
    [SerializeField] private GameObject systemControlCanvasPrefab;
    private GameObject systemControlCanvas;
    private Image[] systemControlIcons;
    private int currentSystemControlIdx = 1;
    private float nextJoyStickMove = 0f;

    private GameObject currentUITarget;

    // Interaction parameters
    private GameObject interactableObject;
    private bool isHoldingObject = false;
    private float requiredHoldTime = 0.5f;
    private float holdTimer = 0f;


    void Start()
    {
        systemControlCanvas = Instantiate(systemControlCanvasPrefab, gameObject.transform);
        systemControlIcons = systemControlCanvas.GetComponentsInChildren<Image>();
        lastOutline = null;
        lightSaber = GetComponent<LineRenderer>();
        systemControlCanvas.SetActive(false);
    }

    void Update()
    {
        // If trigger button is pressed
        if ((Input.GetButtonUp("js0") || Input.GetKeyUp(KeyCode.R)) && holdTimer < requiredHoldTime && holdTimer > 0f)
        {
            // Teleport player to hit point on floor
            if (hit.collider.CompareTag("Floor"))
            {
                TeleportPlayer(new Vector3(hit.point.x, player.position.y + 0.2f, hit.point.z));
            }
            // Interact with UI buttons
            else if (currentUITarget != null && currentUITarget.GetComponent<UnityEngine.UI.Button>())
            {
                currentUITarget.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            }
            // Interact with soil beds
            if (isHoldingObject && hit.collider.CompareTag("Soilbed"))
            {
                PlantBed bed = hit.collider.GetComponent<PlantBed>();
                // If holding a seed bag, try to plant in the bed
                if (interactableObject.CompareTag("Seedbag"))
                {
                    if (bed.HasEmptySlot())
                    {
                        Debug.Log("Planting seed...");
                        SeedBag seedData = interactableObject.GetComponent<SeedBag>();
                        bed.PlantSeed(seedData.plantPrefab);
                        return;
                    }
                    else
                    {
                        Debug.Log("No empty slots available in this bed!");
                        return;
                    }
                }
                // If holding a watering can, try to water the bed
                else if (interactableObject.CompareTag("Watercan"))
                {
                    if (bed.NeedsWater())
                    {
                        Debug.Log("Watering bed...");
                        bed.WaterBed();
                        return;
                    }
                    else
                    {
                        Debug.Log("This bed doesn't need water right now!");
                        return;
                    }
                }
            }
            // If holing a pot and select a flower, put flower in pot
            else if (isHoldingObject && hit.collider.CompareTag("Flower"))
            {
                if (hit.collider.CompareTag("Flower"))
                {
                    Debug.Log("Picked flower!");
                    GameObject flower = hit.collider.gameObject;
                    flower.transform.SetParent(interactableObject.transform);
                    flower.transform.localPosition = Vector3.zero;
                    return;
                }
            }
        }

        // Pickup or drop object after trigger button is held for a certain amount of time
        if (Input.GetButton("js0") || Input.GetKey(KeyCode.R))
        {
            holdTimer += Time.deltaTime;
            Debug.Log("Hold timer: " + holdTimer);
            if (holdTimer >= requiredHoldTime)
            {
                if (isHoldingObject)
                {
                    interactableObject.transform.SetParent(null);
                    interactableObject.GetComponent<Rigidbody>().isKinematic = false;
                    isHoldingObject = false;
                    interactableObject = null;
                }
                else if (hit.collider.CompareTag("Pot") || hit.collider.CompareTag("Watercan") || hit.collider.CompareTag("Seedbag"))
                {
                    interactableObject = hit.collider.gameObject;
                    interactableObject.transform.SetParent(transform);
                    interactableObject.GetComponent<Rigidbody>().isKinematic = true;
                    interactableObject.transform.localPosition = new Vector3(0.5f, -0.3f, 1f);
                    if (interactableObject.CompareTag("Pot"))
                    {
                        interactableObject.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    }
                    isHoldingObject = true;
                }

                holdTimer = -1f;
            }     
        }
        else if (Input.GetButtonUp("js0") || Input.GetKeyUp(KeyCode.R))
        {
            holdTimer = 0f;
        }

        // Open system control menu if X button is presssed
        if ((Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.Q)) && currentState == GameState.Normal)
        {
            currentState = GameState.SystemControl;
        }

        if (currentState == GameState.SystemControl)
        {
            FreezePlayer(true);
            SystemControl();
        }
    }

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        Vector3 saberOrigin = transform.position + new Vector3(0, -0.5f, 0);
        Ray ray = new Ray(saberOrigin, transform.forward);

        // Shows raycast in game
        lightSaber.enabled = true;
        lightSaber.SetPosition(0, saberOrigin);
        lightSaber.SetPosition(1, saberOrigin + transform.forward * rayDistance);

        UnityEngine.Debug.DrawRay(saberOrigin, transform.forward * rayDistance, Color.red);

        int cominedLayerMask = LayerMask.GetMask("UI", "Interactable", "Floor", "Default");
        if (Physics.Raycast(ray, out hit, rayDistance, cominedLayerMask))
        {
            // Handle UI interactions
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                if (currentUITarget != hit.transform.gameObject)
                {
                    UnityEngine.UI.Image background;
                    if (currentUITarget != null)
                    {
                        background = currentUITarget.GetComponent<UnityEngine.UI.Image>();
                        if (background != null)
                        {
                            background.color = Color.white;
                        }
                    }
                    currentUITarget = hit.transform.gameObject;

                    background = currentUITarget.GetComponent<UnityEngine.UI.Image>();
                    if (background != null)
                    {
                        background.color = Color.yellow;
                    }
                }
            }
            // Handle interactable objects
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
            {
                //currentUITarget = null;
                Outline currentOutline = hit.collider.GetComponent<Outline>();

                // If new object hit, update outline
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


            }
            else
            {
                //currentUITarget = null;
                ClearOutline();
            }
        }
        else
        {
            ClearOutline();
        }

        // Position system control canvas in front of player when menu is open
        if (currentState == GameState.SystemControl)
        {
            systemControlCanvas.transform.position = transform.position + transform.forward * 5f;
            systemControlCanvas.transform.LookAt(player);
            systemControlCanvas.transform.Rotate(0, 180, 0);
            systemControlCanvas.SetActive(true);
        }
    }

    // Clear the outline from the last hit object
    void ClearOutline()
    {
        if (lastOutline != null)
        {
            lastOutline.enabled = false;
            lastOutline = null;
        }
    }

    // Teleport the player to the target position
    void TeleportPlayer(Vector3 targetPosition)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        // Teleport player using NetworkTransform for multiplayer
        NetworkTransform networkTransform = player.GetComponent<NetworkTransform>();
        if (networkTransform != null)
        {
            networkTransform.Teleport(targetPosition);
        }
        else // Single player fallback
        {
            player.position = targetPosition;
        }

        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    // Handle system control menu navigation and selection
    void SystemControl()
    {
        ClearOutline();
        float joyY = Input.GetAxis("Vertical");

        bool pcUp = Input.GetKeyDown(KeyCode.UpArrow);
        bool pcDown = Input.GetKeyDown(KeyCode.DownArrow);

        // Highlight the first option by default when entering the menu
        if (currentSystemControlIdx == 1)
        {
            foreach (Image icon in systemControlIcons) icon.color = Color.white;
            systemControlIcons[currentSystemControlIdx].color = Color.yellow;
        }

        // Navigate menu with joystick or keyboard
        if ((joyY > 0.5f || pcUp) && Time.time > nextJoyStickMove)
        {
            currentSystemControlIdx--;
            if (currentSystemControlIdx < 1) currentSystemControlIdx = systemControlIcons.Length - 1;
            foreach (Image icon in systemControlIcons) icon.color = Color.white;
            systemControlIcons[currentSystemControlIdx].color = Color.yellow;
            nextJoyStickMove = Time.time + 0.2f; // Add a delay to prevent rapid changes
        }
        else if ((joyY < -0.5f || pcDown) && Time.time > nextJoyStickMove)
        {
            currentSystemControlIdx++;
            if (currentSystemControlIdx >= systemControlIcons.Length) currentSystemControlIdx = 1;
            foreach (Image icon in systemControlIcons) icon.color = Color.white;
            systemControlIcons[currentSystemControlIdx].color = Color.yellow;
            nextJoyStickMove = Time.time + 0.2f; // Add a delay to prevent rapid changes
        }

        // Select menu option if B button is pressed
        if (Input.GetButtonDown("js5") || Input.GetKeyDown(KeyCode.E))
        {
            switch (currentSystemControlIdx)
            {
                case 1: // Resume game
                    systemControlCanvas.SetActive(false);
                    FreezePlayer(false);
                    currentState = GameState.Normal;
                    break;
                case 2 : // Quit game
                    Application.Quit();
                    break;
            }
        }
    }

    // Freeze or unfreeze player movement and raycasting
    void FreezePlayer(bool freeze)
    {
        if (player != null) player.GetComponent<CharacterMovement>().speed = freeze ? 0 : mvmtSpeed;
        if (lightSaber != null) lightSaber.enabled = !freeze;
    }

}
