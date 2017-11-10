using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.Tags.ID3;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace dmMusic
{
	[MessagePackObject]
	public class MusicItem
	{
		[IgnoreMember]
		public string Playing { get; set; } = "";
		[Key(0)]
		public string Title { get; set; }
		[Key(1)]
		public string Duration { get; set; }
		[Key(2)]
		public string FilePath { get; set; }
	}

	public class EventArgsFolderOpenComplete : EventArgs
	{
		public string FullPath { get; set; }
		public string FolderName { get; set; }
	}

	/// <summary>
	/// dmItemContent.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class dmItemContent : UserControl
	{
		public EventHandler<EventArgsFolderOpenComplete> FolderOpenComplete { get; set; } = null;
		
		public dmItemContent()
		{
			InitializeComponent();
		}

		public Stream GenerateStreamFromString(string s)
		{
			if (string.IsNullOrEmpty(s))
				return null;

			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		MusicItem CreateMusicItem(TagLib.File f)
		{
			MusicItem musicItem = new MusicItem();

			if (f.Tag.Title == null)
			{
				// ID3 태그가 읽히지 않은것이다.
				// 걍 파일명을 등록한다.
				musicItem.Title = System.IO.Path.GetFileNameWithoutExtension(f.Name);
				musicItem.Duration = "";
			}
			else
			{
				Stream streamT = GenerateStreamFromString(f.Tag.Title);
				Stream streamA = GenerateStreamFromString(f.Tag.FirstAlbumArtist);

				byte[] tmpBuff_T = new byte[4];
				byte[] tmpBuff_A = new byte[4];

				if (streamT != null)
					streamT.Read(tmpBuff_T, 0, 4);

				if (streamA != null)
					streamA.Read(tmpBuff_A, 0, 4);

				byte[] tmpTargets = { 194, 195, 77, 78, 68, 49, 74, 70 };

				string title = f.Tag.Title;
				string artist = f.Tag.FirstAlbumArtist;

				Func<string, string> act = (string tmp) =>
				{
					if (string.IsNullOrEmpty(tmp))
						tmp = "";

					Encoding enc = Encoding.GetEncoding("iso-8859-1");
					byte[] sorceBytes = enc.GetBytes(tmp);
					byte[] encBytes = Encoding.Convert(Encoding.GetEncoding("euc-kr"), Encoding.UTF8, sorceBytes);
					var tmpStr = Encoding.UTF8.GetString(encBytes);
					return tmpStr;
				};

				if (streamT != null && Array.Exists(tmpTargets, x => x == tmpBuff_T[0]))
					title = act(f.Tag.Title);

				if (streamA != null && Array.Exists(tmpTargets, x => x == tmpBuff_A[0]))
					artist = act(f.Tag.FirstAlbumArtist);

				TimeSpan time = TimeSpan.FromSeconds(f.Properties.Duration.TotalSeconds);

				string strDuration = time.ToString(@"mm\:ss");
				string duration = string.Format("{0}", f.Properties.Duration.TotalSeconds);

				musicItem.Title = title + "/" + artist;
				musicItem.Duration = strDuration;
			}

			musicItem.FilePath = f.Name;

			return musicItem;
		}

		public void ListupFile(string path)
		{
			bool isDropFolder = false;

			Task dropTask = new Task(() =>
			{
				FileAttributes attr = File.GetAttributes(path);

				if (attr.HasFlag(FileAttributes.Directory))
					isDropFolder = true;

				if (isDropFolder)
				{
					string[] files = Directory.GetFiles(path, "*.mp3");
					foreach (var file in files)
					{
						Application.Current.Dispatcher.BeginInvoke(new Action(() =>
						{
							musicList.Items.Add(CreateMusicItem(TagLib.File.Create(file)));
						}), DispatcherPriority.Background);
					}
				}
				else
				{
					Application.Current.Dispatcher.BeginInvoke(new Action(() =>
					{
						musicList.Items.Add(CreateMusicItem(TagLib.File.Create(path)));
					}), DispatcherPriority.Background);
				}
			});

			var taskComplete = dropTask.ContinueWith(t =>
			{
				if (isDropFolder && FolderOpenComplete != null)
				{
					string fullPath = System.IO.Path.GetFullPath(path).TrimEnd(System.IO.Path.DirectorySeparatorChar);
					string lastFolderName = fullPath.Split(System.IO.Path.DirectorySeparatorChar).Last();

					FolderOpenComplete(this, new EventArgsFolderOpenComplete()
					{
						FolderName = lastFolderName
					});
				}
			});

			dropTask.Start();
		}
		private void musicList_Drop(object sender, DragEventArgs e)
		{
			var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

			ListupFile(path);
		}
	}
}
