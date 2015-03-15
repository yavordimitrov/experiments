using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using App4;
using App4.iOS;
using Xamarin.Forms.Platform.iOS;
using CoreGraphics;

[assembly: ExportRenderer(typeof(AppIcon), typeof(IconRenderer))]
namespace App4.iOS
{
    public class IconRenderer : BoxRenderer
    {
        private static CGPath PEANUT = Create();
        private static double PEANUTHEIGHT = 17;
        public IconRenderer()
        {
        }
        public override void Draw()
        {
            var cbv = (AppIcon)Element;
            var view = (AppIcon)Element;
            double ratio = view.Height / PEANUTHEIGHT;
            float floatRatio = Convert.ToSingle(ratio);
            CGAffineTransform tr = CGAffineTransform.MakeScale(floatRatio, floatRatio);
            using (var context = UIGraphics.GetCurrentContext())
            {
                context.SetFillColor(cbv.Color.ToCGColor());
                context.AddPath(PEANUT);
                context.DrawPath(CGPathDrawingMode.FillStroke);
            }
        }

        private static CGPath Create()
        {
            CGPath p = new CGPath();
            string path = @"M 9.129 12.529 C 9.129 13.29 8.917 14.059 8.509 14.765 C 7.272 16.908 4.532 17.642 2.389 16.405 C 0.246 15.168 -0.488 12.428 0.749 10.285 C 1.165 9.564 1.752 9.002 2.429 8.622 L 2.472 8.597 C 3.155 8.216 3.746 7.652 4.165 6.926 C 4.552 6.256 4.746 5.528 4.764 4.806 L 4.777 4.599 C 4.76 3.808 4.952 3.002 5.377 2.267 C 6.614 0.124 9.354 -0.61 11.497 0.627 C 13.64,1.8639999999999999,14.374,4.604,13.137,6.747 C 12.715 7.478 12.118 8.045 11.429 8.426 L 11.35 8.481 C 10.703 8.86 10.143 9.408 9.741 10.104 C 9.32 10.833 9.115 11.62 9.128 12.405 L 9.129 12.529 Z";

            String[] tokens = path.Split(separator: new char[2] { ',', ' ' }, options: StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            while (i < tokens.Length)
            {
                String token = tokens[i++];
                if (token.Equals("M"))
                {
                    float x = float.Parse(tokens[i++]);
                    float y = float.Parse(tokens[i++]);
                    p.MoveToPoint(x, y);
                }
                else
                    if (token.Equals("L"))
                    {
                        float x = float.Parse(tokens[i++]);
                        float y = float.Parse(tokens[i++]);
                        p.AddLineToPoint(x, y);
                    }
                    else
                        if (token.Equals("C"))
                        {
                            float x1 = float.Parse(tokens[i++]);
                            float y1 = float.Parse(tokens[i++]);
                            float x2 = float.Parse(tokens[i++]);
                            float y2 = float.Parse(tokens[i++]);
                            float x3 = float.Parse(tokens[i++]);
                            float y3 = float.Parse(tokens[i++]);
                            p.AddCurveToPoint(x1, y1, x2, y2, x3, y3);
                        }
                        else
                            if (token.Equals("Z"))
                            {
                                p.CloseSubpath();
                            }
            }
            return p;
        }
    }
}