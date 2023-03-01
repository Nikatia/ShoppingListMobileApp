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

namespace ShoppingList
{
    public partial class MainPage : CarouselPage
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

        private void shoppingList_ItemTapped(object sender, ItemTappedEventArgs e)
        {

        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddingItems());
        }






        //        try
        //            {
        //                #if DEBUG
        //                HttpClientHandler insecureHandler = GetInsecureHandler();
        //        HttpClient client = new HttpClient(insecureHandler);
        //#else
        //                        HttpClient client = new HttpClient();
        //#endif

        //        client.BaseAddress = new Uri("https://10.0.2.2:7103/");
        //        string json = await client.GetStringAsync("api/shoppingList");

        //        string jsonData = @"{""ProductID"" : ""myusername"", ""Amount"" : ""mypassword""}";

        //        IEnumerable<ListedItem> item = JsonConvert.DeserializeObject<ListedItem[]>(json);
        //        ObservableCollection<ListedItem> dataa = new ObservableCollection<ListedItem>(item);

        //        shoppingList.ItemsSource = dataa;


        //                string jsonData = @"{""username"" : ""myusername"", ""password"" : ""mypassword""}"

        //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        //        HttpResponseMessage response = await client.PostAsync("/foo/login", content);

        //        // this result string should be something like: "{"token":"rgh2ghgdsfds"}"
        //        var result = await response.Content.ReadAsStringAsync();

        //    }
        //            catch (Exception ex)
        //            {
        //                await DisplayAlert("Error", ex.Message.ToString(), "Ok");
        //}
    }
}
