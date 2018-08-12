using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PerfMonX.Behaviors {
	sealed class ListBoxBehavior : Behavior<ListBox> {
		protected override void OnAttached() {
			AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
		}

		protected override void OnDetaching() {
			AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
		}

		private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			SelectedItems = AssociatedObject.SelectedItems;
		}


		public IList SelectedItems {
			get { return (IList)GetValue(SelectedItemsProperty); }
			set { SetValue(SelectedItemsProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemsProperty =
			DependencyProperty.Register(nameof(SelectedItems), typeof(IList),
				typeof(ListBoxBehavior), new PropertyMetadata(null));


	}
}
