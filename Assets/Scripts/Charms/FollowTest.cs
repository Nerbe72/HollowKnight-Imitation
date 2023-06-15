using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowTest : MonoBehaviour
{
    float time;

    void Update()
    {
        time += Time.deltaTime * 2.2f;

        if (time >= 360)
            time = 0;
        float x = (0.3f * Mathf.Sqrt(2) * Mathf.Cos(time)) / (Mathf.Sin(time) * Mathf.Sin(time) + 1);
        float y = (0.3f * Mathf.Sqrt(2) * Mathf.Cos(time) * Mathf.Sin(time)) / (Mathf.Sin(time) * Mathf.Sin(time) + 1);

        transform.localPosition = new Vector3(x, y, 0);
    }
}
