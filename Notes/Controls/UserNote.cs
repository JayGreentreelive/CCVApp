using System;
using System.Xml;
using System.Drawing;
using Notes.PlatformUI;

namespace Notes
{
    /// <summary>
    /// A label displaying Placeholder text. When tapped, allows
    /// a user to enter text via keyboard.
    /// </summary>
    public class UserNote : BaseControl
    {
        public enum DisplayState
        {
            Closed,
            Open
        }

        protected enum TrackingState
        {
            None,
            Holding,
            Moving
        };

        DisplayState CurrentState { get; set; }

        /// <summary>
        /// Actual textfield object.
        /// </summary>
        /// <value>The text field.</value>
        protected PlatformTextField TextField { get; set; }
        protected PlatformView Anchor { get; set; }
        protected RectangleF AnchorFrame { get; set; } //store the frame so we don't access UI objects on a seperate thread.

        protected PointF TrackingLastPos { get; set; }
        protected TrackingState Tracking { get; set; }
        protected System.Timers.Timer TrackingTimer { get; set; }

        protected float MaxAvailableWidth { get; set; }
        protected float MaxNoteWidth { get; set; }
        protected float MinNoteWidth { get; set; }
        protected float HeightPerLine { get; set; }

        protected override void Initialize( )
        {
            base.Initialize( );

            TextField = PlatformTextField.Create( );
            Anchor = PlatformView.Create( );

            Tracking = TrackingState.None;
        }

        public UserNote( CreateParams parentParams, PointF startPos )
        {
            Initialize( );

            //setup our timer for allowing movement/
            TrackingTimer = new System.Timers.Timer();
            TrackingTimer.Interval = 500;
            TrackingTimer.Elapsed += HoldTimerDidFire;


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
            Anchor.BackgroundColor = 0xFF0000FF;


            // Setup the dimensions.
            // The anchor is always the same.
            Anchor.Bounds = new RectangleF( 0, 0, parentParams.Width * .05f, parentParams.Width * .05f );

            // the text field should scale based on how close to the edge.
            MaxAvailableWidth = (parentParams.Width - Anchor.Bounds.Width);
            MinNoteWidth = (parentParams.Width * .10f );
            MaxNoteWidth = (parentParams.Width - Anchor.Bounds.Width) / 2;


            float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - startPos.X ) );
            TextField.Bounds = new RectangleF( 0, 0, width, 0 );


            // Setup the position
            Anchor.Position = startPos;
            TextField.Position = new PointF( Anchor.Frame.Right, 
                                             Anchor.Frame.Bottom );

            // Copy the Anchor's frame into another variable so we dont access UI outside of the main thread.
            AnchorFrame = Anchor.Frame;

