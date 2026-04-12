using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RaycastScript : NetworkBehaviour
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
    [Networked]
    private NetworkObject heldObject { get; set; }

    private bool isHoldingObject = false;
    public Vector3 targetPosition = new Vector3(0.5f, -0.3f, 1f);
    public float requiredHoldTime = 0.5f;
    private float holdTimer = 0f;

    [SerializeField]
    private InteractionSounds interactionSounds;


    void Start()
    {
        if (Runner != null && !(HasStateAuthority && HasInputAuthority))
        {
            return;
        }
        systemControlCanvas = Instantiate(systemControlCanvasPrefab, gameObject.transform);
        systemControlIcons = systemControlCanvas.GetComponentsInChildren<Image>();
        lastOutline = null;
        lightSaber = GetComponent<LineRenderer>();
        systemControlCanvas.SetActive(false);
    }

    void Update()
    {
        if (Runner != null && !(HasStateAuthority && HasInputAuthority))
        {
            return;
        }
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
            // Interact Game Objects if Holding Item
            else if (isHoldingObject && hit.collider != null)
            {
                HandleItemInteractions();
            }
        }

        // Pickup or drop object after trigger button is held for a certain amount of time
        if (Input.GetButton("js0") || Input.GetKey(KeyCode.R))
        {
            if (holdTimer >= 0f)
            {
                holdTimer += Time.deltaTime;
            }
            if (holdTimer >= requiredHoldTime)
            {
                if (isHoldingObject)
                {
                    RPC_DropHeldItem(heldObject);
                }
                else if (hit.collider.CompareTag("Pot") || hit.collider.CompareTag("Watercan") || hit.collider.CompareTag("Seedbag"))
                {
                    RPC_PickupItem(hit.collider.gameObject.GetComponent<NetworkObject>());
                }

                holdTimer = -1f;
            }
        }
        else
        {
            // Debug.Log("Hold timer: " + holdTimer);
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

    void HandleItemInteractions()
    {
        // If looking at soilbed
        if (hit.collider.CompareTag("Soilbed"))
        {
            // If holding a seed bag, try to plant in the bed
            PlantBed bed = hit.collider.GetComponent<PlantBed>();
            if (heldObject.CompareTag("Seedbag"))
            {
                RPC_PlantSeed(bed);
            }
            // If holding a watering can, try to water the bed
            else if (heldObject.CompareTag("Watercan"))
            {
                RPC_WaterBed(bed);
            }
        }
        // If looking at flower
        else if (hit.collider.CompareTag("Flower"))
        {
            // if holing a pot, put flower in pot
            if (heldObject.CompareTag("Pot"))
            {
                Debug.Log("Picked flower!");
                RPC_PickFlower(hit.collider.gameObject.GetComponent<NetworkObject>());               
            }
        }
        // If looking at dye sack
        else if (hit.collider.CompareTag("Dye"))
        {
            if (heldObject.CompareTag("Watercan"))
            {
                // if holing a water can, change its dye type
                DyeSack dyeSack = hit.collider.gameObject.GetComponent<DyeSack>();
                WaterCan waterCan = heldObject.GetComponent<WaterCan>();
                Debug.Log("Changed dye type!");
                waterCan.Change((WaterCan.WaterColor)(int)dyeSack.selectedColor);
            }
        }
        // If looking at well
        else if (hit.collider.CompareTag("Well"))
        {
            if (heldObject.CompareTag("Watercan"))
            {
                // if holing a water can, refill it
                WaterCan waterCan = heldObject.GetComponent<WaterCan>();
                Debug.Log("Refilled Water!");
                waterCan.Fill(3);
                AudioSystem.PlaySFXSpatial(interactionSounds.watercanFillSFX, .4f, gameObject.transform);
            }
        }
        // If looking at NPC
        else if (hit.collider.CompareTag("NPC"))
        {
            if (heldObject.CompareTag("Pot"))
            {
                Debug.Log("Gave item to NPC!");
                RPC_GiveItemToNPC(hit.collider.gameObject.GetComponent<NetworkObject>(), heldObject);
            }
        }
    }

    // Handles giving currently held item to NPC, rn only gives pots
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_GiveItemToNPC(NetworkObject npc, NetworkObject item)
    {
        CustomerNPC customer = npc.GetComponent<CustomerNPC>();
        if (customer != null)
        {
            customer.ReceiveItem(item.gameObject);
            item.transform.SetParent(null);
            item.gameObject.SetActive(false);
            isHoldingObject = false;
        }
    }

    //This can be dropped, so if players arent theere at start it's cooked
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_PickupItem(NetworkObject obj)
    {
        if (HasStateAuthority)
        {
            heldObject = obj;
        }
        heldObject.transform.SetParent(transform);
        heldObject.GetComponent<Rigidbody>().isKinematic = true;
        heldObject.transform.localPosition = targetPosition;
        if (heldObject.CompareTag("Pot"))
        {
            heldObject.transform.localRotation = Quaternion.Euler(-120, 0, -45);
            AudioSystem.PlaySFXSpatial(interactionSounds.potSFX, .5f, gameObject.transform);
        }
        else if (heldObject.CompareTag("Watercan"))
        {
            heldObject.transform.localRotation = Quaternion.Euler(-130, 0, -90);
            AudioSystem.PlaySFXSpatial(interactionSounds.watercanPickupSFX, .3f, gameObject.transform);
        }
        else if (heldObject.CompareTag("Seedbag"))
        {
            AudioSystem.PlaySFXSpatial(interactionSounds.bagSFX, .25f, gameObject.transform);
        }
        isHoldingObject = true;
    }

    

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_PickFlower(NetworkObject obj)
    {
        GameObject flower = obj.gameObject;
        NetworkTransform nt = flower.GetComponent<NetworkTransform>();
        flower.transform.SetParent(heldObject.transform);
        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;
        nt.Teleport(flower.transform.position, flower.transform.rotation);
        AudioSystem.PlaySFXSpatial(interactionSounds.potSFX, .5f, gameObject.transform);
        return;
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_DropHeldItem(NetworkObject objToDrop)
    {
        if (objToDrop != null)
        {
            objToDrop.transform.SetParent(null);
            objToDrop.GetComponent<Rigidbody>().isKinematic = false;
        }
        //heldObject.transform.SetParent(null);
        //heldObject.GetComponent<Rigidbody>().isKinematic = false;
        isHoldingObject = false;
        if (HasStateAuthority)
        {
            heldObject = null;
        }
    }

    void FixedUpdate()
    {
        if (Runner != null && !(HasStateAuthority && HasInputAuthority))
        {
            return;
        }
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
            AudioSystem.PlaySFXSpatial(interactionSounds.teleportSFX, .5f, gameObject.transform);
        }
        else // Single player fallback
        {
            player.position = targetPosition;
            AudioSystem.PlaySFXSpatial(interactionSounds.teleportSFX, .5f, gameObject.transform);
        }

        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    // Handle system control menu navigation and selection
    void SystemControl()
    {
        if (Runner != null && !(HasStateAuthority && HasInputAuthority))
        {
            return;
        }
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
                case 2: // Quit game
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_PlantSeed(PlantBed bed)
    {
        if (bed.HasEmptySlot())
        {
            Debug.Log("Planting seed...");
            SeedBag seedBag = heldObject.GetComponent<SeedBag>();
            if (HasStateAuthority && HasInputAuthority)
            {
                bed.RPC_PlantSeed(seedBag.plantPrefab);
            }
            AudioSystem.PlaySFXSpatial(interactionSounds.bagUseSFX, .2f, bed.gameObject.transform);
            seedBag.use();
            if (seedBag.uses <= 0)
            {
                RPC_DropHeldItem(heldObject);
            }
            return;
        }
        else
        {
            Debug.Log("No empty slots available in this bed!");
            return;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_WaterBed(PlantBed bed)
    {
        if (bed.NeedsWater())
        {
            WaterCan waterCan = heldObject.GetComponent<WaterCan>();

            if (waterCan.uses > 0)
            {
                Debug.Log("Watering bed...");
                
                waterCan.Use();
                if (HasStateAuthority && HasInputAuthority)
                {
                    bed.RPC_WaterBed((WaterColor)(int)waterCan.selectedColor);
                }
                AudioSystem.PlaySFXSpatial(interactionSounds.watercanPourSFX, .5f, bed.gameObject.transform);
                return;
            }
            else
            {
                Debug.Log("Water can is empty...");
            }
        }
        else
        {
            Debug.Log("This bed doesn't need water right now!");
            return;
        }
    }
}
