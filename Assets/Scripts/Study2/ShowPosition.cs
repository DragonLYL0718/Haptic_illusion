using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPosition : MonoBehaviour
{
    public Vector3 Position;
    public Vector3 Rotation;

    // Update is called once per frame
    void Update()
    {
        Position = transform.position;
        Rotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
    }
}
