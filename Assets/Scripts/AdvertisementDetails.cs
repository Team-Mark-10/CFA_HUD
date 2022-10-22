using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
    /// <summary>
    /// A class representing a BLE Advertisement. This is from the Windows UWP BLE events. />
    /// </summary>
    public class AdvertisementDetails
    {
#if ENABLE_WINMD_SUPPORT

        public ulong Address { get; private set; }
        public List<Guid> Services { get; private set; }
        public String LocalName { get; private set; }
        public DateTimeOffset TimeStamp { get; private set; }
        public BluetoothLEAdvertisementType Type { get; private set; }
        public short RSSI { get; private set; }
        public string ManufacturerData { get; private set; }

        public AdvertisementDetails(BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Address = args.BluetoothAddress;
            Services = args.Advertisement.ServiceUuids.ToList();
            LocalName = args.Advertisement.LocalName;
            TimeStamp = args.Timestamp;
            Type = args.AdvertisementType;
            RSSI = args.RawSignalStrengthInDBm;

            BluetoothLEManufacturerData manufacturerData = args.Advertisement.ManufacturerData[0];

            byte[] data = new byte[manufacturerData.Data.Length];
            using (var reader = DataReader.FromBuffer(manufacturerData.Data))
            {
                reader.ReadBytes(data);
            }

            ManufacturerData = string.Format("0x{0}: {1}",
                    args.Advertisement.ManufacturerData[0].CompanyId.ToString("X"),
                    BitConverter.ToString(data));

            List<string> dsStrings = new List<string>();

            foreach (BluetoothLEAdvertisementDataSection ds in args.Advertisement.DataSections)
            {
                var output = CryptographicBuffer.EncodeToHexString(ds.Data);

                dsStrings.Add(output);
            }
        }
#endif
    }
    /// <summary>
    /// A class representing specfic Bluetooth Advertisement that is made to work with the CFA_HUD system.
    /// </summary>

    public class CFAAdvertisementDetails : AdvertisementDetails, IJSONSerializable
    {
        /// <summary>
        /// The data values attached to the advertisement.
        /// </summary>
        public List<ContinuousData> ContinuousData { get; private set; }
        /// <summary>
        /// The <see cref="CFA_HUD.Patient"/> that this advertisement was sent from.
        /// </summary>
        public Patient Patient { get; private set; }

        /// <summary>
        /// When this Advertisement was read by the CFA System
        /// </summary>
        public DateTime TimeOfReading { get; }
        /// <summary>
        /// The data values attached to the advertisement.
        /// 
        /// 
        /// </summary>
#if ENABLE_WINMD_SUPPORT

        /// <summary>
        /// Process a BluetoothLEAdvertisementReceivedEventArgs from Windows UWP Blueooth in to a CFAAdvertisementDetails
        /// </summary>
        public CFAAdvertisementDetails(BluetoothLEAdvertisementReceivedEventArgs args, Patient patient) : base(args)
        {
            ContinuousData = new();

            // 0x16 is the flag for service data.
            foreach (var dataSection in args.Advertisement.GetSectionsByType(0x16)) {
                IBuffer buffer = dataSection.Data;

                byte[] data = new byte[buffer.Length];
                DataReader.FromBuffer(buffer).ReadBytes(data);

                // Byte Breakdown :
                //  0 & 1: Service ID,
                //  2,3,4,5: Float32 value as bytes
                //  6: Confidence byte (confidence should be between 0-100, so only one byte is required.)
                
                string serviceId = BitConverter.ToString(data[0..2]);

                int confidence = data[6];

                byte[] value_bytes = data[2..6];             

                float value = System.BitConverter.ToSingle(value_bytes, 0);

                ContinuousData.Add(new ContinuousData(serviceId, value, confidence));
            }

            Patient = patient;

            TimeOfReading = DateTime.Now;

        }

#endif

        /// <summary>
        /// Gets the <see cref="CFA_HUD.ContinuousData"/> for a specific serviceId.
        /// </summary>
        /// <returns></returns>
        public ContinuousData GetContinuousDataFromService(string serviceId)
        {
            return ContinuousData.Find((data) => data.ServiceId == serviceId);
        }
        /// <summary>
        /// Returns a JSON String for sending over the web.
        /// </summary>
        /// <returns></returns>
        public string ToJSONFormat()
        {
            return $"{{ \"patient\": {Patient.ToJSONFormat()}, \"data\": [{String.Join(",", ContinuousData.Select(x => x.ToJSONFormat()))}] , \"reading_at\": \"{XmlConvert.ToString(TimeOfReading, XmlDateTimeSerializationMode.Utc)}\" }}";
        }
    }
    /// <summary>
    /// Represent a float reading and a confidence value from a specific service id advertised.
    /// </summary>
    public class ContinuousData : IJSONSerializable
    {
        /// <summary>
        /// The actual value of the data reading was on the bluetooth device
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// How confident the value of the reading is. This is sent from the bluetooth device
        /// </summary>
        public int Confidence { get; private set; }
        /// <summary>
        /// What bluetooth service id this was advertised under
        /// </summary>
        public string ServiceId { get; private set; }

        /// <summary>
        /// A human-readable alias for the service id's data.
        /// </summary>
        public string Alias { get; private set; }


        public ContinuousData(string serviceId, float value, int confidence = 1, string alias = null)
        {
            ServiceId = serviceId;
            Value = value;
            Confidence = confidence;
            Alias = alias;
        }

        public string ToJSONFormat()
        {
            return $"{{\"service_id\": \"{ServiceId}\", {(Alias != null ? $"\"alias\": \"{Alias}\"," : "")} \"value\" : {Value}, \"confidence\": {Confidence} }}";
        }
    }

    /// <summary>
    /// A wrapper for a bluetooth advertiser with useful data.
    /// </summary>
    public class Patient : IJSONSerializable
    {
        /// <summary>
        /// Human readable name for the patient. This is part of the ArbitraryData generated for every Patient.
        /// </summary>
        public string Alias { get => Data[0].ToDisplayFormat(); }

        /// <summary>
        /// The Bluetooth advertiser that this patient wraps.
        /// </summary>
        public BLEAdvertiser Advertiser { get; private set; }

        /// <summary>
        /// The arbitrary data associated with the patient.
        /// </summary>
        public List<IArbitraryData> Data { get; private set; }

        public Patient(string alias, BLEAdvertiser advertiser, List<IArbitraryData> data = null)
        {
            var defaultData = new List<IArbitraryData>()
            {
                new ArbitraryStringValue("Name", alias),
                new ArbitraryDateTimeValue("Date of Birth", DateTime.Now),

            };

            if (data != null)
            {
                defaultData.AddRange(data);
            }
            Advertiser = advertiser;
            Data = defaultData;
        }

        public string ToJSONFormat()
        {

            var includedData = new List<IArbitraryData>();
            
            if(Data != null)
            {
                includedData = Data.FindAll(x => x.IsUserSet());
            }


            return $"{{\"alias\": \"{Alias}\", \"bluetooth_id\": \"{Advertiser.Address}\", \"data\" : [{(includedData != null ? string.Join(",", Data.Select(x => x.ToJSONFormat())) : "{}")}] }}";
        }

    }

    public interface IPatientUser
    {
        public void SetPatient(Patient p);
    }

    /// <summary>
    /// A bluetooth advertiser representing a specific device.
    /// </summary>
    public class BLEAdvertiser
    {
        public ulong Address { get; private set; }
        public string LocalName { get; private set; }

        public BLEAdvertiser(ulong address, String localName)
        {
            Address = address;
            LocalName = localName;
        }
    }
}
