using System;
using System.Collections;
using UnityEngine;

public class DyeSack : MonoBehaviour
{
    public enum WaterColor
    {
        White = 0,
        Red = 1,
        Yellow = 2,
        Blue = 3
    }
    public WaterColor selectedColor = WaterColor.White;
}