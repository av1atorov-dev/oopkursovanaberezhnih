using Microsoft.Win32;
using PrisonApp.Models;
using PrisonApp.Services;
using PrisonApp.ViewModels;
using System.Windows;

namespace PrisonApp.Views;

public partial class ExportDialog : Window
{
    private readonly ExportViewModel _viewModel;
    private readonly IReadOnlyCollection<Prisoner> _prisoners;

    public ExportDialog(IEnumerable<Prisoner> prisoners)
    {
        InitializeComponent();
        _prisoners = prisoners.ToList();
        _viewModel = new ExportViewModel();
        DataContext = _viewModel;
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.HasSelection)
        {
            MessageBox.Show(
                this,
                "Оберіть хоча б один стовпець.",
                "Увага",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV файл (*.csv)|*.csv",
            DefaultExt = ".csv",
            AddExtension = true,
            FileName = $"Ув'язнені_{DateTime.Today:yyyy-MM-dd}.csv"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            CsvExportService.Export(_prisoners, _viewModel.Columns, dialog.FileName);
            MessageBox.Show(
                this,
                "Файл успішно збережено.",
                "Експорт завершено",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Не вдалося зберегти файл: {ex.Message}",
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
