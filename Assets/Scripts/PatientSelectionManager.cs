using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class Patient
{
    public string Name { get; private set; }
    public BLEAdvertiser Advertiser { get; private set; }

    public Patient(string name, BLEAdvertiser advertiser)
    {
        Name = name;
        Advertiser = advertiser;
    }
}
public class PatientSelectionManager : MonoBehaviour
{
    public GameObject togglePrefab;
    public GameObject toggleParent;
    public TMP_Text debugText;


    private List<PatientSelectorToggle> toggles;
    private BluetoothLEHRMParser parser;

    private List<Patient> creationQueue = new();
    private bool isOutputDirty = false;

    void Start()
    {
        toggles = new();

        parser = GetComponentInParent<BluetoothLEHRMParser>();
        parser.AdvertiserAdded += OnAdvertiserAdded;

        var advertisers = parser.GetAdvertisers();

        foreach(var a in advertisers)
        {
            InstantiateToggle(new Patient(a.LocalName, a));
        }

        Debug.Log("Manager Start Finished");
    }


    // Update is called once per frame
    void Update()
    {


        if (creationQueue.Count > 0)
        {
            foreach (var patient in creationQueue)
            {
                InstantiateToggle(patient);
            }

            creationQueue.Clear();
            UpdateText();
        }

        if (isOutputDirty)
        {
            UpdateText();
            isOutputDirty = false;
        }
    }

    void OnButtonToggle(object sender, bool isSelected)
    {
        isOutputDirty = true;
    }

    private void UpdateText()
    {
        string newText = "State: ";

        foreach(var t in toggles)
        {
            newText += $"{t.Patient.Name} => {t.IsToggled}, ";
        }

        debugText.text = newText;
    }

    private void InstantiateToggle(Patient patient)
    {
        var toggleGameObject = Instantiate(togglePrefab, toggleParent.transform);
        
        PatientSelectorToggle toggleComponent = toggleGameObject.GetComponent<PatientSelectorToggle>();

        toggleComponent.Patient = patient;

        toggleComponent.ToggleSelected += (sender, e) => OnButtonToggle(sender, true);
        toggleComponent.ToggleDeselected += (sender, e) => OnButtonToggle(sender, false);

        toggles.Add(toggleComponent);

        toggleParent.GetComponentInParent<GridObjectCollection>().UpdateCollection();

        Debug.Log("Toggle instantiated");
    }

    void OnAdvertiserAdded(object sender, AdvertiserAddedEventArgs args)
    {
        Debug.Log("OnAdvertiserAdded");
        creationQueue.Add(new Patient(args.Advertiser.LocalName, args.Advertiser));
    }

}
