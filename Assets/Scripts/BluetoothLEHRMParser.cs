using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using TMPro;

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

#if ENABLE_WINMD_SUPPORT
public class AdvertisementDetails
{
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

        Debug.Log(String.Join(", ", dsStrings));
    }
}

public class CFAAdvertisementDetails : AdvertisementDetails
{

    public int HeartRate { get; private set; }
    public int Confidence { get; private set; }

    public CFAAdvertisementDetails(BluetoothLEAdvertisementReceivedEventArgs args) : base(args)
    {
        IBuffer buffer = args.Advertisement.DataSections[2].Data;
        byte[] data = new byte[buffer.Length];
        DataReader.FromBuffer(buffer).ReadBytes(data);

        HeartRate = data[2];
        Confidence = data[3];
        Debug.Log("Heart Rate: " + HeartRate + ", Confidence: " + Confidence);
    }
}


#endif

public class BLEAdvertiser
{
    public ulong Address { get; private set; }
    public String LocalName { get; private set; }

    public BLEAdvertiser(ulong address, String localName)
    {
        Address = address;
        LocalName = localName;
    }
}

public class AdvertiserAddedEventArgs : EventArgs
{
    public BLEAdvertiser Advertiser { get { return advertiser;  } }
    private BLEAdvertiser advertiser;

    public AdvertiserAddedEventArgs(BLEAdvertiser advertiser)
    {
        this.advertiser = advertiser;
    }
}

public class BluetoothLEHRMParser : MonoBehaviour
{
    public TMP_Text heartRateText;

#if ENABLE_WINMD_SUPPORT
     private BluetoothLEAdvertisementWatcher bleWatcher;
    
     private List<CFAAdvertisementDetails> Advertisements = new List<CFAAdvertisementDetails>();
     private List<CFAAdvertisementDetails> AdvertiserSpecificAdvertisements = new List<CFAAdvertisementDetails>();
#endif
    private List<BLEAdvertiser> Advertisers = new List<BLEAdvertiser>();

    public event EventHandler<AdvertiserAddedEventArgs> AdvertiserAdded;

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
    }

    // Update is called once per frame
    void Update()
    {

#if ENABLE_WINMD_SUPPORT
        if(Advertisements.Count > 0) {
            heartRateText.text = "Total Count: " + Advertisements.Count + ", Latest: " + Advertisements[Advertisements.Count - 1].HeartRate.ToString();
        } 
#endif

    }

    void AddAdvertiser(BLEAdvertiser advertiser)
    {
        Debug.Log("Adding advertiser at parser");
        Advertisers.Add(advertiser);
        AdvertiserAdded.Invoke(this, new AdvertiserAddedEventArgs(advertiser));
    }

#if ENABLE_WINMD_SUPPORT
    private BLEAdvertiser FindAdvertiser(ulong address)
    {
        for (int i = 0; i < this.Advertisers.Count; i++)
        {
            if (this.Advertisers[i].Address == address)
            {
                return this.Advertisers[i];
            }
        }
        return null;
    }
    private void StartBleDeviceWatcher()
    {
        
        bleWatcher.Received += OnAdvertisementRecieved;

        bleWatcher.Start();
        heartRateText.text = "Starting Heart Rate Listener";
    }

    private void OnAdvertisementRecieved(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        BLEAdvertiser advertiser = FindAdvertiser(args.BluetoothAddress);
        CFAAdvertisementDetails details = new CFAAdvertisementDetails(args);
        Advertisements.Add(details);

        if (advertiser == null)
        {
            AddAdvertiser(new BLEAdvertiser(args.BluetoothAddress, args.Advertisement.LocalName));
        }
        else
        {
            AdvertiserSpecificAdvertisements.Add(details);

            // UpdateRecentHistory();
        }
    }

   

#endif
    public List<BLEAdvertiser> GetAdvertisers()
    {
        return Advertisers;
    }
}
