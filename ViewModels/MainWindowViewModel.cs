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
using Kompas6Constants;

namespace DrawingIsFunKompas.ViewModels
{
    internal partial class MainWindowViewModel : ObservableValidator
    {
        /// <summary>
        /// Марка накладки
        /// </summary>
        [ObservableProperty]
        private string _nameMark = "";
        /// <summary>
        /// Валидация вводимы размеров
        /// </summary>
        const string regDimensionsHole = @"((\s*\d+\*\d+(?(,),\d+\s*|\s*))|(\s*\d+(?(,),\d+\s*|\s*)))+"; //" 12,1 " или " 12 " или " 3*25,1 " с повторением
        const string regDimensions = @"((\s*\d+(?(,),\d+\s*|\s*)))"; //" 12,1 " или " 12 " без повторенияЮ только одно число
        #region Размеры контура накладки
        /// <summary>
        /// Ширина накладки
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _withDimensionsStr = "680,00";
        /// <summary>
        /// Допуск на ширину накладки
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _withToleranceStr = "1";
        /// <summary>
        /// Высота накладки
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _heightDimensionsStr = "730,00";
        /// <summary>
        /// Допуск на высоту накладки
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _heightToleranceStr = "1";
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
        /// <summary>
        /// Список диаметров отверстий
        /// </summary>
        [ObservableProperty]
        private string[] _holeDiameters = U.ClearNameFile();
        /// <summary>
        /// Выбранный диаметра отверстия
        /// </summary>
        [ObservableProperty]
        private string _selectHoleDiameter = "25";
        /// <summary>
        /// Черить контур?
        /// </summary>
        [ObservableProperty]
        private bool _isContour = true;
        /// <summary>
        /// Вставлять плавку?
        /// </summary>
        [ObservableProperty]
        private bool _isMelt = true;
        /// <summary>
        /// Вставлять маркировку?
        /// </summary>
        [ObservableProperty]
        private bool _isNameMark = true;

        [ObservableProperty]
        private string _info = "";
        /// <summary>
        /// Начертить накладку/отверстие
        /// </summary>
        [RelayCommand]
        private void Drawing()
        {
            Info = "";
            if (!File.Exists($"{Directory.GetCurrentDirectory()}\\Data\\{SelectHoleDiameter}.frw"))
            {
                Info = "Не найден файл с обозначением отверстия. Проверьте наличие фрагментов в папке Data.";
                return;
            }
            //Проверка на наличие ошибок в полях ввода
            if (GetErrors(nameof(WithDimensionsStr)).Any()) return;
            if (GetErrors(nameof(HeightDimensionsStr)).Any()) return;
            if (GetErrors(nameof(TopDimensionsHoleStr)).Any()) return;
            if (GetErrors(nameof(BottomDimensionsHoleStr)).Any()) return;
            if (GetErrors(nameof(HeightDimensionsHoleStr)).Any()) return;
            if (GetErrors(nameof(HeightToleranceStr)).Any()) return;
            if (GetErrors(nameof(WithToleranceStr)).Any()) return;


            double withDimensions = U.ParsingDimensions(WithDimensionsStr).ToArray()[0];
            double heightDimensions = U.ParsingDimensions(HeightDimensionsStr).ToArray()[0];
            double[] topDimensionsHole = U.ParsingDimensions(TopDimensionsHoleStr).ToArray();
            double[] bottomDimensionsHole = U.ParsingDimensions(BottomDimensionsHoleStr).ToArray();
            double[] heightDimensionsHole = U.ParsingDimensions(HeightDimensionsHoleStr).ToArray();

            if (topDimensionsHole.Length != bottomDimensionsHole.Length)
            {
                Info = "Шаги верхних и нижних размеров, по отверстиям, должны быть равны.";
                return;
            }
            //Увеличиваем ширину накладки на величину расскрытия
            withDimensions += Math.Abs(topDimensionsHole.Sum() - bottomDimensionsHole.Sum());

            KompasObject kompas = (KompasObject)ExMarshal.GetActiveObject("KOMPAS.Application.5");
            if (kompas == null)
            {
                Info = "Запустите компас";
                return;
            }
            IApplication application = (IApplication)kompas.ksGetApplication7();
            IKompasDocument2D kompasDocument2D = (IKompasDocument2D)application.ActiveDocument;
            IKompasDocument2D1 kompasDocument2D1 = (IKompasDocument2D1)kompasDocument2D;
            if (kompasDocument2D == null)
            {
                Info = "Откройте или создайте чертеж или фрагмент";
                return;
            }
            //Получаю интерфейс документ версии API5
            ksDocument2D document2DAPI5 = kompas.TransferInterface(kompasDocument2D, 1,0);
            //Создание группы
            IDrawingGroups drawingGroups = kompasDocument2D1.DrawingGroups;
            DrawingGroup drawingGroup = drawingGroups.Add(true, "Накладка");
            drawingGroup.Open();

            //Включаем объединение "отмены"
            document2DAPI5.ksUndoContainer(true);
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
                x1 -=  heightDimensionsHoletemp[h] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / heightDimensionsHoletemp.Sum();
                x2 +=  heightDimensionsHoletemp[h] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / heightDimensionsHoletemp.Sum();

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
                    //document2DAPI5.ksDestroyObjects(insertionObjectx1.Reference);
                    IInsertionObject insertionObjectx2 = insertionObjects.Add(insertionDefinition);
                    insertionObjectx2.SetPlacement(xh2, y, 0, false);
                    insertionObjectx2.Update();
                    //document2DAPI5.ksDestroyObjects(insertionObjectx2.Reference);
                    xh1 += topDimensionsHole[w];
                    xh2 -= topDimensionsHole[w];
                }
            }

