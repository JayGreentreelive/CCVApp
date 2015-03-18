using System;
using Rock.Mobile.PlatformUI;
using System.Drawing;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.Animation;
using System.IO;

namespace CCVApp.Shared.UI
{
    public class UIOOBE
    {
        public PlatformView View { get; set; }
        public PlatformImageView ImageLogo { get; set; }
        public PlatformImageView ImageBG { get; set; }

        public PlatformLabel WelcomeLabel { get; set; }

        public PlatformButton RegisterButton { get; set; }
        public PlatformButton LoginButton { get; set; }
        public PlatformButton SkipButton { get; set; }

        public UIOOBE( )
        {
        }

        enum OOBE_State
        {
            Startup,
            Welcome,
            RevealControls
        }
        OOBE_State State { get; set; }

        public delegate void OnButtonClick( int index );

        public void Create( object masterView, string bgLayerImageName, string logoImageName, RectangleF frame, OnButtonClick onClick )
        {
            View = PlatformView.Create( );
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );
            View.BackgroundColor = 0x5b0f10ff;
            View.AddAsSubview( masterView );

            ImageBG = PlatformImageView.Create( );
            ImageBG.AddAsSubview( masterView );
            MemoryStream stream = Rock.Mobile.Util.FileIO.AssetConvert.AssetToStream( bgLayerImageName );
            stream.Position = 0;
            ImageBG.Opacity = 0;
            ImageBG.Image = stream;
            ImageBG.SizeToFit( );
            ImageBG.ImageScaleType = PlatformImageView.ScaleType.Center;

            WelcomeLabel = PlatformLabel.Create( );
            WelcomeLabel.SetFont( ControlStylingConfig.Large_Font_Light, ControlStylingConfig.Large_FontSize );
            WelcomeLabel.TextColor = 0xCCCCCCFF;
            WelcomeLabel.Text = OOBEConfig.Welcome;
            WelcomeLabel.Opacity = 0;
            WelcomeLabel.SizeToFit( );
            WelcomeLabel.Position = new PointF( (( View.Frame.Width - WelcomeLabel.Frame.Width ) / 2), View.Frame.Height * .35f );
            WelcomeLabel.AddAsSubview( masterView );


            RegisterButton = PlatformButton.Create( );
            RegisterButton.SetFont( ControlStylingConfig.Large_Font_Light, ControlStylingConfig.Large_FontSize );
            RegisterButton.TextColor = 0xCCCCCCFF;
            RegisterButton.Text = string.Format( OOBEConfig.WantAccount, GeneralConfig.OrganizationShortName );
            RegisterButton.Opacity = 0;
            RegisterButton.SizeToFit( );
            RegisterButton.Position = new PointF( ( ( View.Frame.Width - RegisterButton.Frame.Width ) / 2 ), WelcomeLabel.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 33 ) );
            RegisterButton.ClickEvent = (PlatformButton button ) =>
            {
                onClick( 0 );
            };
            RegisterButton.AddAsSubview( masterView );


