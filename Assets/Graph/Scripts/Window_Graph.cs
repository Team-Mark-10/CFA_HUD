using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Graph : MonoBehaviour
{
    [SerializeField]
    private Sprite circleSprite;
    private float yMinimum = 50f;
    private  float xSize = 50f;
    private RectTransform graphContainer;

    private List<GameObject> gameObjectList;

    List<int>
        valueList0 =
            new List<int>()
            { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, };

    List<int>
        confidenceList0 =
            new List<int>()
            { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, };




    Dictionary<string, List<BPMEntry>> ValueTable = new Dictionary<string, List<BPMEntry>>();


    private void Awake()
    {
        graphContainer =
            transform.Find("graphContainer").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();

        InvokeRepeating("Generate", 1.0f, 1.0f);
    }


    private void ShowGraph(List<BPMEntry> HashList, int colour)
    {
  
      
        GameObject lastCircleGameObject = null;
        for (int i = 0; i < HashList.Count; i++)
        {
            float confidence = HashList[i].Confidence;
            float xPosition = (HashList.Count - i) * xSize;
            float yPosition = HashList[i].BPM+yMinimum;

            GameObject circleGameObject =
                CreateCircle(new Vector2(xPosition, yPosition), colour);
            gameObjectList.Add(circleGameObject);
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
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastCircleGameObject = circleGameObject;
        }

        //uses colour to track overall amount of rolling bpm text to set offset
        float xPositionText = 200f+colour*80;
        float yPositionText = 20f;

        float average = 0;
        for (int i = 0; i < HashList.Count; i++)
        {
            average += HashList[i].BPM;
        }
        average = average / HashList.Count;

        GameObject RollingHeartText = CreateHeartRateText(new Vector2(xPositionText, yPositionText), colour, average);
        gameObjectList.Add(RollingHeartText);

    }


    private GameObject CreateCircle(Vector2 anchoredPosition, int colour)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);


        //apply colour variable to change
        switch (colour % 4)
        {
            case 0:
                gameObject.GetComponent<Image>().color = new Color32(255, 0, 0, 100);
                break;
            case 1:
                gameObject.GetComponent<Image>().color = new Color32(0, 255, 0, 100);
                break;
            case 2:
                gameObject.GetComponent<Image>().color = new Color32(0, 0, 255, 100);
                break;
            case 3:
                gameObject.GetComponent<Image>().color = new Color32(0, 255, 255, 100);
                break;

        }


        return gameObject;
    }

    private GameObject
    CreateDotConnection(
        Vector2 dotPositionA,
        Vector2 dotPositionB,
        float confidence,
        float colour
    )
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);



        //intensity

        confidence = (confidence * 2 + 50);
        //Debug.Log($"converting {confidence}");
        byte vOut = Convert.ToByte(confidence);



        //apply colour variable to change
        switch (colour % 4)
        {
            case 0:
                gameObject.GetComponent<Image>().color = new Color32(vOut, 0, 0, 100);
                break;
            case 1:
                gameObject.GetComponent<Image>().color = new Color32(0, vOut, 0, 100);
                break;
            case 2:
                gameObject.GetComponent<Image>().color = new Color32(0, 0, vOut, 100);
                break;
            case 3:
                gameObject.GetComponent<Image>().color = new Color32(0, vOut, vOut, 100);
                break;

        }


        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles =
            new Vector3(0, 0, (Mathf.Atan2(dir.y, dir.x) * 180 / Mathf.PI));
        return gameObject;
    }

    private GameObject CreateHeartRateText(Vector2 anchoredPosition, int colour, float BPM)
    {

        GameObject gameObject = new GameObject("HeartRateText", typeof(Text));
        gameObject.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        

        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(40, 40);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        gameObject.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        gameObject.GetComponent<Text>().fontSize = 30;

        switch (colour % 4)
        {
            case 0:
                gameObject.GetComponent<Text>().color = new Color32(255, 0, 0, 100);
                break;
            case 1:
                gameObject.GetComponent<Text>().color = new Color32(0, 255, 0, 100);
                break;
            case 2:
                gameObject.GetComponent<Text>().color = new Color32(0, 0, 255, 100);
                break;
            case 3:
                gameObject.GetComponent<Text>().color = new Color32(0, 255, 255, 100);
                break;

        }


        gameObject.GetComponent<UnityEngine.UI.Text>().text = BPM.ToString();



        return gameObject;
    }
    private void Generate()
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();

         



        //GrabBluetooth ID Data
        var BPM = UnityEngine.Random.Range(0, 200);
        var confidence = UnityEngine.Random.Range(40, 100);
        var ID = "1";

        //run perbluetooth update.
        Hasher(ID, BPM, confidence);

        BPM = UnityEngine.Random.Range(0, 200);
        confidence = UnityEngine.Random.Range(40, 100);
        ID = "2";

        Hasher(ID, BPM, confidence);

        BPM = UnityEngine.Random.Range(0, 200);
        confidence = UnityEngine.Random.Range(40, 100);
        ID = "3";

        Hasher(ID, BPM, confidence);

        BPM = UnityEngine.Random.Range(0, 200);
        confidence = UnityEngine.Random.Range(40, 100);
        ID = "4";

        Hasher(ID, BPM, confidence);



        //Iterates through each keu in the hashmap, and shows the graph for each existing key.
        int colour = 0;


        Dictionary<string, List<BPMEntry>>.KeyCollection keys = ValueTable.Keys;
        foreach (string key in keys)
        {
           
            ShowGraph(ValueTable[key], colour);

           

            colour++;
        }
    }

    public void Hasher(string id, int BPM, int confidence)
    {



        //Attemtps to add a value to the hashmap using the ID., Skips this step if it already exists.
        if (ValueTable.ContainsKey(id))
        {
            List<BPMEntry> entries;
            ValueTable.TryGetValue(id, out entries);

            entries.Add(new BPMEntry(BPM, confidence));
            entries.RemoveAt(0);
        }
        else
        {
            var list = new List<BPMEntry>();

            for (int i = 0; i < 14; i++)
            {
                list.Add(new BPMEntry(0, 0));
            }

            list.Add(new BPMEntry(BPM, confidence));

            ValueTable.Add(id, list);
        }

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