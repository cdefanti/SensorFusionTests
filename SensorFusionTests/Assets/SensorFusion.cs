using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorFusion : MonoBehaviour {

    public ViveTrackerReceiver vtData;
    public AccelerometerInput localData;

	// Use this for initialization
	void Start () {
		
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
        posAlpha = Mathf.Clamp(Vector3.Distance(localPos, gtPos) * 10f, 0f, 1f);
        posAlpha = Mathf.Pow(posAlpha, 2f);
        this.transform.position = Vector3.Lerp(localPos, gtPos, posAlpha);


        //rotAlpha = 

        //this.transform.rotation = Quaternion.Slerp(localRot, gtRot, rotAlpha);
	}
}
