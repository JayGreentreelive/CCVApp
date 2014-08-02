using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;
using Notes.Styles;
using System.Text.RegularExpressions;

namespace Notes
{
    /// <summary>
    /// A container that displays children in a vertical stack almost exactly like a stack panel.
    /// </summary>
    public class ListItem : StackPanel
    {
        public ListItem( CreateParams parentParams, XmlReader reader )
        {
            Initialize( );

            // check for attributes we support
            RectangleF bounds = new RectangleF( );
            ParseCommonAttribs( reader, ref bounds );

            //ignore positioning attributes.
            bounds = new RectangleF( );
            bounds.Width = parentParams.Width;

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mListItem );
            mStyle.mAlignment = null; //don't use alignment

            // PADDING
            float leftPadding = Styles.Style.GetStyleValue( mStyle.mPaddingLeft, parentParams.Width );
            float rightPadding = Styles.Style.GetStyleValue( mStyle.mPaddingRight, parentParams.Width );
            float topPadding = Styles.Style.GetStyleValue( mStyle.mPaddingTop, parentParams.Height );
            float bottomPadding = Styles.Style.GetStyleValue( mStyle.mPaddingBottom, parentParams.Height );

            // now calculate the available width based on padding. (Don't actually change our width)
            float availableWidth = bounds.Width - leftPadding - rightPadding;

            // Parse Child Controls
            bool finishedParsing = false;
            while( finishedParsing == false && reader.Read( ) )
            {
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        // let each child have our available width.
                        IUIControl control = Parser.TryParseControl( new CreateParams( availableWidth, parentParams.Height, ref mStyle ), reader );
                        if( control != null )
                        {
                            ChildControls.Add( control );
                        }
                        break;
                    }

                    case XmlNodeType.Text:
                    {
                        // grab the text. remove any weird characters
                        string text = Regex.Replace( reader.Value, @"\t|\n|\r", "" );

                        // now break it into words so we can do word wrapping
                        string[] words = text.Split( ' ' );

                        // the very very first word gets a bullet point!
                        string sentence = "";
                        foreach( string word in words )
                        {
                            // create labels out of each one
                            if( string.IsNullOrEmpty( word ) == false )
                            {
                                sentence += word + " ";
                            }
                        }

                        NoteText textLabel = new NoteText( new CreateParams( availableWidth, parentParams.Height, ref mStyle ), sentence );
                        ChildControls.Add( textLabel );
                        break;
                    }

                    case XmlNodeType.EndElement:
                    {
                        // if we hit the end of our label, we're done.
                        if( reader.Name == "ListItem" )
                        {
                            finishedParsing = true;
                        }

                        break;
                    }
                }
            }

            LayoutStackPanel( bounds, leftPadding, topPadding, availableWidth, bottomPadding );
        }

        public override bool ShouldShowBulletPoint()
        {
            // let our first control (which will be displayed first) decide
            return ChildControls[0].ShouldShowBulletPoint( );
        }
    }
}
