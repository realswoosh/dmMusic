using MessagePack;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace dmMusic
{
	[MessagePackObject]
	public class AppSetting
	{
		public static string AppSettingFile = "AppSetting.mpack";

		[MessagePackObject]
		public class TabItem
		{
			[Key(0)]
			public string Name { get; set; }
			[Key(1)]
			public List<MusicItem> tabItem = new List<MusicItem>();
		}
		
		[Key(0)]
		public int volumn;

		[Key(1)]
		public List<TabItem> tabItemList = new List<TabItem>();

		[Key(3)]
		public double Left;
		[Key(4)]
		public double Top;
		[Key(5)]
		public double Width;
		[Key(6)]
		public double Height;

		public void Load()
		{
			try
			{
				byte[] bytes = File.ReadAllBytes(AppSettingFile);
				AppSetting tmpAppSetting = MessagePackSerializer.Deserialize<AppSetting>(bytes);
				volumn = tmpAppSetting.volumn;
				tabItemList = tmpAppSetting.tabItemList;
				Left = tmpAppSetting.Left;
				Top = tmpAppSetting.Top;
				Width = tmpAppSetting.Width;
				Height = tmpAppSetting.Height;
			}
			catch
			{

			}
		}

		public void Save()
		{
			var bytes = MessagePackSerializer.Serialize(this);

			using (var fs = new FileStream(AppSettingFile, FileMode.Create, FileAccess.Write))
			{
				fs.Write(bytes, 0, bytes.Length);
			}
		}
	}
}
