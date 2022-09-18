using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingIsFunKompas.StaticClasses
{
    internal static class Drawing
    {
        public static double[] TopDimensions { get => _topDimensions; set => _topDimensions = value; }
        private static double[] _topDimensions;
        public static double[] BottomDimensions { get => _bottomDimensions; set => _bottomDimensions = value; }
        private static double[] _bottomDimensions;
        public static double[] LeftDimensions { get => _leftDimensions; set => _leftDimensions = value; }
        private static double[] _leftDimensions;
        public static double[] RightDimensions { get => _rightDimensions; set => _rightDimensions = value; }
        private static double[] _rightDimensions;

        public static double HeightContour { get => TopDimensions.Sum();}
        public static double WidthContour { get => _widthContour; set => _widthContour = value; }
        private static double _widthContour;


    }
}
