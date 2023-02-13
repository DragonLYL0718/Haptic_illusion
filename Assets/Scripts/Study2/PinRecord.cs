using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using Valve.VR;

public class PinRecord : MonoBehaviour
{
    //scale of the pin
    private readonly float xScale = 0.5f;
    private readonly float zScale = 0.5f;
    public readonly float raycastHeight = 6f;
    private Vector3 StartPoint = new Vector3(-4.75f, 0f, -4.75f);

    public GameObject pin;
    public GameObject floor;

    private float[,] hitDistances;
    private GameObject[,] pinArray;

    private float PreSample = 15f;
    private bool PreIllusion = false;

    private int IsInstantiate = 0;
    private bool InitFlag = false;

    private float ratio = 6f / 0.9f;

    [Range(0f, 2f)]
    public float DetectHeight;
    [Range(0f, 2f)]
    public float AdjustHeight;
    private float max = 0;

    private string Pinfilename;
    private string Solefilename;
    private TextWriter tw;
    GameObject ActualPosition;
    private string userID;

    private float RecordTime = 0f;

    //layer mask for Sole Parts
    private int[] layerMasks = new int[6];
    private readonly string[] layers = { "SoleA", "SoleB", "SoleC", "SoleD", "SoleE", "SoleF" };
    int layerMask;

    //public GameObject floor;

    ////Start is called before the first frame update
    //void Start()
    //{

    //}

    private void Awake()
    {
        pinArray = new GameObject[20, 20];
        hitDistances = new float[20, 20];

        //set layermask
        layerMask = 0;
        for (int i = 0; i < 6; i++)
        {
            layerMasks[i] = 6 + i;
        }
        layerMask = LayerMask.GetMask(layers);
    }

    // Update is called once per frame
    void Update()
    {
        Init();
        CreatePinArray();
        DetectSole();
    }

    void Init()
    {
        if(!InitFlag)
        {
            ActualPosition = GameObject.Find("ActualPosition");
            userID = ActualPosition.GetComponent<SurveySystem2>().userID;
            Pinfilename = "./UserLog/pin_data_" + userID + ".csv";
            tw = new StreamWriter(Pinfilename, false);
            tw.Write("Time, Trial Number, Study Part, Sample, Illusion Sample");
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                    tw.Write(", Pin" + (i + 1) + "_" + (j + 1));
            tw.Write(", Total, No Touch");
            tw.Write("\r\n");
            tw.Close();

            Solefilename = "./UserLog/sole_data_" + userID + ".csv";
            tw = new StreamWriter(Solefilename, false);
            tw.WriteLine("Time, Trial Number, Study Part, Sample, Illusion Sample, Sole A, Sole B, Sole C, Sole D, Sole E, Sole F, Total, No Touch");
            tw.Close();

            InitFlag = true;
        }
        //InstantiatePins();
    }

    private void CreatePinArray()
    {
        ////Debug.Log("3");
        //if (IsInstantiate == 2)
        //{
        //    //Debug.Log("2");
            if ((PreSample != Randomize2.samples[SurveySystem2.number] || PreIllusion != Randomize2.illusions[SurveySystem2.number]) && PinController2.isInitialized)
            {
                //Debug.Log("4");
                PreSample = Randomize2.samples[SurveySystem2.number];
                PreIllusion = Randomize2.illusions[SurveySystem2.number];
                StartRaycastHeight();
                //PinControl();
            }
        //}
    }

