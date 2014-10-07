using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.CoreImage;

namespace iOS
{
	partial class ImageCropViewController : UIViewController
	{
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// The source image to crop
        /// </summary>
        /// <value>The source image.</value>
        UIImage SourceImage { get; set; }

        /// <summary>
        /// The image view used to display the source and cropped images.
        /// </summary>
        /// <value>The image view.</value>
        UIImageView ImageView { get; set; }

        /// <summary>
        /// The view representing the region of the image to crop to.
        /// </summary>
        /// <value>The crop view.</value>
        UIView CropView { get; set; }

        /// <summary>
        /// Gets or sets the crop view minimum position.
        /// </summary>
        /// <value>The crop view minimum position.</value>
        PointF CropViewMinPos { get; set; }

        /// <summary>
        /// Gets or sets the crop view max position.
        /// </summary>
        /// <value>The crop view max position.</value>
        PointF CropViewMaxPos { get; set; }

        /// <summary>
        /// Scalar to convert from screen points to image pixels and back
        /// </summary>
        float ScreenToImageScalar { get; set; }

        /// <summary>
        /// The last touch position received. Used for calculating the delta
        /// movement when moving the CropView
        /// </summary>
        /// <value>The last tap position.</value>
        PointF LastTapPos { get; set; }

        /// <summary>
        /// The resulting cropped image
        /// </summary>
        /// <value>The cropped image.</value>
        UIImage CroppedImage { get; set; }

        /// <summary>
        /// Crop mode.
        /// </summary>
        enum CropMode
        {
            None,
            Editing,
            Previewing
        }

        /// <summary>
        /// Determines whether we're editing or previewing the crop
        /// </summary>
        /// <value>The mode.</value>
        CropMode Mode { get; set; }

        /// <summary>
        /// The aspect ratio we should be cropping the picture to.
        /// Example: 1.0f would mean 1:1 width/height, or a square.
        /// 9 / 16 would mean 9:16 (or 16:9), which is "wide screen" like a movie.
        /// </summary>
        /// <value>The crop aspect ratio.</value>
        float CropAspectRatio { get; set; }

        UIButton CancelButton { get; set; }
        UIButton EditButton { get; set; }

		public ImageCropViewController (IntPtr handle) : base (handle)
		{

		}

        public void Begin( UIImage image, float cropAspectRatio )
        {
            SourceImage = image;
            CropAspectRatio = cropAspectRatio;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // set the image
            ImageView = new UIImageView( );
            ImageView.BackgroundColor = UIColor.Black;
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.Frame = View.Frame;

            View.AddSubview( ImageView );


            // create our cropper
            CropView = new UIView( );
            CropView.BackgroundColor = UIColor.Clear;
            CropView.Layer.BorderColor = UIColor.White.CGColor;
            CropView.Layer.BorderWidth = 2;
            CropView.Layer.CornerRadius = 4;

            View.AddSubview( CropView );


            // create our bottom toolbar
            UIToolbar toolbar = new UIToolbar( new RectangleF( 0, View.Bounds.Height - 40, View.Bounds.Width, 40 ) );

            // create the cancel button
            NSString cancelLabel = new NSString( CCVApp.Shared.Config.ImageCrop.CropCancelButton_Text );

            CancelButton = new UIButton(UIButtonType.System);
            CancelButton.Font = Rock.Mobile.PlatformCommon.iOS.LoadFontDynamic( CCVApp.Shared.Config.ImageCrop.CropCancelButton_Font, CCVApp.Shared.Config.ImageCrop.CropCancelButton_Size );
            CancelButton.SetTitle( cancelLabel.ToString( ), UIControlState.Normal );

            SizeF buttonSize = cancelLabel.StringSize( CancelButton.Font );
            CancelButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    SetMode( CropMode.Editing );
                };

            // create the edit button
            NSString editLabel = new NSString( CCVApp.Shared.Config.ImageCrop.CropOkButton_Text );

            EditButton = new UIButton(UIButtonType.System);
            EditButton.Font = Rock.Mobile.PlatformCommon.iOS.LoadFontDynamic( CCVApp.Shared.Config.ImageCrop.CropOkButton_Font, CCVApp.Shared.Config.ImageCrop.CropOkButton_Size );
            EditButton.SetTitle( editLabel.ToString( ), UIControlState.Normal );
            EditButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;

            // determine its dimensions
            buttonSize = editLabel.StringSize( EditButton.Font );
            EditButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
            EditButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( Mode == CropMode.Previewing )
                    {
                        // confirm we're done
                        Springboard.ResignModelViewController( this, CroppedImage );
                    }
                    else
                    {
                        SetMode( CropMode.Previewing );
                    }
                };

            // create a container that will allow us to align the buttons
            UIView buttonContainer = new UIView( new RectangleF( 0, View.Bounds.Height - 40, View.Bounds.Width, 40 ) );
            buttonContainer.AddSubview( EditButton );
            buttonContainer.AddSubview( CancelButton );

            CancelButton.Frame = new RectangleF( (CancelButton.Frame.Width), 0, CancelButton.Frame.Width, CancelButton.Frame.Height );
            EditButton.Frame = new RectangleF( buttonContainer.Frame.Width - ((EditButton.Frame.Width / 2) + (EditButton.Frame.Width * 2)), 0, EditButton.Frame.Width, EditButton.Frame.Height );

            toolbar.SetItems( new UIBarButtonItem[] { new UIBarButtonItem( buttonContainer ) }, false );
            View.AddSubview( toolbar );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // scale the image to match the view's width
            ScreenToImageScalar = (float) SourceImage.Size.Width / (float) View.Bounds.Width;

