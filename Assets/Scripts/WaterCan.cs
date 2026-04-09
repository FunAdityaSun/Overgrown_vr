using System;
using System.Collections;
using UnityEngine;

public class WaterCan : MonoBehaviour
{
    // Define the Enum for your colors
    public enum WaterColor
    {
        White = 0,
        Red = 1,
        Yellow = 2,
        Blue = 3
    }
    public WaterColor selectedColor = WaterColor.White;
    public int uses = 3;


    private SkinnedMeshRenderer meshRenderer;
    private float waterPerUse;

    void Start()
    {
        waterPerUse = 100f / uses;
        uses = 0;

        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        meshRenderer.SetBlendShapeWeight((int)selectedColor, 0);
    }

    public void Use()
    {
        uses--;
        int blendIndex = (int)selectedColor;
        float currentWeight = meshRenderer.GetBlendShapeWeight(blendIndex);
        float targetWeight = currentWeight - waterPerUse;
        StartCoroutine(Animate(blendIndex, currentWeight, targetWeight));
    }

    public void Fill(int newUses)
    {
        meshRenderer.SetBlendShapeWeight(0, 0);
        meshRenderer.SetBlendShapeWeight(1, 0);
        meshRenderer.SetBlendShapeWeight(2, 0);
        meshRenderer.SetBlendShapeWeight(3, 0);

        uses = newUses;
        int blendIndex = (int)selectedColor;
        float currentWeight = meshRenderer.GetBlendShapeWeight(blendIndex);
        float targetWeight = 100f * (newUses / 3f);
        StartCoroutine(Animate(blendIndex, currentWeight, targetWeight));
    }
    public void Change(WaterColor newColor)
    {
        selectedColor = newColor;
        Fill(uses); // dont change water level
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

        if (uses <= 0)
        {
            Debug.Log($"{selectedColor} water is empty!");
        }
    }
}