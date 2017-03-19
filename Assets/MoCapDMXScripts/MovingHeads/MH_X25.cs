﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoCapDMXScripts.MovingHeads
{
    public class MH_X25 : MovingHead
    {
        public enum CHANNELMODE {
            CH12 = 12
        };

        public enum COLOR
        {
            WHITE = 3,
            YELLOW = 7,
            PINK = 12,
            GREEN = 17,
            PEACHBLOW = 22,
            LIGHTBLUE = 27,
            YELLOWGREEN = 32,
            RED = 37,
            DARKBLUE = 42
        };

        public enum GOBOTYPE
        {
            NOGOBO = 0,
            REVOLVERDRUM  = 12,
            CELLSTRUCTURE = 19,
            WHIRLWIND = 28,
            BIOHAZARD = 35,
            ROSE = 44,
            NET = 52,
            STARSHINE = 59
        };

        //Variables for initialization and information
        public int StartAddress { get; private set; }
        public int NumberOfChannels { get; private set; }
        public string Name { get; private set; }
        private int m_dmxDataOffset;
        private List<byte> m_dmxUDPPackage;


        //Variables for saiving current State of Movinghead
        public float fCurrentPanAngle { get; private set; }
        public float fCurrentTiltAngle { get; private set; }

        public uint CurrentPanValue { get; private set; }
        public uint CurrentTiltValue { get; private set; }
        public int CurrentPanAngle { get; private set; }
        public int CurrentTiltAngle { get; private set; }
        public uint CurrentDimmerValue { get; private set; }
        public uint CurrentShutterValue { get; private set; }
        public COLOR CurrentColor { get; private set; }
        public GOBOTYPE CurrentGoboType { get; private set; }

        public Vector3 Location { get; set; }
        public Vector3 NormalVector { get; set; }
        public Vector3 CurrentDirectionVector { get; set; }

        public static float MAXPAN = 540.0f;
        public static float MAXTILT = 270.0f;

        public float PanDegreePerDmxValue = 540.0f / 255.0f;
        public float TiltDegreePerDmxValue = 270.0f / 255.0f;
        public float PanDegreePerDmxValue_16Bit = 540.0f / 65536.0f;
        public float TiltDegreePerDmxValue_16Bit = 270.0f / 65536.0f;

        public MH_X25(int startAddress, CHANNELMODE channelmode, List<byte> dmxUDPPackage, int startIndexOfDMXData, string name = "") {
            StartAddress = startAddress;
            NumberOfChannels = (int)channelmode;
            m_dmxDataOffset = startIndexOfDMXData- 1;
            m_dmxUDPPackage = dmxUDPPackage;
            Name = name;
            CurrentPanValue = 0;
            CurrentTiltValue = 0;
        }

        public void Pan(uint dmxValue) {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot Pan because value has to be between 0 and 255.");
            }
            else {
                if (NumberOfChannels == (int)CHANNELMODE.CH12)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress] = (byte)dmxValue;
                    CurrentPanValue = dmxValue;
                }
            }

        }

        public void Pan(float angle) {
            if (NumberOfChannels == (int)CHANNELMODE.CH12) {
                UInt16 pan = (UInt16)(angle / PanDegreePerDmxValue_16Bit);

                m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 2] = (byte)(pan & 0xff);
                m_dmxUDPPackage[m_dmxDataOffset + StartAddress] = (byte)(0xff & (pan >> 8));
                fCurrentPanAngle = angle;
            }
        }

        public void Tilt(uint dmxValue) {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot Tilt because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH12)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 1] = (byte)dmxValue;
                    CurrentTiltValue = dmxValue;
                }
            }
        }

        public void Tilt(float angle) {
            if (NumberOfChannels == (int)CHANNELMODE.CH12)
            {
                UInt16 tilt = (UInt16)(angle / TiltDegreePerDmxValue_16Bit);

                m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 3] = (byte)(tilt & 0xff);
                m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 1] = (byte)(0xff & (tilt >> 8));
                fCurrentTiltAngle = angle;
            }
        }

        public void MasterDimmer(uint dmxValue) {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot Dimm because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH12)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 7] = (byte)dmxValue;
                    CurrentDimmerValue = dmxValue;
                }
            }
        }
        public void Shutter(uint dmxValue) {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot change Shutter because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH12)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 6] = (byte)dmxValue;
                    CurrentShutterValue = dmxValue;
                }
            }
        }

        public void Color(COLOR color)
        {
            if ((int)color < 0 || (int)color > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot change Shutter because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH12)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 5] = (byte)color;
                    CurrentColor = color;
                }
            }
        }

        public void GoboWheel(GOBOTYPE gobotype)
        {
            if ((int)gobotype < 0 || (int)gobotype > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot change Shutter because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH12)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 8] = (byte)gobotype;
                    CurrentGoboType = gobotype;
                }
            }
        }


        private void SetMaximas() {
            if (NumberOfChannels == (int)CHANNELMODE.CH12) {

            }
        }


        public void PointTo(Vector3 destPoint) {
            
            CurrentDirectionVector = (destPoint - Location);
            float panAngle = Vector2.Angle(new Vector2(this.NormalVector.x, this.NormalVector.z), new Vector2(this.CurrentDirectionVector.x, this.CurrentDirectionVector.z));
            panAngle = CalculateCoursAngle(panAngle);

            float result = (float)(Math.Acos(this.Location.y / (destPoint - Location).magnitude) * (180.0f / Math.PI));
            float tiltAngle = (MAXTILT/2) - result;

            this.Pan(panAngle);
            this.Tilt(tiltAngle);
        }

        private float CalculateCoursAngle(float angle)
        {
            int normXSign;
            int normZSign;
            if (this.NormalVector.x < 0) normXSign = -1;
            else if (this.NormalVector.x > 0) normXSign = 1;
            else normXSign = 0;
            if (this.NormalVector.z < 0) normZSign = -1;
            else if (this.NormalVector.z > 0) normZSign = 1;
            else normZSign = 0;

            if (normXSign == 0 && normZSign == 1)
            {
                if (this.Location.x <=  this.CurrentDirectionVector.x)
                {
                    return angle;
                }
                else
                {
                    return 360 - angle;
                }
            }
            else if (normXSign == -1 && normZSign == 0)
            {
                if (this.Location.z <= this.CurrentDirectionVector.z)
                {
                    return angle;
                }
                else
                {
                    return 360 - angle;
                }
            }
            else if (normXSign == 0 && normZSign == -1)
            {
                if (this.Location.x >= this.CurrentDirectionVector.x)
                {
                    return angle;
                }
                else
                {
                    return 360 - angle;
                }
            }
            else if (normXSign == 1 && normZSign == 0)
            {
                if (this.Location.z >= this.CurrentDirectionVector.z)
                {
                    return angle;
                }
                else
                {
                    return 360 - angle;
                }
            }
            else
            {
                return angle;
            }
        }

        //public void PointTo(Vector3 destPoint) {
        //    Vector3 tempCurrentForSignCheck = CurrentDirectionVector;

        //    CurrentDirectionVector = (destPoint - Location).normalized;

        //    Vector2 normalXZ = new Vector2(this.NormalVector.x, this.NormalVector.z);
        //    Vector2 currentXZ = new Vector2(this.CurrentDirectionVector.x, this.CurrentDirectionVector.z);
        //    float panAngle = Vector2.Angle(normalXZ,currentXZ);

        //    float result = (float)(Math.Acos(this.Location.y / (destPoint - Location).magnitude) * (180.0f/Math.PI));
        //    float tiltAngle = 135 - result;

        //    panAngle = HandlePanAngleResult(currentXZ, panAngle);
        //    this.Pan(panAngle);
        //    this.Tilt(tiltAngle);
        //}


        //private float HandlePanAngleResult(Vector2 XZ, float angle) {
        //    bool xSign = (XZ.x >= 0) ? true: false;
        //    bool zSign = (XZ.y >= 0) ? true : false;

        //    int normXSign; 
        //    int normZSign;
        //    if (this.NormalVector.x < 0) normXSign = -1;
        //    else if (this.NormalVector.x > 0) normXSign = 1;
        //    else normXSign = 0;

        //    if (this.NormalVector.z < 0) normZSign = -1;
        //    else if (this.NormalVector.z > 0) normZSign = 1;
        //    else normZSign = 0;

        //    if (normXSign == -1 && normZSign == 0)
        //    {
        //        if (xSign && zSign) return angle;
        //        else if (xSign && !zSign) return angle;
        //        else if (!xSign && !zSign) return 360 - angle;
        //        else return angle;//360 -angle;
        //    }
        //    else if (normXSign == 1 && normZSign == 0)
        //    {
        //        if (xSign && zSign) return 360 - angle;
        //        else if (xSign && !zSign) return angle;
        //        else if (!xSign && !zSign) return angle;
        //        else return angle;
        //    }
        //    else if (normXSign == 0 && normZSign == -1)
        //    {
        //        if (xSign && zSign) return angle;
        //        else if (xSign && !zSign) return 360 - angle;
        //        else if (!xSign && !zSign) return 360 - angle;
        //        else return angle;
        //    }
        //    else {
        //        if (xSign && zSign) return angle;
        //        else if (xSign && !zSign) return angle;
        //        else if (!xSign && !zSign) return 360- angle;
        //        else return 360 - angle;
        //    } 
        //}

        public override String ToString()
        {
            return "Movinghead: " + Name + "\t     |Startadress: " + StartAddress.ToString() + "\t|Channelmode: " + NumberOfChannels.ToString() + "\n";
        }
    }
}
