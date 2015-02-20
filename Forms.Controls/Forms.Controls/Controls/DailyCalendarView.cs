using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Forms.Controls.Extensions;

namespace Forms.Controls.Controls
{
    public class DailyCalendarView : ScrollView
    {
        private bool _widthReady;
        static Random rnd = new Random();

        RelativeLayout _layout;
        private List<View> _bindedViews;
        double _separatorOffset;
        //top and bottom offset so we dont get labels clipped out of view
        double _offset;
        double _eventAreaWidth;


        public static readonly BindableProperty ItemsProperty = BindableProperty.Create<DailyCalendarView, ObservableCollection<DailyEventItem>>(c => c.Items, new ObservableCollection<DailyEventItem>());
        public static readonly BindableProperty RowHeightProperty = BindableProperty.Create<DailyCalendarView, double>(c => c.RowHeight, 44);


        public ObservableCollection<DailyEventItem> Items
        {
            get { return (ObservableCollection<DailyEventItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public double RowHeight
        {
            get { return (double)GetValue(RowHeightProperty); }
            set { SetValue(RowHeightProperty, value); }
        }

        public DailyCalendarView()
        {
            _separatorOffset = 70;
            _offset = 10;
            _layout = new RelativeLayout();
            _layout.HorizontalOptions = LayoutOptions.FillAndExpand;
            _layout.VerticalOptions = LayoutOptions.FillAndExpand;
            _bindedViews = new List<View>();
            _eventAreaWidth = 300;
            Content = _layout;
            DrawBackDrop();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == ItemsProperty.PropertyName)
            {
                DrawEvents();
            }
        }
        private void DrawBackDrop()
        {

            BoxView lastSeparator = null;

            //Used so we can reference the label's height when adding the other labels
            Label dummyLabel = new Label() { Text = "dummy", IsVisible = false };
            _layout.Children.Add(dummyLabel, xConstraint: null);

            DateTime date = DateTime.Now;

            for (int i = 0; i < 25; i++)
            {
                BoxView separator = new BoxView() { HeightRequest = 1, Color = Color.White, HorizontalOptions = LayoutOptions.FillAndExpand };
                Label timeLabel = new Label() { Text = date.Date.AddHours(i).ToString("hh:mm tt") };

                if (i == 0)
                {
                    _layout.Children.Add(separator,
                        yConstraint: Constraint.RelativeToParent((d) => { return _offset; }),
                        xConstraint: Constraint.RelativeToParent((d) => { return _separatorOffset; }),
                        widthConstraint: Constraint.RelativeToParent((d) => { return d.Width - _separatorOffset; }));

                }
                else
                {
                    _layout.Children.Add(separator,
                        xConstraint: Constraint.RelativeToParent((d) => { return _separatorOffset; }),
                        yConstraint: Constraint.RelativeToView(lastSeparator, (parent, sibling) => { return sibling.Y + sibling.Height + RowHeight; }),
                        widthConstraint: Constraint.RelativeToParent((d) => { return d.Width - _separatorOffset; }));
                }

                _layout.Children.Add(timeLabel,
                        xConstraint: Constraint.RelativeToParent((d) => { return 5; }),
                        yConstraint: Constraint.RelativeToView(separator, (parent, sibling) => { return sibling.Y - dummyLabel.Height / 2; }));

                lastSeparator = separator;
            }
        }

        private void DrawEvents()
        {
            List<Rectangle> rects = new List<Rectangle>();
            double offsetPerMinute = (double)(RowHeight + 1) / 60;
            for (int i = 0; i < Items.Count(); i++)
            {
                double top = Items[i].Start.TotalMinutes * offsetPerMinute + _offset;
                double height = Items[i].End.TotalMinutes * offsetPerMinute - top + _offset;
                rects.Add(new Rectangle(_separatorOffset + _offset / 2, top, _eventAreaWidth, height));
            }
            LayoutEvents(rects);
        }

        /// Pick the left and right positions of each event, such that there are no overlap.
        /// Step 3 in the algorithm.
        void LayoutEvents(List<Rectangle> events)
        {
            var columns = new List<List<Rectangle>>();
            double? lastEventEnding = null;
            foreach (var ev in events.OrderBy(ev => ev.Y).ThenBy(ev => ev.Y + ev.Height))
            {
                if (ev.Y >= lastEventEnding)
                {
                    PackEvents(columns);
                    columns.Clear();
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
                    columns.Add(new List<Rectangle> { ev });
                }

                if (lastEventEnding == null || ev.Y + ev.Height > lastEventEnding.Value)
                {
                    lastEventEnding = ev.Y + ev.Height;
                }
            }
            if (columns.Count > 0)
            {
                PackEvents(columns);
            }
        }

        /// Set the left and right positions for each event in the connected group.
        /// Step 4 in the algorithm.
        void PackEvents(List<List<Rectangle>> columns)
        {
            float numColumns = columns.Count;
            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = 0; j < columns[i].Count; j++)
                {
                    int colSpan = ExpandEvent(columns[i][j], i, columns);

                    Rectangle current = columns[i][j];


                    double left = i / numColumns;
                    double right = (i + colSpan) / numColumns;

                    double x = (left * _eventAreaWidth + _separatorOffset) + _offset / 2;
                    double width = _eventAreaWidth * right - _eventAreaWidth * left;

                    BoxView lbl = new BoxView() { Color = Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)) };
                    _layout.Children.Add(lbl,
                        xConstraint: Constraint.Constant(x),
                        yConstraint: Constraint.Constant(columns[i][j].Y),
                        widthConstraint: Constraint.Constant(width),
                        heightConstraint: Constraint.Constant(columns[i][j].Height));
                    _bindedViews.Add(lbl);
                }
            }
        }

        /// Checks how many columns the event can expand into, without colliding with
        /// other events.
        /// Step 5 in the algorithm.
        int ExpandEvent(Rectangle ev, int iColumn, List<List<Rectangle>> columns)
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
    public class DailyEventItem
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
