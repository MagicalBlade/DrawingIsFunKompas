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

namespace DrawingIsFunKompas.ViewModels
{
    internal partial class MainWindowViewModel : ObservableValidator
    {
        const string regDimensions = @"(\s*\d+(?(\.)\.\d+\s*|\s*))+";
        /// <summary>
        /// Верхний размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _topDimensionsStr = "80.00";


        /// <summary>
        /// Нижний размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _bottomDimensionsStr = "80.00";
        /// <summary>
        /// Левый размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _leftDimensionsStr = "80.00";
        /// <summary>
        /// Правый размер
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(regDimensions)]
        private string _rightDimensionsStr = "80.00";

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
            double[]? topDimensions = U.ParsingDimensions(TopDimensionsStr);
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
            //Чертим контур детали
            if (IsContour)
            {

            }

        }
    }
}
