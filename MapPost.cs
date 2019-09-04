using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Maps.MapControl.WPF;


namespace SimBase_01
{
    class MapPost
    {
        // Public varieties
        public List<string> ModeList { set; get; }
        public LocationCollection DropLatLon { set; get; }
        public List<LocationCollection> ScatterLatLonList { set; get; }
        public List<LocationCollection> ParaScatterLatLonList { set; get; }
        public List<List<DropPoint>> ScatterList { set; get; }
        public List<List<DropPoint>> ParaScatterList { set; get; }

        public Color PolyLineColor { set; get; } = Colors.Red;

        // Set constant value (meter unit)
        private const double EquatorRadius = 6378137;
        private const double PolarRadius = 6356752;

        public MapPost()
        {
            ModeList = new List<string> { "弾道落下", "減速降下", "重ねて表示" };
        }

        public void InitializeDropPoint()
        {
            ScatterList = new List<List<DropPoint>>();
            ParaScatterList = new List<List<DropPoint>>();
        }

        public MapPolyline GetScatterLine(LocationCollection dropLoc)
        {
            var scatterLine = new MapPolyline();

            // Define detail setting
            scatterLine.Locations = dropLoc;

            scatterLine.Stroke = new SolidColorBrush(PolyLineColor);
            scatterLine.StrokeThickness = 2.0;

            return scatterLine;
        }

        public List<LocationCollection> GetScatterCollection(Location basePoint, List<List<DropPoint>> locList)
        {
            var scatterLatLonList = new List<LocationCollection>();

            foreach (var itemList in locList)
            {
                var dropLatLon = new LocationCollection();

                foreach (var item in itemList)
                {
                    var convLoc = ToLatLon(basePoint, item.XPos, item.YPos);
                    dropLatLon.Add(convLoc);
                }

                scatterLatLonList.Add(dropLatLon);
            }

            return scatterLatLonList;
        }

        public MapPolyline GetPolyLine(string selectMode)
        {
            var selectedPolyLine = new MapPolyline();

            if (selectMode == "弾道落下")
            {

            }
            else if (selectMode == "減速降下")
            {

            }
            else if (selectMode == "重ねて表示")
            {

            }

            return selectedPolyLine;
        }

        public Location ToLatLon(Location basePoint, double deltaX, double deltaY)
        {
            // Calculate latitude per 1.0 meter.
            var polarCircleLen = 2.0 * Math.PI * PolarRadius;
            var latPerMeter = 360.0 * (1.0 / polarCircleLen);

            // Calculate longitude per 1.0 meter.
            var localRadius = EquatorRadius * Math.Cos(basePoint.Latitude * Math.PI / 180.0);
            var localCircleLen = 2.0 * Math.PI * localRadius;
            var lonPerMeter = 360.0 * (1.0 / localCircleLen);

            // Calculate unit converter.
            var deltaLat = latPerMeter * deltaY;
            var deltaLon = lonPerMeter * deltaX;

            // Calculate converted Latitude & Longitude
            var convLat = basePoint.Latitude + deltaLat;
            var convLon = basePoint.Longitude + deltaLon;
            var convPoint = new Location(convLat, convLon);

            return convPoint;
        }

        //public void ConvDropPoint(Location basePoint)
        //{
        //    ScatterLatLonList = new List<LocationCollection>();
        //    ParaScatterLatLonList = new List<LocationCollection>();

        //    foreach (var itemList in ScatterList)
        //    {
        //        DropLatLon = new LocationCollection();

        //        foreach (var item in itemList)
        //        {
        //            var convLoc = ToLatLon(basePoint, item.XPos, item.YPos);
        //            DropLatLon.Add(convLoc);
        //        }

        //        ScatterLatLonList.Add(DropLatLon);
        //    }

        //    foreach (var itemList in ParaScatterList)
        //    {
        //        DropLatLon = new LocationCollection();

        //        foreach (var item in itemList)
        //        {
        //            var convLoc = ToLatLon(basePoint, item.XPos, item.YPos);
        //            DropLatLon.Add(convLoc);
        //        }

        //        ParaScatterLatLonList.Add(DropLatLon);
        //    }
        //}



        //public void SetRegion()
        //{
        //    string placeName;
        //    LocationCollection region;
        //    double Radius;
        //    Location CircleCenter;
        //    Location launchPoint;

        //    // Land launch-point in Oshima
        //    placeName = "Oshima (Land)";
        //    launchPoint = new Location(34.735972, 139.420944);
        //    region = new LocationCollection() {
        //    new Location(34.735715, 139.420922),
        //    new Location(34.731750, 139.421719),
        //    new Location(34.733287, 139.424590),
        //    new Location(34.736955, 139.426038),
        //    new Location(34.738908, 139.423597),
        //    new Location(34.740638, 139.420681),
        //    new Location(34.741672, 139.417387),
        //    new Location(34.735715, 139.420922)};
        //    PlaceList.Add(placeName);
        //    RegionPair.Add(placeName, region);
        //    OriginPair.Add(placeName, launchPoint);

