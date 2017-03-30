using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoCapDMXScripts
{


    public class DMXPackage : List<byte>
    {
        public enum PROTOCOL_TYPE
        {
            ARTNET = 0,
            ESP = 1
        }

        public PROTOCOL_TYPE ProtocolType
        {
            get; private set;
        }

        public int StartIndexOfDMXData
        {
            get
            {
                if (ProtocolType == PROTOCOL_TYPE.ARTNET) {
                    return 18;
                }
                else {
                    return 9; //if Protocoltype is ESP
                }
            }
        }

        public DMXPackage(PROTOCOL_TYPE type) {
            ProtocolType = type;

            if (ProtocolType == PROTOCOL_TYPE.ARTNET) {
                byte[] header = {
                    0x41,0x72,0x74,0x2d,0x4e,0x65,0x74,0x00, // Art-Net with nulltermination
                    0x00,0x50, //OPCode: 0x5000= OpOutput/OpDmx  Transmitt lowbyte first
                    0x00,0x0e, // High- and Lowbyte of revision number (current version 14)
                    0x00, //sequence number, for information only
                    0x00, // Physical input port from which dmx data was input
                    0x00, // Subuniverse
                    0x00, //Net
                    0x02,0x00 //length of DMX Data
                };
                this.AddRange(header);
                for (int i = 0; i < 512; i++) {
                    this.Add(0x00); // prefill dmx data with zeros
                }
            }
            if (ProtocolType == PROTOCOL_TYPE.ESP) {
                byte[] header = {
                    0x45,0x53,0x44,0x44, //ESDD
                    0x00, //universe
                    0x00, //startcode
                    0x01, //datatype = DMX Data
                    0x02, 0x00 //length of DMX Data
                };
                this.AddRange(header);
                for (int i = 0; i < 512; i++)
                {
                    this.Add(0x00); // prefill dmx data with zeros
                }
            }
        }
    }
}
