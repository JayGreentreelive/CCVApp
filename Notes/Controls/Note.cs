using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;
using System.IO;
using Notes.Styles;

namespace Notes
{
    public class Note
    {
        public delegate void OnPreReqsComplete( Note note, Exception e );

        protected List<IUIControl> ChildControls { get; set; }

        public string NoteXml { get; protected set; }

        protected Style mStyle;

        protected RectangleF Bounds { get; set; }

        public static void HandlePreReqs( string noteXml, string styleXml, OnPreReqsComplete onPreReqsComplete )
        {
            // now use a reader to get each element
            XmlReader reader = XmlReader.Create( new StringReader( noteXml ) );

            string styleSheetUrl = "";

            bool finishedReading = false;
            while( finishedReading == false && reader.Read( ) )
            {
                // expect the first element to be "Note"
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        if( reader.Name == "Note" )
                        {
                            styleSheetUrl = reader.GetAttribute( "StyleSheet" );
                            if( styleSheetUrl == null )
                            {
                                throw new InvalidDataException( "Could not find attribute 'StyleSheet'. This should be a URL pointing to the style to use." );
                            }
                        }
                        else
                        {
                            throw new InvalidDataException( string.Format( "Expected root element to be <Note>. Found <{0}>", reader.Name ) );
                        }

                        finishedReading = true;
                        break;
                    }
                }
            }

            // Parse the styles. We cannot go any further until this is finished.
            ControlStyles.Initialize( styleSheetUrl, styleXml, (Exception e ) =>
                {
                    // We don't just create the note here because the parent
                    // might need to change threads before creating UI objects
                    onPreReqsComplete( new Note( noteXml ), e );
                } );
        }

        public Note( string noteXml )
        {
            // store our XML
            NoteXml = noteXml;

            mStyle = new Style( );
            mStyle.Initialize( );
        }

        public void Create( float parentWidth, float parentHeight )
        {
            // create a child control list
            ChildControls = new List<IUIControl>( );

            // now use a reader to get each element
            XmlReader reader = XmlReader.Create( new StringReader( NoteXml ) );

            // begin reading the xml stream
            bool finishedReading = false;
            while( finishedReading == false && reader.Read( ) )
            {
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        if( reader.Name == "Note" )
                        {
                            ParseNote( reader, parentWidth, parentHeight );
                        }
                        else
                        {
                            throw new ArgumentException( String.Format( "Expected <Note> elemtn. Found <{0}> instead.", reader.Name ) );
                        }

                        finishedReading = true;
                        break;
                    }
                }
            }
        }

        public void Destroy( object obj )
        {
            // release references to our UI objects
            if( ChildControls != null )
            {
                foreach( IUIControl uiControl in ChildControls )
                {
                    uiControl.RemoveFromView( obj );
                }

                // and clear our UI list
                ChildControls.Clear( );

                NoteXml = null;
            }
        }

        void ParseNote( XmlReader reader, float parentWidth, float parentHeight )
        {
            // check for attributes we support
            RectangleF bounds = new RectangleF( );
            Parser.ParseBounds( reader, ref bounds );

            // LEFT/TOP POSITIONING
            if( bounds.X < 1 )
            {
                // convert % to pixel, based on parent's width
                bounds.X = parentWidth * bounds.X;
            }

            if( bounds.Y < 1 )
            {
                // convert % to pixel, based on parent's width
                bounds.Y = parentHeight * bounds.Y;
            }

            // check for any styles requested in XML
            Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mMainNote );

            // PADDING
            float leftPadding = Styles.Style.GetStyleValue( mStyle.mPaddingLeft, parentWidth );
            float rightPadding = Styles.Style.GetStyleValue( mStyle.mPaddingRight, parentWidth );
            float topPadding = Styles.Style.GetStyleValue( mStyle.mPaddingTop, parentHeight );
            float bottomPadding = Styles.Style.GetStyleValue( mStyle.mPaddingBottom, parentHeight );

            // now calculate the available width based on padding. (Don't actually change our width)
            float availableWidth = parentWidth - leftPadding - rightPadding;

            // begin reading the xml stream
            bool finishedReading = false;
            while( finishedReading == false && reader.Read( ) )
            {
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        IUIControl control = Parser.TryParseControl( new BaseControl.CreateParams( availableWidth, parentHeight, ref mStyle ), reader );
                        ChildControls.Add( control );
                        break;
                    }

                    case XmlNodeType.EndElement:
                    {
                        if( reader.Name == "Note" )
                        {
                            finishedReading = true;
                        }
                        break;
                    }
                }
            }

            // lay stuff out vertically like a stackPanel.
            // layout all controls
            float yOffset = bounds.Y + topPadding; //vertically they should just stack

            // now we must center each control within the stack.
            foreach( IUIControl control in ChildControls )
            {
                RectangleF controlFrame = control.GetFrame( );

                // horizontally position the controls according to their 
                // requested alignment
                Alignment controlAlignment = control.GetHorzAlignment( );

                // adjust by our position
                float xAdjust = 0;
                switch( controlAlignment )
                {
                    case Alignment.Center:
                    {
                        xAdjust = bounds.X + ( ( availableWidth / 2 ) - ( controlFrame.Width / 2 ) );
                        break;
                    }
                    case Alignment.Right:
                    {
                        xAdjust = bounds.X + ( availableWidth - controlFrame.Width );
                        break;
                    }
                    case Alignment.Left:
                    {
                        xAdjust = bounds.X;
                        break;
                    }
                }

                // adjust the next sibling by yOffset
                control.AddOffset( xAdjust + leftPadding, yOffset );

                // and the next sibling must begin there
                yOffset = control.GetFrame( ).Bottom;
            }

            bounds.Height = ( yOffset - bounds.Y ) + bottomPadding;
            Bounds = bounds;
        }

        public void AddToView( object view )
        {
            foreach( IUIControl uiControl in ChildControls )
            {
                uiControl.AddToView( view );
            }
        }

        public RectangleF GetFrame( )
        {
            return Bounds;
        }

        public void TouchesEnded( PointF touch )
        {
            // notify all controls
            foreach( IUIControl control in ChildControls )
            {
                control.TouchesEnded( touch );
            }
        }
    }
}
