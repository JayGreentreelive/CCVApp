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

            //todo: clean up all this crappy code.

            ActivityIndicator.Hidden = true;

            FirstNameText.Enabled = true;
            LastNameText.Enabled = true;
            RequestText.Editable = true;

            SubmitButton.TouchUpInside += (object sender, EventArgs e) => 
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
                            NavigationController.PopViewControllerAnimated( true );
                        });
                };
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            FirstNameText.ResignFirstResponder( );
            LastNameText.ResignFirstResponder( );
            RequestText.ResignFirstResponder( );
        }
	}
}
