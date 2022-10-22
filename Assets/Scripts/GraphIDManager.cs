using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFA_HUD
{

    /// <summary>
    /// The manager for the data selection list on the admin slate. Instances a new graph and toggle when a new data type is received.
    /// </summary>
    public class GraphIDManager : MonoBehaviour
    {
        public BluetoothLEHRMParser parser;

        public GridObjectCollection toggleParent;   
        public GameObject togglePrefab;

        public GameObject graphParent;
        public GameObject graphPrefab;


        private Vector3 spawnPosition = new(0f,0f,0.5f);


        public void Start()
        {
            parser.NewServiceIDReceived += OnNewServiceId;

            RegenerateFields(parser.GetServiceIDs());
        }

        private void OnNewServiceId(object sender, NewServiceIDEventArgs e)
        {
            RegenerateFields(parser.GetServiceIDs());
        }

        /// <summary>
        /// Regenerates the toggles for the given service id list.
        /// </summary>
        /// <param name="ServiceIDList"></param>
        private void RegenerateFields(List<string> ServiceIDList)
        {
            foreach (var data in ServiceIDList)
            {
                InstanceToggle(data);
            }
        }


        private void InstanceToggle(string data)
        {
            StartCoroutine(InstanceToggleCoroutine(data));
        }

        private IEnumerator InstanceToggleCoroutine(string data)
        { 
            var newInstanceBackplate = Instantiate(graphPrefab, spawnPosition, Quaternion.identity, graphParent.transform);

            // Sets the next spawn position to have a slight offset.
            spawnPosition.Set(newInstanceBackplate.transform.position.x-0.01f, newInstanceBackplate.transform.position.y-0.01f, newInstanceBackplate.transform.position.z - 0.01f);

            var newToggle = Instantiate(togglePrefab, toggleParent.transform);

            var wg = newInstanceBackplate.GetComponentInChildren<WindowGraph>();
            wg.ServiceId = data;
            wg.parserGO = parser.gameObject;

            var nt = newToggle.GetComponent<ServiceIdToggle>();
            nt.ServiceGraph = newInstanceBackplate;
            nt.ServiceID = data;

            var scrollObject = toggleParent.GetComponentInParent<ScrollingObjectCollection>();

            scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
            yield return new WaitForEndOfFrame();
            scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
            toggleParent.UpdateCollection();
            scrollObject.UpdateContent();

        }
    }
}