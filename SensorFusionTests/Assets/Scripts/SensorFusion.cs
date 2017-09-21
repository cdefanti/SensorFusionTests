using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorFusion : MonoBehaviour {

    public ViveTrackerReceiver vtData;
    public AccelerometerInput localData;

    private Vector3 velGuess;
    private Vector3 lastKnownPos;

	// Use this for initialization
	void Start () {
        velGuess = Vector3.zero;
        lastKnownPos = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        float posAlpha;
        float rotAlpha;

        Vector3 gtPos = vtData.getPosition();
        Quaternion gtRot = vtData.getRotation();

        Vector3 localPos = localData.transform.position;
        Quaternion localRot = localData.transform.rotation;

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
        Vector3 posGuess = this.transform.position + velGuess * Time.deltaTime;
        posAlpha = Mathf.Clamp(Vector3.Distance(posGuess, gtPos), 0f, 1f);
        //posAlpha = Mathf.Pow(posAlpha, 2f);
        //posAlpha = 1f - posAlpha;
        this.transform.position = Vector3.Lerp(posGuess, gtPos, posAlpha);
        Debug.Log("UNITYDEBUG: " + posAlpha);


        //rotAlpha = 

        //this.transform.rotation = Quaternion.Slerp(localRot, gtRot, rotAlpha);
	}
}
