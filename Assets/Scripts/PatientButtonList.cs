using CFA_HUD;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PatientButtonList : PatientAddedInstancer, IPatientBroadcaster
{
    public event EventHandler<PatientBroadcastEventArgs> PatientPressed;

    public void AddListener(EventHandler<PatientBroadcastEventArgs> handler)
    {
        PatientPressed += handler;
    }

    protected override void AddNewGameObject(GameObject newInstance, Patient patient)
    {
        base.AddNewGameObject(newInstance, patient);
        newInstance.GetComponent<Interactable>().OnClick.AddListener(() => OnPatientPressed(patient));
    }

    private void OnPatientPressed(Patient patient)
    {
        PatientPressed.Invoke(this, new PatientBroadcastEventArgs(patient));
    }
}
