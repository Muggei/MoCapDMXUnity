using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoCapDMXScripts.VirtualController
{
    public class VirtualToggleSwitchByOneBone : VirtualControllerBaseClass
    {
        private String _boneName;
        //private String _switchID;
        private Func<MoCapBone, bool> _stateExpression;
        private VirtualControllerBaseClass _switch;
        private MoCapBone _bone;

        public VirtualToggleSwitchByOneBone(String switchID,String boneName, Func<MoCapBone, bool> expression, VirtualControllerBaseClass virtualControllerToSwitch) {

            _boneName = boneName;
            _stateExpression = expression;
            _switch = virtualControllerToSwitch;
            _controllerID = switchID;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            _bone = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneName);
            //Debug.Log("Bone Y: " + _bone.PositionInCentimeter.y.ToString());
            if (_bone != null) {
                _switch.IsEnabled  = _stateExpression(_bone);
                //Debug.Log("SwitchState = " + _switch);
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID +  " - could not be found (is null)!");
            }
        }
    }

    public class VirtualToggleSwitchOverTimeByTwoBoneParameters : VirtualControllerBaseClass
    {
        private String _boneNameOne;
        private String _boneNameTwo;
        //private String _switchID;
        private Func<MoCapBone,MoCapBone, bool> _stateExpression;
        private VirtualControllerBaseClass _switch;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private float _durationUntilSwitch;
        private float _currentDuration = 0;
        private DateTime? startTime = null;

        public VirtualToggleSwitchOverTimeByTwoBoneParameters(String switchID, String boneNameOne, String boneNameTwo,  Func<MoCapBone,MoCapBone, bool> expression,float durationInMilliSecondsUntilSwitch, VirtualControllerBaseClass virtualControllerToSwitch)
        {
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            _stateExpression = expression;
            _switch = virtualControllerToSwitch;
            _controllerID = switchID;
            _durationUntilSwitch = durationInMilliSecondsUntilSwitch;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
            _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);

            if (_boneOne != null && _boneTwo != null)
            {
                bool checker = _stateExpression(_boneOne, _boneTwo);
                if (checker)
                {
                    if (startTime == null) startTime = DateTime.Now;
                    else {
                        _currentDuration = (float)(DateTime.Now - startTime).Value.TotalMilliseconds;
                    }
                    
                    if (_currentDuration >= _durationUntilSwitch)
                    {
                        _switch.IsEnabled = !_switch.IsEnabled;
                        _currentDuration = 0.0f;
                        startTime = null;
                    }
                }
                else
                {
                    _currentDuration = 0.0f;
                    startTime = null;
                }
            }
            else {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID + " - could not be found (is null)!");
            }
        }
    }

    public class VirtualToggleSwitchByTwoBones : VirtualControllerBaseClass
    {
        private String _boneNameOne;
        private String _boneNameTwo;
        //private String _switchID;
        private Func<MoCapBone, MoCapBone, bool> _stateExpression;
        private VirtualControllerBaseClass _switch;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;

        public VirtualToggleSwitchByTwoBones(String switchID, String boneNameOne, String boneNameTwo, Func<MoCapBone, MoCapBone, bool> expression, VirtualControllerBaseClass virtualControllerToSwitch)
        {
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            _stateExpression = expression;
            _switch = virtualControllerToSwitch;
            _controllerID = switchID;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
            _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);

            if (_boneOne != null && _boneTwo != null)
            {
                bool checker = _stateExpression(_boneOne, _boneTwo);
                //Debug.Log("Check state = " + checker);
                if (checker)
                {

                    _switch.SetActiveState(true);
                }
                else
                {
                    _switch.SetActiveState(false);
                }
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID + " - could not be found (is null)!");
            }
        }
    }

    public class VirtualValueSwitchByTwoBones : VirtualControllerBaseClass{
        private String _boneNameOne;
        private String _boneNameTwo;
        //private String _switchID;
        private Func<MoCapBone, MoCapBone, bool> _stateExpression;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Action<uint>[] _functions;
        private uint _activeValue;
        private uint _inactiveValue;

        public VirtualValueSwitchByTwoBones(String switchID, String boneNameOne, String boneNameTwo, Action<uint>[] functionsToCall, Func<MoCapBone, MoCapBone, bool> expression, uint valueIfActive, uint valueIfInactive)
        {
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            _stateExpression = expression;
            _controllerID = switchID;
            _functions = functionsToCall;
            _activeValue = valueIfActive;
            _inactiveValue = valueIfInactive;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
            _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);

            if (_boneOne != null && _boneTwo != null)
            {
                bool checker = _stateExpression(_boneOne, _boneTwo);
                //Debug.Log("Check state = " + checker);
                if (checker)
                {
                    foreach (Action<uint> act in _functions) {
                        act(_activeValue);
                    }
                }
                else
                {
                    foreach (Action<uint> act in _functions)
                    {
                        act(_inactiveValue);
                    }
                }
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID + " - could not be found (is null)!");
            }
        }
    }

    public class VirtualValueSwitchOverTimeByTwoBones : VirtualControllerBaseClass
    {
        private String _boneNameOne;
        private String _boneNameTwo;
        //private String _switchID;
        private Func<MoCapBone, MoCapBone, bool> _stateExpression;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Action<uint>[] _functions;
        private uint _activeValue;
        private uint _inactiveValue;
        private float _durationUntilSwitch;
        private float _currentDuration = 0;
        private DateTime? startTime = null;
        private bool IsCurrentValueOfActiveType = false;

        public VirtualValueSwitchOverTimeByTwoBones(String switchID, String boneNameOne, String boneNameTwo, Action<uint>[] functionsToCall, Func<MoCapBone, MoCapBone, bool> expression, float durationInMilliSecondsUntilSwitch, bool startingWithState,uint valueIfActive, uint valueIfInactive)
        {
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            _stateExpression = expression;
            _controllerID = switchID;
            _functions = functionsToCall;
            _activeValue = valueIfActive;
            _inactiveValue = valueIfInactive;
            _durationUntilSwitch = durationInMilliSecondsUntilSwitch;
            IsCurrentValueOfActiveType = startingWithState;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
            _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);

            if (_boneOne != null && _boneTwo != null)
            {
                bool checker = _stateExpression(_boneOne, _boneTwo);
                if (checker)
                {
                    if (startTime == null) startTime = DateTime.Now;
                    else
                    {
                        _currentDuration = (float)(DateTime.Now - startTime).Value.TotalMilliseconds;
                    }

                    if (_currentDuration >= _durationUntilSwitch)
                    {
                        IsCurrentValueOfActiveType = !IsCurrentValueOfActiveType;
                        if (IsCurrentValueOfActiveType)
                        {
                            foreach (Action<uint> act in _functions)
                            {
                                act(_activeValue);
                            }
                            _currentDuration = 0.0f;
                            startTime = null;
                        }
                        else {
                            foreach (Action<uint> act in _functions)
                            {
                                act(_inactiveValue);
                            }
                        }
                    }
                }
                else
                {
                    _currentDuration = 0.0f;
                    startTime = null;
                }
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID + " - could not be found (is null)!");
            }
        }
    }

    public class VirtualVoidFuncSwitchOverTimeByTwoBones : VirtualControllerBaseClass {

        private String _boneNameOne;
        private String _boneNameTwo;
        //private String _switchID;
        private Func<MoCapBone, MoCapBone, bool> _stateExpression;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Action[] _functions;
        private float _durationUntilSwitch;
        private float _currentDuration = 0;
        private DateTime? startTime = null;

        public VirtualVoidFuncSwitchOverTimeByTwoBones(String switchID, String boneNameOne, String boneNameTwo, Action[] functionsToCall, Func<MoCapBone, MoCapBone, bool> expression, float durationInMilliSecondsUntilSwitch)
        {
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            _stateExpression = expression;
            _controllerID = switchID;
            _functions = functionsToCall;
            _durationUntilSwitch = durationInMilliSecondsUntilSwitch;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
            _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);

            if (_boneOne != null && _boneTwo != null)
            {
                bool checker = _stateExpression(_boneOne, _boneTwo);
                if (checker)
                {
                    if (startTime == null) startTime = DateTime.Now;
                    else
                    {
                        _currentDuration = (float)(DateTime.Now - startTime).Value.TotalMilliseconds;
                    }

                    if (_currentDuration >= _durationUntilSwitch)
                    {
                            foreach (Action act in _functions)
                            {
                                act();
                            }
                            _currentDuration = 0.0f;
                            startTime = null;
                    }
                }
                else
                {
                    _currentDuration = 0.0f;
                    startTime = null;
                }
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID + " - could not be found (is null)!");
            }
        }
    }

    public class VirtualRandomEnumSwitchOverTimeByTwoBones : VirtualControllerBaseClass {
        private String _boneNameOne;
        private String _boneNameTwo;
        //private String _switchID;
        private Func<MoCapBone, MoCapBone, bool> _stateExpression;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Action<MoCapDMXScripts.MovingHeads.MH_X25.GOBOTYPE>[] _functions;
        private MoCapDMXScripts.MovingHeads.MH_X25.GOBOTYPE[] _EnumPool;
        private float _durationUntilSwitch;
        private float _currentDuration = 0;
        private DateTime? startTime = null;

        public VirtualRandomEnumSwitchOverTimeByTwoBones(String switchID, String boneNameOne, String boneNameTwo, Action<MoCapDMXScripts.MovingHeads.MH_X25.GOBOTYPE>[] functionsToCall, Func<MoCapBone, MoCapBone, bool> expression, float durationInMilliSecondsUntilSwitch, MoCapDMXScripts.MovingHeads.MH_X25.GOBOTYPE[] GoboPool)
        {
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            _stateExpression = expression;
            _controllerID = switchID;
            _functions = functionsToCall;
            _durationUntilSwitch = durationInMilliSecondsUntilSwitch;
            _EnumPool = GoboPool;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
            _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);

            if (_boneOne != null && _boneTwo != null)
            {
                bool checker = _stateExpression(_boneOne, _boneTwo);
                if (checker)
                {
                    if (startTime == null) startTime = DateTime.Now;
                    else
                    {
                        _currentDuration = (float)(DateTime.Now - startTime).Value.TotalMilliseconds;
                    }

                    if (_currentDuration >= _durationUntilSwitch)
                    {
                        System.Random one = new System.Random();
                        int dice = one.Next(0, _EnumPool.Length);
                        foreach (Action<MoCapDMXScripts.MovingHeads.MH_X25.GOBOTYPE> act in _functions)
                        {
                            act(_EnumPool[dice]);
                        }
                        _currentDuration = 0.0f;
                        startTime = null;
                    }
                }
                else
                {
                    _currentDuration = 0.0f;
                    startTime = null;
                }
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID + " - could not be found (is null)!");
            }
        }
    }

    public class VirtualRandomEnumColorSwitchOverTimeByTwoBones : VirtualControllerBaseClass
    {
        private String _boneNameOne;
        private String _boneNameTwo;
        //private String _switchID;
        private Func<MoCapBone, MoCapBone, bool> _stateExpression;
        private MoCapBone _boneOne;
        private MoCapBone _boneTwo;
        private Action<MoCapDMXScripts.MovingHeads.MH_X25.COLOR>[] _functions;
        private MoCapDMXScripts.MovingHeads.MH_X25.COLOR[] _EnumPool;
        private float _durationUntilSwitch;
        private float _currentDuration = 0;
        private DateTime? startTime = null;

        public VirtualRandomEnumColorSwitchOverTimeByTwoBones(String switchID, String boneNameOne, String boneNameTwo, Action<MoCapDMXScripts.MovingHeads.MH_X25.COLOR>[] functionsToCall, Func<MoCapBone, MoCapBone, bool> expression, float durationInMilliSecondsUntilSwitch, MoCapDMXScripts.MovingHeads.MH_X25.COLOR[] GoboPool)
        {
            _boneNameOne = boneNameOne;
            _boneNameTwo = boneNameTwo;
            _stateExpression = expression;
            _controllerID = switchID;
            _functions = functionsToCall;
            _durationUntilSwitch = durationInMilliSecondsUntilSwitch;
            _EnumPool = GoboPool;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _boneOne = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameOne);
            _boneTwo = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneNameTwo);

            if (_boneOne != null && _boneTwo != null)
            {
                bool checker = _stateExpression(_boneOne, _boneTwo);
                if (checker)
                {
                    if (startTime == null) startTime = DateTime.Now;
                    else
                    {
                        _currentDuration = (float)(DateTime.Now - startTime).Value.TotalMilliseconds;
                    }

                    if (_currentDuration >= _durationUntilSwitch)
                    {
                        System.Random two = new System.Random();
                        int dice = two.Next(0, _EnumPool.Length);
                        foreach (Action<MoCapDMXScripts.MovingHeads.MH_X25.COLOR> act in _functions)
                        {
                            act(_EnumPool[dice]);
                        }
                        _currentDuration = 0.0f;
                        startTime = null;
                    }
                }
                else
                {
                    _currentDuration = 0.0f;
                    startTime = null;
                }
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _controllerID + " - could not be found (is null)!");
            }
        }
    }
}
