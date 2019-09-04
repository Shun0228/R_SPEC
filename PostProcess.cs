using System;
using System.Data;
using OxyPlot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Oslo;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;

namespace SimBase_01
{
    class PostProcess
    {
        // Default function (in ODE solver)
        public List<double> timeList { get; private set; }
        public List<double> xPosList { get; private set; }
        public List<double> yPosList { get; private set; }
        public List<double> zPosList { get; private set; }
        public List<double> xVelList { get; private set; }
        public List<double> yVelList { get; private set; }
        public List<double> zVelList { get; private set; }
        public List<double> xAngList { get; private set; }
        public List<double> yAngList { get; private set; }
        public List<double> zAngList { get; private set; }
        public List<double> xQuatList { get; private set; }
        public List<double> yQuatList { get; private set; }
        public List<double> zQuatList { get; private set; }
        public List<double> wQuatList { get; private set; }

        // User define function
        public List<double> alphaList { get; private set; }
        public List<double> betaList { get; private set; }
        public List<double> velBxList { get; private set; }
        public List<double> velByList { get; private set; }
        public List<double> velBzList { get; private set; }
        public List<double> machBxList { get; private set; }
        public List<double> machByList { get; private set; }
        public List<double> machBzList { get; private set; }

        // Max value
        public double maxVelBx { get; private set; }
        public double maxAlt { get; private set; }
        public double maxMach { get; private set; }

        // To store in csv files
        public List<double[]> allResult { get; private set; }

        // For MapPost
        private List<DropPoint> dropPosList;

        // For Table Post
        private DataTable BaseTable;
        public DataTable MaxVelTable { get; private set; }
        public DataTable MaxAltTable { get; private set; }
        public DataTable MaxMachTable { get; private set; }
        private DataRow tempRow;
        private DataRow maxAccRow;
        private DataRow maxVelRow;
        private DataRow maxMachRow;
        private DataRow maxAltRow;
        public Dictionary<string, DataTable> TableDic { get; private set; }
        public ObservableCollection<string> TableComboSource { get; private set; }


        // Properties
        public IDictionary<string, List<double>> ListDic { get; private set; }
        public List<List<DropPoint>> ScatterList { get; private set; }
        public ObservableCollection<string> ResultComboSource { get; private set; }
        public Dictionary<string, string> ResultPair { get; private set; }

        public PostProcess()
        {
            // To store csv file.
            allResult = new List<double[]> { };
            ResultPair = new Dictionary<string, string> { };

            // For Line Plotting
            ResultComboSource = new ObservableCollection<string>();
            TableComboSource = new ObservableCollection<string>();

            // For MapPost
            dropPosList = new List<DropPoint> { };
            ScatterList = new List<List<DropPoint>> { };
        }

        public void InitializePost()
        {
            // Main function of ODE.
            timeList = new List<double> { };
            xPosList = new List<double> { };
            yPosList = new List<double> { };
            zPosList = new List<double> { };
            xVelList = new List<double> { };
            yVelList = new List<double> { };
            zVelList = new List<double> { };
            xAngList = new List<double> { };
            yAngList = new List<double> { };
            zAngList = new List<double> { };
            xQuatList = new List<double> { };
            yQuatList = new List<double> { };
            zQuatList = new List<double> { };
            wQuatList = new List<double> { };

            // Additional function
            alphaList = new List<double> { };
            betaList = new List<double> { };
            velBxList = new List<double> { };
            velByList = new List<double> { };
            velBzList = new List<double> { };
            machBxList = new List<double> { };
            machByList = new List<double> { };
            machBzList = new List<double> { };

            // all result
            allResult = new List<double[]> { };

            
        }

        

        public void LoadResult(List<double[]> tempResult)
        {
            foreach (var item in tempResult)
            {
                timeList.Add(item[0]);
                xPosList.Add(item[1]);
                yPosList.Add(item[2]);
                zPosList.Add(item[3]);
                xVelList.Add(item[4]);
                yVelList.Add(item[5]);
                zVelList.Add(item[6]);
                xAngList.Add(item[7]);
                yAngList.Add(item[8]);
                zAngList.Add(item[9]);
                xQuatList.Add(item[10]);
                yQuatList.Add(item[11]);
                zQuatList.Add(item[12]);
                wQuatList.Add(item[13]);
            }
        }

