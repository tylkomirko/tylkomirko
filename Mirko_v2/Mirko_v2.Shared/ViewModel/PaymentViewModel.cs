using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using MetroLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;

namespace Mirko_v2.ViewModel
{
    public class PaymentViewModel : ViewModelBase
    {
        private readonly ILogger Logger = null;
        private LicenseInformation LicenseInformation;
        private ListingInformation ListingInformation;

        private List<Tuple<string, string>> _products = null;
        public List<Tuple<string, string>> Products
        {
            get { return _products ?? (_products = new List<Tuple<string, string>>()); }
        }

        public PaymentViewModel()
        {
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<PaymentViewModel>();
            /*
#if DEBUG
            LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            LicenseInformation = CurrentApp.LicenseInformation; 
#endif
            
            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
             * */
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Init")
            {
                ListingInformation = await CurrentAppSimulator.LoadListingInformationAsync();
                foreach(var product in ListingInformation.ProductListings)
                {
                    var id = product.Value.ProductId;
                    var price = product.Value.FormattedPrice;

                    Products.Add(new Tuple<string, string>(id, price));
                }

                var products = await CurrentApp.GetUnfulfilledConsumablesAsync();
                foreach (UnfulfilledConsumable product in products)
                {
                    Logger.Info("UnfulfilledConsumable: Product Id: " + product.ProductId + " Transaction Id: " + product.TransactionId);
                    var status = await CurrentApp.ReportConsumableFulfillmentAsync(product.ProductId, product.TransactionId);
                    if (status != FulfillmentResult.Succeeded)
                        Logger.Warn("Transaction " + product.TransactionId + " couldn't be fullfilled. Status: " + status);
                    else
                        Logger.Info("Transaction " + product.TransactionId + " fullfilled.");
                }
            }
        }
    }
}
