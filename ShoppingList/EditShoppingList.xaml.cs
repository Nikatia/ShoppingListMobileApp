using Newtonsoft.Json;
using ShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Xamarin.Forms.Internals.Profile;

namespace ShoppingList
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditShoppingList : ContentPage
    {
        public EditShoppingList()
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

        async void EditButton_Clicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var item = (ListedItem)btn.BindingContext;
            int listedId = item.ListedItemId;
            int product = item.ProductId;
            int amount = item.Amount;

            HttpClientHandler insecureHandler = GetInsecureHandler();
            HttpClient client = new HttpClient(insecureHandler);
            client.BaseAddress = new Uri("https://10.0.2.2:7103/");

            if (amount != 0)
            {
                ListedItem listedItem = new ListedItem() { ListedItemId = listedId, ProductId = product, Amount = amount };
                var content = JsonConvert.SerializeObject(item);

                HttpContent httpContent = new StringContent(content, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PutAsync("api/shoppingList/edit/" + listedId, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Update", "Selected item on the shopping list has been updated", "Ok");
                }
                else
                {
                    var data = await response.Content.ReadAsStringAsync();
                    string ss = data;
                    await DisplayAlert("Error", ss, "Ok");
                }
            }
            else if (amount == 0)
            {
                HttpResponseMessage response = await client.DeleteAsync("api/shoppingList/delete/" + listedId);

                if (response.IsSuccessStatusCode)
                {
                    shoppingList.ItemsSource = null;
                    LoadDataFromRestApi();
                    await DisplayAlert("Delete", "Selected item has been removed from the shopping list.", "Ok");
                }
                else
                {
                    var data = await response.Content.ReadAsStringAsync();
                    string ss = data;
                    await DisplayAlert("Error", ss, "Ok");
                }
            }
            
        }
    }
}