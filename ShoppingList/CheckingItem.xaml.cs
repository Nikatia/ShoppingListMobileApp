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
	public partial class CheckingItem : ContentPage
	{
		int prodId;
        HttpClient client;

        public CheckingItem (int id)
		{
			InitializeComponent ();

			prodId = id;
            GetItem();
        }

        //Load data about product and display picture if it exists
        public async void GetItem ()
		{
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("writeYourLinkHere.net");
            string json = await client.GetStringAsync("api/products/product/" + prodId);

            IEnumerable<Product> item = JsonConvert.DeserializeObject<Product[]>(json);
            ObservableCollection<Product> data = new ObservableCollection<Product>(item);
            string prodName = data.FirstOrDefault().ProductName;
            string prodPic = data.FirstOrDefault().PictureLink;
            upperLabel.Text = prodName;
            if (prodPic != null)
            {
                productPicture.Source = prodPic;
            }
            else
            {
                productPicture.IsVisible = false;
                noImageLabel.IsVisible = true;
            }            
        }
	}
}