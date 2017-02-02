using UnityEngine;
using System.Collections;
//using AssemblyCSharp;

public class MH_MainController : MonoBehaviour {

	[SerializeField] public GameObject MH_RotationBase;
	[SerializeField] public GameObject MH_Head;
	[SerializeField] public Light MH_Lightsource;

    public string Name;

    public int MaxPan;
    public int MaxTilt;

    public int CurrentPan;
    public int CurrentTilt;
    private int m_Pan;
    private int m_Tilt;

    public Vector2 InitialRotationsPanTilt;
    public Vector3 CurrentVector;
    private Color currentLightColor;

	// Use this for initialization
	void Start () {
        CurrentTilt = (int)InitialRotationsPanTilt.y;
        CurrentPan = (int)InitialRotationsPanTilt.x;
        m_Pan = CurrentPan;
        m_Tilt = CurrentTilt;
		this.MH_RotationBase = MH_RotationBase.gameObject;
		this.MH_Head = MH_Head.gameObject;
		this.currentLightColor = MH_Lightsource.color;
        this.MH_RotationBase.transform.Rotate(new Vector3(0,  CurrentPan, 0));
        this.MH_Head.transform.Rotate(new Vector3(0, 0,  135-CurrentTilt));

        //MH_RotationBase.transform.rotation = new Quaternion (0, 0, 0, 0);
        //MH_Head.transform.rotation = new Quaternion (0, 0, 0, 0);
    }

	// Update is called once per frame
	void FixedUpdate () {
		//if (Input.GetKey (KeyCode.LeftArrow)) {
		//	RotateHorizontal(new Vector3 (0.0f, -0.3f, 0.0f));
		//}
		//if (Input.GetKey (KeyCode.RightArrow)) {
		//	RotateHorizontal (new Vector3 (0.0f, 0.3f, 0.0f));
		//}
		//if (Input.GetKey (KeyCode.UpArrow)) {
		//	RotateVertical(new Vector3 (0.0f, 0.0f, -0.3f));
		//}
		//if (Input.GetKey (KeyCode.DownArrow)) {
		//	RotateVertical (new Vector3 (0.0f, 0.0f, 0.3f));
		//}
		//if (Input.GetKeyUp (KeyCode.C)) {
		//	MH_Lightsource.color = SwitchRGBChannelsWithFullIlluminance ();
		//}
	}

    void Update() {
        
        if (m_Pan != CurrentPan)
        {
            //CurrentPan = 0 - CurrentPan;
            this.MH_RotationBase.transform.Rotate(new Vector3(0, m_Pan - CurrentPan, 0));
            m_Pan = CurrentPan;
        }
        if (m_Tilt != CurrentTilt)
        {
            this.MH_Head.transform.Rotate(new Vector3(0, 0, m_Tilt -CurrentTilt));
            m_Tilt = CurrentTilt;
        }

        
        //this.MH_RotationBase.transform.eulerAngles.Set(0, CurrentPan, 0);
        //this.MH_RotationBase.transform.eulerAngles.Set(0, 0, 135 - CurrentTilt);
        //Debug.Log("Current Pan/m_pan: " + CurrentPan + "/" + m_Pan);
        
    }

	private Color SwitchRGBChannelsWithFullIlluminance(){

		if(currentLightColor == Color.white)
		{
			currentLightColor = Color.red;
		}
		else if(currentLightColor == Color.red)
		{
			currentLightColor =  Color.green;
		}
		else if(currentLightColor == Color.green)
		{
			currentLightColor = Color.blue;
		}
		else if(currentLightColor ==Color.blue)
		{
			currentLightColor =  Color.white;
		}
		else{
			currentLightColor =  Color.white;
		}

		return currentLightColor;
	}



	public void RotateVertical(Vector3 rotation){
		MH_Head.transform.Rotate (rotation);
	}

	public void RotateHorizontal(Vector3 rotation){
		MH_RotationBase.transform.Rotate (rotation);
	}
}

