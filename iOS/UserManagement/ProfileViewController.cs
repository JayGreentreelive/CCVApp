using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using CCVApp.Shared.Network;
using CoreAnimation;
using CoreGraphics;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using System.Collections.Generic;
using Rock.Mobile.Util.Strings;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using Rock.Mobile.PlatformSpecific.Util;

namespace iOS
{
	partial class ProfileViewController : UIViewController
	{
        /// <summary>
        /// Reference to the parent springboard for returning apon completion
        /// </summary>
        /// <value>The springboard.</value>
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// True when a change to the profile was made and the user should be prompted
        /// to submit changes.
        /// </summary>
        /// <value><c>true</c> if dirty; otherwise, <c>false</c>.</value>
        protected bool Dirty { get; set; }

        /// <summary>
        /// View for displaying the logo in the header
        /// </summary>
        /// <value>The logo view.</value>
        UIImageView LogoView { get; set; }

        PickerAdjustManager GenderPicker { get; set; }

        PickerAdjustManager BirthdatePicker { get; set; }

		public ProfileViewController (IntPtr handle) : base (handle)
		{
		}

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations( )
        {
            return Springboard.GetSupportedInterfaceOrientations( );
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation( )
        {
            return Springboard.PreferredInterfaceOrientationForPresentation( );
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ScrollView.Parent = this;

            //setup styles
            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            ControlStyling.StyleTextField( NickNameText, ProfileStrings.NickNamePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( NickNameLayer );
            NickNameText.EditingDidBegin += (sender, e) => { Dirty = true; };

            ControlStyling.StyleTextField( LastNameText, ProfileStrings.LastNamePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( LastNameLayer );
            LastNameText.EditingDidBegin += (sender, e) => { Dirty = true; };


            ControlStyling.StyleTextField( EmailText, ProfileStrings.EmailPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( EmailLayer );
            EmailText.EditingDidBegin += (sender, e) => { Dirty = true; };

            ControlStyling.StyleTextField( CellPhoneText, ProfileStrings.CellPhonePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( CellPhoneLayer );
            CellPhoneText.EditingDidBegin += (sender, e) => { Dirty = true; };


            ControlStyling.StyleTextField( StreetText, ProfileStrings.StreetPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( StreetLayer );
            StreetText.EditingDidBegin += (sender, e) => { Dirty = true; };

            ControlStyling.StyleTextField( CityText, ProfileStrings.CityPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( CityLayer );
            CityText.EditingDidBegin += (sender, e) => { Dirty = true; };

            ControlStyling.StyleTextField( StateText, ProfileStrings.StatePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( StateLayer );
            StateText.EditingDidBegin += (sender, e) => { Dirty = true; };

            ControlStyling.StyleTextField( ZipText, ProfileStrings.ZipPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( ZipLayer );
            ZipText.EditingDidBegin += (sender, e) => { Dirty = true; };

            GenderButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    // if they have a gender selected, default to that.
                    if( string.IsNullOrEmpty( GenderText.Text ) == false )
                    {
                        ((UIPickerView)GenderPicker.Picker).Select( CCVApp.Shared.Network.RockGeneralData.Instance.Data.Genders.IndexOf( GenderText.Text ) - 1, 0, false );
                    }

                    GenderPicker.TogglePicker( true );
                };
            ControlStyling.StyleTextField( GenderText, ProfileStrings.GenderPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( GenderLayer );

            BirthdayButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    // setup the default date time to display
                    DateTime initialDate = DateTime.Now;
                    if( string.IsNullOrEmpty( BirthdateText.Text ) == false )
                    {
                        initialDate = DateTime.Parse( BirthdateText.Text );
                    }

                    ((UIDatePicker)BirthdatePicker.Picker).Date = initialDate.DateTimeToNSDate( );
                    BirthdatePicker.TogglePicker( true );
                };
            ControlStyling.StyleTextField( BirthdateText, ProfileStrings.BirthdatePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( BirthdateLayer );


            // setup the home campus chooser
            HomeCampusButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    UIAlertController actionSheet = UIAlertController.Create( ProfileStrings.SelectCampus_SourceTitle, 
                                                                              ProfileStrings.SelectCampus_SourceDescription, 
                                                                              UIAlertControllerStyle.ActionSheet );

                    // for each campus, create an entry in the action sheet, and its callback will assign
                    // that campus index to the user's viewing preference
                    for( int i = 0; i < RockGeneralData.Instance.Data.Campuses.Count; i++ )
                    {
                        UIAlertAction campusAction = UIAlertAction.Create( RockGeneralData.Instance.Data.Campuses[ i ], UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                            {
                                //get the index of the campus based on the selection's title, and then set that campus title as the string
                                int campusIndex = RockGeneralData.Instance.Data.Campuses.IndexOf( obj.Title );
                                //RockMobileUser.Instance.ViewingCampus = RockGeneralData.Instance.Data.Campuses.IndexOf( obj.Title );
                                HomeCampusText.Text = string.Format( ProfileStrings.Viewing_Campus, RockGeneralData.Instance.Data.Campuses[ campusIndex ] );
                            } );

                        actionSheet.AddAction( campusAction );
                    }

                    PresentViewController( actionSheet, true, null );
                };

            ControlStyling.StyleTextField( HomeCampusText, ProfileStrings.CampusPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( HomeCampusLayer );


            ControlStyling.StyleButton( DoneButton, ProfileStrings.DoneButtonTitle, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleButton( LogoutButton, ProfileStrings.LogoutButtonTitle, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );


            // setup the pickers
            UILabel genderPickerLabel = new UILabel( );
            ControlStyling.StyleUILabel( genderPickerLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            genderPickerLabel.Text = ProfileStrings.SelectGenderLabel;

            GenderPicker = new PickerAdjustManager( View, ScrollView, genderPickerLabel, GenderLayer );
            UIPickerView genderPicker = new UIPickerView();
            genderPicker.Model = new GenderPickerModel() { Parent = this };
            GenderPicker.SetPicker( genderPicker );


            UILabel birthdatePickerLabel = new UILabel( );
            ControlStyling.StyleUILabel( birthdatePickerLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            birthdatePickerLabel.Text = ProfileStrings.SelectBirthdateLabel;
            BirthdatePicker = new PickerAdjustManager( View, ScrollView, birthdatePickerLabel, BirthdateLayer );

            UIDatePicker datePicker = new UIDatePicker();
            datePicker.SetValueForKey( UIColor.White, new NSString( "textColor" ) );
            datePicker.Mode = UIDatePickerMode.Date;
            datePicker.MinimumDate = new DateTime( 1900, 1, 1 ).DateTimeToNSDate( );
            datePicker.MaximumDate = DateTime.Now.DateTimeToNSDate( );
            datePicker.ValueChanged += (object sender, EventArgs e ) =>
            {
                NSDate pickerDate = ((UIDatePicker) sender).Date;
                BirthdateText.Text = string.Format( "{0:MMMMM dd yyyy}", pickerDate.NSDateToDateTime( ) );
            };
            BirthdatePicker.SetPicker( datePicker );


            // Allow the return on username and password to start
            // the login process
            NickNameText.ShouldReturn += TextFieldShouldReturn;
            LastNameText.ShouldReturn += TextFieldShouldReturn;

            EmailText.ShouldReturn += TextFieldShouldReturn;

            // If submit is pressed with dirty changes, prompt the user to save them.
            DoneButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( GenderPicker.Revealed == false && BirthdatePicker.Revealed == false)
                    {
                        if( Dirty == true )
                        {
                            // if there were changes, create an action sheet for them to confirm.
                            var actionSheet = new UIActionSheet( ProfileStrings.SubmitChangesTitle );
                            actionSheet.AddButton( GeneralStrings.Yes );
                            actionSheet.AddButton( GeneralStrings.No );
                            actionSheet.AddButton( GeneralStrings.Cancel );

                            actionSheet.CancelButtonIndex = 2;

                            actionSheet.Clicked += SubmitActionSheetClicked;

                            actionSheet.ShowInView( View );
                        }
                        else
                        {
                            Springboard.ResignModelViewController( this, null );
                        }
                    }
                    else
                    {
                        GenderPicker.TogglePicker( false );
                        BirthdatePicker.TogglePicker( false );
                        Dirty = true;
                    }
                };

            // On logout, make sure the user really wants to log out.
            LogoutButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( GenderPicker.Revealed == false && BirthdatePicker.Revealed == false)
                    {
                        // if they tap logout, and confirm it
                        var actionSheet = new UIActionSheet( ProfileStrings.LogoutTitle, null, GeneralStrings.Cancel, GeneralStrings.Yes, null );

                        actionSheet.ShowInView( View );

                        actionSheet.Clicked += (object s, UIButtonEventArgs ev) => 
                            {
                                if( ev.ButtonIndex == actionSheet.DestructiveButtonIndex )
                                {
                                    // then log them out.
                                    RockMobileUser.Instance.LogoutAndUnbind( );

                                    Springboard.ResignModelViewController( this, null );
                                }
                            };
                    }
                    else
                    {
                        GenderPicker.TogglePicker( false );
                        BirthdatePicker.TogglePicker( false );
                        Dirty = true;
                    }
                };

            Dirty = false;

            // logged in sanity check.
            if( RockMobileUser.Instance.LoggedIn == false ) throw new Exception("A user must be logged in before viewing a profile. How did you do this?" );

            // setup the fake header
            HeaderView.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrimaryNavBarConfig.LogoFile;
            LogoView = new UIImageView( new UIImage( imagePath ) );
            HeaderView.AddSubview( LogoView );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            ScrollView.ContentSize = new CGSize( 0, View.Bounds.Height + ( View.Bounds.Height * .25f ) );

            // setup the header shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( HeaderView.Bounds );
            HeaderView.Layer.MasksToBounds = false;
            HeaderView.Layer.ShadowColor = UIColor.Black.CGColor;
            HeaderView.Layer.ShadowOffset = new CGSize( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            LogoView.Layer.Position = new CGPoint( HeaderView.Bounds.Width / 2, HeaderView.Bounds.Height / 2 );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ScrollView.ContentOffset = CGPoint.Empty;

            // set values
            NickNameText.Text = RockMobileUser.Instance.Person.NickName;
            LastNameText.Text = RockMobileUser.Instance.Person.LastName;
            EmailText.Text = RockMobileUser.Instance.Person.Email;

            // setup the phone number
            CellPhoneText.Delegate = new Rock.Mobile.PlatformSpecific.iOS.UI.PhoneNumberFormatterDelegate();
            CellPhoneText.Text = RockMobileUser.Instance.TryGetPhoneNumber( CCVApp.Shared.Config.GeneralConfig.CellPhoneValueId ).Number;
            CellPhoneText.Delegate.ShouldChangeCharacters( CellPhoneText, new NSRange( CellPhoneText.Text.Length, 0 ), "" );

            // address
            StreetText.Text = RockMobileUser.Instance.Street1( );
            CityText.Text = RockMobileUser.Instance.City( );
            StateText.Text = RockMobileUser.Instance.State( );
            ZipText.Text = RockMobileUser.Instance.Zip( );

            // gender
            if ( RockMobileUser.Instance.Person.Gender > 0 )
            {
                GenderText.Text = CCVApp.Shared.Network.RockGeneralData.Instance.Data.Genders[ RockMobileUser.Instance.Person.Gender ];
            }
            else
            {
                GenderText.Text = string.Empty;
            }

            if ( RockMobileUser.Instance.Person.BirthDate.HasValue == true )
            {
                BirthdateText.Text = string.Format( "{0:MMMMM dd yyyy}", RockMobileUser.Instance.Person.BirthDate );
            }
            else
            {
                BirthdateText.Text = string.Empty;
            }
        }

        public void SubmitActionSheetClicked(object sender, UIButtonEventArgs e)
        {
            switch( e.ButtonIndex )
            {
                // submit
                case 0: Dirty = false; SubmitChanges( ); Springboard.ResignModelViewController( this, null ); break;

                // No, don't submit
                case 1: Dirty = false; Springboard.ResignModelViewController( this, null ); break;

                // cancel
                case 2: break;
            }
        }

        public bool TextFieldShouldReturn( UITextField textField )
        {
            if( textField.IsFirstResponder == true )
            {
                textField.ResignFirstResponder();
                return true;
            }

            return false;
        }

        void SubmitChanges()
        {
            // copy all the edited fields into the person object
            RockMobileUser.Instance.Person.Email = EmailText.Text;

            RockMobileUser.Instance.Person.NickName = NickNameText.Text;
            RockMobileUser.Instance.Person.LastName = LastNameText.Text;

            // Update their cell phone. 
            if ( string.IsNullOrEmpty( CellPhoneText.Text ) == false )
            {
                // update the phone number
                RockMobileUser.Instance.UpdateOrAddPhoneNumber( CellPhoneText.Text.AsNumeric( ), CCVApp.Shared.Config.GeneralConfig.CellPhoneValueId );
            }

            // Gender
            if ( string.IsNullOrEmpty( GenderText.Text ) == false )
            {
                RockMobileUser.Instance.Person.Gender = CCVApp.Shared.Network.RockGeneralData.Instance.Data.Genders.IndexOf( GenderText.Text );
            }

            // Birthdate
            if ( string.IsNullOrEmpty( BirthdateText.Text ) == false )
            {
                RockMobileUser.Instance.Person.BirthDate = DateTime.Parse( BirthdateText.Text );
            }

            // request the person object be sync'd with the server. because we save the object locally,
            // if the sync fails, the profile will try again at the next login
            RockMobileUser.Instance.UpdateProfile( null );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // if we're picking a gender, don't allow anything else.
            if ( GenderPicker.Revealed == true )
            {
                GenderPicker.TogglePicker( false );
                Dirty = true;
            }
            else if ( BirthdatePicker.Revealed == true )
            {
                BirthdatePicker.TogglePicker( false );
                Dirty = true;
            }
            else
            {
                base.TouchesEnded( touches, evt );
            
                // if they tap somewhere outside of the text fields, 
                // hide the keyboard
                TextFieldShouldReturn( NickNameText );
                TextFieldShouldReturn( LastNameText );

                TextFieldShouldReturn( CellPhoneText );
                TextFieldShouldReturn( EmailText );

                TextFieldShouldReturn( StreetText );
                TextFieldShouldReturn( CityText );
                TextFieldShouldReturn( StateText );
                TextFieldShouldReturn( ZipText );

                TextFieldShouldReturn( BirthdateText );
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        /// <summary>
        /// Called when the user selects something in the UIPicker
        /// </summary>
        public void PickerSelected( int row, int component )
        {
            // set the button's text to be the item they selected. Note that we now change the color to Active from the original Placeholder
            GenderText.Text = CCVApp.Shared.Network.RockGeneralData.Instance.Data.Genders[ row ];
            GenderText.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
        }

        /// <summary>
        /// The model that defines the object that will be selected in the UI Picker
        /// </summary>
        public class GenderPickerModel : UIPickerViewModel
        {
            public ProfileViewController Parent { get; set; }

            public override nint GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView picker, nint component)
            {
                return RockGeneralData.Instance.Data.Genders.Count - 1;
            }

            public override string GetTitle(UIPickerView picker, nint row, nint component)
            {
                return RockGeneralData.Instance.Data.Genders[ (int) (row + 1) ];
            }

            public override void Selected(UIPickerView picker, nint row, nint component)
            {
                Parent.PickerSelected( (int) (row + 1), (int) component );
            }

            public override UIView GetView(UIPickerView picker, nint row, nint component, UIView view)
            {
                UILabel label = view as UILabel;
                if ( label == null )
                {
                    label = new UILabel();
                    label.TextColor = UIColor.White;
                    label.Text = RockGeneralData.Instance.Data.Genders[ (int) (row + 1) ];
                    label.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
                    label.SizeToFit( );
                }

                return label;
            }
        }
	}
}
