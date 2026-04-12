using Fusion;
using UnityEngine;

public class Flower : NetworkBehaviour
{
    [Networked] private int selectedColor { get; set; }
    [SerializeField] private Material[] flowerMaterials;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    public override void Spawned()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        ApplyMaterial();
    }
    public override void Render()
    {
        if (skinnedMeshRenderer != null && flowerMaterials.Length > 0)
        {
            ApplyMaterial();
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (skinnedMeshRenderer != null && flowerMaterials.Length > 0)
        {
            ApplyMaterial();
        }
    }
    private void ApplyMaterial()
    {
        skinnedMeshRenderer.material = flowerMaterials[selectedColor];
    }
    public void SetFlowerColor(int colorIndex)
    {
        selectedColor = colorIndex;
        ApplyMaterial();
    }
    public int GetCurrentColorIndex()
    {
        return selectedColor;
    }
}