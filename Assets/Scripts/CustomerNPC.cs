using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Threading.Tasks;
using TMPro;

[System.Serializable]
public struct PotData
{
    public string potId;
    public Sprite potSprite;
}

[System.Serializable]
public struct FlowerData
{
    public string flowerId;
    public Sprite flowerSprite;
}

public class CustomerNPC : NetworkBehaviour
{
    public float thinkTime = 3.0f;
    
    // UI Elements
    public GameObject speechBubbleCanvas;
    public Image desiredPotImage;
    public Image desiredFlowerImage;

    // Available items for the NPC to request
    public PotData[] availablePots; 
    public FlowerData[] availableFlowers;

    // The currently requested items
    // private PotData requestedPot;
    // private FlowerData requestedFlower;
    // public bool isWaitingForOrder = false;

    [Networked] public NetworkBool IsOrderReady { get; set; }
    [Networked] public int NetworkedPotIndex { get; set; }
    [Networked] public int NetworkedFlowerIndex { get; set; }

    private Transform mainCamera;
    private ChangeDetector changeDetector;

    private float maxPatience = 240f;
    public TMP_Text patienceText;
    [Networked] public float PatienceLeft { get; set; }

    // void Start()
    // {        
    //     // Hide the bubble at the start
    //     speechBubbleCanvas.SetActive(false); 
        
    //     // Start the thinking process
    //     StartCoroutine(ThinkAndOrder());
    // }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        speechBubbleCanvas.SetActive(false);
        if (HasStateAuthority)
        {
            //StartCoroutine(ThinkAndOrder());
        }
    }

    
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && IsOrderReady)
        {
            if (PatienceLeft > 0)
            {
                PatienceLeft -= Runner.DeltaTime;
                
                if (PatienceLeft <= 0)
                {
                    PatienceLeft = 0;
                    IsOrderReady = false;
                    AIManager manager = FindObjectOfType<AIManager>();
                    manager.RPC_Lose();
                }
            }
        }
    }


    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof (IsOrderReady):
                    if (IsOrderReady)
                    {
                        UpdateLocalUI();
                    }
                    else
                    {
                        speechBubbleCanvas.SetActive(false);
                    }
                    break;
            }
        }

        if (IsOrderReady && patienceText != null)
        {
            patienceText.text = $"{Mathf.CeilToInt(PatienceLeft)}s";

            if (PatienceLeft <= 10f)
            {
                patienceText.color = Color.red;
            }
            else
            {
                patienceText.color = Color.black;
            }
        }
    }

    public void Order()
    {
        // Generate random indices
        int randomPotIndex = Random.Range(0, availablePots.Length);
        int randomFlowerIndex = Random.Range(0, availableFlowers.Length);

        // Update the requested items and UI
        // requestedPot = availablePots[randomPotIndex];
        // requestedFlower = availableFlowers[randomFlowerIndex];
        // desiredPotImage.sprite = requestedPot.potSprite;
        // desiredFlowerImage.sprite = requestedFlower.flowerSprite;

        // speechBubbleCanvas.SetActive(true);

        NetworkedPotIndex = randomPotIndex;
        NetworkedFlowerIndex = randomFlowerIndex;

        PatienceLeft = maxPatience;
        IsOrderReady = true;

        // var task = Impatience();
        // isWaitingForOrder = true;

        //Debug.Log($"NPC wants: {requestedPot.potId} with {requestedFlower.flowerId}");
    }

    // async Task Impatience()
    // {
    //     for (int i = 0; i < 30; i++)
    //     {
    //         await Task.Delay((int)thinkTime * 1000);
    //         //Update Impatience bar
    //         Debug.Log(i*3f);
    //     }
    //     AIManager manager = FindObjectOfType<AIManager>();
    //     manager.Lose();
    // }

    IEnumerator ThinkAndOrder()
    {
        yield return new WaitForSeconds(thinkTime);

        // Generate random indices
        int randomPotIndex = Random.Range(0, availablePots.Length);
        int randomFlowerIndex = Random.Range(0, availableFlowers.Length);

        // Update the requested items and UI
        // requestedPot = availablePots[randomPotIndex];
        // requestedFlower = availableFlowers[randomFlowerIndex];
        // desiredPotImage.sprite = requestedPot.potSprite;
        // desiredFlowerImage.sprite = requestedFlower.flowerSprite;

        // speechBubbleCanvas.SetActive(true);

        NetworkedPotIndex = randomPotIndex;
        NetworkedFlowerIndex = randomFlowerIndex;

        IsOrderReady = true;

        // isWaitingForOrder = true;
        
        //Debug.Log($"NPC wants: {requestedPot.potId} with {requestedFlower.flowerId}");
    }

    private void UpdateLocalUI()
    {
        desiredPotImage.sprite = availablePots[NetworkedPotIndex].potSprite;
        desiredFlowerImage.sprite = availableFlowers[NetworkedFlowerIndex].flowerSprite;
        speechBubbleCanvas.SetActive(true);
    }

    public void ReceiveItem(GameObject givenItem)
    {
        if (!IsOrderReady) return;

        // TODO: Implement logic to check if the given item matches the requested pot and flower
        
        Debug.Log("Player handed an item to the NPC!");

        AIManager manager = FindObjectOfType<AIManager>();
        manager.Despawn(gameObject.GetComponent<NetworkObject>());
    }
}