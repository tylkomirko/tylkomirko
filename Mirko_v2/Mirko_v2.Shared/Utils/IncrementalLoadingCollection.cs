using GalaSoft.MvvmLight.Threading;
using Mirko_v2.ViewModel;
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

namespace Mirko_v2.Utils
{
    public interface IIncrementalSource<T>
    {
        Task<IEnumerable<T>> GetPagedItems(int pageSize, CancellationToken ct);
        //uint LastID();
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
        private uint lastID = uint.MaxValue;
        private CancellationTokenSource cancelToken;
        //private int currentPage;

        public IncrementalLoadingCollection(int itemsPerPage = 10)
        {
            this.source = new T();
            this.itemsPerPage = itemsPerPage;
            this.hasMoreItems = true;
        }

        public bool HasMoreItems
        {
            get { return hasMoreItems; }
            set { hasMoreItems = value; }
        }

        public bool HasNoItems
        {
            get { return hasNoItems; }
            set { hasNoItems = value; base.OnPropertyChanged(new PropertyChangedEventArgs("HasNoItems")); }
        }

        public uint LastID
        {
            get { return lastID; }
            set { lastID = value; }
        }

        public void ClearAll()
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
            cancelToken = null;

            Clear();
            source.ClearCache();

            HasMoreItems = true;
            DispatcherHelper.CheckBeginInvokeOnUI(() => HasNoItems = false);

            lastID = uint.MaxValue;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var dispatcher = Window.Current.Dispatcher;
            cancelToken = new CancellationTokenSource();

            return Task.Run<LoadMoreItemsResult>(
                async () =>
                {
                    try
                    {
                        uint resultCount = 0;
                        var result = await source.GetPagedItems(itemsPerPage, cancelToken.Token);

                        if (result == null || result.Count() == 0)
                        {
                            hasMoreItems = false;
                        }
                        else
                        {
                            resultCount = (uint)result.Count();

                            await dispatcher.RunAsync(
                                CoreDispatcherPriority.Normal,
                                () =>
                                {
                                    this.AddRange(result);

                                    /*foreach (I item in result)
                                        this.Add(item);
                                     */
                                });
                        }

                        return new LoadMoreItemsResult() { Count = resultCount };

                    }
                    catch(Exception)
                    {
                        return new LoadMoreItemsResult() { Count = 0 };
                    }

                }, cancelToken.Token).AsAsyncOperation<LoadMoreItemsResult>();
        }
    }

}
