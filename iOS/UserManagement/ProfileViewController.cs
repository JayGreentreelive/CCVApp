using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using CCVApp.Shared.Network;
using MonoTouch.CoreAnimation;
using System.Drawing;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;

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

            //setup styles
            View.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );

            ControlStyling.StyleTextField( NickNameText, ProfileStrings.NickNamePlaceholder );
            ControlStyling.StyleBGLayer( NickNameLayer );

            ControlStyling.StyleTextField( LastNameText, ProfileStrings.LastNamePlaceholder );
            ControlStyling.StyleBGLayer( LastNameLayer );


            ControlStyling.StyleTextField( EmailText, ProfileStrings.EmailPlaceholder );
            ControlStyling.StyleBGLayer( EmailLayer );

            ControlStyling.StyleTextField( CellPhoneText, ProfileStrings.CellPhonePlaceholder );
            ControlStyling.StyleBGLayer( CellPhoneLayer );


            ControlStyling.StyleTextField( StreetText, ProfileStrings.StreetPlaceholder );
            ControlStyling.StyleBGLayer( StreetLayer );

            ControlStyling.StyleTextField( CityText, ProfileStrings.CityPlaceholder );
            ControlStyling.StyleBGLayer( CityLayer );

            ControlStyling.StyleTextField( StateText, ProfileStrings.StatePlaceholder );
            ControlStyling.StyleBGLayer( StateLayer );

            ControlStyling.StyleTextField( ZipText, ProfileStrings.ZipPlaceholder );
            ControlStyling.StyleBGLayer( ZipLayer );


            ControlStyling.StyleTextField( GenderText, ProfileStrings.GenderPlaceholder );
            ControlStyling.StyleBGLayer( GenderLayer );

            ControlStyling.StyleTextField( BirthdateText, ProfileStrings.BirthdatePlaceholder );
            ControlStyling.StyleBGLayer( BirthdateLayer );

            ControlStyling.StyleButton( DoneButton, ProfileStrings.DoneButtonTitle );
            ControlStyling.StyleButton( LogoutButton, ProfileStrings.LogoutButtonTitle );

            // Allow the return on username and password to start
            // the login process
            NickNameText.ShouldReturn += TextFieldShouldReturn;
            LastNameText.ShouldReturn += TextFieldShouldReturn;

            EmailText.ShouldReturn += TextFieldShouldReturn;

            // If submit is pressed with dirty changes, prompt the user to save them.
            DoneButton.TouchUpInside += (object sender, EventArgs e) => 
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
                };

            // On logout, make sure the user really wants to log out.
            LogoutButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // if they tap logout, and confirm it
                    var actionSheet = new UIActionSheet( ProfileStrings.LogoutTitle, null, GeneralStrings.Cancel, GeneralStrings.Yes, null );

                    actionSheet.ShowInView( View );

                    actionSheet.Clicked += (object s, UIButtonEventArgs ev) => 
                        {
                            if( ev.ButtonIndex == actionSheet.DestructiveButtonIndex )
                            {
                                // then log them out.
                                RockMobileUser.Instance.Logout( );

                                Springboard.ResignModelViewController( this, null );
                            }
                        };

                };

            Dirty = false;

            // logged in sanity check.
            if( RockMobileUser.Instance.LoggedIn == false ) throw new Exception("A user must be logged in before viewing a profile. How did you do this?" );

            NickNameText.Text = RockMobileUser.Instance.Person.NickName;
            LastNameText.Text = RockMobileUser.Instance.Person.LastName;

            EmailText.Text = RockMobileUser.Instance.Person.Email;

            // setup the fake header
            HeaderView.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrimaryNavBarConfig.LogoFile;
            LogoView = new UIImageView( new UIImage( imagePath ) );
            HeaderView.AddSubview( LogoView );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            ScrollView.ContentSize = new SizeF( 0, View.Bounds.Height + ( View.Bounds.Height * .25f ) );

            // setup the header shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( HeaderView.Bounds );
            HeaderView.Layer.MasksToBounds = false;
            HeaderView.Layer.ShadowColor = PlatformBaseUI.GetUIColor( PrimaryContainerConfig.ShadowColor ).CGColor;
            HeaderView.Layer.ShadowOffset = new System.Drawing.SizeF( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            LogoView.Layer.Position = new System.Drawing.PointF( HeaderView.Bounds.Width / 2, HeaderView.Bounds.Height / 2 );
        }

        public void SubmitActionSheetClicked(object sender, UIButtonEventArgs e)
        {
            switch( e.ButtonIndex )
            {
                // submit
                case 0: SubmitChanges( ); Springboard.ResignModelViewController( this, null ); break;

                // No, don't submit
                case 1: Springboard.ResignModelViewController( this, null ); break;

                // cancel
                case 2: break;
            }
        }

        public bool TextFieldShouldReturn( UITextField textField )
        {
            if( textField.IsFirstResponder == true )
            {
                textField.ResignFirstResponder();

                Dirty = true;

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

            // request the person object be sync'd with the server. because we save the object locally,
            // if the sync fails, the profile will try again at the next login
            RockMobileUser.Instance.UpdateProfile( null );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // if they tap somewhere outside of the text fields, 
            // hide the keyboard
            TextFieldShouldReturn( NickNameText );
            TextFieldShouldReturn( LastNameText );

            TextFieldShouldReturn( EmailText );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }
	}
}
