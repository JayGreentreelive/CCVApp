using System;
using Rock.Mobile.PlatformUI;
using System.Xml;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Contains a text label that displays a quote, as well as a text label
            /// with the quote's citation.
            /// Assumes a layout or quote and that cannot be overridden.
            /// </summary>
            public class Quote : BaseControl
            {
                /// <summary>
                /// The quote to display
                /// </summary>
                /// <value>The quote label.</value>
                protected PlatformLabel QuoteLabel { get; set; }

                /// <summary>
                /// The entity being cited.
                /// </summary>
                /// <value>The citation.</value>
                protected PlatformLabel Citation { get; set; }

                /// <summary>
                /// The bounds (including position) of the quote.
                /// </summary>
                /// <value>The bounds.</value>
                protected RectangleF Frame { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    QuoteLabel = PlatformLabel.Create( );
                    Citation = PlatformLabel.Create( );
                }

                public Quote( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    base.ParseCommonAttribs( reader, ref bounds );

                    // take our parent's style but override with anything we set
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mQuote );


                    // create the font that either we or our parent defined
                    QuoteLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    Citation.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );

                    QuoteLabel.TextColor = mStyle.mFont.mColor.Value;
                    Citation.TextColor = mStyle.mFont.mColor.Value;

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        QuoteLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
                        Citation.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }


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

                    //WIDTH
                    if( bounds.Width == 0 )
                    {
                        // if 0, just take the our parents width
                        bounds.Width = Math.Max( 1, parentParams.Width - bounds.X );
                    }
                    // if < 1 it's a percent and we should convert
                    else if( bounds.Width <= 1 )
                    {
                        bounds.Width = Math.Max( 1, parentParams.Width - bounds.X ) * bounds.Width;
                    }

                    // PADDING
                    float leftPadding = Styles.Style.GetStyleValue( mStyle.mPaddingLeft, parentParams.Width );
                    float rightPadding = Styles.Style.GetStyleValue( mStyle.mPaddingRight, parentParams.Width );
                    float topPadding = Styles.Style.GetStyleValue( mStyle.mPaddingTop, parentParams.Height );
                    float bottomPadding = Styles.Style.GetStyleValue( mStyle.mPaddingBottom, parentParams.Height );

                    // now calculate the available width based on padding. (Don't actually change our width)
                    float availableWidth = bounds.Width - leftPadding - rightPadding;

                    // Set the bounds and position for the frame (except Height, which we'll calculate based on the text)
                    QuoteLabel.Frame = new RectangleF( bounds.X + leftPadding, bounds.Y + topPadding, availableWidth, 0 );


                    // expect the citation to be an attribute
                    string result = reader.GetAttribute( "Citation" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        // set and resize the citation to fit
                        Citation.Text = result;
                        Citation.Bounds = bounds;
                        Citation.SizeToFit( );
                    }

                    bool finishedScripture = false;
                    while( finishedScripture == false && reader.Read( ) )
                    {
                        switch( reader.NodeType )
                        {
                            case XmlNodeType.Text:
                            {
                                // grab the text. remove any weird characters
                                string text = Regex.Replace( reader.Value, @"\t|\n|\r", "" );

                                // now break it into words so we can do word wrapping
                                string[] words = text.Split( ' ' );
                                foreach( string word in words )
                                {
                                    // create labels out of each one
                                    if( string.IsNullOrEmpty( word ) == false )
                                    {
                                        QuoteLabel.Text += word + " ";
                                    }
                                }

                                break;
                            }


                            case XmlNodeType.EndElement:
                            {
                                // if we hit the end of our label, we're done.
                                if( reader.Name == "Quote" )
                                {
                                    finishedScripture = true;
                                }

                                break;
                            }
                        }
                    }

                    // adjust the text
                    switch( mStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            QuoteLabel.Text = QuoteLabel.Text.ToUpper( );
                            Citation.Text = Citation.Text.ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            QuoteLabel.Text = QuoteLabel.Text.ToLower( );
                            Citation.Text = Citation.Text.ToLower( );
                            break;
                        }
                    }

                    // We forced the text to fit within the width specified above, so this will simply calculate the height.
                    QuoteLabel.SizeToFit( );

                    // now that we know our text size, we can adjust the citation
                    Citation.Frame = new RectangleF( QuoteLabel.Frame.Left, 
                        QuoteLabel.Frame.Bottom, 
                        QuoteLabel.Frame.Width,
                        Citation.Frame.Height );


                    Citation.TextAlignment = TextAlignment.Right;


                    // get a bounding frame for the quote and citation
                    RectangleF frame = Parser.CalcBoundingFrame( QuoteLabel.Frame, Citation.Frame );

                    // reintroduce vertical padding
                    bounds.Height = frame.Height + topPadding + bottomPadding;

                    // and store that as our bounds
                    Frame = bounds;
                    base.DebugFrameView.Frame = Frame;
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    QuoteLabel.Position = new PointF( QuoteLabel.Position.X + xOffset, 
                        QuoteLabel.Position.Y + yOffset );

                    Citation.Position = new PointF( Citation.Position.X + xOffset, 
                        Citation.Position.Y + yOffset );

                    // update our bounds by the new offsets.
                    Frame = new RectangleF( Frame.X + xOffset, Frame.Y + yOffset, Frame.Width, Frame.Height );
                    base.DebugFrameView.Frame = Frame;
                }

                public override void AddToView( object obj )
                {
                    QuoteLabel.AddAsSubview( obj );
                    Citation.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    QuoteLabel.RemoveAsSubview( obj );
                    Citation.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                public override RectangleF GetFrame( )
                {
                    return Frame;
                }
            }
        }
    }
}
