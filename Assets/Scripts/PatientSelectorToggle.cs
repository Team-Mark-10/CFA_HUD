using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientSelectorToggle : MonoBehaviour
{
    private Patient patient;
    public Patient Patient { get => patient; set => patient = value; }

    public bool IsToggled
    {
        get { return GetComponent<Interactable>().IsToggled; }
        set { GetComponent<Interactable>().IsToggled = value; }
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
        GetComponentInChildren<TextMesh>().text = Patient.Name;

        var receiver = GetComponent<Interactable>().GetReceiver<InteractableOnToggleReceiver>();

        receiver.OnSelect.AddListener(() => OnToggleSelected(new EventArgs()));
        receiver.OnDeselect.AddListener(() => OnToggleDeselected(new EventArgs()));

    }

    // Update is called once per frame
    void Update()
    {

    }
}
