using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlateToggleButton : MonoBehaviour
{
    public GameObject target;
    private void Start()
    {
        GetComponent<PressableButtonHoloLens2>().ButtonPressed.AddListener(() => ToggleVisible());

    }

    public void ToggleVisible()
    {
        target.SetActive(!target.activeInHierarchy);
    }
}

 
   
