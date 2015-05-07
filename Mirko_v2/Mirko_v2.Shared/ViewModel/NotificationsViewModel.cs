using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WykopAPI.Models;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Mirko_v2.Utils;

namespace Mirko_v2.ViewModel
{
    public class HashtagInfoContainer
    {
        public string Name { get; set; }
        public uint Count { get; set; }
    }

    public class NotificationsViewModel : ViewModelBase
    {
        private Timer Timer = null;

        public NotificationsViewModel()
        {
            Timer = new Timer(TimerCallback, null, 100, 60*1000);
        }

        private async void TimerCallback(object state)
        {
            await CheckHashtagNotifications();
            await CheckNotifications();
        }

        #region AppHeader commands
        private RelayCommand _logoTappedCommand = null;
        public RelayCommand LogoTappedCommand
        {
            get { return _logoTappedCommand ?? (_logoTappedCommand = new RelayCommand(ExecuteCommandLogoTapped)); }
        }

        private void ExecuteCommandLogoTapped()
        {
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();
            navService.NavigateTo("MainPage");
        }

        private RelayCommand _hashtagTappedCommand = null;
        public RelayCommand HashtagTappedCommand
        {
            get { return _hashtagTappedCommand ?? (_hashtagTappedCommand = new RelayCommand(ExecuteHashtagTappedCommand)); }
        }

        private async void ExecuteHashtagTappedCommand()
        {
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();

            if (HashtagsCollection.Count == 0)
                await UpdateHashtagDictionary();

            if (HashtagNotificationsCount == 0)
            {
                navService.NavigateTo("HashtagNotificationsPage");
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

                // FIXME
                //var json = EntryNavigationParameterExtensions.fromNotification(n, hashtag);
                //NavigateTo(this, new PageNavigationEventArgs(typeof(FullscreenEntry), json));
            }
            else if (HashtagNotificationsCount > 1)
            {
                // check if all notifications relate to the same hashtag

                int nonZeroesFound = 0;
                string hashtag = null;
                foreach (var item in HashtagsCollection)
                {
                    if (item.Count != 0)
                    {
                        nonZeroesFound++;
                        hashtag = item.Name;

                        if (nonZeroesFound >= 2)
                            break;
                    }
                }

                if (nonZeroesFound <= 1) // all notifications belong to one tag
                {
                    //NavigateTo(this, new PageNavigationEventArgs(typeof(HashtagNotificationsPage), hashtag));
                    // FIXME
                }
                else
                {
                    navService.NavigateTo("HashtagNotificationsPage");
                }

            }
        }

        private RelayCommand _pmTappedCommand = null;
        public RelayCommand PMTappedCommand
        {
            get { return _pmTappedCommand ?? (_pmTappedCommand = new RelayCommand(ExecutePMTappedCommand)); }
        }

        private void ExecutePMTappedCommand()
        {
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();

            if (PMNotifications.Count == 1)
            {
                var conversation = PMNotifications.First();
                /*
                var param = new PMNavigationParameter()
                {
                    UserName = conversation.author,
                    Sex = conversation.author_sex,
                    Group = conversation.author_group,
                };

                NavigateTo(this, new PageNavigationEventArgs(typeof(PMPage), param.toString()));*/
            }
            else
            {
                navService.NavigateTo("ConversationsPage");
            }
        }
        #endregion

        #region Hashtag
        private uint NewestHashtagNotificationID;

        private uint _hashtagNotificationsCount;
        public uint HashtagNotificationsCount
        {
            get { return _hashtagNotificationsCount; }
            set { Set(() => HashtagNotificationsCount, ref _hashtagNotificationsCount, value); }
        }

        private ObservableDictionary<string, ObservableCollectionEx<NotificationViewModel>> _hashtagsDictionary;
        public ObservableDictionary<string, ObservableCollectionEx<NotificationViewModel>> HashtagsDictionary
        {
            get
            {
                if (_hashtagsDictionary == null)
                    _hashtagsDictionary = new ObservableDictionary<string, ObservableCollectionEx<NotificationViewModel>>();
                return _hashtagsDictionary;
            }
        }

        private ObservableCollectionEx<HashtagInfoContainer> _hashtagsCollection;
        public ObservableCollectionEx<HashtagInfoContainer> HashtagsCollection
        {
            get
            {
                if (_hashtagsCollection == null)
                    _hashtagsCollection = new ObservableCollectionEx<HashtagInfoContainer>();
                return _hashtagsCollection;
            }
        }

        private RelayCommand<string> _deleteHashtagNotifications = null;
        public RelayCommand<string> DeleteHashtagNotifications
        {
            get { return _deleteHashtagNotifications ?? (_deleteHashtagNotifications = new RelayCommand<string>(ExecuteDeleteHashtagNotifications)); }
        }

        private async void ExecuteDeleteHashtagNotifications(string hashtag)
        {
            if (!HashtagsDictionary.ContainsKey(hashtag)) return;

            var notifications = HashtagsDictionary[hashtag];
            var IDs = notifications.Select(x => x.Data.ID);

            foreach (var id in IDs)
                await App.ApiService.markAsReadNotification(id);

            HashtagsDictionary.Remove(hashtag);
            await UpdateHashtagDictionary();

            await StatusBarManager.ShowText("Powiadomienia zostały usunięte.");
        }