    private void SetPinHeight(GameObject pin, float height)
    {
        pin.transform.position = transform.TransformPoint(new Vector3(pin.transform.localPosition.x, height, pin.transform.localPosition.z));
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
        int layerMask = 1 << 4;
        int num = 0;
        bool ChangeFlag = false;

        do
        {
            ChangeFlag = false;
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                {
                    Vector3 raycastOrigin = gameObject.transform.TransformPoint(StartPoint + new Vector3(i * xScale, raycastHeight, j * zScale));
                    if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask))
                    {
                        hitDistances[i, j] = raycastHeight - hit.distance * ratio - AdjustHeight;
                        //Debug.Log("Pin: " + hitDistances[i, j]);
                        if (hitDistances[i, j] < -0.1f)
                        {
                            hitDistances[i, j] = -0.1f;
                        }
                        //Debug.DrawRay(raycastOrigin, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
                    }
                    else
                    {
                        hitDistances[i, j] = 0;
                    }

                    if (hit.distance > max)
                    {
                        max = hit.distance;
                        ratio = raycastHeight / max;
                        //Debug.Log((num++) + "-Max: " + max + "," + ratio);
                        ChangeFlag = true;
                    }
                }
        } while(ChangeFlag);
        //Debug.Log(layerMask);
    }

    private void PinControl()
    {
        for (int i = 0; i < 20; i++)
            for (int j = 0; j < 20; j++)
                SetPinHeight(pinArray[i, j], hitDistances[i, j]);
    }

    private void DetectSole()
    {
        //Debug.Log("2");
        //Debug.Log(layerMask);
        if (SurveySystem2.RecordFlag && Randomize2.isRandomized)
        {
            //Debug.Log("1");

            int[,] PinHit = new int[20, 20];
            int TotalPin = 0;
            bool NoTouch = true;
            int[] soleHit = new int[7];

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                {
                    if (hitDistances[i, j] > 0)
                    {
                        //Debug.Log("Ray");
                        Vector3 raycastOrigin = gameObject.transform.TransformPoint(StartPoint + new Vector3(i * xScale, hitDistances[i, j], j * zScale));
                        //pinArray[i, j].transform.position = raycastOrigin;
                        if (Physics.Raycast(raycastOrigin, Vector3.up, out RaycastHit hit, /*Mathf.Infinity*/DetectHeight, layerMask))
                        {
                            PinHit[i, j] = 1;
                            TotalPin++;

                            for (int k = 0; k < 6; k++)
                            {
                                if (hit.collider.gameObject.layer == layerMasks[k])
                                {
                                    soleHit[k] = 1;
                                    //Debug.DrawRay(raycastOrigin, Vector3.up * hit.distance, Color.red, 10f);
                                }
                            }
                            //Debug.DrawRay(raycastOrigin, transform.TransformDirection(Vector3.up) * hit.distance, Color.red);
                        }
                        else
                            PinHit[i, j] = 0;
                        //Debug.DrawRay(raycastOrigin, Vector3.up * hit.distance, Color.red, 0.5f);
                    }
                    //else Debug.Log("No Ray");
                }
            soleHit[6] = soleHit[0] + soleHit[1] + soleHit[2] + soleHit[3] + soleHit[4] + soleHit[5];

            if(TotalPin > 0)
                NoTouch = false;
            RecordTime += Time.deltaTime;

            tw = new StreamWriter(Solefilename, true);
            tw.WriteLine(RecordTime + "," + (SurveySystem2.number + 1) + "," + PinController2.StudyPart + "," + Randomize2.samples[SurveySystem2.number] + "," + Randomize2.illusions[SurveySystem2.number]
                + "," + soleHit[0] + "," + soleHit[1] + "," + soleHit[2] + "," + soleHit[3] + "," + soleHit[4] + "," + soleHit[5] + "," + soleHit[6] + "," + NoTouch);
            tw.Close();
            //Debug.Log(soleHit[0] + "," + soleHit[1] + "," + soleHit[2] + "," + soleHit[3] + "," + soleHit[4] + "," + soleHit[5] + "," + soleHit[6]);

            tw = new StreamWriter(Pinfilename, true);
            tw.Write(RecordTime + "," + (SurveySystem2.number + 1) + "," + PinController2.StudyPart + "," + Randomize2.samples[SurveySystem2.number] + "," + Randomize2.illusions[SurveySystem2.number]);
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                    tw.Write("," + PinHit[i, j]);
            //tw.Write("," + hitDistances[i, j]);
            tw.Write("," + TotalPin + "," + NoTouch);
            tw.Write("\r\n");
            tw.Close();
        }
        else
            RecordTime = 0f;
    }
}

