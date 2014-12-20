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
using CCVApp.Shared.Network;

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
        PointF StartingScrollPos { get; set; }

        /// <summary>
        /// The category picker used for selecting a prayer category
        /// </summary>
        UIPickerView CategoryPicker { get; set; }

        UILabel CategoryLabel { get; set; }

        /// <summary>
        /// True when the category picker is revealed
        /// </summary>
        bool CategoryPickerRevealed { get; set; }

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
            PrayerRequest.Editable = true;

            // skin the controls
            ScrollView.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );
            ScrollView.Parent = this;

            ControlStyling.StyleTextField( FirstNameText, PrayerStrings.CreatePrayer_FirstNamePlaceholderText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( FirstNameBackground );

            ControlStyling.StyleTextField( LastNameText, PrayerStrings.CreatePrayer_LastNamePlaceholderText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( LastNameBackground );

            // setup the prayer request field, which requires a fake "placeholder" text field
            PrayerRequest.Delegate = new TextViewDelegate( );
            PrayerRequest.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            PrayerRequest.TextContainerInset = UIEdgeInsets.Zero;
            PrayerRequest.Font = iOSCommon.LoadFontDynamic( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            PrayerRequest.TextContainer.LineFragmentPadding = 0;
            PrayerRequest.BackgroundColor = UIColor.Clear;
            PrayerRequestPlaceholder.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            PrayerRequestPlaceholder.BackgroundColor = UIColor.Clear;
            PrayerRequestPlaceholder.Text = PrayerStrings.CreatePrayer_PrayerRequest;
            PrayerRequestPlaceholder.Font = iOSCommon.LoadFontDynamic( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            //PrayerRequestPlaceholder.SizeToFit( );
            ControlStyling.StyleBGLayer( PrayerRequestLayer );


            // Setup the anonymous switch
            PostAnonymouslyLabel.Font = iOSCommon.LoadFontDynamic( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            PostAnonymouslyLabel.Text = PrayerStrings.CreatePrayer_PostAnonymously;
            UISwitchAnonymous.OnTintColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.Switch_OnColor );
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
                        FirstNameText.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );

                        LastNameText.Enabled = true;
                        LastNameText.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    }
                };

            // setup the public switch
            MakePublicLabel.Font = iOSCommon.LoadFontDynamic( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            MakePublicLabel.Text = PrayerStrings.CreatePrayer_MakePublic;
            UIPublicSwitch.OnTintColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.Switch_OnColor );

            ControlStyling.StyleBGLayer( SwitchBackground );



            // monitor for text field being edited, and keyboard show/hide notitications
            NSObject handle = NSNotificationCenter.DefaultCenter.AddObserver (KeyboardAdjustManager.TextFieldDidBeginEditingNotification, KeyboardAdjustManager.OnTextFieldDidBeginEditing);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (KeyboardAdjustManager.TextFieldChangedNotification, OnTextChanged);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );


            // setup the category picker and selector button
            CategoryLabel = new UILabel( );
            ControlStyling.StyleUILabel( CategoryLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

            CategoryLabel.Layer.AnchorPoint = PointF.Empty;
            CategoryLabel.Text = PrayerStrings.CreatePrayer_SelectCategoryLabel;
            CategoryLabel.SizeToFit( );
            CategoryLabel.Layer.Position = new PointF( (View.Bounds.Width - CategoryLabel.Bounds.Width) / 2, View.Frame.Bottom );
            View.AddSubview( CategoryLabel );

            CategoryPicker = new UIPickerView( );
            CategoryPicker.Layer.AnchorPoint = PointF.Empty;
            CategoryPicker.Model = new CategoryPickerModel() { Parent = this };
            CategoryPicker.Layer.Position = new PointF( 0, CategoryLabel.Frame.Bottom );
            ControlStyling.StyleBGLayer( CategoryPicker ); //although it's a derived class, it can still be skinned like a straight UIView
            View.AddSubview( CategoryPicker );

            CategoryButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                OnToggleCategoryPicker( true );
            };
            CategoryButton.SetTitle( PrayerStrings.CreatePrayer_CategoryButtonText, UIControlState.Normal );
            CategoryButton.SetTitleColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            CategoryButton.Font = iOSCommon.LoadFontDynamic( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
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
            if( ( string.IsNullOrEmpty( FirstNameText.Text ) == false || UISwitchAnonymous.On == true) &&
                  string.IsNullOrEmpty( PrayerRequest.Text ) == false )
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

                prayerRequest.Text = PrayerRequest.Text;
                prayerRequest.EnteredDateTime = DateTime.Now;
                prayerRequest.ExpirationDate = DateTime.Now.AddYears( 1 );
                prayerRequest.CategoryId = 110; //todo: Let the end user set this.
                prayerRequest.IsActive = true;
                prayerRequest.IsPublic = UIPublicSwitch.On; // use the public switch's state to determine whether it's a public prayer or not.
                prayerRequest.IsApproved = false;

                // launch the post view controller
                Prayer_PostUIViewController postPrayerVC = Storyboard.InstantiateViewController( "Prayer_PostUIViewController" ) as Prayer_PostUIViewController;
                postPrayerVC.PrayerRequest = prayerRequest;
                Task.PerformSegue( this, postPrayerVC );
            }
        }
       
        /// <summary>
        /// Shows / Hides the category picker by animating the picker onto the screen and scrolling
        /// the ScrollView to reveal the category field.
        /// </summary>
        public void OnToggleCategoryPicker( bool enabled )
        {
            // only do something if there's a state change
            if ( CategoryPickerRevealed != enabled )
            {
                //Start an animation
                UIView.BeginAnimations( "AnimateForPicker" );
                UIView.SetAnimationBeginsFromCurrentState( true );
                UIView.SetAnimationDuration( .5f );
                UIView.SetAnimationCurve( UIViewAnimationCurve.EaseInOut );

                if ( enabled == true )
                {
                    // we're going to show it, so hide the nav bar
                    Task.NavToolbar.Reveal( false );

                    // stamp the scroll position of the scrollview
                    StartingScrollPos = ScrollView.ContentOffset;

                    // set the picker to be on screen
                    CategoryLabel.Layer.Position = new PointF( CategoryLabel.Layer.Position.X, View.Bounds.Height - (CategoryLabel.Bounds.Height + CategoryPicker.Bounds.Height) );
                    CategoryPicker.Layer.Position = new PointF( 0, CategoryLabel.Frame.Bottom );

                    // scroll the category field into view and lock scrolling
                    ScrollView.ContentOffset = new PointF( 0, CategoryLayer.Frame.Top - CategoryLayer.Frame.Height );
                    ScrollView.ScrollEnabled = false;
                }
                else
                {
                    // we're hiding the picker, so restore the original scroll position and enable it
                    ScrollView.ContentOffset = StartingScrollPos;
                    ScrollView.ScrollEnabled = true;

                    // move the picker off screen
                    CategoryLabel.Layer.Position = new PointF( CategoryLabel.Layer.Position.X, View.Frame.Bottom );
                    CategoryPicker.Layer.Position = new PointF( 0, CategoryLabel.Frame.Bottom );
                }

                CategoryPickerRevealed = enabled;

                //Commit the animation
                UIView.CommitAnimations( ); 
            }
        }

        /// <summary>
        /// Called when the user selects something in the UIPicker
        /// </summary>
        public void PickerSelected( int row )
        {
            // set the category's text to be the item they selected. Note that we now change the color to Active from the original Placeholder
            CategoryButton.SetTitleColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
            CategoryButton.SetTitle( RockGeneralData.Instance.Data.PrayerCategories[ row ], UIControlState.Normal );
        }

        /// <summary>
        /// The model that defines the object that will be selected in the UI Picker
        /// </summary>
        public class CategoryPickerModel : UIPickerViewModel
        {
            public Prayer_CreateUIViewController Parent { get; set; }

            public override int GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override int GetRowsInComponent(UIPickerView picker, int component)
            {
                return RockGeneralData.Instance.Data.PrayerCategories.Count;
            }

            public override string GetTitle(UIPickerView picker, int row, int component)
            {
                return RockGeneralData.Instance.Data.PrayerCategories[ row ];
            }

            public override void Selected(UIPickerView picker, int row, int component)
            {
                Parent.PickerSelected( row );
            }

            public override UIView GetView(UIPickerView picker, int row, int component, UIView view)
            {
                UILabel label = view as UILabel;
                if ( label == null )
                {
                    label = new UILabel();
                    label.TextColor = UIColor.White;
                    label.Text = RockGeneralData.Instance.Data.PrayerCategories[ row ];
                    label.Font = iOSCommon.LoadFontDynamic( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
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
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // once all the controls are laid out, update the content size to provide a little "bounce"
            ScrollView.ContentSize = new System.Drawing.SizeF( ScrollView.Bounds.Width, ScrollView.Bounds.Height + ( ScrollView.Bounds.Height * .25f ) );
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
            if ( CategoryPickerRevealed == true )
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
