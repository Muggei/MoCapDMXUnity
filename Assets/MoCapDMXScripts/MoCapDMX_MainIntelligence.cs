using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;
using MoCapDMXScripts.MovingHeads;
using MoCapDMXScripts;
using MoCapDMXScripts.VirtualController;

public class MoCapDMX_MainIntelligence : MonoBehaviour {

    //Inspector Fields
    [SerializeField]public string DmxUdpDestIP;
    [SerializeField]public int DmxUdpPort;
    public UDPPackage.PROTOCOL_TYPE dmxProtocolType = UDPPackage.PROTOCOL_TYPE.ESP;

    public Canvas Console;
    public UnityEngine.UI.InputField channelInputField;
    public UnityEngine.UI.InputField valueInputField;

    //UDP Client
    private UdpClient udpClient;
    private UDPPackage dmxUDPPackage;
    private float dmxSendingInterval = 0.03f;
    private float period = 0.0f;

    //UnityStreamer Excecutable (C++)
    private System.Diagnostics.Process natNetStreamerExecutable;

    //Movinghead instances
    private MH_X25 x25_1, x25_2, x25_3, x25_4, x25_5;
    private MH_PicoWash40 pico_1, pico_2, pico_3, pico_4, pico_5, pico_6, pico_7, pico_8, pico_9, pico_10;
    private List<MH_X25> x25ers;
    private List<MH_PicoWash40> picoWashers;

    //Global AudioManipulator
    private AudioManipulator audioManipulator;

    private bool connectMovingHeadsToMoCap = false;
    private string ActorName;

    private int testCounter = 0;

    /// <summary>
    /// In this Awake Method the UnitySample.exe is executed for streaming data from MotiveServer to Unity3D
    /// </summary>
    void Awake() {
        

        natNetStreamerExecutable = new System.Diagnostics.Process();
        natNetStreamerExecutable.StartInfo.FileName = @"C:\Users\Daniel\Downloads\NatNet_SDK_2.10\NatNetSDK\Samples\bin\UnitySample.exe";
        //natNetStreamerExecutable.StartInfo.Arguments = @"10.13.1.1 10.13.1.3 127.0.0.1 C:\MoCapDMXUnity\Assets\Resources MoCapTake.xml";
        natNetStreamerExecutable.StartInfo.Arguments = @"10.13.1.1 10.13.1.3 127.0.0.1";
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
        //natNetStreamerExecutable.StandardInput.Write("q");
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
        audioManipulator = this.GetComponent<AudioManipulator>();

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
            UpdateVisualizationOfMovingheadsInUnityScene();
            period = 0;
        }
        period += UnityEngine.Time.deltaTime;

        //Test the point to method
        //if (Input.GetKeyDown(KeyCode.P)) {
        //    Vector3 point = new Vector3(0,0,0);
        //    switch (testCounter) {
        //        case 0: {
        //                point = new Vector3(x25_1.Location.x, 0, x25_1.Location.z);// new Vector3(0, 0, 0);
        //                dotty.transform.position = point;
        //            }
        //            break;
        //        case 1:
        //            {
        //                point = new Vector3(0, 2, 0);
        //                dotty.transform.position = point;
        //            }
        //            break;
        //        case 2:
        //            {
        //                 point = new Vector3(2, 0, 2);
        //                dotty.transform.position = point;
        //            }
        //            break;
        //        case 3:
        //            {
        //                 point = new Vector3(1, 0, -1);
        //                dotty.transform.position = point;
        //            }
        //            break;
        //        case 4:
        //            {
        //                 point = new Vector3(-3, 2, -3);
        //                dotty.transform.position = point;
        //            }
        //            break;
        //        case 5:
        //            {
        //                 point = new Vector3(-2.5f, 0, 2.5f);
        //                dotty.transform.position = point;
        //                testCounter = -1;
        //            }
        //            break;
        //    }

        //    x25_1.PointTo(point);
        //    x25_2.PointTo(point);
        //    x25_3.PointTo(point);
        //    x25_4.PointTo(point);
        //    x25_5.PointTo(point);
        //    testCounter++;
        //}
        
