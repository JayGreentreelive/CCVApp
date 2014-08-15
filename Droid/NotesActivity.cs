
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
using RockMobile.Network;
using Notes;
using System.Drawing;
using System.IO;

namespace Droid
{
    /// <summary>
    /// Double tap listener
    /// </summary>
    public class DoubleTap : GestureDetector.SimpleOnGestureListener
    {
        /// <summary>
        /// The notes activity that needs notification of a double tap.
        /// </summary>
        public NotesActivity NotesActivity { get; set; }

        public DoubleTap( NotesActivity notesActivity )
        {
            NotesActivity = notesActivity;
        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            return NotesActivity.OnDoubleTap( e );
        }
    }

    /// <summary>
    /// Subclass of Android's ScrollView to allow us to disable scrolling.
    /// </summary>
    public class LockableScrollView : ScrollView
    {
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

        public override bool PageScroll(FocusSearchDirection direction)
        {
            return base.PageScroll(direction);
        }

        public override void RequestChildFocus(View child, View focused)
        {
            base.RequestChildFocus(child, focused);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
        }

        public override View FindFocus()
        {
            return base.FindFocus();
        }

        public override bool OnTouchEvent( MotionEvent ev )
        {
            switch( ev.Action )
            {
                case MotionEventActions.Down:
                {
                    if( ScrollEnabled )
                    {
                        return base.OnTouchEvent( ev );
                    }
                    break;
                }
            }

            return base.OnTouchEvent( ev );
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if( ScrollEnabled == true )
            {
                return base.OnInterceptTouchEvent(ev);
            }
            return false;
        }
    }

    [Activity( Label = "NotesActivity" )]            
    public class NotesActivity : Activity, View.IOnTouchListener
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

        public bool OnDoubleTap(MotionEvent e)
        {
            Note.DidDoubleTap( new PointF( e.GetX( ), e.GetY( ) ) );
            return true;
        }

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            RockMobile.PlatformCommon.Droid.Context = this;

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Notes );

            // create our overridden lockable scroll view
            ScrollView = new LockableScrollView( this );
            ScrollView.ScrollBarStyle = ScrollbarStyles.InsideInset;
            ScrollView.OverScrollMode = OverScrollMode.Always;
            ScrollView.VerticalScrollbarPosition = ScrollbarPosition.Default;
            ScrollView.Focusable = false;
            ScrollView.FocusableInTouchMode = false;
            ScrollView.DescendantFocusability = DescendantFocusability.AfterDescendants;

            // add it to our main layout.
            LinearLayout layout = FindViewById<LinearLayout>( Resource.Id.linearLayout );
            layout.AddView( ScrollView );

            RefreshButton = FindViewById<Button>( Resource.Id.refreshButton );

            Indicator = FindViewById<ProgressBar>( Resource.Id.progressBar );
            Indicator.Visibility = ViewStates.Gone;

            ScrollViewLayout = new RelativeLayout( this );
            ScrollView.AddView( ScrollViewLayout );
            ScrollViewLayout.SetOnTouchListener( this );
            ScrollViewLayout.DescendantFocusability = DescendantFocusability.AfterDescendants;


            GestureDetector = new GestureDetector( this, new DoubleTap( this ) );

            RefreshButton.Click += (object sender, EventArgs e ) =>
            {
                CreateNotes( null, null );
            };

            // get our power management control
            PowerManager pm = PowerManager.FromContext( this );
            WakeLock = pm.NewWakeLock(WakeLockFlags.Full, "Notes");

            //
            NoteXml = null;
            StyleSheetXml = null;
            if( bundle != null )
            {
                NoteXml = bundle.GetString( XML_NOTE_KEY );
                StyleSheetXml = bundle.GetString( XML_STYLE_KEY );
            }
        }

        protected override void OnResume()
        {
            // when we're resuming, take a lock on the device sleeping to prevent it
            base.OnResume( );

            WakeLock.Acquire( );

            // create the notes
            CreateNotes( NoteXml, StyleSheetXml );
        }

        protected override void OnPause()
        {
            // when we're being backgrounded, release our lock so we don't force
            // the device to stay on
            base.OnPause( );

            WakeLock.Release( );

            // what can I say? If we are getting backgounded, android is going to destroy
            // our views, so we need to store off our XML and re-create the note
            // when we resume. Thanks android!
            if( Note != null )
            {
                NoteXml = Note.NoteXml;
                StyleSheetXml = ControlStyles.StyleSheetXml;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy( );

            // save the note state
            if( Note != null && RefreshingNotes == false )
            {
                Note.SaveState( );
            }
        }

        protected override void OnSaveInstanceState( Bundle outState )
        {
            base.OnSaveInstanceState( outState );

            // if we have a note and aren't in the middle of refreshing, store what we have.
            if( Note != null && RefreshingNotes == false )
            {
                Note.SaveState( );
                Note.Destroy( ScrollViewLayout );

                // store out xml in the bundle so we don't have to re-download it
                outState.PutString( XML_NOTE_KEY, Note.NoteXml );
                outState.PutString( XML_STYLE_KEY, ControlStyles.StyleSheetXml );
            }
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            if( GestureDetector.OnTouchEvent( e ) )
            {
                return true;
            }

            base.OnTouchEvent( e );

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

        public void DestroyNotes( )
        {
            if( Note != null )
            {
                Note.Destroy( ScrollViewLayout );
                Note = null;
            }
        }

        void CreateNotes( string noteXml, string styleSheetXml )
        {
            if( RefreshingNotes == false )
            {
                RefreshingNotes = true;

                if( Note != null )
                {
                    Note.SaveState( );
                }

                DestroyNotes( );

                // show a busy indicator
                Indicator.Visibility = ViewStates.Visible;

                // if we don't have BOTH xml strings, re-download
                if( noteXml == null || styleSheetXml == null )
                {
                    HttpWebRequest.Instance.MakeAsyncRequest( "http://www.jeredmcferron.com/sample_note.xml", ( Exception ex, Dictionary<string, string> responseHeaders, string body ) =>
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
                RunOnUiThread( delegate
                    {
                        Note = note;

                        try
                        {
                            // Use the metrics and not ScrollView for dimensions, because depending on when this gets called the ScrollView
                            // may not have its dimensions set yet.
                            Note.Create( this.Resources.DisplayMetrics.WidthPixels, this.Resources.DisplayMetrics.HeightPixels, ScrollViewLayout );

                            // set the requested background color
                            ScrollView.SetBackgroundColor( ( Android.Graphics.Color )RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value ) );

                            // update the height of the scroll view to fit all content
                            RectangleF frame = Note.GetFrame( );

                            int scrollFrameHeight = ( int )frame.Size.Height + ( this.Resources.DisplayMetrics.HeightPixels / 2 );
                            ScrollViewLayout.LayoutParameters.Height = scrollFrameHeight;

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
            RunOnUiThread( delegate
                {
                    AlertDialog.Builder dlgAlert = new AlertDialog.Builder( this );                      
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
