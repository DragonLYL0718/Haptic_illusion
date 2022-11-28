using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinController2 : MonoBehaviour
{
    //locate trackers
    public GameObject leftBottomCorner;
    public GameObject rightBottomCorner;
    private Vector3 ColibrationVector;
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
    private float RodStartX = 10;
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

    private Vector3 sphereStartScale = new Vector3(6, 6, 6);

    void Update()
    {
        Initialize();
        if (!ShoeCalibrated)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                CalibratedShoe();
                ShoeCalibrated = true;
            }
        }
        ChooseGeometry();
        Retarget();
    }

    private void Initialize()
    {
        if (!isInitialized)
        {
            //scale to match physical stick
            distanceBetweenTrackers = Vector3.Distance(leftBottomCorner.transform.position, rightBottomCorner.transform.position);
            ColibrationVector = rightBottomCorner.transform.position - leftBottomCorner.transform.position;

            //Redefine floor coordinates 
            floor.transform.localScale = new Vector3(0.1f * distanceBetweenTrackers, 0.1f * distanceBetweenTrackers, 0.1f * distanceBetweenTrackers);
            floor.transform.position = (leftBottomCorner.transform.position + rightBottomCorner.transform.position)/2 - 
                                  Vector3.Cross((leftBottomCorner.transform.position - rightBottomCorner.transform.position), Vector3.up).normalized * distanceBetweenTrackers / 2;
            floor.transform.RotateAround(floor.transform.position, Vector3.up, + Vector3.SignedAngle(Vector3.right, (rightBottomCorner.transform.position - leftBottomCorner.transform.position), Vector3.up));

            RodStartPosition = rod.transform.localPosition;

            isInitialized = true;
        }
    }

    private void CalibratedShoe()
    { 
        
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
        float x ;
        float y ;
        float z ;
        float zOffset;
        Vector3 newPosition, RelativePosition;
        RelativePosition = vector - rod.transform.position;
        RelativePosition = Quaternion.AngleAxis(floor.transform.localRotation.y, Vector3.up) * RelativePosition;
        x = RelativePosition.x;
        y = RelativePosition.y;
        z = RelativePosition.z;
        zOffset = (x - rod.transform.position.x) * Mathf.Tan(Mathf.Deg2Rad * angle * 1.0f);
        RelativePosition = new Vector3(x, y, z - zOffset + RodStartX * Mathf.Tan(Mathf.Deg2Rad * angle) * 0.5f);
        RelativePosition = Quaternion.AngleAxis(-floor.transform.localRotation.y, Vector3.up) * RelativePosition;
        newPosition = rod.transform.position + RelativePosition;

        return newPosition;
    }

    //Scale up redirection
    private Vector3 ScaleUp(Vector3 vector, float scale)
    {
        Vector3 newPosition = new Vector3((sphere.transform.position.x + (vector.x - sphere.transform.position.x) / scale), (sphere.transform.position.y + (vector.y - sphere.transform.position.y) / scale), (sphere.transform.position.z + (vector.z - sphere.transform.position.z) / scale));
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
                    retargetedPosition.transform.position = ScaleUp(tracker.gameObject.transform.position, scale);
                    break;
                case RetargetingType.Rotation:
                    retargetedPosition.transform.position = Rotate(tracker.gameObject.transform.position, angle);
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
        retargetedPosition.transform.rotation = tracker.gameObject.transform.rotation;
        ChooseInputMode();
        ChooseRedirectionType();
    }
}
