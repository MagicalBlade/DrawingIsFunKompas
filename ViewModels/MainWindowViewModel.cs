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
        private string _heightDimensionsStr = "1200,00";
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
        private string _heightDimensionsHoleStr = "45 80 80";
        #endregion

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
            double[]? withDimensions = new double[0];
            double[]? heightDimensions = new double[0];

            double[]? topDimensionsHole = new double[0];
            double[]? bottomDimensionsHole = new double[0];
            double[]? heightDimensionsHole = new double[0];

            //Проверка на наличие ошибок в полях ввода
            if (!GetErrors(nameof(WithDimensionsStr)).Any())
            {
                withDimensions = U.ParsingDimensions(WithDimensionsStr).ToArray();
            }
            if (!GetErrors(nameof(HeightDimensionsStr)).Any())
            {
                heightDimensions = U.ParsingDimensions(HeightDimensionsStr).ToArray();
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
                lineSegment.X1 = (withDimensions[0] - bottomDimensionsHole.Sum()) / 2 + xBottom;
                lineSegment.X2 = (withDimensions[0] - topDimensionsHole.Sum()) / 2 + xTop;
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
            double y = 0;
            double middlebottom = bottomDimensionsHole[bottomDimensionsHole.Length / 2];
            double middleTop = topDimensionsHole[topDimensionsHole.Length / 2];
            IInsertionObjects insertionObjects = drawingContainer.InsertionObjects;
            IInsertionsManager insertionsManager = (IInsertionsManager)kompasDocument2D;
            InsertionDefinition insertionDefinition = insertionsManager.AddDefinition(
                    Kompas6Constants.ksInsertionTypeEnum.ksTReferenceFragment, "", $"{Directory.GetCurrentDirectory()}\\Data\\D25.frw");
            for (int h = 0; h < heightDimensionsHole.Length; h++)
            {
                double x1 = ((withDimensions[0] - bottomDimensionsHole.Sum()) / 2) - ((middleTop - middlebottom) / (heightDimensionsHole.Length - 1) * h) / 2;
                double x2 = ((withDimensions[0] + bottomDimensionsHole.Sum()) / 2) + ((middleTop - middlebottom) / (heightDimensionsHole.Length - 1) * h) / 2;
                y += heightDimensionsHole[h];
                ILineSegments lineSegments = drawingContainer.LineSegments;
                ILineSegment lineSegment = lineSegments.Add();
                lineSegment.Style = 2; //Тонкая линия
                lineSegment.X1 = x1;
                lineSegment.X2 = x2;
                lineSegment.Y1 = y;
                lineSegment.Y2 = y;
                lineSegment.Update();

                
                //double x = ((withDimensions[0] - bottomDimensionsHole.Sum()) / 2) - ((middleTop - middlebottom) / (heightDimensionsHole.Length - 1) * h) / 2;
                for (int w = 0; w <= topDimensionsHole.Length / 2; w++)
                {
                    IInsertionObject insertionObjectx1 = insertionObjects.Add(insertionDefinition);
                    insertionObjectx1.SetPlacement(x1, y, 0, false);
                    insertionObjectx1.Update();
                    IInsertionObject insertionObjectx2 = insertionObjects.Add(insertionDefinition);
                    insertionObjectx2.SetPlacement(x2, y, 0, false);
                    insertionObjectx2.Update();
                    x1 += topDimensionsHole[w];
                    x2 -= topDimensionsHole[w];
                }
            }


            if (IsContour)
            {
                //Чертим контур накладки
                IRectangles rectangles = drawingContainer.Rectangles;
                IRectangle rectangle = rectangles.Add();
                rectangle.X = 0;
                rectangle.Y = 0;
                rectangle.Height = heightDimensions[0];
                rectangle.Width = withDimensions[0];
                rectangle.Style = 1; //Основная линия
                rectangle.Update();
            }

        }
    }
}
