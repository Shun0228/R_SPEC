using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Microsoft.Research.Oslo;
using MathNet.Numerics.Interpolation;
using Newtonsoft.Json.Linq;

namespace SimBase_01
{
    class Dynamics
    {
        // Generate instance.
        Thrust th = new Thrust();
        Environment env = new Environment();
        Parameter param = new Parameter();
        Rotation rot = new Rotation();
        Wind wind = new Wind();

        // Define property.
        public string ThrustFilePath { set; get; }
        public string ParamFilePath { set; get; }
        public double RefWindVel { set; get; }
        public double RefWindDir { set; get; }
        public JObject GroupList { set; get; }

        private JToken rocketParam;
        private JToken motorParam;
        private JToken envParam;

        // Rocket parameter from JSON
        private double bodyLen;
        private double bodyDiam;
        private double initMass;
        private double finalMass;
        private double initLenCG;
        private double finalLenCG;
        private double initIpy;
        private double finalIpy;
        private double initIr;
        private double finalIr;
        private double lenCP;
        private double Cd;
        private double Cna;
        private double Cmq;
        private double paraVel1;
        private double paraVel2;

        // Calculated parameter.
        private double CdPara1;
        private double CdPara2;
        private double combTime;
        private double detMass;
        private double detLenCG;
        private double bodySq;
        private double paraSq;

        // Launcher parameter
        private double railLen = 5.0;

        // Interpolated items.
        private IInterpolation intpMass;
        private IInterpolation intpLenCG;
        private IInterpolation intpIpy;
        private IInterpolation intpIr;
        private IInterpolation intpCd;
        private IInterpolation intpThrust;

        /// <summary>
        /// Load parameter setting from JSON file.
        /// </summary>
        public void SetParam()
        {
            rocketParam = GroupList["Rocket"];
            motorParam = GroupList["Motor"];
            envParam = GroupList["Environment"];

            // Assign motor parameter.
            th.FilePath = motorParam["Path"].ToString();
            intpThrust = th.LoadThrust();

            // Assign rocket parameter.
            bodyLen = double.Parse(rocketParam["refLen"].ToString());
            bodyDiam = double.Parse(rocketParam["diam"].ToString());
            initMass = double.Parse(rocketParam["massInt"].ToString());
            finalMass = double.Parse(rocketParam["massEnd"].ToString());
            initLenCG = double.Parse(rocketParam["CGLenInt"].ToString());
            finalLenCG = double.Parse(rocketParam["CGLenEnd"].ToString());
            initIpy = double.Parse(rocketParam["IyzInt"].ToString());
            finalIpy = double.Parse(rocketParam["IyzEnd"].ToString());
            initIr = double.Parse(rocketParam["IxInt"].ToString());
            finalIr = double.Parse(rocketParam["IxEnd"].ToString());
            lenCP = double.Parse(rocketParam["CPLen"].ToString());
            Cd = double.Parse(rocketParam["Cd"].ToString());
            Cna = double.Parse(rocketParam["Cna"].ToString());
            Cmq = double.Parse(rocketParam["Cmq"].ToString());
            paraVel1 = double.Parse(rocketParam["vel1st"].ToString());
            //paraVel2 = double.Parse(rocketParam["vel2nd"].ToString());

            railLen = double.Parse(envParam["RailLen"].ToString());

            // Get other motor parameter
            combTime = th.GetCombTime();
            Console.WriteLine(combTime);

            // Calculate derivative.
            detMass = (initMass - finalMass) / combTime;
            detLenCG = (initLenCG - finalLenCG) / combTime;

            // Calculate other parameter.
            bodySq = 0.25 * Math.PI * Math.Pow(bodyDiam, 2);

            // Calculate parachute parameter
            paraSq = 1.0;
            CdPara1 = (finalMass * 9.81) / (0.5 * 1.25 * Math.Pow(paraVel1, 2) * paraSq);
            //CdPara2 = (finalMass * 9.81) / (0.5 * 1.25 * Math.Pow(paraVel2, 2) * paraSq);

            // Calculate time interpolated parameter.
            var timeList = new List<double>() { 0.0, combTime, 100.0 };
            var massList = new List<double>() { initMass, finalMass, finalMass };
            var lenCGList = new List<double>() { initLenCG, finalLenCG, finalLenCG };
            var IpyList = new List<double>() { initIpy, finalIpy, finalIpy };
            var IrList = new List<double>() { initIr, finalIr, finalIr };

            intpMass = param.IntpParam(timeList, massList);
            intpLenCG = param.IntpParam(timeList, lenCGList);
            intpIpy = param.IntpParam(timeList, IpyList);
            intpIr = param.IntpParam(timeList, IrList);

            // Calculate Mach interpolated parameter.
            var machList = new List<double>() { 0.0, 1.0 };
            var CdList = new List<double>() { Cd, Cd };

            intpCd = param.IntpParam(machList, CdList);
        }

