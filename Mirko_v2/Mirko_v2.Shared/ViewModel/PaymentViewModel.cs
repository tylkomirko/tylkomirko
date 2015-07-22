using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MetroLog;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.UI.Popups;

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

        private bool convertionOK = true;

        public PaymentViewModel()
        {
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<PaymentViewModel>();
            
#if DEBUG
            LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            LicenseInformation = CurrentApp.LicenseInformation; 
#endif
            
            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);             
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Init")
            {
#if DEBUG
                StorageFolder installFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
                StorageFile appSimulatorStorageFile = await installFolder.GetFileAsync("WindowsStoreProxy.xml");

                await CurrentAppSimulator.ReloadSimulatorAsync(appSimulatorStorageFile);
                ListingInformation = await CurrentAppSimulator.LoadListingInformationAsync();
#else
                ListingInformation = await CurrentApp.LoadListingInformationAsync();
#endif

                foreach (var product in ListingInformation.ProductListings)
                {
                    var id = product.Value.ProductId;

                    var price = product.Value.FormattedPrice;
                    price = price.Replace("zł", "");
                    try
                    {
                        var numeric = Convert.ToSingle(price);
                        price = numeric.ToString("N2").Replace('.', ',');
                    }
                    catch (Exception)
                    {
                        convertionOK = false;
                        price = product.Value.FormattedPrice;
                    }

                    Products.Add(new Tuple<string, string>(id, price));
                }

                if (convertionOK)
                    Products.Sort((i1, i2) => Convert.ToSingle(i1.Item2).CompareTo(Convert.ToSingle(i2.Item2)));

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

        private RelayCommand<string> _buyProduct = null;
        public RelayCommand<string> BuyProduct
        {
            get { return _buyProduct ?? (_buyProduct = new RelayCommand<string>(ExecuteBuyProduct)); }
        }

        private async void ExecuteBuyProduct(string id)
        {
            try
            {
#if DEBUG
                var purchaseResult = await CurrentAppSimulator.RequestProductPurchaseAsync(id);
                var fulfillmentResult = await CurrentAppSimulator.ReportConsumableFulfillmentAsync(id, purchaseResult.TransactionId);
#else
                var purchaseResult = await CurrentApp.RequestProductPurchaseAsync(id);
                var fulfillmentResult = await CurrentApp.ReportConsumableFulfillmentAsync(id, purchaseResult.TransactionId);
#endif
                var msg = new MessageDialog("");

                if(purchaseResult.Status == ProductPurchaseStatus.Succeeded)
                {
                    msg.Title = "Serdecznie dziękuję!";
                    msg.Content = "Będę miał na czipsy i batony ( ͡° ͜ʖ ͡°).";
                }
                else if (purchaseResult.Status == ProductPurchaseStatus.NotFulfilled)
                {
                    return;
                }
                else
                {
                    msg.Title = "Coś poszło nie tak...";
                    msg.Content = "Wyślij logi do autora aplikacji.";

                    Logger.Warn("ProductPurchaseStatus: " + purchaseResult.Status);
                }

                await msg.ShowAsync();
            } 
            catch(Exception e)
            {
                Logger.Error("Something went wrong while buying.", e);
            }
        }
    }
}
