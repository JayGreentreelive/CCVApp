using System;
using Android.App;
using Android.Widget;

namespace Droid
{
    namespace Tasks
    {
        public class Task
        {
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

