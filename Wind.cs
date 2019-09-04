using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Media.Media3D;


namespace SimBase_01
{
    class Wind
    {
        // Define property.
        public double SetWindVel { set; get; }
        public double SetWindDir { set; get; }

        public Vector3D GetWindVec(double altitude)
        {
            // Define variety.
            double windVelAbs;

            // Define parameter.
            double powCoef = 6.0;
            double refAlt = 2.0;

            // Compute wind velocity.
            if (altitude > 0.0)
            {
                windVelAbs = SetWindVel * Math.Pow((altitude / refAlt), (1.0 / powCoef));
            }
            else
            {
                windVelAbs = 0.0;
            }

            // Compute wind xyz
            var xWind = windVelAbs * Math.Cos(ToRad(SetWindDir));
            var yWind = windVelAbs * Math.Sin(ToRad(SetWindDir));
            var zWind = 0.0;

            Vector3D windVec = new Vector3D(xWind, yWind, zWind);

            return windVec;
        }

        double ToRad(double angleDeg)
        {
            return angleDeg * Math.PI / 180.0;
        }
    }
}
