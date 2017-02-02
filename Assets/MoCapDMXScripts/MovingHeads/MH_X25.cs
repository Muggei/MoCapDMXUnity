using System;
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
            REVOLVERMAGAZIN = 12,
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

        public float PanDegreePerDmxValue = 540.0f / 255.0f;
        public float TiltDegreePerDmxValue = 270.0f / 255.0f;

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
            Vector3 tempCurrentForSignCheck = CurrentDirectionVector;

            CurrentDirectionVector = (destPoint - Location).normalized;

            Vector2 normalXZ = new Vector2(this.NormalVector.x, this.NormalVector.z);
            Vector2 currentXZ = new Vector2(this.CurrentDirectionVector.x, this.CurrentDirectionVector.z);
            float panAngle = Vector2.Angle(normalXZ,currentXZ);

            Vector3 normalTilt = new Vector3(CurrentDirectionVector.x, NormalVector.y, CurrentDirectionVector.z);
            Vector3 currentTilt = new Vector3(CurrentDirectionVector.x,destPoint.y,CurrentDirectionVector.z);
            float tiltAngle = Vector3.Angle(normalTilt, currentTilt);// +45;

            panAngle = HandlePanAngleResult(currentXZ, panAngle);
            

            
            //Debug.DrawRay(Location, normalTilt ,UnityEngine.Color.blue,5.0f);
            //Debug.DrawRay(Location, currentTilt,UnityEngine.Color.blue, 5.0f);

            CurrentPanAngle = (int)panAngle;
            CurrentTiltAngle = 135-(int)tiltAngle;

            Debug.Log(this.ToString()+ ": pan, tilt: " + CurrentPanAngle + "/" + CurrentTiltAngle  );

            //TODO: Implement 16Bit Version of Panning and Tilting
            this.Pan((uint)((uint)CurrentPanAngle / PanDegreePerDmxValue));
            this.Tilt((uint)((uint)CurrentTiltAngle / TiltDegreePerDmxValue));

            //float angleX = Vector3.Angle(new Vector3(this.Location.x, 0, 0), new Vector3(destPoint.x, 0, 0));
            //float angleY = Vector3.Angle(new Vector3(0, this.Location.y, 0), new Vector3(0, destPoint.y, 0));
            //float angleZ = Vector3.Angle(new Vector3(0, 0, this.Location.z), new Vector3(0, 0, destPoint.z));
            //float angleXZ = Vector3.Angle(new Vector3(this.Location.x, 0, this.Location.z).normalized, new Vector3(destPoint.x, 0, destPoint.z).normalized);

            //this.Pan((uint)(angleX / (540.0f / 255.0f)));
            //this.Tilt((uint)(angleY / (270.0f / 255.0f)));

            //Vector2 xzDest = new Vector2(destPoint.x, destPoint.z).normalized;
            //Vector2 xzSrc = new Vector2(this.Location.x, this.Location.z).normalized;

            //Vector2 xyDest = (new Vector2(destPoint.x, destPoint.y)).normalized;
            //Vector2 xySrc = (new Vector2(this.Location.x, this.Location.y)).normalized;

            //float pan = Vector2.Angle(xzSrc, xzDest);
            //float tilt = Vector2.Angle(xySrc, xyDest);

            //this.Pan((uint)(pan / (540.0f / 255.0f)));
            //this.Tilt((uint)(tilt / (270.0f / 255.0f)) + 43);

            //float angle = Vector3.Angle(this.Location, destPoint);
            //float sign = Mathf.Sign(Vector3.Dot(NormalVector, Vector3.Cross(this.Location, destPoint)));
            //float resultingAngle =  angle * sign;

            //float anglePan = Vector3.Angle(this.Location, destPoint);
            //float signPan = Mathf.Sign(Vector3.Dot(Vector3.down, Vector3.Cross(this.Location, destPoint)));
            //float resultingAnglePan = angle * sign;

            //this.Pan((uint)(resultingAnglePan / (540 / 255)));
            //this.Tilt((uint)(resultingAngle / (270 / 255)));
            //Debug.Log("Resulting Angle of Point to Method: " + resultingAngle.ToString() + "// With Vec dest, src and normal: " + destPoint.ToString() + "," + this.Location + "," + NormalVector);
        }

        private float HandlePanAngleResult(Vector2 XZ, float angle) {
            bool xSign = (XZ.x >= 0) ? true: false;
            bool zSign = (XZ.y >= 0) ? true : false;

            int normXSign; 
            int normZSign;
            if (this.NormalVector.x < 0) normXSign = -1;
            else if (this.NormalVector.x > 0) normXSign = 1;
            else normXSign = 0;

            if (this.NormalVector.z < 0) normZSign = -1;
            else if (this.NormalVector.z > 0) normZSign = 1;
            else normZSign = 0;

            if (normXSign == -1 && normZSign == 0)
            {
                if (xSign && zSign) return angle;
                else if (xSign && !zSign) return angle;
                else if (!xSign && !zSign) return 360 - angle;
                else return angle;//360 -angle;
            }
            else if (normXSign == 1 && normZSign == 0)
            {
                if (xSign && zSign) return 360 - angle;
                else if (xSign && !zSign) return angle;
                else if (!xSign && !zSign) return angle;
                else return angle;
            }
            else if (normXSign == 0 && normZSign == -1)
            {
                if (xSign && zSign) return angle;
                else if (xSign && !zSign) return 360 - angle;
                else if (!xSign && !zSign) return 360 - angle;
                else return angle;
            }
            else {
                if (xSign && zSign) return angle;
                else if (xSign && !zSign) return angle;
                else if (!xSign && !zSign) return 360- angle;
                else return 360 - angle;
            }



            
        }

        public override String ToString()
        {
            return "Movinghead: " + Name + "\t     |Startadress: " + StartAddress.ToString() + "\t|Channelmode: " + NumberOfChannels.ToString() + "\n";
        }
    }
}
