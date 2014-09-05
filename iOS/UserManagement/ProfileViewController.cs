using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;

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

		public ProfileViewController (IntPtr handle) : base (handle)
		{
		}

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Allow the return on username and password to start
            // the login process
            TitleField.ShouldReturn += TextFieldShouldReturn;

            FirstNameField.ShouldReturn += TextFieldShouldReturn;
            MiddleNameField.ShouldReturn += TextFieldShouldReturn;
            LastNameField.ShouldReturn += TextFieldShouldReturn;

            NickNameField.ShouldReturn += TextFieldShouldReturn;

            EmailField.ShouldReturn += TextFieldShouldReturn;

            // If submit is pressed with dirty changes, prompt the user to save them.
            SubmitButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( Dirty == true )
                    {
                        // if there were changes, create an action sheet for them to confirm.
                        var actionSheet = new UIActionSheet( "Want to submit your changes?" );
                        actionSheet.AddButton( "Submit" );
                        actionSheet.AddButton( "No Thanks" );
                        actionSheet.AddButton( "Cancel" );

                        actionSheet.CancelButtonIndex = 2;

                        actionSheet.Clicked += SubmitActionSheetClicked;

                        actionSheet.ShowInView( View );
                    }
                    else
                    {
                        Springboard.ResignModelViewController( this );
                    }
                };

            // On logout, make sure the user really wants to log out.
            LogOutButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // if they tap logout, and confirm it
                    var actionSheet = new UIActionSheet( "Are you sure you want to logout?", null, "Cancel", "Logout", null );

                    actionSheet.ShowInView( View );

                    actionSheet.Clicked += (object s, UIButtonEventArgs ev) => 
                        {
                            if( ev.ButtonIndex == actionSheet.DestructiveButtonIndex )
                            {
                                // then log them out.
                                MobileUser.Instance.Logout( );

                                Springboard.ResignModelViewController( this );
                            }
                        };

                };
        }

        public void SubmitActionSheetClicked(object sender, UIButtonEventArgs e)
        {
            switch( e.ButtonIndex )
            {
                // submit
                case 0: SubmitChanges( ); break;

                // No, don't submit
                case 1: Springboard.ResignModelViewController( this ); break;

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
            MobileUser.Instance.Person.Email = EmailField.Text;

            MobileUser.Instance.Person.MiddleName = MiddleNameField.Text;

            MobileUser.Instance.Person.NickName = NickNameField.Text;

            MobileUser.Instance.Person.FirstName = FirstNameField.Text;
            MobileUser.Instance.Person.LastName = LastNameField.Text;

            // request the person object be sync'd with the server. because we save the object locally,
            // if the sync fails, the profile will try again at the next login
            MobileUser.Instance.UpdateProfile( delegate { Springboard.ResignModelViewController( this ); });
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // if they tap somewhere outside of the text fields, 
            // hide the keyboard
            TextFieldShouldReturn( TitleField );

            TextFieldShouldReturn( FirstNameField );
            TextFieldShouldReturn( MiddleNameField );
            TextFieldShouldReturn( LastNameField );

            TextFieldShouldReturn( NickNameField );

            TextFieldShouldReturn( EmailField );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            Dirty = false;

            // logged in sanity check.
            if( MobileUser.Instance.LoggedIn == false ) throw new Exception("A user must be logged in before viewing a profile. How did you do this?" );

            HeaderLabel.Text = string.Format( "Hey {0}", MobileUser.Instance.PreferredName( ) );

            TitleField.Text = "";

            FirstNameField.Text = MobileUser.Instance.Person.FirstName;
            MiddleNameField.Text = MobileUser.Instance.Person.MiddleName;
            LastNameField.Text = MobileUser.Instance.Person.LastName;

            NickNameField.Text = MobileUser.Instance.Person.NickName;

            EmailField.Text = MobileUser.Instance.Person.Email;
        }
	}
}
