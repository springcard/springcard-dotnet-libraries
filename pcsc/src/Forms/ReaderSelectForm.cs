/**
 *
 * \ingroup PCSC 
 *  
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D / SpringCard 
 *
 */
 /*
  *	This software is part of the SPRINGCARD SDK FOR PC/SC
  *
  *   Redistribution and use in source (source code) and binary
  *   (object code) forms, with or without modification, are
  *   permitted provided that the following conditions are met :
  *
  *   1. Redistributed source code or object code shall be used
  *   only in conjunction with products (hardware devices) either
  *   manufactured, distributed or developed by SPRINGCARD,
  *
  *   2. Redistributed source code, either modified or
  *   un-modified, must retain the above copyright notice,
  *   this list of conditions and the disclaimer below,
  *
  *   3. Redistribution of any modified code must be clearly
  *   identified "Code derived from original SPRINGCARD 
  *   copyrighted source code", with a description of the
  *   modification and the name of its author,
  *
  *   4. Redistributed object code must reproduce the above
  *   copyright notice, this list of conditions and the
  *   disclaimer below in the documentation and/or other
  *   materials provided with the distribution,
  *
  *   5. The name of SPRINGCARD may not be used to endorse
  *   or promote products derived from this software or in any
  *   other form without specific prior written permission from
  *   SPRINGCARD.
  *
  *   THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS".
  *   SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD
  *   PARTIES RIGHTS BASED ON THIS SOFTWARE.
  *
  *   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
  *   FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
  *
  *   SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN
  *   THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE
  *   OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.
  *
  *   IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT,
  *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
  *   DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
  *   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
  *   OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
  *   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
  *   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
  *   THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
  *   OF SUCH DAMAGE. 
  *
  **/
using System;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.PCSC;
using SpringCard.LibCs.Windows;
using System.Collections.Generic;
using SpringCard.LibCs;

namespace SpringCard.PCSC.Forms
{
	/**
	 * \brief A simple form to let the user select one PC/SC reader
	 */
	public partial class ReaderSelectForm : Form
	{
		string _selected_reader = null;

		public class Settings
        {
			public string LastReaderName;
			public bool DontOfferToRemember;
			public string PreselectReaderName;
			public string DiscardReaderName;
		}

		private Settings _settings;

		/**
		 * \brief Create the form
		 */
		public ReaderSelectForm() : this(new Settings())
		{

		}

		/**
		 * \brief Create the form
		 */
		public ReaderSelectForm(Settings settings)
		{
			_settings = settings;
			if (_settings == null)
				_settings = new Settings();

			InitializeComponent();
			LoadReaderList();

			cbRemember.Visible = !_settings.DontOfferToRemember;

			if (string.IsNullOrEmpty(_settings.LastReaderName))
				_settings.LastReaderName = AppConfig.ReadString("reader_name");
			
			if (!string.IsNullOrEmpty(_settings.LastReaderName) && (lvReaders.Items.Count > 0))
			{
				for (int i=0; i<lvReaders.Items.Count; i++)
				{
					if (lvReaders.Items[i].Text.Equals(_settings.LastReaderName))
					{
						lvReaders.Items[i].Selected = true;
						
						_selected_reader = _settings.LastReaderName;
						cbRemember.Checked = cbRemember.Visible && AppConfig.ReadBoolean("reader_reconnect");
						break;
					}
				}
			}
		}

		/**
		 * \brief Create the form, specifying the color of the header (default is SpringCard-red)
		 */
		public ReaderSelectForm(Color headerColor) : this(new Settings())
		{
			pTop.BackColor = headerColor;
		}

		/**
		 * \brief Create the form, specifying the color of the header (default is SpringCard-red)
		 */
		public ReaderSelectForm(Settings settings, Color headerColor) : this(settings)
		{
			pTop.BackColor = headerColor;
		}

		/**
		 * \brief Get/Set the reader name
		 */
		public string SelectedReader
		{
			get
			{
				return _selected_reader;
			}
			set
			{
				lvReaders.SelectedItems.Clear();
				for (int i=0; i<lvReaders.Items.Count; i++)
					if (lvReaders.Items[i].Text.Equals(value))
						lvReaders.Items[i].Selected = true;
			}
		}

		public void Preselect(string preselectReaderName)
		{
			_settings.PreselectReaderName = preselectReaderName;
		}

		void LoadReaderList()
		{
			string[] mask = null;

			if (_settings.DiscardReaderName != null)
			{
				SCARD.SCardLogger.trace("Building reader list (discard={0})", _settings.DiscardReaderName);
				mask = _settings.DiscardReaderName.Split('|');
			}
			else
            {
				SCARD.SCardLogger.trace("Building reader list");
			}

			string[] readers = SCARD.Readers;
			
			lvReaders.BeginUpdate();
			lvReaders.Items.Clear();
			if (readers != null)
			{
				for (int i = 0; i < readers.Length; i++)
				{
					bool discard = false;

					if (mask != null)
						for (int j = 0; j < mask.Length; j++)
							if (readers[i].Contains(mask[j]))
								discard = true;

					if (!discard)
						lvReaders.Items.Add(readers[i]);
				}
			}

			lvReaders.EndUpdate();
		}
		
		void LvReadersDoubleClick(object sender, EventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				BtnOKClick(sender, e);
			}
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			if (!_settings.DontOfferToRemember)
				AppConfig.WriteBoolean("reader_reconnect", cbRemember.Checked);

			bool canQuit = true;
			try
			{
				_selected_reader = lvReaders.SelectedItems[0].Text;
				AppConfig.WriteString("reader_name", _selected_reader);
			}
			catch
			{
				MessageBox.Show("No reader selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				canQuit = false;
			}
				
			if(canQuit)
			{
				DialogResult = DialogResult.OK;
				Close();		
			}
		}
		
		void ReaderSelectFormShown(object sender, EventArgs e)
		{
			LoadReaderList();
			
			if (_selected_reader != null)
			{
				for (int i=0; i<lvReaders.Items.Count; i++)
				{
					if (lvReaders.Items[i].Text.Equals(_selected_reader))
					{
						lvReaders.SelectedItems.Clear();
						lvReaders.Items[i].Selected = true;
						btnOK.Enabled = true;
						btnOK.Focus();
						return;
					}
				}
			}
			
			if (_settings.PreselectReaderName != null)
			{
				string[] mask = _settings.PreselectReaderName.Split('|');
				
				for (int i=0; i<lvReaders.Items.Count; i++)
				{
					bool f = true;
					for (int j=0; j<mask.Length; j++)
					{
						if (!lvReaders.Items[i].Text.Contains(mask[j]))
						{
							f = false;
							break;
						}
					}
					if (f)
					{
						lvReaders.SelectedItems.Clear();
						lvReaders.Items[i].Selected = true;
						btnOK.Enabled = true;
						btnOK.Focus();
						return;
					}
				}
			}
		}
		
		void LvReadersSelectedIndexChanged(object sender, EventArgs e)
		{
			for (int i=0; i<lvReaders.Items.Count; i++)
				lvReaders.Items[i].ImageIndex = (lvReaders.Items[i].Selected) ? 0 : -1;

			UpdateDisplay();
		}
		
		void UpdateDisplay()
		{
			btnOK.Enabled = (lvReaders.SelectedItems.Count == 1);
		}
		
		void BtnRefreshLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LoadReaderList();
		}
		
		void ImgRefreshClick(object sender, EventArgs e)
		{
			LoadReaderList();	
		}
	}
}
