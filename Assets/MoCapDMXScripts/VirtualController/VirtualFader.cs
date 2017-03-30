using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoCapDMXScripts.VirtualController
{
    public class VirtualFaderByOneBone : VirtualControllerBaseClass
    {
        Action<float> _function;
        private string _boneName;
        private MoCapBone bone;
        private Func<MoCapBone, float> _parameterUsage;
        private float _defaultIfInactive;

        public VirtualFaderByOneBone(String controllerID, String boneName, Action<float> function, Func<MoCapBone, float> parameterUsage, bool isEnabled = false, float defaultValueIfInactive = 0)
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

    public class VirtualFaderByTwoBonesUINT : VirtualControllerBaseClass
    {
        
        Action<uint> _function;
        private string _boneNameOne;
        private string _boneNameTwo;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Func<MoCapBone, MoCapBone, uint> _mainManipulationFunc;
        private uint _defaultIfInactive;

        public VirtualFaderByTwoBonesUINT(String controllerID, String boneNameOne, String boneNameTwo,
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

    public class VirtualTwoParameterFader_uint : VirtualControllerBaseClass
    {

        Action<uint>[] _function;
        private string _boneNameOne;
        private string _boneNameTwo;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Func<MoCapBone, MoCapBone, uint> _mainManipulationFunc;
        private uint _defaultIfInactive;

        public VirtualTwoParameterFader_uint(String controllerID, String boneNameOne, String boneNameTwo,
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

    public class VirtualTwoParameterFader_f : VirtualControllerBaseClass
    {

        Action<float>[] _functions;
        private string _boneNameOne;
        private string _boneNameTwo;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Func<MoCapBone, MoCapBone, float> _mainManipulationFunc;
        private float _defaultIfInactive;

        public VirtualTwoParameterFader_f(String controllerID, String boneNameOne, String boneNameTwo,
            Action<float>[] targetFunctions, Func<MoCapBone, MoCapBone, float> ParameterUsageFunction, bool isEnabled = false, float defaultValueIfInactive = 0)
        {
            _controllerID = controllerID;
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            IsEnabled = isEnabled;
            _functions = targetFunctions;
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
                    foreach (Action<float> act in _functions)
                    {
                        act(value);
                    }
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
                foreach (Action<float> act in _functions)
                {
                    act(_defaultIfInactive);
                }
            }
        }

    }

    public class VirtualPositionFaderWithMultipleTargets : VirtualControllerBaseClass
    {

        Action<Vector3>[] _function;
        private string _boneNameOne;
        private MoCapBone _boneOne;
        //private Func<MoCapBone, Type> _BoneOneParameterUsage;
        //private Func<MoCapBone, Type> _BoneTwoParameterUsage;
        private Func<MoCapBone, Vector3> _mainManipulationFunc;

        public VirtualPositionFaderWithMultipleTargets(String controllerID, String boneNameOne,
            Action<Vector3>[] targetFunction, Func<MoCapBone, Vector3> ParameterUsageFunction, bool isEnabled = false)
        {
            if (targetFunction[0].Method.GetParameters()[0].ParameterType != ParameterUsageFunction.Method.ReturnType)
            {
                Debug.Log("Targed Function Parametertype does not match the returnType of the Lambdaexpression!");
            }
            _controllerID = controllerID;
            _boneNameOne = boneNameOne;
            IsEnabled = isEnabled;
            _function = targetFunction;
            _mainManipulationFunc = ParameterUsageFunction;
            VirtualControllerCollection.Instance.Add(this);
        }


        public override void Execute()
        {
            if (IsEnabled)
            {
                var time = System.Diagnostics.Stopwatch.StartNew();
                _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
                if (_boneOne != null)
                {
                    Vector3 value = _mainManipulationFunc(_boneOne);
                    foreach (Action<Vector3> func in _function)
                    {
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
                //foreach (Action<uint> func in _function)
                //{
                //    func(_defaultIfInactive);
                //}
            }
        }

    }

    public class VirtualFaderFromInitialValueWithMultipleTargetsUINT : VirtualControllerBaseClass
    {

        Action<uint>[] _function;
        private uint valueAtActivation = 0;
        private bool valueHasBeenSet = false;
        private string _boneNameOne;
        private MoCapBone _boneOne;
        private Func<MoCapBone, uint> _mainManipulationFunc;

        public VirtualFaderFromInitialValueWithMultipleTargetsUINT(String controllerID, String boneNameOne,
            Action<uint>[] targetFunction, Func<MoCapBone, uint> ParameterUsageFunction, bool isEnabled = false)
        {
            if (targetFunction[0].Method.GetParameters()[0].ParameterType != ParameterUsageFunction.Method.ReturnType)
            {
                Debug.Log("Targed Function Parametertype does not match the returnType of the Lambdaexpression!");
            }
            _controllerID = controllerID;
            _boneNameOne = boneNameOne;
            IsEnabled = isEnabled;
            _function = targetFunction;
            _mainManipulationFunc = ParameterUsageFunction;
            VirtualControllerCollection.Instance.Add(this);
        }


        public override void Execute()
        {
            if (IsEnabled)
            {
                //var time = System.Diagnostics.Stopwatch.StartNew();
                _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
                if (_boneOne != null)
                {
                    if (!valueHasBeenSet) {
                        valueHasBeenSet = true;
                        valueAtActivation = _mainManipulationFunc(_boneOne);
                    }
                    uint value = (uint)Math.Abs((uint )_mainManipulationFunc(_boneOne) - valueAtActivation);
                    if (value < 0) value = 0;
                    Debug.Log("Fader with Startvalue: " + value);
                    foreach (Action<uint> func in _function)
                    {
                        func(value);
                    }
                    //_function(value);
                }
                else
                {
                    Debug.Log("Bone for VirtualController could not be found (is null)!");
                }
                //time.Stop();
                //Debug.Log("FaderLambda Execution took: " + time.Elapsed.TotalMilliseconds + " ms!");
            }
            else
            {
                valueAtActivation = 0;
                valueHasBeenSet = false;
            }
        }

    }
}


