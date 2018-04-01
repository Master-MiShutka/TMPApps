﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace TMP.Work.Emcos.Controls
{
	/// <summary>
	/// A Windows Explorer styled search box
	/// </summary>
	public partial class SearchBox : UserControl
	{
		#region Members

		private bool isActive = false;
		private bool changingActiveStatus = false;
		private DispatcherTimer delay = new DispatcherTimer();

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets whether the box will send events on changed content.
		/// </summary>
		public bool IsConnected { get; set; }

		/// <summary>
		/// Gets or sets whether the search box is enabled or not.
		/// </summary>
		public new bool IsEnabled
		{
			get { return Box.IsEnabled; }
			set
			{
				Box.IsEnabled = value;
				byte a = (byte)(value ? 192 : 255);
				byte v = (byte)(value ? 255 : 244);
				SearchBackground.Background = new SolidColorBrush(Color.FromArgb(a, v, v, v));
			}
		}

		/// <summary>
		/// Gets or sets the current state of the search box
		/// </summary>
		public bool IsActive
		{
			get { return isActive; }
			set
			{
				changingActiveStatus = true;
				if (value)
				{
					Box.Text = "";
					Box.FontStyle = System.Windows.FontStyles.Normal;
					Box.Foreground = System.Windows.Media.Brushes.Black;
					SearchBackground.Background = System.Windows.Media.Brushes.White;
					Button.Style = (Style)FindResource("SearchClearButtonStyle");
				}
				else
				{
					Box.Text = "Text";
					Box.FontStyle = System.Windows.FontStyles.Italic;
					Box.Foreground = new SolidColorBrush(Color.FromRgb(0x79, 0x7a, 0x7a));
					SearchBackground.Background = new SolidColorBrush(Color.FromArgb(0xC0, 0xFF, 0xFF, 0xFF));
					Button.Style = (Style)FindResource("SearchButtonStyle");
				}
				changingActiveStatus = false;
				isActive = value;
				DispatchActiveStateChangedEvent(value, Text);
			}
		}

		/// <summary>
		/// Get or sets the search text.
		/// An empty string if the search box is not active.
		/// </summary>
		public String Text
		{
			get
			{
				if (IsActive)
					return Box.Text;
				else
					return "";
			}
			set { Box.Text = value; }
		}

		/// <summary>
		/// Gets or sets the position of the cursor in the text box.
		/// </summary>
		public int Position
		{
			get { return Box.SelectionStart; }
			set { Box.SelectionStart = value; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Create a search box
		/// </summary>
		public SearchBox()
		{
			//U.L(LogLevel.Debug, "SEARCH BOX", "Initialize");
			InitializeComponent();
			//U.L(LogLevel.Debug, "SEARCH BOX", "Initialized");
			delay.Interval = new TimeSpan(0, 0, 0, 0, 500);
			delay.Tick += new EventHandler(Delay_Tick);
			IsConnected = true;
		}

		#endregion

		#region Methods

		#region Public

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			Box.Text = "";
			IsActive = false;
			Box.Text = "??????????????";
			Box.FontStyle = System.Windows.FontStyles.Italic;
			Box.Foreground = new SolidColorBrush(Color.FromRgb(0x79, 0x7a, 0x7a));
			SearchBackground.Background = new SolidColorBrush(Color.FromArgb(0xC0, 0xFF, 0xFF, 0xFF));
			Box.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			Button.Style = (Style)FindResource("SearchButtonStyle");
			DispatchClearedEvent();
		}

		#endregion

		#region Event handlers

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Clicked(object sender, RoutedEventArgs e)
		{
			Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddToNew_Clicked(object sender, RoutedEventArgs e)
		{
			DispatchAddToNewClickedEvent();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddToPlaylist_Clicked(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem)
			{
				MenuItem item = sender as MenuItem;
				var name = item.Header.ToString();
                DispatchAddToPlaylistClickedEvent(name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveFromPlaylist_Clicked(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem)
			{
				MenuItem item = sender as MenuItem;
				var name = item.Header.ToString();
                DispatchRemoveFromPlaylistClickedEvent(name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Box_GotFocus(object sender, RoutedEventArgs e)
		{
			if (Box.Text == "<><><>")
				IsActive = true;
			else
				Box.SelectAll();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Box_LostFocus(object sender, RoutedEventArgs e)
		{
			if (Box.Text == "")
				IsActive = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Box_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (IsActive && IsConnected && !changingActiveStatus)
			{
				delay.Stop();
				delay.Start();
			}
		}

		/// <summary>
		/// Called after a search has been performed.
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The event arguments</param>
		private void Delay_Tick(object sender, EventArgs e)
		{
			delay.Stop();
			DispatchTextChangedEvent(IsActive, Text);
		}
		
		#endregion

		#endregion

		#region Events

		/// <summary>
		/// Invoked when the active status of the search box changes
		/// For example when it receives focus
		/// </summary>
		public event SearchBoxDelegate ActiveStateChanged;

		/// <summary>
		/// TODO
		/// </summary>
		public event SearchBoxClearedDelegate Cleared;

		/// <summary>
		/// TODO
		/// </summary>
		public event SearchBoxAddToNewDelegate AddToNewClicked;

		/// <summary>
		/// TODO
		/// </summary>
		public event SearchBoxAddDelegate AddClicked;

		/// <summary>
		/// TODO
		/// </summary>
		public event SearchBoxRemoveDelegate RemoveClicked;

		/// <summary>
		/// TODO
		/// </summary>
		public event SearchBoxDelegate TextChanged;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void SubscribeActiveStateChanged(SearchBoxDelegate eventHandler)
		{
			ActiveStateChanged += eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void SubscribeTextChanged(SearchBoxDelegate eventHandler)
		{
			TextChanged += eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void SubscribeClearClicked(SearchBoxClearedDelegate eventHandler)
		{
			Cleared += eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void SubscribeAddToNewClicked(SearchBoxAddToNewDelegate eventHandler)
		{
			AddToNewClicked += eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void SubscribeAddClicked(SearchBoxAddDelegate eventHandler)
		{
			AddClicked += eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void SubscribeRemoveClicked(SearchBoxRemoveDelegate eventHandler)
		{
			RemoveClicked += eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void UnsubsribeActiveStateChanged(SearchBoxDelegate eventHandler)
		{
			ActiveStateChanged -= eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void UnsubscribeTextChanged(SearchBoxDelegate eventHandler)
		{
			TextChanged -= eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void UnsubscribeClearClicked(SearchBoxClearedDelegate eventHandler)
		{
			Cleared -= eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void UnsubscribeAddToNewClicked(SearchBoxAddToNewDelegate eventHandler)
		{
			AddToNewClicked -= eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void UnsubscribeAddClicked(SearchBoxAddDelegate eventHandler)
		{
			AddClicked -= eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventHandler"></param>
		public void UnsubscribeRemoveClicked(SearchBoxRemoveDelegate eventHandler)
		{
			RemoveClicked -= eventHandler;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="active"></param>
		/// <param name="text"></param>
		public void DispatchActiveStateChangedEvent(bool active, string text)
		{
            ActiveStateChanged?.Invoke(new SearchBoxEventArgs(active, text));
        }

		/// <summary>
		/// 
		/// </summary>
		public void DispatchClearedEvent()
		{
            Cleared?.Invoke();
        }

		/// <summary>
		/// 
		/// </summary>
		public void DispatchAddToNewClickedEvent()
		{
            AddToNewClicked?.Invoke(new SearchBoxAddToNewEventArgs());
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public void DispatchAddToPlaylistClickedEvent(string name)
		{
            AddClicked?.Invoke(new SearchBoxAddEventArgs(name));
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public void DispatchRemoveFromPlaylistClickedEvent(string name)
		{
            RemoveClicked?.Invoke(new SearchBoxRemoveEventArgs(name));
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="active"></param>
		/// <param name="text"></param>
		public void DispatchTextChangedEvent(bool active, string text)
		{
            TextChanged?.Invoke(new SearchBoxEventArgs(active, text));
        }

		#endregion
	}

	/// <summary>
	/// Provides data for a search box event
	/// </summary>
	public class SearchBoxEventArgs : EventArgs
	{
		#region Members

		private bool isActive;
		private string text;

		#endregion

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public bool IsActive { get { return isActive; } }

		/// <summary>
		/// 
		/// </summary>
		public string Text { get { return text; } }

		#endregion

		#region Methods

		#region Public

		public SearchBoxEventArgs(bool active, string txt)
		{
			isActive = active;
			text = txt;
		}

		#endregion

		#endregion
	}

	/// <summary>
	/// Provides data for the <see cref="SearchBox.AddClicked"/> event
	/// </summary>
	public class SearchBoxAddEventArgs : EventArgs
	{
		#region Members

		private string playlistName;

		#endregion

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public string PlaylistName { get { return playlistName; } }

		#endregion

		#region Methods

		#region Public

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public SearchBoxAddEventArgs(string name)
		{
			playlistName = name;
		}

		#endregion

		#endregion
	}

	/// <summary>
	/// Provides data for the <see cref="SearchBox.RemoveClicked"/> event
	/// </summary>
	public class SearchBoxRemoveEventArgs : EventArgs
	{
		#region Members

		private string playlistName;

		#endregion

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public string PlaylistName { get { return playlistName; } }

		#endregion

		#region Methods

		#region Public

		public SearchBoxRemoveEventArgs(string name)
		{
			playlistName = name;
		}

		#endregion

		#endregion
	}

	/// <summary>
	/// Provides data for the <see cref="SearchBox.AddToNewClicked"/> event
	/// </summary>
	public class SearchBoxAddToNewEventArgs : EventArgs
	{
		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		public SearchBoxAddToNewEventArgs()
		{ }

		#endregion
	}

	#region Delegates

	/// <summary>
	/// 
	/// </summary>
	/// <param name="e"></param>
	public delegate void SearchBoxDelegate(SearchBoxEventArgs e);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="e"></param>
	public delegate void SearchBoxAddDelegate(SearchBoxAddEventArgs e);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="e"></param>
	public delegate void SearchBoxRemoveDelegate(SearchBoxRemoveEventArgs e);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="e"></param>
	public delegate void SearchBoxAddToNewDelegate(SearchBoxAddToNewEventArgs e);

	/// <summary>
	/// 
	/// </summary>
	public delegate void SearchBoxClearedDelegate();

	#endregion
}
