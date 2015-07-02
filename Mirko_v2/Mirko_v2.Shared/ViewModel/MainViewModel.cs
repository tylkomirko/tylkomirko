using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MetroLog;
using Mirko_v2.Controls;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WykopAPI.Models;

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
    public class MainViewModel : ViewModelBase, IResumable
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 

        private readonly ILogger Logger = null;
        private NavigationService NavService = null;
        private Timer Timer = null;
        private bool StartedOffline = false;

        public MainViewModel(NavigationService nav)
        {
            NavService = nav;

            Logger = LogManagerFactory.DefaultLogManager.GetLogger<MainViewModel>();

            Timer = new Timer(TimerCallback, null, 60 * 1000, 60 * 1000);

            StartedOffline = !App.ApiService.IsNetworkAvailable;
            App.ApiService.NetworkStatusChanged += ApiService_NetworkStatusChanged;

            Messenger.Default.Register<EmbedViewModel>(this, "Embed UserControl", (e) => SelectedEmbed = e);
            Messenger.Default.Register<EntryViewModel>(this, "Entry UserControl", (e) => 
            {
                SelectedEntry = e;
                SelectedEntryIsHot = false;
            });

            Messenger.Default.Register<EntryViewModel>(this, "Hot Entry UserControl", (e) => 
            {
                SelectedEntry = e;
                SelectedEntryIsHot = true;
            });

            Messenger.Default.Register<EntryViewModel>(this, "Update", (e) =>
            {
                ObservableCollectionEx<EntryViewModel> col = null;

                if (CurrentPivotItem == 0)
                    col = MirkoEntries;
                else if (CurrentPivotItem == 1)
                    col = HotEntries;
                else if (CurrentPivotItem == 2)
                    col = FavEntries;
                else
                    col = MyEntries;

                SelectedEntry = e;

                var ID = e.Data.ID;
                var oldEntry = col.SingleOrDefault(x => x.Data.ID == ID);
                if(oldEntry != null)
                {
                    var index = col.GetIndex(oldEntry);
                    col.Replace(index, e);
                }
            });
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
            var currentPage = NavService.CurrentPageKey;
            if(currentPage == "PivotPage")
            {
                if (CurrentPivotItem == 0)
                    await CheckNewMirkoEntries();
            } 
            else if(currentPage == "HashtagEntriesPage")
            {
                await CheckNewHashtagEntries();
            }

            if(CurrentPivotItem == 0)
                await SaveCollection(MirkoEntries, "MirkoEntries");
            else if(CurrentPivotItem == 1)
                await SaveCollection(HotEntries, "HotEntries");
        }

        private RelayCommand _timerCallbackCommand = null;
        public RelayCommand TimerCallbackCommand
        {
            get { return _timerCallbackCommand ?? (_timerCallbackCommand = new RelayCommand(() => TimerCallback(null))); }
        }

        #region Properties
        private bool _canGoBack = true;
        public bool CanGoBack
        {
            get { return _canGoBack; }
            set { Set(() => CanGoBack, ref _canGoBack, value); }
        }

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

        private IncrementalLoadingCollection<HotEntrySource, EntryViewModel> _hotEntries = null;
        public IncrementalLoadingCollection<HotEntrySource, EntryViewModel> HotEntries
        {
            get { return _hotEntries ?? (_hotEntries = new IncrementalLoadingCollection<HotEntrySource, EntryViewModel>()); }
        }

        public int HotTimeSpan
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("HotTimeSpan"))
                    return (int)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HotTimeSpan"];
                else
                    return 12;
            }

            set
            {
                if (HotTimeSpan != value)
                {
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HotTimeSpan"] = value;
                    RaisePropertyChanged("HotTimeSpan");
                }
            }
        }

        private IncrementalLoadingCollection<FavEntrySource, EntryViewModel> _favEntries = null;
        public IncrementalLoadingCollection<FavEntrySource, EntryViewModel> FavEntries
        {
            get { return _favEntries ?? (_favEntries = new IncrementalLoadingCollection<FavEntrySource, EntryViewModel>()); }
        }

        private IncrementalLoadingCollection<MyEntrySource, EntryViewModel> _myEntries = null;
        public IncrementalLoadingCollection<MyEntrySource, EntryViewModel> MyEntries
        {
            get { return _myEntries ?? (_myEntries = new IncrementalLoadingCollection<MyEntrySource, EntryViewModel>()); }
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

        private bool _selectedEntryIsHot = false;
        public bool SelectedEntryIsHot
        {
            get { return _selectedEntryIsHot; }
            set { _selectedEntryIsHot = value; }
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
            catch(Exception e)
            {
                Logger.Error("Error while saving collection to " + filename, e);
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
            catch(Exception e)
            {
                Logger.Error("Error reading collection from " + filename, e);
                return null;
            }
        }
        #endregion

        #region Commands
        private RelayCommand _addNewEntryCommand = null;
        public RelayCommand AddNewEntryCommand
        {
            get { return _addNewEntryCommand ?? (_addNewEntryCommand = new RelayCommand(ExecuteAddNewEntryCommand)); }
        }

        private void ExecuteAddNewEntryCommand()
        {
            throw new System.NotImplementedException();
        }

        private RelayCommand _refreshMirkoEntries = null;
        public RelayCommand RefreshMirkoEntries
        {
            get { return _refreshMirkoEntries ?? (_refreshMirkoEntries = new RelayCommand(ExecuteRefreshMirkoEntries)); }
        }

        private async void ExecuteRefreshMirkoEntries()
        {
            Logger.Trace("RefreshMirkoEntries");
            await CheckNewMirkoEntries();
            AddNewMirkoEntries.Execute(null);
        }

        private RelayCommand<string> _goToHashtagPage = null;
        public RelayCommand<string> GoToHashtagPage
        {
            get { return _goToHashtagPage ?? (_goToHashtagPage = new RelayCommand<string>(ExecuteGoToHashtagPage)); }
        }

        private RelayCommand _hotTimeSpanChanged = null;
        public RelayCommand HotTimeSpanChanged
        {
            get { return _hotTimeSpanChanged ?? (_hotTimeSpanChanged = new RelayCommand(() => HotEntries.ClearAll())); }
        }

        private void ExecuteGoToHashtagPage(string tag)
        {
            SelectedHashtag = new Meta() { Hashtag = tag };

            TaggedEntries.ClearAll();
            TaggedNewEntries.Clear();

            NavService.NavigateTo("HashtagEntriesPage");
        }

        private RelayCommand _settingsCommand;
        public RelayCommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(ExecuteSettingsCommand)); }
        }

        private async void ExecuteSettingsCommand()
        {
            NavService.NavigateTo("SettingsPage");
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
                NavService.NavigateTo("LoginPage");
            }
            else
            {
                // log out
                settingsVM.Delete();
                MyEntries.ClearAll();
                FavEntries.ClearAll();
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Logout"));
            }
        }

        private RelayCommand _goToDebugPage;
        public RelayCommand GoToDebugPage
        {
            get { return _goToDebugPage ?? (_goToDebugPage = new RelayCommand(() => NavService.NavigateTo("DebugPage"))); }
        }

        private RelayCommand _addNewMirkoEntries = null;
        public RelayCommand AddNewMirkoEntries
        {
            get { return _addNewMirkoEntries ?? (_addNewMirkoEntries = new RelayCommand(ExecuteAddNewMirkoEntries)); }
        }

        private async void ExecuteAddNewMirkoEntries()
        {
            await DispatcherHelper.RunAsync(() => 
            {
                MirkoEntries.PrependRange(MirkoNewEntries);
                MirkoNewEntries.Clear();
            });
        }

        private RelayCommand _addNewTaggedEntries = null;
        public RelayCommand AddNewTaggedEntries
        {
            get { return _addNewTaggedEntries ?? (_addNewTaggedEntries = new RelayCommand(ExecuteAddNewTaggedEntries)); }
        }

        private async void ExecuteAddNewTaggedEntries()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                TaggedEntries.PrependRange(TaggedNewEntries);
                TaggedNewEntries.Clear();
            });
        }
        #endregion

        #region Functions
        private async Task CheckNewMirkoEntries()
        {
            await StatusBarManager.ShowTextAndProgress("Sprawdzam nowe wpisy...");

            EntryViewModel firstEntry = null;

            if (MirkoNewEntries.Count > 0)
            {
                firstEntry = MirkoNewEntries.First();
            }
            else if (MirkoEntries.Count > 0)
            {
                firstEntry = MirkoEntries.First();
            }
            else
            {
                await StatusBarManager.HideProgress();
                return;
            }

            uint firstEntryID = firstEntry.Data.ID;
            int pageIndex = 0;
            var entriesToSend = new List<EntryViewModel>(20);

            while (true)
            {
                var newEntries = await App.ApiService.getEntries(pageIndex++);

                if (newEntries == null || newEntries.First().ID <= firstEntryID)
                    break;

                var unique = newEntries.Where(x => x.ID > firstEntryID);

                foreach (var uniqueEntry in unique)
                    entriesToSend.Add(new EntryViewModel(uniqueEntry));

                if (unique.Count() < newEntries.Count())
                    break;
            }

            await DispatcherHelper.RunAsync(() => MirkoNewEntries.PrependRange(entriesToSend));
            await StatusBarManager.HideProgress();
        }

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

        #region IResumable
        private int _indexToScrollTo = -1;
        public int IndexToScrollTo
        {
            get { return _indexToScrollTo; }
            set { _indexToScrollTo = value; }
        }

        private ObservableCollectionEx<EntryViewModel> CurrentCollection()
        {
            ObservableCollectionEx<EntryViewModel> collection = null;
            if (CurrentPivotItem == 0)
                collection = MirkoEntries;
            else if (CurrentPivotItem == 1)
                collection = HotEntries;
            else if (CurrentPivotItem == 2)
                collection = FavEntries;
            else if (CurrentPivotItem == 3)
                collection = MyEntries;

            return collection;
        }

        private ListViewEx GetCurrentListView()
        {
            var frame = NavService.CurrentFrame();
            foreach (var lv in frame.GetDescendants<ListViewEx>())
            {
                var tag = (string)lv.Tag;
                if (tag == "LV" + CurrentPivotItem)
                    return lv;
            }

            return null;
        }

        private IEnumerable<EntryViewModel> GetCurrentlyVisibleEntries(out int firstIndex)
        {
            var listView = GetCurrentListView();

            var firstIdx = listView.VisibleItems_FirstIdx();
            firstIndex = firstIdx;
            if (firstIdx <= 15)
                firstIdx = 0;

            var lastIdx = listView.VisibleItems_LastIdx() + 2;

            return CurrentCollection().GetRange(firstIdx, lastIdx - firstIdx);
        }

        public async Task SaveState(string pageName)
        {
            try
            {
                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("VMs", Windows.Storage.CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync("MainViewModel", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                int firstVisibleIndex = 0;

                using (var stream = await file.OpenStreamForWriteAsync())
                using (var sw = new StreamWriter(stream))
                using (var writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.None;
                    JsonSerializer serializer = new JsonSerializer();

                    if (pageName == "PivotPage")
                    {
                        var entries = GetCurrentlyVisibleEntries(out firstVisibleIndex);
                        serializer.Serialize(writer, entries);
                    }
                    else if (pageName == "EntryPage")
                    {
                        serializer.Serialize(writer, SelectedEntry);
                    }
                    else if (pageName == "EmbedPage")
                    {
                        serializer.Serialize(writer, SelectedEmbed);
                    }
                }

                if (pageName == "PivotPage")
                {
                    var settings = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("MainViewModel", Windows.Storage.ApplicationDataCreateDisposition.Always).Values;

                    settings["CurrentPivotItem"] = CurrentPivotItem;
                    settings["FirstIndex"] = firstVisibleIndex;
                }
            } catch(Exception e)
            {
                Logger.Error("Error saving state: ", e);
            }
        }

        public async Task<bool> LoadState(string pageName)
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("MainViewModel", Windows.Storage.ApplicationDataCreateDisposition.Always).Values;

                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("VMs");
                var file = await folder.GetFileAsync("MainViewModel");

                using (var stream = await file.OpenStreamForReadAsync())
                using (var sr = new StreamReader(stream))
                using (var reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    if (pageName == "PivotPage")
                    {
                        if (settings.ContainsKey("CurrentPivotItem"))
                            CurrentPivotItem = (int)settings["CurrentPivotItem"];

                        var entries = serializer.Deserialize<List<EntryViewModel>>(reader);
                        if (CurrentPivotItem == 0)
                            MirkoEntries.PrependRange(entries);
                        else if (CurrentPivotItem == 1)
                            HotEntries.PrependRange(entries);
                        else if (CurrentPivotItem == 2)
                            FavEntries.PrependRange(entries);
                        else if (CurrentPivotItem == 3)
                            MyEntries.PrependRange(entries);

                        if (settings.ContainsKey("FirstIndex"))
                        {
                            IndexToScrollTo = (int)settings["FirstIndex"];
                        }
                    }
                    else if (pageName == "EntryPage")
                    {
                        SelectedEntry = serializer.Deserialize<EntryViewModel>(reader);
                    }
                    else if (pageName == "EmbedPage")
                    {
                        SelectedEmbed = serializer.Deserialize<EmbedViewModel>(reader);
                    }
                }

                return true; // success!

            } catch(Exception e)
            {
                Logger.Error("Error loading state: ", e);
                return false;
            }
        }

        public string GetName()
        {
            return "MainViewModel";
        }
        #endregion
    }
}