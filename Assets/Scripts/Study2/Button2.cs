using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button2 : MonoBehaviour
{  
    public void ChangeColorWhite()
    {
        gameObject.GetComponent<Image>().color = Color.white;
    }

    public void ChangeColorRed()
    {
        gameObject.GetComponent<Image>().color = Color.red;
    }
}
