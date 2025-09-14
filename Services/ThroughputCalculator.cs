using CMetalsWS.Data;
using System;
using System.Linq;

namespace CMetalsWS.Services
{
    public class ThroughputCalculator
    {
        private readonly IGaugeWeightResolver _gaugeWeightResolver;

        public ThroughputCalculator(IGaugeWeightResolver gaugeWeightResolver)
        {
            _gaugeWeightResolver = gaugeWeightResolver;
        }

        /// <summary>
        /// Calculates the production throughput in pounds per hour for a given machine and work order.
        /// </summary>
        /// <param name="machine">The machine performing the work.</param>
        /// <param name="workOrder">The work order being processed.</param>
        /// <returns>The calculated throughput in pounds per hour.</returns>
        public decimal CalculateLbsPerHour(Machine machine, WorkOrder workOrder)
        {
            if (machine.RateUnits == RateUnit.WeightPerHour)
            {
                return machine.RateValue;
            }

            if (machine.RateUnits == RateUnit.SheetsPerHour)
            {
                // This calculation requires a representative item from the work order
                // to determine the average weight of a sheet.
                var representativeItem = workOrder.Items.FirstOrDefault();
                if (representativeItem == null) return 0;

                // The data model does not have a "Gauge" property. We will use the ItemCode
                // as a proxy for the gauge, assuming it contains a gauge identifier like "18GA".
                var gauge = representativeItem.ItemCode;
                if (string.IsNullOrEmpty(gauge)) return 0;

                decimal weightPerSqFt = _gaugeWeightResolver.GetWeightPerSquareFoot(gauge);
                if (weightPerSqFt == 0) return 0;

                // If width and length are not available, we cannot calculate the area.
                if (!representativeItem.Width.HasValue || !representativeItem.Length.HasValue || representativeItem.Width.Value == 0 || representativeItem.Length.Value == 0)
                {
                    return 0;
                }

                // Assuming width and length are in inches, convert to square feet.
                decimal widthFt = representativeItem.Width.Value / 12;
                decimal lengthFt = representativeItem.Length.Value / 12;
                decimal areaSqFt = widthFt * lengthFt;

                decimal avgSheetWeight = areaSqFt * weightPerSqFt;

                return machine.RateValue * avgSheetWeight;
            }

            return 0;
        }
    }
}