        public void SetWind(double windVel, double windDir)
        {
            RefWindVel = windVel;
            RefWindDir = windDir;
            wind.SetWindVel = RefWindVel;
            wind.SetWindDir = RefWindDir;
        }

        public Vector Func(double time, Vector x)
        {
            // Assign solved parameters.
            Vector3D pos = new Vector3D(x[0], x[1], x[2]);
            Vector3D vel = new Vector3D(x[3], x[4], x[5]);
            Vector3D angVel = new Vector3D(x[6], x[7], x[8]);
            Quaternion quat = new Quaternion(x[9], x[10], x[11], x[12]);

            // Normalize quaternion.
            quat.Normalize();

            // Define derivative.
            var dx = Vector.Zeros(x.Length);

            // Define local parameters.
            double alpha, beta;
            double localCd;
            double localCnp, localCny;
            double localCmqp, localCmqy;
            double thrustForce, dragForce;
            double normalForceY, normalForceZ;
            double forceMomY, forceMomZ;

            // Calculate parameter from interpolated item
            double mass = intpMass.Interpolate(time);
            double lenCG = intpLenCG.Interpolate(time);
            double Ipy = intpIpy.Interpolate(time);
            double Ir = intpIr.Interpolate(time);

            // Define parameter which use in dynamics.
            Vector3D localForceBody = new Vector3D();
            Vector3D forceBody = new Vector3D();
            Vector3D dampMom = new Vector3D();
            Vector3D momentBody = new Vector3D();
            Vector3D dVel = new Vector3D();

            // Calculate gravity and gas parameter.
            double gravityAcc = env.GetGravityAcc(pos.Z);
            Vector3D gVec = new Vector3D(0.0, 0.0, gravityAcc);

            // Calculate gas parameter.
            (double temp, double pres, double dens)
                = env.GetGasParam(pos.Z, gravityAcc);
            double soundVel = env.GetSoundVel(temp);

            // Calculate wind vector.
            Vector3D windVec = wind.GetWindVec(pos.Z);

            // Calculate DCM from Quaternion.
            var dcmMat = rot.QuatToDcm(quat);

            // Set DCM.Transpose
            Matrix3D dcmMatInv = new Matrix3D();
            dcmMatInv = dcmMat;
            dcmMatInv.Invert();

            // Convert velocity to Body Coordinate by using DCM.
            var relativeVec = vel - windVec;
            var velBody = rot.ConvVec(dcmMat, relativeVec);
            var velBodyNorm = velBody.Length;

            // Calculate local angle of attack.
            if (velBodyNorm > 0.0)
            {
                alpha = Math.Atan2(velBody.Z, velBody.X);
                beta = Math.Atan2(velBody.Y, velBody.X);
            }
            else
            {
                alpha = 0.0;
                beta = 0.0;
            }

            Console.WriteLine("GlobalVel:{0}", vel);
            Console.WriteLine("BodyVel:{0}", velBody);
            Console.WriteLine("a:{0}, b:{1}", alpha, beta);

            // Aerodynamic coefficient condition
            //if (pos.Length <= railLen && vel.Z >= 0.0)
            if (pos.Length <= railLen)
                {
                // On launcher rail (XY constraint).
                localCd = intpCd.Interpolate(velBodyNorm / soundVel);
                localCnp = 0.0;
                localCny = 0.0;
                localCmqp = 0.0;
                localCmqy = 0.0;
            }
            else
            {
                // Coasting phase (free constraint).
                localCd = intpCd.Interpolate(velBodyNorm / soundVel);
                localCnp = Cna * beta;
                localCny = Cna * alpha;
                localCmqp = Cmq;
                localCmqy = Cmq;

            }

            // Set thrust force.
            if (time <= combTime)
            {
                thrustForce = intpThrust.Interpolate(time);
            }
            else
            {
                thrustForce = 0.0;
            }

            // Calculate each forces.
            dragForce = 0.5 * dens * velBody.LengthSquared * bodySq * localCd;
            normalForceY = -0.5 * dens * velBody.LengthSquared * bodySq * localCnp;
            normalForceZ = -0.5 * dens * velBody.LengthSquared * bodySq * localCny;

            localForceBody.X = thrustForce - dragForce;
            localForceBody.Y = normalForceY;
            localForceBody.Z = normalForceZ;

            // Calculate each damping moment.
            dampMom.X = 0.0;
            dampMom.Y = 0.25 * dens * velBody.Length * bodySq * Math.Pow(bodyLen, 2) * localCmqp * angVel.Y;
            dampMom.Z = 0.25 * dens * velBody.Length * bodySq * Math.Pow(bodyLen, 2) * localCmqy * angVel.Z;

            // Calculate each force moment.
            forceMomY = localForceBody.Z * (lenCP - lenCG);
            forceMomZ = localForceBody.Y * (lenCP - lenCG);

            // Merge each moments.
            momentBody.X = dampMom.X;
            momentBody.Y = dampMom.Y + forceMomY;
            momentBody.Z = dampMom.Z - forceMomZ;

            // Merge gravity force to forceBody.
            forceBody = localForceBody + mass * rot.ConvVec(dcmMat, gVec);

            // Launcher constraint setting
            if ((pos.Length < railLen) && (velBody.X >= 0.0))
            {
                // Launcher constraint
                forceBody.Y = 0.0;
                forceBody.Z = 0.0;
                momentBody = new Vector3D();

                if (forceBody.X < 0.0)
                {
                    forceBody.X = 0.0;
                }

            }

            // Calculate each derivative of velocity.
            var globalForce = rot.ConvVecInv(dcmMat, forceBody);
            dVel = Vector3D.Divide(globalForce, mass);

            // Calculate derivative of Quaternion.
            var dQuat = rot.GetDerivQuat(angVel, quat);

            // Assign each parameter to dx Vector.
            dx[0]  = vel.X;
            dx[1]  = vel.Y;
            dx[2]  = vel.Z;
            dx[3]  = dVel.X;
            dx[4]  = dVel.Y;
            dx[5]  = dVel.Z;
            dx[6]  = momentBody.X / Ir;
            dx[7]  = momentBody.Y / Ipy;
            dx[8]  = momentBody.Z / Ipy;
            dx[9]  = dQuat.X;
            dx[10] = dQuat.Y;
            dx[11] = dQuat.Z;
            dx[12] = dQuat.W;

            return dx;
        }

