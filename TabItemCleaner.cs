using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace dmMusic
{
	public class TabItemCleaner
	{
		public virtual void ClearTabItem(TabItem tabItem)
		{
			RecursiveClearPanelsChildren(tabItem.Header as Panel);

			RecursiveClearPanelsChildren(tabItem.Content as Panel);
		}

		protected virtual void RecursiveClearPanelsChildren(Panel p)
		{
			if (p == null)
				return;

			foreach (UIElement ch in p.Children)
			{
				if (ch is Panel)
				{
					RecursiveClearPanelsChildren(ch as Panel);
				}

				p.Children.Clear();

				p = null;
			}
		}
	}

	public class dmTabItemCleaner : TabItemCleaner
	{
		public override void ClearTabItem(TabItem tabItem)
		{
			(tabItem.Header as dmTabItemHeader).txtHeader = null;

			base.ClearTabItem(tabItem);
		}
	}
}
