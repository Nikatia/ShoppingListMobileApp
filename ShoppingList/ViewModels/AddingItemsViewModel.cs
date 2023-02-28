using ShoppingList;
using ShoppingList.Models;
using System.Collections.Generic;

namespace ShoppingList
{
    public class AddingItemsViewModel : ViewModelBase
    {
        Category selectedCategory;

        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                if (selectedCategory != value)
                {
                    selectedCategory = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}