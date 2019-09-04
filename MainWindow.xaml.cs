using System;
using System.Data;
using System.IO;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Oslo;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Maps.MapControl.WPF;
using System.Text.RegularExpressions;

namespace SimBase_01
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //==============================================================
        // Initialize
        //==============================================================
        MainViewModel mvm = new MainViewModel();
        GraphCombo gc = new GraphCombo();
        PostProcess post = new PostProcess();
        MapPost mp = new MapPost();
        RelateDataGrid spec = new RelateDataGrid();
        RunSim sim;

        public MainWindow()
        {
            InitializeComponent();
            BodyGrid.ItemsSource = bodyData;
            EnvGrid.ItemsSource = envData;
            SafetyRegionGrid.ItemsSource = latlonData;

            spec.Initialize();
            SetSource();
            ThrustPath.Text = "";

            resultPair = new Dictionary<string, string>();
            SetMap();

            BodyGrid.CanUserDeleteRows = false;
            MotorGrid.CanUserDeleteRows = false;
            EnvGrid.CanUserDeleteRows = false;
        }
        
        //==============================================================
        // Tab.1 (Setting Window)
        //==============================================================
        ObservableCollection<Spec> bodyData = new ObservableCollection<Spec>();
        ObservableCollection<Spec> motorData = new ObservableCollection<Spec>();
        ObservableCollection<Spec> envData = new ObservableCollection<Spec>();
        ObservableCollection<Spec> windData = new ObservableCollection<Spec>();
        ObservableCollection<LatLon> latlonData = new ObservableCollection<LatLon>();

        private void NewSpec_Click(object sender, RoutedEventArgs e)
        {
            spec.Initialize();
            SetSource();
            ThrustPath.Text = "";
        }

        private void LoadSpec_Click(object sender, RoutedEventArgs e)
        {
            // Get current directory
            var crtDir = Directory.GetCurrentDirectory();
            var paramDir = crtDir + "\\" + "parameter";

            // Set OpenFileDialog.
            var dlg = new OpenFileDialog();
            dlg.InitialDirectory = paramDir;
            dlg.Filter = "JSON Files (.json)|*.json";

            // Open process
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    spec.LoadItem(dlg.FileName);
                    SetSource();
                    ThrustPath.Text = spec.MotorPath;
                }

                catch (SystemException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        private void SetSource()
        {
            bodyData = spec.BodySet;
            envData = spec.EnvSet;
            windData = spec.WindSet;

            BodyGrid.ItemsSource = bodyData;
            EnvGrid.ItemsSource = envData;
            WindGrid.ItemsSource = windData;
        }

        private void SaveSpec_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            var crtDir = Directory.GetCurrentDirectory();
            var paramDir = crtDir + "\\" + "parameter";

            dlg.InitialDirectory = paramDir;
            dlg.Filter = "JSON Files (.json)|*.json";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (var sw = new StreamWriter(dlg.FileName, false, Encoding.GetEncoding("Shift_JIS")))
                    {
                        // Initialize
                        JsonItemSet jsonSet = new JsonItemSet();

                        // Add items to JSON file
                        foreach (var item in bodyData)
                        {
                            jsonSet.Rocket.Add(item.Name, item.Value);
                        }

                        jsonSet.Motor.Add("Path", ThrustPath.Text);

                        foreach (var item in envData)
                        {
                            jsonSet.Environment.Add(item.Name, item.Value);
                        }

                        foreach (var item in windData)
                        {
                            jsonSet.Wind.Add(item.Name, item.Value);
                        }

                        // Save JSON file
                        var allText = JsonConvert.SerializeObject(jsonSet, Formatting.Indented);
                        sw.WriteLine(allText);
                    }

                }
                catch (SystemException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void SelectMotor_Click(object sender, RoutedEventArgs e)
        {
            // Set OpenFileDialog.
            var dlg = new OpenFileDialog();
            var crtDir = Directory.GetCurrentDirectory();
            dlg.InitialDirectory = crtDir;
            dlg.Filter = "Text or CSV Files|*.txt;*.csv";
            //dlg.Filter = "Text Files (.txt)|*.txt|CSV Files (.csv)|*.csv";

            // Open process
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ThrustPath.Text = dlg.FileName;
                }

                catch (SystemException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void ThrustPath_TextChanged(object sender, EventArgs e)
        {
            var thrustDataList = new List<DataPoint>() { };
            var timeList = new List<double>();
            var forceList = new List<double>();

            try
            {
                var filePath = ThrustPath.Text;
                var sr = new StreamReader(filePath, Encoding.GetEncoding("SHIFT_JIS"));
                var fileExtension = System.IO.Path.GetExtension(filePath);
                spec.MotorPath = ThrustPath.Text;

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
                        thrustDataList.Add(new DataPoint(time, force));
                    }

                }

                Thrust th = new Thrust();
                th.CalcThrustParam(timeList, forceList);
                var timeInterval = th.TimeInterval;
                var thrustAve = th.AveThrust;
                var maxThrust = th.MaxThrust;
                var impulse = th.Impulse;
                motorData.Clear();
                motorData.Add(new Spec("Impulese", "総力積", impulse.ToString("F2"), "N*s"));
                motorData.Add(new Spec("AverageThrust", "平均推力", thrustAve.ToString("F2"), "N"));
                motorData.Add(new Spec("MaxThrust", "最大推力", maxThrust.ToString("F2"), "N"));
                motorData.Add(new Spec("TimeInterval", "時間刻み", timeInterval.ToString(), "sec"));
                MotorGrid.ItemsSource = motorData;
            }
            catch (System.IO.FileNotFoundException ex) { }
            catch (System.ArgumentException arEx) { }

            
            mvm.AddThrustSeries(thrustDataList);
            ThrustPlot.Model = mvm.ThrustModel;
            ThrustPlot.InvalidatePlot(true);
        }

        private void ShowProgress(int percent)
        {
            CalcProgress.Value = percent;
        }

        private void DisableButton()
        {
            CalcButton.IsEnabled = false;
            NewSpec.IsEnabled = false;
            LoadSpec.IsEnabled = false;
            SaveSpec.IsEnabled = false;
        }

        private void EnableButton()
        {
            CalcButton.IsEnabled = true;
            NewSpec.IsEnabled = true;
            LoadSpec.IsEnabled = true;
            SaveSpec.IsEnabled = true;
        }

        private async void CalcSimAsync(object sender, RoutedEventArgs e)
        {
            // Check parameter fill
            var IsFilled = CheckParam();

            if (IsFilled == true)
            {
                // Disable to click Calculate button.
                DisableButton();
                CalcButton.Content = "Calculating ...";

                // Execute rocket simulation.
                var jsonParam = spec.ToJsonFile();
                sim = new RunSim(jsonParam);
                var p = new Progress<int>(ShowProgress);
                await Task.Run(() => sim.MultiExecute(p));

                // Enable to click Calculate button.
                EnableButton();
                CalcButton.Content = "Calculate";

                // Line Plotting process (Tab.2)
                ResultCombo.ItemsSource = sim.ResultComboSource;
                ResultCombo.SelectedIndex = 0;
                UpdateResult();
                GraphTab.IsSelected = true;

                // Map Plotting process (Tab.3)
                var scatterList = sim.ScatterList;
                var paraScatterList = sim.ParaScatterList;

                mp.ScatterList = scatterList;
                mp.ParaScatterList = paraScatterList;

                SetSafetyRegion();
                ModeSelect.SelectedIndex = 0;

                // Table output process (Tab.4)
                TableCombo.ItemsSource = sim.TableComboSource;
                TableCombo.SelectedIndex = 0;
                UpdateTable();

                
            }
            else
            {
                var content = "[ERROR] Please fill all of parameters!";
                DebugBox.Text = content;
                MessageBox.Show(content);
            }
            
        }

        private bool CheckParam()
        {
            // UNDER CONSTRUCTION !!!
            var IsFilled = true;

            foreach(var item in bodyData)
            {
                if (item.Value == "") { IsFilled = false; }
            }

            foreach (var item in envData)
            {
                if (item.Value == "") { IsFilled = false; }
            }

            if (File.Exists(ThrustPath.Text) == false) { IsFilled = false; }

            return IsFilled;
        }

        // For preview Map
        Pushpin prevCenterPin = new Pushpin();
        MapPolyline prevPoly = new MapPolyline();

        private void LatLonBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TextBox check
            if (LatBox.Text=="" || LonBox.Text == "")
            {
                DebugBox.Text = "緯度経度が未入力です";
            }
            else
            {
                // Set latitude and longitude
                var latOrigin = double.Parse(LatBox.Text);
                var lonOrigin = double.Parse(LonBox.Text);
                var mapOrigin = new Location(latOrigin, lonOrigin);

                // Set center of preview MAP
                prevMap.Center = mapOrigin;

                // Check children
                if (prevMap.Children.Contains(prevPoly))
                {
                    prevMap.Children.Clear();
                    prevMap.Children.Add(prevPoly);
                }
                else
                {
                    prevMap.Children.Clear();
                }

                // Add Pushpin to preview MAP
                prevCenterPin = new Pushpin();
                prevCenterPin.Location = mapOrigin;

                prevMap.Children.Add(prevCenterPin);
            }
        }

        private void LatBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            if (LatBox.Text.Length > 0 && e.Text == "-")
            {
                e.Handled = true;
                return;
            }

            var text = LatBox.Text + e.Text;
            e.Handled = regex.IsMatch(text);
        }

        private void LonBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            if (LonBox.Text.Length > 0 && e.Text == "-")
            {
                e.Handled = true;
                return;
            }

            var text = LonBox.Text + e.Text;
            e.Handled = regex.IsMatch(text);
        }

        private void SafetyRegionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (prevMap.Children.Contains(prevCenterPin))
            {
                prevMap.Children.Clear();
                prevMap.Children.Add(prevCenterPin);
            }
            else
            {
                prevMap.Children.Clear();
            }

            var test = "header";
            var regionCollection = new LocationCollection();
            prevPoly = new MapPolyline();

            foreach (var item in latlonData)
            {
                if (item.Lat != null && item.Lon != null)
                {
                    if (item.Lat != "" && item.Lon != "")
                    {
                        test += item.Lat.ToString() + item.Lon.ToString() + ", ";
                        var locPoint = new Location(double.Parse(item.Lat), double.Parse(item.Lon));
                        regionCollection.Add(locPoint);
                    }
                }

            }

            if (regionCollection.Count > 2)
            {
                var firstLoc = regionCollection[0];
                regionCollection.Add(firstLoc);
            }


            prevPoly.Locations = regionCollection;
            prevPoly.Stroke = new SolidColorBrush(Colors.Yellow);
            prevPoly.StrokeThickness = 2.0;
            var dashLength = 2;
            var dashSpace = 1;
            prevPoly.StrokeDashArray = new DoubleCollection(2) { dashLength, dashSpace };

            prevMap.Children.Add(prevPoly);
        }

        private void PlaceNew_Click(object sender, RoutedEventArgs e)
        {
            LatBox.Text = "";
            LonBox.Text = "";
            latlonData.Clear();
        }

        private void PlaceLoad_Click(object sender, RoutedEventArgs e)
        {
            // Get current directory
            var crtDir = Directory.GetCurrentDirectory();
            var locDir = crtDir + "\\" + "location";

            // Set OpenFileDialog.
            var dlg = new OpenFileDialog();
            dlg.InitialDirectory = locDir;
            dlg.Filter = "JSON Files (.json)|*.json";

            // Open process
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    latlonData.Clear();

                    DebugBox.Text = "Loaded";

                    var rawAllText = File.ReadAllText(dlg.FileName);
                    var groupList = JObject.Parse(rawAllText);

                    var originList = groupList["Origin"];
                    var safeList = groupList["SafeRegion"];

                    // Add all key and value to dataSet.
                    foreach (JProperty item in originList)
                    {
                        LatBox.Text = item.Name.ToString();
                        LonBox.Text = item.Value.ToString();
                    }

                    // Add all key and value to dataSet.
                    foreach (JProperty item in safeList)
                    {
                        var value = item.Value;
                        var lat = value["Lat"].ToString();
                        var lon = value["Lon"].ToString();
                        var location = new LatLon(lat, lon);
                        latlonData.Add(location);
                    }
                }

                catch (SystemException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void PlaceSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            var crtDir = Directory.GetCurrentDirectory();
            var locDir = crtDir + "\\" + "location";
            dlg.InitialDirectory = locDir;
            dlg.Filter = "JSON Files (.json)|*.json";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (var sw = new StreamWriter(dlg.FileName, false, Encoding.GetEncoding("Shift_JIS")))
                    {
                        // Initialize
                        PlaceSet placeSet = new PlaceSet();

                        placeSet.Origin.Add(LatBox.Text, LonBox.Text);

                        // Add items to JSON file
                        var locID = 0;
                        foreach (var item in latlonData)
                        {
                            locID += 1;
                            placeSet.SafeRegion.Add(locID, item);
                        }

                        // Save JSON file
                        var allText = JsonConvert.SerializeObject(placeSet, Formatting.Indented);
                        sw.WriteLine(allText);
                    }

                }
                catch (SystemException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void HelpButtonTab1_Click(object sender, RoutedEventArgs e)
        {

        }


        //==============================================================
        // Tab.2 (Graph Window)
        //==============================================================
        Dictionary<string, string> resultPair;

        private void GraphVar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            // Update Axis of MyModel
            string selectItem = GraphVar.SelectedItem.ToString();
            var axisSet = gc.GetAxisTitle(selectItem);
            mvm.UpdateAxis(axisSet);

            // DataPoint update
            mvm.InitializeSeries();
            var listDic = post.ListDic;
            var addListX = listDic[axisSet.AxisX];

            // Set DataPoint to MyModel
            foreach (var yItem in axisSet.AxisY.Split(','))
            {
                var addListY = listDic[yItem];
                var addSeries = post.ToDataSet(addListX, addListY);
                mvm.AddMainSeries(addSeries, yItem);
            }

            // Update Plotmodel
            PlotWindow.Model = mvm.MyModel;
            PlotWindow.InvalidatePlot(true);
        }

        private void ResultCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateResult();
        }

        private void UpdateResult()
        {
            // Get ComboBox item
            var selectCase = ResultCombo.SelectedItem.ToString();
            var resultPair = sim.ResultPair;
            var csvPath = resultPair[selectCase];

            // Get result list from temp directory.
            var tempResult = new List<double[]>();

            // Read result from temp/*.csv
            using (StreamReader sr = new StreamReader(csvPath, Encoding.GetEncoding("Shift_JIS")))
            {
                var line = "";

                while ((line = sr.ReadLine()) != null)
                {
                    var arr = line.Split(',');
                    var result = new double[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        result[i] = double.Parse(arr[i]);
                    }

                    tempResult.Add(result);
                }
            }

            // Reload temp result file in PostProcess
            post.InitializePost();
            post.LoadResult(tempResult);
            post.SetListPair();
            post.CalcExtra();
            post.CalcMaxValue();

            // Check GraphVar ComboBox
            if (GraphVar.SelectedItem != null)
            {
                UpdateGraph();
            }
            else
            {
                GraphVar.SelectedIndex = 0;
                UpdateGraph();
            }

        }

        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PNG Files(.png)|*.png";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var pngExport = new PngExporter { };
                    pngExport.Height = (int)mvm.MyModel.Height;
                    pngExport.Width = (int)mvm.MyModel.Width;
                    pngExport.ExportToFile(mvm.MyModel, dlg.FileName);
                }
                catch (SystemException ex) { }
            }
        }


        //==============================================================
        // Tab.3 (Map Window)
        //==============================================================
        Pushpin launchPin = new Pushpin();
        MapPolyline safePoly = new MapPolyline();
        List<LocationCollection> TrajecScatterLatLon;
        List<LocationCollection> ParaScatterLatLon;

        private void SetMap()
        {
            myMap.Mode = new AerialMode(true);
            ModeSelect.ItemsSource = mp.ModeList;
        }

        private void SetSafetyRegion()
        {
            // Initialize myMap
            myMap.Children.Clear();

            // Get safety region of current place
            var safeLoc = prevPoly.Locations;
            safePoly = new MapPolyline();
            safePoly.Locations = safeLoc;
            safePoly.StrokeDashArray = prevPoly.StrokeDashArray;
            safePoly.Stroke = prevPoly.Stroke;
            safePoly.StrokeThickness = prevPoly.StrokeThickness;

            // Get Launch point
            var launchPoint = prevCenterPin.Location;
            launchPin = new Pushpin();
            launchPin.Location = launchPoint;

            // Add safety region to myMap
            myMap.Children.Add(launchPin);
            myMap.Children.Add(safePoly);

            try
            {
                // Get scatter List from MapPost
                var trajecScatterList = mp.ScatterList;
                var paraScatterList = mp.ParaScatterList;

                // Get scatter List of LocationCollection
                TrajecScatterLatLon = mp.GetScatterCollection(launchPoint, trajecScatterList);
                ParaScatterLatLon = mp.GetScatterCollection(launchPoint, paraScatterList);
            }
            catch (System.Exception ex) { }

            myMap.Center = launchPoint;
            myMap.ZoomLevel = 15;

        }

        private void ModeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get selected mode
            var selectMode = ModeSelect.SelectedItem.ToString();

            // Initialize myMap
            myMap.Children.Clear();
            myMap.Children.Add(safePoly);
            myMap.Children.Add(launchPin);

            var launchPoint = launchPin.Location;

            if (selectMode == "弾道落下")
            {
                // Add scatter plot to myMap
                foreach (var locItem in TrajecScatterLatLon)
                {
                    mp.PolyLineColor = Colors.Red;
                    var scatterPoly = mp.GetScatterLine(locItem);
                    var trajecScatterPoly = scatterPoly;
                    myMap.Children.Add(scatterPoly);
                }
            }
            else if (selectMode == "減速降下")
            {
                foreach (var locItem in ParaScatterLatLon)
                {
                    mp.PolyLineColor = Colors.Orange;
                    var scatterPoly = mp.GetScatterLine(locItem);
                    var paraScatterPoly = scatterPoly;
                    myMap.Children.Add(scatterPoly);
                }
            }
            else if (selectMode == "重ねて表示")
            {
                // Add scatter plot to myMap
                foreach (var locItem in TrajecScatterLatLon)
                {
                    mp.PolyLineColor = Colors.Red;
                    var scatterPoly = mp.GetScatterLine(locItem);
                    var trajecScatterPoly = scatterPoly;
                    myMap.Children.Add(scatterPoly);
                }

                foreach (var locItem in ParaScatterLatLon)
                {
                    mp.PolyLineColor = Colors.Orange;
                    var scatterPoly = mp.GetScatterLine(locItem);
                    var paraScatterPoly = scatterPoly;
                    myMap.Children.Add(scatterPoly);
                }
            }
        }

        //==============================================================
        // Tab.4 (Table Window)
        //==============================================================

        private void TableGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "RowHeader")
            {
                e.Cancel = true;
            }
        }

        private void ExportTable_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TableCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTable();
        }

        private void UpdateTable()
        {
            // Get ComboBox item
            var selectCase = TableCombo.SelectedItem.ToString();
            var tableDic = sim.TableDic;
            var tableVal = tableDic[selectCase];
            
            TableGrid.ItemsSource = tableVal.DefaultView;
        }
    }
}
