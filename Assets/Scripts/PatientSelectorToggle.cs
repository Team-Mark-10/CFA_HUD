using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFA_HUD
{
    /// <summary>
    /// A toggle on the patient selector.
    /// </summary>
    public class PatientSelectorToggle : MonoBehaviour, IPatientUser
    {
        private Patient patient;
        public Patient Patient { get => patient; private set => patient = value; }


        public TMPro.TMP_Text nameText;
        public bool IsToggled
        {
            get { return GetComponent<Interactable>().IsToggled; }
            set { GetComponent<Interactable>().IsToggled = value; }
        }

        public void AddToggleSelectedListener(UnityEngine.Events.UnityAction call)
        {
            GetComponent<Interactable>().GetReceiver<InteractableOnToggleReceiver>().OnSelect.AddListener(call);
        }

        public void AddToggleDeselectedListener(UnityEngine.Events.UnityAction call)
        {
            GetComponent<Interactable>().GetReceiver<InteractableOnToggleReceiver>().OnDeselect.AddListener(call);

        }

        // Start is called before the first frame update
        void Start()
        {
            nameText.text = Patient.Alias;
        }

        public void SetPatient(Patient p)
        {
            Patient = p;
        }
    }

}