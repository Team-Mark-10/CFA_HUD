using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientSelector : MonoBehaviour
{
    public BluetoothLEHRMParser parser;

    public GameObject radioPrefab;

    
    // Start is called before the first frame update
    void Start()
    {
        parser.AdvertiserAdded += OnAdvertisersChanged;
    }

    public void OnAdvertisersChanged (object sender, EventArgs args)
    {
        RegenerateRadioList(parser.GetAdvertisers());
    }

    private void RegenerateRadioList(List<BLEAdvertiser> advertisers)
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
