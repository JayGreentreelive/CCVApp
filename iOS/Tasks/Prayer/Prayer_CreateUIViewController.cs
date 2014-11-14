using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	partial class Prayer_CreateUIViewController : TaskUIViewController
	{
		public Prayer_CreateUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // setup the default control styles
            ActivityIndicator.Hidden = true;
            FirstNameText.Enabled = true;
            LastNameText.Enabled = true;
            RequestText.Editable = true;

            // the heart of this is the submit button
            SubmitButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // ensure that at least the first name and request are valid
                    if( string.IsNullOrEmpty( FirstNameText.Text ) == false &&
                        string.IsNullOrEmpty( RequestText.Text ) == false )
                    {
                        Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                        FirstNameText.Enabled = false;
                        LastNameText.Enabled = false;
                        RequestText.Editable = false;

                        prayerRequest.FirstName = FirstNameText.Text;
                        prayerRequest.LastName = LastNameText.Text;
                        prayerRequest.Text = RequestText.Text;
                        prayerRequest.EnteredDateTime = DateTime.Now;

                        ActivityIndicator.Hidden = false;

                        // submit the request
                        CCVApp.Shared.Network.RockApi.Instance.PutPrayer( prayerRequest, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                            {
                                ActivityIndicator.Hidden = true;

                                if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                {
                                    NavigationController.PopViewControllerAnimated( true );
                                }
                                else
                                {
                                    SpringboardViewController.DisplayError( CCVApp.Shared.Strings.Prayer.Error_Title, CCVApp.Shared.Strings.Prayer.Error_Submit_Message );

                                    FirstNameText.Enabled = true;
                                    LastNameText.Enabled = true;
                                    RequestText.Editable = true;
                                }
                            });
                    }
                };
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
