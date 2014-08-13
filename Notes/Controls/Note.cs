using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;
using System.IO;
using Notes.Styles;

namespace Notes
{
    /// <summary>
    /// The core object that all UI Controls are children of. Does not need to derive from BaseControl.
    /// Includes utility functions for creating a Note object and initializing the styles.
    /// </summary>
    public class Note
    {
        const string USER_NOTE_FILENAME = "userNotes.txt";

        /// <summary>
        /// Delegate for notifying the caller when a note is ready to be created via Note.Create()
        /// </summary>
        public delegate void OnPreReqsComplete( Note note, Exception e );

        /// <summary>
        /// A list of all immediate child controls of the Note. (This list is hierchical and not flat)
        /// </summary>
        /// <value>The child controls.</value>
        protected List<IUIControl> ChildControls { get; set; }

        /// <summary>
        /// A seperate list of User Notes.
        /// </summary>
        /// <value>The user note controls.</value>
        protected List<UserNote> UserNoteControls { get; set; }

        /// <summary>
        /// a reference to the Anchor being touched or moved. This
        /// does NOT point to a note being edited. That's different.
        /// </summary>
        /// <value>The active user note anchor.</value>
        protected IUIControl ActiveUserNoteAnchor { get; set; }

        /// <summary>
        /// The NoteScript XML. Stored for rebuilding notes on an orientation change.
        /// </summary>
        /// <value>The note xml.</value>
        public string NoteXml { get; protected set; }

        /// <summary>
        /// The style settings for the Note. Will be passed to all children.
        /// </summary>
        protected Style mStyle;

        /// <summary>
        /// The bounds (including position) of the note.
        /// </summary>
        /// <value>The bounds.</value>
        protected RectangleF Frame { get; set; }

        /// <summary>
        /// The view that actually contains all controls
        /// </summary>
        /// <value>The master view.</value>
        protected object MasterView { get; set; }

        protected string UserNotePath { get; set; }

        public static void HandlePreReqs( string noteXml, string styleXml, OnPreReqsComplete onPreReqsComplete )
        {
            // now use a reader to get each element
            XmlReader reader = XmlReader.Create( new StringReader( noteXml ) );

            string styleSheetUrl = "";

            bool finishedReading = false;
            while( finishedReading == false && reader.Read( ) )
            {
                // expect the first element to be "Note"
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        if( reader.Name == "Note" )
                        {
                            styleSheetUrl = reader.GetAttribute( "StyleSheet" );
                            if( styleSheetUrl == null )
                            {
                                throw new Exception( "Could not find attribute 'StyleSheet'. This should be a URL pointing to the style to use." );
                            }
                        }
                        else
                        {
                            throw new Exception( string.Format( "Expected root element to be <Note>. Found <{0}>", reader.Name ) );
                        }

                        finishedReading = true;
                        break;
                    }
                }
            }

