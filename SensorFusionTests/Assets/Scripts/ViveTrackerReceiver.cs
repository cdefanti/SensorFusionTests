using System;
using System.Collections;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using UnityEngine;

public class ViveTrackerReceiver : MonoBehaviour {

    private Socket soc;
    private bool recv;

    static readonly object lockObject = new object();

    [Serializable]
    private class ViveTrackerData
    {
        public bool valid;
        public float x, y, z, qx, qy, qz, qw;
    }

    private ViveTrackerData vtData;
    private Vector3 vtPosition;
    private Quaternion vtRotation;

    private Vector3 vtVelocity;
    private Vector3 vtAccel;

    private bool isDataFresh;
    private float lastUpdateTime;

	// Use this for initialization
	void Start () {
        soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        vtData = new ViveTrackerData();
        vtPosition = new Vector3();
        vtRotation = new Quaternion();

        vtVelocity = new Vector3();
        vtAccel = new Vector3();

        Thread pthread = new Thread(new ThreadStart(SocRecv));
        recv = true;
        isDataFresh = false;
        lastUpdateTime = Time.time;
        pthread.Start();
	}
	
	// Update is called once per frame
	void Update () {
		lock (lockObject)
        {
            if (vtData.valid && isDataFresh)
            {
                Vector3 oldPos = new Vector3(vtPosition.x, vtPosition.y, vtPosition.z);
                Vector3 oldVel = new Vector3(vtVelocity.x, vtVelocity.y, vtVelocity.z);
                vtPosition = new Vector3(vtData.x, vtData.y, vtData.z);
                vtRotation = new Quaternion(vtData.qx, vtData.qy, vtData.qz, vtData.qw);
                vtVelocity = (vtPosition - oldPos) / (Time.time - lastUpdateTime);
                vtAccel = (vtVelocity - oldVel) / (Time.time - lastUpdateTime);

                lastUpdateTime = Time.time;
                isDataFresh = false;
            }
        }
	}

    public Vector3 getPosition()
    {
        return vtPosition;
    }

    public Quaternion getRotation()
    {
        return vtRotation;
    }

    public Vector3 getVel()
    {
        return vtVelocity;
    }

    public Vector3 getAccel()
    {
        return vtAccel;
    }

    private void OnApplicationQuit()
    {
        recv = false;
    }

    void SocRecv()
    {
        UdpClient udpc = new UdpClient(10000);
        byte[] buf;
        while (recv)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            buf = udpc.Receive(ref RemoteIpEndPoint);

            lock (lockObject)
            {
                string jsonString = Encoding.ASCII.GetString(buf);
                vtData = JsonUtility.FromJson<ViveTrackerData>(jsonString);
                isDataFresh = true;
            }
        }

        udpc.Close();
    }
}
