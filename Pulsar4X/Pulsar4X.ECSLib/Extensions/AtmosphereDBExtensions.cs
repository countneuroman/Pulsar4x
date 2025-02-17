﻿using Pulsar4X.Orbital;
using System;
using System.Collections.Generic;

namespace Pulsar4X.ECSLib
{
    /// <summary>
    /// Information on the calculations can be found here:
    /// http://aurorawiki.pentarch.org/index.php?title=Terraforming
    /// </summary>
    public static class AtmosphereDBExtensions
    {
        public static bool WouldBeFrozenAtGivenTemperature(this AtmosphericGasSD gas, float temperatureInC)
        {
            return (gas.MeltingPoint > temperatureInC);
        }

        public static float GetAtmosphericPressure(this AtmosphereDB atmosphere)
        {
            double totalPressure = 0;
            var baseTemp = atmosphere.GetParentBaseTemperature();

            foreach (var (gas, pressure) in atmosphere.Composition)
            {
                if (!gas.WouldBeFrozenAtGivenTemperature(baseTemp))
                {
                    totalPressure += pressure;
                }
            }

            return Convert.ToSingle(totalPressure);
        }

        public static float GetGreenhousePressure(this AtmosphereDB atmosphere)
        {
            float totalPressure = 0;
            var baseTemp = atmosphere.GetParentBaseTemperature();

            foreach (var (gas, pressure) in atmosphere.Composition)
            {
                if (gas.GreenhouseEffect > 0 && !gas.WouldBeFrozenAtGivenTemperature(baseTemp)) {
                    totalPressure += pressure;
                }
            }

            return Convert.ToSingle(Math.Round(totalPressure, 3));
        }

        public static float GetAntiGreenhousePressure(this AtmosphereDB atmosphere)
        {
            float totalPressure = 0;
            var baseTemp = atmosphere.GetParentBaseTemperature();

            foreach (var (gas, pressure) in atmosphere.Composition)
            {
                if (gas.GreenhouseEffect < 0 && !gas.WouldBeFrozenAtGivenTemperature(baseTemp))
                {
                    totalPressure += pressure;
                }
            }

            return Convert.ToSingle(Math.Round(totalPressure, 3));
        }

        public static float CalculatedGreenhouseFactor(this AtmosphereDB atmosphere)
        {
            double calculated = 1.0 + (atmosphere.GetAtmosphericPressure() / 10.0f) + atmosphere.GetGreenhousePressure();
            double capped = Math.Min(calculated, 3.0);          // Max of 3.0
            double rounded = Math.Round(capped, 3);             // Round to 3 decimal places
            return Convert.ToSingle(rounded);
        }

        public static float CalulatedSurfaceTemperature(this AtmosphereDB atmosphere, float albedoFactor = 1.0f)
        {
            var ghFactor = atmosphere.CalculatedGreenhouseFactor();
            var calculatedTemperatureK = Temperature.ToKelvin(atmosphere.GetParentBaseTemperature()) * ghFactor * albedoFactor;
            
            return Temperature.ToCelsius(calculatedTemperatureK);
        }

        public static float GetParentBaseTemperature(this AtmosphereDB atmosphere)
        {
            if (!atmosphere.OwningEntity.HasDataBlob<SystemBodyInfoDB>())
            {
                throw new ArgumentException("Parent Entity isn't a System Body");
            }

            var parentBody = atmosphere.OwningEntity.GetDataBlob<SystemBodyInfoDB>();
            return parentBody.BaseTemperature;
        }
    }
}
