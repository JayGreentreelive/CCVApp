
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
using CCVApp.Shared.Network;
using Android.Graphics;
using RestSharp;
using Rock.Mobile.Network;
using CCVApp.Shared.Notes.Model;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesDetailsArrayAdapter : BaseAdapter
            {
                List<Series.Message> Messages { get; set; }

                NotesDetailsFragment ParentFragment { get; set; }

                public NotesDetailsArrayAdapter( NotesDetailsFragment parentFragment, List<Series.Message> messages )
                {
                    ParentFragment = parentFragment;

                    Messages = messages;
                }

                public override int Count 
                {
                    get { return Messages.Count; }
                }

                public override Java.Lang.Object GetItem (int position) 
                {
                    // could wrap a Contact in a Java.Lang.Object
                    // to return it here if needed
                    return null;
                }

                public override long GetItemId (int position) 
                {
                    return 0;
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    //ScaledImageView view = (ScaledImageView) convertView ?? new ScaledImageView( ParentFragment.Activity.BaseContext );
                    TextView view = (TextView) convertView ?? new TextView( ParentFragment.Activity.BaseContext );
                    view.LayoutParameters = new AbsListView.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

                    //view.SetImageBitmap( NewsImage[ position ] );
                    //view.SetScaleType( ImageView.ScaleType.CenterCrop );
                    view.Text = Messages[ position ].Name;

                    return view;
                }
            }

            public class NotesDetailsFragment : TaskFragment
            {
                public List<Series.Message> Messages { get; set; }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.Notes_Details, container, false);
                    view.SetOnTouchListener( this );

                    ListView listView = view.FindViewById<ListView>( Resource.Id.notes_details_list );
                    listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => 
                        {
                            ParentTask.OnClick( this, e.Position );
                        };
                    listView.SetOnTouchListener( this );
                    listView.Adapter = new NotesDetailsArrayAdapter( this, Messages );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( true );

                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.DisplayShareButton( false, null );
                }
            }
        }
    }
}

