using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;
using MoCapDMXScripts.MovingHeads;
using MoCapDMXScripts;

public class MoCapDMX_MainIntelligence : MonoBehaviour {

    [SerializeField]public string DmxUdpDestIP;
    [SerializeField]public int DmxUdpPort;
    public UDPPackage.PROTOCOL_TYPE dmxProtocolType = UDPPackage.PROTOCOL_TYPE.ESP;

    public Canvas Console;
    public UnityEngine.UI.InputField channelInputField;
    public UnityEngine.UI.InputField valueInputField;

    private UdpClient udpClient;
    private UDPPackage dmxUDPPackage;
    private float dmxSendingInterval = 0.04f;
    private float period = 0.0f;

    private System.Diagnostics.Process natNetStreamerExecutable;

    private MH_X25 x25_1, x25_2, x25_3, x25_4, x25_5;
    private MH_PicoWash40 pico_1, pico_2, pico_3, pico_4, pico_5, pico_6, pico_7, pico_8, pico_9, pico_10;
    private List<MH_X25> x25ers;
    private List<MH_PicoWash40> picoWashers;

    private bool connectMovingHeadsToMoCap = false;
    private string ActorName;

    private int testCounter = 0;

    /// <summary>
    /// In this Awake Method the UnitySample.exe is executed for streaming data from MotiveServer to Unity3D
    /// </summary>
    void Awake() {
        

        natNetStreamerExecutable = new System.Diagnostics.Process();
        natNetStreamerExecutable.StartInfo.FileName = @"C:\Users\Daniel\Downloads\NatNet_SDK_2.10\NatNetSDK\Samples\bin\UnitySample.exe";
        natNetStreamerExecutable.StartInfo.Arguments = @"10.13.1.1 10.13.1.7 127.0.0.1 C:\MoCapDMXUnity\Assets\Resources MoCapTake.xml";
        natNetStreamerExecutable.StartInfo.UseShellExecute = false;
        natNetStreamerExecutable.Start();
        
    }


    /// <summary>
    /// In this ApplicationQuit Method the UnitySample.Exe is killed
    /// </summary>
    void OnApplicationQuit() {
        ResetOrInitializeMovingHeads();
        //Must be called here, because the Update Method isnt called anymore
        SendDmxUdpPackage();
        natNetStreamerExecutable.StandardInput.Write("q");
        //natNetStreamerExecutable.Kill();
    }

    GameObject dotty;


    // Use this for initialization
    void Start () {
        //for tests only 
        dotty = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dotty.transform.position = new Vector3(10, 10, 10);
        dotty.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        dotty.GetComponent<MeshRenderer>().material = Resources.Load("Black4", typeof(Material)) as Material;

        ActorName = this.GetComponent<MoCapDMXScripts.MoCapDataHandler>().Actor;

        Console.enabled = false;
        Console.gameObject.SetActive(false);

        //Initialize the DMX UDP Client
        InitializeUDPClient();
        //Initialize DMX UDP Package
        ResetOrInitializeMovingHeads();
        //Reset All movingheads on program start
        SendDmxUdpPackage();
        
        //Set Up Movingheads of H 1.5
        //SetUpMovingHeads();



        
    }



