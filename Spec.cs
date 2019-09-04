using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimBase_01
{
    class Spec
    {
        public string Name { get; set; }
        public string NameDesc { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }

        public Spec(string name, string nameDesc, string value, string unit)
        {
            this.Name = name;
            this.NameDesc = nameDesc;
            this.Value = value;
            this.Unit = unit;
        }
    }

    public class BodyItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    class JsonItemSet
    {
        public IDictionary<string, string> Rocket { get; set; }
        public IDictionary<string, string> Motor { get; set; }
        public IDictionary<string, string> Environment { get; set; }
        public IDictionary<string, string> Wind { get; set; }

        public JsonItemSet()
        {
            this.Rocket = new Dictionary<string, string>();
            this.Motor = new Dictionary<string, string>();
            this.Environment = new Dictionary<string, string>();
            this.Wind = new Dictionary<string, string>();
        }
    }

    class PlaceSet
    {
        public IDictionary<string, string> Origin { get; set; }
        public IDictionary<int, LatLon> SafeRegion { get; set; }

        public PlaceSet()
        {
            this.Origin = new Dictionary<string, string>();
            this.SafeRegion = new Dictionary<int, LatLon>();
        }
    }

    class AxisSet
    {
        public string AxisX { get; set; }
        public string AxisY { get; set; }
        public string UnitX { get; set; }
        public string UnitY { get; set; }
        public string Title { get; set; }

        public AxisSet(string axisX, string axisY, string unitX, string unitY, string title)
        {
            this.AxisX = axisX;
            this.AxisY = axisY;
            this.UnitX = unitX;
            this.UnitY = unitY;
            this.Title = title;
        }
    }

    class DropPoint
    {
        public double XPos;
        public double YPos;

        public DropPoint(double xPos, double yPos)
        {
            this.XPos = xPos;
            this.YPos = yPos;
        }
    }

    class LatLon
    {
        public string Lat { get; set; }
        public string Lon { get; set; }

        public LatLon(string lat, string lon)
        {
            this.Lat = lat;
            this.Lon = lon;
        }

        public LatLon()
        {
        }
    }
}
