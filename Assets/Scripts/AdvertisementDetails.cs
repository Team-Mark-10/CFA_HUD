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

    public class CFAAdvertisementDetails : AdvertisementDetails, IJSONSerializable
    {
        public List<ContinuousData> ContinuousData { get; private set; }
        public Patient Patient { get; private set; }

        public DateTime TimeOfReading { get; }

#if ENABLE_WINMD_SUPPORT

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
        public ContinuousData GetContinuousDataFromService(string serviceId)
        {
            return ContinuousData.Find((data) => data.ServiceId == serviceId);
        }

        public string ToJSONFormat()
        {
            return $"{{ \"patient\": {Patient.ToJSONFormat()}, \"data\": [{String.Join(",", ContinuousData.Select(x => x.ToJSONFormat()))}] , \"reading_at\": \"{XmlConvert.ToString(TimeOfReading, XmlDateTimeSerializationMode.Utc)}\" }}";
        }
    }

    public class ContinuousData : IJSONSerializable
    {

        public float Value { get; private set; }
        public int Confidence { get; private set; }
        public string ServiceId { get; private set; }
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

    public class Patient : IJSONSerializable
    {
        internal IEnumerable<object> ContinuousData;

        public string Alias { get; private set; }
        public BLEAdvertiser Advertiser { get; private set; }
        public List<IArbitraryData> Data { get; private set; }

        public Patient(string alias, BLEAdvertiser advertiser, List<IArbitraryData> data = null)
        {
            Alias = alias;
            Advertiser = advertiser;
            Data = data;
        }

        public string ToJSONFormat()
        {
            return $"{{\"alias\": \"{Alias}\", \"bluetooth_id\": \"{Advertiser.Address}\", \"data\" : [{(Data != null ? string.Join(",", Data.Select(x => x.ToJSONFormat())) : "{}")}] }}";
        }

    }

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
