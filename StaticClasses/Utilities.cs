using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingIsFunKompas.StaticClasses
{
    internal static class Utilities
    {
        public static double[]? ParsingDimensions(string strDimensions)
        {
            string[] dimensions = strDimensions.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (dimensions.Length == 0)
            {
                return null;
            }
            double[] result = new double[dimensions.Length];
            for (int i = 0; i < dimensions.Length; i++)
            {
                try
                {
                    result[i] = double.Parse(dimensions[i]);

                }
                catch (FormatException)
                {
                    return null;
                }
            }
            return result;
        }
    }
}
