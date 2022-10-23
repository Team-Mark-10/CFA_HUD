using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// A script for the selection of patients on the admin slate. This controls which lines are visible on the graphs.
    /// </summary>
    /// 
    [RequireComponent(typeof(PatientButtonList))]
    public class PatientSelectionManager : MonoBehaviour
    {
        public event EventHandler<PatientSelectionUpdatedEventArgs> PatientSelectionUpdated;


        private PatientButtonList buttonList;

        private void Start()
        {
            buttonList = GetComponent<PatientButtonList>();

            buttonList.PatientPressed += (_,_) => OnPatientSelectionUpdated();
        }

        protected virtual void OnPatientSelectionUpdated()
        {
            Dictionary<string, bool> activation = new();

            foreach (var toggle in buttonList.Interactables.Select(x => x.GetComponent<PatientSelectorToggle>())) 
            {
                activation.Add(toggle.Patient.Advertiser.Address.ToString(), toggle.IsToggled);
            }


            PatientSelectionUpdated.Invoke(this, new PatientSelectionUpdatedEventArgs(activation));
        }

    }

}