            //Вставка плавки
            if(IsMelt)
            {
                InsertionDefinition iDMelt = insertionsManager.AddDefinition(
                    Kompas6Constants.ksInsertionTypeEnum.ksTBodyFragment, "", $"{Directory.GetCurrentDirectory()}\\Data\\Плавка.frw");
                IInsertionObject iOMelt = insertionObjects.Add(iDMelt);
                if (heightDimensionsHole.Length > 2)
                {
                    iOMelt.SetPlacement(withDimensions / 2, heightDimensionsHole[0] + heightDimensionsHole[1] / 2, 0, false);
                }
                else
                {
                    iOMelt.SetPlacement(withDimensions / 2, heightDimensions / 2, 0, false);
                }
                iOMelt.Update();
            }
            //Вставка маркировки
            if(IsNameMark)
            {
                InsertionDefinition iDNameMArk = insertionsManager.AddDefinition(
                    Kompas6Constants.ksInsertionTypeEnum.ksTBodyFragment, "", $"{Directory.GetCurrentDirectory()}\\Data\\Маркировка.frw");
                IInsertionObject iONameMArk = insertionObjects.Add(iDNameMArk);
                if (heightDimensionsHole.Length > 2)
                {
                    iONameMArk.SetPlacement(withDimensions / 2, heightDimensionsHole[0] + heightDimensionsHole[1] + heightDimensionsHole[2] / 2, 0, false);
                }
                else if(bottomDimensionsHole.Length > 1)
                {
                    iONameMArk.SetPlacement(withDimensions / 2 + bottomDimensionsHole[bottomDimensionsHole.Length / 2], heightDimensions / 2, 0, false);
                }
                else
                {
                    iONameMArk.SetPlacement(withDimensions / 2, heightDimensions / 2 + 70, 0, false);
                }
                iONameMArk.Update();
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
            if (bdhSingl.Any()) bdhShift = 18;
            //Первый размер
            if (IsContour)
            {
                DrawingDimension(0, 0, bottomRim, heightDimensionsHole[0], 0, -bdhShift / view.Scale,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "");
            }
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
            if (IsContour)
            {
                DrawingDimension(bottomRim + bdh.Sum(), heightDimensionsHole[0],
                    withDimensions, 0,
                    withDimensions + 20, -bdhShift / view.Scale, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "*");
            }
            #endregion

            #region Верхние горизонтальные размеры отверстий
            double topRim = (withDimensions - topDimensionsHole.Sum()) / 2; //Обрез по верхнему краю
            double tdhShift = 12; //Смещиние размера относительно точек построения
            List<double[]> tdhSingl = new(); //Координаты одиночного размера если есть объединенные размеры
            double[] tdh = U.FindTriple(topDimensionsHole, ref prefix, ref tdhShift, ref tdhSingl);
            if (tdhSingl.Any()) tdhShift = 18;
            //Первый размер
            if (IsContour)
            {
                DrawingDimension(0, heightDimensions, topRim, heightDimensionsHole.Sum(), 0, heightDimensions + (tdhShift - 7) / view.Scale,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "");
            }
            //Промежуточные размеры
            double xtdh1 = topRim;
            double xtdh2 = topRim + tdh[0];
            for (int i = 0; i < tdh.Length; i++)
            {
                DrawingDimension(xtdh1, heightDimensionsHole.Sum(),
                    xtdh2, heightDimensionsHole.Sum(),
                    xtdh1 + (xtdh2 - xtdh1) / 2, heightDimensions + (tdhShift - 7) / view.Scale,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, prefix[i], "");
                if (i + 1 == tdh.Length)
                {
                    break;
                }
                xtdh1 += tdh[i];
                xtdh2 += tdh[i + 1];
            }
            //Одиночные размеры если есть объединенные размеры
            if (tdhSingl.Any())
            {
                for (int i = 0; i < tdhSingl.Count; i++)
                {
                    DrawingDimension(tdhSingl[i][0] + topRim, heightDimensionsHole.Sum(),
                    tdhSingl[i][1] + topRim, heightDimensionsHole.Sum(),
                    tdhSingl[i][0] + topRim + 1, heightDimensions + 4 / view.Scale,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "");
                }
            }
            //Последний размер
            if (IsContour)
            {
                DrawingDimension(topRim + tdh.Sum(), heightDimensionsHole.Sum(),
                    withDimensions, heightDimensions,
                    withDimensions + 20, heightDimensions + (tdhShift - 7) / view.Scale, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "*");
            }
            #endregion

            #region Вертикальные размеры отверстий
            double hdhShift = 4; //Смещиние размера относительно точек построения
            List<double[]> hdhSingl = new(); //Координаты одиночного размера если есть объединенные размеры
            double[] hdh = U.FindTriple(heightDimensionsHole, ref prefix, ref hdhShift, ref hdhSingl);
            heightDimensionsHoletemp = new double[hdh.Length];
            hdh.CopyTo(heightDimensionsHoletemp, 0);
            heightDimensionsHoletemp[0] = 0;
            //Первый размер
            if (IsContour)
            {
                DrawingDimension(0, 0, bottomRim, heightDimensionsHole[0], -(hdhShift) / view.Scale, 1,
                Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical, "", "");
            }

            //Промежуточные размеры
            double xd1 = bottomRim;
            double xd2 = bottomRim;
            double yhdh1 = hdh[0];
            double yhdh2 = hdh[0] + hdh[1];
            for (int i = 1; i < hdh.Length; i++)
            {
                xd1 -= heightDimensionsHoletemp[i - 1] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / heightDimensionsHoletemp.Sum();
                xd2 -= heightDimensionsHoletemp[i] * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / heightDimensionsHoletemp.Sum();
                DrawingDimension(xd1, yhdh1,
                    xd2, yhdh2,
                    - (hdhShift) / view.Scale , yhdh1 + (yhdh2 - yhdh1) / 2,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical, prefix[i], "");
                if (i + 1 == hdh.Length)
                {
                    break;
                }
                yhdh1 += hdh[i];
                yhdh2 += hdh[i + 1];
            }
            //Одиночные размеры если есть объединенные размеры
            if (hdhSingl.Any())
            {
                double x1singl;
                double x2singl;
                for (int i = 0; i < hdhSingl.Count; i++)
                {
                    x1singl = bottomRim - (hdhSingl[i][0] - hdhSingl[0][0]) * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / heightDimensionsHoletemp.Sum();
                    x2singl = bottomRim - (hdhSingl[i][1] - hdhSingl[0][0]) * (topDimensionsHole.Sum() - bottomDimensionsHole.Sum()) / 2 / heightDimensionsHoletemp.Sum();
                    DrawingDimension(x1singl, hdhSingl[i][0],
                    x2singl, hdhSingl[i][1],
                    - 4 / view.Scale, hdhSingl[i][0] + 1,
                    Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical, "", "");
                }
            }

            //Последний размер
            if (IsContour)
            {
                DrawingDimension(topRim, heightDimensionsHole.Sum(),
                0, heightDimensions,
                - (hdhShift) / view.Scale, heightDimensions + 1,
                Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical, "", "*");
            }
            #endregion

            //Габаритные размеры накладки
            if (IsContour)
            {
                //Горизонтальный размер контура
                DrawingDimension(0, 0, withDimensions, 0, 0, - (bdhShift + 10) / view.Scale, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDHorizontal, "", "", WithToleranceStr);
                //Вертикальный размер контура
                DrawingDimension(0, 0, 0, heightDimensions, - (8 + hdhShift) / view.Scale, 1, Kompas6Constants.ksLineDimensionOrientationEnum.ksLinDVertical, "", "", HeightToleranceStr);
            }

            //Вставляем текст с названием марки
            if (NameMark != "")
            {
                IDrawingTexts drawingTexts = drawingContainer.DrawingTexts;
                IDrawingText drawingText = drawingTexts.Add();
                drawingText.Allocation = Kompas6Constants.ksAllocationEnum.ksAlCentre;
                drawingText.X = withDimensions / 2;
                drawingText.Y = heightDimensions + (tdhShift + 2) / view.Scale;
                IText text = (IText)drawingText;
                text.Str = NameMark;
                ITextLine textline = text.TextLine[0];
                object[] textItems = textline.TextItems;
                foreach (ITextItem item in textItems)
                {
                    ITextFont textFont = (ITextFont)item;
                    textFont.Underline = true;
                    item.Update();
                }
                drawingText.Update();
            }

            //Закрываем группу
            drawingGroup.Close();

            #region Создаем фантом и вставляем группу в чертеж по полученным координатам
            double xPhantom = 0, yPhantom = 0;
            ksPhantom phantom = kompas.GetParamStruct(6);
            phantom.phantom = 1; //Указываем тип фантом "Фантом для сдвига группы"
            ksType1 type1 = phantom.GetPhantomParam();
            type1.gr = drawingGroup.Reference;
            if (document2DAPI5.ksCursorEx(null, ref xPhantom, ref yPhantom, phantom, null) == 0) //Вызываем курсор для указания точки вставки. Если была нажата Esc, прерываем вставку.
            {
                document2DAPI5.ksDeleteObj(type1.gr);
                return;
            }
            document2DAPI5.ksMoveObj(drawingGroup.Reference, xPhantom, yPhantom);
            drawingGroup.Store(); 
            #endregion


            //Завершаем объединение "отмены"
            document2DAPI5.ksUndoContainer(false);

            ///Методы
            ///Черчение размеров
            void DrawingDimension(double x1, double y1, double x2, double y2, double x3, double y3,
                Kompas6Constants.ksLineDimensionOrientationEnum orientation, string prefix, string suffix, string tolerance = "")
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

                if (tolerance != "")
                {
                    dimensionText.DeviationOn = true;
                    dimensionText.HighDeviation.Str = tolerance;
                    dimensionText.LowDeviation.Str = "-" + tolerance;
                    dimensionText.DeviationType = Kompas6Constants.ksDimensionDeviationEnum.ksDimDeviation;
                }

                linedimension.Update();
            }

        }
    }
}
