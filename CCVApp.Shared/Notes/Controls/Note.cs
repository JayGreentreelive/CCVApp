using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;
using System.IO;

using CCVApp.Shared.Notes.Styles;
using CCVApp.Shared.Notes.Model;
using Newtonsoft.Json;
using Rock.Mobile.PlatformUI;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// The core object that all UI Controls are children of. Does not need to derive from BaseControl.
            /// Includes utility functions for creating a Note object and initializing the styles.
            /// </summary>
            public class Note
            {
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
                protected UserNote ActiveUserNoteAnchor { get; set; }

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

                /// <summary>
                /// The path for loading/saving user notes
                /// </summary>
                /// <value>The user note path.</value>
                protected string UserNotePath { get; set; }

                /// <summary>
                /// The height of the device, used during user note creation.
                /// </summary>
                /// <value>The height of the device.</value>
                protected float DeviceHeight { get; set; }

                /// <summary>
                /// When true we're waiting for our timer to tick and load notes.
                /// This prevents the note state from being overwritten by the notes reloading
                /// while the timer is still pending
                /// </summary>
                /// <value><c>true</c> if loading note state; otherwise, <c>false</c>.</value>
                protected bool LoadingNoteState { get; set; }

                /// <summary>
                /// To speed up note generation, we delay note state loading with a timer.
                /// This allows the notes to draw and then the 500ms file i/o to occur after.
                /// </summary>
                /// <value>The load state timer.</value>
                protected System.Timers.Timer LoadStateTimer { get; set; }

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
                }

                public void Create( float parentWidth, float parentHeight, object masterView, string userNoteFileName )
                {
                    // setup our note timer that will wait to load our notes until AFTER the notes are created,
                    // as opposed to the same tick. This cuts down 500ms from the create time.
                    LoadStateTimer = new System.Timers.Timer();
                    LoadStateTimer.AutoReset = false;
                    LoadStateTimer.Interval = 25;
                    LoadStateTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                        {
                            // when the timer fires, hide the toolbar.
                            // Although the timer fires on a seperate thread, because we queue the reveal
                            // on the main (UI) thread, we don't have to worry about race conditions.
                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { LoadState( UserNotePath ); } );
                        };

                    UserNotePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), userNoteFileName);

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
                    DeviceHeight = parentHeight;

                    // get the style first
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mMainNote );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    SizeF parentSize = new SizeF( parentWidth, parentHeight );
                    Parser.ParseBounds( reader, ref parentSize, ref bounds );

                    // Parent note doesn't support margins.

                    // PADDING
                    float leftPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingLeft, parentWidth, 0 );
                    float rightPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingRight, parentWidth, 0 );
                    float topPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingTop, parentHeight, 0 );
                    float bottomPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingBottom, parentHeight, 0 );

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
                                IUIControl control = Parser.TryParseControl( new BaseControl.CreateParams( this, availableWidth, parentHeight, ref mStyle ), reader );
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
                        RectangleF controlMargin = control.GetMargin( );

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
                                xAdjust = bounds.X + ( availableWidth - (controlFrame.Width + controlMargin.Width) );
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
                        yOffset = control.GetFrame( ).Bottom + controlMargin.Height;
                    }

                    bounds.Width = parentWidth;
                    bounds.Height = ( yOffset - bounds.Y ) + bottomPadding;
                    Frame = bounds;

                    AddControlsToView( );

                    // kick off the timer that will load the user note state
                    if( LoadingNoteState == false )
                    {
                        LoadingNoteState = true;
                        LoadStateTimer.Start( );
                    }
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

                public void GetControlOfType<TControlType>( List<IUIControl> controlList ) where TControlType : class
                {
                    // let each child add itself and its children
                    foreach( IUIControl control in ChildControls )
                    {
                        control.GetControlOfType<TControlType>( controlList );
                    }
                }

                public bool HitTest( PointF touch )
                {
                    // So, see if the user is tapping on a UserNoteAnchor.
                    foreach( UserNote control in UserNoteControls )
                    {
                        // If a user note returns true, its anchor is being touched.
                        if( control.HitTest( touch ) == true )
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public bool TouchesBegan( PointF touch )
                {
                    // We receive TouchesBegan if anything except a TextField was tapped.
                    // The only control we have that needs this is the UserNote for its Anchor.

                    // So, see if the user is tapping on a UserNoteAnchor.
                    foreach( UserNote control in UserNoteControls )
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

                        // does this note want to be deleted?
                        if( ActiveUserNoteAnchor.State == UserNote.TouchState.Delete )
                        {
                            ActiveUserNoteAnchor.Dispose( MasterView );

                            // remove it from our list. Because our next step will be
                            // to clear the anchor ref, that will effectively delete the note (eligible for garbage collection)
                            UserNoteControls.Remove( ActiveUserNoteAnchor );
                        }

                        // clear the user note
                        ActiveUserNoteAnchor = null;
                    }
                    else
                    {
                        // Since a UserNote Anchor was NOT touched, we know it was a "general"
                        // area of the screen, and can allow the keyboard to hide.
                        foreach( UserNote userNote in UserNoteControls )
                        {
                            userNote.NoteTouchesCleared( );
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
                        UserNote userNote = new UserNote( new BaseControl.CreateParams( this, Frame.Width, Frame.Height, ref mStyle ), DeviceHeight, touch );
                        UserNoteControls.Add( userNote );

                        userNote.AddToView( MasterView );
                    }
                }

                public void SaveState()
                {
                    // if we're waiting for our notes to load, don't allow saving! We'll
                    // save a blank state over our real notes!
                    if( LoadingNoteState == false )
                    {
                        // open a stream
                        using (StreamWriter writer = new StreamWriter(UserNotePath, false))
                        {
                            NoteState noteState = new NoteState( );

                            // User Notes
                            noteState.UserNoteContentList = new List<NoteState.UserNoteContent>( );
                            foreach( UserNote note in UserNoteControls )
                            {
                                noteState.UserNoteContentList.Add( note.GetContent( ) );
                            }
                            //


                            //Reveal Boxes
                            List<IUIControl> revealBoxes = new List<IUIControl>( );
                            GetControlOfType<RevealBox>( revealBoxes );

                            noteState.RevealBoxStateList = new List<NoteState.RevealBoxState>( );
                            foreach( RevealBox revealBox in revealBoxes )
                            {
                                noteState.RevealBoxStateList.Add( revealBox.GetState( ) );
                            }
                            //

                            // now we can serialize this and save it.
                            string json = JsonConvert.SerializeObject( noteState );
                            writer.WriteLine( json );
                        }
                    }
                }

                protected void LoadState( string filePath )
                {
                    // sanity check to make sure the notes were requested to load.
                    if( LoadingNoteState == true )
                    {
                        NoteState noteState = null;

                        // if the file exists
                        if(System.IO.File.Exists(filePath) == true)
                        {
                            // read it
                            using (StreamReader reader = new StreamReader(filePath))
                            {
                                // grab the stream that reprents a list of all their notes
                                string json = reader.ReadLine();

                                if( json != null )
                                {
                                    noteState = JsonConvert.DeserializeObject<NoteState>( json ) as NoteState;
                                }
                            }
                        }

                        if( noteState != null )
                        {
                            // restore each user note
                            foreach( NoteState.UserNoteContent note in noteState.UserNoteContentList )
                            {
                                // create the note, add it to our list, and to the view
                                UserNote userNote = new UserNote( new BaseControl.CreateParams( this, Frame.Width, Frame.Height, ref mStyle ), DeviceHeight, note );
                                UserNoteControls.Add( userNote );
                                userNote.AddToView( MasterView );
                            }

                            // collect all the reveal boxes and restore them
                            List<IUIControl> revealBoxes = new List<IUIControl>( );
                            GetControlOfType<RevealBox>( revealBoxes );

                            foreach(RevealBox revealBox in revealBoxes )
                            {
                                // for each reveal box, find its appropriate state object by matching the content text.
                                NoteState.RevealBoxState state = noteState.RevealBoxStateList.Find( rbs => rbs.Text == revealBox.Text );
                                if( state != null )
                                {
                                    revealBox.SetRevealed( state.Revealed );
                                }
                            }
                        }

                        LoadingNoteState = false;
                    }
                }

                public void GetNotesForEmail( out string htmlStream )
                {
                    // first setup the string that will contain the notes
                    htmlStream = "<!DOCTYPE html PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n<HTML><Body>\n";

                    // make a COPY of the user notes so the html stream generator can modify it
                    IUIControl[] userNotes = UserNoteControls.ToArray( );

                    List<IUIControl> userNoteListCopy = new List<IUIControl>( );
                    for(int i = 0; i < userNotes.Length; i++)
                    {
                        userNoteListCopy.Add( userNotes[i] );
                    }


                    // let each control recursively build its html stream.
                    // provide the user note list so it can embed user notes in the appropriate spots.
                    foreach( IUIControl control in ChildControls )
                    {
                        control.BuildHTMLContent( ref htmlStream, userNoteListCopy );
                    }

                    // if there happened to be any user note that wasn't already added, drop it in at the bottom
                    if( userNoteListCopy.Count > 0)
                    {
                        foreach( UserNote note in userNoteListCopy )
                        {
                            htmlStream += "UserNote - " + note.GetContent( ).Text + "\n";
                        }
                    }
                }
            }
        }
    }
}
