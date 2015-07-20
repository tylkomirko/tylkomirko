using GalaSoft.MvvmLight;
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
using Windows.Storage.Pickers;
using WykopAPI.Models;

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

    public class NewEntryViewModel : ViewModelBase, IFileOpenPickerContinuable, IResumable
    {
        private NavigationService NavService = null;

        private ObservableCollectionEx<NewEntryContainer> _responses = null;
        public ObservableCollectionEx<NewEntryContainer> Responses
        {
            get { return _responses ?? (_responses = new ObservableCollectionEx<NewEntryContainer>()); }
        }

        private NewEntry _data = null;
        public NewEntry Data
        {
            get { return _data ?? (_data = new NewEntry()); }
            set { Set(() => Data, ref _data, value); }
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
                if (Data.IsEditing)
                {
                    Responses.Add(new NewEntryContainer()
                    {
                        Text = HTMLUtils.HTMLtoWYPOK(entries.First().DataBase.Text),
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

        private RelayCommand _addAttachment = null;
        [JsonIgnore]
        public RelayCommand AddAttachment
        {
            get { return _addAttachment ?? (_addAttachment = new RelayCommand(() => NavService.NavigateTo("AttachmentPage"))); }
        }

        private RelayCommand _openPicker = null;
        [JsonIgnore]
        public RelayCommand OpenPicker
        {
            get { return _openPicker ?? (_openPicker = new RelayCommand(ExecuteOpenPicker)); }
        }

        private void ExecuteOpenPicker()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.PickSingleFileAndContinue();
        }

        public async void ContinueFileOpenPicker(Windows.ApplicationModel.Activation.FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count() > 0)
            {
                StorageFile file = args.Files[0];
                var s = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                Data.FileStream = s.AsStreamForRead();
                Data.FileName = file.Name;
                Data.AttachmentName = file.DisplayName;

                NavService.GoBack();
            }
        }

        public async Task AddFile(IStorageItem item) // used in share target activation
        {
            var file = item as StorageFile;
            var s = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            Data.FileStream = s.AsStreamForRead();
            Data.FileName = file.Name;
            Data.AttachmentName = file.DisplayName;

            Responses.Clear();
            Responses.Add(new NewEntryContainer());
        }

        private RelayCommand _acceptAttachments = null;
        [JsonIgnore]
        public RelayCommand AcceptAttachments
        {
            get { return _acceptAttachments ?? (_acceptAttachments = new RelayCommand(() => NavService.GoBack())); }
        }

        public void RemoveAttachment()
        {
            Data.RemoveAttachment();
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
            Data.Text = txt;

            if (Data.IsEditing)
                await SendEdited();
            else
                await SendNew();
        }

        private async Task SendNew()
        {
            string suffix = Data.EntryID == 0 ? " wpis" : " komentarz";
            await StatusBarManager.ShowTextAndProgressAsync("Wysyłam" + suffix + "...");

            uint entryID = await App.ApiService.addEntry(Data);

            if (entryID != 0)
            {
                await StatusBarManager.ShowTextAsync("Dodano" + suffix + ".");
                Data.RemoveAttachment();
                Data.Text = null;

                NavService.GoBack();

                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                if (Data.EntryID == 0)
                {
                    var entry = await App.ApiService.getEntry(entryID);
                    if (entry != null)
                        mainVM.MirkoEntries.Insert(0, new EntryViewModel(entry));
                }
                else
                {
                    var entry = await App.ApiService.getEntry(Data.EntryID);
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
            string suffix = Data.CommentID == 0 ? " wpis" : " komentarz";
            await StatusBarManager.ShowTextAndProgressAsync("Edytuje" + suffix + "...");

            uint entryID = await App.ApiService.editEntry(Data);

            if(entryID != 0)
            {
                await StatusBarManager.HideProgressAsync();
                Data.RemoveAttachment();
                Data.Text = null;

                NavService.GoBack();

                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                Entry entry = null;

                if (Data.EntryID == 0)
                    entry = await App.ApiService.getEntry(entryID);
                else
                    entry = await App.ApiService.getEntry(Data.EntryID);

                if (entry != null)
                    Messenger.Default.Send<EntryViewModel>(new EntryViewModel(entry), "Update");
            }
            else
            {
                suffix = Data.CommentID == 0 ? " wpisu" : " komentarza";
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

                Data = vm.Data;
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
