using System;
using System.Xml;
using System.Drawing;
using System.IO;
using RockMobile.Network;

namespace Notes
{
    namespace Styles
    {
        // Objects that define a style
        public enum Alignment
        {
            Inherit,
            Left,
            Right,
            Center
        }

        public struct FontParams
        {
            public string mName;
            public float? mSize;
            public uint? mColor;

            public FontParams( string name, float size, uint color )
            {
                mName = name;
                mSize = size;
                mColor = color;
            }
        }

        //Style class
        public struct Style
        {
            public uint? mBackgroundColor;
            public Alignment? mAlignment;
            public FontParams mFont;

            public float? mPaddingLeft;
            public float? mPaddingTop;
            public float? mPaddingRight;
            public float? mPaddingBottom;

            public void Initialize( )
            {
                mFont = new FontParams( );
            }

            // Utility function for retrieving a value without knowing if it's percent or not.
            public static float GetStyleValue( float? value, float valueForPerc )
            {
                float styleValue = 0;

                if( value.HasValue )
                {
                    float realValue = value.Value;

                    // If it's a percent
                    if( realValue <= 1 )
                    {
                        // convert using valueForPerc as the source
                        styleValue = valueForPerc * realValue;
                    }
                    else
                    {
                        // otherwise take the value
                        styleValue = realValue;
                    }
                }

                return styleValue;
            }

            // Utility function for parsing common style attributes
            public static void ParseStyleAttributes( XmlReader reader, ref Style style )
            {
                // This builds a style with the following conditions
                // Values from XML come first. This means the control specifically asked for this.
                // Values already set come second. This means the parent specifically asked for this.
                // Padding is an exception AND DOES NOT INHERIT
                // Unlike the WithDefaults version, this will allow style values to remain null. Important
                // for container controls that don't want to force styles.
                string result = reader.GetAttribute( "FontName" );
                if( string.IsNullOrEmpty( result ) == false )
                {
                    style.mFont.mName = result;
                }

                result = reader.GetAttribute( "FontSize" );
                if( string.IsNullOrEmpty( result ) == false )
                {
                    style.mFont.mSize = float.Parse( result );
                }

                result = reader.GetAttribute( "FontColor" );
                if( string.IsNullOrEmpty( result ) == false )
                {
                    style.mFont.mColor = Convert.ToUInt32( result, 16 );
                }

                // check for alignment
                result = reader.GetAttribute( "Alignment" );
                if( string.IsNullOrEmpty( result ) == false )
                {
                    switch( result )
                    {
                        case "Left": style.mAlignment = Styles.Alignment.Left; break;
                        case "Right": style.mAlignment = Styles.Alignment.Right; break;
                        case "Center": style.mAlignment = Styles.Alignment.Center; break;

                        default: throw new Exception( "Unknown alignment type specified." );
                    }
                }

                // check for background color
                result = reader.GetAttribute( "BackgroundColor" );
                if( string.IsNullOrEmpty( result ) == false )
                {
                    style.mBackgroundColor = Convert.ToUInt32( result, 16 );
                }

                // check for padding; DOES NOT INHERIT
                result = reader.GetAttribute( "Padding" );
                if( string.IsNullOrEmpty( result ) == false )
                {
                    float denominator = 1.0f;
                    if( result.Contains( "%" ) )
                    {
                        result = result.Trim( '%' );
                        denominator = 100.0f;
                    }

                    // set it for all padding values
                    style.mPaddingLeft = float.Parse( result ) / denominator;
                    style.mPaddingTop = style.mPaddingLeft;
                    style.mPaddingRight = style.mPaddingLeft;
                    style.mPaddingBottom = style.mPaddingLeft;
                }
                else
                {
                    result = reader.GetAttribute( "PaddingLeft" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        float denominator = 1.0f;
                        if( result.Contains( "%" ) )
                        {
                            result = result.Trim( '%' );
                            denominator = 100.0f;
                        }

                        style.mPaddingLeft = float.Parse( result ) / denominator;
                    }
                    else
                    {
                        style.mPaddingLeft = null;
                    }

                    result = reader.GetAttribute( "PaddingTop" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        float denominator = 1.0f;
                        if( result.Contains( "%" ) )
                        {
                            result = result.Trim( '%' );
                            denominator = 100.0f;
                        }

                        style.mPaddingTop = float.Parse( result ) / denominator;
                    }
                    else
                    {
                        style.mPaddingTop = null;
                    }

                    result = reader.GetAttribute( "PaddingRight" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        float denominator = 1.0f;
                        if( result.Contains( "%" ) )
                        {
                            result = result.Trim( '%' );
                            denominator = 100.0f;
                        }

                        style.mPaddingRight = float.Parse( result ) / denominator;
                    }
                    else
                    {
                        style.mPaddingRight = null;
                    }

                    result = reader.GetAttribute( "PaddingBottom" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        float denominator = 1.0f;
                        if( result.Contains( "%" ) )
                        {
                            result = result.Trim( '%' );
                            denominator = 100.0f;
                        }

                        style.mPaddingBottom = float.Parse( result ) / denominator;
                    }
                    else
                    {
                        style.mPaddingBottom = null;
                    }
                }
            }

