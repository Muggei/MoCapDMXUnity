using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Threading;
using UnityEngine;
using MoCapDMXScripts.MovingHeads;
using MoCapDMXScripts;
using MoCapDMXScripts.VirtualController;



public class MoCapDMX_MainIntelligence : MonoBehaviour {
    
    //Inspector Fields
    public string ServerIP;
    public string DeviceIP;
    public bool SaveSession;
    public string SaveFileName;

    [SerializeField]public string DmxUdpDestIP;
    [SerializeField]public int DmxUdpPort;
    public UDPPackage.PROTOCOL_TYPE dmxProtocolType = UDPPackage.PROTOCOL_TYPE.ESP;
    

    public Canvas Console;
    public UnityEngine.UI.InputField channelInputField;
    public UnityEngine.UI.InputField valueInputField;

    //UDP Client
    private UdpClient udpClient;
    private UDPPackage dmxUDPPackage;
    private float dmxSendingInterval = 0.022f;
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
        natNetStreamerExecutable.StartInfo.FileName = @"C:\MoCapDMXUnity\MoCapDMX_DataInput\Release\MoCapDMXDataInput.exe";
        if (SaveSession)
        {
            natNetStreamerExecutable.StartInfo.Arguments = ServerIP + " " + DeviceIP + @" 127.0.0.1 C:\MoCapDMXUnity\MoCapDMX\Assets\Resources " + SaveFileName + ".xml";
        }
        else {
            natNetStreamerExecutable.StartInfo.Arguments = ServerIP + " " + DeviceIP + @" 127.0.0.1";
        }
        
