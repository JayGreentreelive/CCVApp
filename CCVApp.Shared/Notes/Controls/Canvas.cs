using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;

using CCVApp.Shared.Notes.Styles;
using Rock.Mobile.PlatformUI;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// UI Control that allows absolute layout of children relative to the Canvas' X/Y position.
            /// </summary>
            public class Canvas : BaseControl
            {
                /// <summary>
                /// List of owned and managed child controls.
                /// </summary>
                /// <value>The child controls.</value>
                protected List<IUIControl> ChildControls { get; set; }

                /// <summary>
                /// The view representing any surrounding border for the canvas.
                /// </summary>
                /// <value>The border view.</value>
                protected PlatformView BorderView { get; set; }

                /// <summary>
                /// The bounds (including position) of this control.
                /// </summary>
                /// <value>The bounds.</value>
                protected RectangleF Frame { get; set; }

                /// <summary>
                /// The alignment that child controls should take within this control.
                /// </summary>
                /// <value>The child horz alignment.</value>
                protected Alignment ChildHorzAlignment { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    ChildControls = new List<IUIControl>( );

                    ChildHorzAlignment = Alignment.Inherit;

                    BorderView = PlatformView.Create( );
                }

                public Canvas( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    ParseCommonAttribs( reader, ref bounds );

                    // take our parent's style but override with anything we set
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mCanvas );

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
                    //


                    // now read what our children's alignment should be
                    // check for alignment
                    string result = reader.GetAttribute( "ChildAlignment" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        switch( result )
                        {
                            case "Left":
                                ChildHorzAlignment = Alignment.Left;
                                break;
                            case "Right":
                                ChildHorzAlignment = Alignment.Right;
                                break;
                            case "Center":
                                ChildHorzAlignment = Alignment.Center;
                                break;
                            default:
                                ChildHorzAlignment = mStyle.mAlignment.Value;
                                break;
                        }
                    }
                    else
                    {
                        // if it wasn't specified, use OUR alignment.
                        ChildHorzAlignment = mStyle.mAlignment.Value;
                    }

                    // LEFT/TOP POSITIONING
                    if( bounds.X < 1 )
                    {
                        // convert % to pixel, based on parent's width
                        bounds.X = parentParams.Width * bounds.X;
                    }

                    if( bounds.Y < 1 )
                    {
                        // convert % to pixel, based on parent's width
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
                    float availableWidth = bounds.Width - leftPadding - rightPadding - (borderPaddingPx * 2);


                    // Parse Child Controls
                    bool finishedParsing = false;
                    while( finishedParsing == false && reader.Read( ) )
                    {
                        switch( reader.NodeType )
                        {
                            case XmlNodeType.Element:
                                {
                                    // let each child have our available width.
                                    Style style = new Style( );
                                    style = mStyle;
                                    style.mAlignment = ChildHorzAlignment;
                                    IUIControl control = Parser.TryParseControl( new CreateParams( this, availableWidth, parentParams.Height, ref style ), reader );
                                    if( control != null )
                                    {
                                        ChildControls.Add( control );
                                    }
                                    break;
                                }

                            case XmlNodeType.EndElement:
                                {
                                    // if we hit the end of our label, we're done.
                                    if( reader.Name == "Canvas" )
                                    {
                                        finishedParsing = true;
                                    }

                                    break;
                                }
                        }
                    }


                    // layout all controls
                    float yOffset = bounds.Y + topPadding + borderPaddingPx; //vertically they should just stack
                    float height = 0;

                    // now we must center each control within the stack.
                    foreach( IUIControl control in ChildControls )
                    {
                        RectangleF controlFrame = control.GetFrame( );

                        // horizontally position the controls according to their 
                        // requested alignment
                        Alignment controlAlignment = control.GetHorzAlignment( );

                        // adjust by our position
                        float xAdjust = 0;
                        switch( controlAlignment )
                        {
                            case Alignment.Center:
                                xAdjust = bounds.X + ( ( availableWidth / 2 ) - ( controlFrame.Width / 2 ) );
                                break;
                            case Alignment.Right:
                                xAdjust = bounds.X + ( availableWidth - controlFrame.Width );
                                break;
                            case Alignment.Left:
                                xAdjust = bounds.X;
                                break;
                        }

                        // adjust the next sibling by yOffset
                        control.AddOffset( xAdjust + leftPadding + borderPaddingPx, yOffset );

                        // track the height of the grid by the control lowest control 
                        height = control.GetFrame( ).Bottom > height ? control.GetFrame( ).Bottom : height;
                    }

                    // we need to store our bounds. We cannot
                    // calculate them on the fly because we
                    // would lose any control defined offsets, which would throw everything off.
                    bounds.Height = height + bottomPadding + borderPaddingPx;

                    // setup our bounding rect for the border
                    bounds = new RectangleF( bounds.X, 
                                             bounds.Y,
                                             bounds.Width, 
                                             bounds.Height);

                    // and store that as our bounds
                    BorderView.Frame = bounds;

                    Frame = bounds;

                    // store our debug frame
                    SetDebugFrame( Frame );
                }

                public override bool TouchesEnded( PointF touch )
                {
                    // let each child handle it
                    foreach( IUIControl control in ChildControls )
                    {
                        // if a child consumes it, stop and report it was consumed.
                        if(control.TouchesEnded( touch ))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    // position each interactive label relative to ourselves
                    foreach( IUIControl control in ChildControls )
                    {
                        control.AddOffset( xOffset, yOffset );
                    }

                    BorderView.Position = new PointF( BorderView.Position.X + xOffset,
                                                      BorderView.Position.Y + yOffset );

                    // update our bounds by the new offsets.
                    Frame = new RectangleF( Frame.X + xOffset, Frame.Y + yOffset, Frame.Width, Frame.Height );
                }

                public override void AddToView( object obj )
                {
                    BorderView.AddAsSubview( obj );

                    // let each child do the same thing
                    foreach( IUIControl control in ChildControls )
                    {
                        control.AddToView( obj );
                    }

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    BorderView.RemoveAsSubview( obj );

                    // let each child do the same thing
                    foreach( IUIControl control in ChildControls )
                    {
                        control.RemoveFromView( obj );
                    }

                    TryRemoveDebugLayer( obj );
                }

                public override RectangleF GetFrame( )
                {
                    return Frame;
                }

                public override bool ShouldShowBulletPoint( )
                {
                    // as a container, it wouldn't really make sense to show a bullet point.
                    return false;
                }

                protected override List<IUIControl> GetChildControls( )
                {
                    return ChildControls;
                }
            }
        }
    }
}
