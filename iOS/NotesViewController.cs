
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using RockMobile.Network;
using Notes;
using System.Collections.Generic;

namespace CCVApp
{
    // create a subclass of UIScrollView so we can intercept its touch events
    public class CustomScrollView : UIScrollView
    {
        public UIViewController Interceptor { get; set; }

        public override void TouchesEnded( NSSet touches, UIEvent evt )
        {
            if( Interceptor != null )
            {
                Interceptor.TouchesEnded( touches, evt );
            }
        }
    }

    public partial class NotesViewController : UIViewController
    {
        /// <summary>
        /// Displays when content is being downloaded.
        /// </summary>
        /// <value>The indicator.</value>
        UIActivityIndicatorView Indicator { get; set; }

        /// <summary>
        /// Reloads the NoteScript
        /// </summary>
        /// <value>The refresh button.</value>
        UIButton RefreshButton { get; set; }

        /// <summary>
        /// Displays the actual Note content
        /// </summary>
        /// <value>The user interface scroll view.</value>
        CustomScrollView UIScrollView { get; set; }

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

        public NotesViewController( ) : base( "NotesViewController", null )
        {
        }

        public override void DidReceiveMemoryWarning( )
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning( );
			
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillLayoutSubviews( )
        {
            base.ViewWillLayoutSubviews( );
        }

        public override void ViewDidLayoutSubviews( )
        {
            base.ViewDidLayoutSubviews( );

            RefreshButton.Layer.Position = new PointF( View.Bounds.Width / 2, RefreshButton.Bounds.Height + 10 );

            UIScrollView.Frame = new RectangleF( 0, 0, View.Bounds.Width, View.Bounds.Height );
            UIScrollView.Layer.Position = new PointF( UIScrollView.Layer.Position.X, UIScrollView.Layer.Position.Y + RefreshButton.Frame.Bottom);

            Indicator.Layer.Position = new PointF( View.Bounds.Width / 2, View.Bounds.Height / 2 );

            // re-create our notes with the new dimensions
            string noteXml = null;
            string styleSheetXml = null;
            if( Note != null )
            {
                noteXml = Note.NoteXml;
                styleSheetXml = ControlStyles.StyleSheetXml;
            }
            CreateNotes( noteXml, styleSheetXml );
        }

        public override void ViewDidLoad( )
        {
            base.ViewDidLoad( );

            UIScrollView = new CustomScrollView( );
            UIScrollView.Interceptor = this;
            UIScrollView.Frame = View.Frame;
            UIScrollView.BackgroundColor = UIColor.Black;

            View.BackgroundColor = UIScrollView.BackgroundColor;
            View.AddSubview( UIScrollView );


            // add a busy indicator
            Indicator = new UIActivityIndicatorView( UIActivityIndicatorViewStyle.White );
            UIScrollView.AddSubview( Indicator );

            // add a refresh button for debugging
            RefreshButton = UIButton.FromType( UIButtonType.System );
            RefreshButton.SetTitle( "Refresh", UIControlState.Normal );
            RefreshButton.SizeToFit( );

            // if they tap the refresh button, refresh the list
            RefreshButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                CreateNotes( null, null );
            };
            View.AddSubview( RefreshButton );
        }

        public override void TouchesEnded( NSSet touches, UIEvent evt )
        {
            base.TouchesEnded( touches, evt );

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                if( Note != null )
                {
                    Note.TouchesEnded( touch.LocationInView( UIScrollView ) );
                }
            }
        }

        public void DestroyNotes( )
        {
            if( Note != null )
            {
                Note.Destroy( null );
                Note = null;
            }
        }

        public void CreateNotes( string noteXml, string styleSheetXml )
        {
            if( RefreshingNotes == false )
            {
                RefreshingNotes = true;

                DestroyNotes( );

                // show a busy indicator
                Indicator.StartAnimating( );

                // if we don't have BOTH xml strings, re-download
                if( noteXml == null || styleSheetXml == null )
                {
                    HttpWebRequest.Instance.MakeAsyncRequest( "http://www.jeredmcferron.com/sample_note.xml", (Exception ex, Dictionary<string, string> responseHeaders, string body ) =>
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
                InvokeOnMainThread( delegate
                    {
                        Note = note;

                        try
                        {
                            Note.Create( UIScrollView.Bounds.Width, UIScrollView.Bounds.Height );

                            //todo: we can't pass in an iOS type like this...
                            Note.AddToView( this.UIScrollView );

                            // take the requested background color
                            UIScrollView.BackgroundColor = Notes.PlatformUI.iOSLabel.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value );
                            View.BackgroundColor = UIScrollView.BackgroundColor; //Make the view itself match too

                            // update the height of the scroll view to fit all content
                            RectangleF frame = Note.GetFrame( );
                            UIScrollView.ContentSize = new SizeF( UIScrollView.Bounds.Width, frame.Size.Height + ( UIScrollView.Bounds.Height / 2 ) );

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
            Indicator.StopAnimating( );

            // flag that we're clear to refresh again
            RefreshingNotes = false;
        }

        protected void ReportException( string errorMsg, Exception e )
        {
            new NSObject( ).InvokeOnMainThread( delegate
                {
                    // explain that we couldn't generate notes
                    UIAlertView alert = new UIAlertView( );
                    alert.Title = "Note Error";
                    alert.Message = errorMsg + "\n" + e.Message;
                    alert.AddButton( "Ok" );
                    alert.Show( );

                    FinishNotesCreation( );
                } );
        }
    }
}