        /// <summary>
        /// Vector function to solve deployment phase.
        /// </summary>
        public Vector DeployEq(double time, Vector x)
        {
            // Assign solved parameters.
            Vector3D pos = new Vector3D(x[0], x[1], x[2]);
            Vector3D vel = new Vector3D(x[3], x[4], x[5]);
            Vector3D angVel = new Vector3D(x[6], x[7], x[8]);
            Quaternion quat = new Quaternion(x[9], x[10], x[11], x[12]);

            // Normalize quaternion.
            quat.Normalize();

            // Define derivative.
            var dx = Vector.Zeros(x.Length);

            // Calculate parameter from interpolated item
            double mass = intpMass.Interpolate(time);
            double lenCG = intpLenCG.Interpolate(time);
            double Ipy = intpIpy.Interpolate(time);
            double Ir = intpIr.Interpolate(time);

            // Define parameter which use in dynamics.
            Vector3D globalForce = new Vector3D();
            Vector3D dVel = new Vector3D();

            // Calculate gravity and gas parameter.
            double gravityAcc = env.GetGravityAcc(pos.Y);
            Vector3D gVec = new Vector3D(0.0, 0.0, gravityAcc);

            // Calculate gas parameter.
            (double temp, double pres, double dens)
                = env.GetGasParam(pos.Z, gravityAcc);
            double soundVel = env.GetSoundVel(temp);

            // Calculate wind vector.
            Vector3D windVec = wind.GetWindVec(pos.Z);
            var relativeVec = vel - windVec;

            // Calculate parachute drag
            var dragForce = 0.5 * dens * relativeVec.LengthSquared * paraSq * CdPara1;

            // Convert to gloval coordinate.
            var vecCoef = -1.0 * dragForce / relativeVec.Length; 
            globalForce = Vector3D.Multiply(vecCoef, relativeVec) + Vector3D.Multiply(mass, gVec);
            dVel = Vector3D.Divide(globalForce, mass);

            // Assign each parameter to dx Vector.
            dx[0] = vel.X;
            dx[1] = vel.Y;
            dx[2] = vel.Z;
            dx[3] = dVel.X;
            dx[4] = dVel.Y;
            dx[5] = dVel.Z;
            dx[6] = 0.0;
            dx[7] = 0.0;
            dx[8] = 0.0;
            dx[9] = 0.0;
            dx[10] = 0.0;
            dx[11] = 0.0;
            dx[12] = 0.0;

            return dx;
        }


        /// <summary>
        /// Vector function to solve.
        /// This is the equation of throwing ball.
        /// </summary>
        public Vector f(double time, Vector x)
        {
            // Assign states
            var xAcc = x[0];
            var yAcc = x[1];
            var xVel = x[2];
            var yVel = x[3];
            var xPos = x[4];
            var yPos = x[5];
            var dx = Vector.Zeros(x.Length);

            // Get environment parameter.
            double gravityAcc = env.GetGravityAcc(yPos);
            (double temp, double pres, double dens)
                = env.GetGasParam(yPos, gravityAcc);

            //

            // Set thrust force.
            var testThrust = intpThrust.Interpolate(time);
            //Console.WriteLine("Thrust intp : {0}", testThrust);

            // Assign derivatives (rocket dynamics)
            dx[0] = 0.0;
            dx[1] = -9.81;
            dx[2] = xAcc;
            dx[3] = yAcc;
            dx[4] = xVel;
            dx[5] = yVel;

            return dx;
        }
    }
}
