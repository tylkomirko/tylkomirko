using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MetroLog;
using Mirko.Utils;
using NotificationsExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
{
    public class HashtagInfoContainer : INotifyPropertyChanged, IComparable
    {
        public string Name { get; set; }

        private uint _count;
        public uint Count
        {
            get { return _count; }
            set 
            { 
                _count = value;
                OnPropertyChanged();
            }
        }

        public HashtagInfoContainer(string name, uint count)
        {
            Name = name;
            Count = count;
        }

        public int CompareTo(object obj)
        {
            var second = (HashtagInfoContainer)obj;

            var count = second.Count.CompareTo(this.Count);
            if (count == 0)
                return this.Name.CompareTo(second.Name);
            else
                return count;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NotificationsViewModel : ViewModelBase
    {
        private readonly ILogger Logger = null;

        private Timer Timer = null;
        private SemaphoreSlim TimerSemaphore = null;
        private NavigationService NavService = null;

        public NotificationsViewModel(NavigationService nav)
        {
            Timer = new Timer(TimerCallback, null, 100, 60*1000);
            TimerSemaphore = new SemaphoreSlim(1);
            NavService = nav;

            Logger = LogManagerFactory.DefaultLogManager.GetLogger<NotificationsViewModel>();

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
            Messenger.Default.Register<NotificationMessage<string>>(this, ReadMessage);
            Messenger.Default.Register<NotificationMessage<Notification>>(this, ReadMessage);
            Messenger.Default.Register<NotificationMessage<IEnumerable<uint>>>(this, ReadMessage);
            Messenger.Default.Register<EntryViewModel>(this, "Update", (e) =>
            {
                if (e == null) return;

                try
                {
                    var oldEntries = HashtagFlipEntries.Where(x => x.Data.ID == e.Data.ID);
                    foreach (var oldEntry in oldEntries)
                    {
                        oldEntry.Data.CommentCount = e.Data.CommentCount;
                        oldEntry.Comments.Clear();
                        oldEntry.Comments.AddRange(e.Comments);
                        oldEntry.Data.Date = e.Data.Date;
                        oldEntry.Data.Text = e.Data.Text;
                        oldEntry.Data.VoteCount = e.Data.VoteCount;
                        oldEntry.Data.Voters = e.Data.Voters;
                    }

                    Messenger.Default.Send(e, "HashtagFlipEntries Updated");
                }
                catch (Exception ex)
                {
                    Logger.Warn("NotificationsViewModel, message Update: ", ex);
                    App.TelemetryClient.TrackException(ex);
                }
            });

            ObservedHashtags = SimpleIoc.Default.GetInstance<CacheViewModel>().ObservedHashtags;
            ObservedHashtags.CollectionChanged += (s, e) => UpdateHashtagsCollection();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            TimerSemaphore?.Dispose();
        }

        private void UpdateBadge()
        {
            uint count = HashtagNotificationsCount + AtNotificationsCount + (uint)PMNotifications.Count;
            NotificationsManager.SetBadge(count);
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Logout")
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    this.HashtagsCollection.Clear();
                    this.HashtagsDictionary.Clear();
                    this.ObservedHashtags.Clear();
                    this.AtNotifications.ClearAll();
                    this.PMNotifications.Clear();

                    this.HashtagNotificationsCount = 0;
                    this.AtNotificationsCount = 0;
                    this.PMNotificationsCount = 0;
                });
            }
            else if(obj.Notification == "Login" || obj.Notification == "Wake up")
            {
                TimerCallback(null);
            }
        }

        private void ReadMessage(NotificationMessage<string> obj)
        {
            if(obj.Notification == "Clear PM")
            {
                var userName = obj.Content;
                DeletePMNotifications(userName);
            }
        }

        private void ReadMessage(NotificationMessage<Notification> obj)
        {
            if(obj.Notification == "Go to")
            {
                this.SelectedAtNotification = new NotificationViewModel(obj.Content);
                GoToNotification.Execute(null);
            }
        }

        private void ReadMessage(NotificationMessage<IEnumerable<uint>> obj)
        {
            if (obj.Notification == "Remove notifications from entries IDs")
            {
                foreach (var ID in obj.Content)
                    DeleteHashtagNotificationFromEntryID.Execute(ID);
            }
        }

        private async void TimerCallback(object state)
        {
            await TimerSemaphore.WaitAsync();

            try
            {
                await Task.Run(async () =>
                {
                    await CheckHashtagNotifications();
                    await CheckNotifications();
                });
            }
            catch (Exception) { }

            // i dunno what it's for, but i'm too scared to remove it
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Update"));
            UpdateBadge();

            TimerSemaphore.Release();
        }

        #region AppHeader commands
        private RelayCommand _logoTappedCommand = null;
        public RelayCommand LogoTappedCommand
        {
            get
            {
                return _logoTappedCommand ?? (_logoTappedCommand = new RelayCommand(() =>
                {
                    if (App.IsMobile)
                        NavService.NavigateTo("PivotPage");
                }));
            }       
        }

        private RelayCommand _hashtagTappedCommand = null;
        public RelayCommand HashtagTappedCommand
        {
            get { return _hashtagTappedCommand ?? (_hashtagTappedCommand = new RelayCommand(ExecuteHashtagTappedCommand)); }
        }

        private async void ExecuteHashtagTappedCommand()
        {
            if (HashtagNotificationsCount == 0)
            {
                NavService.NavigateTo("HashtagSelectionPage");
            }
            else if (HashtagNotificationsCount == 1)
            {
                NotificationViewModel n = null;
                string hashtag = null;

                foreach (var tmp in HashtagsDictionary)
                {
                    var collection = tmp.Value;
                    if (collection.Count == 1)
                    {
                        n = collection[0];
                        hashtag = tmp.Key;
                    }
                }

                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                var entryID = n.Data.Entry.ID;
                EntryViewModel entryVM = mainVM.OtherEntries.SingleOrDefault(x => x.Data.ID == entryID);

                NavService.NavigateTo("EntryPage");

                if(entryVM == null)
                {
                    await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpis...");
                    mainVM.SelectedEntry = null;
                    var entry = await App.ApiService.GetEntry(entryID);

                    if (entry != null)
                    {
                        entryVM = new EntryViewModel(entry);
                        mainVM.OtherEntries.Add(entryVM);

                        await StatusBarManager.HideProgressAsync();
                    }
                    else
                    {
                        await StatusBarManager.ShowTextAsync("Nie udało się pobrać wpisu.");
                    }
                }

                mainVM.SelectedEntry = entryVM;
                await ExecuteDeleteHashtagNotification(n.Data.ID);
            }
            else if (HashtagNotificationsCount > 1)
            {
                // check if all notifications relate to the same hashtag
                var tags = HashtagsCollection.Where(x => x.Count > 0).Distinct();

                if (tags.Count() == 1) // all notifications belong to one tag
                {
                    var hashtag = tags.First();
                    CurrentHashtag = hashtag;
                    CurrentHashtagNotifications = HashtagsDictionary[hashtag.Name];
                    NavService.NavigateTo("HashtagNotificationsPage");
                }
                else
                {
                    NavService.NavigateTo("HashtagSelectionPage");
                }

            }
        }

        private RelayCommand _atTappedCommand = null;
        public RelayCommand AtTappedCommand
        {
            get { return _atTappedCommand ?? (_atTappedCommand = new RelayCommand(ExecuteAtTappedCommand)); }
        }

        private void ExecuteAtTappedCommand()
        {
            if (AtNotificationsCount == 1)
            {
                var notificationVM = AtNotifications.First();

                if (notificationVM.Data.Type == NotificationType.EntryDirected 
                    || notificationVM.Data.Type == NotificationType.CommentDirected)
                {
                    SelectedAtNotification = notificationVM;
                    GoToNotification.Execute(null);
                }
                else
                {
                    NavService.NavigateTo("AtNotificationsPage");
                }
            }
            else
            {
                NavService.NavigateTo("AtNotificationsPage");
            }
        }

        private RelayCommand _pmTappedCommand = null;
        public RelayCommand PMTappedCommand
        {
            get { return _pmTappedCommand ?? (_pmTappedCommand = new RelayCommand(ExecutePMTappedCommand)); }
        }

        private void ExecutePMTappedCommand()
        {
            if (PMNotifications.Count == 1)
            {
                var notification = PMNotifications.First();
                var userName = notification.AuthorName;
                var pmVM = SimpleIoc.Default.GetInstance<MessagesViewModel>();

                var conversation = pmVM.ConversationsList.FirstOrDefault(x => x.Data.AuthorName == userName);
                if (conversation.Data != null)
                {
                    pmVM.CurrentConversation = conversation;
                    conversation.UpdateMessagesCommand.Execute(null);

                    NavService.NavigateTo("ConversationPage");

                    DeletePMNotification.Execute(notification.ID);
                }
                else
                {
                    NavService.NavigateTo("ConversationsPage");
                }
            }
            else
            {
                NavService.NavigateTo("ConversationsPage");
            }
        }
        #endregion

        #region Hashtag
        private ObservableCollectionEx<string> ObservedHashtags = null;

        private uint _hashtagNotificationsCount;
        public uint HashtagNotificationsCount
        {
            get { return _hashtagNotificationsCount; }
            set { Set(() => HashtagNotificationsCount, ref _hashtagNotificationsCount, value); }
        }

        private HashtagInfoContainer _currentHashtag = null;
        public HashtagInfoContainer CurrentHashtag
        {
            get { return _currentHashtag; }
            set { Set(() => CurrentHashtag, ref _currentHashtag, value); }
        }

        private ObservableCollectionEx<NotificationViewModel> _currentHashtagNotifications = null;
        public ObservableCollectionEx<NotificationViewModel> CurrentHashtagNotifications
        {
            get { return _currentHashtagNotifications; }
            set { Set(() => CurrentHashtagNotifications, ref _currentHashtagNotifications, value); }
        }

        private NotificationViewModel _selectedHashtagNotification = null;
        public NotificationViewModel SelectedHashtagNotification
        {
            get { return _selectedHashtagNotification; }
            set { Set(() => SelectedHashtagNotification, ref _selectedHashtagNotification, value); }
        }

        private Dictionary<string, ObservableCollectionEx<NotificationViewModel>> _hashtagsDictionary;
        public Dictionary<string, ObservableCollectionEx<NotificationViewModel>> HashtagsDictionary
        {
            get { return _hashtagsDictionary ?? (_hashtagsDictionary = new Dictionary<string, ObservableCollectionEx<NotificationViewModel>>()); }
        }

        private ObservableCollectionEx<HashtagInfoContainer> _hashtagsCollection;
        public ObservableCollectionEx<HashtagInfoContainer> HashtagsCollection
        {
            get { return _hashtagsCollection ?? (_hashtagsCollection = new ObservableCollectionEx<HashtagInfoContainer>()); }
        }

        private ObservableCollectionEx<EntryViewModel> _hashtagFlipEntries = null;
        public ObservableCollectionEx<EntryViewModel> HashtagFlipEntries
        {
            get { return _hashtagFlipEntries ?? (_hashtagFlipEntries = new ObservableCollectionEx<EntryViewModel>()); }
        }

        private EntryViewModel _hashtagFlipCurrentEntry = null;
        public EntryViewModel HashtagFlipCurrentEntry
        {
            get { return _hashtagFlipCurrentEntry; }
            set { Set(() => HashtagFlipCurrentEntry, ref _hashtagFlipCurrentEntry, value); }
        }

        private RelayCommand<uint> _deleteHashtagNotification = null;
        public RelayCommand<uint> DeleteHashtagNotification
        {
            get { return _deleteHashtagNotification ?? (_deleteHashtagNotification = new RelayCommand<uint>(async (id) => await ExecuteDeleteHashtagNotification(id))); }
        }

        private RelayCommand<uint> _deleteHashtagNotificationFromEntryID = null;
        public RelayCommand<uint> DeleteHashtagNotificationFromEntryID
        {
            get { return _deleteHashtagNotificationFromEntryID ?? (_deleteHashtagNotificationFromEntryID = new RelayCommand<uint>(async (id) => await ExecuteDeleteHashtagNotification(id, false))); }
        }

        private async Task ExecuteDeleteHashtagNotification(uint ID, bool notificationID = true /* else: entryID */)
        {
            try
            {
                ObservableCollectionEx<NotificationViewModel> collection = null;
                NotificationViewModel notification = null;

                foreach(var col in HashtagsDictionary.Values)
                {
                    NotificationViewModel tmp = null;
                    if(notificationID)
                        tmp = col.SingleOrDefault(x => x.Data.ID == ID);
                    else
                        tmp = col.SingleOrDefault(x => x.Data.Entry.ID == ID);

                    if (tmp != null)
                    {
                        notification = tmp;
                        collection = col;
                        break;
                    }
                }

                if (notification != null && notification.Data.IsNew)
                {
                    notification.MarkAsReadCommand.Execute(null);
                    await DispatcherHelper.RunAsync(() =>
                    {
                        collection.Remove(notification);
                        if (collection.Count == 0)
                        {
                            var keyValuePair = HashtagsDictionary.FirstOrDefault(x => x.Value.Equals(collection));
                            if(!string.IsNullOrEmpty(keyValuePair.Key))
                                HashtagsDictionary.Remove(keyValuePair.Key);
                        }
                    });
                    UpdateHashtagsCollection();

                    UpdateBadge();
                }
            }
            catch (Exception e) 
            {
                string txt = notificationID ? " ID " : " entry ID ";
                Logger.Error("Couldn't delete hashtag notification with " + txt + ID, e);
            }
        }

        private RelayCommand<string> _deleteHashtagNotifications = null;
        public RelayCommand<string> DeleteHashtagNotifications
        {
            get { return _deleteHashtagNotifications ?? (_deleteHashtagNotifications = new RelayCommand<string>(async (h) => await ExecuteDeleteHashtagNotifications(h))); }
        }

        private async Task ExecuteDeleteHashtagNotifications(string hashtag)
        {
            if (!HashtagsDictionary.ContainsKey(hashtag) || string.IsNullOrEmpty(hashtag)) return;

            StatusBarManager.ShowTextAndProgress("Usuwam powiadomienia...");

            ObservableCollectionEx<NotificationViewModel> notifications = null;
            HashtagsDictionary.TryGetValue(hashtag, out notifications);

            if(notifications == null)
            {
                StatusBarManager.HideProgress();
                return;
            }

            var notificationsCopy = notifications.ToList(); // make a copy
            foreach (var notification in notificationsCopy)
            {
                bool success = await App.ApiService.ReadNotification(notification.Data.ID);
                Logger.Trace("Removing notification " + notification.Data.ID + ". success: " + success);
            }

            notifications.Clear();
            
            HashtagsDictionary.Remove(hashtag);
            UpdateHashtagsCollection();

            StatusBarManager.ShowText("Powiadomienia zostały usunięte.");
        }

        private RelayCommand _deleteAllHashtagNotifications = null;
        public RelayCommand DeleteAllHashtagNotifications
        {
            get { return _deleteAllHashtagNotifications ?? (_deleteAllHashtagNotifications = new RelayCommand(ExecuteDeleteAllHashtagNotifications)); }
        }

        private async void ExecuteDeleteAllHashtagNotifications()
        {
            var success = await App.ApiService.ReadHashtagNotifications();
            if(success)
            {
                HashtagsDictionary.Clear();
                UpdateHashtagsCollection();
            }
        }

        private RelayCommand _deleteCurrentHashtagNotifications = null;
        public RelayCommand DeleteCurrentHashtagNotifications
        {
            get { return _deleteCurrentHashtagNotifications ?? (_deleteCurrentHashtagNotifications = new RelayCommand(ExecuteDeleteCurrentHashtagNotifications)); }
        }

        private void ExecuteDeleteCurrentHashtagNotifications()
        {
            DeleteHashtagNotifications.Execute(CurrentHashtag.Name);
            NavService.GoBack();
        }

        private RelayCommand<string> _observeHashtag = null;
        public RelayCommand<string> ObserveHashtag
        {
            get { return _observeHashtag ?? (_observeHashtag = new RelayCommand<string>(ExecuteObserveHashtag)); }
        }

        private async void ExecuteObserveHashtag(string hashtag)
        {
            if (string.IsNullOrEmpty(hashtag)) return;

            var success = await App.ApiService.ObserveTag(hashtag);
            if (success)
            {
                await DispatcherHelper.RunAsync(() => ObservedHashtags.Add(hashtag));
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Save ObservedHashtags"));
                UpdateHashtagsCollection();

                StatusBarManager.ShowText("Obserwujesz " + hashtag + ".");
            }
            else
            {
                StatusBarManager.ShowText("Coś poszło nie tak...");
            }
        }

        private RelayCommand<string> _unobserveHashtag = null;
        public RelayCommand<string> UnobserveHashtag
        {
            get { return _unobserveHashtag ?? (_unobserveHashtag = new RelayCommand<string>(ExecuteUnobserveHashtag)); }
        }

        private async void ExecuteUnobserveHashtag(string hashtag)
        {
            if (string.IsNullOrEmpty(hashtag)) return;

            if (HashtagsDictionary.ContainsKey(hashtag) && HashtagsDictionary[hashtag].Count > 0)
                await ExecuteDeleteHashtagNotifications(hashtag);

            var success = await App.ApiService.UnobserveTag(hashtag);
            if (success)
            {
                await DispatcherHelper.RunAsync(() => ObservedHashtags.Remove(hashtag));
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Save ObservedHashtags"));
                UpdateHashtagsCollection();

                StatusBarManager.ShowText("Przestałeś obserwować " + hashtag + ".");
            }
            else
            {
                StatusBarManager.ShowText("Coś poszło nie tak...");
            }
        }

        private RelayCommand _goToHashtagNotificationsPage = null;
        public RelayCommand GoToHashtagNotificationsPage
        {
            get { return _goToHashtagNotificationsPage ?? (_goToHashtagNotificationsPage = new RelayCommand(ExecuteGoToHashtagNotificationsPage)); }
        }

        private async void ExecuteGoToHashtagNotificationsPage()
        {
            ObservableCollectionEx<NotificationViewModel> col;
            if (!HashtagsDictionary.TryGetValue(CurrentHashtag.Name, out col))
            {
                // no notifications. go to hashtag entries page.
                SimpleIoc.Default.GetInstance<MainViewModel>().GoToHashtagPage.Execute(CurrentHashtag.Name);
                return;
            }
            else
            {
                CurrentHashtagNotifications = col;
            }

            if (CurrentHashtagNotifications.Count > 1)
            {
                NavService.NavigateTo("HashtagNotificationsPage");
            }
            else if (CurrentHashtagNotifications.Count == 1)
            {
                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                await DispatcherHelper.RunAsync(() => mainVM.SelectedEntry = null);

                NavService.NavigateTo("EntryPage");

                var notification = CurrentHashtagNotifications[0].Data;

                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpis...");
                var entry = await App.ApiService.GetEntry(notification.Entry.ID);
                if (entry != null)
                {
                    var entryVM = new EntryViewModel(entry);
                    await DispatcherHelper.RunAsync(() =>
                    {
                        mainVM.OtherEntries.Add(entryVM);
                        mainVM.SelectedEntry = entryVM;
                    });

                    await StatusBarManager.HideProgressAsync();
                    await ExecuteDeleteHashtagNotification(notification.ID);
                }
                else
                {
                    await StatusBarManager.ShowTextAsync("Nie udało się pobrać wpisu.");
                }
            }
        }

        private RelayCommand _goToFlipPage = null;
        public RelayCommand GoToFlipPage
        {
            get { return _goToFlipPage ?? (_goToFlipPage = new RelayCommand(ExecuteGoToFlipPage)); }
        }

        private int flipPageVisitCounter = 0;

        private async void ExecuteGoToFlipPage()
        {
            System.Diagnostics.Debug.WriteLine("ExecuteGoToFlipPage");
            flipPageVisitCounter++;
            if (SelectedHashtagNotification == null) return;
            var index = CurrentHashtagNotifications.GetIndex(SelectedHashtagNotification);

            await DispatcherHelper.RunAsync(() =>
            {
                HashtagFlipEntries.Clear();
                foreach(var notification in CurrentHashtagNotifications)
                {
                    var n = notification.Data;

                    HashtagFlipEntries.Add(new EntryViewModel()
                    {
                        Data = new Entry() 
                        { 
                            ID = n.Entry.ID,
                            AuthorName = n.AuthorName,
                            AuthorGroup = n.AuthorGroup,
                            AuthorSex = n.AuthorSex,
                            AuthorAvatarURL = n.AuthorAvatarURL,
                            Date = n.Date,
                        },
                    });
                }

                HashtagFlipCurrentEntry = HashtagFlipEntries[index];
            });

            NavService.NavigateTo("HashtagFlipPage");

            await ExecuteHashtagFlipSelectionChanged(index);
        }

        private RelayCommand<int> _hashtagFlipSelectionChanged = null;
        public RelayCommand<int> HashtagFlipSelectionChanged
        {
            get { return _hashtagFlipSelectionChanged ?? (_hashtagFlipSelectionChanged = new RelayCommand<int>(async (i) => await ExecuteHashtagFlipSelectionChanged(i))); }
        }

        private int flipViewIndexZeroCounter = 0;

        private async Task ExecuteHashtagFlipSelectionChanged(int currentIndex)
        {
            System.Diagnostics.Debug.WriteLine("ExecuteHashtagFlipSelectionChanged: currentIndex = " + currentIndex);
            if (currentIndex == -1 || HashtagFlipEntries[currentIndex].Data.Text != null) return;

            if (flipPageVisitCounter > 1 && currentIndex == 0)
            {
                flipViewIndexZeroCounter++;
                if (flipViewIndexZeroCounter == 1)
                    return;
                else
                    flipViewIndexZeroCounter = 0;
            }

            StatusBarManager.ShowTextAndProgress("Pobieram wpis...");

            var currentEntryID = HashtagFlipEntries[currentIndex].Data.ID;
#if WINDOWS_UWP
            /* In Windows 10, when UI thread is under heavy load while FlipView changes pages
               it often lags and what's even worse - FlipView sometimes remains kind of inbetween pages.
               Adding a bit of delay resolves this issue.
               I should report it to MSFT. */
            await Task.Delay(300);
#endif
            var entry = await App.ApiService.GetEntry(currentEntryID);
            if (entry != null)
            {
                var entryVM = new EntryViewModel(entry);
                await DispatcherHelper.RunAsync(() => HashtagFlipEntries.Replace(currentIndex, entryVM));
                await StatusBarManager.HideProgressAsync();

                var notification = CurrentHashtagNotifications.SingleOrDefault(x => x.Data.Entry.ID == currentEntryID);
                if(notification != null)
                    await ExecuteDeleteHashtagNotification(notification.Data.ID);
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się pobrać wpisu.");
            }
        }

        public async Task CheckHashtagNotifications()
        {
            uint pageIndex = 1;

            if (ObservedHashtags.Count == 0)
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Update ObservedHashtags"));

            while (true)
            {
                var notifications = await App.ApiService.GetHashtagNotifications(pageIndex++);

                if (notifications == null || notifications.Count == 0)
                    break;

                var newNotifications = notifications.Where(x => x.IsNew);

                // process each page right after downloading it.
                // makes UX so much better on slow network
                UpdateHashtagDictionary(newNotifications);

                if (!notifications.Last().IsNew || pageIndex == 9)
                    break;
            }
        }

        private void UpdateHashtagDictionary(IEnumerable<Notification> notifications)
        {
            /* 1. Create local dictionary made of notifications passed as the parameter.
             * 2. Merge local dictionary with global HashtagsDictionary.
             * 3. Update HashtagsCollection. */

            if (notifications.Count() == 0) return;

            foreach(var notification in notifications)
            {
                // extract hashtag
                var body = notification.Text;
                var index = body.IndexOf('#');
                var nextIndex = body.IndexOf(' ', index);
                var hashtag = body.Substring(index, nextIndex - index);

                if (HashtagsDictionary.ContainsKey(hashtag))
                {
                    var col = HashtagsDictionary[hashtag];
                    if (!col.Select(x => x.Data.ID).Contains(notification.ID))
                        DispatcherHelper.RunAsync(() => col.Add(new NotificationViewModel(notification))).AsTask().Wait();
                }
                else
                {
                    var col = new ObservableCollectionEx<NotificationViewModel>() { new NotificationViewModel(notification) };
                    HashtagsDictionary[hashtag] = col;
                }
            }

            DispatcherHelper.RunAsync(() =>
            {
                foreach (var col in HashtagsDictionary.Values)
                    col.Sort();
            }).AsTask().Wait();

            UpdateHashtagsCollection();
        }

        private void UpdateHashtagsCollection()
        {
            /* dictionaries are now merged. now, we have to update HashtagsCollection.
             * first, we'll create new instance, then update the old one. */
            var newHashCol = new List<HashtagInfoContainer>();

            foreach (var hashtag in HashtagsDictionary.Keys)
            {
                var count = (uint)HashtagsDictionary[hashtag].Count;
                newHashCol.Add(new HashtagInfoContainer(hashtag, count));
            }

            // now add observed hashtags (without notifications)
            var hashColNames = newHashCol.Select(x => x.Name);
            foreach (var hashtag in ObservedHashtags)
            {
                if (!hashColNames.Contains(hashtag))
                    newHashCol.Add(new HashtagInfoContainer(hashtag, 0));
            }

            /* now, we have to update HashtagsCollection.
             first, let's gather current hashtag names. */
            var newHashColNames = newHashCol.Select(x => x.Name).ToList();
            hashColNames = HashtagsCollection.Select(x => x.Name).ToList();

            var hashesToRemove = hashColNames.Where(x => !newHashColNames.Contains(x));

            foreach (var hashtag in hashesToRemove)
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    HashtagsCollection.Remove(HashtagsCollection.First(x => x.Name == hashtag)));

            // now let's update.
            hashColNames = HashtagsCollection.Select(x => x.Name).ToList();
            foreach (var hashtagInfo in newHashCol)
            {
                var hashtag = hashtagInfo.Name;
                var count = hashtagInfo.Count;

                if (hashColNames.Contains(hashtag))
                {
                    var info = HashtagsCollection.First(x => x.Name == hashtag);
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        info.Count = count);
                }
                else
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        HashtagsCollection.Add(new HashtagInfoContainer(hashtag, count)));
                }
            }

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var sum = HashtagsCollection.Sum(x => x.Count);
                HashtagNotificationsCount = (uint)sum;
                HashtagsCollection.Sort();
            });

        }
