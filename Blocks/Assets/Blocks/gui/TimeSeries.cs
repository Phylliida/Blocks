using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSeries : MonoBehaviour
{

    float[] data;

    public bool modified = false;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (data != null && !modified)
        {
        }
    }
}
