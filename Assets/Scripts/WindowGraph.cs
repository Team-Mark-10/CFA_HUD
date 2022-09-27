﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WindowGraph : MonoBehaviour
{
    private const int circleSize = 11;
    private const int DotCount = 15;
    private readonly float yMinimum = 50f;
    private readonly float xSize = 50f;

    private Font heartRateTextFont;
    private readonly Color32[] Colours = new[] { new Color32(255, 0, 0, 100), new Color32(0, 255, 0, 100), new Color32(0, 0, 255, 100), new Color32(0, 255, 255, 100) };

    [SerializeField]
    private Sprite circleSprite;
    [SerializeField]
    private Sprite deadSprite;

    [SerializeField]
    private GameObject parserGO;

    [SerializeField]
    private GameObject selectorGO;

    private RectTransform graphContainer;

    private List<GameObject> chartObjectList;

    public List<string> FilterIds { get => filterIds; }
    private readonly List<string>  filterIds = new();

    private readonly Dictionary<string, List<BPMEntry>> Lines = new();
    private readonly Dictionary<string, BPMEntry> Latest = new();


    private void Awake()
    {
        heartRateTextFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        graphContainer =
            transform.Find("graphContainer").GetComponent<RectTransform>();

        chartObjectList = new();

        // Sets the graph to regenerate every second.
        InvokeRepeating("GenerateChart", 1.0f, 1.0f);
    }

    private void Start()
    {
        var parser = parserGO.GetComponent<BluetoothLEHRMParser>();

#if ENABLE_WINMD_SUPPORT
         parser.AdvertisementReceived += OnAdvertisementReceived;
#endif


        var selector = selectorGO.GetComponent<PatientSelectionManager>();
        selector.PatientSelectionUpdated += OnPatientSelectionUpdated;
    }

#if ENABLE_WINMD_SUPPORT
    private void OnAdvertisementReceived(object sender, AdvertisementReceivedEventArgs e)
    {
        AddEntry(e.Advertiser.Address.ToString(), new BPMEntry(e.Advertisement.HeartRate, e.Advertisement.Confidence));
    }
#endif

    private void OnPatientSelectionUpdated(object sender, PatientSelectionUpdatedEventArgs e)
    {
        filterIds.Clear();

        foreach (var activationState in e.PatientActivation)
        {
            Debug.Log($"{activationState.Key} -- {activationState.Value}");
            if(!activationState.Value)
            {
                filterIds.Add(activationState.Key);
            }
        }
    }

    /// <summary>
    /// Renders a list of BPM data pertaining to a single patient.
    /// </summary>
    /// <param name="patientBPMData">List of BPM data</param>
    /// <param name="colour">The colour of the line to be drawn</param>
    private void RenderLine(int lineIndex, List<BPMEntry> patientBPMData, Color32 colour, string key)
    {
        GameObject lastCircleGameObject = null;
        for (int i = 0; i < patientBPMData.Count; i++)
        {
            float confidence = patientBPMData[i].Confidence;
            float xPosition = (patientBPMData.Count - i) * xSize;
            float yPosition = patientBPMData[i].BPM + yMinimum;

            GameObject circleGameObject =
                CreateCircle(new Vector2(xPosition, yPosition), colour,Latest.ContainsKey(key));

            chartObjectList.Add(circleGameObject);

            // If not the first circle, draw a line between the new circle and the last one.
            if (lastCircleGameObject != null)
            {
                GameObject
                    dotConnectionGameObject =
                        CreateDotConnection(lastCircleGameObject
                            .GetComponent<RectTransform>()
                            .anchoredPosition,
                        circleGameObject
                            .GetComponent<RectTransform>()
                            .anchoredPosition, confidence, colour);

                chartObjectList.Add(dotConnectionGameObject);
            }
            lastCircleGameObject = circleGameObject;
        }

        float xPositionText = 200f + lineIndex * 80;
        float yPositionText = 20f;

        float average = patientBPMData.ConvertAll((entry) => entry.BPM).Aggregate((a, b) => a + b) / patientBPMData.Count;

        GameObject RollingHeartText = CreateHeartRateText(new Vector2(xPositionText, yPositionText), colour, average);
        chartObjectList.Add(RollingHeartText);
    }

    /// <summary>
    /// Creates a circle represent a BPM reading on a chart line.
    /// </summary>
    /// <param name="anchoredPosition"></param>
    /// <param name="colour"></param>
    /// <returns></returns>
    private GameObject CreateCircle(Vector2 anchoredPosition, Color32 colour, bool alive)
    {
        GameObject gameObject = new("circle", typeof(Image));

        gameObject.transform.SetParent(graphContainer, false);
        if (alive)
        {
            gameObject.GetComponent<Image>().sprite = circleSprite;
        } else
        {
            gameObject.GetComponent<Image>().sprite = deadSprite;
        }
      

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new(circleSize, circleSize);
        rectTransform.anchorMin = new(0, 0);
        rectTransform.anchorMax = new(0, 0);

        gameObject.GetComponent<Image>().color = colour;

        return gameObject;
    }

    /// <summary>
    /// Draws a connecting line between two dots on the chart
    /// </summary>
    /// <param name="dotPositionA"></param>
    /// <param name="dotPositionB"></param>
    /// <param name="confidence"></param>
    /// <param name="colour"></param>
    /// <returns></returns>
    private GameObject
    CreateDotConnection(
        Vector2 dotPositionA,
        Vector2 dotPositionB,
        float confidence,
        Color32 colour
    )
    {
        GameObject connectingLine = new("dotConnection", typeof(Image));
        connectingLine.transform.SetParent(graphContainer, false);

        //intensity
        confidence = (confidence * 2 + 50);
        byte vOut = Convert.ToByte(confidence);

        Color32 alphaAppliedColour = new(colour.r, colour.g, colour.b, vOut);
        connectingLine.GetComponent<Image>().color = alphaAppliedColour;

        RectTransform rectTransform = connectingLine.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        rectTransform.anchorMin = new(0, 0);
        rectTransform.anchorMax = new(0, 0);

        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.sizeDelta = new(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + .5f * distance * dir;
        rectTransform.localEulerAngles =
            new(0, 0, (Mathf.Atan2(dir.y, dir.x) * 180 / Mathf.PI));

        return connectingLine;
    }

    /// <summary>
    /// Creates the BPM summary for each line on the bottom of the chart
    /// </summary>
    /// <param name="anchoredPosition"></param>
    /// <param name="colour"></param>
    /// <param name="BPM"></param>
    /// <returns></returns>
    private GameObject CreateHeartRateText(Vector2 anchoredPosition, Color32 colour, float BPM)
    {

        GameObject heartRateTextGameObject = new("HeartRateText", typeof(Text));
        heartRateTextGameObject.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = heartRateTextGameObject.GetComponent<RectTransform>();


        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new(40, 40);
        rectTransform.anchorMin = new(0, 0);
        rectTransform.anchorMax = new(0, 0);

        Text text = heartRateTextGameObject.GetComponent<Text>();

        text.font = heartRateTextFont;
        text.fontSize = 30;
        text.color = colour;

        heartRateTextGameObject.GetComponent<UnityEngine.UI.Text>().text = BPM.ToString();

        return heartRateTextGameObject;
    }

    /// <summary>
    /// 
    /// </summary>
    private void GenerateChart()
    {
        foreach (GameObject gameObject in chartObjectList)
        {
            Destroy(gameObject);
        }
        chartObjectList.Clear();

        
        foreach(var entry in Latest)
        {
            AppendLineEntry(entry.Key, entry.Value);
        }

   

        int index = 0;
        foreach (string key in Lines.Keys)
        {
            if(!filterIds.Contains(key))
            {
                RenderLine(index, Lines[key], GetColour(index),key);
            }
            index++;

        }

        Latest.Clear();

    }

    /// <summary>
    /// Adds a value to the cache for the next render. Replaces value if it is already there to show the latest reading.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entry"></param>
    public void AddEntry(string id, BPMEntry entry)
    {
        if(Latest.ContainsKey(id))
        {
            Latest[id] = entry;

        } else
        {
            Latest.Add(id, entry);
        }
    }

    /// <summary>
    /// Adds entry to the line to render.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entry"></param>
    public void AppendLineEntry(string id, BPMEntry entry)
    {
        //Attemtps to add a value to the hashmap using the ID., Skips this step if it already exists.
        if (Lines.ContainsKey(id))
        {
            List<BPMEntry> entries;
            Lines.TryGetValue(id, out entries);

            entries.Add(entry);
            entries.RemoveAt(0);
        }
        else
        {
            // Generates a list of (0,0) vectors to pad the new list.
            var list = Enumerable.Range(0, DotCount - 1).Select(x => new BPMEntry(0,0)).ToList(); 
            Lines.Add(id, list);
        }

    }

    private Color32 GetColour(int index)
    {
        return Colours[index % Colours.Length];
    }
}

public class BPMEntry
{
    public int BPM { get; set; }
    public int Confidence { get; set; }

    public BPMEntry(int bPM, int confidence)
    {
        BPM = bPM;
        Confidence = confidence;
    }
}