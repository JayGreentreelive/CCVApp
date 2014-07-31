using System;
using System.Drawing;

namespace Notes
{
    namespace PlatformUI
    {
        // put common utility things here (enums, etc)
        public enum TextAlignment
        {
            Left,
            Center,
            Right,
            Justified,
            Natural
        }

        /// <summary>
        /// The base platformUI that provides an interface to platform specific UI controls.
        /// </summary>
        public abstract class PlatformBaseUI
        {
            #if __ANDROID__
            public static Android.Graphics.Color GetUIColor( uint color )
            {
                // break out the colors as 255 components for android
                return new Android.Graphics.Color(
                    ( byte )( ( color & 0xFF000000 ) >> 24 ),
                    ( byte )( ( color & 0x00FF0000 ) >> 16 ), 
                    ( byte )( ( color & 0x0000FF00 ) >> 8 ), 
                    ( byte )( ( color & 0x000000FF ) ) );
            }
            #endif

            #if __IOS__
            public static MonoTouch.UIKit.UIColor GetUIColor( uint color )
            {
                // break out the colors and convert to 0-1 for iOS
                return new MonoTouch.UIKit.UIColor(
                    ( float )( ( color & 0xFF000000 ) >> 24 ) / 255,
                    ( float )( ( color & 0x00FF0000 ) >> 16 ) / 255, 
                    ( float )( ( color & 0x0000FF00 ) >> 8 ) / 255, 
                    ( float )( ( color & 0x000000FF ) ) / 255 );
            }
            #endif

            // Properties
            public abstract void SetFont( string fontName, float fontSize );

            public uint BackgroundColor
            {
                set { setBackgroundColor( value ); }
            }

            protected abstract void setBackgroundColor( uint backgroundColor );

            public float Opacity
            {
                get { return getOpacity( ); }
                set { setOpacity( value ); }
            }

            protected abstract float getOpacity( );

            protected abstract void setOpacity( float opacity );

            public float ZPosition
            {
                get { return getZPosition( ); }
                set { setZPosition( value ); }
            }

            protected abstract float getZPosition( );

            protected abstract void setZPosition( float zPosition );

            public RectangleF Bounds
            {
                get { return getBounds( ); }
                set { setBounds( value ); }
            }

            protected abstract RectangleF getBounds( );

            protected abstract void setBounds( RectangleF bounds );

            public RectangleF Frame
            {
                get { return getFrame( ); }
                set { setFrame( value ); }
            }

            protected abstract RectangleF getFrame( );

            protected abstract void setFrame( RectangleF frame );

            public PointF Position
            {
                get { return getPosition( ); }
                set { setPosition( value ); }
            }

            protected abstract PointF getPosition( );

            protected abstract void setPosition( PointF position );

            public uint TextColor
            {
                //get { return getTextColor(); }
                set { setTextColor( value ); }
            }
            //protected abstract uint getTextColor();
            protected abstract void setTextColor( uint color );

            public string Text
            {
                get { return getText( ); }
                set { setText( value ); }
            }

            protected abstract string getText( );

            protected abstract void setText( string text );

            public TextAlignment TextAlignment
            {
                get { return getTextAlignment( ); }
                set { setTextAlignment( value ); }
            }

            protected abstract TextAlignment getTextAlignment( );

            protected abstract void setTextAlignment( TextAlignment alignment );


            public abstract void AddAsSubview( object masterView );

            public abstract void RemoveAsSubview( object masterView );

            public abstract void SizeToFit( );
        }
    }
}

