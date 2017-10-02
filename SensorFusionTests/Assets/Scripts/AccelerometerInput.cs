using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerometerInput : MonoBehaviour {

    private Vector3 accel;
    private Vector3 vel;
    private Vector3 pos;
    private Quaternion rot;

	// Use this for initialization
    void Start () {
        accel = new Vector3();
        vel = new Vector3();
    }

    public Vector3 getAccel()
    {
        return accel;
    }

    public Vector3 getVel()
    {
        return vel;
    }

    public Vector3 getPos()
    {
        return pos;
    }

    public Quaternion getRot()
    {
        return rot;
    }
	
	// Update is called once per frame
    void Update () {
        rot = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
        accel = rot * Input.acceleration;
        accel.Scale(new Vector3(-1, 1, 1)); // x is flipped in phone -> Unity axis
        accel += rot * Vector3.up;
        vel += accel * Time.deltaTime;
        pos += vel * Time.deltaTime;
    }
}
