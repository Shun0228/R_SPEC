using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Oslo;


namespace SimBase_01
{
    class Environment
    {
        public double GetGravityAcc(double altitude)
        {
            // Define constant value.
            double GravityGround = -9.80665;
            double EarthRadius = 6378136.6;

            // Compute gravity acceleration.
            var gravityAcc = GravityGround * Math.Pow(EarthRadius / (EarthRadius + altitude), 2);

            return gravityAcc;
        }

        public (double temp, double pres, double dens) GetGasParam(double altitude, double accG)
        {
            // Define out parameter.
            double temp, pres, dens;

            accG = Math.Abs(accG);

            // Define constant value.
            double GasConst = 287.0;
            double tempGround = ToKelvin(15.0);
            double presGround = 1.013 * Math.Pow(10, 5);

            // Define derivative of temperature.
            double DerivTemp1 = -6.5 * Math.Pow(10, -3);
            double DerivTemp2 = 0.0;
            double DerivTemp3 = 1.0 * Math.Pow(10, -3);

            // Define altitude boundary[m].
            double altBound1 = 11000.0;
            double altBound2 = 20000.0;
            double altBound3 = 32000.0;

            // Define temperature boundary value.
            double tempBound1 = tempGround + DerivTemp1 * altBound1;
            double tempBound2 = tempBound1;
            double tempBound3 = tempBound2 + DerivTemp3 * (altBound3 - altBound2);

            // Define pressure boundary value.
            double presBound1 = presGround * Math.Pow((tempBound1 / tempGround), (accG / (-DerivTemp1 * GasConst)));
            double presBound2 = presBound1 * Math.Exp(accG * (altBound2 - altBound1) / (GasConst * tempBound1));
            double presBound3 = presBound2 * Math.Pow((tempBound3 / tempBound2), (accG / (-DerivTemp3 * GasConst)));
            

            // Compute temperature and pressure by altitude[m].
            if (altitude < altBound1)
            {
                temp = tempGround + DerivTemp1 * altitude;
                pres = presGround * Math.Pow((temp / tempGround), (accG / (DerivTemp1 * GasConst)));
            }
            else if ((altBound1 <= altitude) && (altitude < altBound2))
            {
                temp = tempBound1 + DerivTemp2 * (altitude - altBound1);
                pres = presBound1 * Math.Exp(accG * (altBound2 - altitude) / (GasConst * tempBound1));
            }
            else if ((altBound2 <= altitude) && (altitude < altBound3))
            {
                temp = tempBound2 + DerivTemp3 * (altitude - altBound2);
                pres = presBound2 * Math.Pow((temp / tempBound2), (accG / (-DerivTemp3 * GasConst)));
            }
            else if (altBound3 <= altitude)
            {
                // This region is not defined !
                // Below values are temporary setting, it is not correct value.
                temp = tempBound3;
                pres = presBound3;
            }
            else
            {
                // Under ground (not physical).
                temp = tempGround;
                pres = presGround;
            }

            dens = pres / (GasConst * temp);

            return (temp, pres, dens);
        }

        public double GetSoundVel(double temp)
        {
            double gamma = 1.4;
            double GasConst = 287.0;

            var soundVel = Math.Sqrt(gamma * GasConst * temp);

            return soundVel;
        }

        static double ToKelvin(double tempCelsius)
        {
            return tempCelsius + 237.15;
        }

        static double ToCelsius(double tempKelvin)
        {
            return tempKelvin - 237.15;
        }

    }
}
