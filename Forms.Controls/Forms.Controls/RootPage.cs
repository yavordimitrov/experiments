using Forms.Controls.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Xamarin.Forms;

namespace Forms.Controls
{
    public class RootPage : ContentPage
    {

        public RootPage()
        {
           
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
           
            DailyCalendarView view = new DailyCalendarView();
            view.SetBinding(DailyCalendarView.ItemsProperty, "Items");
            view.BindingContext = new ItemsViewModel();
            view.HorizontalOptions = LayoutOptions.FillAndExpand;
            view.VerticalOptions = LayoutOptions.FillAndExpand;
            Content = view;
        }

    }

    public class ItemsViewModel
    {
        public ObservableCollection<DailyEventItem> Items { get; set; }

        public ItemsViewModel()
        {
            Items = new ObservableCollection<DailyEventItem>(GetItems());
        }

        private List<DailyEventItem> GetItems()
        {
            Random rnd = new Random();
            List<DailyEventItem> items = new List<DailyEventItem>();

            for (int i = 0; i < 25; i++)
            {
                TimeSpan start = TimeSpan.FromMinutes((double)rnd.Next(0, 22 * 60));
                TimeSpan end = TimeSpan.FromMinutes((double)rnd.Next((int)start.TotalMinutes, (int)start.TotalMinutes + 120));
                items.Add(new DailyEventItem() { Start = start, End = end, Color = Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)) });
            }
            return items;
        }
    }
}
