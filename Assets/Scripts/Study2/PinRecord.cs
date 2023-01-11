using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinRecord : MonoBehaviour
{
    //scale of the pin
    private readonly float xScale = 0.5f;
    private readonly float zScale = 0.5f;
    public readonly float raycastHeight = 6f;
    private readonly Vector3 StartPoint = new Vector3(-4.75f, 0.125f, -4.75f);

    public GameObject pin;

    private float[,] hitDistances;

    private GameObject[,] pinArray;

    private float PreSample = 15f;
    private bool PreIllusion = false;

    private int IsInstantiate = 0;

    private readonly float ratio = 6f / 0.9189f;

    [Range(0f, 1f)]
    public float DetectHeight;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //    

    //}

    private void Awake()
    {
        //pinArray = new GameObject[20, 20];
        hitDistances = new float[20, 20];
    }

    // Update is called once per frame
    void Update()
    {
        //InstantiatePins();
        CreatePinArray();
    }

    private void CreatePinArray()
    {
        if (IsInstantiate == 2)
        {
            if (PreSample != Randomize2.samples[SurveySystem2.number] || PreIllusion != Randomize2.illusions[SurveySystem2.number])
            {
                PreSample = Randomize2.samples[SurveySystem2.number];
                PreIllusion = Randomize2.illusions[SurveySystem2.number];
                StartRaycastHeight();
                //PinControl();

            }
        }
    }

    private void SetPinHeight(GameObject pin, float height)
    {
        pin.transform.localPosition = new Vector3(pin.transform.localPosition.x, height, pin.transform.localPosition.z);
    }

    //Copy pin
    private void InstantiatePins()
    {
        if (IsInstantiate == 0 || IsInstantiate == 1)
        {
            for (int i = 10 * IsInstantiate; i < (IsInstantiate + 1) * 10; i++)
                for (int j = 0; j < 20; j++)
                    pinArray[i, j] = Instantiate(pin, transform.TransformPoint(StartPoint + new Vector3(i * xScale, 0, j * zScale)), Quaternion.identity, gameObject.transform);
            IsInstantiate++;
        }
    }

    //Measure the height of the pin
    private void StartRaycastHeight()
    {
        float max = 0;
        int layerMask = 1 << 7;
        RaycastHit hit;

        for (int i = 0; i < 20; i++)
            for (int j = 0; j < 20; j++)
            {
                Vector3 raycastOrigin = transform.TransformPoint(StartPoint + new Vector3(i * xScale, raycastHeight, j * zScale));
                if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, Mathf.Infinity, layerMask))
                {
                    hitDistances[i, j] = raycastHeight - pin.transform.localScale.y / 2 - hit.distance * ratio;
                    if (hitDistances[i, j] < 0)
                    {
                        hitDistances[i, j] = 0;
                    }
                    //Debug.DrawRay(raycastOrigin, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
                }
                else
                {
                    hitDistances[i, j] = 0;
                }

                //if (hit.distance > max)
                //{ 
                //    max = hit.distance;
                //    Debug.Log(max);
                //}
            }
    }

    private void PinControl()
    {
        for (int i = 0; i < 20; i++)
            for (int j = 0; j < 20; j++)
                SetPinHeight(pinArray[i, j], hitDistances[i, j]);
    }

    private void DetectSole()
    {
        int layerMask = 1 << 6;

        for (int i = 0; i < 20; i++)
            for (int j = 0; j < 20; j++)
            {
                Vector3 raycastOrigin = transform.TransformPoint(StartPoint + new Vector3(i * xScale, hitDistances[i, j], j * zScale));
                if (Physics.Raycast(raycastOrigin, Vector3.up, out RaycastHit hit, DetectHeight, layerMask))
                {
                    hitDistances[i, j] = raycastHeight - pin.transform.localScale.y / 2 - hit.distance * ratio;
                    if (hitDistances[i, j] < 0)
                    {
                        hitDistances[i, j] = 0;
                    }
                    //Debug.DrawRay(raycastOrigin, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
                }
                else
                {
                    hitDistances[i, j] = 0;
                }
            }
    }
}

