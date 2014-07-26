#if __ANDROID__
using System;
using System.Drawing;
using Android.Widget;
using Android.Graphics;
using Android.Views;


namespace Notes
{
    namespace PlatformUI
    {
        public class DroidLabel : PlatformLabel
        {
            TextView Label { get; set; }

            public DroidLabel( )
            {
                Label = new TextView(PlatformCommonUI.Context);
                Label.LayoutParameters = new ViewGroup.LayoutParams(0, 0);

                //Label.SetBackgroundResource(0);
                //Label.Layer.AnchorPoint = new PointF (0, 0);
                //Label.TextAlignment = UITextAlignment.Left;
                //Label.LineBreakMode = UILineBreakMode.WordWrap;
                //Label.Lines = 0;
            }

            //TODO: Do not let outside entities call this (right now I am till I finish the port)
            public static Android.Graphics.Color GetUIColor(uint color)
            {
                return new Android.Graphics.Color(
                    (byte)((color & 0xFF000000) >> 24), //TODO: obviously completely unacceptable.
                    (byte)((color & 0x00FF0000) >> 16), 
                    (byte)((color & 0x0000FF00) >> 8), 
                    (byte)((color & 0x000000FF)));
            }

            // Properties
            public override void SetFont(string fontName, float fontSize)
            {
                Typeface fontFace = Typeface.CreateFromAsset(PlatformCommonUI.Context.Assets, "fonts/" + fontName.ToLower() + ".ttf");
                Label.SetTypeface(fontFace, TypefaceStyle.Normal);
                Label.SetTextSize(Android.Util.ComplexUnitType.Pt, fontSize);
            }

            protected override void setBackgroundColor(uint backgroundColor)
            {
                Label.SetBackgroundColor(GetUIColor(backgroundColor));
            }

            protected override float getOpacity()
            {
                return Label.Alpha;
            }

            protected override void setOpacity(float opacity)
            {
                Label.Alpha = opacity;
            }

            protected override float getZPosition()
            {
                // TODO: There's no current way I can find in Android to do this. But,
                // we only use it for debugging.
                return 0.0f;
            }
            protected override void setZPosition(float zPosition)
            {
                // TODO: There's no current way I can find in Android to do this.
            }

            protected override RectangleF getBounds()
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                return new RectangleF(0, 0, Label.LayoutParameters.Width, Label.LayoutParameters.Height);
            }

            protected override void setBounds(RectangleF bounds)
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                Label.LayoutParameters.Width = (int)bounds.Width;
                Label.LayoutParameters.Height = (int)bounds.Height;
            }

            protected override RectangleF getFrame()
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                RectangleF frame = new RectangleF(Label.GetX(), Label.GetY(), Label.LayoutParameters.Width, Label.LayoutParameters.Height);
                return frame;
            }

            protected override void setFrame(RectangleF frame)
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                setPosition( new System.Drawing.PointF(frame.X, frame.Y) );

                RectangleF bounds = new RectangleF(frame.Left, frame.Top, frame.Width, frame.Height);
                setBounds(bounds);
            }

            protected override System.Drawing.PointF getPosition()
            {
                return new System.Drawing.PointF(Label.GetX(), Label.GetY());
            }
            protected override void setPosition(System.Drawing.PointF position)
            {
                Label.SetX(position.X);
                Label.SetY(position.Y);
            }

            protected override void setTextColor(uint color)
            {
                Label.SetTextColor( GetUIColor(color) );
            }

            protected override string getText()
            {
                return Label.Text;
            }

            protected override void setText(string text)
            {
                Label.Text = text;
            }

            protected override TextAlignment getTextAlignment()
            {
                // gonna have to do a stupid transform
                switch(Label.Gravity)
                {
                    case GravityFlags.Center: return TextAlignment.Center;
                    case GravityFlags.Left: return TextAlignment.Left;
                    case GravityFlags.Right: return TextAlignment.Right;
                    default: return TextAlignment.Left;
                }
            }

            protected override void setTextAlignment(TextAlignment alignment)
            {
                // gonna have to do a stupid transform
                switch(alignment)
                {
                    case TextAlignment.Center: Label.Gravity = GravityFlags.Center; break;
                    case TextAlignment.Left: Label.Gravity = GravityFlags.Left; break;
                    case TextAlignment.Right: Label.Gravity = GravityFlags.Right; break;
                    default: Label.Gravity = GravityFlags.Left; break;
                }
            }

            public override void AddAsSubview(object masterView)
            {
                // we know that masterView will be an iOS View.
                RelativeLayout view = masterView as RelativeLayout;
                if(view == null)
                {
                    throw new InvalidCastException("Object passed to Android AddAsSubview must be a RelativeLayout.");
                }

                view.AddView(Label);
            }

            public override void RemoveAsSubview()
            {
                //Label.RemoveFromSuperview();
                ((LinearLayout)Label.RootView).RemoveView(Label);
            }

            public override void SizeToFit()
            {
                Rect bounds = new Rect();
                Label.Paint.GetTextBounds(Label.Text, 0, Label.Text.Length, bounds);

                // add an extra space at the end to compensate for what iOS does. 
                Label.LayoutParameters.Width = Math.Min(bounds.Width(), Label.LayoutParameters.Width);

                int numRows = 0;
                int index = 0;

                int length = Label.Text.Length;
                while(index < length - 1)
                {
                    // 
                    index += Label.Paint.BreakText(Label.Text.ToCharArray(), index, length - index, Label.LayoutParameters.Width, null);

                    numRows++;
                }

                Label.LayoutParameters.Height = (numRows + 2) * bounds.Height();
                    
            }
        }
    }
}
#endif
