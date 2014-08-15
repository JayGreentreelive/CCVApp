using System;
using System.Xml;
using System.Drawing;
using RockMobile.PlatformUI;

namespace Notes
{
    /// <summary>
    /// A header describes a Title, Speaker and Date with a default layout that can 
    /// be overridden via NoteScript or Style.
    /// </summary>
    public class Header : BaseControl
    {
        /// <summary>
        /// The Title control for the header.
        /// </summary>
        protected PlatformLabel mTitle;

        /// <summary>
        /// The Date control for the header.
        /// </summary>
        protected const float DEFAULT_DATE_Y_OFFSET = .15f;
        protected PlatformLabel mDate;

        /// <summary>
        /// The speaker control for the header
        /// </summary>
        protected const float DEFAULT_SPEAKER_Y_OFFSET = .15f;
        protected PlatformLabel mSpeaker;

        /// <summary>
        /// The bounds (including position) of the header.
        /// </summary>
        /// <value>The frame.</value>
        protected RectangleF Frame { get; set; }

        protected override void Initialize( )
        {
            base.Initialize( );

            mTitle = null;
            mDate = null;
            mSpeaker = null;
        }

        public Header( CreateParams parentParams, XmlReader reader )
        {
            Initialize( );

            // check for attributes we support
            RectangleF bounds = new RectangleF( );
            ParseCommonAttribs( reader, ref bounds );

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mHeaderContainer );

            // if our left position is requested as a %, then that needs to be % of parent width
            if( bounds.X < 1 )
            {
                bounds.X = parentParams.Width * bounds.X;
            }

            // if our top position is requested as a %, then that needs to be % of parent width
            if( bounds.Y < 1 )
            {
                bounds.Y = parentParams.Height * bounds.Y;
            }

            //WIDTH
            if( bounds.Width == 0 )
            {
                // if 0, just take the our parents width
                bounds.Width = Math.Max( 1, parentParams.Width - bounds.X );
            }
            // if < 1 it's a percent and we should convert
            else if( bounds.Width <= 1 )
            {
                bounds.Width = Math.Max( 1, parentParams.Width - bounds.X ) * bounds.Width;
            }

            // PADDING
            float leftPadding = Styles.Style.GetStyleValue( mStyle.mPaddingLeft, parentParams.Width );
            float rightPadding = Styles.Style.GetStyleValue( mStyle.mPaddingRight, parentParams.Width );
            float topPadding = Styles.Style.GetStyleValue( mStyle.mPaddingTop, parentParams.Height );
            float bottomPadding = Styles.Style.GetStyleValue( mStyle.mPaddingBottom, parentParams.Height );

            // now calculate the available width based on padding. (Don't actually change our width)
            float availableWidth = bounds.Width - leftPadding - rightPadding;


            bool finishedHeader = false;
            while( finishedHeader == false && reader.Read( ) )
            {
                // look for the next tag type
                switch( reader.NodeType )
                {
                    // we expect elements
                    case XmlNodeType.Element:
                    {
                        // determine which element it is and setup appropriately
                        switch( reader.Name )
                        {
                            case "Title":
                            {   
                                // check for attributes we support
                                RectangleF elementBounds = new RectangleF( 0, 0, availableWidth, parentParams.Height );
                                Parser.ParseBounds( reader, ref elementBounds );

                                ParseHeaderElement( reader, availableWidth, parentParams.Height, out mTitle, ref elementBounds, ref ControlStyles.mHeaderTitle );
                                break;
                            }

                            case "Date":
                            {
                                // check for attributes we support
                                RectangleF elementBounds = new RectangleF( 0, 0, availableWidth, parentParams.Height );
                                Parser.ParseBounds( reader, ref elementBounds );

                                ParseHeaderElement( reader, availableWidth, parentParams.Height, out mDate, ref elementBounds, ref ControlStyles.mHeaderDate );
                                break;
                            }

                            case "Speaker":
                            {
                                // check for attributes we support
                                RectangleF elementBounds = new RectangleF( 0, 0, availableWidth, parentParams.Height );
                                Parser.ParseBounds( reader, ref elementBounds );

                                ParseHeaderElement( reader, availableWidth, parentParams.Height, out mSpeaker, ref elementBounds, ref ControlStyles.mHeaderSpeaker );
                                break;
                            }
                        }
                        break;
                    }

                    case XmlNodeType.EndElement:
                    {
                        if( reader.Name == "Header" )
                        {
                            // flag that we're done reading the header
                            finishedHeader = true;
                        }
                        break;
                    }
                }
            }

            // offset the controls according to our layout
            mTitle.Position = new PointF( mTitle.Position.X + bounds.X + leftPadding, 
                                          mTitle.Position.Y + bounds.Y + topPadding );

            // guarantee date and speaker are below title.
            mDate.Position = new PointF( mDate.Position.X + bounds.X + leftPadding, 
                                         mTitle.Frame.Bottom + mDate.Position.Y + bounds.Y + topPadding );

