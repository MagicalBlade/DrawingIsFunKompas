using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using U = DrawingIsFunKompas.StaticClasses.Utilities;
using Dr = DrawingIsFunKompas.StaticClasses.Drawing;
using System.ComponentModel.DataAnnotations;
using KompasAPI7;
using Kompas6API5;
using DrawingIsFunKompas.StaticClasses;
using System.IO;

namespace DrawingIsFunKompas.ViewModels
{
    internal partial class MainWindowViewModel : ObservableValidator
    {
        /// <summary>
        /// Валидация вводимы размеров
        /// </summary>
        const string regDimensionsHole = @"((\s*\d+\*\d+(?(,),\d+\s*|\s*))|(\s*\d+(?(,),\d+\s*|\s*)))+"; //" 12,1 " или " 12 " или " 3*25,1 " с повторением
        const string regDimensions = @"((\s*\d+(?(,),\d+\s*|\s*)))"; //" 12,1 " или " 12 " без повторенияЮ только одно число
        #region Размеры контура накладки
        /// <summary>
        /// Верхний размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _withDimensionsStr = "360,00";
        /// <summary>
        /// Левый размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _heightDimensionsStr = "730,00";
        #endregion
        #region Размеры отверстий
        /// <summary>
        /// Верхний размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _topDimensionsHoleStr = "80 120 80";
        /// <summary>
        /// Нижний размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _bottomDimensionsHoleStr = "80 110 80";
        /// <summary>
        /// Левый размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _heightDimensionsHoleStr = "45 5*80 2*120";
        #endregion
        [ObservableProperty]
        private string[] _holeDiameters = U.ClearNameFile();
        [ObservableProperty]
        private string _selectHoleDiameter = "25";
        [ObservableProperty]
        private bool _isContour = true;

