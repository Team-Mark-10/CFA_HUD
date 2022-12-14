using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CFA_HUD
{
    public class WindowGraph : MonoBehaviour
    {
        public static Dictionary<string, Color32> activeLineColours = new();
        public List<string> FilterIds { get; } = new();
        public string ServiceId { get => serviceId; set => serviceId = value; }

        public GameObject valueGroup;
        public GameObject axisLabelGroup;

        [SerializeField]
        private string serviceId;

        private const int circleSize = 11;
        private const int DotCount = 15;
        private const int AmountOfAxisLabels = 7;
        private readonly float xSize = 50f;

        private readonly Color32[] Colours = new[] { new Color32(255, 0, 0, 100), new Color32(0, 255, 0, 100), new Color32(0, 0, 255, 100), new Color32(0, 255, 255, 100) };

        [SerializeField]
        private Sprite circleSprite;
        [SerializeField]
        private Sprite deadSprite;

        [SerializeField]
        private Sprite upSprite;

        
        public GameObject parserGO;

        
        public GameObject selectorGO;

        private readonly List<GameObject> chartObjectList = new();

        private readonly Dictionary<string, Queue<CheckedContinuousData>> Lines = new();
        private readonly Dictionary<string, ContinuousData> Latest = new();

        private readonly List<Text> valueTexts = new();
        private readonly List<Text> axisLabels = new();

        private Font valueTextFont;

        private RectTransform graphContainer;
        private GraphData graphData;

        private void Awake()
        {
            valueTextFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            graphContainer =
                transform.Find("graphContainer").GetComponent<RectTransform>();

            // Sets the graph to regenerate every second.
        }

        private void Start()
        {
            var parser = parserGO.GetComponent<BluetoothLEHRMParser>();
            parser.AdvertisementReceived += OnAdvertisementReceived;

            var selector = selectorGO.GetComponent<PatientSelectionManager>();
            selector.PatientSelectionUpdated += OnPatientSelectionUpdated;

            SetGraphData(SelectGraphType(serviceId));

            InvokeRepeating("GenerateChart", 1.0f, 1.0f);

        }

        private void SetGraphData(GraphData graphData)
        {
            foreach(Transform child in axisLabels.Select(x => x.transform))
            {
                Destroy(child);
            }
            this.graphData = graphData;

            transform.Find("MenuText").GetComponent<Text>().text = graphData.Title;

            int i;
            float increment = graphData.Ymax / 7;  //6 being the amount of y axis labels to be populated.
            for (i = 0; i < AmountOfAxisLabels; i++)
            {
                //In the event that number is less then 7, this processes it as a float later on.
                axisLabels.Add(GenerateYAxisLabel(graphData.AxisLabel, i, increment));
            }

        }

        private void OnAdvertisementReceived(object sender, AdvertisementReceivedEventArgs e)
        {
            var data = e.Advertisement.GetContinuousDataFromService(serviceId);
            if (data != null)
            {
                AddEntry(e.Advertisement.Patient.Advertiser.Address.ToString(), data);
            }


        }

        private GraphData SelectGraphType(string ID) {
            //Takes a ID and applies a swtich statement based of prehardcoded ids
            //This returns a MAX and MIN Y , a scale name and a graph name
            switch (ID)
            {
                case "0D-18":
                    return new GraphData(ID, "Heart Rate", "BPM", 280, 0);
                case "13-27":
                    return new GraphData(ID, "Accelerometer", "M/s", 3, 0);
                case "3":
                    return new GraphData(ID, "Sleep", "Hours", 240, 0);
                case "4":
                    return new GraphData(ID, "Temperture", "*C", 240, 0);
                default:
                    Debug.Log("Unknown Blueooth ID.");
                    return new GraphData(ID, "Unknown", "", 300, 0);
            }
        }

        private void OnPatientSelectionUpdated(object sender, PatientSelectionUpdatedEventArgs e)
        {
            FilterIds.Clear();

            foreach (var activationState in e.PatientActivation)
            {
                if (!activationState.Value)
                {
                    FilterIds.Add(activationState.Key);
                }
            }
        }

        /// <summary>
        /// Renders a list of BPM data pertaining to a single patient.
        /// </summary>
        /// <param name="patientBPMData">List of BPM data</param>
        /// <param name="colour">The colour of the line to be drawn</param>
        private void RenderLine(int lineIndex, List<CheckedContinuousData> checkedData, Color32 colour, GraphData graph)
        {
           bool maxHeight = false;
            GameObject lastCircleGameObject = null;
            for (int i = 0; i < checkedData.Count; i++)
            {
                float confidence = checkedData[i].Data.Confidence;

                float xPosition = (checkedData.Count - i - 1) * xSize;
                // float yPosition = (checkedData[i].Data.Value*100)/(graph.Ymax)/100*240+47; //factor of y max
                float yPosition = (checkedData[i].Data.Value / graph.Ymax * graphContainer.sizeDelta.y);

               
                var upperLimit = graphContainer.sizeDelta.y + graphContainer.anchoredPosition.y;

                if (yPosition >= upperLimit)
                {
                    yPosition = upperLimit;
                    maxHeight = true;

                }

                GameObject circleGameObject =
                    CreateCircle(new Vector2(xPosition, yPosition), colour, checkedData[i].IsAssumed, maxHeight);

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

            float average = checkedData.ConvertAll((entry) => entry.Data.Value).Aggregate((a, b) => a + b) / checkedData.Count;

            SetValueText(lineIndex, colour, average);
        }

        /// <summary>
        /// Creates a circle represent a BPM reading on a chart line.
        /// </summary>
        /// <param name="anchoredPosition"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        private GameObject CreateCircle(Vector2 anchoredPosition, Color32 colour, bool isAssumed, bool height)
        {
            GameObject gameObject = new("circle", typeof(Image));

            gameObject.transform.SetParent(graphContainer, false);
         
            if (isAssumed)
            {
                gameObject.GetComponent<Image>().sprite = deadSprite;

            }
            else
            {
                gameObject.GetComponent<Image>().sprite = circleSprite;

            }

            if (height)

            {

                gameObject.GetComponent<Image>().sprite = upSprite;
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
        private void SetValueText(int lineIndex, Color32 colour, float value)
        {
            var valueText = valueTexts[lineIndex];

            value = (int)Math.Round(value, 0);
            valueText.text = value.ToString();
        }

        private Text GenerateYAxisLabel(string name, int index, float increment) {
            GameObject YAxisLabel = new("YAxisLabel", typeof(Text));
            YAxisLabel.transform.SetParent(axisLabelGroup.transform, false);
            RectTransform rectTransform = YAxisLabel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new(20, graphContainer.sizeDelta.y / AmountOfAxisLabels * index);
            rectTransform.sizeDelta = new(80, graphContainer.sizeDelta.y / AmountOfAxisLabels);
            rectTransform.anchorMin = new(0, 0);
            rectTransform.anchorMax = new(0, 0);

            Text text = YAxisLabel.GetComponent<Text>();
            text.font = valueTextFont;
            text.alignment = TextAnchor.MiddleLeft;
            text.fontSize = 14;

            var value = increment * index;
            if (increment > AmountOfAxisLabels) {
                value = (int) Math.Round(value, 0);
            } else {
                value = (float) Math.Round(value, 2);
            }

            YAxisLabel.GetComponent<UnityEngine.UI.Text>().text = (name + " " + value);

            return text;

        }
        /// <summary>
        /// Generates a new chart state
        /// </summary>
        private void GenerateChart()
        {
            foreach (GameObject gameObject in chartObjectList)
            {
                Destroy(gameObject);
            }
            chartObjectList.Clear();

            var ids = Lines.Keys.Union(Latest.Keys);

            var count = 0;
            foreach (var key in Latest.Keys)
            {
                if (!Lines.Keys.Contains(key))
                {
                    PrepareNewLine(key, Lines.Count + count); count += 1;
                }

            }

            foreach (var lineId in ids)
            {
                if (Latest.ContainsKey(lineId))
                {
                    AppendLineEntry(lineId, new CheckedContinuousData(Latest[lineId], false));
                }
                else
                {
                    AppendAssumedLineEntry(lineId);
                }
            }

            Latest.Clear();

            //Run only for revelant ID's
            int index = 0;
            foreach (string key in Lines.Keys)
            {
                if (!FilterIds.Contains(key))
                {
                    RenderLine(index, Lines[key].ToList(), activeLineColours[key], graphData);
                }
                index++;

            }


        }

        private void PrepareNewLine(string newKey, int newLineIndex)
        {
            if (!activeLineColours.ContainsKey(newKey))
            {
                activeLineColours.Add(newKey, GetColour(activeLineColours.Count));
            }

            GameObject ValueText = new("ValueText", typeof(Text));
            ValueText.transform.SetParent(valueGroup.transform, false);
            RectTransform rectTransform = ValueText.GetComponent<RectTransform>();

            rectTransform.sizeDelta = new(80, 40);

            Text text = ValueText.GetComponent<Text>();
            text.font = valueTextFont;
            text.color = activeLineColours[newKey];
            text.fontSize = 24;

            valueTexts.Add(text);
        }

        private void AppendAssumedLineEntry(string id)
        {
            //Attemtps to add a value to the hashmap using the ID., Skips this step if it already exists.
            if (Lines.ContainsKey(id))
            {
                Lines.TryGetValue(id, out Queue<CheckedContinuousData> entries);

                entries.Enqueue(entries.Last());
                entries.Dequeue();
            }
            else
            {
                // Generates a list of (0,0) vectors to pad the new list.
                var queue = new Queue<CheckedContinuousData>(Enumerable.Range(0, DotCount - 1).Select(x => new CheckedContinuousData(new ContinuousData(serviceId, 0, 0), true)));

                Lines.Add(id, queue);
            }
        }

        /// <summary>
        /// Adds a value to the cache for the next render. Replaces value if it is already there to show the latest reading.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entry"></param>
        public void AddEntry(string id, ContinuousData entry)
        {
            if (entry.ServiceId == serviceId)
            {
                if (Latest.ContainsKey(id))
                {
                    Latest[id] = entry;

                }
                else
                {
                    Latest.Add(id, entry);
                }
            }

        }

        /// <summary>
        /// Adds entry to the line to render.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entry"></param>
        private void AppendLineEntry(string id, CheckedContinuousData entry)
        {
            //Attemtps to add a value to the hashmap using the ID., Skips this step if it already exists.
            if (Lines.ContainsKey(id))
            {
                Lines.TryGetValue(id, out Queue<CheckedContinuousData> entries);

                entries.Enqueue(entry);
                entries.Dequeue();
            }
            else
            {
                // Generates a list of (0,0) vectors to pad the new queue.
                var queue = new Queue<CheckedContinuousData>(Enumerable.Range(0, DotCount - 1).Select(x => new CheckedContinuousData(new ContinuousData(serviceId, 0, 0), true)));

                Lines.Add(id, queue);
            }

        }

        private Color32 GetColour(int index)
        {
            return Colours[index % Colours.Length];
        }
    }
    /// <summary>
    /// A wrapper for <see cref="ContinuousData"/> regarding whether this graphed value was an assumption or not.
    /// </summary>
    public class CheckedContinuousData
    {
        public ContinuousData Data { get; }
        public bool IsAssumed { get; }

        public CheckedContinuousData(ContinuousData data, bool isAssumed)
        {
            Data = data;
            IsAssumed = isAssumed;
        }
    }
    /// <summary>
    /// A data type to configure the details of the graph.
    /// </summary>
    public class GraphData
    {

        public string BluetoothID { get; set; }
        public string Title { get; set; }

        public string AxisLabel { get; set; }

        public int Ymin { get; set; }

        public float Ymax { get; set; }
        public GraphData(string bluetoothID, string title, string axisLabel, float ymax, int ymin)
        {
            BluetoothID = bluetoothID;
            Title = title;
            AxisLabel = axisLabel;
            Ymax = ymax;
            Ymin = ymin;
        }
    
    }
}
