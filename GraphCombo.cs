using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace SimBase_01
{
    class GraphCombo
    {
        private List<string> titleList; 
        private List<AxisSet> axisList; 

        public GraphCombo()
        {
            axisList = new List<AxisSet>() { };
            axisList.Add(new AxisSet("Time", "X,Y,Z", "sec", "m", "Position"));
            axisList.Add(new AxisSet("Time", "Alpha,Beta", "sec", "deg", "Angle of Attack"));
            axisList.Add(new AxisSet("Time", "Vbx,Vby,Vbz", "sec", "m/s", "Velocity (Body Axis)"));
            axisList.Add(new AxisSet("Time", "Machbx,Machby,Machbz", "sec", "-", "Mach number (Body Axis)"));
            axisList.Add(new AxisSet("Time", "Z", "sec", "m", "Altitude"));
            axisList.Add(new AxisSet("Time", "U,V,W", "sec", "m/s", "Velocity"));
            axisList.Add(new AxisSet("Time", "P,Q,R", "sec", "rad/s", "Angular Velocity"));
            axisList.Add(new AxisSet("Time", "QX,QY,QZ,QW", "sec", "-", "Quaternion"));
        }
   
        public List<string> GetTitleList()
        {
            titleList = new List<string>() { };

            foreach (var item in axisList)
            {
                titleList.Add(item.Title);
            }

            return titleList;
        }

        public AxisSet GetAxisTitle(string graphTitle)
        {
            AxisSet axisSet = new AxisSet("", "", "", "", "");

            foreach (var item in axisList)
            {
                if (item.Title == graphTitle)
                {
                    axisSet = item;
                }
            }

            return axisSet;
        }
    }
}