        [ObservableProperty]
        private string? _info;
        /// <summary>
        /// Начертить накладку/отверстие
        /// </summary>
        [RelayCommand]
        private void Drawing()
        {
            Info = "";
            double withDimensions =0;
            double heightDimensions = 0;

            double[] topDimensionsHole = new double[0];
            double[] bottomDimensionsHole = new double[0];
            double[] heightDimensionsHole = new double[0];

            //Проверка на наличие ошибок в полях ввода
            if (!GetErrors(nameof(WithDimensionsStr)).Any())
            {
                withDimensions = U.ParsingDimensions(WithDimensionsStr).ToArray()[0];
            }
            if (!GetErrors(nameof(HeightDimensionsStr)).Any())
            {
                heightDimensions = U.ParsingDimensions(HeightDimensionsStr).ToArray()[0];
            }
            if (!GetErrors(nameof(TopDimensionsHoleStr)).Any())
            {
                topDimensionsHole = U.ParsingDimensions(TopDimensionsHoleStr).ToArray();
            }
            if (!GetErrors(nameof(BottomDimensionsHoleStr)).Any())
            {
                bottomDimensionsHole = U.ParsingDimensions(BottomDimensionsHoleStr).ToArray();
            }
            if (!GetErrors(nameof(HeightDimensionsHoleStr)).Any())
            {
                heightDimensionsHole = U.ParsingDimensions(HeightDimensionsHoleStr).ToArray();
            }
            if (topDimensionsHole.Length != bottomDimensionsHole.Length)
            {
                Info = "Шаги верхних и нижних размеров, по отверстиям, должны быть равны.";
                return;
            }

            KompasObject kompas = (KompasObject)ExMarshal.GetActiveObject("KOMPAS.Application.5");
            IApplication application = (IApplication)kompas.ksGetApplication7();
            IKompasDocument2D kompasDocument2D = (IKompasDocument2D)application.ActiveDocument;
            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2D.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;
            IView view = views.ActiveView;
            IDrawingContainer drawingContainer = (IDrawingContainer)view;
            //Чертим вертикальные линии
            double xTop = 0;
            double xBottom = 0;
            for (int i = 0; i <= topDimensionsHole.Length; i++)
            {
                ILineSegments lineSegments = drawingContainer.LineSegments;
                ILineSegment lineSegment = lineSegments.Add();
                lineSegment.Style = 2; //Тонкая линия
                lineSegment.X1 = (withDimensions - bottomDimensionsHole.Sum()) / 2 + xBottom;
                lineSegment.X2 = (withDimensions - topDimensionsHole.Sum()) / 2 + xTop;
                lineSegment.Y1 = heightDimensionsHole[0];
                lineSegment.Y2 = heightDimensionsHole.Sum();
                lineSegment.Update();
                if (i == topDimensionsHole.Length)
                {
                    break;
                }
                xTop += topDimensionsHole[i];
                xBottom += bottomDimensionsHole[i];
            }
            //Чертим горизонтальные линии
            double y = heightDimensionsHole[0];
            heightDimensionsHole[0] = 0;
            double x1 = (withDimensions - bottomDimensionsHole.Sum()) / 2;
            double x2 = x1 + bottomDimensionsHole.Sum();
            IInsertionObjects insertionObjects = drawingContainer.InsertionObjects;
            IInsertionsManager insertionsManager = (IInsertionsManager)kompasDocument2D;
            InsertionDefinition insertionDefinition = insertionsManager.AddDefinition(
                    Kompas6Constants.ksInsertionTypeEnum.ksTBodyFragment, "", $"{Directory.GetCurrentDirectory()}\\Data\\{SelectHoleDiameter}.frw");
            for (int h = 0; h < heightDimensionsHole.Length; h++)
            {
                y += heightDimensionsHole[h];
                x1 -=  heightDimensionsHole[h] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / (heightDimensionsHole.Sum() - heightDimensionsHole[0]);
                x2 +=  heightDimensionsHole[h] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / (heightDimensionsHole.Sum() - heightDimensionsHole[0]);

                ILineSegments lineSegments = drawingContainer.LineSegments;
                ILineSegment lineSegment = lineSegments.Add();
                lineSegment.Style = 2; //Тонкая линия
                lineSegment.X1 = x1;
                lineSegment.X2 = x2;
                lineSegment.Y1 = y;
                lineSegment.Y2 = y;
                lineSegment.Update();
                //Вставка условного обозначения отверстий
                double xh1 = x1;
                double xh2 = x2;
                for (int w = 0; w <= topDimensionsHole.Length / 2; w++)
                {
                    IInsertionObject insertionObjectx1 = insertionObjects.Add(insertionDefinition);
                    insertionObjectx1.SetPlacement(xh1, y, 0, false);
                    insertionObjectx1.Update();
                    IInsertionObject insertionObjectx2 = insertionObjects.Add(insertionDefinition);
                    insertionObjectx2.SetPlacement(xh2, y, 0, false);
                    insertionObjectx2.Update();
                    xh1 += topDimensionsHole[w];
                    xh2 -= topDimensionsHole[w];
                }
                
            }

            if (IsContour)
            {
                //Чертим контур накладки
                IRectangles rectangles = drawingContainer.Rectangles;
                IRectangle rectangle = rectangles.Add();
                rectangle.X = 0;
                rectangle.Y = 0;
                rectangle.Height = heightDimensions;
                rectangle.Width = withDimensions;
                rectangle.Style = 1; //Основная линия
                rectangle.Update();
            }

            //Простановка размеров
            double shift = 8 / view.Scale;
            ISymbols2DContainer symbols2DContainer = (ISymbols2DContainer)view;
            ILineDimensions lineDimensions = symbols2DContainer.LineDimensions;
            //Горизонтальный размер контура
            DrawingDimension(0, 0, withDimensions, 0, 0,- shift, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal);
            //Вертикальный размер контура
            DrawingDimension(0, 0, 0, heightDimensions, - shift, 1, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical);
            /*
            //Нижние горизонтальные размеры отверстий
            double y1 = 0;
            double y2 = 0;
            for (int i = 0; i < heightDimensionsHole.Length; i++)
            {
                y2 += heightDimensionsHole[i];
                double x1 = ((withDimensions - bottomDimensionsHole.Sum()) / 2) - ((topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / (heightDimensionsHole.Length - 1) * i) / 2;
                double x2 = ((withDimensions - bottomDimensionsHole.Sum()) / 2) - ((topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / (heightDimensionsHole.Length - 1) * i + 1) / 2;
                DrawingDimension(x1, y1, x2, y2, - shift, 10, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical);
                y1 += heightDimensionsHole[i];
            }
            */


            ///Методы
            ///Черчение размеров
            void DrawingDimension(double x1, double y1, double x2, double y2, double x3, double y3,
                Kompas6Constants.ksLineDimensionOrientationEnum orientation)
            {
                ILineDimension linedimension = lineDimensions.Add();
                IDimensionParams dimensionParams = (IDimensionParams)linedimension;
                dimensionParams.ArrowType1 = Kompas6Constants.ksArrowEnum.ksNotch;
                dimensionParams.ArrowType2 = Kompas6Constants.ksArrowEnum.ksNotch;

                linedimension.Orientation = orientation;
                linedimension.X1 = x1;
                linedimension.Y1 = y1;
                linedimension.X2 = x2;
                linedimension.Y2 = y2;
                linedimension.X3 = x3;
                linedimension.Y3 = y3;
                linedimension.Update();
            }

        }
    }
}