            // get the scaled dimensions, maintaining aspect ratio
            float scaledWidth = (float)SourceImage.Size.Width * (1.0f / ScreenToImageScalar);
            float scaledHeight = (float)SourceImage.Size.Height * (1.0f / ScreenToImageScalar);


            // set the crop bounds
            CropView.Frame = new RectangleF( View.Frame.X, View.Frame.Y, scaledWidth, scaledWidth * CropAspectRatio );


            // set our clamp values
            CropViewMinPos = new PointF( (View.Bounds.Width - scaledWidth) / 2,
                                         (View.Bounds.Height - scaledHeight) / 2 );

            CropViewMaxPos = new PointF( CropViewMinPos.X + (scaledWidth - CropView.Bounds.Width),
                                         CropViewMinPos.Y + (scaledHeight - CropView.Bounds.Height) );




            // start in editing mode (obviously)
            SetMode( CropMode.Editing );

            CropView.Layer.Position = new PointF( 0, 0 );

            // force the crop view into correct position
            MoveCropView( new PointF( 0, 0 ) );
        }

        void MoveCropView( PointF delta )
        {
            // update the crop view by how much it should be moved
            float xPos = CropView.Frame.X + delta.X;
            float yPos = CropView.Frame.Y + delta.Y;

            // clamp to valid bounds
            xPos = Math.Max( CropViewMinPos.X, Math.Min( xPos, CropViewMaxPos.X ) );
            yPos = Math.Max( CropViewMinPos.Y, Math.Min( yPos, CropViewMaxPos.Y ) );

            CropView.Frame = new RectangleF( xPos, yPos, CropView.Frame.Width, CropView.Frame.Height );
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                LastTapPos = touch.LocationInView( View );
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            // adjust by the amount moved
            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                PointF touchPoint = touch.LocationInView( View );

                PointF delta = new PointF( touchPoint.X - LastTapPos.X, touchPoint.Y - LastTapPos.Y );

                MoveCropView( delta );

                LastTapPos = touchPoint;
            }
        }

        void SetMode( CropMode mode )
        {
            if( mode == Mode )
            {
                throw new Exception( string.Format( "Crop Mode {0} requested, but already in that mode.", mode ) );
            }

            switch( mode )
            {
                case CropMode.Editing:
                {
                    // there's nothing to cancel while editing, so disable the button.
                    CancelButton.Enabled = false;

                    // turn on our cropper
                    CropView.Hidden = false;

                    // and set the source image to the scaled source.
                    ImageView.Image = SourceImage;
                    break;
                }

                case CropMode.Previewing:
                {
                    // allow them to cancel this crop and try again.
                    CancelButton.Enabled = true;

                    // create the cropped image
                    CroppedImage = CropImage( SourceImage, new System.Drawing.RectangleF( CropView.Frame.X - CropViewMinPos.X, 
                                                                                          CropView.Frame.Y - CropViewMinPos.Y, 
                                                                                          CropView.Frame.Width, 
                                                                                          CropView.Frame.Height ) );

                    // set the scaled cropped image
                    ImageView.Image = CroppedImage;

                    // hide the cropper
                    CropView.Hidden = true;
                    break;
                }
            }

            Mode = mode;
        }

        UIImage CropImage( UIImage sourceImage, RectangleF cropDimension )
        {
            // convert our position on screen to where it should be in the image
            float pixelX = cropDimension.X * ScreenToImageScalar;
            float pixelY = cropDimension.Y * ScreenToImageScalar;

            // same for height, since the image was scaled down to fit the screen.
            float width = (float) cropDimension.Width * ScreenToImageScalar;
            float height = (float) cropDimension.Height * ScreenToImageScalar;

            // create our transform and apply it to our crop region, so we crop in the image's space
            CGAffineTransform transform = CGAffineTransform.MakeIdentity( );

            float rotationDegrees = 0.0f;
            switch( sourceImage.Orientation )
            {
                case UIImageOrientation.Up: break;

                case UIImageOrientation.RightMirrored:
                case UIImageOrientation.Right: rotationDegrees = -90; break;

                case UIImageOrientation.LeftMirrored:
                case UIImageOrientation.Left: rotationDegrees = 90; break;

                case UIImageOrientation.DownMirrored:
                case UIImageOrientation.Down: rotationDegrees = -180; break;
            }
            transform.Rotate( rotationDegrees * Rock.Mobile.Math.Util.DegToRad );

            // pull the crop back by its width / height so we rotate about its center
            RectangleF scaledCrop = new RectangleF( pixelX - (width / 2), pixelY - (height / 2 ), width, height );
            RectangleF transformedCrop = transform.TransformRect( scaledCrop );

            // now restore its position so its pivot is in the corner again
            transformedCrop = new RectangleF( transformedCrop.X + (transformedCrop.Width / 2), 
                                              transformedCrop.Y + (transformedCrop.Height / 2 ), 
                                              transformedCrop.Width, 
                                              transformedCrop.Height );

            // crop the image with the transformed crop region
            CGImage croppedImage = sourceImage.CGImage.WithImageInRect( transformedCrop );


            // get the new image, but re-apply its transform
            CIImage ciImage = new CIImage( croppedImage );
            CIImage rotatedImage = ciImage.ImageByApplyingTransform( transform );

            // create a context and render it back out to a CGImage.
            CIContext ciContext = CIContext.FromOptions( null );
            CGImage rotatedCGImage = ciContext.CreateCGImage( rotatedImage, rotatedImage.Extent );

            return new UIImage( rotatedCGImage );
        }
	}
}
