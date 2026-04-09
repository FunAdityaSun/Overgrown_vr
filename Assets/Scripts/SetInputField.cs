using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetInputField : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    char inputChar;
    
    public void AddToInputField()
    {
        if (inputField.text.Length < 4)
        {
            inputField.text = inputField.text + inputChar;
        } 
    }

    public void DeleteFromInputField()
    {
        if (inputField.text.Length>0)
        {
            inputField.text = inputField.text.Remove(inputField.text.Length - 1);
        }
    }

    public void HideUI(GameObject ui)
    {
        ui.SetActive(false);
    }
}
