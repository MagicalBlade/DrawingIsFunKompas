﻿using CommunityToolkit.Mvvm.ComponentModel;
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
        private string _withDimensionsStr = "690,00";
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
        private string _topDimensionsHoleStr = "3*80 120 3*80";
        /// <summary>
        /// Нижний размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _bottomDimensionsHoleStr = "3*80 110 3*80";
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
            double[] heightDimensionsHoletemp = new double[heightDimensionsHole.Length];
            heightDimensionsHole.CopyTo(heightDimensionsHoletemp, 0);
            heightDimensionsHoletemp[0] = 0;
            double x1 = (withDimensions - bottomDimensionsHole.Sum()) / 2;
            double x2 = x1 + bottomDimensionsHole.Sum();
            IInsertionObjects insertionObjects = drawingContainer.InsertionObjects;
            IInsertionsManager insertionsManager = (IInsertionsManager)kompasDocument2D;
            InsertionDefinition insertionDefinition = insertionsManager.AddDefinition(
                    Kompas6Constants.ksInsertionTypeEnum.ksTBodyFragment, "", $"{Directory.GetCurrentDirectory()}\\Data\\{SelectHoleDiameter}.frw");
            for (int h = 0; h < heightDimensionsHoletemp.Length; h++)
            {
                y += heightDimensionsHoletemp[h];
                x1 -=  heightDimensionsHoletemp[h] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / (heightDimensionsHoletemp.Sum() - heightDimensionsHoletemp[0]);
                x2 +=  heightDimensionsHoletemp[h] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / (heightDimensionsHoletemp.Sum() - heightDimensionsHoletemp[0]);

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
            ISymbols2DContainer symbols2DContainer = (ISymbols2DContainer)view;
            ILineDimensions lineDimensions = symbols2DContainer.LineDimensions;

            #region Нижние горизонтальные размеры отверстий
            double bottomRim = (withDimensions - bottomDimensionsHole.Sum()) / 2; //Обрез по нижнему краю
            List<string> prefix = new List<string>(); //Превикс размера
            double bdhShift = 8; //Смещиние размера относительно точек построения
            List<double[]> bdhSingl = new(); //Координаты одиночного размера если есть объединенные размеры
            double[] bdh = U.FindTriple(bottomDimensionsHole, ref prefix, ref bdhShift, ref bdhSingl);
            //Первый размер
            DrawingDimension(0, 0, bottomRim, heightDimensionsHole[0], 0, -bdhShift / view.Scale,
                Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "");
            //Промежуточные размеры
            double xbdh1 = bottomRim;
            double xbdh2 = bottomRim + bdh[0];
            for (int i = 0; i < bdh.Length; i++)
            {
                DrawingDimension(xbdh1, heightDimensionsHole[0],
                    xbdh2, heightDimensionsHole[0],
                    xbdh1 + (xbdh2 - xbdh1) / 2, -bdhShift / view.Scale,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, prefix[i], "");
                if (i + 1 == bdh.Length)
                {
                    break;
                }
                xbdh1 += bdh[i];
                xbdh2 += bdh[i + 1];
            }
            //Одиночные размеры если есть объединенные размеры
            if (bdhSingl.Any())
            {
                for (int i = 0; i < bdhSingl.Count; i++)
                {
                    DrawingDimension(bdhSingl[i][0] + bottomRim, heightDimensionsHole[0],
                    bdhSingl[i][1] + bottomRim, heightDimensionsHole[0],
                    bdhSingl[i][0] + bottomRim + 1, -8 / view.Scale,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "");
                }
            }
            //Последний размер
            DrawingDimension(bottomRim + bdh.Sum(), heightDimensionsHole[0],
                withDimensions, 0,
                withDimensions + 20, -bdhShift / view.Scale, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "*"); 
            #endregion


            //Вертикальные размеры отверстий
            /*
            double y1 = 0;
            double y2 = 0;
            for (int i = 0; i < heightDimensionsHole.Length; i++)
            {
                y2 += heightDimensionsHole[i];
                x1 = ((withDimensions - bottomDimensionsHole.Sum()) / 2) - ((topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / (heightDimensionsHole.Length - 1) * i) / 2;
                x2 = ((withDimensions - bottomDimensionsHole.Sum()) / 2) - ((topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / (heightDimensionsHole.Length - 1) * i + 1) / 2;
                DrawingDimension(x1, y1, x2, y2, - shift, 10, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical);
                y1 += heightDimensionsHole[i];
            }
            */

            //Горизонтальный размер контура
            DrawingDimension(0, 0, withDimensions, 0, 0, - (bdhShift + 10) / view.Scale, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "");
            //Вертикальный размер контура
            DrawingDimension(0, 0, 0, heightDimensions, -8 / view.Scale, 1, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical, "", "");


            ///Методы
            ///Черчение размеров
            void DrawingDimension(double x1, double y1, double x2, double y2, double x3, double y3,
                Kompas6Constants.ksLineDimensionOrientationEnum orientation, string prefix, string suffix)
            {
                ILineDimension linedimension = lineDimensions.Add();
                IDimensionParams dimensionParams = (IDimensionParams)linedimension;
                dimensionParams.InitDefaultValues();
                IDimensionText dimensionText = (IDimensionText)linedimension;
                TextLine textLineSuf = dimensionText.Suffix;
                textLineSuf.Str = suffix;
                TextLine textLinePref = dimensionText.Prefix;
                textLinePref.Str = prefix;

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
