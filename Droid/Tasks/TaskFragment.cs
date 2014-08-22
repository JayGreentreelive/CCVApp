
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Droid
{
    namespace Tasks
    {
        public class TaskFragment : Fragment
        {
            protected Task ParentTask { get; set; }

            public TaskFragment( Task parentTask ) : base( )
            {
                ParentTask = parentTask;
            }
        }
    }
}

