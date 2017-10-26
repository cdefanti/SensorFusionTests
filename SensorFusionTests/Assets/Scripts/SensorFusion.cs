using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorFusion : MonoBehaviour {

    public AccelerometerInput localData;

    private Vector3 velGuess;
    private Vector3 lastKnownPos;

    private Vector3 gtPos;
    private Quaternion gtRot;
    private float gtLastUpdateTime;

    public bool fuse = false;

	// Use this for initialization
	void Start () {
        velGuess = Vector3.zero;
        lastKnownPos = Vector3.zero;
	}

    public void UpdateGTData(Vector3 pos, Quaternion rot, Vector3 vel, float time)
    {
        gtPos = pos;
        gtRot = rot;
        velGuess = vel;
        gtLastUpdateTime = time;
        transform.position = gtPos;
        /*
        transform.position = Vector3.Lerp(gtPos, transform.position, Vector3.Distance(this.transform.position, gtPos));
        
        Quaternion imu = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);

        Quaternion inv = Quaternion.Inverse(imu);
        Quaternion optical = rot * inv;
        Quaternion oldOrientation = this.transform.rotation;

        float yOpt = optical.eulerAngles.y;
        float yOld = oldOrientation.eulerAngles.y;
        float yDiff = Mathf.Abs(yOpt - yOld);
        if (yDiff > 180f)
        {
            if (yOpt < yOld)
            {
                yOpt += 360f;
            }
            else
            {
                yOld += 360f;
            }
            yDiff = Mathf.Abs(yOpt - yOld);
        }
        float t = yDiff / 180f;
        t = t * t;
        float yNew = Mathf.LerpAngle(yOld, yOpt, t);
        this.transform.rotation = Quaternion.AngleAxis(yNew, Vector3.up);
        */
    }

    // Update is called once per frame
    void Update () {
        return;
        if (Input.GetMouseButtonUp(0))
        {
            MouseUp();
        }
        float posAlpha;
        float rotAlpha;

        Vector3 localPos = localData.transform.position;
        Quaternion localRot = localData.transform.rotation;
        /*
        // blend position
        if (lastKnownPos != gtPos)
        {
            if (lastKnownPos != Vector3.zero)
            {
                velGuess = gtPos - lastKnownPos;
            }
            lastKnownPos = gtPos;
        }
        else
        {
            Vector3 localAccel = localData.getAccel();
            velGuess += localAccel;
        }
        */
        
        //posAlpha = 1f - posAlpha;
        if (fuse)
        {
            Vector3 posGuess = this.transform.position + velGuess * Time.deltaTime;
            this.transform.position = posGuess;
        }
        //rotAlpha = 

        //this.transform.rotation = Quaternion.Slerp(localRot, gtRot, rotAlpha);
	}

    void MouseUp()
    {
        fuse = !fuse;
    }
}
