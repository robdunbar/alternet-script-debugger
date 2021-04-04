// Copyright (c) 2021 Cognex Corporation. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace RuntimeServer
{
    /// <summary>
    /// Used to redirect Trace output to a TextBox.
    /// </summary>
    public class TextBoxWriter : TextWriterTraceListener
    {
        private readonly TextBox _textBox;

        public TextBoxWriter(TextBox textBox)
        {
            _textBox = textBox;
        }

        /// <inheritdoc />
        public override void WriteLine(string message)
        {
            base.WriteLine(message);
            AddText(message + Environment.NewLine);
        }

        private void AddText(string message)
        {
            _textBox.BeginInvoke((Action)(() =>
                                                 {
                                                     _textBox.Text = message + _textBox.Text;
                                                 }));
        }
    }
}
