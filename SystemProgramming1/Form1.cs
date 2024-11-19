using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SystemProgramming1
{
    public partial class Form1 : Form
    {

        bool firstPassError = false;
        FirstSecondPass pass;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           firstPassError = true;
           pass = new FirstSecondPass();
           Clear_tables();
           pass.DeleteEmptyRows(DG_Source);

           //создаем динамический массив куда помещаем нашу таблицу исходного кода
           string[,] arr_SourceCode = new string[DG_Source.RowCount-1, DG_Source.Columns.Count];

           for (int i = 0; i < DG_Source.RowCount-1;i++ )
           {
               for (int j = 0; j < DG_Source.Columns.Count; j++)
                    arr_SourceCode[i, j] = Convert.ToString(DG_Source.Rows[i].Cells[j].Value);
           }
             

           //создаем динамический массив куда помещаем нашу таблицу кодов операций
           string[,] arr_OperCode = new string[DG_OperCode.RowCount - 1, DG_OperCode.Columns.Count];

           for (int i = 0; i < DG_OperCode.RowCount - 1; i++)
           {
                for (int j = 0; j < DG_OperCode.Columns.Count; j++)
                {
                    arr_OperCode[i, j] = Convert.ToString(DG_OperCode.Rows[i].Cells[j].Value);
                    arr_OperCode[i, j] = arr_OperCode[i, j].ToUpper();
                }
            }
              

           //проверяем таблицу кодов операций
           if (pass.CheckOperationCodeTable(ref arr_OperCode))
           {
               //делаем первый проход
               if (pass.FirstPass(arr_SourceCode, arr_OperCode,dataGrid_supportTable, dataGrid_symbol_table))
               {
                  firstPassError = false;
                  button2.Enabled = true;
                  AddErrorTextBox(tbErrorOnePass, pass.ErrorMessage);
               }
               else
               {
                  AddErrorTextBox(tbErrorOnePass, pass.ErrorMessage);
               }
           }
           else
           {
              AddErrorTextBox(tbErrorOnePass, pass.ErrorMessage);
           }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DG_Source.Rows.Add("PROG", "START", "100", "");
            DG_Source.Rows.Add("", "JMP", "L1", "");
            DG_Source.Rows.Add("A1", "RESB", "10", "");
            DG_Source.Rows.Add("A2", "RESW", "20", "");
            DG_Source.Rows.Add("B1", "WORD", "40", "");
            DG_Source.Rows.Add("B2", "BYTE", "X" + '"' + "2F4C008A" + '"', "");
            DG_Source.Rows.Add("B3", "BYTE", "C" + '"' + "Hello!" + '"', "");
            DG_Source.Rows.Add("B4", "BYTE", "12", "");
            DG_Source.Rows.Add("L1", "LOADR1", "B1", "");
            DG_Source.Rows.Add("", "LOADR2", "B4", "");
            DG_Source.Rows.Add("", "ADD", "R1", "R2");
            DG_Source.Rows.Add("", "SAVER1", "B1", "");
            DG_Source.Rows.Add("", "INT", "200", "");
            DG_Source.Rows.Add("", "END", "100", "");

            DG_OperCode.Rows.Add("JMP", "01", "4");
            DG_OperCode.Rows.Add("LOADR1", "02", "4");
            DG_OperCode.Rows.Add("LOADR2", "03", "4");
            DG_OperCode.Rows.Add("ADD", "04", "2");
            DG_OperCode.Rows.Add("SAVER1", "05", "4");
            DG_OperCode.Rows.Add("INT", "06", "2");

            DG_Source.Update();
            DG_OperCode.Update();
        }

        void Clear_tables()
        {
            dataGrid_supportTable.Rows.Clear();
            dataGrid_symbol_table.Rows.Clear();
            tbErrorOnePass.Clear();
            tbErrorTwoPass.Clear();
            tbBinaryCode.Items.Clear();
        }

        void AddErrorTextBox(TextBox textBox_first_errors, string message)
        {
            textBox_first_errors.Text += message + Environment.NewLine;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tbBinaryCode.Items.Clear();
            tbErrorTwoPass.Clear();

            if (firstPassError == false)
                if (pass.Second_pass(tbBinaryCode))
                {
                    AddErrorTextBox(tbErrorTwoPass, pass.ErrorMessage);
                }
                else
                {
                    AddErrorTextBox(tbErrorTwoPass, pass.ErrorMessage);
                }
            else
                MessageBox.Show("Выполните первый проход");

        }

        private void dataGrid_Source_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            Clear_tables();
            button2.Enabled = false;
        }

        private void dataGrid_symbol_table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void tbErrorOnePass_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}