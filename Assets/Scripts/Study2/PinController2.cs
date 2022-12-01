using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class PinController2 : MonoBehaviour
{
    //locate trackers
    public GameObject leftBottomCorner;
    public GameObject rightBottomCorner;
    private bool ShoeCalibrated = false;

    private float distanceBetweenTrackers;

    private bool isInitialized = false;

    //Move one pin
    //public float interval;
    //Tilt rod by this angle
    [Range(0, 40)]
    public float angle;
    private float prevAngle = 0;
    //private float prevAngle;
    [Range(1, 1.6f)]
    public float scale;

    public GameObject sphere;
    public GameObject rod;
    public GameObject floor;
    public GameObject sole;
    private readonly float RodStartX = 10;
    [SerializeField]
    private Vector3 RodStartPosition;

    public enum Geometry
    {
        Sphere,
        Rod
    };
    public Geometry geometry;

    public enum RetargetingType
    {
        ScalingUp,
        Rotation
    };
    [HideInInspector]
    public RetargetingType type;

    //virtual image (red ball) of actual position(white ball)
    public GameObject tracker;
    public GameObject retargetedPosition;

    public enum InputMode
    {
        Manual,
        Automatic
    }
    public InputMode inputMode;

    private readonly Vector3 sphereStartScale = new Vector3(6, 6, 6);

    private string filename;
    private TextWriter tw;
    private string userID;

    [Range(0, 1f)]
    public float DeltaScaleUp;
    [Range(0, 1f)]
    public float DeltaRotation;
    private float RecordTime = 0;
    private string StudyPart;

    void Update()
    {
        Initialize();
        CalibratedShoe();
        ChooseGeometry();
        Retarget();
        RecordFoot();
    }

    private void Initialize()
    {
        if (!isInitialized)
        {
            //scale to match physical stick
            distanceBetweenTrackers = Vector3.Distance(leftBottomCorner.transform.position, rightBottomCorner.transform.position);

            //Redefine floor coordinates 
            floor.transform.localScale = new Vector3(0.1f * distanceBetweenTrackers, 0.1f * distanceBetweenTrackers, 0.1f * distanceBetweenTrackers);
            floor.transform.position = (leftBottomCorner.transform.position + rightBottomCorner.transform.position)/2 - 
                                  Vector3.Cross((leftBottomCorner.transform.position - rightBottomCorner.transform.position), Vector3.up).normalized * distanceBetweenTrackers / 2;
            floor.transform.RotateAround(floor.transform.position, Vector3.up, + Vector3.SignedAngle(Vector3.right, (rightBottomCorner.transform.position - leftBottomCorner.transform.position), Vector3.up));
            RodStartPosition = rod.transform.localPosition;

            userID = GetComponent<SurveySystem2>().userID;
            filename = "./UserLog/sole_data_" + userID + ".csv";
            tw = new StreamWriter(filename, false);
            tw.WriteLine("Time, Trial Number, Study Part, Sample, Illusion Sample, Sole A, Sole B, Sole C, Sole D, Sole E, Sole F, No Touch");
            tw.Close();

            if (type == RetargetingType.ScalingUp)
                StudyPart = "ScalingUp";
            else
                StudyPart = "Rotation";

            isInitialized = true;
        }
    }

    private void CalibratedShoe()
    {
        if (!ShoeCalibrated)
            if (Input.GetKeyDown(KeyCode.K))
            { 
                ShoeCalibrated = true;
            }
    }

    //Sphere, Rod, Stairs
    private void ChooseGeometry()
    {
        //Choose the geometry 
        switch (geometry)
        {
            // simulate sphere
            case Geometry.Sphere:
                type = RetargetingType.ScalingUp;
                ImitateSphere();
                break;

            //tilt rod
            case Geometry.Rod:
                type = RetargetingType.Rotation;
                ImitateRod();
                break;

            default:
                break;
        }
    }

    //Sphere
    private void ImitateSphere()
    {
        rod.SetActive(false);
        sphere.SetActive(true);

        if(inputMode == InputMode.Automatic)
            sphere.transform.localScale = sphereStartScale;
        else
            sphere.transform.localScale = sphereStartScale * scale/* / 2*/;
    }

    //Rod
    private void ImitateRod()
    {
        sphere.SetActive(false);
        rod.SetActive(true);

        if (inputMode == InputMode.Automatic)
            angle = Randomize2.samples[SurveySystem2.number];

        float newLength = RodStartX / Mathf.Cos(Mathf.Deg2Rad * angle);

        if (prevAngle != angle)
        {
            rod.transform.RotateAround(floor.transform.TransformPoint(rod.transform.position), Vector3.up, -prevAngle);
            rod.transform.localScale = new Vector3(newLength, rod.transform.localScale.y, rod.transform.localScale.z);
            rod.transform.RotateAround(floor.transform.TransformPoint(rod.transform.position), Vector3.up, angle);
            rod.transform.localPosition = RodStartPosition + (RodStartX * Mathf.Tan(Mathf.Deg2Rad * angle) * 0.5f) * Vector3.forward;
        }

        prevAngle = angle;
    }

    //Angle redirection
    private Vector3 Rotate(Vector3 vector, float angle)
    {
        float x, y, z, zOffset;
        Vector3 newPosition, RelativePosition;

        vector = floor.transform.InverseTransformPoint(vector);
        RelativePosition = vector - rod.transform.localPosition;
        x = RelativePosition.x;
        y = RelativePosition.y;
        z = RelativePosition.z;
        zOffset = (x - rod.transform.localPosition.x) * Mathf.Tan(Mathf.Deg2Rad * angle);
        RelativePosition = new Vector3(x, y, z - zOffset + RodStartX * Mathf.Tan(Mathf.Deg2Rad * angle) * 0.5f);
        RelativePosition += rod.transform.localPosition;
        newPosition = floor.transform.TransformPoint(RelativePosition);

        return newPosition;
    }

    //Scale up redirection
    private Vector3 ScaleUp(Vector3 vector, float scale)
    {
        Vector3 newPosition;

        vector = floor.transform.InverseTransformPoint(vector);
        newPosition = new Vector3((sphere.transform.position.x + (vector.x - sphere.transform.position.x) / scale), (sphere.transform.position.y + (vector.y - sphere.transform.position.y) / scale), (sphere.transform.position.z + (vector.z - sphere.transform.position.z) / scale));
        newPosition = floor.transform.TransformPoint(newPosition);

        return newPosition;
    }

    //Manual or Automatic
    private void ChooseInputMode()
    {
        switch (inputMode)
        {
            // set angle and scale factor manually from inspector
            case InputMode.Manual:
                SurveySystem2.timeIsRunning = false;
                break;

            //set sample angles automatically
            case InputMode.Automatic:
                if (SurveySystem2.number < Randomize2.samples.Length)
                {
                    angle = Randomize2.samples[SurveySystem2.number];
                    scale = Randomize2.samples[SurveySystem2.number];
                }
                else
                {
                    Debug.Log("This is the end of the experiment");
                    inputMode = InputMode.Manual;
                    SurveySystem2.timeIsRunning = false;
                }
                break;
            default:
                break;
        }
    }

    private void ChooseRedirectionType()
    {
        if(Randomize2.illusions[SurveySystem2.number])
        {
            switch (type)
            {
                case RetargetingType.ScalingUp:
                    retargetedPosition.transform.position = ScaleUp(tracker.transform.position, scale);
                    break;
                case RetargetingType.Rotation:
                    retargetedPosition.transform.position = Rotate(tracker.transform.position, angle);
                    break;
                default:
                    break;
            }
        }
        else
            retargetedPosition.transform.position = tracker.transform.position;

        if(inputMode == InputMode.Manual)
            retargetedPosition.transform.position = tracker.transform.position;
    }

    //Redirecting tracker
    private void Retarget()
    {
        retargetedPosition.transform.rotation = tracker.transform.rotation;
        ChooseInputMode();
        ChooseRedirectionType();
    }

    private void RecordFoot()
    {
        RecordTime += Time.deltaTime;

        if (SurveySystem2.RecordFlag)
        {
            if (type == RetargetingType.ScalingUp)
                RecordScaleUpFoot();
            else
                RecordRotateFoot();
        }
        else
            RecordTime = 0;
    }

    private void RecordScaleUpFoot()
    {
        float ContactDistant;
        int[] soleHit = new int[7];

        if (Randomize2.illusions[SurveySystem2.number])
            ContactDistant = 3f + DeltaScaleUp;
        else
            ContactDistant = 3f * scale + DeltaScaleUp;

        for(int i = 0; i < 6; i++) 
        {
            if(IsSoleContact(i, ContactDistant))
            {
                soleHit[i] = 1;
                soleHit[6]++;
            }
        }

        tw = new StreamWriter(filename, true);
        tw.WriteLine(RecordTime + "," + (SurveySystem2.number + 1) + "," + StudyPart + "," + scale + "," + Randomize2.illusions[SurveySystem2.number] 
            + "," + soleHit[0] + "," + soleHit[1] + "," + soleHit[2] + "," + soleHit[3] + "," + soleHit[4] + "," + soleHit[5] + "," + soleHit[6]);
        tw.Close();
    }

    private void RecordRotateFoot()
    {
        int[] soleHit = new int[7];

        for (int i = 0; i < 6; i++)
        {
            if(IsSoleContact(i))
            {
                soleHit[i] = 1;
                soleHit[6]++;
            }
        }

        tw = new StreamWriter(filename, true);
        tw.WriteLine(RecordTime + "," + (SurveySystem2.number + 1) + "," + StudyPart + "," + angle + "," + Randomize2.illusions[SurveySystem2.number]
            + "," + soleHit[0] + "," + soleHit[1] + "," + soleHit[2] + "," + soleHit[3] + "," + soleHit[4] + "," + soleHit[5] + "," + soleHit[6]);
        tw.Close();
    }

    private bool IsSoleContact(int i, float Distant)
    {
        if(Vector3.Distance(tracker.transform.GetChild(i).localPosition, floor.transform.position) < Distant)
            return true;
        else
            return false;
    }

    private bool IsSoleContact(int i)
    {
        if(sole.transform.GetChild(i).position.y < 1f + DeltaRotation)
        {
            if (Randomize2.illusions[SurveySystem2.number])
            {
                if(sole.transform.GetChild(i).position.z > -(5 + DeltaRotation) && sole.transform.GetChild(i).position.z < -(4.5 - DeltaRotation))
                    return true;
            }
            else
            {
                float ReferencePoint = -5 + RodStartX * Mathf.Tan(Mathf.Deg2Rad * angle) * 0.5f;
                if(sole.transform.GetChild(i).position.z > ReferencePoint - 0.5 - DeltaRotation - sole.transform.GetChild(i).position.x * Mathf.Tan(Mathf.Deg2Rad * angle) &&
                    sole.transform.GetChild(i).position.z < ReferencePoint + 0.5 + DeltaRotation - sole.transform.GetChild(i).position.x * Mathf.Tan(Mathf.Deg2Rad * angle))
                    return true;
            }
        }

        return false;
    }
}
