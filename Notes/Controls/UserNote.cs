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
        /// <summary>
        /// Actual textfield object.
        /// </summary>
        /// <value>The text field.</value>
        protected PlatformTextField TextField { get; set; }

        /// <summary>
        /// The view representing the note's "Anchor"
        /// </summary>
        protected PlatformView Anchor { get; set; }

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

        protected override void Initialize( )
        {
            base.Initialize( );

            TextField = PlatformTextField.Create( );
            Anchor = PlatformView.Create( );
        }

        public UserNote( BaseControl.CreateParams createParams, string noteText )
        {
            // first de-serialize this note
            Model.MobileNote mobileNote = Notes.Model.MobileNote.Deserialize( noteText );

            Create( createParams, mobileNote.Position, mobileNote.Text );

            // new notes are open by default. So if we're restoring one that was closed,
            // keep it closed.
            if( mobileNote.WasOpen == false )
            {
                CloseNote( );
            }
        }

        public UserNote( CreateParams parentParams, PointF startPos )
        {
            Create( parentParams, startPos, null );
        }

        public void Create( CreateParams parentParams, PointF startPos, string startingText )
        {
            Initialize( );

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
            MaxAvailableWidth = ( parentParams.Width - Anchor.Bounds.Width );
            MaxAvailableHeight = ( parentParams.Height - Anchor.Bounds.Height );
            MinNoteWidth = (parentParams.Width * .10f );
            MaxNoteWidth = (parentParams.Width - Anchor.Bounds.Width) / 2;


            float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - startPos.X ) );
            TextField.Bounds = new RectangleF( 0, 0, width, 0 );


            // Setup the position
            Anchor.Position = startPos;

            // validate its bounds
            ValidateBounds( );

            // set the actual note textfield relative to the anchor
            TextField.Position = new PointF( Anchor.Frame.Right, 
                                             Anchor.Frame.Bottom );

            // set the starting text if it was provided
            if( startingText != null )
            {
                TextField.Text = startingText;
            }
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
            PointF labelToTouch = new PointF( touch.X - (Anchor.Frame.X + Anchor.Frame.Width / 2), 
                                              touch.Y - (Anchor.Frame.Y + Anchor.Frame.Height / 2));

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
                // Begin tracking for movement
                DidMoveNote            = false;
                TrackingLastPos        = touch;
                Anchor.BackgroundColor = 0x00FF00FF;
                Console.WriteLine( "UserNote WILL BEGIN MOVING" );

                return true;
            }

            return false;
        }

        public override void TouchesMoved( PointF touch )
        {
            // if we're moving, update by the amount we moved.
            PointF delta = new PointF( touch.X - TrackingLastPos.X, touch.Y - TrackingLastPos.Y );

            AddOffset( delta.X, delta.Y );

            // stamp our position
            TrackingLastPos = touch;

            DidMoveNote = true;

            Console.WriteLine( "UserNote MOVING" );
        }

        public override bool TouchesEnded( PointF touch )
        {
            bool consumed = false;

            // first, if we were moving, don't do anything except cancel movement.
            if( DidMoveNote == true)
            {
                DidMoveNote = false;
                consumed = true;
            }
            // only manage the note if it wasn't moved, because we
            // do not want it to toggle after repositioning it.
            else
            {
                // if the touch that was released was in our anchor, we will toggle
                if( TouchInAnchorRange( touch ) )
                {
                    // if it's open and they tapped in the note anchor, close it.
                    if( TextField.Hidden == false )
                    {
                        CloseNote();
                        consumed = true;
                    }
                    // if it's closed and they tapped in the note anchor, open it
                    else
                    {
                        OpenNote( );
                        consumed = true;
                    }
                }
            }


            //revert the color to red
            Anchor.BackgroundColor = 0xFF0000FF;

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
            float maxX = MaxAvailableWidth - Anchor.Frame.Width;
            if( Anchor.Position.X + xOffset < Anchor.Frame.Width )
            {
                // watch the left side
                xOffset += Anchor.Frame.Width - (Anchor.Position.X + xOffset);
            }
            else if( Anchor.Position.X + xOffset > maxX )
            {
                // and the right
                xOffset -= (Anchor.Position.X + xOffset) - maxX;
            }

            // Check Y...
            float maxY = MaxAvailableHeight - Anchor.Frame.Height;
            if( Anchor.Position.Y + yOffset < Anchor.Frame.Height )
            {
                yOffset += Anchor.Frame.Height - (Anchor.Position.Y + yOffset);
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


            // Scale the textfield to no larger than the remaining width of the screen 
            float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - Anchor.Frame.X ) );
            TextField.Bounds = new RectangleF( 0, 0, width, TextField.Bounds.Height);
        }

        void ValidateBounds()
        {
            // clamp X & Y movement to within margin of the screen
            float maxX = MaxAvailableWidth - MaxNoteWidth;//Anchor.Frame.Width;
            float xPos = Math.Max( Math.Min( Anchor.Frame.X, maxX ), Anchor.Frame.Width );

            float maxY = MaxAvailableHeight - Anchor.Frame.Height;
            float yPos = Math.Max( Math.Min( Anchor.Frame.Y, maxY ), Anchor.Frame.Height );

            Anchor.Position = new PointF( xPos, yPos );


            // Scale the textfield to no larger than the remaining width of the screen 
            float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAvailableWidth - Anchor.Frame.X ) );
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
            TextField.Hidden = false;
        }

        public void CloseNote()
        {
            TextField.Hidden = true;
        }

        public override RectangleF GetFrame( )
        {
            base.DebugFrameView.Frame = TextField.Frame;
            return TextField.Frame;
        }

        public string Serialize( )
        {
            return Notes.Model.MobileNote.Serialize( new PointF( Anchor.Frame.X, Anchor.Frame.Y ), TextField.Text, !TextField.Hidden );
        }
    }
}
