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
            FileInfo[] fileinfo = new DirectoryInfo($"{Directory.GetCurrentDirectory()}\\Data").GetFiles();
            List<string> namefile = new();
            foreach (var item in fileinfo)
            {
                namefile.Add(item.Name.Split('.')[0]);
            }
            return namefile.ToArray();
        }
    }
}
