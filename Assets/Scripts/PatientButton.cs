using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CFA_HUD;
public class PatientButton : MonoBehaviour
{
    public Patient Patient { get;  set; }

    public TMP_Text mainText;

    // Start is called before the first frame update
    void Start()
    {
        mainText.text = Patient.Alias;
    }
}
