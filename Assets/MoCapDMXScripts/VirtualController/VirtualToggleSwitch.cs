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
        private String _switchID;
        private Func<MoCapBone, bool> _stateExpression;
        private VirtualControllerBaseClass _switch;
        private MoCapBone _bone;

        public VirtualToggleSwitchByOneBone(String switchID,String boneName, Func<MoCapBone, bool> expression, VirtualControllerBaseClass virtualControllerToSwitch) {
            _boneName = boneName;
            _stateExpression = expression;
            _switch = virtualControllerToSwitch;
            _switchID = switchID;
            VirtualControllerCollection.Instance.Add(this);
        }

        public override void Execute()
        {
            _bone = CurrentMoCapFrame.Instance.bones.Find(x => x.Name == _boneName);
            Debug.Log("Bone Y: " + _bone.PositionInCentimeter.y.ToString());
            if (_bone != null) {
                _switch.IsEnabled  = _stateExpression(_bone);
                Debug.Log("SwitchState = " + _switch);
            }
            else
            {
                Debug.Log("Bone for VirtualSwitch - " + _switchID +  " - could not be found (is null)!");
            }
        }
    }
}
