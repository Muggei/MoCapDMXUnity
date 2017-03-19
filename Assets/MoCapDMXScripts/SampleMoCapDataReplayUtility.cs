using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml;

namespace Assets
{
    public class SampleMoCapDataReplayUtility : MonoBehaviour
    {
        private int maxFrameCount = 0;
        private int MocapFrame = 1;
        private int currentFrame = 1;

        [Tooltip("Needs to be located in the Project's Resources Folder.")]
        public string XMLFileName;
        

        [InspectorButton("OnLoadButtonClicked")]
        public bool LoadXML;
        private bool xmlIsLoaded = false;

        [InspectorButton("OnReplayClicked")]
        public bool ReplayLoop;
        private bool ReplayRunning = false;
        
        public MoCapDMXScripts.MoCapDataHandler DMXDataHandler;
        private XmlDocument xmlDoc = new XmlDocument();
        private XmlNodeList skeletonDescriptionList;
        private XmlNodeList frameInfoList;


        private void OnLoadButtonClicked() {

            xmlDoc.Load(Application.dataPath + "/Resources/" + XMLFileName);
            xmlIsLoaded = true;

            skeletonDescriptionList = xmlDoc.GetElementsByTagName("SkeletonDescriptions");

            frameInfoList = xmlDoc.GetElementsByTagName("Frame");
            maxFrameCount = frameInfoList.Count;

            Debug.Log("XML Load finished!");
            DMXDataHandler.OnSamplePacket("<?xml version =\"1.0\" ?><Stream>\n" + skeletonDescriptionList[0].InnerXml + "</Stream>");
        }

        private void OnReplayClicked()
        {
            if(xmlIsLoaded) ReplayRunning = !ReplayRunning;
        }


        void FixedUpdate()
        {
            if (ReplayRunning)
            {
                //Update method is 60fps, recorded session is 120fps. skip every second frame for compensation
                MocapFrame+=2;
                if (MocapFrame >= maxFrameCount) MocapFrame = 1;
            }

            if (currentFrame == MocapFrame)
            {
                return;
            }

            currentFrame = MocapFrame;
            
            string frameString = frameInfoList[MocapFrame].OuterXml +  frameInfoList[MocapFrame].InnerXml;
            DMXDataHandler.OnSamplePacket("<?xml version=\"1.0\" ?><Stream>\n" + frameString + "</Stream>");
        }
    }
}
