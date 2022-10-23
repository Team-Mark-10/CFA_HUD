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
    public interface IPatientBroadcaster
    {
        public void AddListener(EventHandler<PatientBroadcastEventArgs> handler);
    }

    public class PatientBroadcastEventArgs : EventArgs
    {
        public PatientBroadcastEventArgs(Patient patient)
        {
            Patient = patient;
        }

        public Patient Patient { get; private set; }
    }

    public class NewServiceIDEventArgs : EventArgs
    { 
        public string Data { set; get; }
    
        public NewServiceIDEventArgs(string data)
        {
            Data = data;
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

    /// <summary>
    /// This script handles all the bluetooth interfacing with external devices.
    /// </summary>
    public class BluetoothLEHRMParser : MonoBehaviour
    {
        /// <summary>
        /// The default url to API for the HttpClient. This can be overriden through <see cref="SetAPIAddress(string)"/>
        /// </summary>
        const string DEFAULT_API_URL = "http://localhost:8080";

        [SerializeField]
        private float syncPeriod = 10f;

#if ENABLE_WINMD_SUPPORT
        private BluetoothLEAdvertisementWatcher bleWatcher;
#endif
        public event EventHandler<PatientBroadcastEventArgs> AdvertiserAdded;
        public event EventHandler<AdvertisementReceivedEventArgs> AdvertisementReceived;
        public event EventHandler<NewServiceIDEventArgs> NewServiceIDReceived;

        public event EventHandler<bool> DBConnectionSuccess;

        /// <summary>
        /// All advertisements over the lifetime of the application.
        /// </summary>
        private readonly List<CFAAdvertisementDetails> Advertisements = new();

        /// <summary>
        /// All advertisements awaiting upload to the DB through the API.
        /// </summary>
        private readonly List<CFAAdvertisementDetails> UploadCache = new();

        /// <summary>
        /// The master list of patients in the application.
        /// </summary>
        private readonly List<Patient> Patients = new();

        /// <summary>
        /// The master list of service ID's encountered in the application.
        /// </summary>
        private readonly List<string> ServiceIDList = new ();


        /// <summary>
        /// The HttpClient used to connect to the CFA API.
        /// </summary>
        private readonly HttpClient apiClient = new() { BaseAddress = new Uri(DEFAULT_API_URL), Timeout = TimeSpan.FromSeconds(10),  };

        /// <summary>
        /// Whether a successful authenticated connection to the API was established.
        /// </summary>
        private bool dbConnectionActive = false;

        // Start is called before the first frame update
        void Start()
        {
            InitialiseApiClient();

            SetupBluetoothWatcher();
        }

        private void SetupBluetoothWatcher()
        {
#if ENABLE_WINMD_SUPPORT
            bleWatcher = new BluetoothLEAdvertisementWatcher();

            var manufacturerData = new BluetoothLEManufacturerData();

            manufacturerData.CompanyId = 0x0590;

            var writer = new DataWriter();
            writer.WriteString("CFA");

            manufacturerData.Data = writer.DetachBuffer();

            bleWatcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
            bleWatcher.Received += OnAdvertisementRecieved;

            bleWatcher.Start();

#endif
        }

        private void InitialiseApiClient()
        {
            apiClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CFA_HUD", "0.1"));

            // Load username and password set in previous runs of the application.
            var username = PlayerPrefs.GetString("username");
            var pwdHash = PlayerPrefs.GetString("password");
            var apiUrl = PlayerPrefs.GetString("apiUrl");

            if (apiUrl != null)
            {
                try
                {
                    apiClient.BaseAddress = new Uri(apiUrl);
                }
                catch 
                {
                    apiClient.BaseAddress = new Uri(DEFAULT_API_URL);
                }
            }

            // Sets up the Authorization header for the api client.
            if (username != null && pwdHash != null)
            {
                var authenticationString = $"{username}:{pwdHash}";

                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authenticationString));

                apiClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Basic " + base64EncodedAuthenticationString);

            }

            TestDBConnection();

            InvokeRepeating("SyncWithDB", syncPeriod, syncPeriod);

        }

        /**
         * Calls the API and sees if it is active. 
         **/
        private async Task TestDBConnection()
        {
            Debug.Log("Testing API...");

            var response = await apiClient.GetAsync($"status");

            Debug.Log($"API reports {response.StatusCode}");

            dbConnectionActive = response.StatusCode == System.Net.HttpStatusCode.OK;

            DBConnectionSuccess.Invoke(this, dbConnectionActive);
        }

        /// <summary>
        /// Uploads the advertisements in the upload cache to the DB and removes them if successful.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SyncWithDB()
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

        /// <summary>
        /// Post a list of readings to the DB through the API.
        /// </summary>
        /// <param name="advertisements"></param>
        /// <returns></returns>
        private async Task<bool> PostReadings(List<CFAAdvertisementDetails> advertisements)
        {
            var jsonObject = $"{{ \"readings\": [{string.Join(",", advertisements.Select(x => x.ToJSONFormat()))}] }}";

            var stringContent = new StringContent(jsonObject, System.Text.Encoding.UTF8, "application/json");
            var response = await apiClient.PostAsync("readings", stringContent);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// Generates and tracks a patient from a given bluetooth advertiser.
        /// </summary>
        /// <param name="advertiser">The advertiser to be associated with the Patient</param>
        /// <param name="alias">A human readable name for the patient</param>
        /// <returns></returns>
        private Patient AddAdvertiserAsPatient(BLEAdvertiser advertiser, string alias)
        {
            if (alias == null)
            {
                alias = $"Patient {Patients.Count + 1}";
            }

            Patient newPatient = new(alias, advertiser);

            Patients.Add(newPatient);
            AdvertiserAdded.Invoke(this, new PatientBroadcastEventArgs(newPatient));

            return newPatient;
        }

        /// <summary>
        /// Finds a patient from the tracked patients list with the given bluetooth address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
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
       /// <summary>
       /// Handles an incoming bluetooth advertisement from an external bluetooth device.
       /// </summary>
        private void OnAdvertisementRecieved(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Patient existingPatient = FindPatient(args.BluetoothAddress);

            // Adds a new patient instance for a new device.
            if (existingPatient == null)
            {
                var advertiser = new BLEAdvertiser(args.BluetoothAddress, args.Advertisement.LocalName);
                existingPatient = AddAdvertiserAsPatient(advertiser, null);
            }
            

            CFAAdvertisementDetails details = new CFAAdvertisementDetails(args, existingPatient);
            
            // Checks if there are any new data types.
            foreach (var data in details.ContinuousData)
            {
                if (!ServiceIDList.Contains(data.ServiceId) && data.ServiceId != null)
                {
                     ServiceIDList.Add(data.ServiceId);
                     NewServiceIDReceived?.Invoke(this, new NewServiceIDEventArgs(data.ServiceId));
                }

            }

            Advertisements.Add(details);
            UploadCache.Add(details);

            AdvertisementReceived?.Invoke(this, new AdvertisementReceivedEventArgs(details));

            // To clear memory
            if (Advertisements.Count > ServiceIDList.Count * 1000)
            {
                Advertisements.RemoveRange(0, ServiceIDList.Count * 900);
            }
        }
#endif
        public List<Patient> GetPatients()
        {
           
            return Patients;
        }

        public List<string> GetServiceIDs()
        {
            return ServiceIDList;
        }

        /// <summary>
        /// Saves a new username and password (hashed) to local storage for use with the API. Persists through launches of the application.
        /// </summary>
        /// <param name="username">The new username.</param>
        /// <param name="password">The new password string.</param>
        public void SetNewLoginDetails(string username, string password)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("password", hash);

                var authenticationString = $"{username}:{hash}";

                // Basic HTTP Authorization is encoded in Base64
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authenticationString));
                apiClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Basic " + base64EncodedAuthenticationString);
            }
        }

        /// <summary>
        /// Attempts to set the API address.
        /// </summary>
        /// <param name="text">The new API candidate</param>
        /// <returns>If the new API was valid.</returns>
        public bool SetAPIAddress(string text)
        {
            try
            {
                new Uri(text);
            }catch
            {
                Debug.Log("Invalid URI was specified.");
                return false;
            }

            PlayerPrefs.SetString("apiUrl", text);
            return true;
        }
    }
}

