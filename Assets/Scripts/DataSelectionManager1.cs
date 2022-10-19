using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CFA_HUD
{
    public class DataSelectionUpdatedEventArgs : EventArgs
    {
        public Dictionary<string, bool> DataActivation { get; private set; }

        public DataSelectionUpdatedEventArgs(Dictionary<string, bool> dataActivation)
        {
            DataActivation = dataActivation;
        }
    }
    public class DataSelectionManager1 : MonoBehaviour
    {
        public GameObject togglePrefab;
        public GridObjectCollection toggleParent;
        public TMP_Text debugText;


        private List<DataSelectorToggle> toggles;
        private BluetoothLEHRMParser parser;

        private List<string> creationQueue = new List<string>();
      

        private bool isOutputDirty = false;

        public event EventHandler<DataSelectionUpdatedEventArgs> DataSelectionUpdated;

        protected virtual void OnDataSelectionUpdated(object sender, NewServiceIDEventArgs args)
        {
            Dictionary<string, bool> activation = new();

            foreach (var toggle in toggles)
            {
                activation.Add(toggle.Data, toggle.IsToggled);
            }

            Debug.Log(activation.ToString());

            DataSelectionUpdated.Invoke(this, new DataSelectionUpdatedEventArgs(activation));

            isOutputDirty = true;
        }

        void Start()
        {
            toggles = new();

            parser = GetComponentInParent<BluetoothLEHRMParser>();
            parser.NewServiceIDReceived += OnDataSelectionUpdated;

            foreach (var data in parser.GetServiceIDs())
            {
                InstantiateToggle(data);
            }

            Debug.Log("Manager Start Finished");
        }


        // Update is called once per frame
        void Update()
        {
            if (creationQueue.Count > 0)
            {
                foreach (var data in creationQueue)
                {
                    InstantiateToggle(data);
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
           
            isOutputDirty = true;
        }

        private void UpdateText()
        {
            string newText = "State: ";

            foreach (var t in toggles)
            {
                newText += $"{t.Data} => {t.IsToggled}, ";
            }

            debugText.text = newText;
        }

        private void InstantiateToggle(string data)
        {
            StartCoroutine(InstantiateToggleCoroutine(data));

        }

        private IEnumerator InstantiateToggleCoroutine(string data)
        {
            var toggleGameObject = Instantiate(togglePrefab, toggleParent.transform);

            DataSelectorToggle toggleComponent = toggleGameObject.GetComponent<DataSelectorToggle>();

            toggleComponent.Data = data;

            toggleComponent.AddToggleSelectedListener(OnButtonToggle);
            toggleComponent.AddToggleDeselectedListener(OnButtonToggle);

            toggles.Add(toggleComponent);
            yield return new WaitForEndOfFrame();
            toggleParent.UpdateCollection();
            yield return new WaitForEndOfFrame();
            GetComponentInChildren<ScrollingObjectCollection>().UpdateContent();

            Debug.Log("Toggle instantiated");
        }

        void OnAdvertiserAdded(object sender, NewServiceIDEventArgs args)
        {
            Debug.Log("OnAdvertiserAdded");
            creationQueue.Add(args.Data);
        }

    }

}
