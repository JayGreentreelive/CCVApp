﻿using System;
using System.Xml;
using System.Drawing;
using Rock.Mobile.PlatformUI;
using System.Threading;

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
                protected PlatformView Anchor { get; set; }
                protected RectangleF AnchorFrame { get; set; }

                /// <summary>
                /// Delete button
                /// </summary>
                /// <value>The anchor.</value>
                protected PlatformView DeleteButton { get; set; }

                /// <summary>
                /// Tracks the movement of the note as a user repositions it.
                /// </summary>
                protected PointF TrackingLastPos { get; set; }

                /// <summary>
                /// True if the note was moved after being tapped.
                /// </summary>
                protected bool DidMoveNote { get; set; }

                /// <summary>
                /// The width of the note's parent. Used to 
                /// create the initial size as a percentage of the available width,
                /// and to keep the note within boundaries.
                /// </summary>
                protected float MaxAvailableWidth { get; set; }

                /// <summary>
                /// The height of the note's parent. Used to 
                /// create the initial size as a percentage of the available height,
                /// and to keep the note within boundaries.
                /// </summary>
                protected float MaxAvailableHeight { get; set; }

                /// <summary>
                /// The maximum width of the note.
                /// </summary>
                protected float MaxNoteWidth { get; set; }

                /// <summary>
                /// The minimum width of the note.
                /// </summary>
                protected float MinNoteWidth { get; set; }

                /// <summary>
                /// Protects the main thread TouchesEnded from race conditions
                /// with the timer callback thread.
                /// </summary>
                /// <value>The lock.</value>
                // JHM: Don't think we need this because we're simply invoking on the main thread,
                // therefore there cannot be a race condition, since everything gets serialized to a single thread.
                //protected Mutex Lock { get; set; }

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


                protected override void Initialize( )
                {
                    base.Initialize( );

                    //Lock = new Mutex( );

                    TextField = PlatformTextField.Create( );
                    Anchor = PlatformView.Create( );
                    DeleteButton = PlatformView.Create( );
                }

                public UserNote( BaseControl.CreateParams createParams, float deviceHeight, string noteText )
                {
                    // first de-serialize this note
                    Model.MobileNote mobileNote = Notes.Model.MobileNote.Deserialize( noteText );

                    Create( createParams, deviceHeight, mobileNote.Position, mobileNote.Text );

                    // new notes are open by default. So if we're restoring one that was closed,
                    // keep it closed.
                    if( mobileNote.WasOpen == false )
                    {
                        CloseNote( );
                    }
                }

                public UserNote( CreateParams parentParams, float deviceHeight, PointF startPos )
                {
                    Create( parentParams, deviceHeight, startPos, null );
                }

                public void Create( CreateParams parentParams, float deviceHeight, PointF startPos, string startingText )
                {
                    Initialize( );

                    //magic number ratio that works well!
                    #if __IOS__
                    float heightTouchRatio = .048f;
                    #endif
                    #if __ANDROID__
                    float heightTouchRatio = .078f;
                    #endif

                    // the touch range differs based on various device sizes. It makes more sense
                    // to give a larger area for a tablet and a smaller area to a phone.
                    AnchorTouchMaxDist = deviceHeight * heightTouchRatio;
                    AnchorTouchMaxDist *= AnchorTouchMaxDist;

                    //setup our timer for allowing movement/
                    DeleteTimer = new System.Timers.Timer();
                    DeleteTimer.Interval = 1000;
                    DeleteTimer.Elapsed += DeleteTimerDidFire;
                    DeleteTimer.AutoReset = false;

                    // take our parent's style or in defaults
                    mStyle = parentParams.Style;
                    Styles.Style.MergeStyleAttributesWithDefaults( ref mStyle, ref ControlStyles.mTextInput );

                    // flag that we want this text field to grow as more text is added
                    TextField.ScaleHeightForText = true;


                    // Setup the font
                    TextField.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    TextField.TextColor = mStyle.mFont.mColor.Value;
                    TextField.Placeholder = "Enter note";
                    TextField.PlaceholderTextColor = mStyle.mFont.mColor.Value;

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        TextField.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }
                    TextField.BackgroundColor = 0xFFFFFFFF;


                    // Setup the anchor color
                    Anchor.BackgroundColor = 0x0000FFFF;


                    // Setup the dimensions.
                    // The anchor is always the same.
                    Anchor.Bounds = new RectangleF( 0, 0, parentParams.Width * .05f, parentParams.Width * .05f );

                    // the text field should scale based on how close to the edge.
                    MaxAvailableWidth = ( parentParams.Width - Anchor.Bounds.Width );
                    MaxAvailableHeight = ( parentParams.Height - Anchor.Bounds.Height );
                    MinNoteWidth = (parentParams.Width * .10f );
                    MaxNoteWidth = (parentParams.Width - Anchor.Bounds.Width) / 2;


                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - startPos.X ) );
                    TextField.Bounds = new RectangleF( 0, 0, width, 0 );

                    DeleteButton.Bounds = new RectangleF( 0, 0, Anchor.Bounds.Width / 2, Anchor.Bounds.Height / 2 );


                    // Setup the position
                    Anchor.Position = startPos;
                    AnchorFrame = Anchor.Frame;

                    // validate its bounds
                    ValidateBounds( );

                    // set the position for the delete button
                    DeleteButton.Position = new PointF( AnchorFrame.Left - DeleteButton.Bounds.Width / 2, 
                                                        AnchorFrame.Top - DeleteButton.Bounds.Height / 2 );

                    DeleteButton.BackgroundColor = 0xFF0000FF;
                    DeleteButton.Hidden = true;

                    // set the actual note textfield relative to the anchor
                    TextField.Position = new PointF( AnchorFrame.Right, 
                        AnchorFrame.Bottom );

                    // set the starting text if it was provided
                    if( startingText != null )
                    {
                        TextField.Text = startingText;
                    }
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
                            TrackingLastPos        = touch;
                            Anchor.BackgroundColor = 0x00FF00FF;
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
                public override bool TouchesEnded( PointF touch )
                {
                    // wait for the timer thread to be finished
                    //Lock.WaitOne( );

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

                            Anchor.BackgroundColor = 0x0000FFFF;
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

                            Anchor.BackgroundColor = 0x0000FFFF;
                            break;
                        }

                        case TouchState.Delete:
                        {
                            Console.WriteLine( "User Wants to delete note" );
                            break;
                        }
                    }

                    DeleteTimer.Stop();

                    //Lock.ReleaseMutex( );

                    return consumed;
                }

                protected void DeleteTimerDidFire(object sender, System.Timers.ElapsedEventArgs e)
                {
                    // wait for the main thread to be finished
                    // JHM: Don't think we need this because we're simply invoking on the main thread,
                    // therefore there cannot be a race condition, since everything gets serialized to a single thread.
                    //Lock.WaitOne( );

                    // if they're still in range and haven't moved the note yet, activate delete mode.
                    if ( TouchInAnchorRange( TrackingLastPos ) && State == TouchState.Hold )
                    {
                        // reveal the delete button
                        Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate {  DeleteButton.Hidden = false; } );
                        DeleteEnabled = true;
                    }

                    //Lock.ReleaseMutex( );
                }

                public void Dispose( object masterView )
                {
                    // release the mutex
                    //Lock.Dispose( );

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
                    float maxX = MaxAvailableWidth - AnchorFrame.Width;
                    if( Anchor.Position.X + xOffset < AnchorFrame.Width )
                    {
                        // watch the left side
                        xOffset += AnchorFrame.Width - (Anchor.Position.X + xOffset);
                    }
                    else if( Anchor.Position.X + xOffset > maxX )
                    {
                        // and the right
                        xOffset -= (Anchor.Position.X + xOffset) - maxX;
                    }

                    // Check Y...
                    float maxY = MaxAvailableHeight - AnchorFrame.Height;
                    if( Anchor.Position.Y + yOffset < AnchorFrame.Height )
                    {
                        yOffset += AnchorFrame.Height - (Anchor.Position.Y + yOffset);
                    }
                    else if (Anchor.Position.Y + yOffset > maxY )
                    {
                        yOffset -= (Anchor.Position.Y + yOffset) - maxY;
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
                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - AnchorFrame.X ) );
                    TextField.Bounds = new RectangleF( 0, 0, width, TextField.Bounds.Height);
                }

                void ValidateBounds()
                {
                    // clamp X & Y movement to within margin of the screen
                    float maxX = MaxAvailableWidth - MaxNoteWidth;//AnchorFrame.Width;
                    float xPos = Math.Max( Math.Min( AnchorFrame.X, maxX ), AnchorFrame.Width );

                    float maxY = MaxAvailableHeight - AnchorFrame.Height;
                    float yPos = Math.Max( Math.Min( AnchorFrame.Y, maxY ), AnchorFrame.Height );

                    Anchor.Position = new PointF( xPos, yPos );
                    AnchorFrame = Anchor.Frame;

                    // Scale the textfield to no larger than the remaining width of the screen 
                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - AnchorFrame.X ) );
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
                    base.DebugFrameView.Frame = TextField.Frame;
                    return TextField.Frame;
                }

                public string Serialize( )
                {
                    return Notes.Model.MobileNote.Serialize( new PointF( AnchorFrame.X, AnchorFrame.Y ), TextField.Text, !TextField.Hidden );
                }
            }
        }
    }
}