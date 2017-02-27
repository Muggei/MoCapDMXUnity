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

        public VirtualSingleParameterFader(String controllerID, String boneName, Action<float> function, Func<MoCapBone, float> parameterUsage, bool isEnabled = false, float defaultValueIfInactive = 0)
        {
            _controllerID = controllerID;
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
                    float value = _parameterUsage(bone);
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

    public class VirtualTwoParameterFaderUINT : VirtualControllerBaseClass
    {
        
        Action<uint> _function;
        private string _boneNameOne;
        private string _boneNameTwo;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        //private Func<MoCapBone, Type> _BoneOneParameterUsage;
        //private Func<MoCapBone, Type> _BoneTwoParameterUsage;
        private Func<MoCapBone, MoCapBone, uint> _mainManipulationFunc;
        private uint _defaultIfInactive;

        public VirtualTwoParameterFaderUINT(String controllerID, String boneNameOne, String boneNameTwo,
            Action<uint> targetFunction, Func<MoCapBone, MoCapBone, uint> ParameterUsageFunction, bool isEnabled = false, uint defaultValueIfInactive = 0)
        {
            if (targetFunction.Method.GetParameters()[0].ParameterType != ParameterUsageFunction.Method.ReturnType) {
                Debug.Log("Targed Function Parametertype does not match the returnType of the Lambdaexpression!");
            }
            _controllerID = controllerID;
            _boneNameOne= boneNameOne;
            _boneNameTwo = boneNameTwo;
            IsEnabled = isEnabled;
            _function = targetFunction;
            _mainManipulationFunc = ParameterUsageFunction;
            _defaultIfInactive = defaultValueIfInactive;
            VirtualControllerCollection.Instance.Add(this);
        }


        public override void Execute()
        {
            if (IsEnabled)
            {
                var time = System.Diagnostics.Stopwatch.StartNew();
                _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
                _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);
                if (_boneOne != null && _boneTwo != null)
                {
                    uint value = _mainManipulationFunc(_boneOne,_boneTwo);
                    _function(value);
                }
                else
                {
                    Debug.Log("Bone for VirtualController could not be found (is null)!");
                }
                time.Stop();
                //Debug.Log("FaderLambda Execution took: " + time.Elapsed.TotalMilliseconds + " ms!");
            }
            else
            {
                _function(_defaultIfInactive);
            }
        }

    }

    public class VirtualTwoParameterFaderWithMultipleTargetsUINT : VirtualControllerBaseClass
    {

        Action<uint>[] _function;
        private string _boneNameOne;
        private string _boneNameTwo;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        //private Func<MoCapBone, Type> _BoneOneParameterUsage;
        //private Func<MoCapBone, Type> _BoneTwoParameterUsage;
        private Func<MoCapBone, MoCapBone, uint> _mainManipulationFunc;
        private uint _defaultIfInactive;

        public VirtualTwoParameterFaderWithMultipleTargetsUINT(String controllerID, String boneNameOne, String boneNameTwo,
            Action<uint>[] targetFunction, Func<MoCapBone, MoCapBone, uint> ParameterUsageFunction, bool isEnabled = false, uint defaultValueIfInactive = 0)
        {
            if (targetFunction[0].Method.GetParameters()[0].ParameterType != ParameterUsageFunction.Method.ReturnType)
            {
                Debug.Log("Targed Function Parametertype does not match the returnType of the Lambdaexpression!");
            }
            _controllerID = controllerID;
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            IsEnabled = isEnabled;
            _function = targetFunction;
            _mainManipulationFunc = ParameterUsageFunction;
            _defaultIfInactive = defaultValueIfInactive;
            VirtualControllerCollection.Instance.Add(this);
        }


        public override void Execute()
        {
            if (IsEnabled)
            {
                var time = System.Diagnostics.Stopwatch.StartNew();
                _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
                _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);
                if (_boneOne != null && _boneTwo != null)
                {
                    uint value = _mainManipulationFunc(_boneOne, _boneTwo);
                    foreach (Action<uint> func in _function) {
                        func(value);
                    }
                    //_function(value);
                }
                else
                {
                    Debug.Log("Bone for VirtualController could not be found (is null)!");
                }
                time.Stop();
                //Debug.Log("FaderLambda Execution took: " + time.Elapsed.TotalMilliseconds + " ms!");
            }
            else
            {
                foreach (Action<uint> func in _function)
                {
                    func(_defaultIfInactive);
                }
            }
        }

    }

    public class VirtualTwoParameterFaderFLOAT : VirtualControllerBaseClass
    {

        Action<float> _function;
        private string _boneNameOne;
        private string _boneNameTwo;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        //private Func<MoCapBone, Type> _BoneOneParameterUsage;
        //private Func<MoCapBone, Type> _BoneTwoParameterUsage;
        private Func<MoCapBone, MoCapBone, float> _mainManipulationFunc;
        private float _defaultIfInactive;

        public VirtualTwoParameterFaderFLOAT(String controllerID, String boneNameOne, String boneNameTwo,
            Action<float> targetFunction, Func<MoCapBone, MoCapBone, float> ParameterUsageFunction, bool isEnabled = false, float defaultValueIfInactive = 0)
        {
            if (targetFunction.Method.GetParameters()[0].ParameterType != ParameterUsageFunction.Method.ReturnType)
            {
                Debug.Log("Targed Function Parametertype does not match the returnType of the Lambdaexpression!");
            }
            _controllerID = controllerID;
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            IsEnabled = isEnabled;
            _function = targetFunction;
            _mainManipulationFunc = ParameterUsageFunction;
            _defaultIfInactive = defaultValueIfInactive;
            VirtualControllerCollection.Instance.Add(this);
        }


        public override void Execute()
        {
            if (IsEnabled)
            {
                var time = System.Diagnostics.Stopwatch.StartNew();
                _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
                _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);
                if (_boneOne != null && _boneTwo != null)
                {
                    float value = _mainManipulationFunc(_boneOne, _boneTwo);
                    _function(value);
                }
                else
                {
                    Debug.Log("Bone for VirtualController could not be found (is null)!");
                }
                time.Stop();
                //Debug.Log("FaderLambda Execution took: " + time.Elapsed.TotalMilliseconds + " ms!");
            }
            else
            {
                _function(_defaultIfInactive);
            }
        }

    }
}


