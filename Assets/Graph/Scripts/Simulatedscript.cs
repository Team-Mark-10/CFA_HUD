using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulatedscript : MonoBehaviour
{
    //Timing feature
    float timer = 0f;

    float updatetime = 2f;

    //Simulated Data
    string ID1 = "1";

    string ID2 = "2";

    string ID3 = "3";

    public WindowGraph graph;

    private void Start()
    {
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > updatetime)
        {
            //Call other function with simulated data
            graph
                .AddEntry(ID1,
                new DataEntry(Random.Range(30, 200), Random.Range(70, 100)));

            graph
                .AddEntry(ID2,
                new DataEntry(Random.Range(30, 200), Random.Range(70, 100)));
            graph
                .AddEntry(ID3,
                new DataEntry(Random.Range(30, 200), Random.Range(70, 100)));

            timer = 0;
        }
    }
}
