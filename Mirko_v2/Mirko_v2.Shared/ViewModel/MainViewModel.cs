using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WykopAPI.Models;
using Mirko_v2.Utils;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using GalaSoft.MvvmLight.Threading;

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

        private Timer Timer = null;
        private bool StartedOffline = false;

        public MainViewModel()
        {
            Timer = new Timer(TimerCallback, null, 60 * 1000, 60 * 1000);
            StartedOffline = !App.ApiService.IsNetworkAvailable;
            App.ApiService.NetworkStatusChanged += ApiService_NetworkStatusChanged;

            Messenger.Default.Register<EntryViewModel>(this, "Entry UserControl", (e) => SelectedEntry = e);
            Messenger.Default.Register<EmbedViewModel>(this, "Embed UserControl", (e) => SelectedEmbed = e);
        }

        private async void ApiService_NetworkStatusChanged(object sender, WykopAPI.NetworkEventArgs e)
        {
            if(e.IsNetworkAvailable)
            {
                if(StartedOffline)
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        MirkoEntries.Clear();
                        MirkoEntries.HasMoreItems = true;
                    });

                    StartedOffline = false;
                }
                else
                {
                    await DispatcherHelper.RunAsync(() => MirkoEntries.HasMoreItems = true);
                }
            }
        }

        private async void TimerCallback(object state)
        {
            // check new entries
            var currentPage = SimpleIoc.Default.GetInstance<INavigationService>().CurrentPageKey;
            if(currentPage == "PivotPage" )
            {

            } 
            else if(currentPage == "HashtagEntriesPage")
            {
                await CheckNewHashtagEntries();
            }

            if(CurrentPivotItem == 0)
            {
                await SaveCollection(MirkoEntries, "MirkoEntries");
            }
        }

        #region Properties
        private IncrementalLoadingCollection<MirkoEntrySource, EntryViewModel> _mirkoEntries = null;
        public IncrementalLoadingCollection<MirkoEntrySource, EntryViewModel> MirkoEntries
        {
            get { return _mirkoEntries ?? (_mirkoEntries = new IncrementalLoadingCollection<MirkoEntrySource, EntryViewModel>()); }
        }

        private ObservableCollectionEx<EntryViewModel> _mirkoNewEntries = null;
        public ObservableCollectionEx<EntryViewModel> MirkoNewEntries
        {
            get { return _mirkoNewEntries ?? (_mirkoNewEntries = new ObservableCollectionEx<EntryViewModel>()); }
        }

        private Meta _selectedHashtag = null;
        public Meta SelectedHashtag
        {
            get { return _selectedHashtag; }
            set { Set(() => SelectedHashtag, ref _selectedHashtag, value); }
        }

        private IncrementalLoadingCollection<TaggedEntrySource, EntryViewModel> _taggedEntries = null;
        public IncrementalLoadingCollection<TaggedEntrySource, EntryViewModel> TaggedEntries
        {
            get { return _taggedEntries ?? (_taggedEntries = new IncrementalLoadingCollection<TaggedEntrySource, EntryViewModel>()); }
        }

        private ObservableCollectionEx<EntryViewModel> _taggedNewEntries = null;
        public ObservableCollectionEx<EntryViewModel> TaggedNewEntries
        {
            get { return _taggedNewEntries ?? (_taggedNewEntries = new ObservableCollectionEx<EntryViewModel>()); }
        }

        private ObservableCollectionEx<EntryViewModel> _otherEntries = null;
        public ObservableCollectionEx<EntryViewModel> OtherEntries
        {
            get { return _otherEntries ?? (_otherEntries = new ObservableCollectionEx<EntryViewModel>()); }
        }

        private int _currentPivotItem = 0;
        public int CurrentPivotItem
        {
            get { return _currentPivotItem; }
            set { Set(() => CurrentPivotItem, ref _currentPivotItem, value); }
        }

        private EntryViewModel _selectedEntry = null;
        public EntryViewModel SelectedEntry
        {
            get { return _selectedEntry; }
            set { Set(() => SelectedEntry, ref _selectedEntry, value); }
        }

        private CommentViewModel _commentToScrollInto = null;
        public CommentViewModel CommentToScrollInto
        {
            get { return _commentToScrollInto; }
            set { Set(() => CommentToScrollInto, ref _commentToScrollInto, value); }
        }

        private EmbedViewModel _selectedEmbed = null;
        public EmbedViewModel SelectedEmbed
        {
            get { return _selectedEmbed; }
            set { Set(() => SelectedEmbed, ref _selectedEmbed, value); }
        }
        #endregion

        #region Saving/loading
        private async Task SaveCollection(ICollection<EntryViewModel> col, string filename)
        {
            if (col == null || filename == null) return;
            if (col.Count == 0) return;

            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            try
            {
                var file = await folder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                var items = col.Take(50);

                using (var stream = await file.OpenStreamForWriteAsync())
                using (var streamWriter = new StreamWriter(stream))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.None;
                    serializer.Serialize(jsonWriter, items);
                }
            }
            catch(Exception)
            {

            }
        }

        public async Task<List<EntryViewModel>> ReadCollection(string filename)
        {
            if (filename == null) return null;

            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            try
            {
                var file = await folder.GetFileAsync(filename);

                using (var stream = await file.OpenStreamForReadAsync())
                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return serializer.Deserialize<List<EntryViewModel>>(jsonReader);
                }
            } 
            catch(Exception)
            {
                return null;
            }
        }
        #endregion

        #region Commands
        private RelayCommand _addNewEntryCommand;
        public RelayCommand AddNewEntryCommand
        {
            get { return _addNewEntryCommand ?? (_addNewEntryCommand = new RelayCommand(ExecuteAddNewEntryCommand)); }
        }

        private void ExecuteAddNewEntryCommand()
        {
            throw new System.NotImplementedException();
        }

        private RelayCommand<string> _goToHashtagPage = null;
        [JsonIgnore]
        public RelayCommand<string> GoToHashtagPage
        {
            get { return _goToHashtagPage ?? (_goToHashtagPage = new RelayCommand<string>(ExecuteGoToHashtagPage)); }
        }

        private void ExecuteGoToHashtagPage(string tag)
        {
            if (SelectedHashtag != null && SelectedHashtag.Hashtag == tag)
                return;

            SelectedHashtag = new Meta() { Hashtag = tag };

            TaggedEntries.ClearAll();
            TaggedNewEntries.Clear();

            SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo("HashtagEntriesPage");
        }

        private RelayCommand _settingsCommand;
        public RelayCommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(ExecuteSettingsCommand)); }
        }

        private async void ExecuteSettingsCommand()
        {
            SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo("SettingsPage");
            await StatusBarManager.HideStatusBar();
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
        #endregion

        #region Functions
        private async Task CheckNewHashtagEntries()
        {
            await StatusBarManager.ShowTextAndProgress("Sprawdzam nowe wpisy...");

            EntryViewModel firstEntry = null;

            if (TaggedNewEntries.Count > 0)
            {
                firstEntry = TaggedNewEntries.First();
            }
            else if (TaggedEntries.Count > 0)
            {
                firstEntry = TaggedEntries.First();
            }
            else
            {
                await StatusBarManager.HideProgress();
                return;
            }

            uint firstEntryID = firstEntry.Data.ID;
            int pageIndex = 1;
            var entriesToSend = new List<EntryViewModel>(20);

            while (true)
            {
                var taggedEntries = await App.ApiService.getTaggedEntries(SelectedHashtag.Hashtag, pageIndex++);
                var newEntries = taggedEntries.Entries;

                if (newEntries == null || newEntries.First().ID <= firstEntryID)
                    break;

                var unique = newEntries.Where(x => x.ID > firstEntryID);

                foreach(var uniqueEntry in unique)
                    entriesToSend.Add(new EntryViewModel(uniqueEntry));

                if (unique.Count() < newEntries.Count)
                    break;
            }

            await DispatcherHelper.RunAsync(() => TaggedNewEntries.PrependRange(entriesToSend));
            await StatusBarManager.HideProgress();
        }
        #endregion
    }
}