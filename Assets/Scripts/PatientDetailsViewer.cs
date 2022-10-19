using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CFA_HUD;
using System.Text;
using Microsoft.MixedReality.Toolkit.UI;

public class PatientDetailsViewer : MonoBehaviour
{
    public PatientButtonList buttonList;
    public PressableButtonHoloLens2 editButton;

    public TMPro.TMP_Text nameText;
    public TMPro.TMP_Text detailsText;

    private bool isEditing = false;
    public bool IsEditing => isEditing;


    public Patient Patient { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        buttonList.PatientPressed += OnButtonListPatientPressed;

        editButton.ButtonPressed.AddListener(ToggleEditing);
    }

    public void ToggleEditing()
    {
        if (isEditing) StopEditing();
        else StartEditing();
    }


    private void StartEditing()
    {
        isEditing = true;


    }

    private bool StopEditing()
    {
        isEditing = false;

        return !isEditing;
    }

    private void OnButtonListPatientPressed(object sender, PatientBroadcastEventArgs e)
    {
        if (!IsEditing)
        {
            Patient = e.Patient;
            RenderDetails();
        }
    }

    private void RenderDetails()
    {
        nameText.text = Patient.Alias;

        StringBuilder s = new();

        s.AppendJoin(":", "Device", Patient.Advertiser.LocalName);
        s.AppendLine();

        s.AppendLine("Custom Data");

        foreach(var ad in Patient.Data)
        {
            s.AppendJoin(":", ad.GetName(), ad.ToDisplayFormat());
        }

        detailsText.text = s.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