    // Update is called once per frame
    void Update() {

        if (period > dmxSendingInterval) {
            SendDmxUdpPackage();
            //Debug.Log("Dmx UDP sent after: " + period.ToString());
            period = 0;
        }
        period += UnityEngine.Time.deltaTime;

        //Test the point to method
        if (Input.GetKeyDown(KeyCode.P)) {
            Vector3 point = new Vector3(0,0,0);
            switch (testCounter) {
                case 0: {
                         point = new Vector3(0, 2, 0);
                        dotty.transform.position = point;
                    }
                    break;
                case 1:
                    {
                         point = new Vector3(1, 4, 1);
                        dotty.transform.position = point;
                    }
                    break;
                case 2:
                    {
                         point = new Vector3(1, 0, -3);
                        dotty.transform.position = point;
                    }
                    break;
                case 3:
                    {
                         point = new Vector3(-1, 3, -1);
                        dotty.transform.position = point;
                    }
                    break;
                case 4:
                    {
                         point = new Vector3(-2, 0, 1);
                        dotty.transform.position = point;
                        testCounter = -1;
                    }
                    break;
            }

            x25_1.PointTo(point);
            x25_2.PointTo(point);
            x25_3.PointTo(point);
            x25_4.PointTo(point);
            x25_5.PointTo(point);

            //VISUALIZATION OF X25s in UNITY
            MH_MainController scripty1 = GameObject.Find(x25_1.Name).GetComponent<MH_MainController>();
            scripty1.CurrentPan = x25_1.CurrentPanAngle;
            scripty1.CurrentTilt = x25_1.CurrentTiltAngle;

            MH_MainController scripty2 = GameObject.Find(x25_2.Name).GetComponent<MH_MainController>();
            scripty2.CurrentPan = x25_2.CurrentPanAngle;
            scripty2.CurrentTilt = x25_2.CurrentTiltAngle;

            MH_MainController scripty3 = GameObject.Find(x25_3.Name).GetComponent<MH_MainController>();
            scripty3.CurrentPan = x25_3.CurrentPanAngle;
            scripty3.CurrentTilt = x25_3.CurrentTiltAngle;

            MH_MainController scripty4 = GameObject.Find(x25_4.Name).GetComponent<MH_MainController>();
            scripty4.CurrentPan = x25_4.CurrentPanAngle;
            scripty4.CurrentTilt = x25_4.CurrentTiltAngle;

            MH_MainController scripty5 = GameObject.Find(x25_5.Name).GetComponent<MH_MainController>();
            scripty5.CurrentPan = x25_5.CurrentPanAngle;
            scripty5.CurrentTilt = x25_5.CurrentTiltAngle;

            

            testCounter++;
            
            //SendDmxUdpPackage();
        }
        
        //Test the moving heads
        if (Input.GetKey(KeyCode.I)) {
            SetAllMovingHeadsTo100PercentWhiteAndLuminance();
            //SendDmxUdpPackage();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetOrInitializeMovingHeads();
        }

        //Save Session to XML (session only records when the bool is set)
        if (Input.GetKeyDown(KeyCode.X)) {
            SlipStream.SaveXmlData(@"C:\Users\Daniel\Desktop\MoCapData.xml");
        }

        //Console Command
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1)) {
            if (Console.isActiveAndEnabled) {
                Console.enabled = false;
                Console.gameObject.SetActive(false);
            }
            else{
                Console.enabled = true;
                Console.gameObject.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift)) {
            connectMovingHeadsToMoCap = !connectMovingHeadsToMoCap;
            Debug.Log("Connection toggled to: " + connectMovingHeadsToMoCap);
        }
        if (connectMovingHeadsToMoCap)
        {
            ProcessMoCapDMX();
        }
	}


    void InitializeUDPClient() {
        udpClient = new UdpClient(DmxUdpDestIP,DmxUdpPort);
        udpClient.Connect(DmxUdpDestIP, DmxUdpPort);
    }

    public void SendCustomDMXData() {
        int channel = Convert.ToInt32(channelInputField.textComponent.text) - 1;
        int value = Convert.ToInt32(valueInputField.textComponent.text);

        dmxUDPPackage[dmxUDPPackage.StartIndexOfDMXData + (int)channel] = (byte)value;

        //SendDmxUdpPackage();
    }

    private void SetUpMovingHeads() {
        x25ers = new List<MH_X25>();
        x25_1 = new MH_X25(1, MH_X25.CHANNELMODE.CH12, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "X25_Middle");
        x25_2 = new MH_X25(13, MH_X25.CHANNELMODE.CH12, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "X25_DoorSideBack");
        x25_3 = new MH_X25(25, MH_X25.CHANNELMODE.CH12, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "X25_WindowSideBack");
        x25_4 = new MH_X25(37, MH_X25.CHANNELMODE.CH12, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "X25_WindowSideFront");
        x25_5 = new MH_X25(49, MH_X25.CHANNELMODE.CH12, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "X25_DoorSideFront");

        x25_1.NormalVector = new Vector3(0, 0.5f, 0.5f);
        x25_1.Location = new Vector3(-0.2450f, 2.2691f, -0.0214f);
        Debug.DrawRay(x25_1.Location, x25_1.NormalVector, UnityEngine.Color.red, 5.0f);
        x25_1.CurrentDirectionVector = x25_1.NormalVector;

        x25_2.NormalVector = new Vector3(-0.5f, 0.5f, 0);
        x25_2.Location = new Vector3(4.2234f, 2.2691f, 2.8780f);
        Debug.DrawRay(x25_2.Location, x25_2.NormalVector, UnityEngine.Color.red, 5.0f);
        x25_2.CurrentDirectionVector = x25_2.NormalVector;

        x25_3.NormalVector = new Vector3(0.5f, 0.5f, 0);
        x25_3.Location = new Vector3(-4.6666f, 2.2691f, 2.6870f);
        Debug.DrawRay(x25_3.Location, x25_3.NormalVector, UnityEngine.Color.red, 5.0f);
        x25_3.CurrentDirectionVector = x25_3.NormalVector;

        x25_4.NormalVector = new Vector3(0.5f, 0.5f, 0);
        x25_4.Location = new Vector3(-4.7965f, 2.2691f, -3.0309f);
        Debug.DrawRay(x25_4.Location, x25_4.NormalVector, UnityEngine.Color.red, 5.0f);
        x25_4.CurrentDirectionVector = x25_4.NormalVector;

        x25_5.NormalVector = new Vector3(-0.5f, 0.5f, 0);
        x25_5.Location = new Vector3(4.2196f, 2.2691f, -3.2928f);
        Debug.DrawRay(x25_5.Location, x25_5.NormalVector, UnityEngine.Color.red, 5.0f);
        x25_5.CurrentDirectionVector = x25_5.NormalVector;

        x25ers.Add(x25_1);
        x25ers.Add(x25_2);
        x25ers.Add(x25_3);
        x25ers.Add(x25_4);
        x25ers.Add(x25_5);

        picoWashers = new List<MH_PicoWash40>();
        pico_1 = new MH_PicoWash40(101, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_BackMiddle");
        pico_2 = new MH_PicoWash40(126, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_BackDoorSide");
        pico_3 = new MH_PicoWash40(151, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_DoorSideBack");
        pico_4 = new MH_PicoWash40(176, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_MiddleDoorSide");
        pico_5 = new MH_PicoWash40(201, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_MiddleWindowSide");
        pico_6 = new MH_PicoWash40(226, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_WindowSideBack");
        pico_7 = new MH_PicoWash40(251, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_WindowSideFront");
        pico_8 = new MH_PicoWash40(276, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_FrontWindowSide");
        pico_9 = new MH_PicoWash40(301, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_FrontDoorSide");
        pico_10 = new MH_PicoWash40(326, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "Pico40_DoorSideFront");

        picoWashers.Add(pico_1);
        picoWashers.Add(pico_2);
        picoWashers.Add(pico_3);
        picoWashers.Add(pico_4);
        picoWashers.Add(pico_5);
        picoWashers.Add(pico_6);
        picoWashers.Add(pico_7);
        picoWashers.Add(pico_8);
        picoWashers.Add(pico_9);
        picoWashers.Add(pico_10);

        UpdateVisualizationOfMovingheadsInUnityScene();
    }

    private void SetAllMovingHeadsTo100PercentWhiteAndLuminance() {
        foreach (MH_X25 mh in x25ers) {
            mh.MasterDimmer(255);
            mh.Shutter(4);
            mh.Color(MH_X25.COLOR.WHITE);
        }

        foreach (MH_PicoWash40 mh in picoWashers) {
            mh.AllColorLEDs(MH_PicoWash40.COLORCHANNEL.White, 255);
            mh.MasterDimmer(255);
            
        }
    }


    private void SendDmxUdpPackage() {
        udpClient.Send(dmxUDPPackage.ToArray(), dmxUDPPackage.Count);
    }

    private void ResetOrInitializeMovingHeads() {
        //dmxUDPPackage = new UDPPackage(dmxProtocolType);
        if (dmxUDPPackage == null)
        {
            dmxUDPPackage = new UDPPackage(dmxProtocolType);
        }
        else
        {
            for (int i = 0; i < 512; i++)
            {
                dmxUDPPackage[i + dmxUDPPackage.StartIndexOfDMXData] = (byte)0x00; // prefill dmx data with zeros
            }

        }

        SetUpMovingHeads();
        //SendDmxUdpPackage();
    }

    private void ProcessMoCapDMX() {
        MoCapBone Head = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_Head");
        MoCapBone Chest = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_Chest");

        if (Chest != null) {
            float yPosInCentiMeter = Chest.Position.y * 100;
            //if (yPosInCentiMeter <= 100.0f)
            //{
            //    this.GetComponent<AudioManipulator>().SetMasterVolume(-(100-yPosInCentiMeter)/4);
            //}
            //else {
            //    this.GetComponent<AudioManipulator>().SetMasterVolume(0.0f);
            //}
            if (yPosInCentiMeter <= 100.0f)
            {
                //Debug.Log("FREQ SET: " + ((100 - yPosInCentiMeter) * 4));
                this.GetComponent<AudioManipulator>().SetHighPassFilter((100 - yPosInCentiMeter) * 100);
            }
            else
            {
                this.GetComponent<AudioManipulator>().SetHighPassFilter(10.0f);
            }
        }

        if (Head != null) {
            Vector3 headPos = new Vector3(Head.Position.x, 0.0f, Head.Position.z);

            x25_1.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            x25_2.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            x25_3.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            x25_4.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            x25_5.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));

            //x25_1.PointTo(Head.Position);
            //x25_2.PointTo(Head.Position);
            //x25_3.PointTo(Head.Position);
            //x25_4.PointTo(Head.Position);
            //x25_5.PointTo(Head.Position);

            UpdateVisualizationOfMovingheadsInUnityScene();
        }

        MoCapBone LeftHand = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_LFArm");
        MoCapBone RightHand = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_RFArm");
        if (LeftHand != null && RightHand != null)
        {

            float pan = LeftHand.Rotation.eulerAngles.y / (540.0f / 255.0f);
            pico_10.Pan((uint)pan);
            pico_3.Pan((uint)pan);
            //x25_5.Pan((uint)pan);

            float tilt = LeftHand.Rotation.eulerAngles.z / (180.0f / 255.0f);
            float tiltx25 = LeftHand.Rotation.eulerAngles.z / (270.0f / 255.0f);
            pico_10.Tilt((uint)tilt);
            pico_3.Tilt((uint)tilt);
            //x25_5.Tilt((uint)tilt);

            float panRight = (RightHand.Rotation.eulerAngles.y + 180.0f) / (540.0f / 255.0f);
            pico_6.Pan((uint)(panRight));
            pico_7.Pan((uint)(panRight));
           // x25_4.Pan((uint)(panRight));

            float tiltRight = (RightHand.Rotation.eulerAngles.z - 180.0f) / (180.0f / 255.0f);
            float tiltRightx25 = (RightHand.Rotation.eulerAngles.z - 90.0f) / (270.0f / 255.0f);
            pico_6.Tilt((uint)(tiltRight));
            pico_7.Tilt((uint)(tiltRight));
            //x25_4.Tilt((uint)tiltRightx25);
            float distance = (Vector3.Distance(LeftHand.Position, RightHand.Position)) * 100.0f;
            //Debug.Log("Tilt Left und Tilt Right: " + tilt + "/" + tiltRight);
            if (distance < 30.0f)
            {
                pico_6.Strobo((uint)(255 - distance * 3));
                pico_7.Strobo((uint)(255 - distance * 3));
                pico_4.Strobo((uint)(255 - distance * 3));
                pico_10.Strobo((uint)(255 - distance * 3));
            }
            else
            {
                pico_4.Strobo(0);
                pico_6.Strobo(0);
                pico_7.Strobo(0);
                pico_10.Strobo(0);
            }
            //SendDmxUdpPackage();
        }
    }


    public void UpdateVisualizationOfMovingheadsInUnityScene() {
        MH_MainController scripty1 = GameObject.Find(x25_1.Name).GetComponent<MH_MainController>();
        scripty1.CurrentPan = x25_1.CurrentPanAngle;
        scripty1.CurrentTilt = x25_1.CurrentTiltAngle;

        MH_MainController scripty2 = GameObject.Find(x25_2.Name).GetComponent<MH_MainController>();
        scripty2.CurrentPan = x25_2.CurrentPanAngle;
        scripty2.CurrentTilt = x25_2.CurrentTiltAngle;

        MH_MainController scripty3 = GameObject.Find(x25_3.Name).GetComponent<MH_MainController>();
        scripty3.CurrentPan = x25_3.CurrentPanAngle;
        scripty3.CurrentTilt = x25_3.CurrentTiltAngle;

        MH_MainController scripty4 = GameObject.Find(x25_4.Name).GetComponent<MH_MainController>();
        scripty4.CurrentPan = x25_4.CurrentPanAngle;
        scripty4.CurrentTilt = x25_4.CurrentTiltAngle;

        MH_MainController scripty5 = GameObject.Find(x25_5.Name).GetComponent<MH_MainController>();
        scripty5.CurrentPan = x25_5.CurrentPanAngle;
        scripty5.CurrentTilt = x25_5.CurrentTiltAngle;
    }
}



