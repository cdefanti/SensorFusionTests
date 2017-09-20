using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerometerInput : MonoBehaviour {

  private Vector3 accel;
  private Vector3 vel;

	// Use this for initialization
	void Start () {
    accel = new Vector3();
    vel = new Vector3();
	}
	
	// Update is called once per frame
	void Update () {
    this.transform.rotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
    accel = Input.acceleration;
    accel.Scale(new Vector3(-1, 1, 1)); // x is flipped in phone -> Unity axis
    accel += this.transform.up;
    vel += accel * Time.deltaTime;
    this.transform.position += vel * Time.deltaTime;
    //Debug.Log(string.Format("UNITYDEBUG: acc {0} {1} {2}", accel.x, accel.y, accel.z));
    //Debug.Log(string.Format("UNITYDEBUG: vel {0} {1} {2}", vel.x, vel.y, vel.z));
    //Debug.Log(string.Format("UNITYDEBUG: pos {0} {1} {2}", this.transform.position.x, this.transform.position.y, this.transform.position.z));

    // debug
    this.transform.position = vel;
  }
}
