using UnityEngine;

[CreateAssetMenu(fileName = "InteractionSounds", menuName = "Scriptable Objects/InteractionSounds")]
public class InteractionSounds : ScriptableObject
{
    public AudioClip teleportSFX;
    public AudioClip bagSFX;
    public AudioClip bagUseSFX;
    public AudioClip potSFX;
    public AudioClip watercanPourSFX;
    public AudioClip watercanFillSFX;
    public AudioClip watercanPickupSFX;
}
