using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;

namespace CFA_HUD
{
    public class PatientAddedEventArgs : EventArgs
    {
        public Patient Patient { get; }

        public PatientAddedEventArgs(Patient patient)
        {
            Patient = patient;
        }
    }
    public class AdvertisementReceivedEventArgs : EventArgs
    {
        public CFAAdvertisementDetails Advertisement { get; }

        public AdvertisementReceivedEventArgs(CFAAdvertisementDetails advertisement)
        {
            Advertisement = advertisement;
        }
    }

    public class BluetoothLEHRMParser : MonoBehaviour
    {

        const string API_URL = "http://localhost:8080";

        public TMP_Text debugText;

#if ENABLE_WINMD_SUPPORT
         private BluetoothLEAdvertisementWatcher bleWatcher;
#endif
        public event EventHandler<PatientAddedEventArgs> AdvertiserAdded;
        public event EventHandler<AdvertisementReceivedEventArgs> AdvertisementReceived;

        private readonly List<CFAAdvertisementDetails> Advertisements = new();
        private readonly List<Patient> Patients = new();

        private readonly HttpClient apiClient = new();

        protected virtual void OnAdvertisementReceived(AdvertisementReceivedEventArgs e)
        {
            var handler = AdvertisementReceived;
            handler?.Invoke(this, e);
        }

        // Start is called before the first frame update
        void Start()
        {

#if ENABLE_WINMD_SUPPORT
            bleWatcher = new BluetoothLEAdvertisementWatcher();

            var manufacturerData = new BluetoothLEManufacturerData();

            manufacturerData.CompanyId = 0x0590;

            var writer = new DataWriter();
            writer.WriteString("CFA");

            manufacturerData.Data = writer.DetachBuffer();

            bleWatcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);

            StartBleDeviceWatcher();

#endif
            TestDBConnection();
        }

        /**
         * Calls the API and sees if it is active. 
         **/
        async Task<bool> TestDBConnection()
        {
            Debug.Log("Testing API...");

            var response = await apiClient.GetAsync($"{API_URL}/status");

            Debug.Log($"API reports {response.StatusCode}");

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        //        async Task<bool> PostReadings(List<CFAAdvertisementDetails> advertisements) 
        //        {
        //#if ENABLE_WINMD_SUPPORT

        //#endif
        //        }

        // Update is called once per frame
        void Update()
        {

#if ENABLE_WINMD_SUPPORT
        if(Advertisements.Count > 0) {

        } 
#endif

        }

        /***
         * Generates and tracks a patient from a given bluetooth advertiser.
         */
        private Patient AddAdvertiserAsPatient(BLEAdvertiser advertiser, string alias)
        {
            if (alias == null)
            {
                alias = $"Patient {Patients.Count + 1}";
            }

            Patient newPatient = new(alias, advertiser);
            Debug.Log($"Creating new patient {alias} with bid {advertiser.Address}");

            Patients.Add(newPatient);
            AdvertiserAdded.Invoke(this, new PatientAddedEventArgs(newPatient));

            return newPatient;
        }

        /***
         * Finds a patient from the tracked patients list with the given bluetooth address.
         */
        private Patient FindPatient(ulong address)
        {
            for (int i = 0; i < Patients.Count; i++)
            {
                if (Patients[i].Advertiser.Address == address)
                {
                    return Patients[i];
                }
            }
            return null;
        }

#if ENABLE_WINMD_SUPPORT
      
        private void StartBleDeviceWatcher()
        {
        
            bleWatcher.Received += OnAdvertisementRecieved;

            bleWatcher.Start();
            debugText.text = "Starting Heart Rate Listener";
        }

        private void OnAdvertisementRecieved(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Patient patient = FindPatient(args.BluetoothAddress);

            if (patient == null)
            {
                var advertiser = new BLEAdvertiser(args.BluetoothAddress, args.Advertisement.LocalName);
                 patient = AddAdvertiserAsPatient(advertiser);
            }
            
            CFAAdvertisementDetails details = new CFAAdvertisementDetails(args, patient);
            Advertisements.Add(details);

            OnAdvertisementReceived(new AdvertisementReceivedEventArgs(details));
        }

#endif
        public List<Patient> GetPatients()
        {
            return Patients;
        }
    }
}