        //Test the moving heads
        if (Input.GetKey(KeyCode.I)) {
            SetAllMovingHeadsTo100PercentWhiteAndLuminance();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetOrInitializeMovingHeads();
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
        if (Input.GetKeyUp(KeyCode.F)) {
            SetUpVirtualControllers();
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
        Debug.DrawRay(x25_1.Location, x25_1.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_1.CurrentDirectionVector = x25_1.NormalVector;

        x25_2.NormalVector = new Vector3(-0.5f, 0.5f, 0);
        x25_2.Location = new Vector3(4.2234f, 2.2691f, 2.8780f);
        Debug.DrawRay(x25_2.Location, x25_2.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_2.CurrentDirectionVector = x25_2.NormalVector;

        x25_3.NormalVector = new Vector3(0.5f, 0.5f, 0);
        x25_3.Location = new Vector3(-4.6666f, 2.2691f, 2.6870f);
        Debug.DrawRay(x25_3.Location, x25_3.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_3.CurrentDirectionVector = x25_3.NormalVector;

        x25_4.NormalVector = new Vector3(0.5f, 0.5f, 0);
        x25_4.Location = new Vector3(-4.7965f, 2.2691f, -3.0309f);
        Debug.DrawRay(x25_4.Location, x25_4.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_4.CurrentDirectionVector = x25_4.NormalVector;

        x25_5.NormalVector = new Vector3(-0.5f, 0.5f, 0);
        x25_5.Location = new Vector3(4.2196f, 2.2691f, -3.2928f);
        Debug.DrawRay(x25_5.Location, x25_5.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_5.CurrentDirectionVector = x25_5.NormalVector;

        x25ers.Add(x25_1);
        x25ers.Add(x25_2);
        x25ers.Add(x25_3);
        x25ers.Add(x25_4);
        x25ers.Add(x25_5);

        picoWashers = new List<MH_PicoWash40>();
        pico_1 = new MH_PicoWash40(101, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_BackMiddle");
        pico_2 = new MH_PicoWash40(126, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_BackDoorSide");
        pico_3 = new MH_PicoWash40(151, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_DoorSideBack");
        pico_4 = new MH_PicoWash40(176, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_MiddleDoorSide");
        pico_5 = new MH_PicoWash40(201, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_MiddleWindowSide");
        pico_6 = new MH_PicoWash40(226, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_WindowSideBack");
        pico_7 = new MH_PicoWash40(251, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_WindowSideFront");
        pico_8 = new MH_PicoWash40(276, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_FrontWindowSide");
        pico_9 = new MH_PicoWash40(301, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_FrontDoorSide");
        pico_10 = new MH_PicoWash40(326, MH_PicoWash40.CHANNELMODE.CH25, dmxUDPPackage, dmxUDPPackage.StartIndexOfDMXData, "PicoWash40_DoorSideFront");

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

    private void SetUpVirtualControllers() {
        //VirtualOneParameterFader fader1 = new VirtualOneParameterFader(ActorName + "_LFArm", pico_10.Pan, VirtualOneParameterFaderBoneParameter.YRotation, true,ManipulationType.Add,180.0f);
        //VirtualOneParameterFader faderLam = new VirtualOneParameterFader(ActorName + "_LFArm", pico_10.Pan, VirtualOneParameterFaderBoneParameter.YRotation, true, ManipulationType.Add, 180.0f);
        //VirtualOneParameterFader fader2 = new VirtualOneParameterFader(ActorName + "_LFArm", pico_10.Tilt, VirtualOneParameterFaderBoneParameter.ZRotation, true);
        VirtualSingleParameterFader cutoffControl = new VirtualSingleParameterFader("cutoffFader", ActorName + "_Chest", audioManipulator.SetHighPassFilter, (x => (100 - x.PositionInCentimeter.y) * 100), true,10.0f);
        VirtualToggleSwitchByOneBone stateOfVolumeContro = new VirtualToggleSwitchByOneBone("VolumeControlToggle", ActorName + "_Chest", (x => x.PositionInCentimeter.y <= 100), cutoffControl);

        VirtualTwoParameterFaderWithMultipleTargetsUINT stroboEffectFader = new VirtualTwoParameterFaderWithMultipleTargetsUINT(
            "stroboFader", ActorName + "_LHand", ActorName + "_RHand", new Action<uint>[]{ pico_9.Strobo, pico_8.Strobo, pico_5.Strobo, pico_4.Strobo},
            ((one,two) => (uint)(255 - (Vector3.Distance(one.Position, two.Position) * 100))) ,true,0);



        VirtualSingleParameterFader pico10_Pan = new VirtualSingleParameterFader("pico10Pan", ActorName + "_LFArm", pico_10.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualSingleParameterFader pico10_Tilt = new VirtualSingleParameterFader("pico10Tilt", ActorName + "_LFArm", pico_10.Tilt, (x => x.Rotation.eulerAngles.z), true);
        VirtualSingleParameterFader pico3_Pan = new VirtualSingleParameterFader("pico3Pan", ActorName + "_LFArm", pico_3.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualSingleParameterFader pico3_Tilt = new VirtualSingleParameterFader("pico3Tilt", ActorName + "_LFArm", pico_3.Tilt, (x => x.Rotation.eulerAngles.z), true);
        VirtualSingleParameterFader pico6_Pan = new VirtualSingleParameterFader("pico6Pan", ActorName + "_RFArm", pico_6.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualSingleParameterFader pico6_Tilt = new VirtualSingleParameterFader("pico6Tilt", ActorName + "_RFArm", pico_6.Tilt, (x => 360 - x.Rotation.eulerAngles.z), true);
        VirtualSingleParameterFader pico7_Pan = new VirtualSingleParameterFader("pico7Pan", ActorName + "_RFArm", pico_7.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualSingleParameterFader pico7_Tilt = new VirtualSingleParameterFader("pico7Tilt", ActorName + "_RFArm", pico_7.Tilt, (x => 360 - x.Rotation.eulerAngles.z), true);
    }

    private void ProcessMoCapDMX() {
        VirtualControllerCollection.ExecuteAllControllers();


        //MarkerFunctionalityLink Link1 = new MarkerFunctionalityLink(ActorName + "_Head", x25_1.Pan, true);
        //MarkerFunctionalityLink Link2 = new MarkerFunctionalityLink(ActorName + "_Head", x25_2.Pan, true);
        //MarkerFunctionalityLink Link3 = new MarkerFunctionalityLink(ActorName + "_Head", x25_3.Pan, true);
        //MarkerFunctionalityLink Link4 = new MarkerFunctionalityLink(ActorName + "_Head", x25_4.Pan, true);
        //MarkerFunctionalityLink Link5 = new MarkerFunctionalityLink(ActorName + "_Head", x25_5.Pan, true);
        //MarkerFunctionalityLink Link6 = new MarkerFunctionalityLink(ActorName + "_Head", pico_1.Pan, true);
        //MarkerFunctionalityLink Link7 = new MarkerFunctionalityLink(ActorName + "_Head", pico_2.Pan, true);
        //MarkerFunctionalityLink Link8 = new MarkerFunctionalityLink(ActorName + "_LUArm", pico_1.Tilt, true);
        //MarkerFunctionalityLink Link9 = new MarkerFunctionalityLink(ActorName + "_RUArm", pico_2.Tilt, true);

        //foreach (MarkerFunctionalityLink link in GlobalLinkerCollection.Instance)
        //{
        //    link.Excecute();
        //}

        MoCapBone Head = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_Head");
        //MoCapBone Chest = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_Chest");

        //if (Chest != null) {
        //    float yPosInCentiMeter = Chest.Position.y * 100;
        //    //if (yPosInCentiMeter <= 100.0f)
        //    //{
        //    //    this.GetComponent<AudioManipulator>().SetMasterVolume(-(100-yPosInCentiMeter)/4);
        //    //}
        //    //else {
        //    //    this.GetComponent<AudioManipulator>().SetMasterVolume(0.0f);
        //    //}
        //    if (yPosInCentiMeter <= 100.0f)
        //    {
        //        //Debug.Log("FREQ SET: " + ((100 - yPosInCentiMeter) * 4));
        //        this.GetComponent<AudioManipulator>().SetHighPassFilter((100 - yPosInCentiMeter) * 100);
        //    }
        //    else
        //    {
        //        this.GetComponent<AudioManipulator>().SetHighPassFilter(10.0f);
        //    }
        //}

        if (Head != null)
        {
            //Vector3 headPos = new Vector3(Head.Position.x, 0.0f, Head.Position.z);

            //x25_1.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            //x25_2.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            //x25_3.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            //x25_4.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));
            //x25_5.PointTo(new Vector3(Head.Position.x, 0.0f, Head.Position.z));

            //    x25_1.PointTo(Head.Position);
            //    x25_2.PointTo(Head.Position);
            //    x25_3.PointTo(Head.Position);
            //    x25_4.PointTo(Head.Position);
            //    x25_5.PointTo(Head.Position);


        }

        MoCapBone LeftHand = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_LFArm");
        MoCapBone RightHand = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_RFArm");
        MoCapBone Chest = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == ActorName + "_Chest");
        if (LeftHand != null && RightHand != null)
        {
           
            //    float distance = (Vector3.Distance(LeftHand.Position, RightHand.Position)) * 100.0f;
            //    if (distance < 30.0f)
            //    {
            //        pico_6.Strobo((uint)(255 - distance * 3));
            //        pico_7.Strobo((uint)(255 - distance * 3));
            //        pico_4.Strobo((uint)(255 - distance * 3));
            //        pico_10.Strobo((uint)(255 - distance * 3));
            //    }
            //    else
            //    {
            //        pico_4.Strobo(0);
            //        pico_6.Strobo(0);
            //        pico_7.Strobo(0);
            //        pico_10.Strobo(0);
            //    }
        }
        }


    public void UpdateVisualizationOfMovingheadsInUnityScene() {
        MH_MainController scripty1 = GameObject.Find(x25_1.Name).GetComponent<MH_MainController>();
        scripty1.CurrentPan = x25_1.fCurrentPanAngle;
        scripty1.CurrentTilt = x25_1.fCurrentTiltAngle;

        MH_MainController scripty2 = GameObject.Find(x25_2.Name).GetComponent<MH_MainController>();
        scripty2.CurrentPan = x25_2.fCurrentPanAngle;
        scripty2.CurrentTilt = x25_2.fCurrentTiltAngle;

        MH_MainController scripty3 = GameObject.Find(x25_3.Name).GetComponent<MH_MainController>();
        scripty3.CurrentPan = x25_3.fCurrentPanAngle;
        scripty3.CurrentTilt = x25_3.fCurrentTiltAngle;

        MH_MainController scripty4 = GameObject.Find(x25_4.Name).GetComponent<MH_MainController>();
        scripty4.CurrentPan = x25_4.fCurrentPanAngle;
        scripty4.CurrentTilt = x25_4.fCurrentTiltAngle;

        MH_MainController scripty5 = GameObject.Find(x25_5.Name).GetComponent<MH_MainController>();
        scripty5.CurrentPan = x25_5.fCurrentPanAngle;
        scripty5.CurrentTilt = x25_5.fCurrentTiltAngle;

        MH_MainController pico1Script = GameObject.Find(pico_1.Name).GetComponent<MH_MainController>();
        pico1Script.CurrentPan = pico_1.fCurrentPanAngle;
        pico1Script.CurrentTilt = pico_1.fCurrentTiltAngle;

        MH_MainController pico2Script = GameObject.Find(pico_2.Name).GetComponent<MH_MainController>();
        pico2Script.CurrentPan = pico_2.fCurrentPanAngle;
        pico2Script.CurrentTilt = pico_2.fCurrentTiltAngle;

        MH_MainController pico3Script = GameObject.Find(pico_3.Name).GetComponent<MH_MainController>();
        pico3Script.CurrentPan = pico_3.fCurrentPanAngle;
        pico3Script.CurrentTilt = pico_3.fCurrentTiltAngle;
        MH_MainController pico4Script = GameObject.Find(pico_4.Name).GetComponent<MH_MainController>();
        pico4Script.CurrentPan = pico_4.fCurrentPanAngle;
        pico4Script.CurrentTilt = pico_4.fCurrentTiltAngle;
        MH_MainController pico5Script = GameObject.Find(pico_5.Name).GetComponent<MH_MainController>();
        pico5Script.CurrentPan = pico_5.fCurrentPanAngle;
        pico5Script.CurrentTilt = pico_5.fCurrentTiltAngle;
        MH_MainController pico6Script = GameObject.Find(pico_6.Name).GetComponent<MH_MainController>();
        pico6Script.CurrentPan = pico_6.fCurrentPanAngle;
        pico6Script.CurrentTilt = pico_6.fCurrentTiltAngle;
        MH_MainController pico7Script = GameObject.Find(pico_7.Name).GetComponent<MH_MainController>();
        pico7Script.CurrentPan = pico_7.fCurrentPanAngle;
        pico7Script.CurrentTilt = pico_7.fCurrentTiltAngle;
        MH_MainController pico8Script = GameObject.Find(pico_8.Name).GetComponent<MH_MainController>();
        pico8Script.CurrentPan = pico_8.fCurrentPanAngle;
        pico8Script.CurrentTilt = pico_8.fCurrentTiltAngle;
        MH_MainController pico9Script = GameObject.Find(pico_9.Name).GetComponent<MH_MainController>();
        pico9Script.CurrentPan = pico_9.fCurrentPanAngle;
        pico9Script.CurrentTilt = pico_9.fCurrentTiltAngle;
        MH_MainController pico10Script = GameObject.Find(pico_10.Name).GetComponent<MH_MainController>();
        pico10Script.CurrentPan = pico_10.fCurrentPanAngle;
        pico10Script.CurrentTilt = pico_10.fCurrentTiltAngle;
    }
}



