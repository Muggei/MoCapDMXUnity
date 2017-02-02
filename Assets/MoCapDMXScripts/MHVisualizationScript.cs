using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MHVisualizationScript : MonoBehaviour {

    public GameObject RotationBase;
    public GameObject TiltingBase;

    public string Name;

    public int MaxPan;
    public int MaxTilt;

    public Vector3 CurrentVector;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        SetPanAndTilt();
	}

    void SetPanAndTilt() {
        Vector3.Angle(this.TiltingBase.transform.rotation.eulerAngles, CurrentVector);

        float pan = Vector2.Angle(
            new Vector2(this.TiltingBase.transform.rotation.x, this.TiltingBase.transform.rotation.z),
            new Vector2(CurrentVector.x, CurrentVector.z));

        this.RotationBase.transform.Rotate(new Vector3(0, pan, 0));
    }
}
