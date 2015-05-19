using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Utils
{
    public interface IIncrementalSource<T>
    {
        Task<IEnumerable<T>> GetPagedItems(int pageSize);
        void ClearCache();
    }

    public class IncrementalLoadingCollection<T, I> : ObservableCollectionEx<I>,
         ISupportIncrementalLoading
         where T : IIncrementalSource<I>, new()
    {
        private T source;
        private int itemsPerPage;
        private bool hasMoreItems;
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

        public void ClearAll()
        {
            this.Clear();
            source.ClearCache();
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var dispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(
                async () =>
                {
                    uint resultCount = 0;
                    var result = await source.GetPagedItems(itemsPerPage);

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

                }).AsAsyncOperation<LoadMoreItemsResult>();
        }
    }

}
