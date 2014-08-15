using System;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using RockMobile.PlatformUI;

namespace Notes
{
    /// <summary>
    /// Control that lays out basic text. Used by Paragraphs.
    /// </summary>
    public class NoteText : BaseControl
    {
        /// <summary>
        /// Actual text label.
        /// </summary>
        /// <value>The platform label.</value>
        protected PlatformLabel PlatformLabel { get; set; }

        protected NoteText( )
        {
        }

        public NoteText( CreateParams parentParams, string text )
        {
            base.Initialize( );

            PlatformLabel = PlatformLabel.Create( );

            // check for attributes we support
            RectangleF bounds = new RectangleF( );

            // take our parent's style, and for anything not set by them use the default.
            mStyle = parentParams.Style;
            Styles.Style.MergeStyleAttributesWithDefaults( ref mStyle, ref ControlStyles.mText );

            PlatformLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
            PlatformLabel.TextColor = mStyle.mFont.mColor.Value;

            if( mStyle.mBackgroundColor.HasValue )
            {
                PlatformLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
            }

            // set the dimensions and position
            if( bounds.Width == 0 )
            {
                // always take the available width, in case this control
                // is specified to be offset relative to its parent
                bounds.Width = parentParams.Width - bounds.X;
            }
            PlatformLabel.Bounds = bounds;

            // get text
            SetText( text );

            // position ourselves in absolute coordinates, and trust our parent to offset us to be relative to them.
            PlatformLabel.Position = new PointF( bounds.X, bounds.Y );
        }

        public void SetText( string text )
        {
            switch( mStyle.mTextCase )
            {
                case Styles.TextCase.Upper:
                {
                    PlatformLabel.Text = text.ToUpper( );
                    break;
                }

                case Styles.TextCase.Lower:
                {
                    PlatformLabel.Text = text.ToLower( );
                    break;
                }

                case Styles.TextCase.Normal:
                {
                    PlatformLabel.Text = text;
                    break;
                }
            }

            // resize the label to fit the text
            PlatformLabel.SizeToFit( );
        }

        public override void AddOffset( float xOffset, float yOffset )
        {
            base.AddOffset( xOffset, yOffset );

            PlatformLabel.Position = new PointF( PlatformLabel.Position.X + xOffset, 
                PlatformLabel.Position.Y + yOffset );
        }

        public override void AddToView( object obj )
        {
            PlatformLabel.AddAsSubview( obj );

            TryAddDebugLayer( obj );
        }

        public override void RemoveFromView( object obj )
        {
            PlatformLabel.RemoveAsSubview( obj );

            TryRemoveDebugLayer( obj );
        }

        public override RectangleF GetFrame( )
        {
            return PlatformLabel.Frame;
        }
    }
}
