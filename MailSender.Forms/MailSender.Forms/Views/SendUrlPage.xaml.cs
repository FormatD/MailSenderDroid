using CodeMill.VMFirstNav;
using MailSender.Forms.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MailSender.Forms.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SendUrlPage : ContentPage, IViewFor<SendUrlViewModel>
    {
		public SendUrlPage ()
		{
			InitializeComponent ();

            BindingContext = new SendUrlViewModel();
        }

        SendUrlViewModel vm;
        public SendUrlViewModel ViewModel
        {
            get => vm;
            set
            {
                vm = value;
                BindingContext = vm;
            }
        }
    }
}