using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PrisonApp.Models;
using PrisonApp.Services;
using PrisonApp.ViewModels;

namespace PrisonApp.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel(new JsonDataService());
        _viewModel.AddRequested += ViewModel_AddRequested;
        _viewModel.EditRequested += ViewModel_EditRequested;
        _viewModel.DeleteRequested += ViewModel_DeleteRequested;
        _viewModel.StatisticsRequested += ViewModel_StatisticsRequested;
        _viewModel.ErrorOccurred += ViewModel_ErrorOccurred;

        DataContext = _viewModel;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        await _viewModel.InitializeAsync();
    }

    private async void ViewModel_AddRequested(object? sender, EventArgs e)
    {
        var editViewModel = new EditPrisonerViewModel();
        var editWindow = new EditPrisonerWindow(editViewModel) { Owner = this };

        if (editWindow.ShowDialog() == true && editViewModel.SavedPrisoner is not null)
        {
            await _viewModel.AddPrisonerAsync(editViewModel.SavedPrisoner);
        }
    }

    private async void ViewModel_EditRequested(object? sender, Prisoner prisoner)
    {
        var editViewModel = new EditPrisonerViewModel(prisoner.Clone());
        var editWindow = new EditPrisonerWindow(editViewModel) { Owner = this };

        if (editWindow.ShowDialog() == true && editViewModel.SavedPrisoner is not null)
        {
            await _viewModel.UpdatePrisonerAsync(editViewModel.SavedPrisoner);
        }
    }

    private async void ViewModel_DeleteRequested(object? sender, Prisoner prisoner)
    {
        var result = MessageBox.Show(
            this,
            $"Видалити запис про {prisoner.FullName}? Цю дію неможливо скасувати.",
            "Підтвердження видалення",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            await _viewModel.DeletePrisonerAsync(prisoner);
        }
    }

    private void ViewModel_StatisticsRequested(object? sender, EventArgs e)
    {
        var statisticsWindow = new StatisticsWindow(_viewModel.CreateStatisticsViewModel())
        {
            Owner = this
        };

        statisticsWindow.ShowDialog();
    }

    private void ViewModel_ErrorOccurred(object? sender, string message)
    {
        MessageBox.Show(this, message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void PrisonersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_viewModel.EditCommand.CanExecute(null) && PrisonersDataGrid.SelectedItem is Prisoner)
        {
            _viewModel.EditCommand.Execute(null);
        }
    }

    private void PrisonersDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(_viewModel.RefreshSequenceNumbers));
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            this,
            "PrisonApp\n\nНастільний застосунок для обліку ув'язнених, пошуку, фільтрації та перегляду статистики.",
            "Про програму",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
