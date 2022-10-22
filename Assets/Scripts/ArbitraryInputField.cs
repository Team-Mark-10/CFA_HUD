using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using CFA_HUD;

/// <summary>
/// Represents a data field to be used in the arbitrary data field editor.
/// </summary>
public class ArbitraryInputField : MonoBehaviour
{
    public TMPro.TMP_Text nameText;
    public MRTKUGUIInputField MRTKUGUIInputField;

    public IArbitraryData ArbitraryData { get ; set ; }
    
    // Start is called before the first frame update
    void Start()
    {
        nameText.text = ArbitraryData.GetName();
        MRTKUGUIInputField.text = ArbitraryData.ToDisplayFormat();
        MRTKUGUIInputField.onEndEdit.AddListener(OnEndEdit);
    }

    void OnEndEdit(string newValue)
    {

        if(!ArbitraryData.TrySetValue(newValue))
        {
            MRTKUGUIInputField.text = ArbitraryData.ToDisplayFormat();
        };
        Debug.Log(ArbitraryData.ToDisplayFormat());
    }
}
