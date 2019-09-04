using Microsoft.Research.Oslo;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SimBase_01
{
    class RunSim
    {
        // Properties
        public List<List<DropPoint>> ScatterList { get; private set; }
        public IDictionary<string, List<double>> ListDic { get; private set; }
        public ObservableCollection<string> ResultComboSource { get; private set; }
        public Dictionary<string, string> ResultPair { get; private set; }
        public List<List<DropPoint>> ParaScatterList { get; private set; }
        public IDictionary<string, List<double>> ParaListDic { get; private set; }
        public ObservableCollection<string> ParaResultComboSource { get; private set; }
        public Dictionary<string, string> ParaResultPair { get; private set; }
        
        //
        public bool IsParaDeploy { get; private set; } = false;

        // For post Table
        public DataTable MaxVelTable { get; private set; }
        public DataTable MaxAltTable { get; private set; }
        public DataTable MaxMachTable { get; private set; }
        public Dictionary<string, DataTable> TableDic { get; private set; }
        public ObservableCollection<string> TableComboSource { get; private set; }

        // Private varieties
        private double windVelInit;
        private double windVelFin;
        private double windVelInterval;
        private double windDirInterval;
        private readonly JObject allParam;
        private readonly string crtDir;
        private readonly string tempDir;

        // For MapPost
        private List<DropPoint> dropPosList;
        
        // Instance definition.
        PostProcess post = new PostProcess();
        PostProcess postPara = new PostProcess();


        public RunSim(JObject jsonParam)
        {
            allParam = jsonParam;

            // Set other parameter.
            crtDir = Directory.GetCurrentDirectory();
            tempDir = crtDir + "\\temp";

            ReadJson();
        }

        private void ReadJson()
        {
            //var envParam = allParam["Environment"];
            var windParam = allParam["Wind"];
            windVelInit = double.Parse(windParam["RefWindVelInit"].ToString());
            windVelFin = double.Parse(windParam["RefWindVelFinal"].ToString());
            windVelInterval = double.Parse(windParam["RefWindVelInterval"].ToString());
            windDirInterval = double.Parse(windParam["RefWindDirInterval"].ToString());

        }

        public void MultiExecute(IProgress<int> progress)
        {
            // For MapPost
            dropPosList = new List<DropPoint> { };
            ScatterList = new List<List<DropPoint>> { };

            // Hardcoding !
            var windDirInit = 0.0;
            var windDirFin = 360.0;

            // Compute total case number.
            var velCaseNum = Math.Floor((windVelFin - windVelInit) / windVelInterval + 1);
            var dirCaseNum = Math.Floor((windDirFin - windDirInit) / windDirInterval);
            var totalCaseNum = (int)velCaseNum * (int)dirCaseNum;
            var iter = 0;

            // Setup of post table
            post.InitializeTable();

            for (var wDir = windDirInit; wDir < windDirFin; wDir += windDirInterval)
            {
                post.AddColumnHeader(wDir);
            }

            // Table post
            MaxVelTable = post.MaxVelTable;
            MaxAltTable = post.MaxAltTable;
            MaxMachTable = post.MaxMachTable;

            // Wind parameter loop
            for (var wVel = windVelInit; wVel <= windVelFin; wVel += windVelInterval)
            {
                post.NewRowHeader(wVel);
                var i = 1;

                for (var wDir = windDirInit; wDir < windDirFin; wDir += windDirInterval)
                {
                    // Compute progress status
                    iter += 1;
                    int percentage = iter * 100 / totalCaseNum;
                    progress.Report(percentage);

                    // Run simulation.
                    SingleExecute(wVel, wDir);

                    // Add DropPoint to DropPoint List.
                    post.AddDropPoint();
                    if (IsParaDeploy) { postPara.AddDropPoint(); }

                    // Add Max value to Table
                    post.AddMaxVal(i,post.maxVelBx);
                    i += 1;
                }

                post.AddRowTable();

                // Add DropPoint List to Scatter List.
                post.AddDropList();
                if (IsParaDeploy) { postPara.AddDropList(); }
            }

            // Get functions from PostProcess (Trajectory)
            ScatterList = post.ScatterList;
            ListDic = post.ListDic;
            ResultComboSource = post.ResultComboSource;
            ResultPair = post.ResultPair;

            post.SetTablePair();
            TableDic = post.TableDic;

            TableComboSource = post.TableComboSource;

            // Get functions from PostProcess (Deploy parachute)
            if (IsParaDeploy)
            {
                ParaScatterList = postPara.ScatterList;
                ParaListDic = post.ListDic;
                ParaResultComboSource = post.ResultComboSource;
                ParaResultPair = post.ResultPair;
            }
        }

        public void SingleExecute(double windVel, double windDir)
        {
            // Generate instance
            Dynamics dy = new Dynamics();
            Initital init = new Initital();

            // Postprocess setting
            post.InitializePost();
            post.SetListPair();
            postPara.InitializePost();
            postPara.SetListPair();

            // Send rocket specification setting to Dynamics.
            dy.GroupList = allParam;
            dy.SetParam();
            dy.SetWind(windVel, windDir);

            // Define initial vector.
            init.GroupList = allParam;
            init.SetRailParam();
            var initVec = init.GetInitVec();

            // Define computational parameter.
            var tinit = 0.0;
            var tfinal = 100;
            var maxStep = 0.01;
            var opts = new Options { };
            opts.MaxStep = maxStep;
            opts.AbsoluteTolerance = Math.Pow(10, -4);

            // Trajectory calculation
            foreach (var sol in Ode.GearBDF(tinit, initVec, dy.Func, opts).SolveTo(tfinal))
            {
                if (sol.X[2] >= -0.1)
                {
                    post.AddResult(sol);
                }
                else
                {
                    break;
                }
            }

            // Save result of trajectory case
            var mode = "Trajectory";
            post.SaveResultCsv(mode, tempDir, windVel, windDir);

            // Postprocess for Max value
            post.CalcExtra();
            post.CalcMaxValue();

            // Deployment calculation
            IsParaDeploy = true;
            if (IsParaDeploy)
            {
                // Deploy 1st parachute calculation.
                var deployTime = post.GetDeployInitTime();
                var deployInitVec = post.GetDeployInitVec();

                // Postprocess setting
                postPara.InitializePost();
                postPara.SetListPair();

                foreach (var sol in Ode.GearBDF(deployTime, deployInitVec,
                    dy.DeployEq, opts).SolveTo(tfinal + deployTime))
                {
                    if (sol.X[2] >= -0.1)
                    {
                        postPara.AddResult(sol);
                    }
                    else
                    {
                        break;
                    }
                }

                // Save result of deployment case
                mode = "DeployPara";
                postPara.SaveResultCsv(mode, tempDir, windVel, windDir);
            }

        }

        
    }

}
