using CSCore.SoundOut;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace dmMusic
{
	public class ExtendedTabControl : TabControl
	{
		private readonly static TabItemFactory tabItemFactory = new dmTabItemFactory();
		private readonly static TabItemCleaner tabItemCleaner = new dmTabItemCleaner();

		public MouseButtonEventHandler MusicListMouseDoubleClick = null;
	
		private List<Key> pressedKey = new List<Key>();
		
		public ExtendedTabControl()
		{
			AllowDrop = true;

			Loaded += ExtendedTabControl_Loaded;
			SelectionChanged += ExtendedTabControl_SelectionChanged;
			Drop += ExtendedTabContro_DragDrop;
		}

		public dmTabItem AddTabItem()
		{
			var tabItem = tabItemFactory.CreateTabItem(true);
			var tabContent = new dmItemContent();
			tabContent.FolderOpenComplete += ExtendedTabControl_FolderOpenComplete;
			tabContent.musicList.MouseDoubleClick += MusicListMouseDoubleClick;
			tabItem.Content = tabContent;
			Items.Insert(Items.Count, tabItem);

			return (dmTabItem)tabItem;
		}

		public void AddTabItem(AppSetting.TabItem savedTabItem)
		{

			var tabItem = AddTabItem();
			tabItem.HeaderText = savedTabItem.Name;

			foreach (var mItem in savedTabItem.tabItem)
			{
				((dmItemContent)tabItem.Content).musicList.Items.Add(mItem);
			}
		}
		public void CloseTabItem()
		{
			var item = SelectedItem as dmTabItem;
			if (item == null)
				return;

			CloseTabItem(item);
		}

		public void CloseTabItem(TabItem tab)
		{
			SelectedIndex = Items.IndexOf(tab) - 1;
			tabItemCleaner.ClearTabItem(tab);
			Items.Remove(tab);
		}

		void ExtendedTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
		}

		void ExtendedTabControl_Loaded(object senbder, RoutedEventArgs e)
		{
			//AddChild(tabItemFactory.CreateTabItem(true));
			//AddChild(addsTab);

			//Items.Insert(Items.Count, tabItemFactory.CreateTabItem(true));
			//AddTabItem();
		}

		void ExtendedTabControl_FolderOpenComplete(object sender, EventArgsFolderOpenComplete args)
		{
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				var selectedItem = (dmTabItem)SelectedItem;
				selectedItem.HeaderText = args.FolderName;
			}));
		}
		
		void ExtendedTabContro_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Source.Equals(this) == false)
				return;

			/*
			if (Items.Count != 0)
				return;
			*/

			var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

			var tabItem = AddTabItem();

			((dmItemContent)tabItem.Content).ListupFile(path);
		}
	}
}
