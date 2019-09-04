using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Oslo;
using System.Windows.Media.Media3D;
using Newtonsoft.Json.Linq;

namespace SimBase_01
{
    class Initital
    {
        // Define property.
        public double RailElev { set; get; }
        public double RailAzi { set; get; }
        public JObject GroupList { set; get; }

        public void SetRailParam()
        {
            var envParam = GroupList["Environment"];

            RailElev = double.Parse(envParam["RailElev"].ToString());
            RailAzi = double.Parse(envParam["RailAzi"].ToString());
        }

        public Vector GetInitVec()
        {
            // Set calculated functions.
            var vecFunc = new string[]
            {
                "posX", "posY", "posZ",
                "velX", "velY", "velZ",
                "angVelRoll", "angVelPitch", "angVelYaw",
                "quatX", "quatY", "quatZ", "quatW"
            };

            int n = vecFunc.Length;
            Vector x0 = Vector.Zeros(n);

            // Set initial value.
            var posX = x0[0] = 0.0;
            var posY = x0[1] = 0.0;
            var posZ = x0[2] = 0.0;
            var velX = x0[3] = 0.0;
            var velY = x0[4] = 0.0;
            var velZ = x0[5] = 0.0;
            var angVelRoll = x0[6] = 0.0;
            var angVelPitch = x0[7] = 0.0;
            var angVelYaw = x0[8] = 0.0;

            // Set initial quaternion by launcher setting.

            ////// Set XY angle and set vector. (ENU)
            //double xVec = 1.0 * Math.Cos((railAzi + 90.0) * Math.PI / 180.0);
            //double yVec = 1.0 * Math.Sin((railAzi + 90.0) * Math.PI / 180.0);
            //var aziVec = new Vector3D(xVec, yVec, 0.0);
            // Set elevation and rotate quaternion.
            //var initQuat = new Quaternion(aziVec, railElev);

            //var quatAzi = new Quaternion(new Vector3D(0, 0, 1), -railAzi);
            //var quatElev = new Quaternion(aziVec, railElev);
            //var initQuat = Quaternion.Multiply(quatAzi, quatElev);
            // Set elevation and rotate quaternion.
            //var initQuat = new Quaternion(aziVec, railElev);

            // Add 2018/11/03
            var theta = -1.0 * RailElev * Math.PI / 180.0;
            var psi = RailAzi * Math.PI / 180.0;

            var qTheta = Math.Acos(-1.0 * (1.0 - Math.Cos(theta) - Math.Cos(psi) - Math.Cos(theta) * Math.Cos(psi)) / 2.0);
            var qLambda0 = -1.0 * Math.Sin(theta) * Math.Sin(psi) / (2.0 * Math.Sin(qTheta));
            var qLambda1 = Math.Sin(theta) * (1.0 + Math.Cos(psi)) / (2.0 * Math.Sin(qTheta));
            var qLambda2 = Math.Sin(psi) * (1.0 + Math.Cos(theta)) / (2.0 * Math.Sin(qTheta));

            var qX = qLambda0 * Math.Sin(qTheta / 2.0);
            var qY = qLambda1 * Math.Sin(qTheta / 2.0);
            var qZ = qLambda2 * Math.Sin(qTheta / 2.0);
            var qW = Math.Cos(qTheta / 2.0);

            var initQuat = new Quaternion(qX, qY, qZ, qW);

            // Add Quat to initial vec.
            var quatX = x0[9] = initQuat.X;
            var quatY = x0[10] = initQuat.Y;
            var quatZ = x0[11] = initQuat.Z;
            var quatW = x0[12] = initQuat.W;

            return x0;
        }
    }
}
