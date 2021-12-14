﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MetroFramework.Forms;
using ProyekRPL.Module;
using System.Collections;

namespace ProyekRPL.Apps.Cashier
{
    public partial class MainForm : MetroForm
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        public MainForm()
        {
            InitializeComponent();

            RegisterHotKey(this.Handle, 1, 0x0000, (int)Keys.F1);
            RegisterHotKey(this.Handle, 2, 0x0000, (int)Keys.F2);
            RegisterHotKey(this.Handle, 3, 0x0000, (int)Keys.F3);
            RegisterHotKey(this.Handle, 4, 0x0000, (int)Keys.F4);
            RegisterHotKey(this.Handle, 5, 0x0000, (int)Keys.F5);
            RegisterHotKey(this.Handle, 6, 0x0000, (int)Keys.F6);
        }

        protected override void WndProc(ref Message m)
        {
            // Kode ini untuk override WndProc untuk membaca hotkey yang
            // diregister saat initialisasi komponen
            if (m.Msg == 0x0312)
            {
                switch (m.WParam.ToInt32())
                {
                    case 1: MenuSearchTxt.Focus(); break;
                    case 2: BuyBtn.PerformClick(); break;
                    case 3: PrintBtn.PerformClick(); break;
                    case 4: ResetBtn.PerformClick(); break;
                    case 5: CustomerNameTxt.Focus(); break;
                    case 6: TableNumberTxt.Focus(); TableNumberTxt.Select(0, TableNumberTxt.Text.Length); break;
                }
            }

            base.WndProc(ref m);
        }

        private void MenuInsertDatagrid(string[][] data)
        {
            ArrayList arr = new ArrayList();
            MenuDataGrid.Rows.Clear();
            foreach (string[] s in data)
            {
                arr.Add(s[0]);
                arr.Add(s[1]);
                arr.Add(s[2]);
                MenuDataGrid.Rows.Add(arr.ToArray());
                arr = new ArrayList();
            }
        }

        private void OrderInsertDatagrid(string[] data)
        {
            ArrayList arr = new ArrayList();
            arr.Add(data[0]);
            arr.Add(data[1]);
            arr.Add(data[2]);
            arr.Add("1");
            arr.Add(data[2]);
            OrderDataGrid.Rows.Add(arr.ToArray());
        }

        private void RefreshMenuData()
        {
            this.MenuInsertDatagrid(SQL.GetDataQuery("SELECT id, nama_menu, harga FROM menu"));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.RefreshMenuData();
            this.CustomerNameTxt.Focus();
        }

        private void MenuSearchTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            string query = MenuSearchTxt.Text;
            query = string.Format("SELECT * FROM menu WHERE id LIKE '%{0}%' OR nama_menu LIKE '%{0}%'", query);
            this.MenuInsertDatagrid(SQL.GetDataQuery(query));

            MenuSearchTxt.Text = "";
            MenuDataGrid.Focus();
        }

        private void MenuDataGrid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter) return;
            List<string> list = new List<string>();
            for (int i = 0; i < 3; i++) list.Add(DataGridHelper.GetValueSelectedRow(MenuDataGrid, i, 1));
            var data = list.ToArray();

            int rowNum = DataGridHelper.IsIncludeCell(OrderDataGrid, 0, data[0]);

            if (rowNum < 0)
            {
                this.OrderInsertDatagrid(data);
            }
            else
            {
                int qty = int.Parse(OrderDataGrid.Rows[rowNum].Cells[3].Value.ToString()) + 1;
                OrderDataGrid.Rows[rowNum].Cells[3].Value = qty.ToString();
            }

            this.RefreshMenuData();
        }

        private void MenuDataGrid_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SendKeys.SendWait("{ENTER}");
        }

        private void BuyBtn_Click(object sender, EventArgs e)
        {
            //
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            //
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            this.OrderDataGrid.Rows.Clear();
            this.CustomerNameTxt.Clear();
            this.CustomerNameTxt.Focus();
        }
    }
}
