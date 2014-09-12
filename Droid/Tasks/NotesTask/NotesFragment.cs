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

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            /// <summary>
            /// Double tap listener
            /// </summary>
            public class DoubleTap : GestureDetector.SimpleOnGestureListener
            {
                /// <summary>
                /// The notes activity that needs notification of a double tap.
                /// </summary>
                public NotesFragment Notes { get; set; }

                public DoubleTap( NotesFragment notes )
                {
                    Notes = notes;
                }

                public override bool OnDoubleTap(MotionEvent e)
                {
                    return Notes.OnDoubleTap( e );
                }
            }

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
            }
                       
            public class NotesFragment : TaskFragment, View.IOnTouchListener
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
                /// Used for notification of a double tap.
                /// </summary>
                GestureDetector GestureDetector { get; set; }

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

                public bool OnDoubleTap(MotionEvent e)
                {
                    Note.DidDoubleTap( new PointF( e.GetX( ), e.GetY( ) ) );
                    return true;
                }

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


                    GestureDetector = new GestureDetector( Rock.Mobile.PlatformCommon.Droid.Context, new DoubleTap( this ) );

                    RefreshButton.Click += (object sender, EventArgs e ) =>
                    {
                        CreateNotes( null, null );
                    };

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

                    // create the notes
                    CreateNotes( NoteXml, StyleSheetXml );
                }

                public override void OnPause()
                {
                    // when we're being backgrounded, release our lock so we don't force
                    // the device to stay on
                    base.OnPause( );

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

                public bool OnInterceptTouchEvent(MotionEvent ev)
                {
                    // called by the LockableScrollView. This allows us to shut the
                    // springboard if it's open and the user touches the note.
                    if( ParentTask.NavbarFragment.SpringboardRevealed == true )
                    {
                        ParentTask.NavbarFragment.RevealSpringboard( false );
                        return false;
                    }
                    return true;
                }

                public override bool OnTouch( View v, MotionEvent e )
                {
                    if( GestureDetector.OnTouchEvent( e ) )
                    {
                        return true;
                    }

                    switch( e.Action )
                    {
                        case MotionEventActions.Down:
                        {
                            if( Note != null )
                            {
                                if( Note.TouchesBegan( new PointF( e.GetX( ), e.GetY( ) ) ) )
                                {
                                    ScrollView.ScrollEnabled = false;
                                }
                            }

                            break;
                        }

                        case MotionEventActions.Move:
                        {
                            if( Note != null )
                            {
                                Note.TouchesMoved( new PointF( e.GetX( ), e.GetY( ) ) );
                            }

                            break;
                        }

                        case MotionEventActions.Up:
                        {
                            if( Note != null )
                            {
                                Note.TouchesEnded( new PointF( e.GetX( ), e.GetY( ) ) );
                            }

                            ScrollView.ScrollEnabled = true;

                            break;
                        }
                    }
                    return true;
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
                            HttpWebRequest.Instance.MakeAsyncRequest( CCVApp.Shared.Config.Note.BaseURL + NoteName + CCVApp.Shared.Config.Note.Extension, ( Exception ex, Dictionary<string, string> responseHeaders, string body ) =>
                                {
                                    if( ex == null )
                                    {
                                        HandleNotePreReqs( body, null );
                                    }
                                    else
                                    {
                                        ReportException( "NoteScript Download Error", ex );
                                    }
                                } );
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
                                    // Use the metrics and not ScrollView for dimensions, because depending on when this gets called the ScrollView
                                    // may not have its dimensions set yet.
                                    Note.Create( this.Resources.DisplayMetrics.WidthPixels, this.Resources.DisplayMetrics.HeightPixels, ScrollViewLayout, NoteName + CCVApp.Shared.Config.Note.UserNoteSuffix );

                                    // set the requested background color
                                    ScrollView.SetBackgroundColor( ( Android.Graphics.Color )Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value ) );

                                    // update the height of the scroll view to fit all content
                                    RectangleF frame = Note.GetFrame( );

                                    int scrollFrameHeight = ( int )frame.Size.Height + ( this.Resources.DisplayMetrics.HeightPixels / 2 );
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
                    Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                        {
                            AlertDialog.Builder dlgAlert = new AlertDialog.Builder( Rock.Mobile.PlatformCommon.Droid.Context );                      
                            dlgAlert.SetTitle( "Note Error" ); 
                            dlgAlert.SetMessage( errorMsg + "\n" + e.Message ); 
                            dlgAlert.SetPositiveButton( "Ok", delegate(object sender, DialogClickEventArgs ev )
                                {
                                } );
                            dlgAlert.Create( ).Show( );

                            FinishNotesCreation( );
                        } );
                }
            }
        }
    }
}
