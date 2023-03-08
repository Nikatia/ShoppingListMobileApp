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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingList
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddingItems : ContentPage
    {
        List<ListedItem> selectedProductList = new List<ListedItem>(); //temporary list for accepting later

        public AddingItems()
        {
            InitializeComponent();
            BindingContext = new AddingItemsViewModel();
        }

        protected override async void OnAppearing()
        {
            pickerCategory.ItemsSource = await GetCategoriesAsync(); //Loads categories to Picker
            base.OnAppearing();
        }
        
        //gets all categories from Category Table through API
        private async Task<List<Category>> GetCategoriesAsync()
        {
            List<Category> CategoriesList = new List<Category>();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("writeYourLinkHere.net");
            string json = await client.GetStringAsync("api/category");
            CategoriesList = JsonConvert.DeserializeObject<List<Category>>(json);

            return CategoriesList;
        }

        //Loads all products from selected category and displays them in productList ListView
        async void LoadProductsFromRestApi(int catId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("writeYourLinkHere.net");
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

        //Showes selected category name in hidden WhatCategory label
        async void LoadCategoryNameFromRestApi(int catId)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("writeYourLinkHere.net");
                string categoryName = await client.GetStringAsync("api/category/" + catId);
                WhatCategory.Text = categoryName;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message.ToString(), "Ok");
            }
        }

        //When Category is selected from Picker, then...
        private void pickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            newProduct.IsVisible = true;
            int catId = Convert.ToInt32(WhatCategory2.Text);
            LoadProductsFromRestApi(catId);
            LoadCategoryNameFromRestApi(catId);
        }

        //Adding Product to temporary shopping list.
        private void Button_Clicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var item = (ListedItem)btn.BindingContext;
            bool exists = false;

            if (item.Amount == 0) 
            {
                foreach (var i in selectedProductList)
                {
                    //if item already exists on temporary list and entered amount is 0, then remove it from temporary list
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

                if (exists == false) //if product doesn't exist on temporary list, add it
                {
                    selectedProductList.Add(item);
                    selectedList.ItemsSource = null;
                    selectedList.ItemsSource = selectedProductList;
                }
                else //if it doesn't, then update list
                {
                    selectedList.ItemsSource = null;
                    selectedList.ItemsSource = selectedProductList;
                }
            }
        }

        //Transfer temporary list to shopping list
        private async void AddItems_Clicked(object sender, EventArgs e)
        {
            try
            {
                bool success = false;
                foreach (var i in selectedProductList)
                {
                    int product = i.ProductId;
                    int amount = i.Amount;

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("writeYourLinkHere.net");
                    string checkResponse = await client.GetStringAsync("api/shoppingList/check/" + i.ProductId);

                    //if item already exists on the shopping list, then increase its amount by number, that has been entered
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
                    else //if item does not exist on the shopping list, then add it
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

        //If before focusing amount is 0, then after focusing change it to empty box
        private void Amount_Focused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            if (entry.Text == "0")
            {
                entry.Text = "";
            }
        }

        //If after unfocusing amount entry box is empty, change it to 0
        private void Amount_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            if (entry.Text == "")
            {
                entry.Text = "0";
            }
        }

        //Adding new product to selected category
        private async void NewProduct_Clicked(object sender, EventArgs e)
        {
            //hidden label WhatCategory, which was assigned in LoadCategoryNameFromRestApi();
            string categoryName = WhatCategory.Text; 

            if (categoryName == "") //if there is no category selected
            {
                await DisplayAlert("Error", "Please, select category from dropdown list first", "Ok");
              
            }
            else
            {
                int categoryId = Convert.ToInt32(WhatCategory2.Text);
                string productName = await DisplayPromptAsync("New " + categoryName + " Product", "What is new Product's Name?");
                if (productName != null) //if "cancel" was not pressed
                {
                    if (productName.Length == 0)
                    {
                        await DisplayAlert("Error", "Product name cannot be empty", "Ok");
                    }
                    else if (productName.Length > 150) //150 characters is max in the database
                    {
                        await DisplayAlert("Error", "Product name is too long. Max 150 characters allowed.", "Ok");
                    }
                    else
                    {
                        //First check if product with given name already exists in the database
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("writeYourLinkHere.net");
                        HttpResponseMessage response = await client.GetAsync("api/products/productExists/" + productName);

                        if (response.IsSuccessStatusCode)//if product exists
                        {
                            string oldCategoryId = await client.GetStringAsync("api/products/productExists/" + productName);

                            string oldCategoryName = await client.GetStringAsync("api/category/" + oldCategoryId);

                            await DisplayAlert("Product Exists", "Product with a name, which you are trying to create already exists. It is located in " + oldCategoryName + " Category.", "Ok");
                        }
                        else //if product does not exist
                        {
                            //ask for product's picture link
                            string productPicture = await DisplayPromptAsync("Picture link for " + productName, "If you don't want to include picture, leave it empty and press Ok.");
                            if (productPicture != null) //if pressed "Ok"
                            {
                                Product product = new Product() { CategoryId = categoryId, ProductName = productName, PictureLink = productPicture };
                                var content = JsonConvert.SerializeObject(product);


                                HttpContent httpContent = new StringContent(content, Encoding.UTF8);
                                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                HttpResponseMessage newProductResponse = await client.PostAsync("api/products", httpContent);
                                LoadProductsFromRestApi(categoryId);

                                await DisplayAlert("Product Added", "Product has been added", "Ok");
                            }
                            else //if pressed "Cancel"
                            {
                                await DisplayAlert("Cancelled", "Adding " + productName + " has been cancelled.", "Ok");
                            }
                        }
                    }
                }
            }            
        }
    }
}