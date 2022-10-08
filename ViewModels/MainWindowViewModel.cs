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
        const string regDimensionsHole = @"((\s*\d+\*\d+(?(,),\d+\s*|\s*))|(\s*\d+(?(,),\d+\s*|\s*)))+"; //" 12,1 " или " 12 " или " 3*25,1 "
        const string regDimensions = @"((\s*\d+(?(,),\d+\s*|\s*)))+"; //" 12,1 " или " 12 "
        #region Размеры контура накладки
        /// <summary>
        /// Верхний размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _withDimensionsStr = "200,00";
        /// <summary>
        /// Левый размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _heightDimensionsStr = "200,00";
        #endregion

        #region Размеры отверстий
        /// <summary>
        /// Верхний размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _topDimensionsHoleStr = "80,00";
        /// <summary>
        /// Нижний размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _bottomDimensionsHoleStr = "80,00";
        /// <summary>
        /// Левый размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
        private string _leftDimensionsHoleStr = "80,00";
        /// <summary>
        /// Правый размер отверстий
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensionsHole)]
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
            double[]? withDimensions = new double[0];
            double[]? bottomDimensions = new double[0];
            double[]? heightDimensions = new double[0];
            double[]? rightDimensions = new double[0];
            //Проверка на наличие ошибок в полях ввода
            if (!GetErrors(nameof(WithDimensionsStr)).Any())
            {
                withDimensions = U.ParsingDimensions(WithDimensionsStr).ToArray();
            }
            if (!GetErrors(nameof(HeightDimensionsStr)).Any())
            {
                heightDimensions = U.ParsingDimensions(HeightDimensionsStr).ToArray();
            }

            KompasObject kompas = (KompasObject)ExMarshal.GetActiveObject("KOMPAS.Application.5");
            IApplication application = (IApplication)kompas.ksGetApplication7();
            IKompasDocument2D kompasDocument2D = (IKompasDocument2D)application.ActiveDocument;
            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2D.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;
            IView view = views.ActiveView;
            IDrawingContainer drawingContainer = (IDrawingContainer)view;
            ILineSegments lineSegments = drawingContainer.LineSegments;
            for (int v = 0; v < withDimensions.Length; v++)
            {
                ILineSegment lineSegment = lineSegments.Add();
                lineSegment.X1 = withDimensions[v];
                lineSegment.X2 = bottomDimensions[v];
                lineSegment.Y1 = heightDimensions[0];
                lineSegment.Y2 = heightDimensions.Sum() - heightDimensions[^1];
                lineSegment.Style = 0;
                lineSegment.Update();
            }


           
            if (IsContour)
            {

            }

        }
    }
}
