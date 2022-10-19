using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CFA_HUD
{
    public class ArbitraryDataManager : MonoBehaviour
    {
        public GameObject arbitraryDataFieldPrefab;
        public GridObjectCollection arbitraryDataParent;

        public void Start()
        {
            Patient test = new Patient("asdfas", new BLEAdvertiser(234, "asdfsdf"), new List<IArbitraryData>() { new ArbitraryStringValue("Address", "No address"), new ArbitraryBoolValue("Ceiliac", true)});
            RegenerateFields(test);
        }

        public void RegenerateFields(Patient patient)
        {
            var children = new List<GameObject>();
            foreach (Transform child in arbitraryDataParent.transform) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));


            foreach (var data in patient.Data)
            {
                InstantiateArbitraryDataField(data);
            }
        }


        private void InstantiateArbitraryDataField(IArbitraryData data)
        {
            StartCoroutine(InstantiateArbitraryDataFieldCoroutine(data));
        }

        private IEnumerator InstantiateArbitraryDataFieldCoroutine(IArbitraryData data)
        {
            var newInstance = Instantiate(arbitraryDataFieldPrefab, arbitraryDataParent.transform);

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