using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Xamarin.Forms.Internals.Profile;
using ShoppingList.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace ShoppingList
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            LoadDataFromRestApi();
        }

        private HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }

        async void LoadDataFromRestApi()
        {
            try
            {
                
#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
#else
                        HttpClient client = new HttpClient();
#endif

                client.BaseAddress = new Uri("https://10.0.2.2:7103/");
                string json = await client.GetStringAsync("api/shoppingList");

                IEnumerable<ListedItem> item = JsonConvert.DeserializeObject<ListedItem[]>(json);
                ObservableCollection<ListedItem> dataa = new ObservableCollection<ListedItem>(item);

                shoppingList.ItemsSource = dataa;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message.ToString(), "Ok");
            }
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddingItems());
        }

        private async void PurchasedButton_Clicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var item = (ListedItem)btn.BindingContext;
            int listedItem = item.ListedItemId;

            HttpClientHandler insecureHandler = GetInsecureHandler();
            HttpClient client = new HttpClient(insecureHandler);
            client.BaseAddress = new Uri("https://10.0.2.2:7103/");
            HttpResponseMessage response = await client.DeleteAsync("api/shoppingList/purchased/" + listedItem);

            if (response.IsSuccessStatusCode)
            {
                shoppingList.ItemsSource = null;
                LoadDataFromRestApi();
                await DisplayAlert("Purchsed", item.ProductName + " has been purchased", "Ok");
            }
            else
            {
                var data = await response.Content.ReadAsStringAsync();
                string ss = data;
                await DisplayAlert("Error", ss , "Ok");
            }

        }

        private async void DeleteWholeList_Clicked(object sender, EventArgs e)
        {
            HttpClientHandler insecureHandler = GetInsecureHandler();
            HttpClient client = new HttpClient(insecureHandler);
            client.BaseAddress = new Uri("https://10.0.2.2:7103/");
            HttpResponseMessage response = await client.DeleteAsync("api/shoppingList/");

            if (response.IsSuccessStatusCode)
            {
                shoppingList.ItemsSource = null;
                LoadDataFromRestApi();
                await DisplayAlert("Deleted", "All items in shopping list have been deleted", "Ok");
            }
            else
            {
                var data = await response.Content.ReadAsStringAsync();
                string ss = data;
                await DisplayAlert("Error", ss, "Ok");
            }
        }

        async void EditList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditShoppingList());
        }
    }
}
