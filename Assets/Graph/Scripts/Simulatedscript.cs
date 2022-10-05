using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulatedscript : MonoBehaviour
{
    //Timing feature
    float timer = 0f;
    float updatetime = 10f;

    //Simulated Data
    string ID = "1";
    int bpm = 0;
    int confidence = 0;

    public WindowGraph graph;

    private void Start()
    {

    }
    void Update()
    {
        timer += Time.deltaTime;

        bpm = Random.Range(30, 150);
        confidence = Random.Range(50, 100);

        if (timer > updatetime)
        {
            //Call other function with simulated data

            graph.AddEntry(ID, new BPMEntry(bpm, confidence));

            timer = 0;
            Debug.Log("Called with: " + bpm + " " + confidence);

        }

    }
}
