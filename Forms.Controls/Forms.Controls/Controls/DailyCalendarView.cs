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

        RelativeLayout _layout;
        private List<View> _bindedViews;
        double _separatorOffset;
        double _offset;
        double _eventAreaWidth;

        private Rectangle _lastTransformBeforeExpand;
        private View _expandedView;

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
            TapGestureRecognizer rootTapRecognizer = new TapGestureRecognizer();
            rootTapRecognizer.Command = new Command(() =>
            {
                if (_expandedView != null)
                {
                    _expandedView.LayoutTo(_lastTransformBeforeExpand);
                    _expandedView = null;
                }
            });
            _layout.GestureRecognizers.Add(rootTapRecognizer);
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
                RenderEvents();
            }
        }

        private void DrawBackDrop()
        {
            BoxView lastSeparator = null;

            ///Since layout is calculated when displayed we dont have a way to get 
            ///the width of the elements at the time we are inserting them
            ///Used so we can reference the label's height when adding the other labels
            Label dummyLabel = new Label() { Text = "not visible", IsVisible = false };

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

        void Tapped(View sender)
        {
            View lbl = sender as View;
            Rectangle curerentTransform = new Rectangle(lbl.X, lbl.Y, lbl.Width, lbl.Height);
            Rectangle targetTransform = new Rectangle(_separatorOffset + _offset / 2, lbl.Y, _eventAreaWidth, lbl.Height);
            _layout.LowerChild(lbl);
            if (_expandedView != null)
            {
                if (_expandedView != lbl)
                {
                    _expandedView.LayoutTo(_lastTransformBeforeExpand);
                    _lastTransformBeforeExpand = curerentTransform;

                    lbl.LayoutTo(targetTransform);

                    _expandedView = lbl;
                }
                else
                {
                    _expandedView.LayoutTo(_lastTransformBeforeExpand);
                    _expandedView = null;
                }
            }
            else
            {
                _expandedView = lbl;
                _lastTransformBeforeExpand = curerentTransform;
                _expandedView.LayoutTo(targetTransform);
            }
        }
        /// Set the top and bottom positions of each event
        private void RenderEvents()
        {
            List<EventRectangle> rects = new List<EventRectangle>();
            double offsetPerMinute = (double)(RowHeight + 1) / 60;
            for (int i = 0; i < Items.Count(); i++)
            {
                double minimumMinutes = Math.Max(Items[i].End.TotalMinutes, Items[i].Start.TotalMinutes + 30);
                double top = Items[i].Start.TotalMinutes * offsetPerMinute + _offset;
                double height = minimumMinutes * offsetPerMinute - top + _offset;
                rects.Add(new EventRectangle(_separatorOffset + _offset / 2, top, _eventAreaWidth, height, Items[i]));
            }
            LayoutColumns(rects);
            PlaceEvents(rects);
        }

        private void PlaceEvents(List<EventRectangle> constraints)
        {
            int i = 0;

            foreach (var constraint in constraints)
            {
                Label lbl = new Label() {LineBreakMode= Xamarin.Forms.LineBreakMode.TailTruncation, Text = "Pick the left and right positions of each event", BackgroundColor = (constraint.Context as DailyEventItem).Color };
                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.CommandParameter = lbl;
                recognizer.Command = new Command<View>((v) => { Tapped(v); });
                lbl.GestureRecognizers.Add(recognizer);
                _layout.Children.Add(lbl,
                    xConstraint: Constraint.Constant(constraint.X + 1),
                    yConstraint: Constraint.Constant(constraint.Y),
                    widthConstraint: Constraint.Constant(constraint.Width - 2),
                    heightConstraint: Constraint.Constant(constraint.Height));
                _bindedViews.Add(lbl);
                i++;
            }
        }


        /// Pick the left and right positions of each event, such that they don't no overlap.
        private void LayoutColumns(List<EventRectangle> events)
        {
            var groupOfOverlapingEvents = new List<List<EventRectangle>>();
            double? lastEventEnding = null;
            foreach (var ev in events.OrderBy(ev => ev.Y).ThenBy(ev => ev.Y + ev.Height))
            {
                if (ev.Y >= lastEventEnding)
                {
                    PackGroups(groupOfOverlapingEvents);
                    groupOfOverlapingEvents.Clear();
                    lastEventEnding = null;
                }
                bool placed = false;
                foreach (var col in groupOfOverlapingEvents)
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
                    groupOfOverlapingEvents.Add(new List<EventRectangle> { ev });
                }

                if (lastEventEnding == null || ev.Y + ev.Height > lastEventEnding.Value)
                {
                    lastEventEnding = ev.Y + ev.Height;
                }
            }
            if (groupOfOverlapingEvents.Count > 0)
            {
                PackGroups(groupOfOverlapingEvents);
            }
        }

        /// Set the left and right positions for each event in the connected group.
        private void PackGroups(List<List<EventRectangle>> columns)
        {
            float numColumns = columns.Count;
            int i = 0;
            foreach (var col in columns)
            {
                foreach (var rect in col)
                {
                    int colSpan = ExpandEvent(rect, i, columns);
                    double left = i / numColumns;
                    double right = (i + colSpan) / numColumns;
                    double x = (left * _eventAreaWidth + _separatorOffset) + _offset / 2;
                    double width = _eventAreaWidth * right - _eventAreaWidth * left;
                    rect.X = x;
                    rect.Width = width;
                }
                i++;
            }
        }

        /// Checks how many columns the event can expand into, without colliding with
        /// other events.
        private int ExpandEvent(EventRectangle ev, int iColumn, List<List<EventRectangle>> columns)
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

        private class EventRectangle
        {
            public EventRectangle(double x, double y, double width, double height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
            public EventRectangle(double x, double y, double width, double height, object context)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Context = context;
            }

            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public object Context { get; set; }

            public bool CollidesWith(EventRectangle second)
            {
                return Y < second.Y + second.Height && second.Y < Y + Height;
            }
        }
    }



    public class DailyEventItem
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public Color Color { get; set; }
        public object Context { get; set; }
    }
}