            LoginButton = PlatformButton.Create( );
            LoginButton.SetFont( ControlStylingConfig.Large_Font_Light, ControlStylingConfig.Large_FontSize );
            LoginButton.TextColor = 0xCCCCCCFF;
            LoginButton.Text = string.Format( OOBEConfig.HaveAccount, GeneralConfig.OrganizationShortName );
            LoginButton.Opacity = 0;
            LoginButton.SizeToFit( );
            LoginButton.Position = new PointF( ( ( View.Frame.Width - LoginButton.Frame.Width ) / 2 ), RegisterButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 33 ) );
            LoginButton.ClickEvent = (PlatformButton button ) =>
                {
                    onClick( 1 );
                };
            LoginButton.AddAsSubview( masterView );


            SkipButton = PlatformButton.Create( );
            SkipButton.SetFont( ControlStylingConfig.Large_Font_Light, ControlStylingConfig.Large_FontSize );
            SkipButton.TextColor = 0xCCCCCCFF;
            SkipButton.Text = OOBEConfig.SkipAccount;
            SkipButton.Opacity = 0;
            SkipButton.SizeToFit( );
            SkipButton.Position = new PointF( ( ( View.Frame.Width - SkipButton.Frame.Width ) / 2 ), LoginButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 33 ) );
            SkipButton.ClickEvent = (PlatformButton button ) =>
                {
                    onClick( 2 );
                };
            SkipButton.AddAsSubview( masterView );


            stream = Rock.Mobile.Util.FileIO.AssetConvert.AssetToStream( logoImageName );
            stream.Position = 0;
            ImageLogo = PlatformImageView.Create( );
            ImageLogo.AddAsSubview( masterView );
            ImageLogo.Image = stream;
            ImageLogo.SizeToFit( );
            ImageLogo.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;

            ImageLogo.Frame = new RectangleF( (( View.Frame.Width - ImageLogo.Frame.Width ) / 2), (( View.Frame.Height - ImageLogo.Frame.Height ) / 2) + 2, ImageLogo.Frame.Width, ImageLogo.Frame.Height );

            State = OOBE_State.Startup;
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            ImageLogo.Destroy( );
            ImageBG.Destroy( );
        }

        public void PerformStartup( )
        {
            // Fade in the background image
            SimpleAnimator_Float imageBGAlphaAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .25f, delegate(float percent, object value )
                {
                    ImageBG.Opacity = (float)value;
                },
                null );
            imageBGAlphaAnim.Start( );

            /*SimpleAnimator_Color bgColorAnim = new SimpleAnimator_Color( View.BackgroundColor, ControlStylingConfig.BackgroundColor, .25f, delegate(float percent, object value )
                {
                    View.BackgroundColor = (uint)value;
                },
                null );
            bgColorAnim.Start( );*/

            // Fade OUT the logo
            SimpleAnimator_Float imageAlphaAnim = new SimpleAnimator_Float( ImageLogo.Opacity, 0.00f, .13f, delegate(float percent, object value )
                {
                    ImageLogo.Opacity = (float)value;
                },
                null );
            imageAlphaAnim.Start( );

            // Scale UP the logo
            SimpleAnimator_SizeF imageSizeAnim = new SimpleAnimator_SizeF( ImageLogo.Frame.Size, new SizeF( View.Frame.Width, View.Frame.Height ), .25f, delegate(float percent, object value )
                {
                    SizeF imageSize = (SizeF)value;
                    ImageLogo.Frame = new RectangleF( ( View.Frame.Width - imageSize.Width ) / 2, ( View.Frame.Height - imageSize.Height ) / 2, imageSize.Width, imageSize.Height );
                },
                delegate 
                {
                    // when finished, wait, then go to the next state
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = 500;
                    timer.AutoReset = false;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            // do this ON the UI thread
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    View.BackgroundColor = ControlStylingConfig.BackgroundColor;
                                    EnterNextState( OOBE_State.Welcome );
                                });
                        };
                    timer.Start( );
                } );
            imageSizeAnim.Start( );
        }

        void PerformWelcome( )
        {
            SimpleAnimator_Float anim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                {
                    WelcomeLabel.Opacity = (float)value;
                },
                delegate
                {
                    // when finished, wait, then go to the next state
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = 1000;
                    timer.AutoReset = false;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            // do this ON the UI thread
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                EnterNextState( OOBE_State.RevealControls );
                            });
                        };
                    timer.Start( );
                } );
            anim.Start( );
        }

        void PerformRevealControls( )
        {
            // this will be fun. Chain the animations so they go serially. Start with moving up Welcome
            SimpleAnimator_PointF posAnim = new SimpleAnimator_PointF( WelcomeLabel.Position, new PointF( WelcomeLabel.Position.X, View.Frame.Height * .25f ), .25f,
                delegate(float posPercent, object posValue )
                {
                    WelcomeLabel.Position = (PointF) posValue;
                },
                delegate
                {
                    // now fade in Register
                    SimpleAnimator_Float regAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                        {
                            RegisterButton.Opacity = (float)value;

                        },
                        delegate
                        {
                            // now Login
                            SimpleAnimator_Float loginAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                                {
                                    LoginButton.Opacity = (float)value;
                                },
                                delegate
                                {
                                    // finally skip
                                    SimpleAnimator_Float skipAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                                        {
                                            SkipButton.Opacity = (float)value;
                                        },
                                        delegate
                                        {
                                        });
                                    skipAnim.Start( );
                                });
                                loginAnim.Start( );
                        } );
                    regAnim.Start( );
                    
                });
            posAnim.Start( );
        }

        void EnterNextState( OOBE_State nextState )
        {
            switch( nextState )
            {
                case OOBE_State.Startup:
                {
                    PerformStartup( ); 
                    break;
                }

                case OOBE_State.Welcome:
                {
                    PerformWelcome( );
                    break;
                }

                case OOBE_State.RevealControls:
                {
                    PerformRevealControls( );
                    break;
                }
            }

            State = nextState;
        }
    }
}

