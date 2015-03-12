using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Strings;
using System.Collections.Generic;
using CoreGraphics;
using CCVApp.Shared.Config;
using CCVApp.Shared.Network;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using Rock.Mobile.Animation;

namespace iOS
{
    partial class Prayer_CreateUIViewController : TaskUIViewController
	{
        // setup a delegate to manage text editing notifications
        public class TextViewDelegate : UITextViewDelegate
        {
            public override bool ShouldBeginEditing(UITextView textView)
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName( Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager.TextFieldDidBeginEditingNotification, NSValue.FromCGRect( textView.Frame ) );
                return true;
            }

            public override void Changed(UITextView textView)
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName( Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager.TextFieldChangedNotification, NSValue.FromCGRect( textView.Frame ) );
            }
        }

        /// <summary>
        /// List of the handles for our NSNotifications
        /// </summary>
        List<NSObject> ObserverHandles { get; set; }

        /// <summary>
        /// The keyboard manager that will adjust the UIView to not be obscured by the software keyboard
        /// </summary>
        KeyboardAdjustManager KeyboardAdjustManager { get; set; }

        /// <summary>
        /// The starting position of the scrollView so we can restore after the user uses the UIPicker
        /// </summary>
        /// <value>The starting scroll position.</value>
        CGPoint StartingScrollPos { get; set; }

        PickerAdjustManager PickerAdjustManager { get; set; }

        public Prayer_CreateUIViewController (IntPtr handle) : base (handle)
        {
            ObserverHandles = new List<NSObject>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // set the background view to black so we don't get white aliasing flicker during
            // the pan
            View.BackgroundColor = UIColor.Black;

            // create our keyboard adjustment manager, which works to make sure text fields scroll into visible
            // range when a keyboard appears
            KeyboardAdjustManager = new Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager( View, ScrollView );

            // setup the default control styles
            PrayerRequest.Editable = true;

            // skin the controls
            ScrollView.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            ScrollView.Parent = this;

            ControlStyling.StyleTextField( FirstNameText, PrayerStrings.CreatePrayer_FirstNamePlaceholderText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( FirstNameBackground );

            ControlStyling.StyleTextField( LastNameText, PrayerStrings.CreatePrayer_LastNamePlaceholderText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( LastNameBackground );

            // setup the prayer request field, which requires a fake "placeholder" text field
            PrayerRequest.Delegate = new TextViewDelegate( );
            PrayerRequest.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            PrayerRequest.TextContainerInset = UIEdgeInsets.Zero;
            PrayerRequest.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            PrayerRequest.TextContainer.LineFragmentPadding = 0;
            PrayerRequest.BackgroundColor = UIColor.Clear;
            PrayerRequestPlaceholder.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            PrayerRequestPlaceholder.BackgroundColor = UIColor.Clear;
            PrayerRequestPlaceholder.Text = PrayerStrings.CreatePrayer_PrayerRequest;
            PrayerRequestPlaceholder.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            //PrayerRequestPlaceholder.SizeToFit( );
            ControlStyling.StyleBGLayer( PrayerRequestLayer );


            // Setup the anonymous switch
            PostAnonymouslyLabel.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            PostAnonymouslyLabel.Text = PrayerStrings.CreatePrayer_PostAnonymously;
            UISwitchAnonymous.OnTintColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Switch_OnColor );
            UISwitchAnonymous.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    if( UISwitchAnonymous.On == true )
                    {
                        FirstNameText.Enabled = false;
                        FirstNameText.Text = string.Empty;

                        LastNameText.Enabled = false;
                        LastNameText.Text = string.Empty;
                    }
                    else
                    {
                        FirstNameText.Enabled = true;
                        FirstNameText.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );

                        LastNameText.Enabled = true;
                        LastNameText.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    }
                };

            // setup the public switch
            MakePublicLabel.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            MakePublicLabel.Text = PrayerStrings.CreatePrayer_MakePublic;
            UIPublicSwitch.OnTintColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Switch_OnColor );

            ControlStyling.StyleBGLayer( SwitchBackground );



            // monitor for text field being edited, and keyboard show/hide notitications
            NSObject handle = NSNotificationCenter.DefaultCenter.AddObserver (Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager.TextFieldDidBeginEditingNotification, KeyboardAdjustManager.OnTextFieldDidBeginEditing);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager.TextFieldChangedNotification, OnTextChanged);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );


            // setup the category picker and selector button
            UILabel categoryLabel = new UILabel( );
            ControlStyling.StyleUILabel( categoryLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            categoryLabel.Text = PrayerStrings.CreatePrayer_SelectCategoryLabel;

            PickerAdjustManager = new PickerAdjustManager( View, ScrollView, categoryLabel, CategoryLayer );
            UIPickerView pickerView = new UIPickerView();
            pickerView.Model = new CategoryPickerModel() { Parent = this };
            PickerAdjustManager.SetPicker( pickerView );


            CategoryButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                OnToggleCategoryPicker( true );
            };
            CategoryButton.SetTitle( PrayerStrings.CreatePrayer_CategoryButtonText, UIControlState.Normal );
            CategoryButton.SetTitleColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            CategoryButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( CategoryLayer );


            // setup the submit button
            ControlStyling.StyleButton( SubmitButton, PrayerStrings.CreatePrayer_SubmitButtonText, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            SubmitButton.TouchUpInside += SubmitPrayerRequest;
        }

        /// <summary>
        /// Builds a prayer request from data in the UI Fields and kicks off the post UI Control
        /// </summary>
        void SubmitPrayerRequest(object sender, EventArgs e)
        {
            // ensure they either put a first name or enabled anonymous, and ensure there's a prayer request
            if ( ( (string.IsNullOrEmpty( FirstNameText.Text ) == false && string.IsNullOrEmpty( LastNameText.Text ) == false) || UISwitchAnonymous.On == true ) &&
                    string.IsNullOrEmpty( PrayerRequest.Text ) == false )
            {
                Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                EnableControls( false );

                if ( UISwitchAnonymous.On == true )
                {
                    prayerRequest.FirstName = "Anonymous";
                    prayerRequest.LastName = "Anonymous";
                }
                else
                {
                    prayerRequest.FirstName = FirstNameText.Text;
                    prayerRequest.LastName = LastNameText.Text;
                }

                prayerRequest.Text = PrayerRequest.Text;
                prayerRequest.EnteredDateTime = DateTime.Now;
                prayerRequest.ExpirationDate = DateTime.Now.AddYears( 1 );
                prayerRequest.CategoryId = RockGeneralData.Instance.Data.PrayerCategoryToId( CategoryButton.Title( UIControlState.Normal ) );
                prayerRequest.IsActive = true;
                prayerRequest.IsPublic = UIPublicSwitch.On; // use the public switch's state to determine whether it's a public prayer or not.
                prayerRequest.Guid = Guid.NewGuid( );
                prayerRequest.IsApproved = false;

                // launch the post view controller
                Prayer_PostUIViewController postPrayerVC = Storyboard.InstantiateViewController( "Prayer_PostUIViewController" ) as Prayer_PostUIViewController;
                postPrayerVC.PrayerRequest = prayerRequest;
                Task.PerformSegue( this, postPrayerVC );
            }
            else
            {
                // Update the first name background color
                // if they left the name field blank and didn't turn on Anonymous, flag the field.
                uint targetNameColor = ControlStylingConfig.BG_Layer_Color; 
                if( string.IsNullOrEmpty( FirstNameText.Text ) && UISwitchAnonymous.On == false )
                {
                    targetNameColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                }
                Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetNameColor, FirstNameBackground );


                // Update the LAST name background color
                // if they left the name field blank and didn't turn on Anonymous, flag the field.
                uint targetLastNameColor = ControlStylingConfig.BG_Layer_Color; 
                if( string.IsNullOrEmpty( LastNameText.Text ) && UISwitchAnonymous.On == false )
                {
                    targetLastNameColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                }
                Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetLastNameColor, LastNameBackground );


                // Update the prayer background color
                uint targetPrayerColor = string.IsNullOrEmpty( PrayerRequest.Text ) ? ControlStylingConfig.BadInput_BG_Layer_Color : ControlStylingConfig.BG_Layer_Color;
                Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetPrayerColor, PrayerRequestLayer );
            }
        }
       
        /// <summary>
        /// Shows / Hides the category picker by animating the picker onto the screen and scrolling
        /// the ScrollView to reveal the category field.
        /// </summary>
        public void OnToggleCategoryPicker( bool enabled )
        {
            if ( enabled == true )
            {
                // we're going to show it, so hide the nav bar
                Task.NavToolbar.Reveal( false );

                // force the keyboard to hide.
                PrayerRequest.ResignFirstResponder( );
                FirstNameText.ResignFirstResponder( );
                LastNameText.ResignFirstResponder( );
            }

            PickerAdjustManager.TogglePicker( enabled );
        }

        /// <summary>
        /// Called when the user selects something in the UIPicker
        /// </summary>
        public void PickerSelected( int row )
        {
            // set the category's text to be the item they selected. Note that we now change the color to Active from the original Placeholder
            CategoryButton.SetTitleColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
            CategoryButton.SetTitle( RockGeneralData.Instance.Data.PrayerCategories[ row ].Name, UIControlState.Normal );
        }

        /// <summary>
        /// The model that defines the object that will be selected in the UI Picker
        /// </summary>
        public class CategoryPickerModel : UIPickerViewModel
        {
            public Prayer_CreateUIViewController Parent { get; set; }

            public override nint GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView picker, nint component)
            {
                return RockGeneralData.Instance.Data.PrayerCategories.Count;
            }

            public override string GetTitle(UIPickerView picker, nint row, nint component)
            {
                return RockGeneralData.Instance.Data.PrayerCategories[ (int)row ].Name;
            }

            public override void Selected(UIPickerView picker, nint row, nint component)
            {
                Parent.PickerSelected( (int)row );
            }

            public override UIView GetView(UIPickerView picker, nint row, nint component, UIView view)
            {
                UILabel label = view as UILabel;
                if ( label == null )
                {
                    label = new UILabel();
                    label.TextColor = UIColor.White;
                    label.Text = RockGeneralData.Instance.Data.PrayerCategories[ (int)row ].Name;
                    label.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
                    label.SizeToFit( );
                }

                return label;
            }
        }

        void OnTextChanged( NSNotification notification )
        {
            KeyboardAdjustManager.OnTextFieldChanged( notification );

            TogglePlaceholderText( );
        }

        void TogglePlaceholderText( )
        {
            // toggle our fake placeholder text
            if ( PrayerRequest.Text == "" )
            {
                PrayerRequestPlaceholder.Hidden = false;
            }
            else
            {
                PrayerRequestPlaceholder.Hidden = true;
            }
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

            TogglePlaceholderText( );

            // prepopulate the name fields if we have them
            if ( RockMobileUser.Instance.LoggedIn == true )
            {
                FirstNameText.Text = RockMobileUser.Instance.Person.NickName;
                LastNameText.Text = RockMobileUser.Instance.Person.LastName;
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // once all the controls are laid out, update the content size to provide a little "bounce"
            ScrollView.ContentSize = new CGSize( ScrollView.Bounds.Width, ScrollView.Bounds.Height + ( ScrollView.Bounds.Height * .25f ) );
        }

        void EnableControls( bool enabled )
        {
            FirstNameText.Enabled = enabled;
            LastNameText.Enabled = enabled;

            PrayerRequest.Editable = enabled;

            UISwitchAnonymous.Enabled = enabled;
            UIPublicSwitch.Enabled = enabled;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // if we're picking a category, don't allow anything else.
            if ( PickerAdjustManager.Revealed == true )
            {
                OnToggleCategoryPicker( false );
            }
            else
            {
                base.TouchesEnded( touches, evt );

                // ensure that tapping anywhere outside a text field will hide the keyboard
                FirstNameText.ResignFirstResponder( );
                LastNameText.ResignFirstResponder( );
                PrayerRequest.ResignFirstResponder( );
            }
        }
	}
}