        private RelayCommand _deleteAllHashtagNotifications = null;
        public RelayCommand DeleteAllHashtagNotifications
        {
            get { return _deleteAllHashtagNotifications ?? (_deleteAllHashtagNotifications = new RelayCommand(ExecuteDeleteAllHashtagNotifications)); }
        }

        private async void ExecuteDeleteAllHashtagNotifications()
        {
            await App.ApiService.readHashtagNotifications();
        }

        public async Task CheckHashtagNotifications()
        {
            uint pageIndex = 1;
            var notificationsList = new List<Notification>(50);

            while (true)
            {
                var notifications = await App.ApiService.getHashtagNotifications(pageIndex++);

                if (notifications == null || notifications.Count == 0)
                    break;

                var newNotifications = notifications.Where(x => x.IsNew && x.ID > this.NewestHashtagNotificationID);
                notificationsList.AddRange(newNotifications);

                if (!notifications.Last().IsNew)
                    break;
            }

            if (notificationsList.Count > 0)
                this.NewestHashtagNotificationID = notificationsList.First().ID;

            var dic = this.HashtagsDictionary;
            foreach (var item in notificationsList)
            {
                var body = item.Text;
                var index = body.IndexOf('#');
                var nextIndex = body.IndexOf(' ', index);

                var tagName = body.Substring(index, nextIndex - index);

                if (dic.ContainsKey(tagName))
                {
                    var list = dic[tagName];
                    list.Add(new NotificationViewModel(item));
                }
                else
                {
                    dic[tagName] = new ObservableCollectionEx<NotificationViewModel>() { new NotificationViewModel(item) };
                }
            }

            await UpdateHashtagDictionary();
        }

        private async Task UpdateHashtagDictionary(bool forcedSorting = false)
        {
            var dic = this.HashtagsDictionary;
            var orderedNames = dic.Keys.OrderByDescending(x => dic[x].Count).ToList();

            //if (this.HashtagsCollection.Count > 0)
            //    await DispatcherHelper.RunAsync(() => this.HashtagsCollection.Clear());

            uint count = 0;

            var tmp = new List<HashtagInfoContainer>();
            foreach (var name in orderedNames)
            {
                var c = (uint)dic[name].Count;
                if (c > 0)
                {
                    count += c;
                    tmp.Add(new HashtagInfoContainer
                    {
                        Name = name,
                        Count = c,
                    });
                }
            }

            await DispatcherHelper.RunAsync(() =>
            {
                this.HashtagsCollection.Clear();
                this.HashtagsCollection.AddRange(tmp);
                this.HashtagNotificationsCount = count;
            });

            tmp.Clear();

            // now add observed hashtags without notifications
            var observedTags = SimpleIoc.Default.GetInstance<CacheViewModel>().ObservedHashtags;
            foreach (var tag in observedTags)
            {
                bool itemFound = false;

                foreach (var addedTag in this.HashtagsCollection)
                {
                    if (addedTag.Name == tag)
                    {
                        itemFound = true;
                        break;
                    }
                }

                if (!itemFound)
                    tmp.Add(new HashtagInfoContainer() { Name = tag, Count = 0 });
            }

            await DispatcherHelper.RunAsync(() => this.HashtagsCollection.AddRange(tmp));
            tmp = null;

            // now sort
            if (count > 0 || forcedSorting)
            {
                var groups = this.HashtagsCollection.GroupBy(x => x.Count);
                int itemsToRemove = this.HashtagsCollection.Count();

                var sortedGroups = new List<HashtagInfoContainer>();

                foreach (var group in groups)
                {
                    var sortedGroup = group.OrderBy(x => x.Name);
                    sortedGroups.AddRange(sortedGroup);
                }

                await DispatcherHelper.RunAsync(() =>
                {
                    this.HashtagsCollection.AddRange(sortedGroups);

                    for (int i = 0; i < itemsToRemove; i++)
                        this.HashtagsCollection.RemoveAt(0);
                });

                sortedGroups = null;
            }
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
        #endregion

        private async Task CheckNotifications()
        {
            uint pageIndex = 1;
            IEnumerable<Notification> notificationsTemp = null;
            var newNotifications = new List<Notification>();

            // download new notifications
            do
            {
                var notificationsDL = await App.ApiService.getNotifications(pageIndex++);
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
                var conversations = await App.ApiService.getConversations();
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
                //if (userName == App.NotificationsViewModel.CurrentUserName)
                //    continue;
                // FIXME?

                var conversation = pmVM.ConversationsList.First(x => x.Data.AuthorName == userName);

                if (conversation != null)
                {
                    if (conversation.Data.Status != ConversationStatus.New)
                        conversation.Data.Status = ConversationStatus.New;
                }
                else
                {
                    var conv = new Conversation();
                    conv.AuthorName = userName;
                    conv.Status = ConversationStatus.New;
                    pmVM.ConversationsList.Insert(0, new ConversationViewModel(conv));
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
            /*
            var atNotifications = newNotifications.Where(x => x.type != "pm");
            this.AtNotificationsCount = atNotifications.Count();

            if (atNotifications.Count() > 0)
            {
                this.AtNotificationsStream.PrependRange(atNotifications);
            }
             * */
        }
    }
}
