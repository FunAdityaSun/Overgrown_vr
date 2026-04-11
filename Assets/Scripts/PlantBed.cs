using Fusion;
using UnityEngine;

// Class to represent each individual planting slot in the plant bed
public struct PlantSlot :INetworkStruct
{
    public NetworkBool isPlanted;
    public NetworkBool isWatered;
    public NetworkBool isGrown;
    public NetworkPrefabRef assignedPrefab;
    public float growthTimer;

    public WaterColor selectedColor;
}
public enum WaterColor
{
    White = 0,
    Red = 1,
    Yellow = 2,
    Blue = 3
}

public class PlantBed : NetworkBehaviour
{
    public Material[] plantMaterials;
    public int numSlots;

    [Networked, Capacity(8)]
    private NetworkArray<PlantSlot> plantSlots => default; 

    void Start()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        // Loop through every slot to check if it's currently growing
        for (int i = 0; i < numSlots; i++)
        {
            if (plantSlots[i].isWatered && !plantSlots[i].isGrown)
            {
                var slot = plantSlots[i];
                slot.growthTimer -= Runner.DeltaTime;
                plantSlots.Set(i, slot);
                Debug.Log(plantSlots[i].growthTimer);
                if (plantSlots[i].growthTimer <= 0)
                {
                    GrowPlant(plantSlots[i],i);
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
        for (int i = 0; i < numSlots; i++)
        {
            var slot = plantSlots[i];
            if (slot.isPlanted && !slot.isWatered && !slot.isGrown) return true;
        }
        return false;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    // Plant a seed in the first available empty slot
    public void RPC_PlantSeed(NetworkPrefabRef plantPrefab)
    {
        for (int i = 0; i < numSlots; i++)
        {
            var slot = plantSlots[i];
            if (!slot.isPlanted)
            {
                slot.assignedPrefab = plantPrefab;
                slot.isPlanted = true;
                slot.growthTimer = 5f;
                plantSlots.Set(i, slot);
                return;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    // Water all planted seeds that are currently waiting for water
    public void RPC_WaterBed(WaterColor newColor)
    {
        for (int i = 0; i < numSlots; i++)
        {
            var slot = plantSlots[i];
            if (slot.isPlanted && !slot.isWatered && !slot.isGrown)
            {
                slot.isWatered = true;
                slot.selectedColor = newColor;
                plantSlots.Set(i, slot);
            }
        }
    }

    // Spawn the plant prefab at the designated spawn point
    private void GrowPlant(PlantSlot slot, int index)
    {
        slot.isGrown = true;
        plantSlots.Set(index, slot);

        // keep a reference to the new plant so we can change color
        Transform temp = transform.GetChild(index);
        NetworkObject newPlant = Runner.Spawn(slot.assignedPrefab, temp.position, temp.rotation);
        SkinnedMeshRenderer renderer = newPlant.GetComponent<SkinnedMeshRenderer>();
        int colorIndex = (int)slot.selectedColor;
        renderer.material = plantMaterials[colorIndex];
    }
}