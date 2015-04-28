using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WykopAPI.Models;
using Mirko_v2.Utils;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;

namespace Mirko_v2.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 

        public MainViewModel()
        {

        }

        private IncrementalLoadingCollection<MirkoEntrySource, EntryViewModel> _mirkoEntries = null;
        public IncrementalLoadingCollection<MirkoEntrySource, EntryViewModel> MirkoEntries
        {
            get { return _mirkoEntries ?? (_mirkoEntries = new IncrementalLoadingCollection<MirkoEntrySource, EntryViewModel>()); }
        }

        private RelayCommand _addNewEntryCommand;
        public RelayCommand AddNewEntryCommand
        {
            get { return _addNewEntryCommand ?? (_addNewEntryCommand = new RelayCommand(ExecuteAddNewEntryCommand)); }
        }

        private void ExecuteAddNewEntryCommand()
        {
            throw new System.NotImplementedException();
        }

        private RelayCommand _settingsCommand;
        public RelayCommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(ExecuteSettingsCommand)); }
        }

        private void ExecuteSettingsCommand()
        {
            throw new System.NotImplementedException();
        }

        private RelayCommand _logInOutCommand;
        public RelayCommand LogInOutCommand
        {
            get { return _logInOutCommand ?? (_logInOutCommand = new RelayCommand(ExecuteLogInOutCommand)); }
        }

        private void ExecuteLogInOutCommand()
        {
            var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();
            if (settingsVM.UserInfo == null)
            {
                // login
                SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo("LoginPage");
            }
            else
            {
                // log out
                settingsVM.Delete();
            }
        }
        
    }
}