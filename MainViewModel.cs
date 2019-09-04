using System.IO;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace SimBase_01
{
    class MainViewModel
    {
        public MainViewModel()
        {
            //==============================================
            // MyModel setting
            MyModel.Title = "Result Graph";

            // Generate object.
            var voidSeries = new LineSeries { };
            plotAxisX = new LinearAxis { };
            plotAxisY = new LinearAxis { };

            // X axis setting
            plotAxisX.Position = AxisPosition.Bottom;
            plotAxisX.Title = "X Axis";
            plotAxisX.TitleFontSize = 16;
            plotAxisX.MajorGridlineStyle = LineStyle.Dot;
            plotAxisX.IsZoomEnabled = false;

            // Y axis setting
            plotAxisY.Position = AxisPosition.Left;
            plotAxisY.Title = "Y Axis";
            plotAxisY.TitleFontSize = 16;
            plotAxisY.MajorGridlineStyle = LineStyle.Dot;
            plotAxisY.IsZoomEnabled = false;
            plotAxisY.AxisTitleDistance = 15;

            // Legend setting
            //MyModel.LegendTitle = "test";

            // Add to MyModel.
            MyModel.Series.Add(voidSeries);
            MyModel.Axes.Add(plotAxisX);
            MyModel.Axes.Add(plotAxisY);

            //==============================================
            // ThrustModel setting
            thrustAxisX = new LinearAxis { };
            thrustAxisY = new LinearAxis { };
            var thrustSeries = new LineSeries { };

            // X axis setting
            thrustAxisX.Position = AxisPosition.Bottom;
            thrustAxisX.Title = "time";
            thrustAxisX.TitleFontSize = 16;
            thrustAxisX.MajorGridlineStyle = LineStyle.Dot;
            thrustAxisX.IsZoomEnabled = false;
            thrustAxisX.Unit = "sec";

            // Y axis setting
            thrustAxisY.Position = AxisPosition.Left;
            thrustAxisY.Title = "Thrust Force";
            thrustAxisY.TitleFontSize = 16;
            thrustAxisY.MajorGridlineStyle = LineStyle.Dot;
            thrustAxisY.IsZoomEnabled = false;
            thrustAxisY.Unit = "N";
            thrustAxisY.AxisTitleDistance = 15;

            ThrustModel.Title = "Thrust History";
            ThrustModel.Series.Add(thrustSeries);
            ThrustModel.Axes.Add(thrustAxisX);
            ThrustModel.Axes.Add(thrustAxisY);

            //==============================================
            // ComboBox initial setting
            GraphCombo gc = new GraphCombo();
            var axisTitleList = gc.GetTitleList();
            ComboItems.AddRange(axisTitleList);
        }

        //==============================================================
        // Tab.1 (Setting Window)
        //==============================================================
        public PlotModel ThrustModel { get; private set; } = new PlotModel() { };

        private LinearAxis thrustAxisX;
        private LinearAxis thrustAxisY;

        public void AddThrustSeries(List<DataPoint> thrustList)
        {
            var thrustSeries = new LineSeries { };

            thrustSeries.Points.AddRange(thrustList);
            ThrustModel.Series.Clear();
            ThrustModel.Series.Add(thrustSeries);
        }

        //==============================================================
        // Tab.2 (Graph Window)
        //==============================================================
        public PlotModel MyModel { get; private set; } = new PlotModel() { };
        public List<string> ComboItems { get; private set; } = new List<string>() { };

        private LinearAxis plotAxisX;
        private LinearAxis plotAxisY;
        
        public void UpdateAxis(AxisSet item)
        {
            plotAxisX.Title = item.AxisX;
            plotAxisY.Title = item.AxisY;

            plotAxisX.Unit = item.UnitX;
            plotAxisY.Unit = item.UnitY;

            MyModel.Title = item.Title;
        }

        public void AddListSeries(List<DataPoint> inputList, string title)
        {
            var addSeries = new LineSeries
            {
                Title = title,
            };

            addSeries.Points.AddRange(inputList);
            MyModel.Series.Add(addSeries);
        }

        public void InitializeSeries()
        {
            MyModel.Series.Clear();
        }

        public void AddMainSeries(List<DataPoint> mainList, string legendTitle)
        {
            var addSeries = new LineSeries { };

            addSeries.Title = legendTitle;
            addSeries.Points.AddRange(mainList);
            MyModel.Series.Add(addSeries);
        }
    }
}
