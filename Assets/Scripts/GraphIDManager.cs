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
        public GameObject arbitraryDataFieldPrefab;
        public GridObjectCollection arbitraryDataParent;

        private BluetoothLEHRMParser ServiceIDList;

        public void Start()
        {
          List<string> ServiceIDList = new List<string>();
            ServiceIDList.Add("test1");

            ServiceIDList.Add("test2");
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
            var newInstance = Instantiate(DataSelection, arbitraryDataParent.transform);

            newInstance.GetComponent<ArbitraryInputField>().ArbitraryData = data;

            var scrollObject = arbitraryDataParent.GetComponentInParent<ScrollingObjectCollection>();

            scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
            yield return new WaitForEndOfFrame();
            scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
            arbitraryDataParent.UpdateCollection();
            scrollObject.UpdateContent();

        }
    }
}