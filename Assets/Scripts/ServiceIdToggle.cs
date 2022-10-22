using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A toggle that switches a graph on or off.
/// </summary>

public class ServiceIdToggle : MonoBehaviour
{
    public string ServiceID { get; set; }
    public GameObject ServiceGraph { get; set; }

    public TMPro.TMP_Text text;

    private bool state;

    // Start is called before the first frame update
    void Start()
    {
        text.text = ServiceID;

        state = ServiceGraph.gameObject.activeSelf;

        GetComponent<Interactable>().OnClick.AddListener(() => OnToggle());
    }

    public void OnToggle()
    {
        ServiceGraph.gameObject.SetActive(!state);
        state = !state;
    }

}
