using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoCapDMXScripts.MovingHeads
{
    public class MH_PicoWash40 : MovingHead
    {
        public enum CHANNELMODE {
            CH25 = 25
        };

        public enum COLORCHANNEL
        {
            White = 3,
            Red = 0,
            Green = 1,
            Blue = 2
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
        public uint CurrentDimmerValue { get; private set; }
        public uint CurrentStroboValue { get; private set; }

        public Vector3 Location { get; set; }

        public static float MAXPAN = 540.0f;
        public static float MAXTILT = 180.0f;
        public float PanDegreePerDmxValue_16Bit = MAXPAN / 65536.0f;
        public float TiltDegreePerDmxValue_16Bit = MAXTILT / 65536.0f;




        public MH_PicoWash40(int startAddress, CHANNELMODE channelmode, List<byte> dmxUDPPackage, int startIndexOfDMXData, string name = "")
        {
            StartAddress = startAddress;
            NumberOfChannels = (int)channelmode;
            m_dmxDataOffset = startIndexOfDMXData - 1;
            m_dmxUDPPackage = dmxUDPPackage;
            Name = name;
            CurrentPanValue = 0;
            CurrentTiltValue = 0;
        }

        public void Pan(uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot Pan because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress] = (byte)dmxValue;
                    CurrentPanValue = dmxValue;
                }
            }

        }

        public void Pan(float angle)
        {
            if (NumberOfChannels == (int)CHANNELMODE.CH25)
            {
                UInt16 pan = (UInt16)(angle / PanDegreePerDmxValue_16Bit);

                m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 1] = (byte)(pan & 0xff);
                m_dmxUDPPackage[m_dmxDataOffset + StartAddress] = (byte)(0xff & (pan >> 8));
                fCurrentPanAngle = angle;
                //Debug.Log("CURRENT ANGLE OF " + this.ToString() +  " Pan: " + fCurrentPanAngle);
            }
        }

        public void Tilt(uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot Tilt because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 2] = (byte)dmxValue;
                    CurrentTiltValue = dmxValue;
                }
            }
        }

        public void Tilt(float angle)
        {
            //if (angle <= MAXTILT) angle -= MAXTILT;
            if (NumberOfChannels == (int)CHANNELMODE.CH25)
            {
                if (angle <= MAXTILT)
                {
                    UInt16 tilt = (UInt16)(angle / TiltDegreePerDmxValue_16Bit);

                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 3] = (byte)(tilt & 0xff);
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 2] = (byte)(0xff & (tilt >> 8));
                    fCurrentTiltAngle = angle;
                }
                else {
                    UInt16 tilt = (UInt16)(MAXTILT / TiltDegreePerDmxValue_16Bit);

                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 3] = (byte)(tilt & 0xff);
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 2] = (byte)(0xff & (tilt >> 8));
                    fCurrentTiltAngle = angle;
                }
            }
        }
        public void MasterDimmer(uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot Dimm because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 5] = (byte)dmxValue;
                    CurrentDimmerValue = dmxValue;
                }
            }
        }

        public void Strobo(uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot activate Strobo because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 6] = (byte)dmxValue;
                    CurrentStroboValue = dmxValue;
                }
            }
        }

        public void ColorLED1(COLORCHANNEL colorchannel, uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot activate Strobo because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 11 + (int)colorchannel] = (byte)dmxValue;
                    CurrentDimmerValue = dmxValue;
                }
            }
        }

        public void ColorLED2(COLORCHANNEL colorchannel, uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot activate Strobo because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 15 + (int)colorchannel] = (byte)dmxValue;
                    CurrentDimmerValue = dmxValue;
                }
            }
        }
        public void ColorLED3(COLORCHANNEL colorchannel, uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot activate Strobo because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 19 + (int)colorchannel] = (byte)dmxValue;
                    CurrentDimmerValue = dmxValue;
                }
            }
        }
        public void ColorLED4(COLORCHANNEL colorchannel, uint dmxValue)
        {
            if (dmxValue < 0 || dmxValue > 255)
            {
                Debug.LogAssertion(this.ToString() + " Cannot activate Strobo because value has to be between 0 and 255.");
            }
            else
            {
                if (NumberOfChannels == (int)CHANNELMODE.CH25)
                {
                    m_dmxUDPPackage[m_dmxDataOffset + StartAddress + 7 + (int)colorchannel] = (byte)dmxValue;
                    CurrentDimmerValue = dmxValue;
                }
            }
        }

        public void AllColorLEDs(COLORCHANNEL colorchannel, uint dmxValue)
        {
            this.ColorLED1(colorchannel, dmxValue);
            this.ColorLED2(colorchannel, dmxValue);
            this.ColorLED3(colorchannel, dmxValue);
            this.ColorLED4(colorchannel, dmxValue);
        }

        public void AllLEDsWhite(uint dmxValue) {
            this.ColorLED1(COLORCHANNEL.White, dmxValue);
            this.ColorLED2(COLORCHANNEL.White, dmxValue);
            this.ColorLED3(COLORCHANNEL.White, dmxValue);
            this.ColorLED4(COLORCHANNEL.White, dmxValue);
        }
        public void AllLEDsBlue(uint dmxValue)
        {
            this.ColorLED1(COLORCHANNEL.Blue, dmxValue);
            this.ColorLED2(COLORCHANNEL.Blue, dmxValue);
            this.ColorLED3(COLORCHANNEL.Blue, dmxValue);
            this.ColorLED4(COLORCHANNEL.Blue, dmxValue);
        }
        public void AllLEDsGreen(uint dmxValue)
        {
            this.ColorLED1(COLORCHANNEL.Green, dmxValue);
            this.ColorLED2(COLORCHANNEL.Green, dmxValue);
            this.ColorLED3(COLORCHANNEL.Green, dmxValue);
            this.ColorLED4(COLORCHANNEL.Green, dmxValue);
        }
        public void AllLEDsRed(uint dmxValue)
        {
            this.ColorLED1(COLORCHANNEL.Red, dmxValue);
            this.ColorLED2(COLORCHANNEL.Red, dmxValue);
            this.ColorLED3(COLORCHANNEL.Red, dmxValue);
            this.ColorLED4(COLORCHANNEL.Red, dmxValue);
        }

        //public void PointTo(Vector3 point)
        //{
        //    float x, y, z;

        //    int panVal = CustomHelpers::PanAndTiltAngleCalculator(
        //        std::get < 0 > (this->currentVector),
        //        std::get < 2 > (this->currentVector),
        //        XCoordinate,
        //        ZCoordinate,
        //        std::get < 0 > (*point),
        //        std::get < 2 > (*point),
        //        x,
        //        z
        //        );

        //    this->SetCurrentVector(x, std::get < 1 > (this->currentVector), z);

        //    int tiltVal = CustomHelpers::PanAndTiltAngleCalculator(
        //        std::get < 0 > (this->currentVector),
        //        std::get < 1 > (this->currentVector),
        //        XCoordinate,
        //        YCoordinate,
        //        std::get < 0 > (*point),
        //        std::get < 1 > (*point),
        //        x,
        //        y
        //        );
        //    this->SetCurrentVector(x, y, std::get < 2 > (this->currentVector));

        //    double bla = panVal * (255.0 / 540.0);
        //    float doIT = this->currentPanValue + bla;
        //    this->Pan(doIT);
        //    this->Tilt(currentTiltValue + (tiltVal * (255 / 180)));
        //}
    }
}
