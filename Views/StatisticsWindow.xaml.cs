using System.Windows;
using PrisonApp.ViewModels;

namespace PrisonApp.Views;

public partial class StatisticsWindow : Window
{
    public StatisticsWindow(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
