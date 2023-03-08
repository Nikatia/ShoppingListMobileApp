﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingList
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage()) 
            {
                BarBackgroundColor = Color.FromHex("#bb86fc")
            };
        }

    protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
