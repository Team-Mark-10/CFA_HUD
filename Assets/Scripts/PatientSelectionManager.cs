using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CFA_HUD
{
    public class PatientSelectionUpdatedEventArgs : EventArgs
    {
        public Dictionary<string, bool> PatientActivation { get; private set; }

        public PatientSelectionUpdatedEventArgs(Dictionary<string, bool> patientActivation)
        {
            PatientActivation = patientActivation;
        }
    }
    public class PatientSelectionManager : PatientAddedInstancer
    {
        public event EventHandler<PatientSelectionUpdatedEventArgs> PatientSelectionUpdated;

        protected virtual void OnPatientSelectionUpdated()
        {
            Dictionary<string, bool> activation = new();

            foreach (var toggle in Interactables)
            {
                activation.Add(toggle.GetComponent<PatientSelectorToggle>().Patient.Advertiser.Address.ToString(), toggle.IsToggled);
            }

            Debug.Log(activation.ToString());

            PatientSelectionUpdated.Invoke(this, new PatientSelectionUpdatedEventArgs(activation));
        }

        protected override void AddNewGameObject(GameObject newInstance, Patient patient)
        {
            var toggle = newInstance.GetComponent<PatientSelectorToggle>();

            toggle.Patient = patient;

            toggle.AddToggleSelectedListener(() => OnPatientSelectionUpdated());
            toggle.AddToggleDeselectedListener(() => OnPatientSelectionUpdated());
        }

    }

}
