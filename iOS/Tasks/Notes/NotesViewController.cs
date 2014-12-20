using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.IO;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;

using Rock.Mobile.Network;
using CCVApp.Shared.Notes;
using RestSharp;
using System.Net;
using System.Text;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using Rock.Mobile.PlatformCommon;

namespace iOS
{
    // create a subclass of UIScrollView so we can intercept its touch events
    public class CustomScrollView : UIScrollView
    {
        public NotesViewController Interceptor { get; set; }

        // UIScrollView will check for scrolling and suppress touchesBegan
        // if the user is scrolling. We want to allow our controls to consume it
        // before that.
        public override UIView HitTest(PointF point, UIEvent uievent)
        {
            // transform the point into absolute coords (as if there was no scrolling)
            PointF absolutePoint = new PointF( ( point.X - ContentOffset.X ) + Frame.Left,
                                               ( point.Y - ContentOffset.Y ) + Frame.Top );

            if ( Frame.Contains( absolutePoint ) )
            {
                // Base OS controls need to know whether to process & consume
                // input or pass it up to the higher level (us.)
                // We decide that based on whether the HitTest intersects any of our controls.
                // By returning true, it can know "Yes, this hits something we need to know about"
                // and it will result in us receiving TouchBegan
                if( Interceptor.HitTest( point ) )
                {
                    return null;
                }
            }
            return base.HitTest(point, uievent);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            if( Interceptor != null )
            {
                Interceptor.TouchesBegan( touches, evt );
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            if( Interceptor != null )
            {
                Interceptor.TouchesMoved( touches, evt );
            }
        }

        public override void TouchesEnded( NSSet touches, UIEvent evt )
        {
            if( Interceptor != null )
            {
                Interceptor.TouchesEnded( touches, evt );
            }
        }
    }

    public class NotesScrollViewDelegate : UIScrollViewDelegate
    {
        public NotesViewController Parent { get; set; }

        PointF LastPos { get; set; }

        double LastTime { get; set; }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            LastTime = NSDate.Now.SecondsSinceReferenceDate;
            LastPos = scrollView.ContentOffset;
        }

        public override void Scrolled( UIScrollView scrollView )
        {
            double timeLapsed = NSDate.Now.SecondsSinceReferenceDate - LastTime;

            if( timeLapsed > .10f )
            {
                float delta = scrollView.ContentOffset.Y - LastPos.Y;

                // notify our parent
                Parent.ViewDidScroll( delta );

                LastTime = NSDate.Now.SecondsSinceReferenceDate;
                LastPos = scrollView.ContentOffset;
            }
        }
    }

    public partial class NotesViewController : TaskUIViewController
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
        /// The current orientation of the device. We track this
        /// so we can know when it changes and only rebuild the notes then.
        /// </summary>
        /// <value>The orientation.</value>
		UIDeviceOrientation Orientation { get; set; }

        /// <summary>
        /// A list of the handles returned when adding observers to OS events
        /// </summary>
        /// <value>The observer handles.</value>
        List<NSObject> ObserverHandles { get; set; }

        /// <summary>
        /// The manager that ensures views being edited are visible when the keyboard comes up.
        /// </summary>
        /// <value>The keyboard adjust manager.</value>
        KeyboardAdjustManager KeyboardAdjustManager { get; set; }

        /// <summary>
        /// The amount of times to try downloading the note before
        /// reporting an error to the user (which should be our last resort)
        /// We set it to 0 in debug because that means we WANT the error, as
        /// the user could be working on notes and need the error.
        /// </summary>
        #if DEBUG
        static int MaxDownloadAttempts = 0;
        #else
        static int MaxDownloadAttempts = 5;
        #endif

        /// <summary>
        /// The currently loaded Note XML, so that we can avoid downloading 
        /// when we don't have to
        /// </summary>
        string CachedNoteXml { get; set; }

        /// <summary>
        /// The currently loaded Style XML, so that we can avoid downloading when
        /// we don't have to
        /// </summary>
        string CachedStyleXml { get; set; }

        /// <summary>
        /// The currently cached note name
        /// </summary>
        string CachedNoteName { get; set; }

        /// <summary>
        /// The amount of times we've attempted to download the current note.
        /// When it hits 0, we'll just fail out and tell the user to check their network settings.
        /// </summary>
        /// <value>The note download retries.</value>
        int NoteDownloadRetries { get; set; }

        public NotesViewController( ) : base( )
        {
            ObserverHandles = new List<NSObject>();
        }

        public override void DidReceiveMemoryWarning( )
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning( );

