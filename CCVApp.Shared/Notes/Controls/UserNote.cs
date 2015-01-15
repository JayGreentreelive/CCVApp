using System;
using System.Xml;
using System.Drawing;
using Rock.Mobile.PlatformUI;
using System.Threading;
using System.Collections.Generic;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;

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
            public class UserNote : BaseControl
            {
                /// <summary>
                /// Actual textfield object.
                /// </summary>
                /// <value>The text field.</value>
                protected PlatformTextField TextField { get; set; }

                /// <summary>
                /// The view representing the note's "Anchor"
                /// </summary>
                protected PlatformLabel Anchor { get; set; }
                protected RectangleF AnchorFrame { get; set; }

                /// <summary>
                /// Delete button
                /// </summary>
                /// <value>The anchor.</value>
                protected PlatformLabel DeleteButton { get; set; }

                /// <summary>
                /// Tracks the movement of the note as a user repositions it.
                /// </summary>
                protected PointF TrackingLastPos { get; set; }

                /// <summary>
                /// True if the note was moved after being tapped.
                /// </summary>
                protected bool DidMoveNote { get; set; }

                /// <summary>
                /// The furthest on X a note is allowed to be moved.
                /// </summary>
                protected float MaxAllowedX { get; set; }

                /// <summary>
                /// The furthest on Y a note is allowed to be moved.
                /// </summary>
                protected float MaxAllowedY { get; set; }

                /// <summary>
                /// The maximum width of the note.
                /// </summary>
                protected float MaxNoteWidth { get; set; }

                /// <summary>
                /// The minimum width of the note.
                /// </summary>
                protected float MinNoteWidth { get; set; }

                /// <summary>
                /// The timer monitoring whether the user held long enough to
                /// enable deleting.
                /// </summary>
                /// <value>The delete timer.</value>
                protected System.Timers.Timer DeleteTimer { get; set; }

                /// <summary>
                /// Manages the state of the note. 
                /// None - means it isn't being interacted with
                /// Hold - The user is holding their finger on it
                /// Moving - The user is dragging it around
                /// Delete - The note should be deleted.
                /// </summary>
                public enum TouchState
                {
                    None,
                    Hold,
                    Moving,
                    Delete,
                };
                public TouchState State { get; set; }

                /// <summary>
                /// True when a note is eligible for delete. Tapping on it while this is true will delete it.
                /// </summary>
                /// <value><c>true</c> if delete enabled; otherwise, <c>false</c>.</value>
                bool DeleteEnabled { get; set; }

                /// <summary>
                /// The maximum you can be from an anchor to be considered touching it.
                /// </summary>
                /// <value>The anchor touch range.</value>
                float AnchorTouchMaxDist { get; set; }

                /// <summary>
                /// the width of the screen so we know
                /// what the remaining width is when moving the note around.
                /// </summary>
                /// <value>The width of the screen.</value>
                float ScreenWidth { get; set; }

                /// <summary>
                /// The value to scale the positions by to get percentage and back
                /// </summary>
                /// <value>The position scalar.</value>
                PointF PositionTransform { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    TextField = PlatformTextField.Create( );
                    Anchor = PlatformLabel.Create( );
                    DeleteButton = PlatformLabel.Create( );
                }

                public UserNote( BaseControl.CreateParams createParams, float deviceHeight, Model.NoteState.UserNoteContent userNoteContent )
                {
                    PositionTransform = new PointF( createParams.Width, createParams.Height );

                    PointF startPos = new PointF( userNoteContent.PositionPercX * PositionTransform.X, userNoteContent.PositionPercY * PositionTransform.Y );
                    Create( createParams, deviceHeight, startPos, userNoteContent.Text );

                    // new notes are open by default. So if we're restoring one that was closed,
                    // keep it closed.
                    if( userNoteContent.WasOpen == false )
                    {
                        CloseNote( );
                    }

                    // since we're restoring an existing user note,
                    // we want to turn off scaling so we can adjust the height 
                    // for all the text
                    TextField.ScaleHeightForText = false;

                    TextField.SizeToFit( );

                    // a small hack, but calling SizeToFit breaks
                    // the note width, so this will restore it.
                    ValidateBounds( );

                    // now we can turn it back on so that if they continue to edit,
                    // it will grow.
                    TextField.ScaleHeightForText = true;
                }

                public UserNote( CreateParams parentParams, float deviceHeight, PointF startPos )
                {
                    Create( parentParams, deviceHeight, startPos, null );
                }

                public void Create( CreateParams parentParams, float deviceHeight, PointF startPos, string startingText )
                {
                    Initialize( );

                    PositionTransform = new PointF( parentParams.Width, parentParams.Height );

                    //JHM 10-28-14 - Moved this to the bottom, and now just using double
                    // the anchor's width.
                    //magic number ratio that works well!
                    /*#if __IOS__
                    float heightTouchRatio = .048f;
                    #endif
                    #if __ANDROID__
                    float heightTouchRatio = .078f;
                    #endif

                    // the touch range differs based on various device sizes. It makes more sense
                    // to give a larger area for a tablet and a smaller area to a phone.
                    AnchorTouchMaxDist = deviceHeight * heightTouchRatio;
                    AnchorTouchMaxDist *= AnchorTouchMaxDist;*/

                    //setup our timer for allowing movement/
                    DeleteTimer = new System.Timers.Timer();
                    DeleteTimer.Interval = 1000;
                    DeleteTimer.Elapsed += DeleteTimerDidFire;
                    DeleteTimer.AutoReset = false;

                    // take our parent's style or in defaults
                    mStyle = parentParams.Style;
                    Styles.Style.MergeStyleAttributesWithDefaults( ref mStyle, ref ControlStyles.mUserNote );

                    // flag that we want this text field to grow as more text is added
                    TextField.ScaleHeightForText = true;
                    TextField.DynamicTextMaxHeight = NoteConfig.UserNote_MaxHeight;

                    // Setup the font
                    TextField.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    TextField.TextColor = mStyle.mFont.mColor.Value;
                    TextField.Placeholder = MessagesStrings.UserNote_Placeholder;
                    TextField.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
                    TextField.KeyboardAppearance = CCVApp.Shared.Config.GeneralConfig.iOSPlatformUIKeyboardAppearance;
                     
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

                    // Setup the anchor color
                    Anchor.Text = NoteConfig.UserNote_Icon;
                    Anchor.TextColor = NoteConfig.UserNote_IconColor;
                    Anchor.SetFont( ControlStylingConfig.Icon_Font_Primary, NoteConfig.UserNote_IconSize );
                    Anchor.SizeToFit();
                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        Anchor.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }
                    else
                    {
                        Anchor.BackgroundColor = 0;
                    }

                    // store the width of the screen so we know
                    // what the remaining width is when moving the note around.
                    ScreenWidth = parentParams.Width * .95f;

                    // Don't let the note's width be less than twice the anchor width. Any less
                    // and we end up with text clipping.
                    MinNoteWidth = (Anchor.Bounds.Width * 2);

                    // Dont let the note be any wider than the screen - twice the min width. This allows a little
                    // free play so it doesn't feel like the note is always attached to the right edge.
                    MaxNoteWidth = Math.Min( ScreenWidth - (MinNoteWidth * 2), (MinNoteWidth * 6) );

                    // set the allowed X/Y so we don't let the user move the note off-screen.
                    MaxAllowedX = ( ScreenWidth - MinNoteWidth );
                    MaxAllowedY = ( parentParams.Height - Anchor.Bounds.Height );

                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAllowedX - startPos.X ) );
                    TextField.Bounds = new RectangleF( 0, 0, width, 0 );


                    // setup the delete button
                    DeleteButton.Text = NoteConfig.UserNote_DeleteIcon;
                    DeleteButton.TextColor = NoteConfig.UserNote_DeleteIconColor;
                    DeleteButton.SetFont( ControlStylingConfig.Icon_Font_Primary, NoteConfig.UserNote_DeleteIconSize );
                    DeleteButton.Hidden = true;
                    DeleteButton.SizeToFit( );
                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        DeleteButton.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }
                    else
                    {
                        DeleteButton.BackgroundColor = 0;
                    }



                    // Setup the position
                    Anchor.Position = startPos;
                    AnchorFrame = Anchor.Frame;

                    AnchorTouchMaxDist = AnchorFrame.Width * 2;
                    AnchorTouchMaxDist *= AnchorTouchMaxDist;


                    // validate its bounds
                    ValidateBounds( );

                    // set the position for the delete button
                    DeleteButton.Position = new PointF( AnchorFrame.Left - DeleteButton.Bounds.Width / 2, 
                                                        AnchorFrame.Top - DeleteButton.Bounds.Height / 2 );

                    // set the actual note textfield relative to the anchor
                    TextField.Position = new PointF( AnchorFrame.Left, 
                                                     AnchorFrame.Bottom );

                    // set the starting text if it was provided
                    if( startingText != null )
                    {
                        TextField.Text = startingText;
                    }

                    SetDebugFrame( TextField.Frame );
                }

                public bool TouchInDeleteButtonRange( PointF touch )
                {
                    // create a vector from the note anchor's center to the touch
                    PointF labelToTouch = new PointF( touch.X - (DeleteButton.Frame.X + DeleteButton.Frame.Width / 2), 
                                                      touch.Y - (DeleteButton.Frame.Y + DeleteButton.Frame.Height / 2));

                    float distSquared = Rock.Mobile.Math.Util.MagnitudeSquared( labelToTouch );
                    if( distSquared < AnchorTouchMaxDist )
                    {
                        return true;
                    }

                    return false;
                }

                public bool TouchInAnchorRange( PointF touch )
                {
                    // create a vector from the note anchor's center to the touch
                    PointF labelToTouch = new PointF( touch.X - (AnchorFrame.X + AnchorFrame.Width / 2), 
                                                      touch.Y - (AnchorFrame.Y + AnchorFrame.Height / 2));

                    float distSquared = Rock.Mobile.Math.Util.MagnitudeSquared( labelToTouch );
                    if( distSquared < AnchorTouchMaxDist )
                    {
                        return true;
                    }

                    return false;
                }

                public bool HitTest( PointF touch )
                {
                    if( TouchInAnchorRange( touch ) )
                    {
                        return true;
                    }

                    return false;
                }

                public override bool TouchesBegan( PointF touch )
                {
                    // is a user wanting to interact?
                    bool consumed = false;

                    // if delete is enabled, see if they tapped within range of the delete button
                    if( DeleteEnabled )
                    {
                        if( TouchInDeleteButtonRange( touch ) )
                        {
                            // if they did, we consume this and bye bye note.
                            consumed = true;

                            State = TouchState.Delete;
                        }
                    }
                    // if the touch is in our region, begin tracking
                    else if( TouchInAnchorRange( touch ) )
                    {
                        consumed = true;

                        if( State == TouchState.None )
                        {
                            // Enter the hold state
                            State = TouchState.Hold;

                            // Store our starting touch and kick off our delete timer
                            TrackingLastPos = touch;
                            DeleteTimer.Start();
                            Console.WriteLine( "UserNote Hold" );
                        }
                    }

                    return consumed;
                }

                // By design, this will only be called on the UserNote that received a TouchesBegan IN ANCHOR RANGE.
                static float sMinDistForMove = 625;
                public override void TouchesMoved( PointF touch )
                {
                    // We would be in the hold state if this is the first TouchesMoved 
                    // after TouchesBegan.
                    if( DeleteEnabled == false )
                    {
                        // if we're moving, update by the amount we moved.
                        PointF delta = new PointF( touch.X - TrackingLastPos.X, touch.Y - TrackingLastPos.Y );

                        // if we're in the hold state, require a small amount of moving before committing to movement.
                        if( State == TouchState.Hold )
                        {
                            float magSquared = Rock.Mobile.Math.Util.MagnitudeSquared( delta );
                            if( magSquared > sMinDistForMove )
                            {
                                // stamp our position as the new starting position so we don't
                                // get a "pop" in movement.
                                TrackingLastPos = touch;

                                State = TouchState.Moving;
                                Console.WriteLine( "UserNote MOVING" );
                            }
                        }
                        else if( State == TouchState.Moving )
                        {
                            AddOffset( delta.X, delta.Y );

                            // stamp our position
                            TrackingLastPos = touch;
                        }
                    }
                }

                // By design, this will only be called on the UserNote that received a TouchesBegan IN ANCHOR RANGE.
                public override IUIControl TouchesEnded( PointF touch )
                {
                    bool consumed = false;

                    switch( State )
                    {
                        case TouchState.None:
                        {
                            // don't do anything if our state is none
                            break;
                        }

                        case TouchState.Moving:
                        {
                            // if we were moving, don't do anything except exit the movement state.
                            consumed = true;
                            State = TouchState.None;

                            Console.WriteLine( "UserNote Finished Moving" );
                            break;
                        }

                        case TouchState.Hold:
                        {
                            consumed = true;
                            State = TouchState.None;

                            // if delete enabled was turned on while holding
                            // (which would happen if a timer fired while holding)
                            // then don't toggle, they are deciding what to delete.
                            if( DeleteEnabled == false )
                            {
                                // if it's open and they tapped in the note anchor, close it.
                                if( TextField.Hidden == false )
                                {
                                    CloseNote();
                                }
                                // if it's closed and they tapped in the note anchor, open it
                                else
                                {
                                    OpenNote( );
                                }
                            }
                            break;
                        }

                        case TouchState.Delete:
                        {
                            Console.WriteLine( "User Wants to delete note" );
                            break;
                        }
                    }

                    DeleteTimer.Stop();

                    return consumed == true ? this : null;
                }

                protected void DeleteTimerDidFire(object sender, System.Timers.ElapsedEventArgs e)
                {
                    // if they're still in range and haven't moved the note yet, activate delete mode.
                    if ( TouchInAnchorRange( TrackingLastPos ) && State == TouchState.Hold )
                    {
                        // reveal the delete button
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate {  DeleteButton.Hidden = false; } );
                        DeleteEnabled = true;
                    }
                }

                public void Dispose( object masterView )
                {
                    // remove it from the UI
                    RemoveFromView( masterView );

                    // todo: do something here to fix android's focus issue
                }

                public void NoteTouchesCleared( )
                {
                    // This is called by our parent when we can safely assume NO NOTE
                    // was touched in the latest OnTouch/HoldTouch/EndTouch.

                    // This is important because to exit delete mode, hide a keyboard, etc.,
                    // we only want to do that when no other note is touched.
                    TextField.ResignFirstResponder( );

                    // force an offset so that the size of our box refreshes to not have the whitespace at the bottom.
                    AddOffset( 0, 0 );

                    if( DeleteEnabled == true )
                    {
                        DeleteEnabled = false;
                        Console.WriteLine( "Clearing Delete Mode" );

                        DeleteButton.Hidden = true;
                    }
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    // clamp X & Y movement to within margin of the screen
                    if( Anchor.Position.X + xOffset < MinNoteWidth )
                    {
                        // watch the left side
                        xOffset += MinNoteWidth - (Anchor.Position.X + xOffset);
                    }
                    else if( Anchor.Position.X + xOffset > MaxAllowedX )
                    {
                        // and the right
                        xOffset -= (Anchor.Position.X + xOffset) - MaxAllowedX;
                    }

                    // Check Y...
                    if( Anchor.Position.Y + yOffset < AnchorFrame.Height )
                    {
                        yOffset += AnchorFrame.Height - (Anchor.Position.Y + yOffset);
                    }
                    else if (Anchor.Position.Y + yOffset > MaxAllowedY )
                    {
                        yOffset -= (Anchor.Position.Y + yOffset) - MaxAllowedY;
                    }

                    // Now that offsets have been clamped, reposition the note
                    base.AddOffset( xOffset, yOffset );

                    TextField.Position = new PointF( TextField.Position.X + xOffset, 
                                                     TextField.Position.Y + yOffset );


                    Anchor.Position = new PointF( Anchor.Position.X + xOffset,
                                                  Anchor.Position.Y + yOffset );

                    DeleteButton.Position = new PointF( DeleteButton.Position.X + xOffset,
                                                        DeleteButton.Position.Y + yOffset );

                    AnchorFrame = Anchor.Frame;


                    // Scale the textfield to no larger than the remaining width of the screen 
                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, ScreenWidth - AnchorFrame.X ) );
                    TextField.Bounds = new RectangleF( 0, 0, width, TextField.Bounds.Height);
                }

                void ValidateBounds()
                {
                    // clamp X & Y movement to within margin of the screen
                    float xPos = Math.Max( Math.Min( AnchorFrame.X, MaxAllowedX ), MinNoteWidth );

                    float yPos = Math.Max( Math.Min( AnchorFrame.Y, MaxAllowedY ), AnchorFrame.Height );

                    Anchor.Position = new PointF( xPos, yPos );
                    AnchorFrame = Anchor.Frame;

                    // Scale the textfield to no larger than the remaining width of the screen 
                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, ScreenWidth - AnchorFrame.X ) );
                    TextField.Bounds = new RectangleF( 0, 0, width, TextField.Bounds.Height);
                }

                public override void AddToView( object obj )
                {
                    TextField.AddAsSubview( obj );
                    Anchor.AddAsSubview( obj );
                    DeleteButton.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    TextField.RemoveAsSubview( obj );
                    Anchor.RemoveAsSubview( obj );
                    DeleteButton.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                public void OpenNote()
                {
                    TextField.Hidden = false;
                    Console.WriteLine( "Opening Note" );
                }

                public void CloseNote()
                {
                    TextField.Hidden = true;
                    Console.WriteLine( "Closing Note" );
                }

                public override RectangleF GetFrame( )
                {
                    return TextField.Frame;
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    htmlStream += "<br><p><b>User Note - " + TextField.Text + "</b></p>";
                }

                public Notes.Model.NoteState.UserNoteContent GetContent( )
                {
                    return new Notes.Model.NoteState.UserNoteContent( AnchorFrame.X / PositionTransform.X, AnchorFrame.Y / PositionTransform.Y, TextField.Text, !TextField.Hidden );
                }
            }
        }
    }
}