            OpenNote( );
        }

        public bool TouchInAnchorRange( PointF touch )
        {
            //TODO: Fix this, we shouldn't have to change it per platform
            #if __IOS__
            float maxDist = 625.0f;
            #endif
            #if __ANDROID__
            float maxDist = 2500.0f;
            #endif

            // create a vector from the note anchor's center to the touch
            PointF labelToTouch = new PointF( touch.X - (AnchorFrame.X + AnchorFrame.Width / 2), 
                                              touch.Y - (AnchorFrame.Y + AnchorFrame.Height / 2));

            float distSquared = RockMobile.Math.Util.MagnitudeSquared( labelToTouch );
            if( distSquared < maxDist )
            {
                return true;
            }

            return false;
        }

        public override bool TouchesBegan( PointF touch )
        {
            // is a user wanting to move us?

            // if the touch is in our region, begin tracking
            if( TouchInAnchorRange( touch ) )
            {
                Console.WriteLine( "UserNote BEGAN HOLDING");

                // flag that they're now holding
                Tracking = TrackingState.Holding;

                TrackingTimer.Start();
                TrackingLastPos = touch;

                return true;
            }

            return false;
        }

        protected void HoldTimerDidFire(object sender, System.Timers.ElapsedEventArgs e)
        {
            if( Tracking == TrackingState.Holding )
            {
                // are they still holding within the valid region?
                // if the touch is in our region, begin tracking
                if ( TouchInAnchorRange( TrackingLastPos ) )
                {
                    // and update the state
                    Tracking = TrackingState.Moving;

                    Anchor.BackgroundColor = 0x00FF00FF;

                    Console.WriteLine( "UserNote WILL BEGIN MOVING" );
                }
                else
                {
                    Console.WriteLine( "UserNote WONT MOVE. TOO FAR" );
                }
            }
            else
            {
                Console.WriteLine( "UserNote WAS NOT HOLDING WHEN TIMER FIRED." );
            }

            TrackingTimer.Stop();
        }

        public override void TouchesMoved( PointF touch )
        {
            // are we moving?
            if( Tracking == TrackingState.Holding )
            {
                // are we holding to see if they want to move?
                TrackingLastPos = touch;

                Console.WriteLine( "UserNote HOLDING MOVING" );
            }
            if (Tracking == TrackingState.Moving )
            {
                // if we're moving, update by the amount we moved.
                PointF delta = new PointF( touch.X - TrackingLastPos.X, touch.Y - TrackingLastPos.Y );

                AddOffset( delta.X, delta.Y );

                // stamp our position
                TrackingLastPos = touch;

                Console.WriteLine( "UserNote MOVING" );
            }
        }

        public override bool TouchesEnded( PointF touch )
        {
            bool consumed = false;

            // first, if we were moving, don't do anything except cancel movement.
            if( Tracking == TrackingState.Moving )
            {
                consumed = true;
            }
            // manage the note only if it's not none, because that means we
            // tapped IN it. If it's none, then this ending tap may or may not be in the note, but the tap DOWN wasnt.
            else if (Tracking != TrackingState.None )
            {
                // if the touch that was released was in our anchor, we will toggle
                if( TouchInAnchorRange( touch ) )
                {
                    // if it's open and they tapped in the note anchor, close it.
                    if( CurrentState == DisplayState.Open )
                    {
                        CloseNote();
                        consumed = true;
                    }
                    // if it's closed and they tapped in the note anchor, open it
                    else if( CurrentState == DisplayState.Closed )
                    {
                        OpenNote( );
                        consumed = true;
                    }
                }
            }

            // always turn off tracking once we've released
            Anchor.BackgroundColor = 0xFF0000FF; //revert the color to red
            Tracking = TrackingState.None;
            TrackingTimer.Stop();

            return consumed;
        }

        public void ResignFirstResponder( )
        {
            // We let ResignFirstResponder be its own function so that
            // the Note can let us know when to hide our keyboard. 

            // We cannot do it ourselves because it might not be THIS UserNote
            // that's being edited. It could be that this one was just toggled
            // open/close/moved which should NOT cause the keyboard to hide.
            TextField.ResignFirstResponder( );
        }

        public override void AddOffset( float xOffset, float yOffset )
        {
            // clamp X & Y movement to within margin of the screen
            float maxX = MaxAvailableWidth - AnchorFrame.Width;
            if( Anchor.Position.X + xOffset < (AnchorFrame.Width * 2) )
            {
                // watch the left side
                xOffset += (AnchorFrame.Width * 2) - (Anchor.Position.X + xOffset);
            }
            else if( Anchor.Position.X + xOffset > maxX )
            {
                // and the right
                xOffset -= (Anchor.Position.X + xOffset) - maxX;
            }

            //TODO - add Y
            /*float maxY = MaxAvailableWidth - AnchorFrame.Width;
            if( Anchor.Position.X + xOffset > maxX )
            {
                xOffset -= (Anchor.Position.X + xOffset) - maxX;
            }*/

            base.AddOffset( xOffset, yOffset );

            TextField.Position = new PointF( TextField.Position.X + xOffset, 
                                             TextField.Position.Y + yOffset );


            Anchor.Position = new PointF( Anchor.Position.X + xOffset,
                                          Anchor.Position.Y + yOffset );

            AnchorFrame = Anchor.Frame;


            // Scale the textfield to no larger than the remaining width of the screen 
            float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - AnchorFrame.X ) );
            TextField.Bounds = new RectangleF( 0, 0, width, TextField.Bounds.Height);
        }

        public override void AddToView( object obj )
        {
            TextField.AddAsSubview( obj );
            Anchor.AddAsSubview( obj );

            TryAddDebugLayer( obj );
        }

        public override void RemoveFromView( object obj )
        {
            TextField.RemoveAsSubview( obj );
            Anchor.RemoveAsSubview( obj );

            TryRemoveDebugLayer( obj );
        }

        public void OpenNote()
        {
            CurrentState = DisplayState.Open;
            TextField.Hidden = false;
        }

        public void CloseNote()
        {
            CurrentState = DisplayState.Closed;
            TextField.Hidden = true;
        }

        public override RectangleF GetFrame( )
        {
            base.DebugFrameView.Frame = TextField.Frame;
            return TextField.Frame;
        }
    }
}
