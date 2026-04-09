using UnityEngine;

// Class to represent each individual planting slot in the plant bed
public class PlantSlot
{
    public Transform spawnPoint;
    public bool isPlanted = false;
    public bool isWatered = false;
    public bool isGrown = false;
    public GameObject assignedPrefab;
    public float growthTimer = 5f;

    public WaterColor selectedColor = WaterColor.White;
}
public enum WaterColor
{
    White = 0,
    Red = 1,
    Yellow = 2,
    Blue = 3
}

public class PlantBed : MonoBehaviour
{
    public Material[] plantMaterials;
    private PlantSlot[] plantSlots; 

    void Start()
    {
        
    }

    void Awake()
    {
        InitializeSlots();
    }

    // Initialize the plant slots based on the child transforms of the plant bed
    private void InitializeSlots()
    {
        int slotCount = transform.childCount;
        plantSlots = new PlantSlot[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            plantSlots[i] = new PlantSlot();
            plantSlots[i].spawnPoint = transform.GetChild(i);
        }
    }

    void FixedUpdate()
    {
        // Loop through every slot to check if it's currently growing
        for (int i = 0; i < plantSlots.Length; i++)
        {
            if (plantSlots[i].isWatered && !plantSlots[i].isGrown)
            {
                plantSlots[i].growthTimer -= Time.deltaTime;

                if (plantSlots[i].growthTimer <= 0)
                {
                    GrowPlant(plantSlots[i]);
                }
            }
        }
    }

    // Check if there is at least one empty slot available for planting
    public bool HasEmptySlot()
    {
        foreach (var slot in plantSlots)
        {
            if (!slot.isPlanted) return true;
        }
        return false;
    }

    // Check if there is at least one planted seed that is waiting for water
    public bool NeedsWater()
    {
        foreach (var slot in plantSlots)
        {
            if (slot.isPlanted && !slot.isWatered && !slot.isGrown) return true;
        }
        return false;
    }

    // Plant a seed in the first available empty slot
    public void PlantSeed(GameObject plantPrefab)
    {
        foreach (var slot in plantSlots)
        {
            if (!slot.isPlanted)
            {
                slot.assignedPrefab = plantPrefab;
                slot.isPlanted = true;
                Debug.Log("Planted at: " + slot.spawnPoint.name);
                return;
            }
        }
    }

    // Water all planted seeds that are currently waiting for water
    public void WaterBed(WaterColor newColor)
    {
        foreach (var slot in plantSlots)
        {
            if (slot.isPlanted && !slot.isWatered && !slot.isGrown)
            {
                slot.isWatered = true;
                slot.selectedColor = newColor;
            }
        }
    }

    // Spawn the plant prefab at the designated spawn point
    private void GrowPlant(PlantSlot slot)
    {
        slot.isGrown = true;

        // keep a reference to the new plant so we can change color
        GameObject newPlant = Instantiate(slot.assignedPrefab, slot.spawnPoint.position, slot.spawnPoint.rotation);
        SkinnedMeshRenderer renderer = newPlant.GetComponent<SkinnedMeshRenderer>();
        int colorIndex = (int)slot.selectedColor;
        renderer.material = plantMaterials[colorIndex];
    }
}