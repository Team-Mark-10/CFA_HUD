using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientSelectorToggle : MonoBehaviour
{
    [SerializeField]
    private string patientName;

    public string PatientName { get => patientName; private set => patientName = value; }
    public ulong BluetoothId { get; private set; }
    public bool IsToggled
    {
        get { return GetComponent<Interactable>().IsToggled; }
    }

    public event EventHandler ToggleSelected;
    public event EventHandler ToggleDeselected;


    protected virtual void OnToggleSelected(EventArgs e)
    {
        EventHandler handler = ToggleSelected;
        handler.Invoke(this, e);
    }

    protected virtual void OnToggleDeselected(EventArgs e)
    {
        EventHandler handler = ToggleDeselected;
        handler.Invoke(this, e);
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponentInChildren<TextMesh>().text = PatientName;

        var receiver = GetComponent<Interactable>().GetReceiver<InteractableOnToggleReceiver>();

        receiver.OnSelect.AddListener(() => OnToggleSelected(new EventArgs()));
        receiver.OnDeselect.AddListener(() => OnToggleDeselected(new EventArgs()));

    }

    public void SetDetails(string name, ulong bluetoothId)
    {
        patientName = name;
        BluetoothId = bluetoothId;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
