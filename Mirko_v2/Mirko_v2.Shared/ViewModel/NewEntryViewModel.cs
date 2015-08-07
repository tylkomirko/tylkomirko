using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using WykopSDK.API.Models;
using WykopSDK.Parsers;

namespace Mirko_v2.ViewModel
{
    public class NewEntryContainer : INotifyPropertyChanged
    {
        internal class EntryPreviewConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                EntryViewModel entryVM = null;
                try
                {
                    entryVM = serializer.Deserialize<EntryViewModel>(reader);
                }
                catch (Exception) { }

                if (entryVM != null)
                {
                    return entryVM;
                }
                else
                {
                    var commentVM = serializer.Deserialize<CommentViewModel>(reader);
                    return commentVM;
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var basicVM = value as EntryBaseViewModel;
                var entryVM = basicVM as EntryViewModel;
                if (entryVM != null)
                {
                    serializer.Serialize(writer, entryVM);
                }
                else
                {
                    var commentVM = basicVM as CommentViewModel;
                    serializer.Serialize(writer, commentVM);
                }
            }
        }

        private EntryBaseViewModel _preview = null;
        [JsonConverter(typeof(EntryPreviewConverter))]
        public EntryBaseViewModel Preview
        {
            get { return _preview; }
            set { _preview = value; OnPropertyChanged(); }
        }

