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

        private bool isOutputDirty = false;

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

            isOutputDirty = true;
        }


        // Update is called once per frame
        public override void Update()
        {
            base.Update();

            if (isOutputDirty)
            {
                UpdateText();
                isOutputDirty = false;
            }
        }

        protected override void AddNewGameObject(GameObject newInstance, Patient patient)
        {
            var toggle = newInstance.GetComponent<PatientSelectorToggle>();

            toggle.Patient = patient;

            toggle.AddToggleSelectedListener(OnButtonToggle);
            toggle.AddToggleDeselectedListener(OnButtonToggle);

            UpdateText();

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

            foreach (var t in Interactables)
            {
                var toggle = t.GetComponent<PatientSelectorToggle>();
                newText += $"{toggle.Patient.Alias} => {t.IsToggled}, ";
            }

        }

    }

}
