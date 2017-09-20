﻿using System;
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
        public float x, y, z, qx, qy, qz, qw;
    }

    private ViveTrackerData vtData;
    private Vector3 vtPosition;
    private Quaternion vtRotation;

	// Use this for initialization
	void Start () {
        soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        vtData = new ViveTrackerData();
        vtPosition = new Vector3();
        vtRotation = new Quaternion();

        Thread pthread = new Thread(new ThreadStart(SocRecv));
        recv = true;
        pthread.Start();
	}
	
	// Update is called once per frame
	void Update () {
		lock (lockObject)
        {
            vtPosition = new Vector3(vtData.x, vtData.y, vtData.z);
            vtRotation = new Quaternion(vtData.qx, vtData.qy, vtData.qz, vtData.qw);
            Debug.Log(vtPosition);
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
            }
        }

        udpc.Close();
    }
}
