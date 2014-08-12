using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;
using Notes.Styles;

namespace Notes
{
    /// <summary>
    /// A container that displays children in a vertical stack.
    /// </summary>
    public class List : BaseControl
    {
        protected const string ListTypeBullet = "Bullet";
        protected const string ListTypeNumbered = "Numbered";

        /// <summary>
        /// Children to display
        /// </summary>
        /// <value>The child controls.</value>
        protected List<IUIControl> ChildControls { get; set; }

        /// <summary>
        /// The bounds (including position) of the stack panel.
        /// </summary>
        /// <value>The bounds.</value>
        protected RectangleF Bounds { get; set; }

        protected override void Initialize( )
        {
            base.Initialize( );

            ChildControls = new List<IUIControl>( );
        }

        public List( CreateParams parentParams, XmlReader reader )
        {
            Initialize( );

            // check for attributes we support
            RectangleF bounds = new RectangleF( );
            ParseCommonAttribs( reader, ref bounds );

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mList );

            // parse for the desired list style. Default to Bullet if they didn't put anything.
            string listTypeStr = reader.GetAttribute( "Type" );
            if( string.IsNullOrEmpty( listTypeStr ) == true)
            {
                listTypeStr = ListTypeBullet;
            }

            // convert indentation if it's a percentage
            float listIndentation = mStyle.mListIndention.Value;
            if( listIndentation < 1 )
            {
                listIndentation = parentParams.Width * listIndentation;
            }


            // LEFT/TOP POSITIONING
            if( bounds.X < 1 )
            {
                // convert % to pixel, based on parent's width
                bounds.X = parentParams.Width * bounds.X;
            }

            if( bounds.Y < 1 )
            {
                // convert % to pixel, based on parent's width
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
            // also consider the indention amount of the list.
            float availableWidth = bounds.Width - leftPadding - rightPadding - listIndentation;


            // Parse Child Controls
            int numberedCount = 1;

            // don't force our alignment, bullet style or indentation on children.
            Style style = new Style( );
            style = mStyle;
            style.mAlignment = null;
            style.mListIndention = null;
            style.mListBullet = null;

            bool finishedParsing = false;
            while( finishedParsing == false && reader.Read( ) )
            {
                switch( reader.NodeType )
                {
                    case XmlNodeType.Element:
                    {
                        // Create the prefix for this list item.
                        string listItemPrefixStr = mStyle.mListBullet + " ";
                        if( listTypeStr == ListTypeNumbered )
                        {
                            listItemPrefixStr = numberedCount.ToString() + ". ";
                        }

                        NoteText textLabel = new NoteText( new CreateParams( availableWidth, parentParams.Height, ref style ), listItemPrefixStr );
                        ChildControls.Add( textLabel );


                        // create our actual child, but throw an exception if it's anything but a ListItem.
                        IUIControl control = Parser.TryParseControl( new CreateParams( availableWidth - textLabel.GetFrame().Width, parentParams.Height, ref style ), reader );

                        ListItem listItem = control as ListItem;
                        if( listItem == null ) throw new Exception( String.Format("Only a <ListItem> may be a child of a <List>. Found element <{0}>.", control.GetType( ) ) );


                        // if it will actually use the bullet point, increment our count.
                        if( listItem.ShouldShowBulletPoint() == true )
                        {
                            numberedCount++;
                        }
                        else
                        {
                            // otherwise give it a blank space, and keep our count the same.
                            textLabel.SetText("  ");
                        }

                        // and finally add the actual list item.
                        ChildControls.Add( control );
                        break;
                    }

                    case XmlNodeType.EndElement:
                    {
                        // if we hit the end of our label, we're done.
                        if( reader.Name == "List" )
                        {
                            finishedParsing = true;
                        }

                        break;
                    }
                }
            }


            // layout all controls
            float xAdjust = bounds.X + listIndentation; 
            float yOffset = bounds.Y + topPadding; //vertically they should just stack

            // we know each child is a NoteText followed by ListItem. So, lay them out 
            // as: * - ListItem
            //     * - ListItem
            foreach( IUIControl control in ChildControls )
            {
                RectangleF controlFrame = control.GetFrame( );

                // position the control
                control.AddOffset( xAdjust + leftPadding, yOffset );

                // is this the item prefix?
                if( (control as NoteText) != null )
                {
                    // and update xAdjust so the actual item starts after.
                    xAdjust += controlFrame.Width;
                }
                else
                {
                    // reset the values for the next line.
                    xAdjust = bounds.X + listIndentation;
                    yOffset = control.GetFrame( ).Bottom;
                }
            }

            // we need to store our bounds. We cannot
            // calculate them on the fly because we
            // would lose any control defined offsets, which would throw everything off.
            bounds.Height = ( yOffset - bounds.Y ) + bottomPadding;
            Bounds = bounds;

            // store our debug frame
            base.DebugFrameView.Frame = Bounds;
        }

        public override bool TouchesEnded( PointF touch )
        {
            // let each child handle it
            foreach( IUIControl control in ChildControls )
            {
                // if a child consumes it, stop and report it was consumed.
                if(control.TouchesEnded( touch ))
                {
                    return true;
                }
            }

            return false;
        }

        public override void AddOffset( float xOffset, float yOffset )
        {
            base.AddOffset( xOffset, yOffset );

            // position each interactive label relative to ourselves
            foreach( IUIControl control in ChildControls )
            {
                control.AddOffset( xOffset, yOffset );
            }

            // update our bounds by the new offsets.
            Bounds = new RectangleF( Bounds.X + xOffset, Bounds.Y + yOffset, Bounds.Width, Bounds.Height );
            base.DebugFrameView.Frame = Bounds;
        }

        public override void AddToView( object obj )
        {
            // let each child do the same thing
            foreach( IUIControl control in ChildControls )
            {
                control.AddToView( obj );
            }

            TryAddDebugLayer( obj );
        }

        public override void RemoveFromView( object obj )
        {
            // let each child do the same thing
            foreach( IUIControl control in ChildControls )
            {
                control.RemoveFromView( obj );
            }

            TryRemoveDebugLayer( obj );
        }

        public override RectangleF GetFrame( )
        {
            return Bounds;
        }

        public override bool ShouldShowBulletPoint( )
        {
            // as a container, it wouldn't really make sense to show a bullet point.
            return false;
        }
    }
}
