using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CFA_HUD
{
    /// <summary>
    /// A script that manages the editing of a patient's arbitrary data.
    /// </summary>
    public class ArbitraryDataManager : MonoBehaviour
    {
        /// <summary>
        /// The prefab representing a data field. Must have a top-level ArbitraryInputField component.
        /// </summary>
        public GameObject arbitraryDataFieldPrefab;

        /// <summary>
        /// The collection to add the field instances to.
        /// </summary>
        public GameObject arbitraryDataParent;

        /// <summary>
        /// Regenerates the editor view.
        /// </summary>
        /// <param name="patient"></param>
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
                InstantiateArbitraryDataFieldCoroutine(data);
        }

        private void InstantiateArbitraryDataFieldCoroutine(IArbitraryData data)
        {
            var newInstance = Instantiate(arbitraryDataFieldPrefab, arbitraryDataParent.transform);

            newInstance.GetComponent<ArbitraryInputField>().ArbitraryData = data;

            //var scrollObject = arbitraryDataParent.GetComponentInParent<ScrollingObjectCollection>();

            //scrollObject.GetComponentInChildren<ClippingBox>().enabled = true;
            //yield return new WaitForEndOfFrame();
            //scrollObject.GetComponentInChildren<ClippingBox>().enabled = false;
            //arbitraryDataParent.UpdateCollection();
            //scrollObject.UpdateContent();

        }
    }
}