using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CFA_HUD;
using System.Text;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;

/// <summary>
///  The script controlling the patient details viewer.
/// </summary>
public class PatientDetailsViewer : MonoBehaviour
{
    public Patient Patient { get; set; }
    public bool IsEditing => isEditing;
    private bool isEditing = false;

    public PatientButtonList buttonList;
    public PressableButtonHoloLens2 editButton;

    public TMPro.TMP_Text nameText;
    public TMPro.TMP_Text detailsText;

    public GameObject viewer;
    public GameObject editor;

    public MRTKUGUIInputField newFieldNameInputField;

    // Start is called before the first frame update
    void Start()
    {
        buttonList.PatientPressed += OnButtonListPatientPressed;

        editButton.GetComponent<Interactable>().OnClick.AddListener(() => ToggleEditing());

        if (Patient == null)
        {
            viewer.SetActive(false);
            editor.SetActive(false);

            editButton.gameObject.SetActive(false);
        }
    }

    public void ToggleEditing()
    {
        if (isEditing) StopEditing();
        else StartEditing();
    }


    private void StartEditing()
    {

        isEditing = true;

        viewer.SetActive(false);
        editor.SetActive(true);

        editor.GetComponent<ArbitraryDataManager>().RegenerateFields(Patient);

    }

    private bool StopEditing()
    {
        isEditing = false;


        EnableViewer();

        return !isEditing;
    }

    private void EnableViewer()
    {
        RenderDetails();

        editButton.gameObject.SetActive(true);

        viewer.SetActive(true);
        editor.SetActive(false);
    }

    private void OnButtonListPatientPressed(object sender, PatientBroadcastEventArgs e)
    {

        if (!IsEditing)
        {
            Patient = e.Patient;
            editor.GetComponent<ArbitraryDataManager>().RegenerateFields(Patient);

            RenderDetails();
             
            EnableViewer();
        } else
        {
            if (StopEditing()) {
                Patient = e.Patient;
                editor.GetComponent<ArbitraryDataManager>().RegenerateFields(Patient);

                RenderDetails();
            };
        }
    }

    /// <summary>
    /// Renders the arbitary data as a string on the slate.
    /// </summary>
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
            s.AppendLine();
        }

        detailsText.text = s.ToString();
    }

    public void AddField(int type)
    {
        string name = newFieldNameInputField.text;

        if (name.Length > 0 && isEditing && Patient != null)
        {
            switch (type)
            {
                case 0:
                    Patient.Data.Add(new ArbitraryIntValue(name, 0));
                    break;
                case 1:
                    Patient.Data.Add(new ArbitraryFloatValue(name, 0));

                    break;
                case 2:
                    Patient.Data.Add(new ArbitraryBoolValue(name, false));

                    break;
                case 3:
                    Patient.Data.Add(new ArbitraryStringValue(name, ""));

                    break;
                case 4:
                    Patient.Data.Add(new ArbitraryDateTimeValue(name, DateTime.Now));
                    break;
            }

            editor.GetComponent<ArbitraryDataManager>().RegenerateFields(Patient);

        }
    }
}
