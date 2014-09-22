using System;
using Rock.Mobile.PlatformUI;
using System.Xml;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
                /// The view representing any surrounding border for the quote.
                /// </summary>
                /// <value>The border view.</value>
                protected PlatformView BorderView { get; set; }

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
                    BorderView = PlatformView.Create( );
                }

                public Quote( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mQuote );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    SizeF parentSize = new SizeF( parentParams.Width, parentParams.Height );
                    ParseCommonAttribs( reader, ref parentSize, ref bounds );

                    // Get margins and padding
                    RectangleF padding;
                    RectangleF margin;
                    GetMarginsAndPadding( ref mStyle, ref parentSize, ref bounds, out margin, out padding );

                    // apply margins to as much of the bounds as we can (bottom must be done by our parent container)
                    ApplyImmediateMargins( ref bounds, ref margin, ref parentSize );
                    Margin = margin;


                    // create the font that either we or our parent defined
                    QuoteLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    Citation.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );

                    QuoteLabel.TextColor = mStyle.mFont.mColor.Value;
                    Citation.TextColor = mStyle.mFont.mColor.Value;

                    Citation.BackgroundColor = 0;
                    QuoteLabel.BackgroundColor = 0;


                    // check for border styling
                    int borderPaddingPx = 0;
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        BorderView.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        BorderView.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        BorderView.BorderWidth = mStyle.mBorderWidth.Value;
                        borderPaddingPx = (int)Rock.Mobile.PlatformUI.PlatformBaseUI.UnitToPx( mStyle.mBorderWidth.Value + CCVApp.Shared.Config.Note.BorderPadding );
                    }

                    if( mStyle.mTextInputBackgroundColor.HasValue )
                    {
                        BorderView.BackgroundColor = mStyle.mTextInputBackgroundColor.Value;
                    }
                    else
                    {
                        if( mStyle.mBackgroundColor.HasValue )
                        {
                            BorderView.BackgroundColor = mStyle.mBackgroundColor.Value;
                        }
                    }

                    // now calculate the available width based on padding. (Don't actually change our width)
                    float availableWidth = bounds.Width - padding.Left - padding.Width - (borderPaddingPx * 2);

                    // Set the bounds and position for the frame (except Height, which we'll calculate based on the text)
                    QuoteLabel.Frame = new RectangleF( bounds.X + padding.Left + borderPaddingPx, bounds.Y + padding.Top + borderPaddingPx, availableWidth, 0 );


                    // expect the citation to be an attribute
                    string result = reader.GetAttribute( "Citation" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        // set and resize the citation to fit
                        Citation.Text = result;
                        Citation.Bounds = new RectangleF( bounds.X + padding.Left + borderPaddingPx, bounds.Y + padding.Top + borderPaddingPx, availableWidth, 0 );
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
                    // for citation width, attempt to use quote width, but if there was no quote text,
                    // the width will be 0, so we'll fallback to the citation width.

                    RectangleF frame;
                    if( string.IsNullOrEmpty( QuoteLabel.Text ) != true )
                    {   
                        // when taking the citation frame, use whichever width is larger,
                        // because it's certainly possible the quote is shorter than the citation.
                        Citation.Frame = new RectangleF( QuoteLabel.Frame.Left, 
                                                         QuoteLabel.Frame.Bottom, 
                                                         Math.Max( Citation.Frame.Width, QuoteLabel.Frame.Width ),
                                                         Citation.Frame.Height );

                        // get a bounding frame for the quote and citation
                        frame = Parser.CalcBoundingFrame( QuoteLabel.Frame, Citation.Frame );
                    }
                    else
                    {
                        Citation.Frame = new RectangleF( QuoteLabel.Frame.Left, 
                                                         QuoteLabel.Frame.Top, 
                                                         Citation.Frame.Width,
                                                         Citation.Frame.Height );

                        // get a bounding frame for the quote and citation
                        frame = Citation.Frame;
                    }

                    Citation.TextAlignment = TextAlignment.Right;

                    // reintroduce vertical padding
                    frame.Height = frame.Height + padding.Top + padding.Height;

                    // setup our bounding rect for the border

                    // because we're basing it off of the largest control (quote or citation),
                    // we need to reintroduce the border padding.
                    frame = new RectangleF( frame.X - borderPaddingPx, 
                                            frame.Y - borderPaddingPx, 
                                            frame.Width + borderPaddingPx * 2, 
                                            frame.Height + borderPaddingPx * 2 );

                    // and store that as our bounds
                    BorderView.Frame = frame;
                    Frame = frame;
                    SetDebugFrame( Frame );
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    QuoteLabel.Position = new PointF( QuoteLabel.Position.X + xOffset, 
                        QuoteLabel.Position.Y + yOffset );

                    Citation.Position = new PointF( Citation.Position.X + xOffset, 
                        Citation.Position.Y + yOffset );

                    BorderView.Position = new PointF( BorderView.Position.X + xOffset,
                        BorderView.Position.Y + yOffset );

                    // update our bounds by the new offsets.
                    Frame = new RectangleF( Frame.X + xOffset, Frame.Y + yOffset, Frame.Width, Frame.Height );
                }

                public override void AddToView( object obj )
                {
                    BorderView.AddAsSubview( obj );
                    QuoteLabel.AddAsSubview( obj );
                    Citation.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    BorderView.RemoveAsSubview( obj );
                    QuoteLabel.RemoveAsSubview( obj );
                    Citation.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                public override void GetNotesForEmail( List<PlatformBaseUI> controlList )
                {
                    controlList.Add( QuoteLabel );
                    controlList.Add( Citation );
                }

                public override RectangleF GetFrame( )
                {
                    return Frame;
                }
            }
        }
    }
}
