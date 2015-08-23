using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MetroLog;
using Mirko.Controls;
using Mirko.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using WykopSDK.API.Models;
using WykopSDK.Utils;

namespace Mirko.ViewModel
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

        private double _listViewHeaderHeight;
        public double ListViewHeaderHeight
        {
            get { return _listViewHeaderHeight; }
            set { Set(() => ListViewHeaderHeight, ref _listViewHeaderHeight, value); }
        }

        public MainViewModel(NavigationService nav)
        {
            NavService = nav;

            Logger = LogManagerFactory.DefaultLogManager.GetLogger<MainViewModel>();

            var startPage = SimpleIoc.Default.GetInstance<SettingsViewModel>().SelectedStartPage;
            if (startPage == StartPage.HOT)
                _currentPivotItem = 1;
            else if (startPage == StartPage.FAV)
                _currentPivotItem = 2;
            else if (startPage == StartPage.MY)
                _currentPivotItem = 3;

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

            Messenger.Default.Register<NotificationMessage>(this, (e) =>
            {
                if (e.Notification == "Login")
                    TimerCallback(null);
            });

            Messenger.Default.Register<EntryViewModel>(this, "Update", (e) =>
            {
                var col = CurrentCollection();

                SelectedEntry = e;

                var oldEntries = col.Where(x => x.Data.ID == e.Data.ID);
                foreach(var oldEntry in oldEntries)
                {
                    oldEntry.Data.CommentCount = e.Data.CommentCount;
                    oldEntry.Comments.Clear();
                    oldEntry.Comments.AddRange(e.Comments);
                    oldEntry.Data.Date = e.Data.Date;
                    oldEntry.Data.Text = e.Data.Text;
                    oldEntry.Data.VoteCount = e.Data.VoteCount;
                    oldEntry.Data.Voters = e.Data.Voters;
                }

                Messenger.Default.Send(e, "Updated");
            });

            Messenger.Default.Register<uint>(this, "Remove entry", id =>
            {
                var col = CurrentCollection();
                var entry = col.SingleOrDefault(x => x.Data.ID == id);
                if (entry != null)
                    col.Remove(entry);
                else
                {
                    entry = OtherEntries.SingleOrDefault(x => x.Data.ID == id);
                    if (entry != null)
                        OtherEntries.Remove(entry);
                }
            });

            Messenger.Default.Register<Tuple<uint, uint>>(this, "Remove comment", (e) =>
            {
                var rootID = e.Item1;
                var commentID = e.Item2;

                var col = CurrentCollection();
                var entry = col.SingleOrDefault(x => x.Data.ID == rootID);
                if(entry == null)
                    entry = OtherEntries.SingleOrDefault(x => x.Data.ID == rootID);
                if (entry == null && SelectedEntry != null && SelectedEntry.Data.ID == rootID)
                    entry = SelectedEntry;

                if(entry != null)
                {
                    var comment = entry.Comments.SingleOrDefault(x => x.Data.ID == commentID);
                    if(comment != null)
                    {
                        comment.Data.Deleted = true;
                        comment.Data.Text = "[Komentarz usuniêty]";
                    }
                }
            });
        }

        private async void ApiService_NetworkStatusChanged(object sender, NetworkEventArgs e)
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
            set { Set(() => HotEntries, ref _hotEntries, value); }
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

        public MyEntriesTypeEnum MyEntriesType
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("MyEntriesType"))
                    return (MyEntriesTypeEnum)Enum.Parse(typeof(MyEntriesTypeEnum), (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["MyEntriesType"]);
                else
                    return MyEntriesTypeEnum.ALL;
            }

            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["MyEntriesType"] = value.ToString();
                RaisePropertyChanged("MyEntriesType");
            }
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
            var vm = SimpleIoc.Default.GetInstance<NewEntryViewModel>();
            vm.NewEntry.EntryID = 0;
            vm.NewEntry.IsEditing = false;
            vm.GoToNewEntryPage();
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
            //AddNewMirkoEntries.Execute(null);
        }

        private RelayCommand _goToYourProfile = null;
        public RelayCommand GoToYourProfile
        {
            get { return _goToYourProfile ?? (_goToYourProfile = new RelayCommand(ExecuteGoToYourProfile)); }
        }

        private void ExecuteGoToYourProfile()
        {
            var username = App.ApiService.UserInfo.UserName;
            var profilesVM = SimpleIoc.Default.GetInstance<ProfilesViewModel>();
            profilesVM.GoToProfile.Execute(username);
        }

        private RelayCommand _refreshTaggedEntries = null;
        public RelayCommand RefreshTaggedEntries
        {
            get { return _refreshTaggedEntries ?? (_refreshTaggedEntries = new RelayCommand(async () => await CheckNewHashtagEntries())); }
        }

        private RelayCommand<uint> _goToEntryPage = null;
        public RelayCommand<uint> GoToEntryPage
        {
            get { return _goToEntryPage ?? (_goToEntryPage = new RelayCommand<uint>(ExecuteGoToEntryPage)); }
        }

        private async void ExecuteGoToEntryPage(uint entryID)
        {
            EntryViewModel entryVM = OtherEntries.SingleOrDefault(x => x.Data.ID == entryID);

            NavService.NavigateTo("EntryPage");

            if (entryVM == null)
            {
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpis...");
                SelectedEntry = null;
                var entry = await App.ApiService.GetEntry(entryID);

                if (entry != null)
                {
                    entryVM = new EntryViewModel(entry);
                    OtherEntries.Add(entryVM);

                    await StatusBarManager.HideProgressAsync();
                }
                else
                {
                    await StatusBarManager.ShowTextAsync("Nie uda³o siê pobraæ wpisu.");
                }
            }

            SelectedEntry = entryVM;
        }

        private RelayCommand<string> _goToHashtagPage = null;
        public RelayCommand<string> GoToHashtagPage
        {
            get { return _goToHashtagPage ?? (_goToHashtagPage = new RelayCommand<string>(ExecuteGoToHashtagPage)); }
        }

        private void ExecuteGoToHashtagPage(string tag)
        {
            SelectedHashtag = new Meta() { Hashtag = tag };

            TaggedEntries.ClearAll();
            TaggedNewEntries.Clear();

            Messenger.Default.Send<NotificationMessage>(new NotificationMessage("HashtagEntriesPage reload"));
            NavService.NavigateTo("HashtagEntriesPage");
        }

        private RelayCommand _settingsCommand;
        public RelayCommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(() => NavService.NavigateTo("SettingsPage"))); }
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
                WykopSDK.WykopSDK.VaultStorage.RemoveCredentials();
                settingsVM.Delete();
                MyEntries.ClearAll();
                FavEntries.ClearAll();
                Messenger.Default.Send(new NotificationMessage("Logout"));
            }
        }

        private RelayCommand _goToDebugPage;
        public RelayCommand GoToDebugPage
        {
            get { return _goToDebugPage ?? (_goToDebugPage = new RelayCommand(() => NavService.NavigateTo("DebugPage"))); }
        }

        private RelayCommand _goToDonationPage;
        public RelayCommand GoToDonationPage
        {
            get { return _goToDonationPage ?? (_goToDonationPage = new RelayCommand(() => NavService.NavigateTo("DonationPage"))); }
        }

        private RelayCommand _goToBlacklistPage;
        public RelayCommand GoToBlacklistPage
        {
            get { return _goToBlacklistPage ?? (_goToBlacklistPage = new RelayCommand(() => NavService.NavigateTo("BlacklistPage"))); }
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
            await StatusBarManager.ShowTextAndProgressAsync("Sprawdzam nowe wpisy...");

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
                await StatusBarManager.HideProgressAsync();
                return;
            }

            uint firstEntryID = firstEntry.Data.ID;
            int pageIndex = 0;
            var entriesToSend = new List<EntryViewModel>(20);

            while (true)
            {
                var newEntries = await App.ApiService.GetEntries(pageIndex++);

                if (newEntries == null || newEntries.First().ID <= firstEntryID)
                    break;

                var unique = newEntries.Where(x => x.ID > firstEntryID);

                foreach (var uniqueEntry in unique)
                    entriesToSend.Add(new EntryViewModel(uniqueEntry));

                if (unique.Count() < newEntries.Count())
                    break;
            }

            await DispatcherHelper.RunAsync(() => MirkoNewEntries.PrependRange(entriesToSend));
            await StatusBarManager.HideProgressAsync();
        }

        private async Task CheckNewHashtagEntries()
        {
            await StatusBarManager.ShowTextAndProgressAsync("Sprawdzam nowe wpisy...");

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
                await StatusBarManager.HideProgressAsync();
                return;
            }

            uint firstEntryID = firstEntry.Data.ID;
            int pageIndex = 1;
            var entriesToSend = new List<EntryViewModel>(20);

            while (true)
            {
                var taggedEntries = await App.ApiService.GetTaggedEntries(SelectedHashtag.Hashtag, pageIndex++);

                if (taggedEntries == null || taggedEntries.Entries.First().ID <= firstEntryID)
                    break;

                var newEntries = taggedEntries.Entries;
                var unique = newEntries.Where(x => x.ID > firstEntryID);

                foreach(var uniqueEntry in unique)
                    entriesToSend.Add(new EntryViewModel(uniqueEntry));

                if (unique.Count() < newEntries.Count)
                    break;
            }

            await DispatcherHelper.RunAsync(() => TaggedNewEntries.PrependRange(entriesToSend));
            await StatusBarManager.HideProgressAsync();
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
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("VMs", CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync("MainViewModel", CreationCollisionOption.ReplaceExisting);
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
                var settings = ApplicationData.Current.LocalSettings.CreateContainer("MainViewModel", ApplicationDataCreateDisposition.Always).Values;

                settings["CurrentPivotItem"] = CurrentPivotItem;
                settings["FirstIndex"] = firstVisibleIndex;
            }
        }

        public async Task<bool> LoadState(string pageName)
        {
            var settings = ApplicationData.Current.LocalSettings.CreateContainer("MainViewModel", ApplicationDataCreateDisposition.Always).Values;

            var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("VMs");
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
        }

        public string GetName()
        {
            return "MainViewModel";
        }
        #endregion
    }
}