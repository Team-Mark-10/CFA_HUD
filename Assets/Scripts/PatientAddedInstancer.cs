using CFA_HUD;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientAddedInstancer : MonoBehaviour
{
    public GridObjectCollection parentObject;
    public GameObject instancePrefab;

    private List<Interactable> interactables;
    private readonly List<Patient> creationQueue = new();

    private BluetoothLEHRMParser parser;

    public List<Interactable> Interactables { get => interactables;  }

    // Start is called before the first frame update
    void Start()
    {
        interactables = new();

        parser = GetComponentInParent<BluetoothLEHRMParser>();
        parser.AdvertiserAdded += OnAdvertiserAdded;

        foreach (var patient in parser.GetPatients())
        {
            InstantiateGameObject(patient);
        }

        Debug.Log("Manager Start Finished");
    }
    void OnAdvertiserAdded(object sender, PatientBroadcastEventArgs args)
    {
        Debug.Log("OnAdvertiserAdded");
        creationQueue.Add(args.Patient);
    }

    private void InstantiateGameObject(Patient patient)
    {
        StartCoroutine(InstantiateGameObjectCoroutine(patient));

    }

    private IEnumerator InstantiateGameObjectCoroutine(Patient patient)
    {
        var newInstance = Instantiate(instancePrefab, parentObject.transform);

        AddNewGameObject(newInstance, patient);

        interactables.Add(newInstance.GetComponent<Interactable>());

        var cbox = parentObject.GetComponentInParent<ScrollingObjectCollection>().GetComponentInChildren<ClippingBox>();
        cbox.enabled = true;


        yield return new WaitForEndOfFrame();
        Debug.Log("Bang");
        parentObject.UpdateCollection();
        yield return new WaitForEndOfFrame();
        Debug.Log("Boom");

        //  parentObject.GetComponentInParent<ScrollingObjectCollection>().UpdateContent();
        cbox.enabled = false;

        Debug.Log("Object instantiated");
    }

    protected virtual void AddNewGameObject(GameObject newInstance, Patient patient)
    {

    }

   
    // Update is called once per frame
    public virtual void Update()
    {
        if (creationQueue.Count > 0)
        {
            foreach (var patient in creationQueue)
            {
                InstantiateGameObject(patient);
            }

            creationQueue.Clear();
        }
    }
}
