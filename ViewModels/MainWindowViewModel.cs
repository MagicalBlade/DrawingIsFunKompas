using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using U = DrawingIsFunKompas.StaticClasses.Utilities;

namespace DrawingIsFunKompas.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _topDimensions;
        [ObservableProperty]
        private string? _bottomDimensions;
        [ObservableProperty]
        private string? _leftDimensions;
        [ObservableProperty]
        private string? _rightDimensions;

        [ObservableProperty]
        private string? _info;

        [RelayCommand]
        private void Drawing()
        {
            foreach (var item in U.ParsingDimensions(TopDimensions))
            {
                MessageBox.Show($"{item}");
            }
        }
    }
}
