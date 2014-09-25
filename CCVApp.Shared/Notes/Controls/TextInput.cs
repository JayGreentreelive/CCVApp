using System;
using System.Xml;
using System.Drawing;
using Rock.Mobile.PlatformUI;
using System.Collections.Generic;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// A label displaying Placeholder text. When tapped, allows
            /// a user to enter text via keyboard.
            /// </summary>
            public class TextInput : BaseControl
            {
                /// <summary>
                /// Actual textfield object.
                /// </summary>
                /// <value>The text field.</value>
                protected PlatformTextField TextField { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    TextField = PlatformTextField.Create( );
                }

                public TextInput( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mTextInput );

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
                    TextField.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    TextField.TextColor = mStyle.mFont.mColor.Value;
                   
                    // check for border styling
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        TextField.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        TextField.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        TextField.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    if( mStyle.mTextInputBackgroundColor.HasValue )
                    {
                        TextField.BackgroundColor = mStyle.mTextInputBackgroundColor.Value;
                    }
                    else
                    {
                        if( mStyle.mBackgroundColor.HasValue )
                        {
                            TextField.BackgroundColor = mStyle.mBackgroundColor.Value;
                        }
                    }

                   
                    // set the dimensions and position
                    TextField.Bounds = bounds;
                    TextField.Placeholder = " ";

                    // get the hint text if it's as an attribute
                    string result = reader.GetAttribute( "PlaceHolder" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        TextField.Placeholder = result;
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
                                        case "PlaceHolder":
                                        {
                                            TextField.Placeholder = reader.ReadElementContentAsString( );
                                            break;
                                        }
                                    }
                                    break;
                                }

                                case XmlNodeType.EndElement:
                                {
                                    // if we hit the end of our label, we're done.
                                    if( reader.Name == "TextInput" )
                                    {
                                        finishedLabel = true;
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    // adjust the text according to the style
                    switch( mStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            TextField.Placeholder = TextField.Placeholder.ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            TextField.Placeholder = TextField.Placeholder.ToLower( );
                            break;
                        }
                    }

                    // size to fit to calculate the height, then reset our width with that height.
                    TextField.SizeToFit( );
                    TextField.Frame = new RectangleF( bounds.X, bounds.Y, bounds.Width, TextField.Bounds.Height );

                    // set the color of the hint text
                    TextField.PlaceholderTextColor = mStyle.mFont.mColor.Value;
                }

                public override bool TouchesEnded( PointF touch )
                {
                    // hide the keyboard
                    TextField.ResignFirstResponder( );

                    return false;
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    TextField.Position = new PointF( TextField.Position.X + xOffset, 
                        TextField.Position.Y + yOffset );
                }

                public override void AddToView( object obj )
                {
                    TextField.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    TextField.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    htmlStream += TextField.Text;
                }
                public override PlatformBaseUI GetPlatformControl()
                {
                    return TextField;
                }

                public override RectangleF GetFrame( )
                {
                    return TextField.Frame;
                }
            }
        }
    }
}
