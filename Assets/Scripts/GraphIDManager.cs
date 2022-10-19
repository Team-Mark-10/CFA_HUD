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
    

        public GridObjectCollection arbitraryDataParent;

        private BluetoothLEHRMParser ServiceIDList;

        public GameObject BackplatePrefab;

        public GameObject GraphParent;

        Vector3 SpawnMyPosition = new Vector3(0f,0f,0.5f);


        public void Start()
        {
          List<string> ServiceIDList = new List<string>();
            ServiceIDList.Add("0D-18");
            ServiceIDList.Add("13-27");


            RegenerateFields(ServiceIDList);
        }

        //prehaps do list of data and iterate through
        private void RegenerateFields(List<string> ServiceIDList)
        {
            foreach (var data in ServiceIDList)
            {
                InstantiateArbitraryDataField(data);
            }
        }


        private void InstantiateArbitraryDataField(string data)
        {
            StartCoroutine(InstantiateArbitraryDataFieldCoroutine(data));
        }

        private IEnumerator InstantiateArbitraryDataFieldCoroutine(string data)
        { 
            var newInstanceBackplate = Instantiate(BackplatePrefab, SpawnMyPosition, Quaternion.identity, GraphParent.transform);

            SpawnMyPosition.Set(newInstanceBackplate.transform.position.x-0.01f, newInstanceBackplate.transform.position.y-0.01f, newInstanceBackplate.transform.position.z - 0.01f);


            //Slightly infront of previous graph not yet implemented


            //set new instance serviceID to data

            newInstanceBackplate.GetComponentInChildren<WindowGraph>().ServiceId = data;

            var scrollObject = arbitraryDataParent.GetComponentInParent<ScrollingObjectCollection>();

            scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
            yield return new WaitForEndOfFrame();
            scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
            arbitraryDataParent.UpdateCollection();
            scrollObject.UpdateContent();

        }
    }
}