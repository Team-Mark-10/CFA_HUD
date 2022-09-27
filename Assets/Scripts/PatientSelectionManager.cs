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

public class PatientSelectionUpdatedEventArgs : EventArgs
{
    public Dictionary<string, bool> PatientActivation { get; private set; }

    public PatientSelectionUpdatedEventArgs(Dictionary<string, bool> patientActivation)
    {
        PatientActivation = patientActivation;
    }
}
public class PatientSelectionManager : MonoBehaviour
{
    public GameObject togglePrefab;
    public GridObjectCollection toggleParent;
    public TMP_Text debugText;


    private List<PatientSelectorToggle> toggles;
    private BluetoothLEHRMParser parser;

    private readonly List<Patient> creationQueue = new();
    private bool isOutputDirty = false;

    public event EventHandler<PatientSelectionUpdatedEventArgs> PatientSelectionUpdated;

    protected virtual void OnPatientSelectionUpdated()
    {
        Dictionary<string, bool> activation = new();

        foreach(var toggle in toggles)
        {
            activation.Add(toggle.Patient.Advertiser.Address.ToString(), toggle.IsToggled);
        }

        Debug.Log(activation.ToString());

        PatientSelectionUpdated.Invoke(this, new PatientSelectionUpdatedEventArgs(activation));

        isOutputDirty = true;
    }

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

    void OnButtonToggle()
    {
        Debug.Log("Button Toggled");
        OnPatientSelectionUpdated();
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
        StartCoroutine(InstantiateToggleCoroutine(patient));

    }

    private IEnumerator InstantiateToggleCoroutine(Patient patient)
    {
        var toggleGameObject = Instantiate(togglePrefab, toggleParent.transform);
        
        PatientSelectorToggle toggleComponent = toggleGameObject.GetComponent<PatientSelectorToggle>();

        toggleComponent.Patient = patient;

        toggleComponent.AddToggleSelectedListener(OnButtonToggle);
        toggleComponent.AddToggleDeselectedListener(OnButtonToggle);

        toggles.Add(toggleComponent);
        yield return new WaitForEndOfFrame();
        toggleParent.UpdateCollection();
        yield return new WaitForEndOfFrame();
        GetComponentInChildren<ScrollingObjectCollection>().UpdateContent();

        Debug.Log("Toggle instantiated");
    }

    void OnAdvertiserAdded(object sender, AdvertiserAddedEventArgs args)
    {
        Debug.Log("OnAdvertiserAdded");
        creationQueue.Add(new Patient(args.Advertiser.LocalName, args.Advertiser));
    }

}