        private string _text = null;
        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged(); }
        }

        private string _selectedText = null;
        public string SelectedText
        {
            get { return _selectedText; }
            set { _selectedText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NewEntryViewModel : NewEntryBaseViewModel, IResumable
    {
        private NavigationService NavService = null;

        private ObservableCollectionEx<NewEntryContainer> _responses = null;
        public ObservableCollectionEx<NewEntryContainer> Responses
        {
            get { return _responses ?? (_responses = new ObservableCollectionEx<NewEntryContainer>()); }
        }

        private NewEntryBaseViewModel _attachmentTarget = null;
        [JsonIgnore]
        public NewEntryBaseViewModel AttachmentTarget
        {
            get { return _attachmentTarget; }
            set { Set(() => AttachmentTarget, ref _attachmentTarget, value); }
        }

        public NewEntryViewModel()
        {
        }

        public NewEntryViewModel(NavigationService nav)
        {
            NavService = nav;
        }

        public void GoToNewEntryPage(List<EntryBaseViewModel> entries = null)
        {
            Responses.Clear();

            if(entries != null)
            {
                if (NewEntry.IsEditing)
                {
                    Responses.Add(new NewEntryContainer()
                    {
                        Text = HtmlToWykop.Convert(entries.First().DataBase.Text),
                    });
                }
                else
                {
                    foreach (var entry in entries)
                        Responses.Add(new NewEntryContainer()
                        {
                            Preview = entry,
                            Text = "@" + entry.DataBase.AuthorName + ": ",
                        });
                }
            }
            else
            {
                Responses.Add(new NewEntryContainer()
                {
                    Text = "",
                });
            }

            NavService.NavigateTo("NewEntryPage");
        }

        public void AddFiles(IReadOnlyList<IStorageItem> items) // used in share target activation
        {
            var files = items.Cast<StorageFile>();
            NewEntry.Files = files.ToArray();

            if (files.Count() == 1)
                NewEntry.AttachmentName = files.First().Name;
            else
                NewEntry.SetAttachmentName(items.Count);

            Responses.Clear();
            Responses.Add(new NewEntryContainer());
        }

        private RelayCommand _sendMessageCommand = null;
        [JsonIgnore]
        public RelayCommand SendMessageCommand
        {
            get { return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(ExecuteSendMessageCommand)); }
        }

        private async void ExecuteSendMessageCommand()
        {
            var txt = string.Join("\n", Responses.Select(x => x.Text));
            if (string.IsNullOrEmpty(txt))
                txt = " \n ";
            NewEntry.Text = txt;

            if (NewEntry.IsEditing)
                await SendEdited();
            else
                await SendNew();
        }

        private async Task SendNew()
        {
            string suffix = NewEntry.EntryID == 0 ? " wpis" : " komentarz";
            await StatusBarManager.ShowTextAndProgressAsync("Wysyłam" + suffix + "...");

            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
            var files = NewEntry.Files;
            if(NewEntry.EntryID == 0 && files != null && files.Length > 1)
            {
                // add multiple attachments
                var firstFile = files[0];
                uint mainEntryID = 0;

                using (var fileStream = await firstFile.OpenStreamForReadAsync())
                    mainEntryID = await App.ApiService.addEntry(NewEntry, fileStream, firstFile.Name);

                if(mainEntryID == 0)
                {
                    await StatusBarManager.ShowTextAsync("Nie udało się dodać wpisu.");
                    return;
                }

                App.TelemetryClient.TrackEvent("New entry");

                EntryViewModel entryVM = null;

                if (!App.ShareTargetActivated)
                {
                    NavService.GoBack();

                    var entry = await App.ApiService.getEntry(mainEntryID);
                    entryVM = new EntryViewModel(entry);
                    if (entry != null)
                        mainVM.MirkoEntries.Insert(0, entryVM);
                }

                NewEntry.EntryID = mainEntryID;
                NewEntry.Text = " \n ";

                await StatusBarManager.ShowTextAndProgressAsync("Wysyłam komentarze...");
                for (int i = 1; i < files.Length; i++)
                {
                    using (var fileStream = await files[i].OpenStreamForReadAsync())
                    {
                        var id = await App.ApiService.addEntry(NewEntry, fileStream, files[i].Name);
                        if(id != 0)
                            App.TelemetryClient.TrackEvent("New comment");
                    }
                }

                await StatusBarManager.ShowTextAsync("Wpis został dodany.");

                NewEntry.RemoveAttachment();
                NewEntry.Text = null;

                if (!App.ShareTargetActivated)
                {
                    var entry = await App.ApiService.getEntry(mainEntryID);
                    var idx = mainVM.MirkoEntries.GetIndex(entryVM);
                    mainVM.MirkoEntries.Replace(idx, new EntryViewModel(entry));
                }
                else
                {
                    await Task.Delay(600);
                    NavService.GoBack();
                }

                return;
            }

            // add single attachment
            uint entryID = 0;
            if(NewEntry.Files != null)
            {
                using (var fileStream = await NewEntry.Files[0].OpenStreamForReadAsync())
                    entryID = await App.ApiService.addEntry(NewEntry, fileStream, NewEntry.Files[0].Name);
            }
            else
            {
                entryID = await App.ApiService.addEntry(NewEntry);
            }

            if (entryID != 0)
            {
                await StatusBarManager.ShowTextAsync("Dodano" + suffix + ".");
                NewEntry.RemoveAttachment();
                NewEntry.Text = null;

                if(App.ShareTargetActivated)
                {
                    await Task.Delay(600);
                    NavService.GoBack();

                    return;
                }

                NavService.GoBack();

                if (NewEntry.EntryID == 0)
                {
                    App.TelemetryClient.TrackEvent("New entry");

                    var entry = await App.ApiService.getEntry(entryID);
                    if (entry != null)
                        mainVM.MirkoEntries.Insert(0, new EntryViewModel(entry));
                }
                else
                {
                    App.TelemetryClient.TrackEvent("New comment");

                    var entry = await App.ApiService.getEntry(NewEntry.EntryID);
                    if (entry != null)
                        Messenger.Default.Send<EntryViewModel>(new EntryViewModel(entry), "Update");
                }

            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się dodać wpisu.");
            }
        }

        private async Task SendEdited()
        {
            string suffix = NewEntry.CommentID == 0 ? " wpis" : " komentarz";
            await StatusBarManager.ShowTextAndProgressAsync("Edytuje" + suffix + "...");

            uint entryID = await App.ApiService.editEntry(NewEntry);

            if(entryID != 0)
            {
                await StatusBarManager.HideProgressAsync();
                NewEntry.RemoveAttachment();
                NewEntry.Text = null;

                NavService.GoBack();

                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                Entry entry = null;

                if (NewEntry.EntryID == 0)
                {
                    entry = await App.ApiService.getEntry(entryID);
                    App.TelemetryClient.TrackEvent("Edited entry");
                }
                else
                {
                    entry = await App.ApiService.getEntry(NewEntry.EntryID);
                    App.TelemetryClient.TrackEvent("Edited comment");
                }

                if (entry != null)
                    Messenger.Default.Send<EntryViewModel>(new EntryViewModel(entry), "Update");
            }
            else
            {
                suffix = NewEntry.CommentID == 0 ? " wpisu" : " komentarza";
                await StatusBarManager.ShowTextAsync("Nie udało się edytować" + suffix + ".");
            }
        }

        #region IResumable
        public async Task SaveState(string pageName)
        {
            var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("VMs", Windows.Storage.CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync("NewEntryViewModel", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            using (var sw = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                JsonSerializer serializer = new JsonSerializer();

                serializer.Serialize(writer, this);
            }
        }

        public async Task<bool> LoadState(string pageName)
        {
            var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("VMs");
            var file = await folder.GetFileAsync("NewEntryViewModel");

            using (var stream = await file.OpenStreamForReadAsync())
            using (var sr = new StreamReader(stream))
            using (var reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                var vm = serializer.Deserialize<NewEntryViewModel>(reader);

                NewEntry = vm.NewEntry;
                Responses.Clear();
                Responses.AddRange(vm.Responses);

                vm = null;
            }

            return true; // success!
        }

        public string GetName()
        {
            return "NewEntryViewModel";
        }
        #endregion
    }
}
