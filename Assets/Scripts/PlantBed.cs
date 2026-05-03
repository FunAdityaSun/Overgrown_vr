using Fusion;
using UnityEngine;
using System.Collections.Generic;

public struct PlantSlot : INetworkStruct
{
    public NetworkBool isPlanted;
    public NetworkBool isWatered;
    public NetworkBool isGrown;
    public NetworkPrefabRef assignedPrefab;
    public float growthTimer;
    public WaterColor selectedColor;
}

public enum WaterColor { White = 0, Red = 1, Yellow = 2, Blue = 3 }

public class PlantBed : NetworkBehaviour
{
    public Material[] plantMaterials;

    private int numSlots;
    private ParticleSystem[] slotParticles;
    private ParticleSystem[] seedParticles;
    private bool[] _localIsPlanted;

    [Networked, Capacity(8)]
    private NetworkArray<PlantSlot> plantSlots => default;

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // get slots automatically from children
        numSlots = transform.childCount;
        slotParticles = new ParticleSystem[numSlots];
        seedParticles = new ParticleSystem[numSlots];
        _localIsPlanted = new bool[numSlots];

        for (int i = 0; i < numSlots; i++)
        {
            Transform slot = transform.GetChild(i);

            Transform slotParticlesTransform = slot.Find("plant_particles");
            Transform seedParticlesTransform = slot.Find("seed_particles");

            if (slotParticlesTransform != null) slotParticles[i] = slotParticlesTransform.GetComponentInChildren<ParticleSystem>();
            if (seedParticlesTransform != null) seedParticles[i] = seedParticlesTransform.GetComponentInChildren<ParticleSystem>();
            if (slotParticles[i] != null) slotParticles[i]?.Stop();
            if (seedParticles[i] != null) seedParticles[i]?.Stop();
        }
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(plantSlots))
            {
                UpdateParticles();
            }
        }
    }

    private void UpdateParticles()
    {
        for (int i = 0; i < numSlots; i++)
        {
            if (seedParticles[i] != null)
            {
                if (plantSlots[i].isPlanted && !_localIsPlanted[i])
                {
                    seedParticles[i].Play();
                    _localIsPlanted[i] = true;
                }
                else if (!plantSlots[i].isPlanted && _localIsPlanted[i])
                {
                    seedParticles[i].Stop();
                    _localIsPlanted[i] = false;
                }
            }

            if (slotParticles[i] == null) continue;

            if (plantSlots[i].isWatered && !plantSlots[i].isGrown)
            {
                if (!slotParticles[i].isPlaying) slotParticles[i].Play();
            }
            else
            {
                if (slotParticles[i].isPlaying) slotParticles[i].Stop();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        for (int i = 0; i < numSlots; i++)
        {
            var slot = plantSlots[i];
            if (slot.isWatered && !slot.isGrown)
            {
                slot.growthTimer -= Runner.DeltaTime;

                if (slot.growthTimer <= 0)
                {
                    GrowPlant(slot, i);
                }
                else
                {
                    plantSlots.Set(i, slot);
                }
            }
        }
    }

    public bool HasEmptySlot()
    {
        for (int i = 0; i < numSlots; i++)
        {
            if (!plantSlots[i].isPlanted) return true;
        }
        return false;
    }

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
    public void RPC_PlantSeed(NetworkPrefabRef plantPrefab)
    {
        for (int i = 0; i < numSlots; i++)
        {
            var slot = plantSlots[i];
            if (!slot.isPlanted)
            {
                slot.assignedPrefab = plantPrefab;
                slot.isPlanted = true;
                slot.isWatered = false;
                slot.isGrown = false;
                slot.growthTimer = 5f;
                plantSlots.Set(i, slot);
                return;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
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

    private void GrowPlant(PlantSlot slot, int index)
    {
        slot.isGrown = true;
        slot.isWatered = false;
        plantSlots.Set(index, slot);

        Transform spawnPoint = transform.GetChild(index);
        NetworkObject newPlant = Runner.Spawn(slot.assignedPrefab, spawnPoint.position, spawnPoint.rotation);

        if (newPlant.TryGetComponent<Flower>(out var flower))
        {
            flower.SetFlowerColor((int)slot.selectedColor);
            flower.ParentBed = this;
            flower.SlotIndex = index;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ClearSlot(int index)
    {
        var slot = plantSlots[index];
        slot.isPlanted = false;
        slot.isWatered = false;
        slot.isGrown = false;
        plantSlots.Set(index, slot);
    }
}