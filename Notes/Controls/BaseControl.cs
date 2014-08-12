using System;
using System.Drawing;
using System.Xml;
using Notes.Styles;
using Notes.PlatformUI;

namespace Notes
{
    /// <summary>
    /// Contains common properties and methods used by all UI Controls.
    /// </summary>
    public abstract class BaseControl : IUIControl
    {
        /// <summary>
        /// Layer used for the debug frame.
        /// </summary>
        protected PlatformLabel DebugFrameView { get; set; }

        /// <summary>
        /// If true, displays a debug frame around the bounds of the control.
        /// </summary>
        protected bool ShowDebugFrame { get; set; }

        /// <summary>
        /// Defines the style that this control should use.
        /// </summary>
        protected Style mStyle;

        /// <summary>
        /// Used to pass creation params from parent to child.
        /// </summary>
        public class CreateParams
        {
            public float Height { get; set; }

            public float Width { get; set; }

            public Style Style { get; set; }

            public CreateParams( float width, float height, ref Style style )
            {
                Height = height;
                Width = width;
                Style = style;
            }
        }

        protected virtual void Initialize( )
        {
            mStyle = new Style( );
            mStyle.Initialize( );
            mStyle.mAlignment = Alignment.Inherit;

            //Debugging - show the grid frames
            DebugFrameView = PlatformLabel.Create( );
            DebugFrameView.Opacity = .50f;
            DebugFrameView.BackgroundColor = 0x0000FFFF;
            DebugFrameView.ZPosition = 100;
            //Debugging
        }

        public BaseControl( )
        {
        }

        protected virtual void ParseCommonAttribs( XmlReader reader, ref RectangleF bounds )
        {
            // check for positioning attribs
            Parser.ParseBounds( reader, ref bounds );

            // check for a debug frame
            string result = reader.GetAttribute( "Debug" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                ShowDebugFrame = bool.Parse( result );
            }
            else
            {
                ShowDebugFrame = false;
            }
        }

        public virtual void AddOffset( float xOffset, float yOffset )
        {
            if( ShowDebugFrame )
            {
                DebugFrameView.Position = new PointF( DebugFrameView.Position.X + xOffset, 
                    DebugFrameView.Position.Y + yOffset );
            }
        }

        public virtual void AddToView( object obj )
        {
        }

        public virtual void RemoveFromView( object obj )
        {
        }

        public void TryAddDebugLayer( object obj )
        {
            // call this at the _end_ so it is the highest level 
            // view on Android.
            if( ShowDebugFrame )
            {
                DebugFrameView.AddAsSubview( obj );
            }
        }

        public void TryRemoveDebugLayer( object obj )
        {
            // call this at the _end_ so it is the highest level 
            // view on Android.
            if( ShowDebugFrame )
            {
                DebugFrameView.RemoveAsSubview( obj );
            }
        }

        public virtual bool TouchesBegan( PointF touch )
        {
            return false;
        }

        public virtual void TouchesMoved( PointF touch )
        {
        }

        public virtual bool TouchesEnded( PointF touch )
        {
            return false;
        }

        public Alignment GetHorzAlignment( )
        {
            return mStyle.mAlignment.Value;
        }

        public virtual RectangleF GetFrame( )
        {
            return new RectangleF( );
        }

        public virtual bool ShouldShowBulletPoint( )
        {
            //the default behavior is that we should want a bullet point
            return true;
        }
    }
}
    