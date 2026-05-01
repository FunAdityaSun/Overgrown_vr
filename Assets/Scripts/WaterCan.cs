using Fusion;
using UnityEngine;
using System.Collections;

public class WaterCan : NetworkBehaviour
{
    public enum WaterColor
    {
        White = 0,
        Red = 1,
        Yellow = 2,
        Blue = 3
    }

    [Networked, OnChangedRender(nameof(OnColorChanged))]
    public WaterColor selectedColor { get; set; }

    [Networked, OnChangedRender(nameof(OnUsesChanged))]
    public int uses { get; set; }

    private SkinnedMeshRenderer meshRenderer;
    private Coroutine activeAnimation;

    public override void Spawned()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        RefreshVisuals();
    }

    public void Use()
    {
        uses--;
    }

    public void Fill(int newUses)
    {
        RPC_SetFill(newUses);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetFill(int newUses)
    {
        uses = newUses;
    }

    public void Change(WaterColor newColor)
    {
        RPC_Change(newColor);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Change(WaterColor newColor)
    {
        selectedColor = newColor;
        Fill(uses);
    }

    void OnUsesChanged()
    {
        int blendIndex = (int)selectedColor;
        float currentWeight = meshRenderer.GetBlendShapeWeight(blendIndex);
        float targetWeight = (uses / 3f) * 100f;

        if (activeAnimation != null) StopCoroutine(activeAnimation);
        activeAnimation = StartCoroutine(Animate(blendIndex, currentWeight, targetWeight));
    }
    void OnColorChanged()
    {
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (meshRenderer == null) return;

        for (int i = 0; i < 4; i++)
        {
            meshRenderer.SetBlendShapeWeight(i, 0);
        }

        float targetWeight = (uses / 3f) * 100f;
        meshRenderer.SetBlendShapeWeight((int)selectedColor, targetWeight);
    }

    IEnumerator Animate(int index, float start, float end)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float current = Mathf.Lerp(start, end, elapsed / duration);
            meshRenderer.SetBlendShapeWeight(index, current);
            yield return null;
        }

        meshRenderer.SetBlendShapeWeight(index, end);
    }
}