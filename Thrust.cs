using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Interpolation;

namespace SimBase_01
{
    class Thrust
    {
        public string FilePath { set; get; }
        public string IntpMethod { set; get; } = "linear";
        public IInterpolation IntpThrust { set; get; }

        public double TimeInterval { set; get; }
        public double AveThrust { set; get; }
        public double MaxThrust { set; get; }
        public double Impulse { set; get; }


        double[] timeArray;
        double[] forceArray;

        /// <summary>
        /// <para> 1. Load thrust data (time, force). </para>
        /// <para> 2. Generate interpolated thrust curve. </para>
        /// </summary>
        public IInterpolation LoadThrust()
        {
            var timeList = new List<double>();
            var forceList = new List<double>();
            IInterpolation intpThrust;

            StreamReader sr = new StreamReader(FilePath, Encoding.GetEncoding("SHIFT_JIS"));
            var fileExtension = System.IO.Path.GetExtension(FilePath);

            // Open thrust file.
            while (sr.EndOfStream == false)
            {
                string line = sr.ReadLine();
                string[] splitData = null;

                if (fileExtension == ".txt")
                {
                    splitData = line.Split('\t');
                }
                else if (fileExtension == ".csv")
                {
                    splitData = line.Split(',');
                }

                // Omit comment lines
                if (double.TryParse(splitData[0], out double time)
                    && double.TryParse(splitData[1], out double force))
                {
                    timeList.Add(time);
                    forceList.Add(force);
                }
            }

            // Convert List to Array.
            int listLen = timeList.Count;
            timeArray = new double[listLen];
            forceArray = new double[listLen];

            timeArray = timeList.ToArray();
            forceArray = forceList.ToArray();

            // Select interpolation method.
            switch (IntpMethod)
            {
                case "linear":
                    intpThrust = Interpolate.Linear(timeArray, forceArray);
                    break;

                case "cubic":
                    intpThrust = Interpolate.CubicSpline(timeArray, forceArray);
                    break;

                default:
                    intpThrust = Interpolate.Linear(timeArray, forceArray);
                    break;
            }

            return intpThrust;
        }

        public double GetCombTime()
        {
            return timeArray.Last();
        }

        public void CalcThrustParam(List<double> timeList, List<double> forceList)
        {
            // ERROR
            TimeInterval = timeList[1] - timeList[0];
            AveThrust = forceList.Mean();
            MaxThrust = forceList.Max();

            SetIntpThrust(timeList, forceList);
            var timeInit = 0.0;
            var timeEnd = timeList.Max();
            var subdiv = 1000;
            Impulse = MathNet.Numerics.Integration.SimpsonRule.IntegrateComposite(ThrustFunc, timeInit, timeEnd, subdiv);
        }

        private void SetIntpThrust(List<double> timeList, List<double> forceList)
        {
            var dataNum = timeList.Count;
            var timeArray = timeList.ToArray();
            var forceArray = forceList.ToArray();

            // Select interpolation method.
            switch (IntpMethod)
            {
                case "linear":
                    IntpThrust = Interpolate.Linear(timeArray, forceArray);
                    break;

                case "cubic":
                    IntpThrust = Interpolate.CubicSpline(timeArray, forceArray);
                    break;

                default:
                    IntpThrust = Interpolate.Linear(timeArray, forceArray);
                    break;
            }
        }

        private double ThrustFunc(double time)
        {
            var force = IntpThrust.Interpolate(time);
            return force;
        }
    }
}
