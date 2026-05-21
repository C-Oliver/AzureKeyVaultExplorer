// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Vault.Explorer
{
    public partial class ExceptionDialog : FormTelemetry
    {
        public ExceptionDialog(Exception e)
        {
            InitializeComponent();
            uxRichTextBoxCaption.Rtf = string.Format(@"{{\rtf1\ansi Oops... Unhandled exception of type \b {0} \b0 has occurred: \b {1} \b0 To ignore this error just click Continue, otherwise click Quit.}}", e.GetType().Name, Utils.GetRtfUnicodeEscapedString(e.Message));
            // Sanitize stack trace — remove file paths to avoid leaking internal directory structure
            var sanitizedDetails = System.Text.RegularExpressions.Regex.Replace(e.ToString(), @"in [A-Za-z]:\\[^\r\n]+", "in [redacted]");
            uxTextBoxExceptionDetails.Text = sanitizedDetails;
            uxTextBoxExceptionDetails.Select(0, 0);
        }

        private void uxButtonQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
