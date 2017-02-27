﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoCapDMXScripts.VirtualController
{
    public class VirtualControllerBaseClass
    {
        public bool IsEnabled;
        protected String _controllerID;

        public VirtualControllerBaseClass() {
        }

        public virtual void Execute()
        {
        }
        public void SetActiveState(bool isActive) {
            IsEnabled = isActive;
        }

        public override string ToString()
        {
            return "Viritual Controller: " + _controllerID;
        }
    }


}
