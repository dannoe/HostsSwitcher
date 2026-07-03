using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;

namespace Barbar.HostsSwitcher {
  [SupportedOSPlatform("windows")]
  public partial class FormCopy : Form {
    public string FileName {
      get { return txtFileName.Text; }
    }

    public FormCopy(string inputText) {
      InitializeComponent();
      lblText.Text = inputText;
      txtFileName.Text = string.Empty;
    }
  }
}
