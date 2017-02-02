using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace MoCapDMXScripts
{
    public class MoCapDataHandler  : MonoBehaviour
    {
        public GameObject SlipStreamObject;

        public bool ShowMocapData = false;
        private bool currentShowMocapData = false;

        void Start()
        {
            SlipStreamObject.GetComponent<SlipStream>().PacketNotification += new PacketReceivedHandler(OnPacketReceived);
            PrepareBoneDictionary();
            PrepareBoneToSkeleton();
        }

        void Update()
        {
            //== if there is new data or settings changed, apply data and retarget ==--

            if (mNew || ShowMocapData != currentShowMocapData || currentActor != Actor)
            {
                if (mPacket == null)
                {
                    return;
                }

                currentShowMocapData = ShowMocapData;

                if (currentActor != Actor)
                {
                    currentActor = Actor;
                    PrepareBoneDictionary();
                    PrepareBoneToSkeleton();
                }

                mNew = false;


                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(mPacket);

                //== frame id and timestamp ==-
                XmlNodeList frameInfoList = xmlDoc.GetElementsByTagName("Frame");
                for (int index = 0; index < frameInfoList.Count; index++)
                {
                    CurrentMoCapFrame.Instance.frame = System.Convert.ToInt32(frameInfoList[index].Attributes[0].InnerText);
                    CurrentMoCapFrame.Instance.timeStamp = (float)System.Convert.ToDouble(frameInfoList[index].Attributes[1].InnerText);
                }
                
                //== skeletons ==--

                XmlNodeList boneList = xmlDoc.GetElementsByTagName("Bone");
                List<MoCapDMXScripts.MoCapBone> bones = new List<MoCapBone>();
                for (int index = 0; index < boneList.Count; index++)
                {
                    int iD = System.Convert.ToInt32(boneList[index].Attributes["ID"].InnerText);
                    string boneName = boneList[index].Attributes["Name"].InnerText;

                    float x = (float)System.Convert.ToDouble(boneList[index].Attributes["x"].InnerText);
                    float y = (float)System.Convert.ToDouble(boneList[index].Attributes["y"].InnerText);
                    float z = (float)System.Convert.ToDouble(boneList[index].Attributes["z"].InnerText);

                    float qx = (float)System.Convert.ToDouble(boneList[index].Attributes["qx"].InnerText);
                    float qy = (float)System.Convert.ToDouble(boneList[index].Attributes["qy"].InnerText);
                    float qz = (float)System.Convert.ToDouble(boneList[index].Attributes["qz"].InnerText);
                    float qw = (float)System.Convert.ToDouble(boneList[index].Attributes["qw"].InnerText);

                    //== coordinate system conversion (right to left handed) ==--

                    x = -x;
                    qx = -qx;
                    qw = -qw;

                    bones.Add(new MoCapBone(iD, boneName, new Vector3(x, y, z), new Quaternion(qx, qy, qz, qw)));
                }
                CurrentMoCapFrame.Instance.bones = bones;

                
                //== rigid bodies ==-- 

                //== skip rigid bodies ==--

                XmlNodeList rbList = xmlDoc.GetElementsByTagName("RigidBody");

                for (int index = 0; index < rbList.Count; index++)
                {
                    int id = System.Convert.ToInt32(rbList[index].Attributes["ID"].InnerText);
                    string name = rbList[index].Attributes["Name"].InnerText;

                    float x = (float)System.Convert.ToDouble(rbList[index].Attributes["x"].InnerText);
                    float y = (float)System.Convert.ToDouble(rbList[index].Attributes["y"].InnerText);
                    float z = (float)System.Convert.ToDouble(rbList[index].Attributes["z"].InnerText);

                    float qx = (float)System.Convert.ToDouble(rbList[index].Attributes["qx"].InnerText);
                    float qy = (float)System.Convert.ToDouble(rbList[index].Attributes["qy"].InnerText);
                    float qz = (float)System.Convert.ToDouble(rbList[index].Attributes["qz"].InnerText);
                    float qw = (float)System.Convert.ToDouble(rbList[index].Attributes["qw"].InnerText);

                    //== coordinate system conversion (right to left handed) ==--

                    x = -x;
                    qx = -qx;
                    qw = -qw;
                }
            }
        }

        private string mPacket;
        private bool mNew = false;
        /// <summary>
        /// incoming real-time pose data
        /// </summary>
        public void OnPacketReceived(object sender, string Packet) {
            mPacket = Packet;
            mNew = true;
        }

        public void OnSamplePacket(string packet) {
            mPacket = packet.Replace("Trent", Actor);
            mNew = true;
        }


        //== bone mapping look-up table ==--

        public string Actor = "Trent";
        private string currentActor = "";
        private Dictionary<string, string> mBoneDictionary = new Dictionary<string, string>();
        public void PrepareBoneDictionary()
        {

            mBoneDictionary.Clear();
            mBoneDictionary.Add(Actor + "_Hip", "Hips");
            mBoneDictionary.Add(Actor + "_Ab", "Spine");
            mBoneDictionary.Add(Actor + "_Chest", "Spine1");
            mBoneDictionary.Add(Actor + "_Neck", "Neck");
            mBoneDictionary.Add(Actor + "_Head", "Head");
            mBoneDictionary.Add(Actor + "_LShoulder", "LeftShoulder");
            mBoneDictionary.Add(Actor + "_LUArm", "LeftArm");
            mBoneDictionary.Add(Actor + "_LFArm", "LeftForeArm");
            mBoneDictionary.Add(Actor + "_LHand", "LeftHand");
            mBoneDictionary.Add(Actor + "_RShoulder", "RightShoulder");
            mBoneDictionary.Add(Actor + "_RUArm", "RightArm");
            mBoneDictionary.Add(Actor + "_RFArm", "RightForeArm");
            mBoneDictionary.Add(Actor + "_RHand", "RightHand");
            mBoneDictionary.Add(Actor + "_LThigh", "LeftUpLeg");
            mBoneDictionary.Add(Actor + "_LShin", "LeftLeg");
            mBoneDictionary.Add(Actor + "_LFoot", "LeftFoot");
            mBoneDictionary.Add(Actor + "_RThigh", "RightUpLeg");
            mBoneDictionary.Add(Actor + "_RShin", "RightLeg");
            mBoneDictionary.Add(Actor + "_RFoot", "RightFoot");
            mBoneDictionary.Add(Actor + "_LToe", "LeftToeBase");
            mBoneDictionary.Add(Actor + "_RToe", "RightToeBase");
        }

        private Dictionary<string, string> mBoneToSkeleton = new Dictionary<string, string>();
        public void PrepareBoneToSkeleton()
        {
            mBoneToSkeleton.Clear();
            mBoneToSkeleton.Add("Hips", Actor + "_Hip");
            mBoneToSkeleton.Add("Spine", Actor + "_Ab");
            mBoneToSkeleton.Add("Chest", Actor + "_Chest");
            mBoneToSkeleton.Add("Neck", Actor + "_Neck");
            mBoneToSkeleton.Add("Head", Actor + "_Head");
            mBoneToSkeleton.Add("LeftShoulder", Actor + "_LShoulder");
            mBoneToSkeleton.Add("LeftUpperArm", Actor + "_LUArm");
            mBoneToSkeleton.Add("LeftLowerArm", Actor + "_LFArm");
            mBoneToSkeleton.Add("LeftHand", Actor + "_LHand");
            mBoneToSkeleton.Add("RightShoulder", Actor + "_RShoulder");
            mBoneToSkeleton.Add("RightUpperArm", Actor + "_RUArm");
            mBoneToSkeleton.Add("RightLowerArm", Actor + "_RFArm");
            mBoneToSkeleton.Add("RightHand", Actor + "_RHand");
            mBoneToSkeleton.Add("LeftUpperLeg", Actor + "_LThigh");
            mBoneToSkeleton.Add("LeftLowerLeg", Actor + "_LShin");
            mBoneToSkeleton.Add("LeftFoot", Actor + "_LFoot");
            mBoneToSkeleton.Add("RightUpperLeg", Actor + "_RThigh");
            mBoneToSkeleton.Add("RightLowerLeg", Actor + "_RShin");
            mBoneToSkeleton.Add("RightFoot", Actor + "_RFoot");
            mBoneToSkeleton.Add("LeftToeBase", Actor + "_LToe");
            mBoneToSkeleton.Add("RightToeBase", Actor + "_RToe");
        }
    }
}
