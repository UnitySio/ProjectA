using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BufferingAnimation : MonoBehaviour
{
    public Image image;
    public RectTransform rect;

    private Vector3 eulerRotation;
    private float waitTime = 0f;
    void Update()
    {
        float time = Time.deltaTime * 1.5f;
        
        waitTime += time;
        eulerRotation.z -= (time / 1) * 180;
        
        if (waitTime > 1.5f)
        {
            waitTime = 0f;
            image.fillClockwise = !image.fillClockwise;
        }
        
        if (image.fillClockwise)
        {
            image.fillAmount = Mathf.Clamp(image.fillAmount + time, 0.07f, 0.93f);
        }
        else
        {
            image.fillAmount = Mathf.Clamp(image.fillAmount - time, 0.07f, 0.93f);
        }

        rect.localRotation = Quaternion.Euler(eulerRotation);
    }
}
