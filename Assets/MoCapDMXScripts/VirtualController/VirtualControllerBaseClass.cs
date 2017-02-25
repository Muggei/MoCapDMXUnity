using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoCapDMXScripts.VirtualController
{
    public class VirtualControllerBaseClass
    {
        protected bool IsEnabled;

        public VirtualControllerBaseClass() {
        }

        public virtual void Execute()
        {
        }
        public void SetActiveState(bool isActive) {
            IsEnabled = isActive;
        }
    }


}
