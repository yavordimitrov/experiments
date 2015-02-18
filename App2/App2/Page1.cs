using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace App2
{
    public class Page1 : ContentPage
    {
        public int RowHeight { get; set; }
        public int SeparatorOffset { get; set; }
        public int Padding { get; set; }
        private List<View> bindedViews = new List<View>();
        private RelativeLayout relative { get; set; }
        Random rnd = new Random();
        public Page1()
        {
            RowHeight = 44;
            Padding = 10;
            SeparatorOffset = 50;
            ScrollView view = new ScrollView() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            relative = new RelativeLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            BoxView lastSeparator = null;
            Label dummyLabel = new Label() { Text = "dummy", IsVisible = false };
            relative.Children.Add(dummyLabel, xConstraint: null);
            for (int i = 0; i < 25; i++)
            {
                BoxView separator = new BoxView() { HeightRequest = 1, Color = Color.White, HorizontalOptions = LayoutOptions.FillAndExpand };
                Label timeLabel = new Label() { Text = i.ToString() };


                if (i == 0)
                {
                    relative.Children.Add(separator,
                        yConstraint: Constraint.RelativeToParent((d) => { return Padding; }),
                        xConstraint: Constraint.RelativeToParent((d) => { return SeparatorOffset; }),
                        widthConstraint: Constraint.RelativeToParent((d) => { return d.Width - SeparatorOffset; }));

                }
                else
                {
                    relative.Children.Add(separator,
                        xConstraint: Constraint.RelativeToParent((d) => { return SeparatorOffset; }),
                        yConstraint: Constraint.RelativeToView(lastSeparator, (parent, sibling) => { return sibling.Y + sibling.Height + RowHeight; }),
                        widthConstraint: Constraint.RelativeToParent((d) => { return d.Width - SeparatorOffset; }));
                }

                relative.Children.Add(timeLabel,
                        xConstraint: Constraint.RelativeToParent((d) => { return 5; }),
                        yConstraint: Constraint.RelativeToView(separator, (parent, sibling) => { return sibling.Y - dummyLabel.Height / 2; }));

                lastSeparator = separator;

            }
            Button add = new Button(){ Text = "clickMe"};
            add.Clicked += add_Clicked;
            relative.Children.Add(add, xConstraint: Constraint.RelativeToParent((d) => { return 0; }));
            view.Content = relative;

            Content = view;
        }

        void add_Clicked(object sender, EventArgs e)
        {
            foreach (var view in bindedViews)
            {
                relative.Children.Remove(view);
            }
            bindedViews.Clear();
            relative.ForceLayout();
            Load();
        }
        void Load()
        {
            foreach (var rect in CalculateTransforms(GetItems()))
            {
                var binde =new BoxView() { Color = Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)) };
                relative.Children.Add(binde,
                    xConstraint: Constraint.Constant(rect.X),
                    yConstraint: Constraint.Constant(rect.Y),
                    widthConstraint: Constraint.Constant(rect.Width),
                    heightConstraint: Constraint.Constant(rect.Height));
                bindedViews.Add(binde);
            }
        }
        private List<Rectangle> CalculateTransforms(List<ScheduleItem> items)
        {
            List<Rectangle> rects = new List<Rectangle>();
            double offsetPerMinute = (double)(RowHeight + 1) / 60;
            double totalWidth = 300;
            foreach (var columns in LayoutEvents(items))
            {
                foreach (var item in columns)
                {
                    double top = item.Start.TotalMinutes * offsetPerMinute + Padding;
                    double left = item.Left * totalWidth + SeparatorOffset;
                    double width = totalWidth * item.Right - totalWidth * item.Left;
                    double height = ((item.End.TotalMinutes * offsetPerMinute) - top) + Padding;
                    rects.Add(new Rectangle(left, top, width, height));
                }
            }
            return rects;
        }
        private List<ScheduleItem> GetItems()
        {
            List<ScheduleItem> items = new List<ScheduleItem>();

            for (int i = 0; i < 50; i++)
            {
                TimeSpan start = TimeSpan.FromMinutes((double)rnd.Next(0, 22 * 60));
                TimeSpan end = TimeSpan.FromMinutes((double)rnd.Next((int)start.TotalMinutes, (int)start.TotalMinutes + 120));
                items.Add(new ScheduleItem() { Start = start, End = end });
            }
            return items;
        }


        /// Pick the left and right positions of each event, such that there are no overlap.
        /// Step 3 in the algorithm.
        List<List<ScheduleItem>> LayoutEvents(IEnumerable<ScheduleItem> events)
        {
            var columns = new List<List<ScheduleItem>>();
            TimeSpan? lastEventEnding = null;
            foreach (var ev in events.OrderBy(ev => ev.Start).ThenBy(ev => ev.End))
            {
                if (ev.Start >= lastEventEnding)
                {
                    PackEvents(columns);
                    lastEventEnding = null;
                }
                bool placed = false;
                foreach (var col in columns)
                {
                    if (!col.Last().CollidesWith(ev))
                    {
                        col.Add(ev);
                        placed = true;
                        break;
                    }
                }
                if (!placed)
                {
                    columns.Add(new List<ScheduleItem> { ev });
                }
                if (lastEventEnding == null || ev.End > lastEventEnding.Value)
                {
                    lastEventEnding = ev.End;
                }
            }
            if (columns.Count > 0)
            {
                PackEvents(columns);
            }
            return columns;
        }

        /// Set the left and right positions for each event in the connected group.
        /// Step 4 in the algorithm.
        void PackEvents(List<List<ScheduleItem>> columns)
        {
            float numColumns = columns.Count;
            int iColumn = 0;
            foreach (var col in columns)
            {
                foreach (var ev in col)
                {
                    int colSpan = ExpandEvent(ev, iColumn, columns);
                    ev.Left = iColumn / numColumns;
                    ev.Right = (iColumn + colSpan) / numColumns;
                }
                iColumn++;
            }
        }

        /// Checks how many columns the event can expand into, without colliding with
        /// other events.
        /// Step 5 in the algorithm.
        int ExpandEvent(ScheduleItem ev, int iColumn, List<List<ScheduleItem>> columns)
        {
            int colSpan = 1;
            foreach (var col in columns.Skip(iColumn + 1))
            {
                foreach (var ev1 in col)
                {
                    if (ev1.CollidesWith(ev))
                    {
                        return colSpan;
                    }
                }
                colSpan++;
            }
            return colSpan;
        }



    }






    public class ScheduleItem
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public double Left { get; set; }
        public double Right { get; set; }

        public bool CollidesWith(ScheduleItem other)
        {
            return Start < other.End && other.Start < End;
        }
    }
}