            // Release any cached data, images, etc that aren't in use.
        }

        protected void SaveNoteState( )
        {
            // request quick backgrounding so we can save our user notes
            int taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            if( Note != null )
            {
                Note.SaveState( );
            }

            UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }

        public override void ViewDidLayoutSubviews( )
        {
            base.ViewDidLayoutSubviews( );

			if (Orientation != UIDevice.CurrentDevice.Orientation) 
			{
				Orientation = UIDevice.CurrentDevice.Orientation;

				//note: the frame height of the nav bar is what it CURRENTLY is, not what it WILL be after we rotate. So, when we go from Portrait to Landscape,
				// it says 40, but it's gonna be 32. Conversely, going back, we use 32 and it's actually 40, which causes us to start this view 8px too high.
                #if DEBUG
				RefreshButton.Layer.Position = new PointF (View.Bounds.Width / 2, (RefreshButton.Frame.Height / 2));

				UIScrollView.Frame = new RectangleF (0, 0, View.Bounds.Width, View.Bounds.Height - RefreshButton.Frame.Height );
				UIScrollView.Layer.Position = new PointF (UIScrollView.Layer.Position.X, UIScrollView.Layer.Position.Y + RefreshButton.Frame.Bottom);
                #else
                UIScrollView.Frame = new RectangleF (0, 0, View.Bounds.Width, View.Bounds.Height );
                UIScrollView.Layer.Position = new PointF (UIScrollView.Layer.Position.X, UIScrollView.Layer.Position.Y );
                #endif

				Indicator.Layer.Position = new PointF (View.Bounds.Width / 2, View.Bounds.Height / 2);

				// re-create our notes with the new dimensions
				CreateNotes (CachedNoteXml, CachedStyleXml);
			}

            // don't let the back button work when in landscape mode.
            Task.NavToolbar.SetBackButtonEnabled( UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.Portrait ? true : false );
        }

        public override void ViewDidLoad( )
        {
            base.ViewDidLoad( );

            Orientation = UIDeviceOrientation.Unknown;

            UIScrollView = new CustomScrollView();
            UIScrollView.Interceptor = this;
            UIScrollView.Frame = View.Frame;
            UIScrollView.BackgroundColor = PlatformBaseUI.GetUIColor( 0x1C1C1CFF );
            UIScrollView.Delegate = new NotesScrollViewDelegate() { Parent = this };
            UIScrollView.Layer.AnchorPoint = new PointF( 0, 0 );

            UITapGestureRecognizer tapGesture = new UITapGestureRecognizer();
            tapGesture.NumberOfTapsRequired = 2;
            tapGesture.AddTarget( this, new MonoTouch.ObjCRuntime.Selector( "DoubleTapSelector:" ) );
            UIScrollView.AddGestureRecognizer( tapGesture );

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

            KeyboardAdjustManager = new KeyboardAdjustManager( View, UIScrollView );

            #if DEBUG
            View.AddSubview( RefreshButton );
            #endif
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // since we're reappearing, we know we're safe to reset our download count
            NoteDownloadRetries = MaxDownloadAttempts;
            Console.WriteLine( "Resetting Download Attempts" );

            // if the note name doesn't match our cache, dump our cache 
            // so we download the correct note
            if ( CachedNoteName != NoteName )
            {
                CachedNoteXml = null;
                CachedStyleXml = null;

                CreateNotes( CachedNoteXml, CachedStyleXml );
            }

            // don't let the back button work when in landscape mode.
            Task.NavToolbar.SetBackButtonEnabled( UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.Portrait ? true : false );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            UIApplication.SharedApplication.IdleTimerDisabled = true;

            // monitor for text field being edited, and keyboard show/hide notitications
            NSObject handle = NSNotificationCenter.DefaultCenter.AddObserver (KeyboardAdjustManager.TextFieldDidBeginEditingNotification, KeyboardAdjustManager.OnTextFieldDidBeginEditing);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (KeyboardAdjustManager.TextFieldChangedNotification, KeyboardAdjustManager.OnTextFieldChanged);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            foreach ( NSObject handle in ObserverHandles )
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver( handle );
            }

            ObserverHandles.Clear( );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ViewResigning( );
        }

        /// <summary>
        /// Called when the view will dissapear, or when the task sees that the app is going into the background.
        /// </summary>
        public void ViewResigning()
        {
            SaveNoteState( );
            UIApplication.SharedApplication.IdleTimerDisabled = false;
        }

        public void ShareNotes()
        {
            if ( Note != null )
            {
                string emailNote;
                Note.GetNotesForEmail( out emailNote );

                var items = new NSObject[] { new NSString( emailNote ) };

                UIActivityViewController shareController = new UIActivityViewController( items, null );
                shareController.SetValueForKey( new NSString( NotePresentableName ), new NSString( "subject" ) );

                shareController.ExcludedActivityTypes = new NSString[] { UIActivityType.PostToFacebook, 
                                                                         UIActivityType.AirDrop, 
                                                                         UIActivityType.PostToTwitter, 
                                                                         UIActivityType.CopyToPasteboard, 
                                                                         UIActivityType.Message };

                PresentViewController( shareController, true, null );
            }
        }

        public bool HitTest( PointF point )
        {
            if( Note != null )
            {
                // Base OS controls need to know whether to process & consume
                // input or pass it up to the higher level (us.)
                // We decide that based on whether the HitTest intersects any of our controls.
                // By returning true, it can know "Yes, this hits something we need to know about"
                // and it will result in us receiving TouchBegan
                if( Note.HitTest( point ) == true )
                {
                    return true;
                }
            }

            return false;
        }

        public bool HandleTouchBegan( PointF point )
        {
            if( Note != null )
            {
                // if the note consumed touches Began, don't allow the UIScroll View to scroll.
                if( Note.TouchesBegan( point ) == true )
                {
                    UIScrollView.ScrollEnabled = false;
                    return true;
                }
            }

            return false;
        }

        public bool TouchingUserNote( NSSet touches, UIEvent evt )
        {
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && Note != null)
            {
                return Note.TouchingUserNote( touch.LocationInView( UIScrollView ) );
            }

            return false;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            Console.WriteLine( "Touches Began" );

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                HandleTouchBegan( touch.LocationInView( UIScrollView ) );
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved( touches, evt );

            Console.WriteLine( "Touches MOVED" );

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                if( Note != null )
                {
                    Note.TouchesMoved( touch.LocationInView( UIScrollView ) );
                }
            }
        }

        public override void TouchesEnded( NSSet touches, UIEvent evt )
        {
            base.TouchesEnded( touches, evt );

            Console.WriteLine( "Touches Ended" );

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                if( Note != null )
                {
                    // should we visit a website?
                    string activeUrl = Note.TouchesEnded( touch.LocationInView( UIScrollView ) );
                    if ( string.IsNullOrEmpty( activeUrl ) == false )
                    {
                        NotesWebViewController viewController = new NotesWebViewController( );
                        viewController.ActiveUrl = activeUrl;
                        Task.PerformSegue( this, viewController );
                    }
                }
            }

            // when a touch is released, re-enabled scrolling
            UIScrollView.ScrollEnabled = true;
        }

        public void ViewDidScroll( float scrollDelta )
        {
            // notify our task that fast scrolling was detected
            Task.ViewDidScroll( scrollDelta );
        }

        [MonoTouch.Foundation.Export("DoubleTapSelector:")]
        public void HandleTapGesture(UITapGestureRecognizer tap)
        {
            if( Note != null )
            {
                if( tap.State == UIGestureRecognizerState.Ended )
                {
                    Note.DidDoubleTap( tap.LocationInView( UIScrollView ) );
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
                // if we're recreating the notes, reset our scrollview.
                UIScrollView.ContentOffset = PointF.Empty;

                Console.WriteLine( "CREATING NOTES" );

                RefreshingNotes = true;

                SaveNoteState( );

                DestroyNotes( );

                // show a busy indicator
                Indicator.StartAnimating( );

                // if we don't have BOTH xml strings, re-download
                if( noteXml == null || styleSheetXml == null )
                {
                    Console.WriteLine( "CACHE INVALID. DOWNLOADING NEW NOTES" );

                    //download the notes
                    Rock.Mobile.Network.HttpRequest request = new HttpRequest();

                    RestRequest restRequest = new RestRequest( Method.GET );
                    restRequest.RequestFormat = DataFormat.Xml;

                    request.ExecuteAsync( NoteName, restRequest, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, byte[] model )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                            {
                                string body = Encoding.UTF8.GetString(model, 0, model.Length);
                                HandleNotePreReqs( body, null );
                            }
                            else
                            {
                                ReportException( "There was a problem trying to download the note. Please try again.", null );
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
                            // build the filename of the locally stored user data. If there is no "/" because it isn't a URL,
                            // we'll end up using the base name, which is what we want.
                            int lastSlashIndex = NoteName.LastIndexOf( "/" ) + 1;
                            string noteName = NoteName.Substring( lastSlashIndex );

                            Note.Create( UIScrollView.Bounds.Width, UIScrollView.Bounds.Height, this.UIScrollView, noteName + NoteConfig.UserNoteSuffix );

                            // enable scrolling
                            UIScrollView.ScrollEnabled = true;

                            // take the requested background color
                            UIScrollView.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value );
                            View.BackgroundColor = UIScrollView.BackgroundColor; //Make the view itself match too

                            // update the height of the scroll view to fit all content
                            RectangleF frame = Note.GetFrame( );
                            UIScrollView.ContentSize = new SizeF( UIScrollView.Bounds.Width, frame.Size.Height + ( UIScrollView.Bounds.Height / 3 ) );

                            FinishNotesCreation( );

                            CachedNoteXml = Note.NoteXml;
                            CachedStyleXml = ControlStyles.StyleSheetXml;
                            CachedNoteName = NoteName;
                        }
                        catch( Exception ex )
                        {
                            ReportException( "", ex );
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
                    FinishNotesCreation( );

                    // since there was an error, try redownloading the notes
                    if( NoteDownloadRetries > 0 )
                    {
                        Console.WriteLine( "Download error. Trying again" );

                        NoteDownloadRetries--;
                        CreateNotes( null, null );
                    }
                    else 
                    {
                        // we've tried as many times as we're going to. Give up and error.
                        if( e != null )
                        {
                            errorMsg += "\n" + e.Message;
                        }

                        // explain that we couldn't generate notes
                        UIAlertView alert = new UIAlertView( );
                        alert.Title = "Note Error";
                        alert.Message = errorMsg;
                        alert.AddButton( "Ok" );
                        alert.Show( );
                    }
                } );
        }
    }
}

