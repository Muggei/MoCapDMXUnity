using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoCapDMXScripts
{
    public class MathScripts
    {
        public static int PanAndTiltAngleCalculator(float nx, float ny, float ax, float ay, float bx, float by, float newVectorX, float newVectorY)
        {
            int result = 0;

            float abx = bx - ax;
            float aby = by - ay;

            float len_ab = (float)Math.Sqrt(Math.Pow(abx, 2) + Math.Pow(aby, 2));

            float norm_abx = (1 / len_ab) * abx;
            float norm_aby = (1 / len_ab) * aby;
            newVectorX = norm_abx;
            newVectorY = norm_aby;

            result = (int)((Math.Acos(norm_abx * nx + norm_aby * ny)) * (180 / Math.PI));

            //int angleSign = ((nx * norm_aby) > (ny * norm_abx)) ? 1 : -1;
            //result *= angleSign;

            return (int)result;
        }
    }
}
