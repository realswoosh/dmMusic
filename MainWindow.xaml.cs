using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MusicPlayer musicPlayer = new MusicPlayer();

		TabItem currentTabItem = null;

		MusicItem currentMusicItem = null;

		int selectIndex = -1;

		bool isTrackBarThumbDrag = false;
		bool isTh = false;

		Thread th;

		Brush btnDefaultBrush = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));

		List<AppSetting.TabItem> savedTabItemList = null;

		public MainWindow()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{

				String resourceName = "AssemblyLoadingAndReflection." +			   new AssemblyName(args.Name).Name + ".dll";

				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
				{

					Byte[] assemblyData = new Byte[stream.Length];

					stream.Read(assemblyData, 0, assemblyData.Length);

					return Assembly.Load(assemblyData);

				}

			};

			InitializeComponent();

			musicPlayer.PlaybackStopped += MusicPlayer_PlayerStopped;

			//musicPlayer.Open(@"E:\Music\가요\걸그룹노래모음\01 - Dalshabet Girls.mp3", null);
			//musicPlayer.Play();

			extendedTabControl.MusicListMouseDoubleClick += MusicList_MouseDoubleClick;

			AppSetting appSetting = new AppSetting();
			appSetting.Load();

			savedTabItemList = appSetting.tabItemList;

			if (appSetting.Left == 0 &&
				appSetting.Top == 0 &&
				appSetting.Width == 0 &&
				appSetting.Height == 0)
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
			}
			else
			{
				Left = appSetting.Left;
				Top = appSetting.Top;
				Width = appSetting.Width;
				Height = appSetting.Height;
			}

			musicPlayer.Volume = appSetting.volumn;

			if (musicPlayer.Volume == 0)
				musicPlayer.Volume = 10;

			isTh = true;

			th = new Thread(new ThreadStart(RunTrackBar));
			th.Start();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (savedTabItemList != null)
			{
				foreach (var tabItem in savedTabItemList)
				{
					extendedTabControl.AddTabItem(tabItem);
				}

				savedTabItemList.Clear();
				savedTabItemList = null;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			isTh = false;

			musicPlayer.MouseDoubleClickStopReady();	// Stop Event를 삭제 한다. 함수명 정리가 필요함
			musicPlayer.CleanupPlayback();

			AppSetting appSetting = new AppSetting();

			appSetting.volumn = 0;

			foreach (var item in extendedTabControl.Items)
			{
				AppSetting.TabItem appSettingTabItem = new AppSetting.TabItem();

				dmTabItem tabItem = (dmTabItem)item;

				appSettingTabItem.Name = tabItem.HeaderText;

				dmItemContent content = (dmItemContent)tabItem.Content;

				foreach (var musicItem in content.musicList.Items)
				{
					var mItem = musicItem as MusicItem;
					mItem.Playing = "";
					appSettingTabItem.tabItem.Add(mItem);
				}

				appSetting.tabItemList.Add(appSettingTabItem);
			}

			appSetting.Left = this.Left;
			appSetting.Top = this.Top;
			appSetting.Width = this.Width;
			appSetting.Height = this.Height;

			appSetting.volumn = musicPlayer.Volume;

			appSetting.Save();

		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.T)
			{
				extendedTabControl.AddTabItem();
			}
			else if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.W)
			{
				extendedTabControl.CloseTabItem();
			}

			if (e.Key == Key.Space)
			{
				if (musicPlayer.PlaybackState == PlaybackState.Playing)
				{
					musicPlayer.Pause();
					btnPlay.Content = 6;
					btnPlay.Background = btnDefaultBrush;
				}
				else if (musicPlayer.PlaybackState == PlaybackState.Paused)
				{
					musicPlayer.Play();
					btnPlay.Content = 4;
					btnPlay.Background = Brushes.Red;
				}
			}
		}

		MusicItem GetNextMusicItem()
		{
			selectIndex++;

			var tabContent = (dmItemContent)currentTabItem.Content;

			var listView = tabContent.musicList;

			if (listView.Items.Count == selectIndex)
				selectIndex = 0;

			MusicItem mItem = (MusicItem)listView.Items[selectIndex];

			return mItem;
		}

		void MusicPlayer_PlayerStopped(object sender, PlaybackStoppedEventArgs args)
		{
			if (currentTabItem != null)
			{
				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					PlaySound(GetNextMusicItem());
				}), DispatcherPriority.Background);
			}
		}

		private void MusicList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (musicPlayer == null)
				return;

			ListView listView = (ListView)sender;

			var mItem = (MusicItem)listView.SelectedItem;
			if (mItem == null)
				return;

			selectIndex = listView.SelectedIndex;
						
			musicPlayer.MouseDoubleClickStopReady();
			musicPlayer.Stop();

			PlaySound(mItem);
		}

		void PlaySound(MusicItem mItem)
		{
			if (currentMusicItem != null)
				currentMusicItem.Playing = "";

			mItem.Playing = ">";

			musicPlayer.CleanupPlayback();
			musicPlayer.Open(mItem.FilePath, null);
			musicPlayer.Play();

			currentTabItem = (TabItem)extendedTabControl.SelectedItem;
			currentMusicItem = mItem;

			((dmItemContent)currentTabItem.Content).musicList.Items.Refresh();
			
			trackBar.Value = 0;
			trackBar.Maximum = musicPlayer.Length.TotalSeconds;

			btnPlay.Content = 4;
			btnPlay.Background = Brushes.Red;
		}

		void RunTrackBar()
		{
			while (isTh)
			{
				if (musicPlayer.PlaybackState == PlaybackState.Playing)
				{
					if (isTrackBarThumbDrag == false)
					{
						Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
						{
							trackBar.Value = musicPlayer.Position.TotalSeconds;
						}));
					}
				}

				Thread.Sleep(100);
			}

		}

		void TrackBar_ThumbDragStart(object sender, DragStartedEventArgs e)
		{
			isTrackBarThumbDrag = true;
		}

		void TrackBar_ThumbDragCompleted(object sender, DragCompletedEventArgs e)
		{
			if (musicPlayer != null)
				musicPlayer.Position = new TimeSpan(0, 0, (int)trackBar.Value);

			isTrackBarThumbDrag = false;
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (musicPlayer != null &&
				musicPlayer.PlaybackState != PlaybackState.Stopped)
			{
				trackBar.Value = 0;
				btnPlay.Background = btnDefaultBrush;
				musicPlayer.CleanupPlayback();
			}
		}

		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			var content = (dmItemContent)((dmTabItem)extendedTabControl.SelectedItem).Content;

			ListView listView = content.musicList;

			var mItem = (MusicItem)listView.SelectedItem;
			if (mItem == null)
				return;

			selectIndex = listView.SelectedIndex;
			
			musicPlayer.MouseDoubleClickStopReady();
			musicPlayer.Stop();

			PlaySound(mItem);
		}

		private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
				VolumnUP();
			else if (e.Delta < 0)
				VolumnDown();
		}

		void VolumnUP()
		{
			int volumn = musicPlayer.Volume;

			volumn += 3;

			if (volumn > 100)
				volumn = 100;

			musicPlayer.Volume = volumn;
		}

		void VolumnDown()
		{
			int volumn = musicPlayer.Volume;

			volumn -= 3;

			if (volumn < 0)
				volumn = 0;

			musicPlayer.Volume = volumn;
		}
	}
}
