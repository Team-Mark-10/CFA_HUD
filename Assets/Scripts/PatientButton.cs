using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CFA_HUD;
public class PatientButton : MonoBehaviour
{
    public Patient Patient { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        GetComponentInChildren<TextMesh>().text = Patient.Alias;
    }
}
