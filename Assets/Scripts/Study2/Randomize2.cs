using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomize2 : MonoBehaviour
{
    [HideInInspector]
    private readonly float[] angleSamples = {10, 10, 20, 20, 30, 30, 40, 40};
    private readonly float[] scaleSamples = { 1.08f, 1.08f, 1.23f, 1.23f, 1.38f, 1.38f, 1.54f, 1.54f };
    //private readonly float[] scaleSamples = { 1.54f, 1.54f, 1.54f, 1.54f, 1.54f, 1.54f, 1.54f, 1.54f };
    public static float[] samples = new float[16];
    public static bool[] illusions = new bool[16];

    private PinController2.Geometry  Geometry;
    private PinController2.RetargetingType retargetingType;

    private bool isRandomized = false;

    private void Start()
    {
        Geometry = GetComponent<PinController2>().geometry;
        if (Geometry == PinController2.Geometry.Sphere)
            retargetingType = PinController2.RetargetingType.ScalingUp;
        else
            retargetingType = PinController2.RetargetingType.Rotation;
    }

    void Update()
    {
        if (!isRandomized)
        {
            if (retargetingType == PinController2.RetargetingType.Rotation)
            {
                RandomizeTrial(angleSamples);
            }
            else if (retargetingType == PinController2.RetargetingType.ScalingUp)
            {
                RandomizeTrial(scaleSamples);
            }

            for (int i = 0; i < samples.Length; i++)
            {
                Debug.Log((i + 1) + " " + samples[i] + " " + illusions[i]);
            }
            isRandomized = true;
        }
    }

    private void Randomize(float[] array)
    {
        //shuffling array elements
        for (int t = 0; t < array.Length; t++)
        {
            float tmp = array[t];
            int r = Random.Range(t, array.Length);
            array[t] = array[r];
            array[r] = tmp;
        }
    }

    private void RandomizeTrial(float[] array)
    {
        Randomize(array);
        for (int i = 0; i < 8; i++)
        {
            samples[2 * i] = array[i];
            samples[2 * i + 1] = array[i];
            int r = Random.Range(1, 20);
            if (r < 11)
            {
                illusions[2 * i] = true;
                illusions[2 * i + 1] = false;
                //illusions[2 * i + 1] = true;
            }
            else
            {
                illusions[2 * i] = false;
                //illusions[2 * i] = true;
                illusions[2 * i + 1] = true;
            }
        }
    }
}
