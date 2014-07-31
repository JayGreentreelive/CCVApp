
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
        ScrollView ScrollView { get; set; }

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

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            Notes.PlatformUI.DroidCommon.Context = this;

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Notes );

            ScrollView = FindViewById<ScrollView>( Resource.Id.scrollView );
            RefreshButton = FindViewById<Button>( Resource.Id.refreshButton );
            Indicator = FindViewById<ProgressBar>( Resource.Id.progressBar );
            Indicator.Visibility = ViewStates.Gone;

            ScrollViewLayout = new RelativeLayout( this );
            ScrollView.AddView( ScrollViewLayout );
            ScrollViewLayout.SetOnTouchListener( this );

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
            base.OnTouchEvent( e );

            switch( e.Action )
            {
                case MotionEventActions.Up:
                {
                    if( Note != null )
                    {
                        Note.TouchesEnded( new PointF( e.GetX( ), e.GetY( ) ) );
                    }

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
                            Note.Create( this.Resources.DisplayMetrics.WidthPixels, this.Resources.DisplayMetrics.HeightPixels );
                            Note.AddToView( ScrollViewLayout );

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
