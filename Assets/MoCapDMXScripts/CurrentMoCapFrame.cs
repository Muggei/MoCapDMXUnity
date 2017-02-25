using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoCapDMXScripts
{
    public class MoCapBone {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 PositionInCentimeter { get; private set; }
        public Quaternion Rotation { get; private set; }

        public MoCapBone(int ID, string name, Vector3 position, Quaternion rotation) {
            this.ID = ID;
            this.Name = name;
            this.Position = position;
            this.PositionInCentimeter = new Vector3(position.x * 100, position.y * 100, position.z * 100);
            this.Rotation = rotation;
        }
    }

    public class CurrentMoCapFrame
    {
        private static CurrentMoCapFrame instance;

        private CurrentMoCapFrame() { }

        public static CurrentMoCapFrame Instance {
            get {
                if (instance == null) {
                    instance = new CurrentMoCapFrame();
                }
                return instance;
            }
        }

        public int frame { get; set; }
        public float timeStamp { get; set; }
        public List<MoCapBone> bones { get; set; }


        public override string ToString()
        {
            return "Current Frame: " + this.frame + "/ TimeStamp: " + timeStamp.ToString();
        }
    }
}
