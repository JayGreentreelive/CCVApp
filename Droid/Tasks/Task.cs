using System;
using Android.App;
using Android.Widget;

namespace Droid
{
    namespace Tasks
    {
        /// <summary>
        /// A task represents a "section" of the app, like the news, group finder,
        /// notes, etc. It contains all of the pages that make up that particular section.
        /// </summary>
        public class Task
        {
            /// <summary>
            /// Reference to the parent navbar fragment
            /// </summary>
            /// <value>The navbar fragment.</value>
            protected NavbarFragment NavbarFragment { get; set; }

            public Task( NavbarFragment navFragment )
            {
                NavbarFragment = navFragment;
            }

            public virtual void OnClick( Fragment source, int buttonId )
            {
            }

            public virtual Fragment StartingFragment()
            {
                return null;
            }
        }
    }
}

