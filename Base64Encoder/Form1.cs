using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Base64Encoder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void encodeBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                string fileName = openFileDialog.SafeFileName;
                int charsInExtension = fileName.Length - fileName.LastIndexOf('.') - 1;
                saveFileDialog.FileName = fileName.Remove(fileName.Length - charsInExtension, charsInExtension) + "b64";
                saveFileDialog.Filter = "B64 File (*.b64)|*.b64";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    B64Encoder encoder = new B64Encoder(openFileDialog.FileName, saveFileDialog.FileName);
                    try
                    {
                        encoder.encode();
                        MessageBox.Show("Kodowanie zakończone sukcesem.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SystemException er)
                    {
                        MessageBox.Show(er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void decodeBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "B64 File (*.b64)|*.b64";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                B64Encoder encoder = new B64Encoder(openFileDialog.FileName);
                try
                {
                    encoder.guessTheExtension();

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    string fileName = encoder.DstFileName.Remove(0, encoder.DstFileName.LastIndexOf('\\') + 1);
                    string extension = fileName.Remove(0, fileName.LastIndexOf('.') + 1);
                    saveFileDialog.FileName = fileName;
                    saveFileDialog.Filter = extension + " File (*." + extension + ")|*." + extension;
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        encoder.DstFileName = saveFileDialog.FileName;
                        encoder.decode();
                        MessageBox.Show("Dekodowanie zakończone sukcesem.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (SystemException er)
                {
                    MessageBox.Show(er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