        public void SetListPair()
        {
            ListDic = new Dictionary<string, List<double>>();

            ListDic.Add("Time", timeList);
            ListDic.Add("X", xPosList);
            ListDic.Add("Y", yPosList);
            ListDic.Add("Z", zPosList);
            ListDic.Add("U", xVelList);
            ListDic.Add("V", yVelList);
            ListDic.Add("W", zVelList);
            ListDic.Add("P", xAngList);
            ListDic.Add("Q", yAngList);
            ListDic.Add("R", zAngList);
            ListDic.Add("QX", xQuatList);
            ListDic.Add("QY", yQuatList);
            ListDic.Add("QZ", zQuatList);
            ListDic.Add("QW", wQuatList);
            ListDic.Add("Alpha", alphaList);
            ListDic.Add("Beta", betaList);
            ListDic.Add("Vbx", velBxList);
            ListDic.Add("Vby", velByList);
            ListDic.Add("Vbz", velBzList);
            ListDic.Add("Machbx", machBxList);
            ListDic.Add("Machby", machByList);
            ListDic.Add("Machbz", machBzList);

        }

        public void AddResult(SolPoint sol)
        {
            timeList.Add(sol.T);
            xPosList.Add(sol.X[0]);
            yPosList.Add(sol.X[1]);
            zPosList.Add(sol.X[2]);
            xVelList.Add(sol.X[3]);
            yVelList.Add(sol.X[4]);
            zVelList.Add(sol.X[5]);
            xAngList.Add(sol.X[6]);
            yAngList.Add(sol.X[7]);
            zAngList.Add(sol.X[8]);
            xQuatList.Add(sol.X[9]);
            yQuatList.Add(sol.X[10]);
            zQuatList.Add(sol.X[11]);
            wQuatList.Add(sol.X[12]);

            var testArr = new double[14];
            testArr[0] = sol.T;
            for (int i=0; i<=12; i++)
            {
                testArr[i+1] = sol.X[i];
            }

            allResult.Add(testArr);
            
        }

        public void CalcExtra()
        {
            Rotation rot = new Rotation();
            Environment env = new Environment();

            for (int i = 0; i < timeList.Count; i++)
            {
                double alpha, beta;

                // Calculate gas parameter.
                var gravityAcc = env.GetGravityAcc(zPosList[i]);
                (var temp, var pres, var dens)
                    = env.GetGasParam(zPosList[i], gravityAcc);
                var soundVel = env.GetSoundVel(temp);

                // Set each parameter to Vector.
                var velGlobal = new Vector3D(xVelList[i], yVelList[i], zVelList[i]);
                var quat = new Quaternion(xQuatList[i], yQuatList[i], zQuatList[i], wQuatList[i]);

                // Calculate DCM from Quaternion.
                Matrix3D dcmMat = new Matrix3D();
                dcmMat = rot.QuatToDcm(quat);
                var velBody = rot.ConvVec(dcmMat, velGlobal);
                var machBody = Vector3D.Divide(velBody, soundVel);


                if (velBody.Length >= 0.0)
                {
                    alpha = Math.Atan2(velBody.Z, velBody.X);
                    beta = Math.Atan2(velBody.Y, velBody.X);
                }
                else
                {
                    alpha = 0.0;
                    beta = 0.0;
                }

                alpha = (alpha / Math.PI) * 180.0;
                beta  = (beta  / Math.PI) * 180.0;

                alphaList.Add(alpha);
                betaList.Add(beta);
                velBxList.Add(velBody.X);
                velByList.Add(velBody.Y);
                velBzList.Add(velBody.Z);
                machBxList.Add(machBody.X);
                machByList.Add(machBody.Y);
                machBzList.Add(machBody.Z);
            }
        }

        public void CalcMaxValue()
        {
            maxVelBx = velBxList.Max();
            maxAlt = zPosList.Max();
            maxMach = machBxList.Max();
        }

        public double GetDeployInitTime()
        {
            // Get list index of max altitude.
            var maxAlt = zPosList.Max();
            var indexNum = zPosList.IndexOf(maxAlt);

            // Get initial time from trajectory result.
            var time = timeList[indexNum];

            return time;
        }

        public Vector GetDeployInitVec()
        {
            // Get list index of max altitude.
            var maxAlt = zPosList.Max();
            var indexNum = zPosList.IndexOf(maxAlt);

            // Set initial vector from trajectory result.
            var vecFunc = new string[]
            {
                "posX", "posY", "posZ",
                "velX", "velY", "velZ",
                "angVelRoll", "angVelPitch", "angVelYaw",
                "quatX", "quatY", "quatZ", "quatW"
            };

            int n = vecFunc.Length;
            Vector x0 = Vector.Zeros(n);

            x0[0] = xPosList[indexNum];
            x0[1] = yPosList[indexNum];
            x0[2] = zPosList[indexNum];
            x0[3] = xVelList[indexNum];
            x0[4] = yVelList[indexNum];
            x0[5] = zVelList[indexNum];
            x0[6] = xAngList[indexNum];
            x0[7] = yAngList[indexNum];
            x0[8] = zAngList[indexNum];
            x0[9] = xQuatList[indexNum];
            x0[10] = yQuatList[indexNum];
            x0[11] = zQuatList[indexNum];
            x0[12] = wQuatList[indexNum];

            return x0;
        }

