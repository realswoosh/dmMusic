using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace dmMusic
{
	public class TabItemFactory
	{
		public virtual TabItem CreateTabItem(bool isSelectedField = false)
		{
			return new TabItem() { IsSelected = isSelectedField };
		}
	}


	public class dmTabItemFactory : TabItemFactory
	{
		public override TabItem CreateTabItem(bool isSelectedField = false)
		{
			return new dmTabItem() { IsSelected = isSelectedField };
		}
	}

}
