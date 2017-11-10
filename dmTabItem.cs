using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace dmMusic
{
	public class dmTabItem : TabItem
	{
		private const String TAG_ADD_TAB_TEXT = "+";
		private const String TAG_NEW_TAB_TEXT = "NewTab";
		private const double DEFAULT_MAX_WIDTH = 150.0d;
		private const double DEFAULT_MAX_WIDTH_ADDS_TAB = 25.0d;

		private static int count = 0;

		public string HeaderText
		{
			get { return ((dmTabItemHeader)Header).txtHeader.Text; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				else
				{
					((dmTabItemHeader)Header).txtHeader.Text = value;
				}
			}
		}

		public dmTabItem()
		{
			Header = new dmTabItemHeader();

			((dmTabItemHeader)Header).txtHeader.Text = string.Format("{0}{1}", TAG_NEW_TAB_TEXT, count++);

			MaxWidth = DEFAULT_MAX_WIDTH;

			// 우선 드래그앤 드랍은 나중에 구현
			// 
			AllowDrop = false;

			var menu = new ContextMenu();

			var menuItem_Rename = new MenuItem() { Header = "Rename" };
			
			menuItem_Rename.Click += (s, args) => 
			{
				ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)args.OriginalSource);
				UIElement placementTarget = cm.PlacementTarget;
				dmTabItem header = placementTarget as dmTabItem;

				Point position = placementTarget.PointToScreen(new Point(0d, 0d)),
				controlPosition = this.PointToScreen(new Point(0d, 0d));
				
				RenameWindow renameWindow = new RenameWindow();
				renameWindow.InitializeTabName = header.HeaderText;
				renameWindow.Owner = Application.Current.MainWindow;
				renameWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

				if (renameWindow.ShowDialog() == true)
				{
					string rename = renameWindow.txtRename.Text;
					Debug.WriteLine(rename);
					header.HeaderText = rename;
				}
			};

			menu.Items.Add(menuItem_Rename);
			
			ContextMenu = menu;
		}


		public dmTabItem TransformToAddsTab()
		{
			this.Header = new TextBlock() { Text = TAG_ADD_TAB_TEXT };
			this.Width = DEFAULT_MAX_WIDTH_ADDS_TAB;
			this.MaxWidth = DEFAULT_MAX_WIDTH_ADDS_TAB;
			this.Content = null;

			return this;
		}
	}
}
