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
        public GridObjectCollection toggleParent;   
        public GameObject togglePrefab;

        public GameObject graphParent;
        public GameObject graphPrefab;

        private Vector3 spawnPosition = new(0f,0f,0.5f);

        private BluetoothLEHRMParser parser;
        private bool queueRegen = false;

        private List<WindowGraph> graphs;
        public void Start()
        {
            parser = GetComponentInParent<BluetoothLEHRMParser>();

            parser.NewServiceIDReceived += OnNewServiceId;

            RegenerateFields(parser.GetServiceIDs());
        }

        private void OnNewServiceId(object sender, NewServiceIDEventArgs e)
        {
            queueRegen = true;
        }

        private void Update()
        {
            if(queueRegen)
            {
                RegenerateFields(parser.GetServiceIDs());
                queueRegen = false;
            }
        }

        /// <summary>
        /// Regenerates the toggles for the given service id list.
        /// </summary>
        /// <param name="ServiceIDList"></param>
        private void RegenerateFields(List<string> ServiceIDList)
        {
            foreach(Transform child in toggleParent.transform)
            {
                Destroy(child);
            }

            foreach (var data in ServiceIDList)
            {
                InstanceToggle(data);
            }
        }


        private void InstanceToggle(string data)
        {
            StartCoroutine(InstantiateGraphAndToggle(data));
        }

        private IEnumerator InstantiateGraphAndToggle(string data)
        {
            var newToggle = Instantiate(togglePrefab, toggleParent.transform);

            var nt = newToggle.GetComponent<ServiceIdToggle>();
            nt.ServiceID = data;

            var found = false;

            foreach (var graph in graphs)
            {
                if(graph.ServiceId == nt.ServiceID)
                {
                    nt.ServiceGraph = graph.transform.parent.gameObject;
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                var newGraph = Instantiate(graphPrefab, spawnPosition, Quaternion.identity, graphParent.transform);
                var wg = newGraph.GetComponentInChildren<WindowGraph>();
                wg.ServiceId = data;
                wg.parserGO = parser.gameObject;
                // Sets the next spawn position to have a slight offset.
                spawnPosition.Set(newGraph.transform.position.x - 0.01f, newGraph.transform.position.y - 0.01f, newGraph.transform.position.z - 0.01f);

                nt.ServiceGraph = newGraph;

                graphs.Add(wg);
            }

            var scrollObject = toggleParent.GetComponentInParent<ScrollingObjectCollection>();

            scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
            toggleParent.UpdateCollection();
            yield return new WaitForEndOfFrame();
            scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
            scrollObject.UpdateContent();

        }
    }
}