            mSpeaker.Position = new PointF( mSpeaker.Position.X + bounds.X + leftPadding, 
                                            mTitle.Frame.Bottom + mSpeaker.Position.Y + bounds.Y + topPadding );

            // determine the lowest control
            float bottomY = mSpeaker.Frame.Bottom > mTitle.Frame.Bottom ? mSpeaker.Frame.Bottom : mTitle.Frame.Bottom;
            bottomY = bottomY > mDate.Frame.Bottom ? bottomY : mDate.Frame.Bottom;

            // set our bounds
            Frame = new RectangleF( bounds.X, bounds.Y, bounds.Width, bottomY + bottomPadding);
            base.DebugFrameView.Frame = Frame;
        }

        void ParseHeaderElement( XmlReader reader, float parentWidth, float parentHeight, out PlatformLabel element, ref RectangleF elementBounds, ref Styles.Style defaultStyle )
        {
            element = PlatformLabel.Create( );

            // if our left position is requested as a %, then that needs to be % of parent width
            if( elementBounds.X < 1 )
            {
                elementBounds.X = parentWidth * elementBounds.X;
            }

            // if our top position is requested as a %, then that needs to be % of parent width
            if( elementBounds.Y < 1 )
            {
                elementBounds.Y = parentHeight * elementBounds.Y;
            }

            // header elements are weird with styles. We don't want any of our parent's styles,
            // so we create our own and mix that with our defaults
            Styles.Style elementStyle = new Styles.Style( );
            elementStyle.Initialize( );

            Styles.Style.ParseStyleAttributesWithDefaults( reader, ref elementStyle, ref defaultStyle );

            element.SetFont( elementStyle.mFont.mName, elementStyle.mFont.mSize.Value );
            element.TextColor = elementStyle.mFont.mColor.Value;

            if( elementStyle.mBackgroundColor.HasValue )
            {
                element.BackgroundColor = elementStyle.mBackgroundColor.Value;
            }

            element.Bounds = elementBounds;

            // get text
            switch( elementStyle.mTextCase )
            {
                case Styles.TextCase.Upper:
                {
                    element.Text = reader.ReadElementContentAsString( ).ToUpper( );
                    break;
                }

                case Styles.TextCase.Lower:
                {
                    element.Text = reader.ReadElementContentAsString( ).ToLower( );
                    break;
                }

                case Styles.TextCase.Normal:
                {
                    element.Text = reader.ReadElementContentAsString( );
                    break;
                }
            }
            element.SizeToFit( );


            // horizontally position the controls according to their 
            // requested alignment
            Styles.Alignment controlAlignment = elementStyle.mAlignment.Value;

            // adjust by our position
            float xAdjust = 0;
            switch( controlAlignment )
            {
                case Styles.Alignment.Center:
                {
                    xAdjust = elementBounds.X + ( ( parentWidth / 2 ) - ( element.Bounds.Width / 2 ) );
                    element.TextAlignment = TextAlignment.Center;
                    break;
                }
                case Styles.Alignment.Right:
                {
                    xAdjust = elementBounds.X + ( parentWidth - element.Bounds.Width );
                    element.TextAlignment = TextAlignment.Right;
                    break;
                }
                case Styles.Alignment.Left:
                {
                    xAdjust = elementBounds.X;
                    element.TextAlignment = TextAlignment.Left;
                    break;
                }
            }

            // adjust position
            element.Position = new PointF( elementBounds.X + xAdjust, elementBounds.Y );
        }

        public override void AddOffset( float xOffset, float yOffset )
        {
            base.AddOffset( xOffset, yOffset );

            mTitle.Position = new PointF( mTitle.Position.X + xOffset, 
                                          mTitle.Position.Y + yOffset );

            mDate.Position = new PointF( mDate.Position.X + xOffset, 
                                         mDate.Position.Y + yOffset );

            mSpeaker.Position = new PointF( mSpeaker.Position.X + xOffset, 
                                            mSpeaker.Position.Y + yOffset );

            // update our bounds by the new offsets.
            Frame = new RectangleF( Frame.X + xOffset, Frame.Y + yOffset, Frame.Width, Frame.Height );
            base.DebugFrameView.Frame = Frame;
        }

        public override void AddToView( object obj )
        {
            mTitle.AddAsSubview( obj );
            mDate.AddAsSubview( obj );
            mSpeaker.AddAsSubview( obj );

            TryAddDebugLayer( obj );
        }

        public override void RemoveFromView( object obj )
        {
            mTitle.RemoveAsSubview( obj );
            mDate.RemoveAsSubview( obj );
            mSpeaker.RemoveAsSubview( obj );

            TryRemoveDebugLayer( obj );
        }

        public override RectangleF GetFrame( )
        {
            return Frame;
        }

        public override bool ShouldShowBulletPoint( )
        {
            // as a container, it wouldn't really make sense to show a bullet point.
            return false;
        }
    }
}