        natNetStreamerExecutable.StartInfo.UseShellExecute = false;
        natNetStreamerExecutable.Start();
        
    }


    /// <summary>
    /// In this ApplicationQuit Method the UnitySample.Exe is killed
    /// </summary>
    void OnApplicationQuit() {
        natNetStreamerExecutable.Kill();
        LogUtility.SaveLogFile();
        ResetOrInitializeMovingHeads();
        //Must be called here, because the Update Method isnt called anymore
        SendDmxUdpPackage();
        //natNetStreamerExecutable.StandardInput.WriteLine("q");
        
    }

    // Use this for initialization
    void Start () {
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

        LogUtility.StartLogTimer();



        
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.P))
        {
            MeasurePrecisionOfPointTo();
        }


        if (Input.GetKey(KeyCode.I))
        {
            SetAllMovingHeadsTo100PercentWhiteAndLuminance();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetOrInitializeMovingHeads();
        }

        //Console Command
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (Console.isActiveAndEnabled)
            {
                Console.enabled = false;
                Console.gameObject.SetActive(false);
            }
            else
            {
                Console.enabled = true;
                Console.gameObject.SetActive(true);
            }
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            SetUpVirtualControllers();
            if(LogUtility.performanceTesting) LogUtility.LogToFile("-------------------VirtualControllers have been initialized------------------------------------------");
        }

        if (Input.GetKeyUp(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift))
        {
            connectMovingHeadsToMoCap = !connectMovingHeadsToMoCap;
            if (LogUtility.performanceTesting) LogUtility.LogToFile("-------------------Connection to SlipStream toggled to: " + connectMovingHeadsToMoCap + "------------");
        }
    }

    // Update is called once per frame
    void FixedUpdate() {

        if (period > dmxSendingInterval)
        {
            SendDmxUdpPackage();
            UpdateVisualizationOfMovingheadsInUnityScene();
            period = 0;
        }
        period += UnityEngine.Time.deltaTime;

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
        x25_1.Location = new Vector3(-0.3000f, 2.2691f, -0.0614f);
        //Debug.DrawRay(x25_1.Location, x25_1.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_1.CurrentDirectionVector = x25_1.NormalVector;

        x25_2.NormalVector = new Vector3(-0.5f, 0.5f, 0);
        x25_2.Location = new Vector3(4.1234f, 2.2691f, 2.4780f);
        //Debug.DrawRay(x25_2.Location, x25_2.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_2.CurrentDirectionVector = x25_2.NormalVector;

        x25_3.NormalVector = new Vector3(0.5f, 0.5f, 0);
        x25_3.Location = new Vector3(-4.6666f, 2.2691f, 2.8870f);
        //Debug.DrawRay(x25_3.Location, x25_3.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_3.CurrentDirectionVector = x25_3.NormalVector;

        x25_4.NormalVector = new Vector3(0.5f, 0.5f, 0);
        x25_4.Location = new Vector3(-4.7965f, 2.2691f, -3.1109f);
        //Debug.DrawRay(x25_4.Location, x25_4.NormalVector, UnityEngine.Color.red, 25.0f);
        x25_4.CurrentDirectionVector = x25_4.NormalVector;

        x25_5.NormalVector = new Vector3(-0.5f, 0.5f, 0);
        x25_5.Location = new Vector3(3.9196f, 2.2691f, -3.1428f);
        //Debug.DrawRay(x25_5.Location, x25_5.NormalVector, UnityEngine.Color.red, 25.0f);
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

        pico_1.NormalVector = new Vector3(0, 0, -1);
        pico_1.Location = new Vector3(0.3065f, 2.3829f, 3.5248f);
        pico_1.CurrentDirectionVector = pico_1.NormalVector;

        pico_2.NormalVector = new Vector3(0, 0, -1);
        pico_2.Location = new Vector3(2.2389f, 2.3829f, 3.4911f);
        pico_2.CurrentDirectionVector = pico_2.NormalVector;

        pico_3.NormalVector = new Vector3(-1, 0, 0);
        pico_3.Location = new Vector3(4.2232f, 2.3829f, 0.5897f);
        pico_3.CurrentDirectionVector = pico_3.NormalVector;

        pico_4.NormalVector = new Vector3(0, 0, 1);
        pico_4.Location = new Vector3(1.2246f, 2.3829f, -0.1128f);
        pico_4.CurrentDirectionVector = pico_4.NormalVector;

        pico_5.NormalVector = new Vector3(0, 0, 1);
        pico_5.Location = new Vector3(-1.7048f, 2.3829f, 0.0658f);
        pico_5.CurrentDirectionVector = pico_5.NormalVector;

        pico_6.NormalVector = new Vector3(1, 0, 0);
        pico_6.Location = new Vector3(-4.6666f, 2.3829f, 0.9123f);
        pico_6.CurrentDirectionVector = pico_6.NormalVector;

        pico_7.NormalVector = new Vector3(1, 0, 0);
        pico_7.Location = new Vector3(-4.7965f, 2.3829f, -0.9604f);
        pico_7.CurrentDirectionVector = pico_7.NormalVector;

        pico_8.NormalVector = new Vector3(0, 0, 1);
        pico_8.Location = new Vector3(-2.2336f, 2.3829f, -4.0614f);
        pico_8.CurrentDirectionVector = pico_8.NormalVector;

        pico_9.NormalVector = new Vector3(0, 0, 1);
        pico_9.Location = new Vector3(2.0017f, 2.3829f, -4.2815f);
        pico_9.CurrentDirectionVector = pico_9.NormalVector;

        pico_10.NormalVector = new Vector3(-1, 0, 0);
        pico_10.Location = new Vector3(4.2196f, 2.3829f, -1.4949f);
        pico_10.CurrentDirectionVector = pico_10.NormalVector;

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

        InitVisualizationScripts();
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
        if (LogUtility.performanceTesting) LogUtility.LogToFile("DMX Data has been sent!");
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
    }

    private void SetUpVirtualControllers() {
        VirtualRandomEnumSwitchOverTimeByTwoBones goboSwitcher = new VirtualRandomEnumSwitchOverTimeByTwoBones("goboSwitcher",
            ActorName + "_Head", ActorName + "_RHand", new Action<MH_X25.GOBOTYPE>[] { x25_1.GoboWheel, x25_2.GoboWheel, x25_3.GoboWheel, x25_4.GoboWheel, x25_5.GoboWheel },
            ((head, rHand) => Vector3.Distance(head.PositionInCentimeter, rHand.PositionInCentimeter) <= 22.0f), 1000.0f,
            new MH_X25.GOBOTYPE[]{ MH_X25.GOBOTYPE.BIOHAZARD, MH_X25.GOBOTYPE.NOGOBO,MH_X25.GOBOTYPE.STARSHINE, MH_X25.GOBOTYPE.WHIRLWIND});

        VirtualRandomEnumColorSwitchOverTimeByTwoBones colorSwitcher = new VirtualRandomEnumColorSwitchOverTimeByTwoBones("colorSwicther",
            ActorName + "_Head", ActorName + "_RHand", new Action<MH_X25.COLOR>[] { x25_1.Color, x25_2.Color, x25_3.Color, x25_4.Color, x25_5.Color},
            ((head, rHand) => Vector3.Distance(head.PositionInCentimeter, rHand.PositionInCentimeter) <= 22.0f), 1000.0f,
            new MH_X25.COLOR[] { MH_X25.COLOR.RED, MH_X25.COLOR.WHITE, MH_X25.COLOR.YELLOW, MH_X25.COLOR.DARKBLUE });

        VirtualVoidFuncSwitchOverTimeByTwoBones audioController = new VirtualVoidFuncSwitchOverTimeByTwoBones("audioController",
            ActorName + "_LHand", ActorName + "_RHand", new Action[] { audioManipulator.StartOrPauseAudio },
            ((lHand, rHand) => lHand.Position.y > 1.8f && rHand.Position.y > 1.8f), 2000.0f);

        VirtualFaderByOneBone cutoffControl = new VirtualFaderByOneBone("cutoffFader", ActorName + "_Chest", 
            audioManipulator.SetHighPassFilter, (x => (100 - x.PositionInCentimeter.y) * 60), true,10.0f);

        VirtualToggleSwitchByOneBone stateOfCutoffController = new VirtualToggleSwitchByOneBone("CutoffControllerToggle", 
            ActorName + "_Chest", (x => x.PositionInCentimeter.y <= 100), cutoffControl);
    
        VirtualValueSwitchByTwoBones flasher = new VirtualValueSwitchByTwoBones("flasher",
            ActorName + "_LHand", ActorName + "_RHand", new Action<uint>[] { pico_1.MasterDimmer,pico_1.AllLEDsWhite,
                pico_9.MasterDimmer, pico_8.MasterDimmer,pico_2.MasterDimmer,pico_2.AllLEDsWhite, pico_9.AllLEDsWhite, pico_8.AllLEDsWhite, },
            ((r, l) => Vector3.Distance(r.PositionInCentimeter, l.PositionInCentimeter) <= 14.0f), 255, 0);

        VirtualValueSwitchByTwoBones strobMaster = new VirtualValueSwitchByTwoBones(
           "StrobeMaster", ActorName + "_RHand", ActorName + "_LFArm", new Action<uint>[] { pico_8.MasterDimmer, pico_8.AllLEDsWhite,
                pico_9.MasterDimmer, pico_9.AllLEDsWhite, pico_8.Strobo, pico_9.Strobo,
                pico_3.Strobo, pico_10.Strobo,pico_6.Strobo, pico_7.Strobo,pico_4.Strobo, pico_5.Strobo,
            x25_1.Shutter,x25_2.Shutter,x25_3.Shutter,x25_4.Shutter,x25_5.Shutter}
            , ((rHand, lFArm) => Vector3.Distance(rHand.PositionInCentimeter, lFArm.PositionInCentimeter) <= 13.0f), 200, 5);

        //VirtualFaderFromInitialValueWithMultipleTargetsUINT faderInitial = new VirtualFaderFromInitialValueWithMultipleTargetsUINT("fadeFromStartvalue", ActorName + "_LHand",
        //    new Action<uint>[] { pico_1.Strobo }, (x => (uint)x.PositionInCentimeter.y ), false);

        //VirtualToggleSwitchByTwoBones initSwitch = new VirtualToggleSwitchByTwoBones("initSwitcher", ActorName + "_LHand", ActorName + "_RHand",
        //    ((b1, b2) => Vector2.Distance(new Vector2(b1.Position.x,b1.Position.z), new Vector2(b2.Position.x,b2.Position.z)) * 100 <= 20), faderInitial);

        //VirtualToggleSwitchByOneBone colorFaderSwitch = new VirtualToggleSwitchByOneBone("colorFaderSwitch", ActorName + "_LFoot", (bone => bone.PositionInCentimeter.y < 20.0f), colorFader);

        VirtualTwoParameterFader_uint colorFader = new VirtualTwoParameterFader_uint(
    "colorFader", ActorName + "_LHand", ActorName + "_RHand",
    new Action<uint>[]{
                pico_5.AllLEDsGreen, pico_4.AllLEDsGreen,
                pico_5.MasterDimmer, pico_4.MasterDimmer,
            pico_3.AllLEDsBlue, pico_6.AllLEDsBlue, pico_7.AllLEDsBlue, pico_10.AllLEDsBlue,
                pico_3.MasterDimmer, pico_6.MasterDimmer, pico_7.MasterDimmer, pico_10.MasterDimmer,
            x25_1.MasterDimmer, x25_2.MasterDimmer, x25_3.MasterDimmer, x25_4.MasterDimmer, x25_5.MasterDimmer},
    ((one, two) => (uint)(255 - (Vector3.Distance(one.Position, two.Position) * 100))), false, 0);

        VirtualToggleSwitchOverTimeByTwoBoneParameters timerColorSwitch = new VirtualToggleSwitchOverTimeByTwoBoneParameters("timesColorFaderSwitch",
            ActorName + "_LShoulder", ActorName + "_RHand", ((one, two) => (Vector3.Distance(one.Position, two.Position) *100) < 20.0f), 1500.0f, colorFader);

        //x25_1.Shutter(5); x25_2.Shutter(5); x25_3.Shutter(5); x25_4.Shutter(5); x25_5.Shutter(5);
        VirtualPositionFaderWithMultipleTargets chaser = new VirtualPositionFaderWithMultipleTargets(
            "personChaser", ActorName + "_Chest", new Action<Vector3>[] {x25_1.PointTo ,x25_2.PointTo, x25_3.PointTo, x25_4.PointTo, x25_5.PointTo ,
                pico_1.PointTo, pico_2.PointTo,pico_4.PointTo, pico_5.PointTo, pico_8.PointTo, pico_9.PointTo},
            (bone => new Vector3(bone.Position.x,0,bone.Position.z)), true);

        VirtualToggleSwitchOverTimeByTwoBoneParameters chaserToggleSwitch = new VirtualToggleSwitchOverTimeByTwoBoneParameters("chaserToggleSwitch",
            ActorName + "_RShoulder", ActorName + "_LHand", ((hand, shoulder) => (float)(Vector3.Distance(hand.Position, shoulder.Position) * 100) < 20.0f), 1000.0f, chaser);

        //VirtualFaderByOneBone pico4_Tilt = new VirtualFaderByOneBone("pico4Tilt", ActorName + "_LUArm", pico_4.Tilt, (x => x.Rotation.eulerAngles.z), true);
        //VirtualFaderByOneBone pico5_Tilt = new VirtualFaderByOneBone("pico5Tilt", ActorName + "_RUArm", pico_5.Tilt, (x => 360- x.Rotation.eulerAngles.z), true);
        //VirtualFaderByOneBone pico8_Tilt = new VirtualFaderByOneBone("pico8Tilt", ActorName + "_RUArm", pico_8.Tilt, (x => 360 - x.Rotation.eulerAngles.z), true);
        //VirtualFaderByOneBone pico9_Tilt = new VirtualFaderByOneBone("pico9Tilt", ActorName + "_LUArm", pico_9.Tilt, (x => x.Rotation.eulerAngles.z), true);

        VirtualFaderByOneBone pico10_Pan = new VirtualFaderByOneBone("pico10Pan", ActorName + "_LFArm", pico_10.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualFaderByOneBone pico10_Tilt = new VirtualFaderByOneBone("pico10Tilt", ActorName + "_LFArm", pico_10.Tilt, (x => x.Rotation.eulerAngles.z), true);
        VirtualFaderByOneBone pico3_Pan = new VirtualFaderByOneBone("pico3Pan", ActorName + "_LFArm", pico_3.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualFaderByOneBone pico3_Tilt = new VirtualFaderByOneBone("pico3Tilt", ActorName + "_LFArm", pico_3.Tilt, (x => x.Rotation.eulerAngles.z), true);
        VirtualFaderByOneBone pico6_Pan = new VirtualFaderByOneBone("pico6Pan", ActorName + "_RFArm", pico_6.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualFaderByOneBone pico6_Tilt = new VirtualFaderByOneBone("pico6Tilt", ActorName + "_RFArm", pico_6.Tilt, (x => 360 - x.Rotation.eulerAngles.z), true);
        VirtualFaderByOneBone pico7_Pan = new VirtualFaderByOneBone("pico7Pan", ActorName + "_RFArm", pico_7.Pan, (x => x.Rotation.eulerAngles.y), true);
        VirtualFaderByOneBone pico7_Tilt = new VirtualFaderByOneBone("pico7Tilt", ActorName + "_RFArm", pico_7.Tilt, (x => 360 - x.Rotation.eulerAngles.z), true);
    }

    private void ProcessMoCapDMX() {
        VirtualControllerCollection.ExecuteAllControllers();
        }



    private MH_MainController scripty1;
    private MH_MainController scripty2;
    private MH_MainController scripty3;
    private MH_MainController scripty4;
    private MH_MainController scripty5;
    private MH_MainController pico1Script;
    private MH_MainController pico2Script;
    private MH_MainController pico3Script;
    private MH_MainController pico4Script;
    private MH_MainController pico5Script;
    private MH_MainController pico6Script;
    private MH_MainController pico7Script;
    private MH_MainController pico8Script;
    private MH_MainController pico9Script;
    private MH_MainController pico10Script;

    

    public void InitVisualizationScripts() {
        scripty1 = GameObject.Find(x25_1.Name).GetComponent<MH_MainController>();
        scripty2 = GameObject.Find(x25_2.Name).GetComponent<MH_MainController>();
        scripty3 = GameObject.Find(x25_3.Name).GetComponent<MH_MainController>();
        scripty4 = GameObject.Find(x25_4.Name).GetComponent<MH_MainController>();
        scripty5 = GameObject.Find(x25_5.Name).GetComponent<MH_MainController>();
        pico1Script = GameObject.Find(pico_1.Name).GetComponent<MH_MainController>();
        pico2Script = GameObject.Find(pico_2.Name).GetComponent<MH_MainController>();
        pico3Script = GameObject.Find(pico_3.Name).GetComponent<MH_MainController>();
        pico4Script = GameObject.Find(pico_4.Name).GetComponent<MH_MainController>();
        pico5Script = GameObject.Find(pico_5.Name).GetComponent<MH_MainController>();
        pico6Script = GameObject.Find(pico_6.Name).GetComponent<MH_MainController>();
        pico7Script = GameObject.Find(pico_7.Name).GetComponent<MH_MainController>();
        pico8Script = GameObject.Find(pico_8.Name).GetComponent<MH_MainController>();
        pico9Script = GameObject.Find(pico_9.Name).GetComponent<MH_MainController>();
        pico10Script = GameObject.Find(pico_10.Name).GetComponent<MH_MainController>();
    }

    public void UpdateVisualizationOfMovingheadsInUnityScene() {
        //MH_MainController scripty1 = GameObject.Find(x25_1.Name).GetComponent<MH_MainController>();
        scripty1.CurrentPan = x25_1.fCurrentPanAngle;
        scripty1.CurrentTilt = x25_1.fCurrentTiltAngle;

        //MH_MainController scripty2 = GameObject.Find(x25_2.Name).GetComponent<MH_MainController>();
        scripty2.CurrentPan = x25_2.fCurrentPanAngle;
        scripty2.CurrentTilt = x25_2.fCurrentTiltAngle;

        //MH_MainController scripty3 = GameObject.Find(x25_3.Name).GetComponent<MH_MainController>();
        scripty3.CurrentPan = x25_3.fCurrentPanAngle;
        scripty3.CurrentTilt = x25_3.fCurrentTiltAngle;

        //MH_MainController scripty4 = GameObject.Find(x25_4.Name).GetComponent<MH_MainController>();
        scripty4.CurrentPan = x25_4.fCurrentPanAngle;
        scripty4.CurrentTilt = x25_4.fCurrentTiltAngle;

        //MH_MainController scripty5 = GameObject.Find(x25_5.Name).GetComponent<MH_MainController>();
        scripty5.CurrentPan = x25_5.fCurrentPanAngle;
        scripty5.CurrentTilt = x25_5.fCurrentTiltAngle;

        //MH_MainController pico1Script = GameObject.Find(pico_1.Name).GetComponent<MH_MainController>();
        pico1Script.CurrentPan = pico_1.fCurrentPanAngle;
        pico1Script.CurrentTilt = pico_1.fCurrentTiltAngle;

        //MH_MainController pico2Script = GameObject.Find(pico_2.Name).GetComponent<MH_MainController>();
        pico2Script.CurrentPan = pico_2.fCurrentPanAngle;
        pico2Script.CurrentTilt = pico_2.fCurrentTiltAngle;

        //MH_MainController pico3Script = GameObject.Find(pico_3.Name).GetComponent<MH_MainController>();
        pico3Script.CurrentPan = pico_3.fCurrentPanAngle;
        pico3Script.CurrentTilt = pico_3.fCurrentTiltAngle;
        //MH_MainController pico4Script = GameObject.Find(pico_4.Name).GetComponent<MH_MainController>();
        pico4Script.CurrentPan = pico_4.fCurrentPanAngle;
        pico4Script.CurrentTilt = pico_4.fCurrentTiltAngle;
        //MH_MainController pico5Script = GameObject.Find(pico_5.Name).GetComponent<MH_MainController>();
        pico5Script.CurrentPan = pico_5.fCurrentPanAngle;
        pico5Script.CurrentTilt = pico_5.fCurrentTiltAngle;
        //MH_MainController pico6Script = GameObject.Find(pico_6.Name).GetComponent<MH_MainController>();
        pico6Script.CurrentPan = pico_6.fCurrentPanAngle;
        pico6Script.CurrentTilt = pico_6.fCurrentTiltAngle;
        //MH_MainController pico7Script = GameObject.Find(pico_7.Name).GetComponent<MH_MainController>();
        pico7Script.CurrentPan = pico_7.fCurrentPanAngle;
        pico7Script.CurrentTilt = pico_7.fCurrentTiltAngle;
        //MH_MainController pico8Script = GameObject.Find(pico_8.Name).GetComponent<MH_MainController>();
        pico8Script.CurrentPan = pico_8.fCurrentPanAngle;
        pico8Script.CurrentTilt = pico_8.fCurrentTiltAngle;
        //MH_MainController pico9Script = GameObject.Find(pico_9.Name).GetComponent<MH_MainController>();
        pico9Script.CurrentPan = pico_9.fCurrentPanAngle;
        pico9Script.CurrentTilt = pico_9.fCurrentTiltAngle;
        //MH_MainController pico10Script = GameObject.Find(pico_10.Name).GetComponent<MH_MainController>();
        pico10Script.CurrentPan = pico_10.fCurrentPanAngle;
        pico10Script.CurrentTilt = pico_10.fCurrentTiltAngle;
    }

    //For Precision Measurement
    public void MeasurePrecisionOfPointTo() {
        foreach (MH_X25 mh in x25ers) {
            mh.MasterDimmer(255);
            mh.Shutter(4);
            mh.Color(MH_X25.COLOR.WHITE);
            mh.GoboWheel(MH_X25.GOBOTYPE.BIOHAZARD);
        }

        Vector3 dest = new Vector3(0,0,0);
        switch (testCounter) {
            case 0: {
                    dest = new Vector3(0, 0, 0);
                } break;
            case 1: {
                    dest = new Vector3(1, 0, 1);
                } break;
            case 2: {
                    dest = new Vector3(1, 0, -1);
                } break;
            case 3: {
                    dest = new Vector3(-1, 0, -1);
                } break;
            case 4: {
                    dest = new Vector3(-1, 0,1 );
                    testCounter = -1;
                } break;
        }

        foreach (MH_X25 mh in x25ers)
        {
            mh.PointTo(dest);
        }
        testCounter++;
    }
}



