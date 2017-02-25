using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoCapDMXScripts.VirtualController
{
    public class VirtualSingleParameterFader : VirtualControllerBaseClass
    {
        Action<float> _function;
        private string _boneName;
        private MoCapBone bone;
        private Func<MoCapBone, float> _parameterUsage;
        private float _defaultIfInactive;

        public VirtualSingleParameterFader(String boneName, Action<float> function, Func<MoCapBone, float> parameterUsage, bool isEnabled = false, float defaultValueIfInactive = 0)
        {
            _boneName = boneName;
            IsEnabled = isEnabled;
            _function = function;
            _parameterUsage = parameterUsage;
            _defaultIfInactive = defaultValueIfInactive;
            VirtualControllerCollection.Instance.Add(this);
        }


        public override void Execute()
        {
            if (IsEnabled)
            {
                var time = System.Diagnostics.Stopwatch.StartNew();
                bone = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneName);
                if (bone != null)
                {
                    if (_boneName == "Daniel_RFArm") Debug.Log("Y and Z of RFArm:" + bone.Rotation.eulerAngles.y + ", " + bone.Rotation.eulerAngles.z);
                    float value = _parameterUsage(bone);
                    if (_boneName == "Daniel_RFArm") Debug.Log("Value after Lambda expression: " + value);
                    _function(value);
                }
                else
                {
                    Debug.Log("Bone for VirtualController could not be found (is null)!");
                }
                time.Stop();
                //Debug.Log("FaderLambda Execution took: " + time.Elapsed.TotalMilliseconds + " ms!");
            }
            else {
                _function(_defaultIfInactive);
            }
        }

    }
}


