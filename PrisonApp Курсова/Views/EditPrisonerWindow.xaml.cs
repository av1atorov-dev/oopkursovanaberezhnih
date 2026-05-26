using System.Windows;
using PrisonApp.ViewModels;

namespace PrisonApp.Views;

public partial class EditPrisonerWindow : Window
{
    public EditPrisonerWindow(EditPrisonerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
    }

    private void ViewModel_RequestClose(object? sender, bool? dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }
}
