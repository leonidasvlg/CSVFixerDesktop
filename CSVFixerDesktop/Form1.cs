using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSVFixerDesktop
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Global Variables
        /// </summary>
        string _filePath = Environment.SpecialFolder.MyDocuments.ToString();
        bool _isFirstLinetheHeader;


        /// <summary>
        /// Initialization, set the first row as column titles
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            mnuFirstRowHeader.Checked = true;
            _isFirstLinetheHeader = true;
        }


        /// <summary>
        /// Will read the only the first file if you drop more than one. This will call the ReadFile function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                _filePath = files[0];
                ReadFile(_filePath);            
            }
        }
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }


        /// <summary>
        /// Read file handler, will check if the csv consists from one line and update the front end elements
        /// </summary>
        /// <param name="filePath"></param>
        private void ReadFile(string filePath)
        {
            int lineChanges = MainFunction.CountLineChanges(filePath);

            if (lineChanges < 2)
            {
                // handle one line csv
                MainFunction.OneLineCsvHandling();
                MessageBox.Show("The csv is consisting from one single line, please specify the correct field count", "Cannot split csv!");

                // clear data grid view
                dataGridView1.Refresh();
                dataGridView1.DataSource = null;

                // refresh the combobox
                cbxFieldsCount.Items.Clear();
                foreach (var item in MainFunction.fieldDividers)
                    cbxFieldsCount.Items.Add(item);

                // Update the statusbar
                statusBar1.Text = "Please choose the correct column number ->";
            }
            else
            {
                MainFunction.GenerateCsvObjectFromString();
                updateDataGridView(MainFunction._parsedCsv);
            }

        }


        /// <summary>
        /// This will get the internal variables of the "MainFunction" class and will update the datagrid view
        /// </summary>
        /// <param name="parsedCsv"></param>
        private void updateDataGridView(List<string[]> parsedCsv)
        {
            // clear the Data Grid View
            dataGridView1.Refresh();
            dataGridView1.DataSource = null;

            // Convert to DataTable.
            DataTable table = new DataTable();

            // Add the column names
            int titleIndex = 0;
            if (_isFirstLinetheHeader == false)
            {
                for (int i = 0; i < parsedCsv[0].Length; i++)
                {
                    table.Columns.Add("col " + (i + 1), typeof(string));
                }
                titleIndex = 0;

                // Update the statusbar
                statusBar1.Text = "Rows: " + parsedCsv.Count();
            }
            else
            {
                for (int i = 0; i < parsedCsv[0].Length; i++)
                {
                    string colName = parsedCsv[0][i];
                    table.Columns.Add(colName, typeof(string));
                }
                titleIndex = 1;

                // Update the statusbar
                statusBar1.Text = "Rows: " + (parsedCsv.Count() - 1);
            }

            // Add the rows
            if (parsedCsv != null)
            {
                for (int i = titleIndex; i < parsedCsv.Count(); i++)
                {
                    table.Rows.Add(parsedCsv[i]);
                }

                // Fill the DataGrid View With data from DataTable
                dataGridView1.DataSource = table;
            }
        }


        /// <summary>
        /// Combobox Automation, will execute on entering a numeric value and press Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxFieldsCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                int fieldsCount = 0;
                if (Int32.TryParse(cbxFieldsCount.Text, out fieldsCount))
                {
                    if (fieldsCount <= MainFunction._maxAllowedFields)
                    {
                        MainFunction.GenerateCsvObject(fieldsCount);

                        updateDataGridView(MainFunction._parsedCsv);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a lower field number", "Dupplicate column name");         
                    }

                }
            }
        }


        /// <summary>
        /// Combobox Automation, will execute on choosing one of the items in the dropdown menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxFieldsCount_SelectedValueChanged(object sender, EventArgs e)
        {
            int fieldsCount = (int)cbxFieldsCount.SelectedItem;

            MainFunction.GenerateCsvObject(fieldsCount);

            updateDataGridView(MainFunction._parsedCsv);
        }


        /// <summary>
        /// Menu -> File -> Exit, will always ask if you want to exit the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really Quit?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Application.Exit();
            }
        }


        /// <summary>
        /// Menu -> File -> Save, will always open a file save dialog window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(_filePath);
            saveFileDialog1.Title = "Save csv file";
            //saveFileDialog1.CheckFileExists = true;
            //saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "Csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = "output";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //File.WriteAllText(saveFileDialog1.FileName, tbxOutput.Text);
            }
        }


        /// <summary>
        /// Menu -> File -> Open, will always open a file open dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(_filePath),
                Title = "Browse csv Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "csv",
                Filter = "csv files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _filePath = openFileDialog1.FileName;
                ReadFile(_filePath);
            }
        }


        /// <summary>
        /// Menu -> Settings -> First Row is Header. Will update the datagrid view each time you click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuFirstRowHeader_Click(object sender, EventArgs e)
        {
            if (mnuFirstRowHeader.Checked == true)
            {
                mnuFirstRowHeader.Checked = false;
                _isFirstLinetheHeader = false;
                updateDataGridView(MainFunction._parsedCsv);
            }
            else
            {
                mnuFirstRowHeader.Checked = true;
                _isFirstLinetheHeader = true;
                updateDataGridView(MainFunction._parsedCsv);
            }
                
        }


        /// <summary>
        /// "About" Window Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            windows.About AboutWindow = new windows.About();
            AboutWindow.Show();
        }


        /// <summary>
        /// top-right X button handler, will ask for confirmation before closing the main window.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }


    }
}
