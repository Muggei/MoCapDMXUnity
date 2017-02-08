using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (Input.GetKey(KeyCode.LeftArrow)) {
            this.transform.RotateAround(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 2.0f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            this.transform.RotateAround(new Vector3(0, 0, 0), new Vector3(0, -1, 0), 2.0f);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            this.transform.Rotate(1,0, 0, Space.Self);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            this.transform.Rotate(-1, 0, 0, Space.Self);
        }
        if (Input.GetKey(KeyCode.KeypadMinus)) {
            this.transform.Translate(0, 0, -0.25f, Space.Self);
        }
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            this.transform.Translate(0, 0, 0.25f, Space.Self);
        }
    }
}
