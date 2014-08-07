using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using RockMobile.Network;

namespace Notes
{
    /// <summary>
    /// Parser contains a set of utility methods to assist in parsing NoteScript.
    /// </summary>
    public class Parser
    {
        public static IUIControl TryParseControl( Notes.BaseControl.CreateParams parentParams, XmlReader reader )
        {
            // either create/parse a new control, or return null.
            switch( reader.Name )
            {
                case "Paragraph": return new Paragraph( parentParams, reader );
                case "Canvas": return new Canvas( parentParams, reader );
                case "StackPanel": return new StackPanel( parentParams, reader );
                case "List": return new List( parentParams, reader );
                case "ListItem": return new ListItem( parentParams, reader );
                case "RevealBox": return new RevealBox( parentParams, reader );
                case "Quote": return new Quote( parentParams, reader );
                case "TextInput": return new TextInput( parentParams, reader );
                case "Header": return new Header(parentParams, reader);
            }

            throw new Exception( String.Format( "Control of type {0} does not exist.", reader.Name ) );
        }

        public static void ParseBounds( XmlReader reader, ref RectangleF bounds )
        {
            // first check without the Margin prefix.
            string result = reader.GetAttribute( "Left" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                bounds.X = ParseBoundValue( result );
            }

            result = reader.GetAttribute( "Top" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                bounds.Y = ParseBoundValue( result );
            }

            //TODO: Support Right, Bottom


            // now with the margin prefix. And it's ok if it overwrites the equivalent non-margin version.
            result = reader.GetAttribute( "MarginLeft" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                bounds.X = ParseBoundValue( result );
            }

            result = reader.GetAttribute( "MarginTop" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                bounds.Y = ParseBoundValue( result );
            }

            //TODO: Support MarginRight / MarginBottom


            // Get width/height
            result = reader.GetAttribute( "Width" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                bounds.Width = ParseBoundValue( result );
            }

            result = reader.GetAttribute( "Height" );
            if( string.IsNullOrEmpty( result ) == false )
            {
                bounds.Height = ParseBoundValue( result );
            }
        }

        static float ParseBoundValue( string value )
        {
            float denominator = 1.0f;
            if( value.Contains( "%" ) )
            {
                value = value.Trim( '%' );
                denominator = 100.0f;
            }

            return float.Parse( value ) / denominator;
        }


        // Return a rect that contains both rects A and B (sort of a bounding box)
        public static RectangleF CalcBoundingFrame( RectangleF frameA, RectangleF frameB )
        {
            RectangleF frame = new RectangleF( );

            // get left edge
            float leftEdge = frameA.Left < frameB.Left ? frameA.Left : frameB.Left;

            // get top edge
            float topEdge = frameA.Top < frameB.Top ? frameA.Top : frameB.Top;

            // get right edge
            float rightEdge = frameA.Right > frameB.Right ? frameA.Right : frameB.Right;

            // get bottom edge
            float bottomEdge = frameA.Bottom > frameB.Bottom ? frameA.Bottom : frameB.Bottom;

            frame.X = leftEdge;
            frame.Y = topEdge;
            frame.Width = rightEdge - leftEdge;
            frame.Height = bottomEdge - topEdge;

            return frame;
        }

        // Returns a rect that is rectA expanded by rectB (inflate in all four directions)
        public static RectangleF CalcExpandedFrame( RectangleF frameA, RectangleF frameB )
        {
            RectangleF expandedRect = frameA;

            expandedRect.X += frameB.Left;
            expandedRect.Y += frameB.Top;
            expandedRect.Width += frameB.Width;
            expandedRect.Height += frameB.Height;

            return expandedRect;
        }
    }
}
