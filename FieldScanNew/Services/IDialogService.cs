using FieldScanNew.ViewModels;

namespace FieldScanNew.Services
{
    public interface IDialogService
    {
        void ShowDialog(IStepViewModel viewModel);
    }
}