            public static void ParseStyleAttributesWithDefaults( XmlReader reader, ref Style style, ref Style defaultStyle )
            {
                // This builds a style with the following conditions
                // Values from XML come first. This means the control specifically asked for this.
                // Values already set come second. This means the parent specifically asked for this.
                // Values from defaultStyle come last. This means no one set the style, so it should use the control default.

                // first use the normal parsing, which will result in a style with potential null values.
                ParseStyleAttributes( reader, ref style );

                // Lastly, merge defaultStyle values for anything null in style 
                MergeStyleAttributesWithDefaults( ref style, ref defaultStyle );
            }

            public static void MergeStyleAttributesWithDefaults( ref Style style, ref Style defaultStyle )
            {
                // validate everything, and for anything null, use the default provided.
                if( style.mFont.mName == null )
                {
                    style.mFont.mName = defaultStyle.mFont.mName;
                }

                if( style.mFont.mSize.HasValue == false )
                {
                    style.mFont.mSize = defaultStyle.mFont.mSize;
                }

                if( style.mFont.mColor.HasValue == false )
                {
                    style.mFont.mColor = defaultStyle.mFont.mColor;
                }

                // check for alignment
                if( style.mAlignment.HasValue == false )
                {
                    style.mAlignment = defaultStyle.mAlignment;
                }

                // check for padding
                if( style.mPaddingLeft.HasValue == false )
                {
                    style.mPaddingLeft = defaultStyle.mPaddingLeft;
                }

                if( style.mPaddingTop.HasValue == false )
                {
                    style.mPaddingTop = defaultStyle.mPaddingTop;
                }

                if( style.mPaddingRight.HasValue == false )
                {
                    style.mPaddingRight = defaultStyle.mPaddingRight;
                }

                if( style.mPaddingBottom.HasValue == false )
                {
                    style.mPaddingBottom = defaultStyle.mPaddingBottom;
                }

                // check for background color
                if( style.mBackgroundColor.HasValue == false )
                {
                    style.mBackgroundColor = defaultStyle.mBackgroundColor;
                }
            }
        }
    }

    //This is a static class containing all the default styles for controls
    public class ControlStyles
    {
        public delegate void StylesCreated( Exception e );

        static StylesCreated mStylesCreatedDelegate;

        static string mStyleSheetUrl;

        public static string StyleSheetXml { get; protected set; }

        // Thse are to be referenced globally as needed
        public static Styles.Style mMainNote;
        public static Styles.Style mParagraph;
        public static Styles.Style mRevealBox;
        public static Styles.Style mTextInput;
        public static Styles.Style mHeader;
        public static Styles.Style mQuote;
        public static Styles.Style mText;

