using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Store;

namespace Mirko_v2.ViewModel
{
    public class PaymentViewModel : ViewModelBase
    {
        private readonly ILogger Logger = null;
        private LicenseInformation LicenseInformation;
        private ListingInformation ListingInformation;

        public PaymentViewModel()
        {
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<PaymentViewModel>();
            LicenseInformation = CurrentApp.LicenseInformation;

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Init")
            {
                //ListingInformation = await CurrentApp.LoadListingInformationAsync();

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
