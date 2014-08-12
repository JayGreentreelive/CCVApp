
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

namespace Droid
{
    public class DoubleTap : GestureDetector.SimpleOnGestureListener
    {
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

    public class LockableScrollView : ScrollView
    {
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

        GestureDetector GestureDetector { get; set; }

        public bool OnDoubleTap(MotionEvent e)
        {
            Note.DidDoubleTap( new PointF( e.GetX( ), e.GetY( ) ) );
            return true;
        }

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            Notes.PlatformUI.DroidCommon.Context = this;

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

            //This is here for reference, in case one of these isn't the default for a scroll view.
            /*<Droid.Droid.LockableScrollView
                android:layout_width="match_parent"
                android:layout_height="match_parent"
                android:scrollbars="vertical"*/

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

            string noteXml = null;
            string styleSheetXml = null;
            if( bundle != null )
            {
                noteXml = bundle.GetString( XML_NOTE_KEY );
                styleSheetXml = bundle.GetString( XML_STYLE_KEY );
            }

            CreateNotes( noteXml, styleSheetXml );
        }

        protected override void OnSaveInstanceState( Bundle outState )
        {
            base.OnSaveInstanceState( outState );

            // if we have a note and aren't in the middle of refreshing, store what we have.
            if( Note != null && RefreshingNotes == false )
            {
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
                            ScrollView.SetBackgroundColor( ( Android.Graphics.Color )Notes.PlatformUI.DroidLabel.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value ) );

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