        static void InitStyle( ref Notes.Styles.Style style )
        {
            // just a default for the default styles (in case somehow it's missing in XML)
            style.mAlignment = Styles.Alignment.Left;
            style.mFont = new Notes.Styles.FontParams( );
            style.mPaddingLeft = 0;
            style.mPaddingTop = 0;
            style.mPaddingRight = 0;
            style.mPaddingBottom = 0;
        }

        public static void Initialize( string styleSheetUrl, string styleSheetXml, StylesCreated stylesCreatedDelegate )
        {
            // Create each control's default style. And put some defaults...for the defaults.
            //(Seriously, that way if it doesn't exist in XML we still have a value.)
            mMainNote = new Styles.Style( );
            InitStyle( ref mMainNote );

            mParagraph = new Styles.Style( );
            InitStyle( ref mParagraph );

            mRevealBox = new Styles.Style( );
            InitStyle( ref mRevealBox );

            mTextInput = new Styles.Style( );
            InitStyle( ref mTextInput );

            mHeader = new Styles.Style( );
            InitStyle( ref mHeader );

            mQuote = new Styles.Style( );
            InitStyle( ref mQuote );

            mText = new Styles.Style( );
            InitStyle( ref mText );

            mStylesCreatedDelegate = stylesCreatedDelegate;


            // store the styles URL so we can download it
            mStyleSheetUrl = styleSheetUrl;

            // if an existing XML stream wasn't provided, download via the URL
            if( styleSheetXml == null )
            {
                HttpWebRequest.Instance.MakeAsyncRequest( mStyleSheetUrl, OnCompletion );
            }
            else
            {
                // otherwise we can directly parse the XML
                ParseStyles( styleSheetXml );
            }
        }

        static void OnCompletion( Exception ex, System.Collections.Generic.Dictionary<string, string> responseHeaders, string body )
        {
            if( ex == null )
            {
                ParseStyles( body );
            }
            else
            {
                mStylesCreatedDelegate( ex );
            }
        }

        protected static void ParseStyles( string styleSheetXml )
        {
            Exception exception = null;
            try
            {
                StyleSheetXml = styleSheetXml;

                // now use a reader to get each element
                XmlReader reader = XmlReader.Create( new StringReader( styleSheetXml ) );

                bool finishedParsing = false;

                // look at each element, as they all define styles for our controls
                while( finishedParsing == false && reader.Read( ) )
                {
                    switch( reader.NodeType )
                    {
                    //Find the control elements
                        case XmlNodeType.Element:
                        {
                            //most controls don't care about anything other than the basic attributes,
                            // so we can use a common Parse function. Certain styles may need to define more specific things,
                            // for which we can add special parse classes
                            switch( reader.Name )
                            {
                                case "Note": Notes.Styles.Style.ParseStyleAttributes( reader, ref mMainNote ); break;
                                case "Paragraph": Notes.Styles.Style.ParseStyleAttributes( reader, ref mParagraph ); break;
                                case "RevealBox": Notes.Styles.Style.ParseStyleAttributes( reader, ref mRevealBox ); break;
                                case "TextInput": Notes.Styles.Style.ParseStyleAttributes( reader, ref mTextInput ); break;
                                case "Quote": Notes.Styles.Style.ParseStyleAttributes( reader, ref mQuote ); break;
                                case "Header": Notes.Styles.Style.ParseStyleAttributes( reader, ref mHeader ); break;
                                case "Text": Notes.Styles.Style.ParseStyleAttributes( reader, ref mText ); break;
                            }
                            break;
                        }

                        case XmlNodeType.EndElement:
                        {
                            if( reader.Name == "Styles" )
                            {
                                finishedParsing = true;
                            }
                            break;
                        }
                    }
                }
            } 
            catch( Exception ex )
            {
                exception = ex;
            }

            mStylesCreatedDelegate( exception );
        }
    }
}
    