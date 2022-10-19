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

    public GameObject viewer;
    public GameObject editor;

    private bool isEditing = false;
    public bool IsEditing => isEditing;


    public Patient Patient { get; set; }
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
        Debug.Log("Toggling");
        if (isEditing) StopEditing();
        else StartEditing();
    }


    private void StartEditing()
    {
        Debug.Log("Start editing");
        isEditing = true;

        viewer.SetActive(false);
        editor.SetActive(true);

    }

    private bool StopEditing()
    {
        Debug.Log("Stop editin");
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
            RenderDetails();
             
            EnableViewer();
        } else
        {
            if (StopEditing()) {
                Patient = e.Patient;
                RenderDetails();
            };
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
