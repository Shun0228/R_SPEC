using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace SimBase_01
{
    class Parameter
    {
        private Dictionary<string, double> paramDic;

        public string FilePath { set; get; }
        public JObject LoadParamJson()
        {
            // Load parameter from JSON file
            var orgParam = File.ReadAllText(FilePath);
            JObject paramJson = JObject.Parse(orgParam);

            return paramJson;
        }

        public Dictionary<string, double> LoadDefaultParam()
        {
            var paramDic = new Dictionary<string, double>();

            paramDic.Add("mass", 1.0);

            return paramDic;
        }

        public IInterpolation IntpParam(List<double> timeList, List<double> paramList)
        {
            // var timeList = new List<double>();
            // var paramList = new List<double>();
            IInterpolation intpVar;

            // Convert List to Array.
            int listLen = timeList.Count;
            var timeArray = new double[listLen];
            var paramArray = new double[listLen];

            timeArray = timeList.ToArray();
            paramArray = paramList.ToArray();

            intpVar = Interpolate.Linear(timeArray, paramArray);

            return intpVar;
        }

    }
}