        //    // Near sea launch-point in Oshima
        //    placeName = "Oshima (Sea)";
        //    //launchPoint = new Location(34.679458, 139.437338);
        //    launchPoint = new Location(34.679691, 139.438248);
        //    region = new LocationCollection();
        //    Radius = 2500.0;
        //    CircleCenter = new Location(34.664797, 139.460070);

        //    for (double deg = 0.0; deg <= 360.0; deg += 10.0)
        //    {
        //        var deltaX = Radius * Math.Cos(deg * Math.PI / 180.0);
        //        var deltaY = Radius * Math.Sin(deg * Math.PI / 180.0);
        //        var convPoint = ToLatLon(CircleCenter, deltaX, deltaY);
        //        region.Add(convPoint);
        //    }
        //    PlaceList.Add(placeName);
        //    RegionPair.Add(placeName, region);
        //    OriginPair.Add(placeName, launchPoint);

        //    // Land launch-point in Noshiro
        //    placeName = "Noshiro (Land)";
        //    launchPoint = new Location(40.138633, 139.984850);
        //    region = new LocationCollection() {
        //    new Location(40.139725, 139.983939),
        //    new Location(40.136127, 139.982133),
        //    new Location(40.135607, 139.981753),
        //    new Location(40.134911, 139.981451),
        //    new Location(40.134821, 139.981692),
        //    new Location(40.135639, 139.983324),
        //    new Location(40.137052, 139.984608),
        //    new Location(40.138053, 139.985781),
        //    new Location(40.139075, 139.986297),
        //    new Location(40.139725, 139.983939)
        //    };
        //    PlaceList.Add(placeName);
        //    RegionPair.Add(placeName, region);
        //    OriginPair.Add(placeName, launchPoint);

        //    // Near sea launch-point in Noshiro
        //    placeName = "Noshiro (Sea)";
        //    launchPoint = new Location(40.242865, 140.010450);
        //    region = new LocationCollection();
        //    Radius = 1500.0;
        //    CircleCenter = new Location(40.248855, 139.975967);

        //    for (double deg = 0.0; deg <= 360.0; deg += 10.0)
        //    {
        //        var deltaX = Radius * Math.Cos(deg * Math.PI / 180.0);
        //        var deltaY = Radius * Math.Sin(deg * Math.PI / 180.0);
        //        var convPoint = ToLatLon(CircleCenter, deltaX, deltaY);
        //        region.Add(convPoint);
        //    }
        //    PlaceList.Add(placeName);
        //    RegionPair.Add(placeName, region);
        //    OriginPair.Add(placeName, launchPoint);
        //}



        //// Using
        //public void AddDropPoint(DropPoint dp)
        //{
        //    dropPosList.Add(dp);
        //}

        //public MapPolygon GetRegionPoly(string place)
        //{
        //    // Set variety
        //    var selectRegion = RegionPair[place];
        //    var regionPoly = new MapPolygon();

        //    // Define detail setting
        //    regionPoly.Locations = selectRegion;
        //    regionPoly.Stroke = new SolidColorBrush(Colors.OrangeRed);
        //    regionPoly.Fill = new SolidColorBrush(Colors.Yellow);
        //    regionPoly.StrokeThickness = 2.0;
        //    regionPoly.Opacity = 0.5;

        //    return regionPoly;
        //}

        //public MapPolyline GetSafeRegion(string place)
        //{
        //    // Set variety
        //    var selectRegion = RegionPair[place];
        //    var regionPoly = new MapPolyline();

        //    // Define detail setting
        //    regionPoly.Locations = selectRegion;
        //    regionPoly.Stroke = new SolidColorBrush(Colors.Yellow);
        //    regionPoly.StrokeThickness = 2.0;
        //    var dashLength = 2;
        //    var dashSpace = 1;
        //    regionPoly.StrokeDashArray = new DoubleCollection(2) { dashLength, dashSpace };

        //    //regionPoly.StrokeDashArray = new DoubleCollection(2) { 1, 1 };
        //    //regionPoly.StrokeDashArray = new DoubleCollection(4) { 1, 4, 2, 1 };

        //    return regionPoly;
        //}




        //public void SetScatterList()
        //{
        //    var firstPoint = dropPosList.First();
        //    dropPosList.Add(firstPoint);
        //    ScatterList.Add(dropPosList);
        //    dropPosList = new List<DropPoint>();
        //}

        // Using 1127



    }
}
