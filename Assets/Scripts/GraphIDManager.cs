using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CFA_HUD
{
    public class GraphIDManager : MonoBehaviour
    {
        public BluetoothLEHRMParser parser;


        public GridObjectCollection toggleParent;
        public GameObject togglePrefab;

        public GameObject BackplatePrefab;

        public GameObject GraphParent;

        Vector3 SpawnMyPosition = new Vector3(0f,0f,0.5f);
        private List<string> ServiceIDList = new List<string>();


        public void Start()
        {
            ServiceIDList.Add("0D-18");
            ServiceIDList.Add("13-27");

            parser.NewServiceIDReceived += OnNewServiceId;

            RegenerateFields(ServiceIDList);
        }

        private void OnNewServiceId(object sender, NewServiceIDEventArgs e)
        {
            ServiceIDList.Add(e.Data);
            RegenerateFields(ServiceIDList);
        }

        //prehaps do list of data and iterate through
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
            var newInstanceBackplate = Instantiate(BackplatePrefab, SpawnMyPosition, Quaternion.identity, GraphParent.transform);

            SpawnMyPosition.Set(newInstanceBackplate.transform.position.x-0.01f, newInstanceBackplate.transform.position.y-0.01f, newInstanceBackplate.transform.position.z - 0.01f);

            var newToggle = Instantiate(togglePrefab, toggleParent.transform);

            //Slightly infront of previous graph not yet implemented


            //set new instance serviceID to data

            var wg = newInstanceBackplate.GetComponentInChildren<WindowGraph>();
            wg.ServiceId = data;
            wg.parserGO = parser.gameObject;

            var scrollObject = toggleParent.GetComponentInParent<ScrollingObjectCollection>();

            scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
            yield return new WaitForEndOfFrame();
            scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
            toggleParent.UpdateCollection();
            scrollObject.UpdateContent();

        }
    }
}