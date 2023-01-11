using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinRecord : MonoBehaviour
{
    //scale of the pin
    private readonly float xScale = 0.5f;
    private readonly float zScale = 0.5f;
    public readonly float raycastHeight = 6f;
    private float max = 0f;
    private readonly Vector3 StartPoint = new Vector3(-4.75f, 0.125f, -4.75f);//For the other cases
    private readonly Vector3 StartPoint2 = new Vector3(-4.5f, 0.125f, -4.5f);//For the case of radius 195

    //private GameObject sphere;
    //private GameObject rod;
    public GameObject pin;

    private float[,] hitDistances;
    private float[,] hitDistances2;

    private GameObject[,] pinArray;//For the other cases
    private GameObject[,] pinArray2;//For the case of radius 195

    //Move one pin;
    private float interval;

    private float PreSample = 15f;
    private bool PreIllusion = false;

    private bool IsInstantiate = false;

    // Start is called before the first frame update
    void Start()
    {
        //sphere = GetComponent<PinController2>().sphere;
        //rod = GetComponent<PinController2>().rod;
        
    }

    private void Awake()
    {
        pinArray = new GameObject[20, 20];
        pinArray2 = new GameObject[19, 19];
        hitDistances = new float[20, 20];
        hitDistances2 = new float[19, 19];
    }

    // Update is called once per frame
    void Update()
    {
        InstantiatePins();
        //CreatePinArray();
    }

    private void CreatePinArray()
    {
        if (PreSample != Randomize2.samples[SurveySystem2.number] || PreIllusion != Randomize2.illusions[SurveySystem2.number])
        {
            PreSample = Randomize2.samples[SurveySystem2.number];
            PreIllusion = Randomize2.illusions[SurveySystem2.number];
            StartRaycastHeight();
        }
    }

    private void SetPinHeight(GameObject pin, float height)
    {
        pin.transform.localPosition = new Vector3(pin.transform.localPosition.x, height, pin.transform.localPosition.y);
    }

    //Copy pin
    private void InstantiatePins()
    {
        if(!IsInstantiate) 
        {
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                    pinArray[i, j] = Instantiate(pin, transform.TransformPoint(StartPoint + new Vector3(i * xScale, 0, j * zScale)), Quaternion.identity, gameObject.transform);

            gameObject.transform.localPosition = new Vector3(-4.5f, 0.125f, -4.5f);//for the case of radius 195
            for (int i = 0; i < 19; i++)
                for (int j = 0; j < 19; j++)
                    pinArray2[i, j] = Instantiate(gameObject, transform.TransformPoint(StartPoint2 + new Vector3(i * xScale, 0, j * zScale)), Quaternion.identity, gameObject.transform);

            IsInstantiate = true;
        }
    }

    //Measure the height of the pin
    private void StartRaycastHeight()
    {
        int layerMask = 1 << 4;
        if (Randomize2.illusions[SurveySystem2.number] == false && PinController2.type == PinController2.RetargetingType.ScalingUp)
        {
            for (int i = 0; i < 19; i++)
                for (int j = 0; j < 19; j++)
                {
                    Vector3 raycastOrigin = transform.TransformPoint(StartPoint2 + new Vector3(i * xScale, raycastHeight, j * zScale));
                    if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask))
                    {
                        hitDistances[i, j] = raycastHeight - pinArray[i, j].transform.localScale.y / 2 - hit.distance;
                        if (hitDistances2[i, j] < 0)
                        {
                            hitDistances2[i, j] = 0;
                        }
                        //Debug.DrawRay(raycastOrigin, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
                    }
                    else
                    {
                        hitDistances2[i, j] = 0;
                    }
                }
        }
        else
        {
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                {
                    Vector3 raycastOrigin = transform.TransformPoint(StartPoint + new Vector3(i * xScale, raycastHeight, j * zScale));
                    if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask))
                    {
                        hitDistances[i, j] = raycastHeight - pinArray[i, j].transform.localScale.y / 2 - hit.distance;
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

                    if (hitDistances[i, j] > max)
                    {
                        max = hitDistances[i, j];
                    }
                }
        }
    }

    private void PinControl()
    {
        if (Randomize2.illusions[SurveySystem2.number] == false && PinController2.type == PinController2.RetargetingType.ScalingUp)
        {
            for (int i = 0; i < 19; i++)
                for (int j = 0; j < 19; j++)
                    SetPinHeight(pinArray2[i, j], hitDistances[i, j]);

        }
        else
        {
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                    SetPinHeight(pinArray[i, j], hitDistances[i, j]);
        }
    }
}

