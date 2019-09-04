using System;
using System.Windows.Media.Media3D;
using System.Linq;
using MathNet.Numerics;

namespace SimBase_01
{
    class Rotation
    {
        public Quaternion GetDerivQuat(Vector3D angVel, Quaternion quat)
        {
            var derivQuat = new Quaternion();

            // Calculate matrix manually.
            derivQuat.X = 0.5 * ( angVel.Z * quat.Y - angVel.Y * quat.Z + angVel.X * quat.W);
            derivQuat.Y = 0.5 * (-angVel.Z * quat.X + angVel.X * quat.Z + angVel.Y * quat.W);
            derivQuat.Z = 0.5 * ( angVel.Y * quat.X - angVel.X * quat.Y + angVel.Z * quat.W);
            derivQuat.W = 0.5 * (-angVel.X * quat.X - angVel.Y * quat.Y - angVel.Z * quat.Z);

            return derivQuat;
        }

        public Matrix3D QuatToDcm(Quaternion q)
        {
            var dcmMat = new Matrix3D();

            dcmMat.M11 = Math.Pow(q.X, 2.0) - Math.Pow(q.Y, 2.0) - Math.Pow(q.Z, 2.0) + Math.Pow(q.W, 2.0);
            dcmMat.M12 = 2.0 * (q.X * q.Y + q.Z * q.W);
            dcmMat.M13 = 2.0 * (q.Z * q.X - q.Y * q.W);
            dcmMat.M14 = 0.0;

            dcmMat.M21 = 2.0 * (q.X * q.Y - q.Z * q.W);
            dcmMat.M22 = Math.Pow(q.Y, 2.0) - Math.Pow(q.Z, 2.0) - Math.Pow(q.X, 2.0) + Math.Pow(q.W, 2.0);
            dcmMat.M23 = 2.0 * (q.Y * q.Z + q.X * q.W);
            dcmMat.M24 = 0.0;

            dcmMat.M31 = 2.0 * (q.Z * q.X + q.Y * q.W);
            dcmMat.M32 = 2.0 * (q.Y * q.Z - q.X * q.W);
            dcmMat.M33 = Math.Pow(q.Z, 2.0) - Math.Pow(q.X, 2.0) - Math.Pow(q.Y, 2.0) + Math.Pow(q.W, 2.0);
            dcmMat.M34 = 0.0;

            dcmMat.OffsetX = 0.0;
            dcmMat.OffsetY = 0.0;
            dcmMat.OffsetZ = 0.0;
            dcmMat.M44 = 1.0;

            return dcmMat;
        }

        public Vector3D ConvVec(Matrix3D dcm, Vector3D baseVec)
        {
            // DCM * Vector
            var xVec = dcm.M11 * baseVec.X + dcm.M12 * baseVec.Y + dcm.M13 * baseVec.Z;
            var yVec = dcm.M21 * baseVec.X + dcm.M22 * baseVec.Y + dcm.M23 * baseVec.Z;
            var zVec = dcm.M31 * baseVec.X + dcm.M32 * baseVec.Y + dcm.M33 * baseVec.Z;

            var convVec = new Vector3D(xVec, yVec, zVec);

            return convVec;
        }

        public Vector3D ConvVecInv(Matrix3D dcm, Vector3D baseVec)
        {
            // DCM * Vector
            var xVec = dcm.M11 * baseVec.X + dcm.M21 * baseVec.Y + dcm.M31 * baseVec.Z;
            var yVec = dcm.M12 * baseVec.X + dcm.M22 * baseVec.Y + dcm.M32 * baseVec.Z;
            var zVec = dcm.M13 * baseVec.X + dcm.M23 * baseVec.Y + dcm.M33 * baseVec.Z;

            var convVec = new Vector3D(xVec, yVec, zVec);

            return convVec;
        }

        public Quaternion DcmToQuat(Matrix3D dcm)
        {
            var quat = new Quaternion();
            var temp = new double[4];

            temp[0] = 0.5 * Math.Sqrt(1.0 + Math.Pow(dcm.M11, 2.0) - Math.Pow(dcm.M22, 2.0) - Math.Pow(dcm.M33, 2.0));
            temp[1] = 0.5 * Math.Sqrt(1.0 - Math.Pow(dcm.M11, 2.0) + Math.Pow(dcm.M22, 2.0) - Math.Pow(dcm.M33, 2.0));
            temp[2] = 0.5 * Math.Sqrt(1.0 - Math.Pow(dcm.M11, 2.0) - Math.Pow(dcm.M22, 2.0) + Math.Pow(dcm.M33, 2.0));
            temp[3] = 0.5 * Math.Sqrt(1.0 + Math.Pow(dcm.M11, 2.0) + Math.Pow(dcm.M22, 2.0) + Math.Pow(dcm.M33, 2.0));

            var qMax = temp.Max();

            if (temp[0] == qMax)
            {
                quat.X = qMax;
                quat.Y = 0.25 * (1.0 / qMax) * (dcm.M12 + dcm.M21);
                quat.Z = 0.25 * (1.0 / qMax) * (dcm.M13 + dcm.M31);
                quat.W = 0.25 * (1.0 / qMax) * (dcm.M23 - dcm.M32);
            }
            else if (temp[1] == qMax)
            {
                quat.X = 0.25 * (1.0 / qMax) * (dcm.M12 + dcm.M21);
                quat.Y = qMax;
                quat.Z = 0.25 * (1.0 / qMax) * (dcm.M32 + dcm.M23);
                quat.W = 0.25 * (1.0 / qMax) * (dcm.M31 - dcm.M13);
            }
            else if (temp[2] == qMax)
            {
                quat.X = 0.25 * (1.0 / qMax) * (dcm.M31 + dcm.M13);
                quat.Y = 0.25 * (1.0 / qMax) * (dcm.M32 + dcm.M23);
                quat.Z = qMax;
                quat.W = 0.25 * (1.0 / qMax) * (dcm.M12 - dcm.M21);
            }
            else
            {
                quat.X = 0.25 * (1.0 / qMax) * (dcm.M23 - dcm.M32);
                quat.Y = 0.25 * (1.0 / qMax) * (dcm.M31 - dcm.M13);
                quat.Z = 0.25 * (1.0 / qMax) * (dcm.M12 - dcm.M21);
                quat.W = qMax;
            }

            return quat;
        }

    }
}
