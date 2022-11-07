using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawingIsFunKompas.StaticClasses
{
    internal static class Utilities
    {
        public static List<double> ParsingDimensions(string strDimensions)
        {
            string[] dimensions = strDimensions.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<double> result = new();
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i].IndexOf("*") != -1)
                {
                    string[] hasStar = dimensions[i].Split('*', StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        int number = int.Parse(hasStar[0]);
                        double value = double.Parse(hasStar[1]);
                        for (int numb = 0; numb < number; numb++)
                        {
                            result.Add(value);
                        }
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Ошибка");
                    }
                }
                else
                {
                    try
                    {
                        result.Add(double.Parse(dimensions[i]));
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Ошибка");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Получение чистого имени файла
        /// </summary>
        /// <returns></returns>
        public static string[] ClearNameFile()
        {
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\Data"))
            {
                return new string[0];
            }
            FileInfo[] fileinfo = new DirectoryInfo($"{Directory.GetCurrentDirectory()}\\Data").GetFiles();
            List<string> namefile = new();
            foreach (var item in fileinfo)
            {
                namefile.Add(item.Name.Split('.')[0]);
            }
            return namefile.ToArray();
        }
        /// <summary>
        /// Поиск трех одинаковых размеров, для объединения их в один
        /// </summary>
        /// <param name="sourse"></param>
        /// <returns></returns>
        public static double[] FindTriple(double[] sourse, ref List<string> prefix, ref double shift, ref List<double[]> singl)
        {
            prefix.Clear();
            if (sourse.Length == 1)
            {
                prefix.Add("");
                return sourse;
            }
            List<double> result = new();
            int counter = 0;
            for (int i = 0; i < sourse.Length; i++)
            {

                while (i < sourse.Length - 1 && sourse[i] == sourse[i + 1])
                {
                    counter++;
                    i++;
                }
                if (counter > 1)
                {
                    singl.Add(new double[2] {result.Sum(), result.Sum() + sourse[i] });
                    result.Add(sourse[i] * (counter + 1));
                    prefix.Add($"{counter + 1}х{sourse[i]}=");
                    counter = 0;
                    shift = 18;
                }
                else if (counter == 1)
                {
                    result.Add(sourse[i - 1]);
                    result.Add(sourse[i]);
                    prefix.Add("");
                    prefix.Add("");
                    counter = 0;
                }
                else
                {
                    result.Add(sourse[i]);
                    prefix.Add("");
                }
            }
            return result.ToArray();
        }
    }
}
