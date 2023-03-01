using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingList
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddingItems : ContentPage
    {
        List<ListedItem> selectedProductList = new List<ListedItem>();
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

        async void LoadCategoriesFromRestApi()
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
                string json = await client.GetStringAsync("api/category");

                IEnumerable<ListedItem> category = JsonConvert.DeserializeObject<ListedItem[]>(json);
                ObservableCollection<ListedItem> data = new ObservableCollection<ListedItem>(category);

                pickerCategory.ItemsSource = data;


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message.ToString(), "Ok");
            }
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

        async void LoadProductsFromRestApi(int catId)
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
                string json = await client.GetStringAsync("api/products/" + catId);

                IEnumerable<ListedItem> item = JsonConvert.DeserializeObject<ListedItem[]>(json);
                ObservableCollection<ListedItem> dataa = new ObservableCollection<ListedItem>(item);

                productList.ItemsSource = dataa;


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message.ToString(), "Ok");
            }
        }

        private void pickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            productList.IsVisible = true;
            int catId = Convert.ToInt32(WhatCategory2.Text);
            LoadProductsFromRestApi(catId);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var item = (ListedItem)btn.BindingContext;
            bool exists = false;

            if (item.Amount == 0)
            {
                foreach (var i in selectedProductList)
                {
                    if (i.ProductId == item.ProductId)
                    {
                        exists = true;
                        selectedProductList.Remove(i);
                        break;
                    }
                }
                if (exists == true)
                {
                    selectedList.ItemsSource = null;
                    selectedList.ItemsSource = selectedProductList;
                }
                else
                {
                    DisplayAlert("Invalid Amount", "You cannot add 0 amount of an item to the shopping list", "Ok");
                }
            }
            else
            {
                foreach (var i in selectedProductList)
                {
                    if (i.ProductId == item.ProductId)
                    {
                        exists = true;
                    }
                }

                if (exists == false)
                {
                    selectedProductList.Add(item);
                    selectedList.ItemsSource = null;
                    selectedList.ItemsSource = selectedProductList;
                }
                else
                {
                    selectedList.ItemsSource = null;
                    selectedList.ItemsSource = selectedProductList;
                }
            }
        }

        private async void AddItems_Clicked(object sender, EventArgs e)
        {
            bool success = false;
            try
            {
                foreach (var i in selectedProductList)
                {
                    int product = i.ProductId;
                    int amount = i.Amount;
                    ListedItem item = new ListedItem() { ProductId = product, Amount = amount };
                    var content = JsonConvert.SerializeObject(item);

                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
                    client.BaseAddress = new Uri("https://10.0.2.2:7103/");

                    HttpContent httpContent = new StringContent(content, Encoding.UTF8);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage response = await client.PostAsync("api/shoppingList", httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        success = true;
                    }
                    else
                    {
                        success = false;
                    }
                }

                if (success == true)
                {
                    selectedProductList.Clear();
                    selectedList.ItemsSource = null;
                    selectedList.ItemsSource = selectedProductList;
                    await DisplayAlert("Success", "All selected items have been added to the shopping list", "Ok");
                }
                else
                {
                    await DisplayAlert("Ups", "Something went wrong while addint the items", "Ok");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("error", ex.Message, "Ok");
            }
        }
    }
}