            // Parse the styles. We cannot go any further until this is finished.
            ControlStyles.Initialize( styleSheetUrl, styleXml, (Exception e ) =>
                {
                    // We don't just create the note here because the parent
                    // might need to change threads before creating UI objects
                    onPreReqsComplete( new Note( noteXml ), e );
                } );
        }

        public Note( string noteXml )
        {
            // store our XML
            NoteXml = noteXml;

            mStyle = new Style( );
            mStyle.Initialize( );

            UserNotePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), USER_NOTE_FILENAME);
        }

        public void Create( float parentWidth, float parentHeight, object masterView )
        {
            MasterView = masterView;

            // create a child control list
            ChildControls = new List<IUIControl>( );
            UserNoteControls = new List<UserNote>( ); //store these seperately so we can back them up and test touch input.

            // now use a reader to get each element
            XmlReader reader = XmlReader.Create( new StringReader( NoteXml ) );

            // begin reading the xml stream
            bool finishedReading = false;
            while( finishedReading == false && reader.Read( ) )
            {
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        if( reader.Name == "Note" )
                        {
                            ParseNote( reader, parentWidth, parentHeight );
                        }
                        else
                        {
                            throw new Exception( String.Format( "Expected <Note> element. Found <{0}> instead.", reader.Name ) );
                        }

                        finishedReading = true;
                        break;
                    }
                }
            }
        }

        public void Destroy( object obj )
        {
            // before destroying notes, save them
            SaveUserNotes( UserNotePath );

            // release references to our UI objects
            if( ChildControls != null )
            {
                foreach( IUIControl uiControl in ChildControls )
                {
                    uiControl.RemoveFromView( obj );
                }

                // and clear our UI list
                ChildControls.Clear( );
            }

            if( UserNoteControls != null )
            {
                // remove (but don't destroy) the notes
                foreach( IUIControl uiControl in UserNoteControls )
                {
                    uiControl.RemoveFromView( obj );
                }

                UserNoteControls.Clear();
            }

            NoteXml = null;
        }

        void ParseNote( XmlReader reader, float parentWidth, float parentHeight )
        {
            // check for attributes we support
            RectangleF bounds = new RectangleF( );
            Parser.ParseBounds( reader, ref bounds );

            // LEFT/TOP POSITIONING
            if( bounds.X < 1 )
            {
                // convert % to pixel, based on parent's width
                bounds.X = parentWidth * bounds.X;
            }

            if( bounds.Y < 1 )
            {
                // convert % to pixel, based on parent's width
                bounds.Y = parentHeight * bounds.Y;
            }

            // check for any styles requested in XML
            Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mMainNote );

            // PADDING
            float leftPadding = Styles.Style.GetStyleValue( mStyle.mPaddingLeft, parentWidth );
            float rightPadding = Styles.Style.GetStyleValue( mStyle.mPaddingRight, parentWidth );
            float topPadding = Styles.Style.GetStyleValue( mStyle.mPaddingTop, parentHeight );
            float bottomPadding = Styles.Style.GetStyleValue( mStyle.mPaddingBottom, parentHeight );

            // now calculate the available width based on padding. (Don't actually change our width)
            float availableWidth = parentWidth - leftPadding - rightPadding;

            // begin reading the xml stream
            bool finishedReading = false;
            while( finishedReading == false && reader.Read( ) )
            {
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        IUIControl control = Parser.TryParseControl( new BaseControl.CreateParams( availableWidth, parentHeight, ref mStyle ), reader );
                        ChildControls.Add( control );
                        break;
                    }

                    case XmlNodeType.EndElement:
                    {
                        if( reader.Name == "Note" )
                        {
                            finishedReading = true;
                        }
                        break;
                    }
                }
            }

            // lay stuff out vertically like a stackPanel.
            // layout all controls
            float yOffset = bounds.Y + topPadding; //vertically they should just stack

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
                    {
                        xAdjust = bounds.X + ( ( availableWidth / 2 ) - ( controlFrame.Width / 2 ) );
                        break;
                    }
                    case Alignment.Right:
                    {
                        xAdjust = bounds.X + ( availableWidth - controlFrame.Width );
                        break;
                    }
                    case Alignment.Left:
                    {
                        xAdjust = bounds.X;
                        break;
                    }
                }

                // adjust the next sibling by yOffset
                control.AddOffset( xAdjust + leftPadding, yOffset );

                // and the next sibling must begin there
                yOffset = control.GetFrame( ).Bottom;
            }

            bounds.Width = parentWidth;
            bounds.Height = ( yOffset - bounds.Y ) + bottomPadding;
            Frame = bounds;

            AddControlsToView( );

            // finally, load the user notes for this note
            LoadUserNotes( UserNotePath );
        }

        protected void AddControlsToView( )
        {
            foreach( IUIControl uiControl in ChildControls )
            {
                uiControl.AddToView( MasterView );
            }
        }

        public RectangleF GetFrame( )
        {
            return Frame;
        }

        public bool TouchesBegan( PointF touch )
        {
            // We receive TouchesBegan if anything except a TextField was tapped.
            // The only control we have that needs this is the UserNote for its Anchor.

            // So, see if the user is tapping on a UserNoteAnchor.
            foreach( IUIControl control in UserNoteControls )
            {
                // If a user note returns true, its anchor is being touched.
                if( control.TouchesBegan( touch ) == true )
                {
                    // Begin tracking this anchor for movement and touchEnd
                    ActiveUserNoteAnchor = control;
                    return true;
                }
            }

            // No UserNote Anchors were touched, so do not
            // say we consumed this.
            return false;
        }

        public void TouchesMoved( PointF touch )
        {
            // If a UserNote anchor was tapped in TouchesBegan,
            // we will have an active Anchor, so notify it of the new position.
            if( ActiveUserNoteAnchor != null )
            {
                ActiveUserNoteAnchor.TouchesMoved( touch );
            }
        }

        public void TouchesEnded( PointF touch )
        {
            // TouchesEnded is tricky. It's reasonable a User will have
            // the keyboard up and decide to open/close/move another Note.
            // This should NOT cause the keyboard to hide. The only time a keyboard
            // should hide is if a User taps on a general area of the screen.

            // To accomplish this, we rely on ActiveUserNoteAnchor. If it's valid in TouchesEnded,
            // that means the touch was in an anchor and not a general part of the screen.

            // If there's an active UserNote Anchor, notify only it.
            if( ActiveUserNoteAnchor != null )
            {
                // Notify the active anchor, and clear it since the user released input.
                ActiveUserNoteAnchor.TouchesEnded( touch );
                ActiveUserNoteAnchor = null;
            }
            else
            {
                // Since a UserNote Anchor was NOT touched, we know it was a "general"
                // area of the screen, and can allow the keyboard to hide.
                foreach( UserNote userNote in UserNoteControls )
                {
                    userNote.ResignFirstResponder( );
                }


                // Now notify all remaining controls until we find out one consumed it.
                // This is purely for efficiency.
                bool consumed = false;
                foreach( IUIControl control in UserNoteControls )
                {
                    // was it consumed?
                    consumed = control.TouchesEnded( touch );
                    if( consumed )
                    {
                        break;
                    }
                }

                // if no user note consumed it, notify all regular controls
                if( consumed == false )
                {
                    // notify all controls
                    foreach( IUIControl control in ChildControls )
                    {
                        // was it consumed?
                        consumed = control.TouchesEnded( touch );
                        
                        if( consumed )
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void DidDoubleTap(PointF touch)
        {
            // do not allow a new note within range of another's anchor.
            bool allowNoteCreation = true;

            foreach( UserNote userNote in UserNoteControls )
            {
                if( userNote.TouchInAnchorRange( touch ) )
                {
                    allowNoteCreation = false;
                }
            }

            if( allowNoteCreation )
            {
                UserNote userNote = new UserNote( new BaseControl.CreateParams( Frame.Width, Frame.Height, ref mStyle ), touch );
                UserNoteControls.Add( userNote );

                userNote.AddToView( MasterView );
            }
        }

        protected void LoadUserNotes( string filePath )
        {
            // if the file exists
            if(System.IO.File.Exists(filePath) == true)
            {
                // read it
                using (StreamReader reader = new StreamReader(filePath))
                {
                    // for each note found
                    string noteJson = reader.ReadLine();
                    while( noteJson != null )
                    {
                        // create the note, add it to our list, and to the view
                        UserNote note = new UserNote( new BaseControl.CreateParams( Frame.Width, Frame.Height, ref mStyle ), noteJson );
                        UserNoteControls.Add( note );
                        note.AddToView( MasterView );

                        noteJson = reader.ReadLine();
                    }
                }
            }
        }

        protected void SaveUserNotes( string filePath )
        {
            // if there are any user notes
            if( UserNoteControls.Count > 0 )
            {
                // open a stream
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // write the serialized json for each note
                    foreach( UserNote note in UserNoteControls )
                    {
                        writer.WriteLine( note.Serialize( ) );
                    }
                }
            }
        }
    }
}
