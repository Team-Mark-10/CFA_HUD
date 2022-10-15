using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class CFAAdvertisementDetails : AdvertisementDetails
    {
        public List<ContinuousData> ContinuousData { get; private set; }
        public Patient Patient { get; private set; }

#if ENABLE_WINMD_SUPPORT

        public CFAAdvertisementDetails(BluetoothLEAdvertisementReceivedEventArgs args, Patient patient) : base(args)
        {
            ContinuousData = new();
            // 0x16 is the flag for service data.
            for (var dataSection in args.Advertisement.GetSectionsByType(0x16)) {
                IBuffer buffer = dataSection.Data;

                byte[] data = new byte[buffer.Length];
                DataReader.FromBuffer(buffer).ReadBytes(data);

                // Bytes 0 & 1 are the service ID, byte 2 is the value, byte 3 is the confidence. This is the schema that has been decided on by us.
                ContinuousData.Add(new ContinuousData(BitConverter.ToString([data[0], data[1]]), data[2], data[3]);
            }

            Patient = patient;
        }

#endif
        public ContinuousData GetContinuousDataFromService(string serviceId)
        {
            return ContinuousData.Find((data) => data.ServiceId == serviceId);
        }
    }

    public class ContinuousData
    {

        public int Value { get; private set; }
        public int Confidence { get; private set; }
        public string ServiceId { get; private set; }

        public ContinuousData(string serviceId, int value, int confidence = 1)
        {
            ServiceId = serviceId;
            Value = value;
            Confidence = confidence;
        }
    }

    public class Patient
    {
        public string Alias { get; private set; }
        public BLEAdvertiser Advertiser { get; private set; }

        public Patient(string alias, BLEAdvertiser advertiser)
        {
            Alias = alias;
            Advertiser = advertiser;
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
