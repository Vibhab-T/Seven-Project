using UnityEngine;
using TMPro;

public class WaypointIndicator : MonoBehaviour
{
    public TextMeshPro numberText;
    public GameObject arrowObject;

    public void SetOrderNumber(int number)
    {
        if (numberText != null)
        {
            numberText.text = number.ToString();
        }
    }
}