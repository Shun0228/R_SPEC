using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimBase_01
{
    class RelateDataGrid
    {
        public ObservableCollection<Spec> BodySet { set; get; }
        public ObservableCollection<Spec> MotorSet { set; get; }
        public ObservableCollection<Spec> EnvSet { set; get; }
        public ObservableCollection<Spec> WindSet { set; get; }
        public List<string> BodyList { get; private set; }
        public List<string> MotorList { get; private set; }
        public List<string> EnvList { get; private set; }
        public List<string> WindList { get; private set; }
        public Dictionary<string, string> DescPair { get; private set; }
        public Dictionary<string, string> UnitPair { get; private set; }
        public Dictionary<string, string> InitValPair { get; private set; }
        public JsonItemSet JsonSet { set; get; }

        public string MotorPath { set; get; }
        public double WindVelInit { set; get; }
        public double WindVelFinal { set; get; }
        public double WindVelInterval { set; get; }
        public double WindDirInterval { set; get; }

        public RelateDataGrid()
        {
            // Generate Parameter dataSet
            BodySet = new ObservableCollection<Spec>();
            MotorSet = new ObservableCollection<Spec>();
            EnvSet = new ObservableCollection<Spec>();
            WindSet = new ObservableCollection<Spec>();
            JsonSet = new JsonItemSet();

            BodyList = new List<string>
            {
                "refLen", "diam", "massInt", "massEnd",
                "CGLenInt", "CGLenEnd", "IyzInt", "IyzEnd",
                "IxInt", "IxEnd", "CPLen", "Cd", "Cna", "Cmq",
                "vel1st" 
            };

            MotorList = new List<string>
            {
                "Impulese", "AverageThrust",
                "MaxThrust", "TimeInterval"
            };

            EnvList = new List<string>
            {
                "RailLen", "RailElev", "RailAzi"
                //"RefWindVelInit", "RefWindVelFinal",
                //"RefWindVelInterval", "RefWindDirInterval"
            };

            WindList = new List<string>
            {
                "RefWindVelInit", "RefWindVelFinal",
                "RefWindVelInterval", "RefWindDirInterval"
            };

            DescPair = new Dictionary<string, string> { };
            DescPair.Add("refLen", "機体全長");
            DescPair.Add("diam", "機体直径");
            DescPair.Add("massInt", "離陸時質量");
            DescPair.Add("massEnd", "燃焼終了時質量");
            DescPair.Add("CGLenInt", "離陸時重心位置");
            DescPair.Add("CGLenEnd", "燃焼終了時重心位置");
            DescPair.Add("IyzInt", "離陸時慣性モーメント(ピッチ方向)");
            DescPair.Add("IyzEnd", "燃焼終了時慣性モーメント(ピッチ方向)");
            DescPair.Add("IxInt", "離陸時慣性モーメント(ロール方向)");
            DescPair.Add("IxEnd", "燃焼終了時慣性モーメント(ロール方向)");
            DescPair.Add("CPLen", "圧力中心位置");
            DescPair.Add("Cd", "抵抗係数");
            DescPair.Add("Cna", "法線力係数傾斜");
            DescPair.Add("Cmq", "減衰モーメント係数");
            DescPair.Add("vel1st", "1段目パラシュート終端速度");
            DescPair.Add("RailLen", "ランチャレール長");
            DescPair.Add("RailElev", "ランチャレール仰角");
            DescPair.Add("RailAzi", "ランチャレール方位角");
            DescPair.Add("RefWindVelInit", "風速下限値");
            DescPair.Add("RefWindVelFinal", "風速上限値");
            DescPair.Add("RefWindVelInterval", "風速刻み");
            DescPair.Add("RefWindDirInterval", "風向刻み");
            DescPair.Add("Impulese", "総力積");
            DescPair.Add("AverageThrust", "平均推力");
            DescPair.Add("MaxThrust", "最大推力");
            DescPair.Add("TimeInterval", "時間刻み");


            UnitPair = new Dictionary<string, string> { };
            UnitPair.Add("refLen", "m");
            UnitPair.Add("diam", "m");
            UnitPair.Add("massInt", "kg");
            UnitPair.Add("massEnd", "kg");
            UnitPair.Add("CGLenInt", "m");
            UnitPair.Add("CGLenEnd", "m");
            UnitPair.Add("IyzInt", "kg*m^2");
            UnitPair.Add("IyzEnd", "kg*m^2");
            UnitPair.Add("IxInt", "kg*m^2");
            UnitPair.Add("IxEnd", "kg*m^2");
            UnitPair.Add("CPLen", "m");
            UnitPair.Add("Cd", "-");
            UnitPair.Add("Cna", "rad^(-1)");
            UnitPair.Add("Cmq", "-");
            UnitPair.Add("vel1st", "m/s");
            UnitPair.Add("RailLen", "m");
            UnitPair.Add("RailElev", "deg");
            UnitPair.Add("RailAzi", "deg");
            UnitPair.Add("RefWindVelInit", "m/s");
            UnitPair.Add("RefWindVelFinal", "m/s");
            UnitPair.Add("RefWindVelInterval", "m/s");
            UnitPair.Add("RefWindDirInterval", "deg");
            UnitPair.Add("Impulese", "N*s");
            UnitPair.Add("AverageThrust", "N");
            UnitPair.Add("MaxThrust", "N");
            UnitPair.Add("TimeInterval", "sec");

            InitValPair = new Dictionary<string, string> { };
            InitValPair.Add("RefWindVelInit", "1.0");
            InitValPair.Add("RefWindVelFinal", "2.0");
            InitValPair.Add("RefWindVelInterval", "1.0");
            InitValPair.Add("RefWindDirInterval", "45.0");
        }

        public void Initialize()
        {
            BodySet.Clear();
            foreach (var item in BodyList)
            {
                var desc = DescPair[item];
                var unit = UnitPair[item];
                var value = "";
                if (InitValPair.ContainsKey(item)) { value = InitValPair[item]; }

                BodySet.Add(new Spec(item, desc, value, unit));
            }

            EnvSet.Clear();
            foreach (var item in EnvList)
            {
                var desc = DescPair[item];
                var unit = UnitPair[item];
                var value = "";
                if (InitValPair.ContainsKey(item)) { value = InitValPair[item]; }

                EnvSet.Add(new Spec(item, desc, value, unit));
            }

            WindSet.Clear();
            foreach (var item in WindList)
            {
                var desc = DescPair[item];
                var unit = UnitPair[item];
                var value = "";
                if (InitValPair.ContainsKey(item)) { value = InitValPair[item]; }

                WindSet.Add(new Spec(item, desc, value, unit));
            }

            MotorSet.Clear();
            foreach (var item in MotorList)
            {
                var desc = DescPair[item];
                var unit = UnitPair[item];
                var value = "";
                if (InitValPair.ContainsKey(item)) { value = InitValPair[item]; }

                MotorSet.Add(new Spec(item, desc, value, unit));
            }

        }

        public void LoadItem(string loadPath)
        {
            // Initialize data
            BodySet.Clear();
            EnvSet.Clear();
            WindSet.Clear();

            // From Json to ObservableCollection
            var rawAllText = File.ReadAllText(loadPath);
            var groupList = JObject.Parse(rawAllText);

            // Set each group.
            var bodyList = groupList["Rocket"];
            var motorList = groupList["Motor"];
            var envList = groupList["Environment"];
            var windList = groupList["Wind"];

            // Add all key and value to dataSet.
            foreach (JProperty item in bodyList)
            {
                var nameDesc = DescPair[item.Name];
                var unit = UnitPair[item.Name];
                BodySet.Add(new Spec(item.Name, nameDesc, item.Value.ToString(), unit));
            }

            MotorPath = motorList["Path"].ToString();

            foreach (JProperty item in envList)
            {
                var nameDesc = DescPair[item.Name];
                var unit = UnitPair[item.Name];
                EnvSet.Add(new Spec(item.Name, nameDesc, item.Value.ToString(), unit));
            }

            foreach (JProperty item in windList)
            {
                var nameDesc = DescPair[item.Name];
                var unit = UnitPair[item.Name];
                WindSet.Add(new Spec(item.Name, nameDesc, item.Value.ToString(), unit));
            }
        }

        public void SetWindParam()
        {
            foreach (var item in EnvSet)
            {
                if (item.Name == "RefWindVelInit") { WindVelInit = double.Parse(item.Value); }
                if (item.Name == "RefWindVelFinal") { WindVelFinal = double.Parse(item.Value); }
                if (item.Name == "RefWindVelInterval") { WindVelInterval = double.Parse(item.Value); }
                if (item.Name == "RefWindDirInterval") { WindDirInterval = double.Parse(item.Value); }
            }
        }

        public JObject ToJsonFile()
        {
            // Initialize
            JsonSet = new JsonItemSet();

            // Add items to JSON file
            foreach (var item in BodySet)
            {
                JsonSet.Rocket.Add(item.Name, item.Value);
            }

            JsonSet.Motor.Add("Path", MotorPath);

            foreach (var item in EnvSet)
            {
                JsonSet.Environment.Add(item.Name, item.Value);
            }

            foreach (var item in WindSet)
            {
                JsonSet.Wind.Add(item.Name, item.Value);
            }

            var allText = JsonConvert.SerializeObject(JsonSet, Formatting.Indented);
            var jsonParse = JObject.Parse(allText);

            return jsonParse;
        }
    }
}
