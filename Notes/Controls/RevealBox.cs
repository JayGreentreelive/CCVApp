using System;
using System.Xml;
using System.Drawing;

namespace Notes
{
    public class RevealBox : NoteText
    {
        protected string HiddenText { get; set; }

        public RevealBox( CreateParams parentParams, XmlReader reader ) : base( )
        {
            // don't call the base constructor that reads. we'll do the reading here.

            Initialize( );


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

            // get text
            string result = reader.GetAttribute( "Text" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                HiddenText = result;
            }

            // parse the rest of the stream
            if( reader.IsEmptyElement == false )
            {
                bool finishedLabel = false;
                while( finishedLabel == false && reader.Read( ) )
                {
                    switch( reader.NodeType )
                    {
                        case XmlNodeType.Element:
                            {
                                switch( reader.Name )
                                {
                                    case "Text":
                                        {
                                            HiddenText = reader.ReadElementContentAsString( );
                                            break;
                                        }
                                }
                                break;
                            }

                        case XmlNodeType.Text:
                            {
                                // support text as embedded in the element
                                HiddenText = reader.Value.Replace( System.Environment.NewLine, "" );

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

            // before applying the hidden text, measure and see what length we should be using.
            // This is totally hacky / temp till we get a real reveal affect in.
            if( HiddenText.Length > "[TOUCH]".Length )
            {
                SetText( HiddenText );
            }
            else
            {
                SetText( "[TOUCH]" );
            }
            PlatformLabel.Text = "[TOUCH]";

            PlatformLabel.Position = new PointF( bounds.X, bounds.Y );
        }

        public override void TouchesEnded( PointF touch )
        {
            // if the touch that was released was in our region, toggle the text
            PointF point = new PointF( touch.X - PlatformLabel.Position.X, touch.Y - PlatformLabel.Position.Y );

            //TODO: We can expand this to a radius to make it easier to tap.
			
            if( PlatformLabel.Bounds.Contains( point ) )
            {
                SetText( HiddenText );
            }
        }
    }
}
