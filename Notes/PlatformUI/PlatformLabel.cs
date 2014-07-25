using System;
using System.Drawing;

namespace Notes
{
    namespace PlatformUI
    {   
        public abstract class PlatformLabel
        {
            public static PlatformLabel Create()
            {
                #if __IOS__
                return new iOSLabel();
                #endif
            }

            // Properties
            public abstract void SetFont(string fontName, float fontSize);

            public uint BackgroundColor
            {
                set { setBackgroundColor(value); }
            }
            protected abstract void setBackgroundColor(uint backgroundColor);

            public float Opacity
            {
                get { return getOpacity(); }
                set { setOpacity(value); }
            }
            protected abstract float getOpacity();
            protected abstract void setOpacity(float opacity);

            public float ZPosition
            {
                get { return getZPosition(); }
                set { setZPosition(value); }
            }
            protected abstract float getZPosition();
            protected abstract void setZPosition(float zPosition);

            public RectangleF Bounds
            {
                get { return getBounds(); }
                set { setBounds(value); }
            }
            protected abstract RectangleF getBounds();
            protected abstract void setBounds(RectangleF bounds);

            public RectangleF Frame
            {
                get { return getFrame(); }
                set { setFrame(value); }
            }
            protected abstract RectangleF getFrame();
            protected abstract void setFrame(RectangleF frame);

            public PointF Position
            {
                get { return getPosition(); }
                set { setPosition(value); }
            }
            protected abstract PointF getPosition();
            protected abstract void setPosition(PointF position);

            public uint TextColor
            {
                //get { return getTextColor(); }
                set { setTextColor(value); }
            }
            //protected abstract uint getTextColor();
            protected abstract void setTextColor(uint color);

            public string Text
            {
                get { return getText(); }
                set { setText(value); }
            }
            protected abstract string getText();
            protected abstract void setText(string text);

            public TextAlignment TextAlignment
            {
                get { return getTextAlignment(); }
                set { setTextAlignment(value); }
            }
            protected abstract TextAlignment getTextAlignment();
            protected abstract void setTextAlignment(TextAlignment alignment);


            public abstract void AddAsSubview(object masterView);
            public abstract void RemoveAsSubview();

            public abstract void SizeToFit();
        }
    }
}

