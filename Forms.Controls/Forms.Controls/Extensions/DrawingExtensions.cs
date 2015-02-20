using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Forms.Controls.Extensions
{
    public static class DrawingExtensions
    {
        public static bool CollidesWith(this Rectangle rect, Rectangle second)
        {
             return rect.Y < second.Y + second.Height && second.Y < rect.Y + rect.Height;
        }
    }
}
