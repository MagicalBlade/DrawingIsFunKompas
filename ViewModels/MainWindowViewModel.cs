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

namespace DrawingIsFunKompas.ViewModels
{
    internal partial class MainWindowViewModel : ObservableValidator
    {
        /// <summary>
        /// Валидация вводимы размеров
        /// </summary>
        const string regDimensions = @"((\s*\d+\*\d+(?(,),\d+\s*|\s*))|(\s*\d+(?(,),\d+\s*|\s*)))+"; //" 12,1 " или " 12 " или " 3*25,1 "
        const string regDimensionsHole = @"((\s*\d+(?(,),\d+\s*|\s*)))+"; //" 12,1 " или " 12 "
        #region Размеры контура накладки
        /// <summary>
        /// Верхний размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _topDimensionsStr = "200,00";
        /// <summary>
        /// Нижний размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _bottomDimensionsStr = "200,00";
        /// <summary>
        /// Левый размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _leftDimensionsStr = "200,00";
        /// <summary>
        /// Правый размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _rightDimensionsStr = "200,00";
        #endregion

        #region Размеры отверстий
        /// <summary>
        /// Верхний размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _topDimensionsHoleStr = "80,00";
        /// <summary>
        /// Нижний размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _bottomDimensionsHoleStr = "80,00";
        /// <summary>
        /// Левый размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _leftDimensionsHoleStr = "80,00";
        /// <summary>
        /// Правый размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _rightDimensionsHoleStr = "80,00"; 
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
            double[]? topDimensions = new double[0];
            double[]? bottomDimensions = new double[0];
            double[]? leftDimensions = new double[0];
            double[]? rightDimensions = new double[0];
            if (!GetErrors(nameof(TopDimensionsStr)).Any())
            {
                topDimensions = U.ParsingDimensions(TopDimensionsStr).ToArray();
            }
            if (!GetErrors(nameof(BottomDimensionsStr)).Any())
            {
                bottomDimensions = U.ParsingDimensions(TopDimensionsStr).ToArray();
            }
            if (!GetErrors(nameof(LeftDimensionsStr)).Any())
            {
                leftDimensions = U.ParsingDimensions(TopDimensionsStr).ToArray();
            }
            if (!GetErrors(nameof(RightDimensionsStr)).Any())
            {
                rightDimensions = U.ParsingDimensions(TopDimensionsStr).ToArray();
            }

            KompasObject kompas = (KompasObject)ExMarshal.GetActiveObject("KOMPAS.Application.5");
            IApplication application = (IApplication)kompas.ksGetApplication7();
            IKompasDocument2D kompasDocument2D = (IKompasDocument2D)application.ActiveDocument;
            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2D.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;
            IView view = views.ActiveView;
            IDrawingContainer drawingContainer = (IDrawingContainer)view;
            ILineSegments lineSegments = drawingContainer.LineSegments;
            for (int v = 0; v < topDimensions.Length; v++)
            {
                ILineSegment lineSegment = lineSegments.Add();
                lineSegment.X1 = topDimensions[v];
                lineSegment.X2 = bottomDimensions[v];
                lineSegment.Y1 = leftDimensions[0];
                lineSegment.Y2 = leftDimensions.Sum() - leftDimensions[^1];
                lineSegment.Style = 0;
                lineSegment.Update();
            }


            /*
            double[]? bottomDimensions = U.ParsingDimensions(BottomDimensionsStr);
            double[]? leftDimensions = U.ParsingDimensions(LeftDimensionsStr);
            double[]? rightDimensions = U.ParsingDimensions(RightDimensionsStr);

            #region Проверки введенных размеров
            if ((topDimensions == null && bottomDimensions == null) || (leftDimensions == null && rightDimensions == null))
            {
                Info = "Введите размеры";
                return;
            }
            //Если введен только один размер из пары то приравниваем их
            if (topDimensions == null && bottomDimensions != null)
            {
                topDimensions = bottomDimensions;
            }
            else if (bottomDimensions == null && topDimensions != null)
            {
                bottomDimensions = topDimensions;
            }

            if (leftDimensions == null && rightDimensions != null)
            {
                leftDimensions = rightDimensions;
            }
            else if (rightDimensions == null && leftDimensions != null)
            {
                rightDimensions = leftDimensions;
            }
            
            //Проверка на null
            if (topDimensions == null || bottomDimensions == null || leftDimensions == null || rightDimensions == null)
            {
                Info = "Проблема с размерами";
                return;
            }
            //Количество шагов должно быть одинаковое в парных размерах
            if (topDimensions.Length != bottomDimensions.Length || leftDimensions.Length != rightDimensions.Length)
            {
                Info = "Количество шагов должно быть одинаковое в парных размерах";
                return;
            }
            //Проверка на прямоугольность
            if (topDimensions.Sum() != bottomDimensions.Sum() || leftDimensions.Sum() != rightDimensions.Sum())
            {
                Info = "Сумма размеров параллельных сторон должны быть равны";
            }
            #endregion
            */
            //Чертим контур детали
            if (IsContour)
            {

            }

        }
    }
}
