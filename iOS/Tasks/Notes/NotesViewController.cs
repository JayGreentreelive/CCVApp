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
            if ( Frame.Contains( point ) )
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

        public override void DecelerationEnded(UIScrollView scrollView)
        {
            Console.WriteLine( "User decelerating at: {0}", scrollView.DecelerationRate );
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
        /// The frame of the text field that was tapped when the keyboard was shown.
        /// Used so we know whether to scroll up the text field or not.
        /// </summary>
        RectangleF Edit_TappedTextFieldFrame { get; set; }

        /// <summary>
        /// The amount the scrollView was scrolled when editing began.
        /// Used so we can restore the scrollView position when editing is finished.
        /// </summary>
        /// <value>The edit start scroll offset.</value>
        PointF Edit_StartScrollOffset { get; set; }

        /// <summary>
        /// The position of the UIScrollView when text editing began.
        /// </summary>
        /// <value>The edit start screen offset.</value>
        PointF Edit_StartScreenOffset { get; set; }

        /// <summary>
        /// The bottom position of the visible area when the keyboard is up.
        /// </summary>
        /// <value>The edit visible area with keyboard bot.</value>
        float Edit_VisibleAreaWithKeyboardBot { get; set; }

        /// <summary>
        /// The URL for this note
        /// </summary>
        /// <value>The note URL.</value>
        public string NoteName { get; set; }

        public NotesViewController( ) : base( )
        {
        }

        public override void DidReceiveMemoryWarning( )
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning( );

            // Release any cached data, images, etc that aren't in use.
        }

        public void MakeActive( )
        {
            UIApplication.SharedApplication.IdleTimerDisabled = true;
        }

        public void MakeInActive( )
        {
            SaveNoteState( );

            UIApplication.SharedApplication.IdleTimerDisabled = false;
        }

        public void OnResignActive( )
        {
            SaveNoteState( );
        }

        public void DidEnterBackground( )
        {
            SaveNoteState( );
        }

        public void WillTerminate( )
        {
            SaveNoteState( );

            DestroyNotes( );
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

            //note: the frame height of the nav bar is what it CURRENTLY is, not what it WILL be after we rotate. So, when we go from Portrait to Landscape,
            // it says 40, but it's gonna be 32. Conversely, going back, we use 32 and it's actually 40, which causes us to start this view 8px too high.
            RefreshButton.Layer.Position = new PointF( View.Bounds.Width / 2, NavigationController.NavigationBar.Frame.Height + (RefreshButton.Frame.Height / 2) );

            UIScrollView.Frame = new RectangleF( 0, 0, View.Bounds.Width, View.Bounds.Height - RefreshButton.Frame.Height - NavigationController.NavigationBar.Frame.Height );
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

            // monitor for text field being edited, and keyboard show/hide notitications
            NSNotificationCenter.DefaultCenter.AddObserver ("TextFieldDidBeginEditing", OnTextFieldDidBeginEditing);
            NSNotificationCenter.DefaultCenter.AddObserver ("TextFieldChanged", OnTextFieldChanged);
            NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, OnKeyboardNotification);
            NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, OnKeyboardNotification);

            UIScrollView = new CustomScrollView( );
            UIScrollView.Interceptor = this;
            UIScrollView.Frame = View.Frame;
            UIScrollView.BackgroundColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x1C1C1CFF );
            UIScrollView.Delegate = new NotesScrollViewDelegate() { Parent = this };

            UITapGestureRecognizer tapGesture = new UITapGestureRecognizer();
            tapGesture.NumberOfTapsRequired = 2;
            tapGesture.AddTarget (this, new MonoTouch.ObjCRuntime.Selector("DoubleTapSelector:"));
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
            View.AddSubview( RefreshButton );
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
                    Note.TouchesEnded( touch.LocationInView( UIScrollView ) );
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

        public void OnKeyboardNotification( NSNotification notification )
        {
            // Issues: Appears like a bug when it can't scroll "up" anymore and the keyboard
            // obstructs a note

            //Start an animation, using values from the keyboard
            UIView.BeginAnimations ("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState (true);
            UIView.SetAnimationDuration (UIKeyboard.AnimationDurationFromNotification (notification));
            UIView.SetAnimationCurve ((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification (notification));

            //Check if the keyboard is becoming visible
            if( notification.Name == UIKeyboard.WillShowNotification )
            {
                // store the original screen positioning / scroll. No matter what, we will
                // undo any scrolling the user did while editing.
                Edit_StartScrollOffset = UIScrollView.ContentOffset;
                Edit_StartScreenOffset = UIScrollView.Layer.Position;

                // get the keyboard frame and transform it into our view's space
                RectangleF keyboardFrame = UIKeyboard.FrameEndFromNotification (notification);
                keyboardFrame = View.ConvertRectToView( keyboardFrame, null );

                // first, get the bottom point of the visible area.
                Edit_VisibleAreaWithKeyboardBot = View.Bounds.Height - keyboardFrame.Height;

                // now get the dist between the bottom of the visible area and the text field (text field's pos also changes as we scroll)
                MaintainEditTextVisibility( );
            }
            else
            {
                // get the keyboard frame and transform it into our view's space
                RectangleF keyboardFrame = UIKeyboard.FrameBeginFromNotification (notification);
                keyboardFrame = View.ConvertRectToView( keyboardFrame, null );

                // restore the screen to the way it was before editing
                UIScrollView.ContentOffset = Edit_StartScrollOffset;
                UIScrollView.Layer.Position = Edit_StartScreenOffset;
            }

            //Commit the animation
            UIView.CommitAnimations (); 
        }

        public void OnTextFieldDidBeginEditing( NSNotification notification )
        {
            // put the text frame in absolute screen coordinates
            RectangleF textFrame = ((NSValue)notification.Object).RectangleFValue;

            // first subtract the amount scrolled by the view.
            float yPos = textFrame.Y - UIScrollView.ContentOffset.Y;
            float xPos = textFrame.X - UIScrollView.ContentOffset.X;

            // now subtract however far down the scroll view is from the top.
            yPos -= View.Frame.Y - UIScrollView.Frame.Y;
            xPos -= View.Frame.X - UIScrollView.Frame.X;

            Edit_TappedTextFieldFrame = new RectangleF( xPos, yPos, textFrame.Width, textFrame.Height );
        }

        public void OnTextFieldChanged( NSNotification notification )
        {
            // put the text frame in absolute screen coordinates
            RectangleF textFrame = ((NSValue)notification.Object).RectangleFValue;

            // first subtract the amount scrolled by the view.
            float yPos = textFrame.Y - UIScrollView.ContentOffset.Y;
            float xPos = textFrame.X - UIScrollView.ContentOffset.X;

            // now subtract however far down the scroll view is from the top.
            yPos -= View.Frame.Y - UIScrollView.Frame.Y;
            xPos -= View.Frame.X - UIScrollView.Frame.X;

            // update it
            Edit_TappedTextFieldFrame = new RectangleF( xPos, yPos, textFrame.Width, textFrame.Height );

            MaintainEditTextVisibility( );
        }

        protected void MaintainEditTextVisibility( )
        {
            // PLUS makes it scroll "up"
            // NEG makes it scroll "down"
            // TextField position MOVES AS THE PAGE IS SCROLLED.
            // It is always relative, however, to the screen. So, if it's near the top, it's 0,
            // whether that's because it was moved down and the screen scrolled up, or it's just at the top naturally.

            // Scroll the view so tha the bottom of the text field is as close as possible to
            // the top of the keyboard without violating scroll constraints

            // determine if they're typing near the bottom of the screen and it needs to scroll.
            float scrollAmount = (Edit_VisibleAreaWithKeyboardBot - Edit_TappedTextFieldFrame.Bottom);

            // clamp to the legal amount we can scroll "down"
            scrollAmount = Math.Min( scrollAmount, UIScrollView.ContentOffset.Y );

            // Now determine the amount of "up" scroll remaining
            float maxScrollAmount = UIScrollView.ContentSize.Height - UIScrollView.Bounds.Height;
            float scrollAmountDistRemainingDown = -(maxScrollAmount - UIScrollView.ContentOffset.Y);

            // and clamp the scroll amount to that, so we don't scroll "up" beyond the contraints
            float allowedScrollAmount = Math.Max( scrollAmount, scrollAmountDistRemainingDown );
            UIScrollView.ContentOffset = new PointF( UIScrollView.ContentOffset.X, UIScrollView.ContentOffset.Y - allowedScrollAmount );

            // if we STILL haven't scrolled enough "up" because of scroll contraints, we'll allow the window itself to move up.
            float scrollDistNeeded = -Math.Min( 0, scrollAmount - scrollAmountDistRemainingDown );
            UIScrollView.Layer.Position = new PointF( UIScrollView.Layer.Position.X, UIScrollView.Layer.Position.Y - scrollDistNeeded );
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

                SaveNoteState( );

                DestroyNotes( );

                // show a busy indicator
                Indicator.StartAnimating( );

                // if we don't have BOTH xml strings, re-download
                if( noteXml == null || styleSheetXml == null )
                {
                    HttpWebRequest.Instance.MakeAsyncRequest( CCVApp.Shared.Config.Note.BaseURL + NoteName + CCVApp.Shared.Config.Note.Extension, (Exception ex, Dictionary<string, string> responseHeaders, string body ) =>
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
                            Note.Create( UIScrollView.Bounds.Width, UIScrollView.Bounds.Height, this.UIScrollView, NoteName + CCVApp.Shared.Config.Note.UserNoteSuffix );

                            // enable scrolling
                            UIScrollView.ScrollEnabled = true;

                            // take the requested background color
                            UIScrollView.BackgroundColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value );
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