        public void AddDropPoint()
        {
            var dropX = xPosList.Last();
            var dropY = yPosList.Last();

            var dp = new DropPoint(dropX, dropY);

            dropPosList.Add(dp);
        }

        public void AddDropList()
        {
            dropPosList.Add(dropPosList.First());
            ScatterList.Add(dropPosList);

            // Initialize automatically
            dropPosList = new List<DropPoint> { };
        }

        public void SaveResultCsv(string calcMode, string tempDir, double wVel, double wDir)
        {
            if (Directory.Exists(tempDir) == false)
            {
                Directory.CreateDirectory(tempDir);
            }

            var csvName = GetCsvName(calcMode, wVel, wDir);
            var csvPath = tempDir + "\\" + csvName;

            try
            {
                using (var sw = new StreamWriter(csvPath, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    foreach (var arr in allResult)
                    {
                        var line = string.Join(",", arr);
                        sw.WriteLine(line);
                    }
                }
            }
            catch (System.Exception ex) { }

            this.SetCsvPair(csvPath, calcMode, wVel, wDir);
        }

        private string GetCsvName(string calcMode, double wVel, double wDir)
        {
            var csvName = calcMode + "_wVel_" + wVel.ToString() + "_wDir_" + wDir.ToString() + ".csv";

            return csvName;
        }

        public void SetCsvPair(string csvPath, string calcMode, double wVel, double wDir)
        {
            var caseName = calcMode + ", WindVel:" + wVel + "m/s, WindDir:" + wDir + "deg";
            ResultComboSource.Add(caseName);
            ResultPair.Add(caseName, csvPath);
        }

        public DropPoint GetDropPoint()
        {
            var dropX = xPosList.Last();
            var dropY = yPosList.Last();
            var dp = new DropPoint(dropX, dropY);

            return dp;
        }

        public List<DataPoint> ToDataSet(List<double> xList, List<double> yList)
        {
            var dataPoints = new List<DataPoint> { };

            for (int i = 0; i < timeList.Count; i++)
            {
                var dataSet = new DataPoint(xList[i], yList[i]);
                dataPoints.Add(dataSet);
            }

            return dataPoints;
        }

        // Table relate
        
        public void InitializeTable()
        {
            // For Table post
            MaxVelTable = new DataTable();
            MaxAltTable = new DataTable();
            MaxMachTable = new DataTable();
            TableComboSource.Clear();

            MaxVelTable.Columns.Add("RowHeader");
            MaxAltTable.Columns.Add("RowHeader");
            MaxMachTable.Columns.Add("RowHeader");
        }

        

        public void AddColumnHeader(double windDir)
        {
            //var col = windDir.ToString("F1") + " deg";
            var col = windDir.ToString() + " deg";
            MaxVelTable.Columns.Add(col);
            MaxAltTable.Columns.Add(col);
            MaxMachTable.Columns.Add(col);
        }

        public void NewRowHeader(double windVel)
        {
            maxVelRow = MaxVelTable.NewRow();
            maxAltRow = MaxAltTable.NewRow();
            maxMachRow = MaxMachTable.NewRow();

            maxVelRow[0] = windVel.ToString("F1") + " m/s";
            maxAltRow[0] = windVel.ToString("F1") + " m/s";
            maxMachRow[0] = windVel.ToString("F1") + " m/s";

            //maxVelRow[0] = windVel.ToString() + " m/s";
            //maxAltRow[0] = windVel.ToString() + " m/s";
            //maxMachRow[0] = windVel.ToString() + " m/s";
        }

        public void AddMaxVal(int indexNum, double MaxVal)
        {
            maxVelRow[indexNum] = maxVelBx;
            maxAltRow[indexNum] = maxAlt;
            maxMachRow[indexNum] = maxMach;
        }

        public void AddRowTable()
        {
            MaxVelTable.Rows.Add(maxVelRow);
            MaxAltTable.Rows.Add(maxAltRow);
            MaxMachTable.Rows.Add(maxMachRow);
        }

        public void SetTablePair()
        {
            TableDic = new Dictionary<string, DataTable>();

            TableDic.Add("Max Velocity [m/s]", MaxVelTable);
            TableDic.Add("Max Altitude [m]", MaxAltTable);
            TableDic.Add("Max Mach Number [-]", MaxMachTable);

            foreach (var table in TableDic)
            {
                var tableName = table.Key;
                TableComboSource.Add(tableName);
            }
        }

    }

}
