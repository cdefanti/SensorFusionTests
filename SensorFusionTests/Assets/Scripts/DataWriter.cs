using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataWriter : MonoBehaviour {

    private bool recording;

    private Dictionary<string, List<Vector3>> vector3sToPrint;

    public SensorFusion data;

	// Use this for initialization
	void Start () {
        recording = false;
        vector3sToPrint = new Dictionary<string, List<Vector3>>();
        vector3sToPrint["acl_gt"] = new List<Vector3>();
        vector3sToPrint["acl_ph"] = new List<Vector3>();

        vector3sToPrint["vel_gt"] = new List<Vector3>();
        vector3sToPrint["vel_ph"] = new List<Vector3>();

        vector3sToPrint["pos_gt"] = new List<Vector3>();
        vector3sToPrint["pos_ph"] = new List<Vector3>();
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonUp(0))
        {
            MouseUp();
        }
        if (recording)
        {
            vector3sToPrint["acl_ph"].Add(data.localData.getAccel());
            vector3sToPrint["vel_ph"].Add(data.localData.getVel());
            vector3sToPrint["pos_ph"].Add(data.localData.getPos());

            //if (vector3sToPrint["pos_gt"].Count == 0 || vector3sToPrint["pos_gt"][vector3sToPrint["pos_gt"].Count - 1] != data.vtData.getPosition())
            //{
                //vector3sToPrint["acl_gt"].Add(data.vtData.getAccel());
                //vector3sToPrint["vel_gt"].Add(data.vtData.getVel());
                //vector3sToPrint["pos_gt"].Add(data.vtData.getPosition());
            //}
        }
	}

    void MouseUp()
    {
        recording = !recording;
        if (!recording)
        {
            SaveData();
        } else
        {
            Debug.Log("recording...");
        }
    }

    void SaveData()
    {
        foreach (KeyValuePair<string, List<Vector3>> dataPoint in vector3sToPrint)
        {
            string path = Application.persistentDataPath + "/" + dataPoint.Key + ".txt";
            Debug.Log("saving " + path);
            string[] output = new string[dataPoint.Value.Count];
            for (int i = 0; i < dataPoint.Value.Count; i++)
            {
                output[i] = string.Format("{0}\t{1}\t{2}", dataPoint.Value[i].x, dataPoint.Value[i].y, dataPoint.Value[i].z);
            }
            File.WriteAllLines(path, output);
            Debug.Log("done.");
        }
    }
}
