using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoCapDMXScripts.VirtualController
{
    public class VirtualControllerCollection : List<VirtualControllerBaseClass>
    {
        private static VirtualControllerCollection instance;

        private VirtualControllerCollection() { }

        public static VirtualControllerCollection Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VirtualControllerCollection();
                }
                return instance;
            }
        }

        public static void ExecuteAllControllers()
        {
            LogUtility.LogToFile("ExcecuteAllControllers started! ControllerCount: " + VirtualControllerCollection.Instance.Count);
            foreach (VirtualControllerBaseClass controller in Instance) {
                controller.Execute();
            }
            LogUtility.LogToFile("ExcecuteAllControllers finished! ControllerCount: " + VirtualControllerCollection.Instance.Count);
        }
    }
}
