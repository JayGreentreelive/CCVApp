#if __IOS__
using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Notes
{
    namespace PlatformUI
    {
        public class iOSTextField : PlatformTextField
        {
            UITextField TextField { get; set; }

            public iOSTextField( )
            {
                TextField = new UITextField( );
                TextField.Layer.AnchorPoint = new PointF( 0, 0 );
                TextField.TextAlignment = UITextAlignment.Left;
            }

            // Properties
            public override void SetFont( string fontName, float fontSize )
            {
                TextField.Font = UIFont.FromName( fontName, fontSize );
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                TextField.Layer.BackgroundColor = PlatformCommonUI.GetUIColor( backgroundColor ).CGColor;
            }

            protected override float getOpacity( )
            {
                return TextField.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                TextField.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return TextField.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                TextField.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return TextField.Bounds;
            }

            protected override void setBounds( RectangleF bounds )
            {
                TextField.Bounds = bounds;
            }

            protected override RectangleF getFrame( )
            {
                return TextField.Frame;
            }

            protected override void setFrame( RectangleF frame )
            {
                TextField.Frame = frame;
            }

            protected override  PointF getPosition( )
            {
                return TextField.Layer.Position;
            }

            protected override void setPosition( PointF position )
            {
                TextField.Layer.Position = position;
            }

            protected override void setTextColor( uint color )
            {
                TextField.TextColor = PlatformCommonUI.GetUIColor( color );
            }

            protected override void setPlaceholderTextColor( uint color )
            {
                TextField.AttributedPlaceholder = new NSAttributedString(
                    TextField.Placeholder,
                    font: TextField.Font,
                    foregroundColor: PlatformCommonUI.GetUIColor( color )
                );
            }

            protected override string getText( )
            {
                return TextField.Text;
            }

            protected override void setText( string text )
            {
                TextField.Text = text;
            }

            protected override TextAlignment getTextAlignment( )
            {
                return ( TextAlignment )TextField.TextAlignment;
            }

            protected override void setTextAlignment( TextAlignment alignment )
            {
                TextField.TextAlignment = ( UITextAlignment )alignment;
            }

            protected override string getPlaceholder( )
            {
                return TextField.Placeholder;
            }

            protected override void setPlaceholder( string placeholder )
            {
                TextField.Placeholder = placeholder;
            }

            public override void ResignFirstResponder( )
            {
                TextField.ResignFirstResponder( );
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new InvalidCastException( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                view.AddSubview( TextField );
            }

            public override void RemoveAsSubview( object obj )
            {
                // Obj is only needed by Android, so we ignore it
                TextField.RemoveFromSuperview( );
            }

            public override void SizeToFit( )
            {
                TextField.SizeToFit( );
            }
        }
    }
}
#endif
