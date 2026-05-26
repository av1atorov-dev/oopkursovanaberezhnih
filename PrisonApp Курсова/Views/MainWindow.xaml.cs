using Microsoft.Win32;
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
        _viewModel.CsvExportRequested += ViewModel_CsvExportRequested;
        _viewModel.JsonExportRequested += ViewModel_JsonExportRequested;
        _viewModel.JsonImportRequested += ViewModel_JsonImportRequested;
        _viewModel.ErrorOccurred += ViewModel_ErrorOccurred;

        DataContext = _viewModel;
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        _viewModel.Initialize();
    }

    private void ViewModel_AddRequested(object? sender, EventArgs e)
    {
        var editViewModel = new EditPrisonerViewModel();
        var editWindow = new EditPrisonerWindow(editViewModel) { Owner = this };

        if (editWindow.ShowDialog() == true && editViewModel.SavedPrisoner is not null)
        {
            _viewModel.AddPrisoner(editViewModel.SavedPrisoner);
        }
    }

    private void ViewModel_EditRequested(object? sender, Prisoner prisoner)
    {
        var editViewModel = new EditPrisonerViewModel(prisoner.Clone());
        var editWindow = new EditPrisonerWindow(editViewModel) { Owner = this };

        if (editWindow.ShowDialog() == true && editViewModel.SavedPrisoner is not null)
        {
            _viewModel.UpdatePrisoner(editViewModel.SavedPrisoner);
        }
    }

    private void ViewModel_DeleteRequested(object? sender, Prisoner prisoner)
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
            _viewModel.DeletePrisoner(prisoner);
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

    private void ViewModel_CsvExportRequested(object? sender, IReadOnlyList<Prisoner> prisoners)
    {
        var dialog = new ExportDialog(prisoners) { Owner = this };
        dialog.ShowDialog();
    }

    private void ViewModel_JsonExportRequested(object? sender, IReadOnlyList<Prisoner> prisoners)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON файл (*.json)|*.json",
            DefaultExt = ".json",
            AddExtension = true,
            FileName = $"Ув'язнені_{DateTime.Today:yyyy-MM-dd}.json"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        if (_viewModel.ExportJson(dialog.FileName, prisoners))
        {
            MessageBox.Show(this, "JSON-файл успішно збережено.", "Експорт завершено", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ViewModel_JsonImportRequested(object? sender, EventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON файл (*.json)|*.json",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            "Імпорт замінить поточний список ув'язнених даними з вибраного JSON-файлу. Продовжити?",
            "Підтвердження імпорту",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        if (_viewModel.ImportJson(dialog.FileName))
        {
            MessageBox.Show(this, "JSON-файл успішно імпортовано.", "Імпорт завершено", MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
