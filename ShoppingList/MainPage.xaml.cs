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

        //Loads Shopping List Table from database through API and displays in shoppingList ListView
        async void LoadDataFromRestApi()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("writeYourLinkHere.net");
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

        //Opens page, where adding new items to shopping list and adding new products to categories is possible
        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddingItems());
        }

        //Deletes item from Shopping list table and adds it to Purchased Table through API
        private async void PurchasedButton_Clicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var item = (ListedItem)btn.BindingContext;
            int listedItem = item.ListedItemId;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("writeYourLinkHere.net");
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

        //Deletes all rows from Shopping list Table through API
        private async void DeleteWholeList_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Delete", "Delete whole shopping list? \r\n\r\nRemember, that it doesn't equal purchasing.", "Delete", "Cancel");

            if (answer == true)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("writeYourLinkHere.net");
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
        }

        //Opens page, where editing shopping list is possible
        async void EditList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditShoppingList());
        }

        //Opens page with product's picture
        private async void shoppingList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListedItem item = (ListedItem)shoppingList.SelectedItem;
            int id = item.ProductId;
            await Navigation.PushAsync(new CheckingItem(id));
        }

        //Back button on Shopping list main page is disabled
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
