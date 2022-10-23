using CFA_HUD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine.UI;
using System;
// A script managing the API authentication slate.
public class AuthSlate : MonoBehaviour
{
    private const string ERROR_MSG = "Invalid url";
    public BluetoothLEHRMParser parser;

    public Text apiField;
    public Text username;
    public Text password;

    public TMP_Text errorText;
    public TMP_Text statusText;

    // Start is called before the first frame update
    void Start()
    {
        statusText.text = "Not connected";

        if (parser == null)
        {
            parser = GetComponentInParent<BluetoothLEHRMParser>();

            parser.DBConnectionSuccess += OnDBConnectionSucces;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDBConnectionSucces (object sender, bool success)
    {
        if (success) statusText.text = "Connection Success";
        else statusText.text = "Failed connection";
    }

    /// <summary>
    /// Event handler for the login button.
    /// </summary>
    public void OnSubmitAuth()
    {

        var valid = true;
        if (apiField.text.Length > 0)
        {
            statusText.text = "Testing connection";
            errorText.text = "";

            valid = parser.SetAPIAddress(apiField.text);
        }

        if (!valid)
        {
            errorText.text = ERROR_MSG;

        } else
        {
            if (username.text.Length > 0 && password.text.Length > 0)
            {
                statusText.text = "Testing connection";
                errorText.text = "";

                parser.SetNewLoginDetails(username.text, password.text);
            }
        }

    }
}
