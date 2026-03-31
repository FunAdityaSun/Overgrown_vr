using UnityEngine;

public class ToggleOutline : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Outline>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onPointerEnter()
    {
        GetComponent<Outline>().enabled = true;
    }
}
