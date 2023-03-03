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

        async void LoadCategoryNameFromRestApi(int catId)
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
                string categoryName = await client.GetStringAsync("api/category/" + catId);
                WhatCategory.Text = categoryName;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message.ToString(), "Ok");
            }
        }

        private void pickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            newProduct.IsVisible = true;
            int catId = Convert.ToInt32(WhatCategory2.Text);
            LoadProductsFromRestApi(catId);
            LoadCategoryNameFromRestApi(catId);
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
            try
            {
                bool success = false;
                foreach (var i in selectedProductList)
                {
                    int product = i.ProductId;
                    int amount = i.Amount;

                    //checking if product is already on the shopping list
                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
                    client.BaseAddress = new Uri("https://10.0.2.2:7103/");
                    string checkResponse = await client.GetStringAsync("api/shoppingList/check/" + i.ProductId);

                    if (checkResponse != "Does not exist")
                    {
                        //checking product's amount on the existing entry
                        IEnumerable<ListedItem> oldItem = JsonConvert.DeserializeObject<ListedItem[]>(checkResponse);
                        ObservableCollection<ListedItem> oldData = new ObservableCollection<ListedItem>(oldItem);
                        int oldAmount = oldData.FirstOrDefault().Amount;
                        int listedId = oldData.FirstOrDefault().ListedItemId;

                        //updating the amount
                        int newAmount = oldAmount + amount;

                        ListedItem listedItem = new ListedItem() { ListedItemId = listedId, ProductId = product, Amount = newAmount };
                        var content = JsonConvert.SerializeObject(listedItem);
                        HttpContent httpContent = new StringContent(content, Encoding.UTF8);
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        HttpResponseMessage response = await client.PutAsync("api/shoppingList/edit/" + listedId, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            success = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                    else
                    {

                        ListedItem item = new ListedItem() { ProductId = product, Amount = amount };
                        var content = JsonConvert.SerializeObject(item);


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
                }

                if (success == true)
                {
                    selectedProductList.Clear();
                    await DisplayAlert("Success", "All selected items have been added to the shopping list", "Ok");
                    await Navigation.PushAsync(new MainPage());
                }
                else
                {
                    await DisplayAlert("Ups", "Something went wrong while adding the items", "Ok");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private void Amount_Focused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            if (entry.Text == "0")
            {
                entry.Text = "";
            }
        }

        private void Amount_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            if (entry.Text == "")
            {
                entry.Text = "0";
            }
        }

        private async void NewProduct_Clicked(object sender, EventArgs e)
        {
            string categoryName = WhatCategory.Text;

            if (categoryName == "")
            {
                await DisplayAlert("Error", "Please, select category from dropdown list first", "Ok");
              
            }
            else
            {
                int categoryId = Convert.ToInt32(WhatCategory2.Text);
                string productName = await DisplayPromptAsync("New " + categoryName + " Product", "What is new Product's Name?");

                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
                client.BaseAddress = new Uri("https://10.0.2.2:7103/");
                HttpResponseMessage response = await client.GetAsync("api/products/productExists/" + productName);


                if (response.IsSuccessStatusCode)
                {
                    string oldCategoryId = await client.GetStringAsync("api/products/productExists/" + productName);

                    string oldCategoryName = await client.GetStringAsync("api/category/" + oldCategoryId);

                    await DisplayAlert("Product Exists", "Product with a name, which you are trying to create already exists. It is located in " + oldCategoryName + " Category.", "Ok");
                }
                else if (productName == null) { }
                else
                {
                    Product product = new Product() { CategoryId = categoryId, ProductName = productName };
                    var content = JsonConvert.SerializeObject(product);


                    HttpContent httpContent = new StringContent(content, Encoding.UTF8);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage newProductResponse = await client.PostAsync("api/products", httpContent);
                    LoadProductsFromRestApi(categoryId);

                    await DisplayAlert("Product Added", "Product has been added", "Ok");
                }
            }            
        }
    }
}