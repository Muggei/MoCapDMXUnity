using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoCapDMXScripts
{
    public class GlobalLinkerCollection : List<MarkerFunctionalityLink>
    {
        private static GlobalLinkerCollection instance;

        private GlobalLinkerCollection() { }

        public static GlobalLinkerCollection Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GlobalLinkerCollection();
                }
                return instance;
            }
        }

    }

    public class MarkerFunctionalityLink
    {
        private Action<uint> _function;
        private Action<float> _dynamicFunction;
        private string _boneName;
        private MoCapBone bone;

        public bool isEnabled { get; set; }

        public MarkerFunctionalityLink(String boneName, Action<uint> function)
        {
            _boneName = boneName;
            _function = function;
            bone = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneName);
            GlobalLinkerCollection.Instance.Add(this);
        }

        public MarkerFunctionalityLink(String boneName, Action<float> function, bool floatingVar)
        {
            _boneName = boneName;
            _dynamicFunction = function;
            bone = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneName);
            GlobalLinkerCollection.Instance.Add(this);
        }

        public void Excecute()
        {
            if (isEnabled == true) {
                _dynamicFunction(bone.Rotation.eulerAngles.y);
            }
            //float yRot = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneName).Rotation.eulerAngles.y;
            //_dynamicFunction(yRot);
        }
    }
}
