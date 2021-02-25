using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroStatBox : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI statText;

    public void SetStatBoxValues(string text)
    {
        statText.text = text;
    }
}
