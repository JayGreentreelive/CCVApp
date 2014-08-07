﻿using System;
using System.Xml;
using System.Drawing;
using Notes.PlatformUI;

namespace Notes
{
    /// <summary>
    /// A text label that is hidden until a user taps on it.
    /// </summary>
    public class RevealBox : NoteText
    {
        public RevealBox( CreateParams parentParams, XmlReader reader ) : base( )
        {
            // don't call the base constructor that reads. we'll do the reading here.
            base.Initialize( );

            PlatformLabel = PlatformLabel.CreateRevealLabel( );

            PlatformLabel.SetFade( 0.0f );

            // check for attributes we support
            RectangleF bounds = new RectangleF( );
            ParseCommonAttribs( reader, ref bounds );

            // if our left position is requested as a %, then that needs to be % of parent width
            if( bounds.X < 1 )
            {
                bounds.X = parentParams.Width * bounds.X;
            }

            // if our top position is requested as a %, then that needs to be % of parent width
            if( bounds.Y < 1 )
            {
                bounds.Y = parentParams.Height * bounds.Y;
            }


            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mRevealBox );

            // create the font that either we or our parent defined
            PlatformLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
            PlatformLabel.TextColor = mStyle.mFont.mColor.Value;

            if( mStyle.mBackgroundColor.HasValue )
            {
                PlatformLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
            }


            // parse the stream
            string revealText = "";
            if( reader.IsEmptyElement == false )
            {
                bool finishedLabel = false;
                while( finishedLabel == false && reader.Read( ) )
                {
                    switch( reader.NodeType )
                    {
                        case XmlNodeType.Text:
                        {
                            // support text as embedded in the element
                            revealText = reader.Value.Replace( System.Environment.NewLine, "" );

                            break;
                        }

                        case XmlNodeType.EndElement:
                        {
                            // if we hit the end of our label, we're done.
                            if( reader.Name == "RevealBox" )
                            {
                                finishedLabel = true;
                            }

                            break;
                        }
                    }
                }
            }

            // adjust the text
            switch( mStyle.mTextCase )
            {
                case Styles.TextCase.Upper:
                {
                    revealText = revealText.ToUpper( );
                    break;
                }

                case Styles.TextCase.Lower:
                {
                    revealText = revealText.ToLower( );
                    break;
                }
            }

            SetText( revealText );

            PlatformLabel.Position = new PointF( bounds.X, bounds.Y );
        }

        public override void TouchesEnded( PointF touch )
        {
            // if the touch that was released was in our region, toggle the text
            PointF point = new PointF( touch.X - PlatformLabel.Position.X, touch.Y - PlatformLabel.Position.Y );

            //TODO: We can expand this to a radius to make it easier to tap.
			
            if( PlatformLabel.Bounds.Contains( point ) )
            {
                //SetText( HiddenText );
                PlatformLabel.AnimateToFade( 1.0f );
            }
        }
    }
}
