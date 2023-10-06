using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class Rotate3DUI : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float maxAngle = 45f;
    public float minAngle = -45f;
    private float currentAngle = 0f;
    private bool positive = true;

    // Start is called before the first frame update
    void Start()
    {
        currentAngle = Random.Range(minAngle, maxAngle);
    }

    // Update is called once per frame
    void Update()
    {
        if (positive) {    
            if (currentAngle < maxAngle)
            {
                transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
                currentAngle += rotationSpeed * Time.deltaTime;
            }
            else
            {
                positive = false;
            }
        }
        else
        {
            if (currentAngle > minAngle)
            {
                transform.Rotate(new Vector3(0, -rotationSpeed * Time.deltaTime, 0));
                currentAngle -= rotationSpeed * Time.deltaTime;
            }
            else
            {
                positive = true;
            }
        }
    }
}
