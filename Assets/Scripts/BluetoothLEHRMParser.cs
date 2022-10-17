using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

#if ENABLE_WINMD_SUPPORT
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
#endif

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

        const string API_URL = "http://20.211.90.206:8080";

        public TMP_Text debugText;

        [SerializeField]
        private float syncPeriod = 10f;

#if ENABLE_WINMD_SUPPORT
         private BluetoothLEAdvertisementWatcher bleWatcher;
#endif
        public event EventHandler<PatientAddedEventArgs> AdvertiserAdded;
        public event EventHandler<AdvertisementReceivedEventArgs> AdvertisementReceived;

        private readonly List<CFAAdvertisementDetails> Advertisements = new();
        private readonly List<CFAAdvertisementDetails> UploadCache = new();

        private readonly List<Patient> Patients = new();

        private readonly HttpClient apiClient = new() { BaseAddress = new Uri(API_URL), Timeout = TimeSpan.FromSeconds(10) };

        private bool dbConnectionActive = false;

        protected virtual void OnAdvertisementReceived(AdvertisementReceivedEventArgs e)
        {
            var handler = AdvertisementReceived;
            handler?.Invoke(this, e);
        }

        // Start is called before the first frame update
        void Start()
        {
            apiClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CFA_HUD", "0.1"));


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

            InvokeRepeating("SyncWithDB", syncPeriod, syncPeriod);
        }

        /**
         * Calls the API and sees if it is active. 
         **/
        async Task TestDBConnection()
        {
            Debug.Log("Testing API...");

            var response = await apiClient.GetAsync($"status");

            Debug.Log($"API reports {response.StatusCode}");

            dbConnectionActive = response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        async Task<bool> SyncWithDB()
        {

            if (dbConnectionActive && UploadCache.Count > 0)
            {
                var cacheShallowCopy = new List<CFAAdvertisementDetails>(UploadCache);
                UploadCache.Clear();

                if (await PostReadings(cacheShallowCopy))
                {
                    return true;

                }
                else
                {
                    Debug.Log("Couldn't post upload cache.");
                    UploadCache.AddRange(cacheShallowCopy);
                    return false;
                }
            }
            else
            {
                await TestDBConnection();
                return false;
            }

        }

        async Task<bool> PostReadings(List<CFAAdvertisementDetails> advertisements)
        {
            var jsonObject = $"{{ \"readings\": [{string.Join(",", advertisements.Select(x => x.ToJSONFormat()))}] }}";

            var stringContent = new StringContent(jsonObject, System.Text.Encoding.UTF8, "application/json");
            var response = await apiClient.PostAsync("readings", stringContent);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
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
                 patient = AddAdvertiserAsPatient(advertiser, null);
            }
            
            CFAAdvertisementDetails details = new CFAAdvertisementDetails(args, patient);
            Advertisements.Add(details);
            UploadCache.Add(details);

            OnAdvertisementReceived(new AdvertisementReceivedEventArgs(details));
        }

#endif
        public List<Patient> GetPatients()
        {
            return Patients;
        }
    }
}