using Newtonsoft.Json;
using ShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingList
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddingItems : ContentPage
    {
        public AddingItems()
        {
            InitializeComponent();
            BindingContext = new AddingItemsViewModel();
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

        protected override async void OnAppearing()
        {
            pickerCategory.ItemsSource = await GetCategoriesAsync();
            base.OnAppearing();
        }

        private async Task<List<Category>> GetCategoriesAsync()
        {
            List<Category> CategoriesList = new List<Category>();


            #if DEBUG
            HttpClientHandler insecureHandler = GetInsecureHandler();
            HttpClient client = new HttpClient(insecureHandler);
            #else
                        HttpClient client = new HttpClient();
            #endif

            client.BaseAddress = new Uri("https://10.0.2.2:7103/");
            string json = await client.GetStringAsync("api/category");
            CategoriesList = JsonConvert.DeserializeObject<List<Category>>(json);

            return CategoriesList;
        }

        private async Task<List<Product>> GetProductsAsync(int catId)
        {
            List<Product> ProductList = new List<Product>();


#if DEBUG
            HttpClientHandler insecureHandler = GetInsecureHandler();
            HttpClient client = new HttpClient(insecureHandler);
#else
                        HttpClient client = new HttpClient();
#endif

            client.BaseAddress = new Uri("https://10.0.2.2:7103/");
            string json = await client.GetStringAsync("api/products/" + catId);
            ProductList = JsonConvert.DeserializeObject<List<Product>>(json);

            return ProductList;
        }
        //async void LoadProductsFromRestApi(int catId)
        //{
        //    try
        //    {

        //        #if DEBUG
        //        HttpClientHandler insecureHandler = GetInsecureHandler();
        //        HttpClient client = new HttpClient(insecureHandler);
        //        #else
        //                HttpClient client = new HttpClient();
        //        #endif

        //        client.BaseAddress = new Uri("https://10.0.2.2:7103/");
        //        string json = await client.GetStringAsync("api/products/" + catId);

        //        IEnumerable<ListedItem> item = JsonConvert.DeserializeObject<ListedItem[]>(json);
        //        ObservableCollection<ListedItem> dataa = new ObservableCollection<ListedItem>(item);

        //        pickerProduct.ItemsSource = dataa;


        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Error", ex.Message.ToString(), "Ok");
        //    }
        //}

        private async void pickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            pickerProduct.IsVisible = true;
            int catId = Convert.ToInt32(WhatCategory2.Text);
            pickerProduct.ItemsSource = await GetProductsAsync(catId);
        }



        private void productList_ItemTapped(object sender, ItemTappedEventArgs e)
        {

        }

        private void AddItems_Clicked(object sender, EventArgs e)
        {

        }
    }
}