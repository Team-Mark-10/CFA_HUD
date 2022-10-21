using CFA_HUD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
public class AuthSlate : MonoBehaviour
{
    public BluetoothLEHRMParser parser;

    public MRTKUGUIInputField username;
    public MRTKUGUIInputField password;

    // Start is called before the first frame update
    void Start()
    {
        if (parser == null)
        {
            parser = GetComponentInParent<BluetoothLEHRMParser>();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSubmitAuth()
    {
        if (username.text.Length > 0 && password.text.Length > 0)
        {
            parser.SetNewLoginDetails(username.text, password.text);
        }
    }
}
