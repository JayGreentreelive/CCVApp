using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Drawing;
using System.IO;

using Rock.Mobile.Network;
using CCVApp.Shared.Notes;
using RestSharp;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            /// <summary>
            /// Subclass of Android's ScrollView to allow us to disable scrolling.
            /// </summary>
            public class LockableScrollView : ScrollView
            {
                /// <summary>
                /// The Notes Fragment, so we can notify on input
                /// </summary>
                /// <value>The notes.</value>
                public NotesFragment Notes { get; set; }

                /// <summary>
                /// True when the scroll view can scroll. False when it cannot.
                /// </summary>
                public bool ScrollEnabled { get; set; }

                public LockableScrollView( Context c ) : base( c )
                {
                    ScrollEnabled = true;
                }

                //This is a total hack but it works perfectly.
                //For some reason, when focus is changed to a text element, the
                //RelativeView gets focus first. Since it's at 0, 0,
                //The scrollView wants to scroll to the TOP, then when the editText
                //gets focus, it jumps back down to its position.
                // This is not an acceptable long term solution, but I really need to move on right now.
                public override void ScrollTo(int x, int y)
                {
                    //base.ScrollTo(x, y);
                }

                public override void ScrollBy(int x, int y)
                {
                    //base.ScrollBy(x, y);
                }

                public override bool OnInterceptTouchEvent(MotionEvent ev)
                {
                    // verify from our parent we can scroll, and that scrolling is enabled
                    if( Notes.OnInterceptTouchEvent( ev ) && ScrollEnabled == true )
                    {
                        return base.OnInterceptTouchEvent(ev);
                    }

                    return false;
                }

                protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
                {
                    base.OnScrollChanged(l, t, oldl, oldt);

                    Notes.OnScrollChanged( t - oldt );
                }
            }
                       
            public class NotesFragment : TaskFragment
            {
                /// <summary>
                /// Tags for storing the NoteScript and Style XML during an orientation change.
                /// </summary>
                const string XML_NOTE_KEY = "NOTE_XML";
                const string XML_STYLE_KEY = "STYLE_XML";

                /// <summary>
                /// Reloads the NoteScript
                /// </summary>
                /// <value>The refresh button.</value>
                Button RefreshButton { get; set; }

                /// <summary>
                /// Displays the Note
                /// </summary>
                /// <value>The scroll view.</value>
                LockableScrollView ScrollView { get; set; }

                /// <summary>
                /// Immediate child of the ScrollView. Parent to all content
                /// </summary>
                /// <value>The scroll view layout.</value>
                RelativeLayout ScrollViewLayout { get; set; }

                /// <summary>
                /// Displays when content is being downloaded.
                /// </summary>
                /// <value>The indicator.</value>
                ProgressBar Indicator { get; set; }

                /// <summary>
                /// True when notes are being refreshed to prevent multiple simultaneous downloads.
                /// </summary>
                /// <value><c>true</c> if refreshing notes; otherwise, <c>false</c>.</value>
                bool RefreshingNotes { get; set; }

                /// <summary>
                /// Actual Note object created by a NoteScript
                /// </summary>
                /// <value>The note.</value>
                Note Note { get; set; }

                /// <summary>
                /// Our wake lock that will keep the device from sleeping while notes are up.
                /// </summary>
                /// <value>The wake lock.</value>
                PowerManager.WakeLock WakeLock { get; set; }

                /// <summary>
                /// reference to the note XML for re-creating the notes in OnResume()
                /// </summary>
                /// <value>The note XM.</value>
                string NoteXml { get; set; }

                /// <summary>
                /// reference to the style XML for re-creating the notes in OnResume()
                /// </summary>
                /// <value>The note XM.</value>
                string StyleSheetXml { get; set; }

                /// <summary>
                /// The URL for this note
                /// </summary>
                /// <value>The note URL.</value>
                public string NoteName { get; set; }

                /// <summary>
                /// A presentable name for the note. Used for things like email subjects
                /// </summary>
                /// <value>The name of the note presentable.</value>
                public string NotePresentableName { get; set; }

                /// <summary>
                /// True when a user note is being moved. Used to know whether to allow 
                /// panning for the springboard or not
                /// </summary>
                /// <value><c>true</c> if moving user note; otherwise, <c>false</c>.</value>
                public bool MovingUserNote { get; protected set; }

                /// <summary>
                /// True when WE are ready to create notes
                /// </summary>
                /// <value><c>true</c> if fragment ready; otherwise, <c>false</c>.</value>
                bool FragmentReady { get; set; }

                /// <summary>
                /// True when a gesture causes a user note to be created. We then know not to 
                /// pass further gesture input to the note until we receive TouchUp
                /// </summary>
                /// <value><c>true</c> if did gesture create note; otherwise, <c>false</c>.</value>
                bool DidGestureCreateNote { get; set; }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    base.OnCreateView( inflater, container, savedInstanceState );

                    // get the root control from our .axml
                    var layout = inflater.Inflate(Resource.Layout.Notes, container, false) as RelativeLayout;

                    // get the refresh button from the layout
                    RefreshButton = layout.FindViewById<Button>( Resource.Id.refreshButton );

                    // create our overridden lockable scroll view
                    ScrollView = new LockableScrollView( Rock.Mobile.PlatformCommon.Droid.Context );
                    ScrollView.ScrollBarStyle = ScrollbarStyles.InsideInset;
                    ScrollView.OverScrollMode = OverScrollMode.Always;
                    ScrollView.VerticalScrollbarPosition = ScrollbarPosition.Default;
                    ScrollView.LayoutParameters = new RelativeLayout.LayoutParams( RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                    ScrollView.Notes = this;
                    ((RelativeLayout.LayoutParams)ScrollView.LayoutParameters).AddRule(LayoutRules.CenterHorizontal);
                    ((RelativeLayout.LayoutParams)ScrollView.LayoutParameters).AddRule(LayoutRules.Below, Resource.Id.refreshButton);

                    // add it to our main layout.
                    layout.AddView( ScrollView );


                    Indicator = layout.FindViewById<ProgressBar>( Resource.Id.progressBar );
                    Indicator.Visibility = ViewStates.Gone;
                    Indicator.SetBackgroundColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( 0 ) );
                    Indicator.BringToFront();

                    // create the layout that will contain the notes
                    ScrollViewLayout = new RelativeLayout( Rock.Mobile.PlatformCommon.Droid.Context );
                    ScrollView.AddView( ScrollViewLayout );
                    ScrollViewLayout.SetOnTouchListener( this );

                    RefreshButton.Click += (object sender, EventArgs e ) =>
                    {
                        CreateNotes( null, null );
                    };

                    #if !DEBUG
                    RefreshButton.Visibility = ViewStates.Gone;
                    #endif

                    // get our power management control
                    PowerManager pm = PowerManager.FromContext( Rock.Mobile.PlatformCommon.Droid.Context );
                    WakeLock = pm.NewWakeLock(WakeLockFlags.Full, "Notes");

                    //
                    NoteXml = null;
                    StyleSheetXml = null;
                    if( savedInstanceState != null )
                    {
                        NoteXml = savedInstanceState.GetString( XML_NOTE_KEY );
                        StyleSheetXml = savedInstanceState.GetString( XML_STYLE_KEY );
                    }

                    return layout;
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    if( newConfig.Orientation == Android.Content.Res.Orientation.Landscape )
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( false );
                    }
                    else
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );
                    }

                    CreateNotes( NoteXml, StyleSheetXml );
                }

                public override void OnResume()
                {
                    // when we're resuming, take a lock on the device sleeping to prevent it
                    base.OnResume( );

                    Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;

                    WakeLock.Acquire( );


                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( true, 
                        delegate 
                        {
                            Intent sendIntent = new Intent();
                            sendIntent.SetAction( Intent.ActionSend );

                            //todo: build a nice subject line
                            sendIntent.PutExtra( Intent.ExtraSubject, NotePresentableName );

                            string noteString = null;
                            Note.GetNotesForEmail( out noteString );

                            sendIntent.PutExtra( Intent.ExtraText, Android.Text.Html.FromHtml( noteString ) );
                            sendIntent.SetType( "text/html" );
                            StartActivity( sendIntent );
                        } );

                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // if the task is ready, go ahead and create the notes. Alternatively, 
                    // if we are resuming from a pause, it's safe to create the notes. If we don't,
                    // the user will see a blank screen.
                    FragmentReady = true;
                    if( ParentTask.TaskReadyForFragmentDisplay == true )
                    {
                        CreateNotes( NoteXml, StyleSheetXml );
                    }
                }

                public void TaskReadyForFragmentDisplay()
                {
                    // our parent task is letting us know it's ready.
                    // if we've had our OnResume called, then we're ready too.

                    // Otherwise, when OnResume IS called, the parent task will be ready
                    // and we'll create the notes then.
                    if( FragmentReady == true )
                    {
                        CreateNotes( NoteXml, StyleSheetXml );
                    }
                }

                public override void OnPause()
                {
                    // when we're being backgrounded, release our lock so we don't force
                    // the device to stay on
                    base.OnPause( );

                    FragmentReady = false;

                    WakeLock.Release( );

                    Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

                    ShutdownNotes( null );
                }

                public override void OnSaveInstanceState( Bundle outState )
                {
                    base.OnSaveInstanceState( outState );

                    ShutdownNotes( outState );
                }

                public override void OnDestroy()
                {
                    base.OnDestroy( );

                    ShutdownNotes( null );
                }

                public void OnScrollChanged( float scrollDelta )
                {
                    // did the user's finger go "up"?
                    if( scrollDelta >= CCVApp.Shared.Config.Note.ScrollRateForNavBarHide )
                    {
                        // hide the nav bar
                        ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                    }
                    // did the user scroll "down"? Android is a little less sensitive, so use 75% of it.
                    else if ( scrollDelta <= (CCVApp.Shared.Config.Note.ScrollRateForNavBarReveal * CCVApp.Shared.Config.Note.ScrollRateForNavBarReveal_AndroidScalar) )
                    {
                        ParentTask.NavbarFragment.NavToolbar.Reveal( true );
                    }
                }

                public bool OnInterceptTouchEvent(MotionEvent ev)
                {
                    // called by the LockableScrollView. This allows us to shut the
                    // springboard if it's open and the user touches the note.
                    if( ParentTask.NavbarFragment.ShouldSpringboardAllowInput( ) )
                    {
                        ParentTask.NavbarFragment.RevealSpringboard( false );
                        return false;
                    }
                    return true;
                }

                public override bool OnDoubleTap(MotionEvent e)
                {
                    // a double tap CAN create a user note. If it did,
                    // we want to know that so we suppress further input until we receive
                    // TouchUp
                    DidGestureCreateNote = Note.DidDoubleTap( new PointF( e.GetX( ), e.GetY( ) ) );

                    return true;
                }

                public override bool OnDownGesture( MotionEvent e )
                {
                    // only processes TouchesBegan if we didn't create a note with this gesture.
                    if ( DidGestureCreateNote == false )
                    {
                        if ( Note != null )
                        {
                            if ( Note.TouchesBegan( new PointF( e.GetX( ), e.GetY( ) ) ) )
                            {
                                ScrollView.ScrollEnabled = false;

                                MovingUserNote = true;
                            }
                        }
                    }
                    return false;
                }

                public override bool OnScrollGesture( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
                {
                    // if we're moving a user note, consume the input so that the 
                    // springboard doesn't receive this, and thus doesn't pan.
                    if ( MovingUserNote == true )
                    {
                        return true;
                    }
                    else
                    {
                        return base.OnScrollGesture( e1, e2, distanceX, distanceY );
                    }
                }

                public override bool OnTouch( View v, MotionEvent e )
                {
                    if ( base.OnTouch( v, e ) == true )
                    {
                        return true;
                    }
                    else
                    {
                        switch ( e.Action )
                        {
                            case MotionEventActions.Move:
                            {
                                // if at any point during a move the task is no longer allowed to receive input,
                                // STOP SCROLLING. It means the user began panning out the view
                                if ( ParentTask.NavbarFragment.ShouldTaskAllowInput( ) == false )
                                {
                                    ScrollView.ScrollEnabled = false;
                                }

                                if ( Note != null )
                                {
                                    Note.TouchesMoved( new PointF( e.GetX( ), e.GetY( ) ) );
                                }

                                break;
                            }

                            case MotionEventActions.Up:
                            {
                                if ( Note != null )
                                {
                                    Note.TouchesEnded( new PointF( e.GetX( ), e.GetY( ) ) );
                                }

                                ScrollView.ScrollEnabled = true;
                                MovingUserNote = false;

                                DidGestureCreateNote = false;

                                break;
                            }
                        }
                    }
                    return false;
                }

                public void ShutdownNotes( Bundle instanceBundle )
                {
                    // shutdown notes ensures that user settings are saved
                    // the note is destroyed and references to it are cleared.
                    if( Note != null )
                    {
                        Note.SaveState( );

                        Note.Destroy( ScrollViewLayout );
                        Note = null;
                    }

                    // if a bundle was provided, store the note XML in it
                    // for fast reloading.
                    if( instanceBundle != null )
                    {
                        // store out xml in the bundle so we don't have to re-download it
                        instanceBundle.PutString( XML_NOTE_KEY, NoteXml );
                        instanceBundle.PutString( XML_STYLE_KEY, StyleSheetXml );
                    }
                }

                void CreateNotes( string noteXml, string styleSheetXml )
                {
                    if( RefreshingNotes == false )
                    {
                        RefreshingNotes = true;

                        ShutdownNotes( null );

                        // show a busy indicator
                        Indicator.Visibility = ViewStates.Visible;

                        // if we don't have BOTH xml strings, re-download
                        if( noteXml == null || styleSheetXml == null )
                        {
                            //download the notes
                            Rock.Mobile.Network.HttpRequest request = new HttpRequest();
                            RestRequest restRequest = new RestRequest( Method.GET );
                            restRequest.RequestFormat = DataFormat.Xml;

                            request.ExecuteAsync( NoteName, restRequest, 
                                delegate(System.Net.HttpStatusCode statusCode, string statusDescription, byte[] model )
                                {
                                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                    {
                                        string body = Encoding.UTF8.GetString( model, 0, model.Length );
                                        HandleNotePreReqs( body, null );
                                    }
                                    else
                                    {
                                        ReportException( "NoteScript Download Error", null );
                                    }
                                } );

                            /*WebRequest.MakeAsyncRequest( CCVApp.Shared.Config.Note.BaseURL + NoteName + CCVApp.Shared.Config.Note.Extension, ( Exception ex, Dictionary<string, string> responseHeaders, string body ) =>
                                {
                                    if( ex == null )
                                    {
                                        HandleNotePreReqs( body, null );
                                    }
                                    else
                                    {
                                        ReportException( "NoteScript Download Error", ex );
                                    }
                                } );*/
                        }
                        else
                        {
                            // if we DO have both, go ahead and create with them.
                            HandleNotePreReqs( noteXml, styleSheetXml );
                        }
                    }
                }

                protected void HandleNotePreReqs( string noteXml, string styleXml )
                {
                    try
                    {
                        Note.HandlePreReqs( noteXml, styleXml, OnPreReqsComplete );
                    } 
                    catch( Exception e )
                    {
                        ReportException( "StyleSheet Error", e );
                    }
                }

                protected void OnPreReqsComplete( Note note, Exception e )
                {
                    if( e != null )
                    {
                        ReportException( "StyleSheet Error", e );
                    }
                    else
                    {
                        Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                            {
                                Note = note;

                                try
                                {
                                    // build the filename of the locally stored user data. If there is no "/" because it isn't a URL,
                                    // we'll end up using the base name, which is what we want.
                                    int lastSlashIndex = NoteName.LastIndexOf( "/" ) + 1;
                                    string noteName = NoteName.Substring( lastSlashIndex );

                                    // Use the metrics and not ScrollView for dimensions, because depending on when this gets called the ScrollView
                                    // may not have its dimensions set yet.
                                    Note.Create( this.Resources.DisplayMetrics.WidthPixels, this.Resources.DisplayMetrics.HeightPixels, ScrollViewLayout, noteName + CCVApp.Shared.Config.Note.UserNoteSuffix );


                                    // set the requested background color
                                    ScrollView.SetBackgroundColor( ( Android.Graphics.Color )Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value ) );

                                    // update the height of the scroll view to fit all content
                                    RectangleF frame = Note.GetFrame( );

                                    int scrollFrameHeight = ( int )frame.Size.Height + ( this.Resources.DisplayMetrics.HeightPixels / 3 );
                                    ScrollViewLayout.LayoutParameters.Height = scrollFrameHeight;

                                    // store the downloaded note and style xml
                                    NoteXml = Note.NoteXml;
                                    StyleSheetXml = ControlStyles.StyleSheetXml;

                                    FinishNotesCreation( );
                                } 
                                catch( Exception ex )
                                {
                                    ReportException( "NoteScript Error", ex );
                                }
                            } );
                    }
                }

                void FinishNotesCreation( )
                {
                    Indicator.Visibility = ViewStates.Gone;

                    // flag that we're clear to refresh again
                    RefreshingNotes = false;
                }

                protected void ReportException( string errorMsg, Exception e )
                {
                    if ( e != null )
                    {
                        errorMsg += e.Message;
                    }

                    Springboard.DisplayError( "Note Error", errorMsg );

                    Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                        {
                            FinishNotesCreation( );
                        } );
                }
            }
        }
    }
}
