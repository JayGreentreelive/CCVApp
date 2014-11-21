using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Strings;
using System.Collections.Generic;
using System.Drawing;
using Rock.Mobile.PlatformCommon;
using CCVApp.Shared.Config;

namespace iOS
{
    partial class Prayer_CreateUIViewController : TaskUIViewController
	{
        // setup a delegate to manage text editing notifications
        public class TextViewDelegate : UITextViewDelegate
        {
            public override bool ShouldBeginEditing(UITextView textView)
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName( KeyboardAdjustManager.TextFieldDidBeginEditingNotification, NSValue.FromRectangleF( textView.Frame ) );
                return true;
            }

            public override void Changed(UITextView textView)
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName( KeyboardAdjustManager.TextFieldChangedNotification, NSValue.FromRectangleF( textView.Frame ) );
            }
        }

        List<NSObject> ObserverHandles { get; set; }
        KeyboardAdjustManager KeyboardAdjustManager { get; set; }

        public Prayer_CreateUIViewController (IntPtr handle) : base (handle)
        {
            ObserverHandles = new List<NSObject>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // create our keyboard adjustment manager, which works to make sure text fields scroll into visible
            // range when a keyboard appears
            KeyboardAdjustManager = new KeyboardAdjustManager( View, ScrollView );

            // setup the default control styles
            ActivityIndicator.Hidden = true;
            RequestText.Editable = true;

            BackingControlView.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_BGColor );
            ScrollView.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_BGColor );
            ScrollView.ContentSize = new System.Drawing.SizeF( ScrollView.Bounds.Width, ScrollView.Bounds.Height + ( ScrollView.Bounds.Height * .25f ) );

            ScrollView.Parent = this;

            FirstNameText.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_FirstNameTextColor );
            FirstNameText.AttributedPlaceholder = new NSAttributedString( PrayerStrings.CreatePrayer_FirstNamePlaceholderText, null, PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_FirstNamePlaceholderTextColor ) );
            FirstNameText.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_FirstNameBackgroundColor );

            FirstNameBackground.Layer.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_FirstNameBackingLayer_BGColor ).CGColor;
            FirstNameBackground.Layer.BorderColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_FirstNameBackingLayer_BorderColor ).CGColor;
            FirstNameBackground.Layer.BorderWidth = PrayerConfig.CreatePrayer_FirstNameBackingLayer_BorderWidth;

            LastNameText.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_LastNameTextColor );
            LastNameText.AttributedPlaceholder = new NSAttributedString( PrayerStrings.CreatePrayer_LastNamePlaceholderText, null, PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_LastNamePlaceholderTextColor ) );
            LastNameText.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_LastNameBackgroundColor );

            LastNameBackground.Layer.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_LastNameBackingLayer_BGColor ).CGColor;
            LastNameBackground.Layer.BorderColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_LastNameBackingLayer_BorderColor ).CGColor;
            LastNameBackground.Layer.BorderWidth = PrayerConfig.CreatePrayer_LastNameBackingLayer_BorderWidth;


            SwitchBackground.Layer.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_SwitchBGColor ).CGColor;
            SwitchBackground.Layer.BorderColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_SwitchBorderColor ).CGColor;
            SwitchBackground.Layer.BorderWidth = PrayerConfig.CreatePrayer_SwitchBorderWidth;

            RequestText.Delegate = new TextViewDelegate( );
            RequestText.Layer.BorderColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_RequestBorderColor ).CGColor;
            RequestText.Layer.BorderWidth = PrayerConfig.CreatePrayer_RequestBorderWidth;
            RequestText.Layer.CornerRadius = PrayerConfig.CreatePrayer_RequestCornerRadius;

            UISwitchAnonymous.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    if( UISwitchAnonymous.On == true )
                    {
                        FirstNameText.Enabled = false;
                        FirstNameText.TextColor = UIColor.DarkGray;

                        LastNameText.Enabled = false;
                        LastNameText.TextColor = UIColor.DarkGray;
                    }
                    else
                    {
                        FirstNameText.Enabled = true;
                        FirstNameText.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_FirstNameTextColor );

                        LastNameText.Enabled = true;
                        LastNameText.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.CreatePrayer_LastNameTextColor );
                    }
                };

            // the heart of this is the submit button
            SubmitButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // ensure they either put a first name or enabled anonymous, and ensure there's a prayer request
                    if( ( string.IsNullOrEmpty( FirstNameText.Text ) == false || UISwitchAnonymous.On == true) &&
                          string.IsNullOrEmpty( RequestText.Text ) == false )
                    {
                        Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                        EnableControls( false );

                        if( UISwitchAnonymous.On == true )
                        {
                            prayerRequest.FirstName = "Anonymous";
                            prayerRequest.LastName = "Anonymous";
                        }
                        else
                        {
                            prayerRequest.FirstName = FirstNameText.Text;
                            prayerRequest.LastName = LastNameText.Text;
                        }

                        prayerRequest.Text = RequestText.Text;
                        prayerRequest.EnteredDateTime = DateTime.Now;
                        prayerRequest.ExpirationDate = DateTime.Now.AddYears( 1 );
                        prayerRequest.CategoryId = 110; //todo: Let the end user set this.
                        prayerRequest.IsActive = true;
                        prayerRequest.IsPublic = UIPublicSwitch.On; // use the public switch's state to determine whether it's a public prayer or not.
                        prayerRequest.IsApproved = false;

                        ActivityIndicator.Hidden = false;

                        // launch the post view controller
                        Prayer_PostUIViewController postPrayerVC = Storyboard.InstantiateViewController( "Prayer_PostUIViewController" ) as Prayer_PostUIViewController;
                        postPrayerVC.PrayerRequest = prayerRequest;
                        Task.PerformSegue( this, postPrayerVC );
                    }
                };

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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // enable all controls
            EnableControls( true );

            UISwitchAnonymous.SetState( false, false );
            UIPublicSwitch.SetState( true, false );
        }

        void EnableControls( bool enabled )
        {
            FirstNameText.Enabled = enabled;
            LastNameText.Enabled = enabled;

            RequestText.Editable = enabled;

            UISwitchAnonymous.Enabled = enabled;
            UIPublicSwitch.Enabled = enabled;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // ensure that tapping anywhere outside a text field will hide the keyboard
            FirstNameText.ResignFirstResponder( );
            LastNameText.ResignFirstResponder( );
            RequestText.ResignFirstResponder( );
        }
	}
}
