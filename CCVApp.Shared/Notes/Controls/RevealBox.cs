using System;
using System.Xml;
using System.Drawing;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Notes.Model;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// A text label that is hidden until a user taps on it.
            /// </summary>
            public class RevealBox : NoteText
            {
                /// <summary>
                /// True when the text is revealed, false when it isn't.
                /// We can't just rely on the fade value because it's a bit different across android / ios.
                /// </summary>
                /// <value><c>true</c> if revealed; otherwise, <c>false</c>.</value>
                public bool Revealed { get; protected set; }

                public string Text { get { return PlatformLabel != null ? PlatformLabel.Text : ""; } }

                public RevealBox( CreateParams parentParams, XmlReader reader ) : base( )
                {
                    // don't call the base constructor that reads. we'll do the reading here.
                    base.Initialize( );

                    Revealed = false;

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

                public override bool TouchesEnded( PointF touch )
                {
                    // create an expanded bounding box and see if the touch fell within that.
                    // we expand the height by 50% in both directions
                    RectangleF boundingBox = new RectangleF( PlatformLabel.Frame.Left, 
                                                             PlatformLabel.Frame.Top - (PlatformLabel.Bounds.Height / 2), 
                                                             PlatformLabel.Bounds.Right, 
                                                             PlatformLabel.Bounds.Bottom + PlatformLabel.Bounds.Height );
                    if( boundingBox.Contains( touch ) )
                    {
                        Revealed = !Revealed;

                        float targetFade = 1.0f - PlatformLabel.GetFade( );

                        PlatformLabel.AnimateToFade( targetFade );
                        return true;
                    }

                    return false;
                }

                public NoteState.RevealBoxState GetState( )
                {
                    return new NoteState.RevealBoxState( PlatformLabel.Text, Revealed );
                }

                public void SetRevealed( bool revealed )
                {
                    Revealed = revealed;
                    PlatformLabel.SetFade( revealed == true ? 1.0f : 0.0f );
                }
            }
        }
    }
}
