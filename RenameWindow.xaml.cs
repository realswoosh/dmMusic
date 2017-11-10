using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace dmMusic
{
	/// <summary>
	/// RenameWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class RenameWindow : Window
	{
		public string InitializeTabName { get; set; }

		public RenameWindow()
		{
			InitializeComponent();
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			txtRename.Text = InitializeTabName;
			txtRename.SelectAll();
			Keyboard.Focus(txtRename);
		}

		private void txtRename_KeyDown(object sender, KeyEventArgs e)
		{
			if (txtRename.Text.Length == 0)
				return;

			if (e.Key == Key.Return)
			{
				this.DialogResult = true;
			}
		}
	}
}
