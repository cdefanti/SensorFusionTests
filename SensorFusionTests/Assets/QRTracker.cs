using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using UnityEngine;

public class QRTracker : MonoBehaviour
{
    private bool recv;

    static readonly object lockObject = new object();

    [Serializable]
    private class QRTrackerData
    {
        public bool valid;
        public float x, y, z;
    }

    private QRTrackerData qrData;
    private Vector3 qrPosition;

    private Vector3 qrVelocity;
    private Vector3 qrAccel;

    private bool isDataFresh;
    private float lastUpdateTime;

    public SensorFusion sf;

    Thread pthread;
    UdpClient udpc;
    Socket soc;

    // Use this for initialization
    void Start()
    {
        qrData = new QRTrackerData();
        qrPosition = new Vector3();

        pthread = new Thread(new ThreadStart(SocRecv));
        recv = true;
        isDataFresh = false;
        lastUpdateTime = Time.time;
        pthread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        lock (lockObject)
        {
            if (qrData.valid && isDataFresh)
            {
                Vector3 oldPos = new Vector3(qrPosition.x, qrPosition.y, qrPosition.z);
                Vector3 oldVel = new Vector3(qrVelocity.x, qrVelocity.y, qrVelocity.z);
                qrPosition = new Vector3(qrData.x, qrData.y, qrData.z);
                qrVelocity = (qrPosition - oldPos) / (Time.time - lastUpdateTime);
                qrAccel = (qrVelocity - oldVel) / (Time.time - lastUpdateTime);

                lastUpdateTime = Time.time;
                isDataFresh = false;

                sf.UpdateGTData(qrPosition, Quaternion.identity, qrVelocity, lastUpdateTime);
            }
        }
    }

    public Vector3 getPosition()
    {
        return qrPosition;
    }

    public Vector3 getVel()
    {
        return qrVelocity;
    }

    public Vector3 getAccel()
    {
        return qrAccel;
    }

    public float getTimeSinceLastUpdate()
    {
        return Time.time - lastUpdateTime;
    }

    void OnDisable()
    {
        if (pthread != null)
            pthread.Abort();

        soc.Close();
        recv = false;
    }

    void SocRecv()
    {
        soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        byte[] buf = new byte[1024];
        IPEndPoint LocalIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.42"), 8000);
        soc.Bind(LocalIpEndPoint);

        Debug.Log("test3");

        while (recv)
        {
            try
            {
                soc.Receive(buf);

                lock (lockObject)
                {
                    string jsonString = Encoding.ASCII.GetString(buf);
                    Debug.Log(jsonString);
                    jsonString = jsonString.Substring(0, jsonString.IndexOf('}')+1);
                    qrData = JsonUtility.FromJson<QRTrackerData>(jsonString);
                    isDataFresh = true;
                }
                Debug.Log("decoded");
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        

        soc.Close();
    }
}