using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PatientSelectionManager : MonoBehaviour
{
    public BluetoothLEHRMParser parser;
    public GameObject togglePrefab;
    public TMP_Text debugText;


    private List<PatientSelectorToggle> toggles;


    void Start()
    {
        toggles = new List<PatientSelectorToggle>();

        parser.AdvertiserAdded += OnAdvertiserAdded;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void OnButtonToggle(object sender, bool isSelected)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        string newText = "State: ";

        foreach(var t in toggles)
        {
            newText += $"{t.PatientName} => {t.IsToggled}";
        }
    }

    void OnAdvertiserAdded(object sender, AdvertiserAddedEventArgs args)
    {
        var toggleGameObject = Instantiate(togglePrefab);

        PatientSelectorToggle toggleComponent = toggleGameObject.GetComponent<PatientSelectorToggle>();

        toggleComponent.SetDetails(args.Advertiser.LocalName, args.Advertiser.Address);

        toggleComponent.ToggleSelected += (sender, e) => OnButtonToggle(sender, true);
        toggleComponent.ToggleDeselected += (sender, e) => OnButtonToggle(sender, false);

        toggles.Add(toggleComponent);
    }

}
