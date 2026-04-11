using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

public class CustomerNPC : MonoBehaviour
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
    private PotData requestedPot;
    private FlowerData requestedFlower;
    public bool isWaitingForOrder = false;

    private Transform mainCamera;

    void Start()
    {        
        // Hide the bubble at the start
        speechBubbleCanvas.SetActive(false); 
        
        // Start the thinking process
        StartCoroutine(ThinkAndOrder());
    }

    void Update()
    {
        
    }

    IEnumerator ThinkAndOrder()
    {
        yield return new WaitForSeconds(thinkTime);

        // Generate random indices
        int randomPotIndex = Random.Range(0, availablePots.Length);
        int randomFlowerIndex = Random.Range(0, availableFlowers.Length);

        // Update the requested items and UI
        requestedPot = availablePots[randomPotIndex];
        requestedFlower = availableFlowers[randomFlowerIndex];
        desiredPotImage.sprite = requestedPot.potSprite;
        desiredFlowerImage.sprite = requestedFlower.flowerSprite;

        speechBubbleCanvas.SetActive(true);
        isWaitingForOrder = true;
        
        Debug.Log($"NPC wants: {requestedPot.potId} with {requestedFlower.flowerId}");
    }

    public void ReceiveItem(GameObject givenItem)
    {
        if (!isWaitingForOrder) return;

        // TODO: Implement logic to check if the given item matches the requested pot and flower
        
        Debug.Log("Player handed an item to the NPC!");
    }
}