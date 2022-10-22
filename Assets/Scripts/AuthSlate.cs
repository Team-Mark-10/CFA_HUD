using CFA_HUD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

// A script managing the API authentication slate.
public class AuthSlate : MonoBehaviour
{
    private const string ERROR_MSG = "Invalid url";
    public BluetoothLEHRMParser parser;

    public MRTKUGUIInputField apiField;
    public MRTKUGUIInputField username;
    public MRTKUGUIInputField password;

    public TMPro.TMP_Text errorText;

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

    /// <summary>
    /// Event handler for the login button.
    /// </summary>
    public void OnSubmitAuth()
    {
        errorText.text = "";

        var valid = true;
        if (apiField.text.Length > 0)
        {
            valid = parser.SetAPIAddress(apiField.text);
        }

        if (!valid)
        {
            errorText.text = ERROR_MSG;

        } else
        {
            if (username.text.Length > 0 && password.text.Length > 0)
            {
                parser.SetNewLoginDetails(username.text, password.text);
            }
        }
       
    }
}
