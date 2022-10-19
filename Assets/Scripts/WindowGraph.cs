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
        public List<string> FilterIds { get; } = new();
        public string ServiceId { get => serviceId; private set => serviceId = value; }
     

        [SerializeField]
        private string serviceId;

        private const int circleSize = 11;
        private const int DotCount = 15;
        private readonly float yMinimum = 50f;
        private readonly float xSize = 50f;

        private readonly Color32[] Colours = new[] { new Color32(255, 0, 0, 100), new Color32(0, 255, 0, 100), new Color32(0, 0, 255, 100), new Color32(0, 255, 255, 100) };

        [SerializeField]
        private Sprite circleSprite;
        [SerializeField]
        private Sprite deadSprite;

        [SerializeField]
        private Sprite upSprite;


        [SerializeField]
        private GameObject parserGO;

        [SerializeField]
        private GameObject selectorGO;

        private readonly List<GameObject> chartObjectList = new();

        private readonly Dictionary<string, Queue<CheckedContinuousData>> Lines = new();
        private readonly Dictionary<string, ContinuousData> Latest = new();

        private Font valueTextFont;

        private RectTransform graphContainer;


        private void Awake()
        {
            valueTextFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            graphContainer =
                transform.Find("graphContainer").GetComponent<RectTransform>();

            // Sets the graph to regenerate every second.
            InvokeRepeating("GenerateChart", 1.0f, 1.0f);
        }

        private void Start()
        {
            var parser = parserGO.GetComponent<BluetoothLEHRMParser>();

            parser.AdvertisementReceived += OnAdvertisementReceived;

            var selector = selectorGO.GetComponent<PatientSelectionManager>();
            selector.PatientSelectionUpdated += OnPatientSelectionUpdated;

          


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
                Debug.Log($"{activationState.Key} -- {activationState.Value}");
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
                

                float xPosition = (checkedData.Count - i) * xSize;
                float yPosition = checkedData[i].Data.Value + yMinimum;

                if (yPosition >= graph.Ymax)
                {
                    yPosition = graph.Ymax;
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

            GameObject RollingValueText = CreateValueText(new Vector2(xPositionText, yPositionText), colour, average);
            chartObjectList.Add(RollingValueText);
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
        private GameObject CreateValueText(Vector2 anchoredPosition, Color32 colour, float value)
        {

            GameObject valueTextGameObject = new("ValueText", typeof(Text));
            valueTextGameObject.transform.SetParent(graphContainer, false);
            RectTransform rectTransform = valueTextGameObject.GetComponent<RectTransform>();


            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new(80, 40);
            rectTransform.anchorMin = new(0, 0);
            rectTransform.anchorMax = new(0, 0);

            Text text = valueTextGameObject.GetComponent<Text>();

            text.font = valueTextFont;
            text.fontSize = 24;
            text.color = colour;
            value = (int)Math.Round(value, 0);
            valueTextGameObject.GetComponent<UnityEngine.UI.Text>().text = value.ToString();

            return valueTextGameObject;
        }
        private GameObject CreateMenuText(string name)
        {
            GameObject menuTextObject = new("MenuText", typeof(Text));
            menuTextObject.transform.SetParent(graphContainer, false);
            RectTransform rectTransform = menuTextObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new(375, 150);
            rectTransform.sizeDelta = new(250, 400);
            rectTransform.anchorMin = new(0, 0);
            rectTransform.anchorMax = new(0, 0);

            Text text = menuTextObject.GetComponent<Text>();
            text.font = valueTextFont;
            text.fontSize = 35;
            menuTextObject.GetComponent<UnityEngine.UI.Text>().text = name;



            return menuTextObject;
        }

        private GameObject CreateYAxisLabel(string name, int pos, float number, bool decimals) {

            GameObject YAxisLabel = new("YAxisLabel", typeof(Text));
            YAxisLabel.transform.SetParent(graphContainer, false);
            RectTransform rectTransform = YAxisLabel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new(20, pos + 40);
            rectTransform.sizeDelta = new(80, 40);
            rectTransform.anchorMin = new(0, 0);
            rectTransform.anchorMax = new(0, 0);

            Text text = YAxisLabel.GetComponent<Text>();
            text.font = valueTextFont;
            text.fontSize = 14;

            if (decimals) {
                number = (int)Math.Round(number, 0);
            } else {
                number = (float)Math.Round(number, 2);
            }

            YAxisLabel.GetComponent<UnityEngine.UI.Text>().text = (name + " " + number);

            return YAxisLabel;


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

            var ids = Lines.Keys.Union(Latest.Keys);

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




            //Menu text to be taken from graphdata
            GraphData GraphData = SelectGraphType(serviceId);
            Debug.Log(serviceId);

            GameObject MenuText = CreateMenuText(GraphData.Title);
            chartObjectList.Add(MenuText);

            //Generates Yaxis labels based off y 
            int i;
            float number = GraphData.Ymax / 6;  //6 being the amount of y axis labels to be populated.
            for (i = 0; i < 7; i++) {

                //In the event that number is less then 7, this processes it as a float later on.
                if (number > 7) {
                    GameObject YAxisLabel = CreateYAxisLabel(GraphData.AxisLabel, i * 40, number * i, true);
                    chartObjectList.Add(YAxisLabel);
                } else
                {
                    GameObject YAxisLabel = CreateYAxisLabel(GraphData.AxisLabel, i * 40, number * i, false);
                    chartObjectList.Add(YAxisLabel);
                }

            }


            //Run only for revelant ID's
            int index = 0;
            foreach (string key in Lines.Keys)
            {
                if (!FilterIds.Contains(key))
                {
                    RenderLine(index, Lines[key].ToList(), GetColour(index), GraphData);
                }
                index++;

            }


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
