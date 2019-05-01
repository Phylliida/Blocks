using Blocks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSeries : MonoBehaviour
{
    FastStackQueue<float> data;
    int posInData = 0;

    public bool modified = false;


    // Start is called before the first frame update
    void Start()
    {
        data = new FastStackQueue<float>(historyLen);

        for (int i = 0; i < historyLen; i++)
        {
            data.Enqueue(0);
        }
    }


    public void Push(float val)
    {
        data.Dequeue();
        data.Enqueue(val);
        modified = true;
    }

    Color32[] colors;

    public string title = "Untitled time series";
    public UnityEngine.UI.Text titleText;
    public UnityEngine.UI.Text maxVal;
    public UnityEngine.UI.Text minVal;
    public UnityEngine.UI.RawImage displayImage;


    public int graphHeight = 100;
    public int historyLen = 100;
    public int widthPerDataPoint = 10;

    public Color filledColor = Color.white;
    public Color unfilledColor = Color.black;

    Color32 ColorToColor32(Color color)
    {
        return new Color32((byte)(color.r * 254), (byte)(color.g * 254), (byte)(color.b * 254), (byte)(color.a * 254));
    }

    Texture2D timeSeriesTexture;


    void UpdateGraph()
    {

        if (UnityEngine.Profiling.Profiler.enabled)
        {
            return;
        }

        this.titleText.text = title;
        if (data != null && data.Count > 0)
        {
            // only recreate the array if it isn't the right size (or isn't created yet)
            if (colors == null || data.Count* widthPerDataPoint * graphHeight != colors.Length)
            {
                colors = new Color32[graphHeight * data.Count* widthPerDataPoint];
            }
        }
        else
        {
            return;
        }

        Color32 filledColor32 = ColorToColor32(filledColor);
        Color32 unfilledColor32 = ColorToColor32(unfilledColor);
        float minVal = float.MaxValue;
        float maxVal = float.MinValue;
        for (int i = 0; i < data.Count; i++)
        {
            minVal = System.Math.Min(data[i], minVal);
            maxVal = System.Math.Max(data[i], maxVal);
        }

        if (minVal == maxVal)
        {
            maxVal = minVal + 1.0f;
        }

        float divVal = (float)System.Math.Max(1, (graphHeight - 1));
        int dataLen = data.Count;
        for (int i = 0; i < dataLen; i++)
        {
            float curP;
            if (maxVal == minVal)
            {
                curP = 0.0f;
            }
            else
            {
               curP = (data[i] - minVal) / (maxVal - minVal);
            }

            for (int y = 0; y < graphHeight; y++)
            {
                float curY = y / divVal;
                int curI = i * widthPerDataPoint + y * widthPerDataPoint * dataLen;
                for (int j = 0; j < widthPerDataPoint; j++)
                {
                    if (curY <= curP)
                    {
                        colors[curI+j] = filledColor32;
                    }
                    else
                    {
                        colors[curI+j] = unfilledColor32;
                    }
                }
            }
        }

        if (timeSeriesTexture == null || timeSeriesTexture.width != data.Count* widthPerDataPoint || timeSeriesTexture.height != graphHeight)
        {
            timeSeriesTexture = new Texture2D(data.Count* widthPerDataPoint, graphHeight, TextureFormat.ARGB32, false);
        }
        timeSeriesTexture.SetPixels32(colors);
        timeSeriesTexture.Apply();

        this.minVal.text = minVal + "";
        this.maxVal.text = maxVal + "";
        this.displayImage.texture = timeSeriesTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (data != null && modified && this.displayImage != null && this.minVal != null && this.maxVal != null && this.titleText != null)
        {
            modified = false;
            UpdateGraph();
        }
        else
        {
            if (this.minVal == null)
            {
                Debug.LogWarning("minVal of TimeSeries is currently null, this needs to be assigned to a ui text component");
            }
            if(this.maxVal == null)
            {
                Debug.LogWarning("maxVal of TimeSeries is currently null, this needs to be assigned to a ui text component");
            }
            if(this.displayImage == null)
            {
                Debug.LogWarning("displayImage of TimeSeries is currently null, this needs to be assigned to a ui raw image component");
            }
            if (this.titleText == null)
            {
                Debug.LogWarning("titleText of TimeSeries is currently null, this needs to be assigned to a ui text image component");
            }
        }
    }
}