#endregion

#region At
        private uint _atNotificationsCount;
        public uint AtNotificationsCount
        {
            get { return _atNotificationsCount; }
            set { Set(() => AtNotificationsCount, ref _atNotificationsCount, value); }
        }

        private NotificationViewModel _selectedAtNotification = null;
        public NotificationViewModel SelectedAtNotification
        {
            get { return _selectedAtNotification; }
            set { Set(() => SelectedAtNotification, ref _selectedAtNotification, value); }
        }

        private IncrementalLoadingCollection<AtNotificationsSource, NotificationViewModel> _atNotifications = null;
        public IncrementalLoadingCollection<AtNotificationsSource, NotificationViewModel> AtNotifications
        {
            get { return _atNotifications ?? (_atNotifications = new IncrementalLoadingCollection<AtNotificationsSource, NotificationViewModel>()); }
        }

        private RelayCommand _goToNotification = null;
        public RelayCommand GoToNotification
        {
            get { return _goToNotification ?? (_goToNotification = new RelayCommand(ExecuteGoToNotification)); }
        }

        private async void ExecuteGoToNotification()
        {
            if (SelectedAtNotification == null || SelectedAtNotification.Data == null)
                return;

            var notification = SelectedAtNotification.Data;
            if (notification.Type == NotificationType.EntryDirected || notification.Type == NotificationType.CommentDirected)
            {
                var entryID = notification.Entry.ID;
                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                var otherEntries = mainVM.OtherEntries;
                var entryVM = otherEntries.SingleOrDefault(x => x.Data.ID == entryID);

                if (entryVM != null)
                {
                    mainVM.SelectedEntry = entryVM;
                }
                else
                {
                    await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpis...");
                    var entryData = await App.ApiService.GetEntry(entryID);
                    if (entryData == null)
                    {
                        await StatusBarManager.ShowTextAsync("Nie udało się pobrać wpisu.");
                    }
                    else
                    {
                        await StatusBarManager.HideProgressAsync();
                        entryVM = new EntryViewModel(entryData);
                        await DispatcherHelper.RunAsync(() => otherEntries.Add(entryVM));
                        mainVM.SelectedEntry = entryVM;
                    }
                }

                if (entryVM != null && notification.Type == NotificationType.CommentDirected)
                    mainVM.CommentToScrollInto = entryVM.Comments.SingleOrDefault(x => x.Data.ID == notification.Comment.CommentID);

                NavService.NavigateTo("EntryPage");
            }
            else if(notification.Type == NotificationType.Observe || notification.Type == NotificationType.Unobserve)
            {
                var profilesVM = SimpleIoc.Default.GetInstance<ProfilesViewModel>();

                profilesVM.GoToProfile.Execute(notification.AuthorName);
            }

            await DispatcherHelper.RunAsync(() =>
            {
                if (SelectedAtNotification.Data.IsNew)
                {
                    if (AtNotificationsCount >= 1)
                        AtNotificationsCount--;

                    SelectedAtNotification.Data.IsNew = false;
                }
            });
            SelectedAtNotification.MarkAsReadCommand.Execute(null);

            UpdateBadge();
        }

        private RelayCommand _deleteAllAtNotifications = null;
        public RelayCommand DeleteAllAtNotifications
        {
            get { return _deleteAllAtNotifications ?? (_deleteAllAtNotifications = new RelayCommand(ExecuteDeleteAllAtNotifications)); }
        }

        private async void ExecuteDeleteAllAtNotifications()
        {
            if(!AtNotifications.Any(x => x.Data.IsNew))
            {
                StatusBarManager.ShowText("Brak nowych powiadomień.");
                return;
            }

            var success = await App.ApiService.ReadNotifications();
            if (!success) return;

            var newNotificationsVM = AtNotifications.Where(x => x.Data.IsNew).ToList();
            foreach (var VM in newNotificationsVM)
            {
                var newVM = new NotificationViewModel(VM.Data);
                newVM.Data.IsNew = false;

                var index = AtNotifications.GetIndex(VM);
                AtNotifications.Replace(index, newVM);
            }

            AtNotificationsCount = 0;

            await StatusBarManager.ShowTextAsync("Powiadomienia zostały usunięte.");
        }
