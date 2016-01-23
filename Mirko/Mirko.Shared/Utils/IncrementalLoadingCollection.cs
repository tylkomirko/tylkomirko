using GalaSoft.MvvmLight.Threading;
using Mirko.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mirko.Utils
{
    public interface IIncrementalSource<T>
    {
        Task<IEnumerable<T>> GetPagedItems(int pageSize, CancellationToken ct);
        void ClearCache();
    }

    public class IncrementalLoadingCollection<T, I> : ObservableCollectionEx<I>,
         ISupportIncrementalLoading
         where T : IIncrementalSource<I>, new()
    {
        private T source;
        private int itemsPerPage;
        private bool hasMoreItems;
        private bool hasNoItems;
        private CancellationTokenSource cts;
        private bool isStopped = false;
        private bool busy = false;

        public IncrementalLoadingCollection(int itemsPerPage = 10)
        {
            this.source = new T();
            this.itemsPerPage = itemsPerPage;
            this.hasMoreItems = true;
        }

        public bool HasMoreItems
        {
            get { return isStopped ? false : hasMoreItems; }
            set { hasMoreItems = value; base.OnPropertyChanged(new PropertyChangedEventArgs("HasMoreItems")); }
        }

        public bool HasNoItems
        {
            get { return hasNoItems; }
            set { hasNoItems = value; base.OnPropertyChanged(new PropertyChangedEventArgs("HasNoItems")); }
        }

        public void ClearAll()
        {
            Cancel();
            Clear();
            source.ClearCache();

            HasMoreItems = true;
            DispatcherHelper.CheckBeginInvokeOnUI(() => HasNoItems = false);
        }

        public void Cancel()
        {
            if (cts != null && busy)
                cts.Cancel();
        }

        public void ForceStop()
        {
            isStopped = true;
            Cancel();
        }

        public void Start()
        {
            isStopped = false;
            hasMoreItems = true;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var dispatcher = Window.Current.Dispatcher;
            cts = new CancellationTokenSource();

            var task = Task.Run<LoadMoreItemsResult>(async () =>
            {
                try
                {
                    busy = true;

                    uint resultCount = 0;
                    var result = await source.GetPagedItems(itemsPerPage, cts.Token);

                    if (result == null || result.Count() == 0)
                    {
                        if (!cts.Token.IsCancellationRequested)
                            hasMoreItems = false;
                    }
                    else
                    {
                        resultCount = (uint)result.Count();

                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () => this.AddRange(result));
                    }

                    return new LoadMoreItemsResult() { Count = resultCount };
                }
                catch (Exception)
                {
                    StatusBarManager.HideProgress();
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                finally
                {
                    cts.Dispose();
                    busy = false;
                }

            }, cts.Token);

            return task.AsAsyncOperation<LoadMoreItemsResult>();
        }
    }

}
