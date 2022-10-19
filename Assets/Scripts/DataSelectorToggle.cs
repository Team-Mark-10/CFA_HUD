using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFA_HUD
{
    public class DataSelectorToggle : MonoBehaviour
    {
        private string data;
        public string Data { get => data; set => data = value; }

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
            GetComponentInChildren<TextMesh>().text = Data;
        }
    }

}