#endregion

#region PM
        private uint _pmNotificationsCount = 0;
        public uint PMNotificationsCount
        {
            get { return _pmNotificationsCount; }
            set { Set(() => PMNotificationsCount, ref _pmNotificationsCount, value); }
        }

        private ObservableCollectionEx<Notification> _pmNotifications = null;
        public ObservableCollectionEx<Notification> PMNotifications
        {
            get { return _pmNotifications ?? (_pmNotifications = new ObservableCollectionEx<Notification>()); }
        }

        private RelayCommand<uint> _deletePMNotification = null;
        public RelayCommand<uint> DeletePMNotification
        {
            get { return _deletePMNotification ?? (_deletePMNotification = new RelayCommand<uint>(async (id) => await ExecuteDeletePMNotification(id))); }
        }

        private async Task ExecuteDeletePMNotification(uint ID)
        {
            try
            {
                var notifications = PMNotifications.Where(x => x.ID == ID && x.IsNew);

                foreach (var notification in notifications)
                {
                    await App.ApiService.ReadNotification(ID);

                    var conversations = SimpleIoc.Default.GetInstance<MessagesViewModel>().ConversationsList;
                    var conversation = conversations.SingleOrDefault(x => x.Data.AuthorName == notification.AuthorName);
                    await DispatcherHelper.RunAsync(() =>
                    {
                        PMNotifications.Remove(notification);
                        if (conversation != null)
                            conversation.Data.Status = ConversationStatus.Read;
                    });                    
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error deleting PM notification ID " + ID, e);
            }
            finally
            {
                UpdateBadge();
            }
        }

        private void DeletePMNotifications(string userName)
        {
            var notifications = PMNotifications.Where(x => x.AuthorName == userName);
            if (notifications == null) return;

            foreach (var notification in notifications)
                DeletePMNotification.Execute(notification.ID);
        }
#endregion

        private async Task CheckNotifications()
        {
            uint pageIndex = 1;
            IEnumerable<Notification> notificationsTemp = null;
            var newNotifications = new List<Notification>();

            // download new notifications
            do
            {
                var notificationsDL = await App.ApiService.GetNotifications(pageIndex++);
                if (notificationsDL == null || notificationsDL.Count == 0)
                    break;

                var supportedTypes = new NotificationType[] 
                { 
                    NotificationType.Observe, NotificationType.Unobserve, 
                    NotificationType.EntryDirected, NotificationType.CommentDirected, 
                    NotificationType.System, NotificationType.Badge, NotificationType.PM 
                };
                notificationsTemp = notificationsDL.Where(x => x.IsNew).Where(x => supportedTypes.Contains(x.Type));

                newNotifications.AddRange(notificationsTemp);

                if (notificationsTemp.Count() == 0 || !notificationsDL.Last().IsNew)
                    break;

            } while (true);

            // now parse them. first PM
            var pmVM = SimpleIoc.Default.GetInstance<MessagesViewModel>();
            var pmNotifications = newNotifications.Where(x => x.Type == NotificationType.PM);

            if (pmVM.ConversationsList.Count == 0)
            {
                var conversations = await App.ApiService.GetConversations();
                if (conversations != null)
                {
                    var tmp = new List<ConversationViewModel>(conversations.Count);
                    foreach(var item in conversations)
                        tmp.Add(new ConversationViewModel(item));

                    await DispatcherHelper.RunAsync(() => pmVM.ConversationsList.AddRange(tmp));

                    tmp = null;
                    conversations = null;
                }
            }

            var tempPMnotifications = new List<Notification>();
            foreach (var item in pmNotifications)
            {
                var userName = item.AuthorName;
                var conversation = pmVM.ConversationsList.FirstOrDefault(x => x.Data.AuthorName == userName);

                if (conversation != null)
                {
                    if (conversation.Data.Status != ConversationStatus.New)
                        await DispatcherHelper.RunAsync(() => conversation.Data.Status = ConversationStatus.New);
                }
                else
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        var conv = new Conversation()
                        {
                            AuthorName = userName,
                            AuthorAvatarURL = item.AuthorAvatarURL,
                            AuthorGroup = item.AuthorGroup,
                            AuthorSex = item.AuthorSex,
                            LastUpdate = item.Date,
                            Status = ConversationStatus.New,
                        };
                        pmVM.ConversationsList.Insert(0, new ConversationViewModel(conv));
                    });
                }

                tempPMnotifications.Add(item);
            }

            await DispatcherHelper.RunAsync(() =>
            {
                this.PMNotifications.Clear();
                this.PMNotifications.AddRange(tempPMnotifications);
                tempPMnotifications = null;
            });

            // now the rest
            var atNotifications = newNotifications.Where(x => x.Type != NotificationType.PM);
            if (atNotifications.Count() > 0)
            {
                var VMs = new List<NotificationViewModel>();
                foreach (var n in atNotifications)
                    VMs.Add(new NotificationViewModel(n));

                await DispatcherHelper.RunAsync(() => 
                {
                    this.AtNotificationsCount = (uint)atNotifications.Count();
                    this.AtNotifications.PrependRange(VMs);
                });
                VMs = null;
            }
        }
    }
}
