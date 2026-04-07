using UnityEngine;
using UnityEngine.UI;

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
    private float mvmtSpeed = 5f;
    
    private Outline lastOutline;

    // Raycast parameters
    public float rayDistance = 20f;
    private bool isHitting = false;
    private RaycastHit hit;
    private LineRenderer lightSaber;

    // System control parameters
    [SerializeField] private GameObject systemControlCanvasPrefab;
    private GameObject systemControlCanvas;
    private Image[] systemControlIcons;
    private int currentSystemControlIdx = 1;
    private float nextJoyStickMove = 0f;

    private GameObject currentUITarget;


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
        if (isHitting)
        {
            if ((Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.E)) && hit.collider != null)
            {
                if (hit.collider.CompareTag("Floor"))
                {
                    TeleportPlayer(new Vector3(hit.point.x, player.position.y + 0.2f, hit.point.z));
                }
            }
        }

        if (Input.GetButtonDown("js5") || Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Hitting R");
            if (currentUITarget != null && currentUITarget.GetComponent<UnityEngine.UI.Button>())
            {
                currentUITarget.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            }
        }

        // Open system control menu
        if ((Input.GetButtonDown("js0") || Input.GetKeyDown(KeyCode.Q)) && currentState == GameState.Normal)
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

        //If hit canvas UI element, trigger "hover" effects
        int layerMask = LayerMask.GetMask("UI");
        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
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
        else
        {
            if (currentUITarget != null)
            {
                UnityEngine.UI.Image background = currentUITarget.GetComponent<UnityEngine.UI.Image>();
                if (background != null)
                {
                    background.color = Color.white;
                }
                currentUITarget = null;
            }
        }

        layerMask = LayerMask.GetMask("Interactable");
        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
        {
            Outline currentOutline = hit.collider.GetComponent<Outline>();
            isHitting = true;

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
            isHitting = false;
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
            player.position = targetPosition;
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

        // Select menu option
        if (Input.GetButtonDown("js5") || Input.GetKeyDown(KeyCode.B))
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
