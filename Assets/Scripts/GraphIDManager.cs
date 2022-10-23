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

        public GameObject selectorGO;

        private Vector3 spawnPosition = new(0f,0f,0.5f);

        private BluetoothLEHRMParser parser;
        private bool queueRegen = false;
        private List<string> queuedIds = new();

        private List<WindowGraph> graphs = new();
        public void Start()
        {
            parser = GetComponentInParent<BluetoothLEHRMParser>();

            parser.NewServiceIDReceived += OnNewServiceId;

            foreach(var id in parser.GetServiceIDs())
            {
                StartCoroutine(AddToggleAndGraph(id));
            }

        }

        private void OnNewServiceId(object sender, NewServiceIDEventArgs e)
        {
            queueRegen = true;
            queuedIds.Add(e.Data);
        }

        private void Update()
        {
            if(queueRegen && queuedIds.Count > 0)
            {
                foreach(var id in queuedIds)
                {
                    StartCoroutine(AddToggleAndGraph(id));
                }
              
                ///RegenerateFields(parser.GetServiceIDs());
                queueRegen = false;
                queuedIds.Clear();
            }
        }

        ///// <summary>
        ///// Regenerates the toggles for the given service id list.
        ///// </summary>
        ///// <param name="ServiceIDList"></param>
        //private void RegenerateFields(List<string> ServiceIDList)
        //{
        //    foreach(Transform child in toggleParent.transform)
        //    {
        //        Destroy(child);
        //    }

        //    InstanceToggle(ServiceIDList);
        //}


        //private void InstanceToggle(List<string> data)
        //{
        //    StartCoroutine(InstantiateGraphsAndToggles(data));
        //}

        private IEnumerator AddToggleAndGraph(string id)
        {
            var newToggle = Instantiate(togglePrefab, toggleParent.transform);

            var nt = newToggle.GetComponent<ServiceIdToggle>();
            nt.ServiceID = id;

            var newGraph = Instantiate(graphPrefab, spawnPosition, Quaternion.identity, graphParent.transform);
            var wg = newGraph.GetComponentInChildren<WindowGraph>();
            wg.ServiceId = id;
            wg.parserGO = parser.gameObject;
            wg.selectorGO = selectorGO;
            // Sets the next spawn position to have a slight offset.
            spawnPosition.Set(newGraph.transform.position.x - 0.01f, newGraph.transform.position.y - 0.01f, newGraph.transform.position.z - 0.01f);

            nt.ServiceGraph = newGraph;

            graphs.Add(wg);

            var scrollObject = toggleParent.GetComponentInParent<ScrollingObjectCollection>();

            scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
            toggleParent.UpdateCollection();
            yield return new WaitForEndOfFrame();
            scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
            scrollObject.UpdateContent();

        }

        //private IEnumerator InstantiateGraphsAndToggles(List<string> data)
        //{
        //    yield return new WaitForEndOfFrame();

        //    foreach(var id in data)
        //    {
        //        var newToggle = Instantiate(togglePrefab, toggleParent.transform);

        //        var nt = newToggle.GetComponent<ServiceIdToggle>();
        //        nt.ServiceID = id;

        //        var found = false;

        //        foreach (var graph in graphs)
        //        {
        //            if (graph.ServiceId == nt.ServiceID)
        //            {
        //                nt.ServiceGraph = graph.transform.parent.gameObject;
        //                found = true;
        //                break;
        //            }
        //        }

        //        if (!found)
        //        {
        //            var newGraph = Instantiate(graphPrefab, spawnPosition, Quaternion.identity, graphParent.transform);
        //            var wg = newGraph.GetComponentInChildren<WindowGraph>();
        //            wg.ServiceId = id;
        //            wg.parserGO = parser.gameObject;
        //            wg.selectorGO = selectorGO;
        //            // Sets the next spawn position to have a slight offset.
        //            spawnPosition.Set(newGraph.transform.position.x - 0.01f, newGraph.transform.position.y - 0.01f, newGraph.transform.position.z - 0.01f);

        //            nt.ServiceGraph = newGraph;

        //            graphs.Add(wg);
        //        }

        //    }

        //    var scrollObject = toggleParent.GetComponentInParent<ScrollingObjectCollection>();

        //    scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
        //    toggleParent.UpdateCollection();
        //    yield return new WaitForEndOfFrame();
        //    scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
        //    scrollObject.UpdateContent();

        //}
    }
}