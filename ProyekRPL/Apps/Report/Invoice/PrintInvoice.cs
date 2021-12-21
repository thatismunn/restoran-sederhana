﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ProyekRPL.Module;

namespace ProyekRPL.Apps.Report.Invoice
{
    public partial class PrintInvoice : Form
    {
        public PrintInvoice(PrintInvoiceParam param)
        {
            InitializeComponent();
            Initial.ConfigDatabase();
            this._param = param;

            IniFile ini = new IniFile("ApplicationConfig.ini");
            RestaurantIdentity.Name = ini.Read("RestaurantName", "General");
            RestaurantIdentity.Address = ini.Read("Address", "General");
            RestaurantIdentity.PhoneNumber = ini.Read("PhoneNumber", "General");

            string query = string.Format("SELECT * FROM v_receipt WHERE pesananID='{0}'", this._param.ID);
            this._data = SQL.GetDataQuery(query);
        }

        private readonly int _widthGraph = 40;

        private readonly string[][] _data;

        private readonly PrintInvoiceParam _param;

        public class PrintInvoiceParam
        {
            public uint ID;
            public int? PayPrice;
        }

        private class RestaurantIdentity
        {
            public static string Name = "";
            public static string Address = "";
            public static string PhoneNumber = "";
        }

        private int CenterTextPosition(string text)
        {
            return (int)Math.Floor((decimal)(this._widthGraph / 2 - text.Length / 2));
        }

        private string EmptyStringCenter(string text)
        {
            return new string(' ', CenterTextPosition(text)) + text;
        }

        private string LimitString(string text, int length)
        {
            return text.Length > length ? text.Substring(0, length) : text;
        }

        private string[] SplitMaxString(string text)
        {
            int max = this._widthGraph;
            List<string> list = new List<string>();

            // String lebih lebar dari batas maximum
            if (text.Length > max)
            {
                string[] split = text.Split(' ');
                List<string> temp = new List<string>();
                foreach (string s in split)
                {
                    temp.Add(s);
                    string strTemp = string.Join(" ", temp.ToArray());
                    if (strTemp.Length > max)
                    {
                        temp.RemoveAt(temp.Count - 1);
                        list.Add(string.Join(" ", temp.ToArray()));
                        temp = new List<string> { s };
                    }
                }
                list.Add(string.Join(" ", temp.ToArray()));
            }
            else
                list.Add(text);

            return list.ToArray();
        }

        private void PrintInvoice_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int startX = 0;
            int startY = 0;
            int offset = 10;

            void DrawString(string text, bool center = false)
            {
                string[] splitted = SplitMaxString(text);
                foreach (string s in splitted)
                {
                    startY += offset * 2;
                    g.DrawString(center ? EmptyStringCenter(s) : s, new Font("Hack NF", 10),
                        new SolidBrush(Color.Black),
                        startX + offset, startY + offset);
                }
            }

            // Awalan dikurang offset 2x
            startY -= offset * 2;

            // Header
            DrawString(RestaurantIdentity.Name);
            DrawString(RestaurantIdentity.Address);
            DrawString("Telp. " + RestaurantIdentity.PhoneNumber);

            // Detail
            startY += offset;
            DateTime dateInvoice = DateTime.Parse(_data[0][5]);
            DrawString(string.Format("No.  : {0}", this._data[0][1]).PadRight(30, ' ') + dateInvoice.ToString("yyyy-MM-dd"));
            DrawString(string.Format("Kasir: {0}", this._data[0][2]).PadRight(32, ' ') + dateInvoice.ToString("HH:mm:ss"));
            DrawString("Nama Pelanggan: " + this._data[0][3]);
            DrawString(new string('-', this._widthGraph));

            // Isi barang
            int total = 0, qty = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                int money = int.Parse(this._data[i][8]);
                int qtyItem = int.Parse(this._data[i][9]);
                DrawString(
                    string.Format("{0}{1}",
                    ((i+1).ToString() + ".").PadRight(4, ' '),
                    LimitString(this._data[i][7], 36).ToUpper())
                );
                DrawString(
                    string.Format("    {0} x {1} = {2}", // (4 + 10 + 3 + 4 + 3) - 40
                    string.Format("{0:N0}", money).PadRight(10, ' '),
                    qtyItem.ToString().PadRight(4, ' '),
                    string.Format("{0:N0}", money * qtyItem).PadLeft(16, ' ')
                    )
                );

                total += money;
                qty += qtyItem;
            }
            DrawString(new string('-', this._widthGraph));

            // Result
            int pay = this._param.PayPrice.HasValue ? this._param.PayPrice.Value : total;
            int diff = pay - total;
            DrawString(
                string.Format("{0} {1} {2}", // (12 + 8) - 40
                "BARIS=" + _data.Length.ToString().PadRight(4, ' '),
                "QTY=" + qty.ToString().PadRight(5, ' '),
                string.Format("{0:N0}", total).PadLeft(19, ' '))
            );
            DrawString(
                string.Format("Tunai{0}={1}", // (40/2) -> (20-6) + 20
                new string(' ', 14),
                string.Format("{0:N0}", pay).PadLeft(20, ' '))
            );
            DrawString(new string(' ', 20) + new string('-', 20));
            DrawString(
                string.Format("Kembali{0}={1}", // (40/2) -> (20-8) + 20
                new string(' ', 12),
                string.Format("{0:N0}", diff).PadLeft(20, ' '))
            );

            // Footer
            startY += offset * 2;
            DrawString("Terima Kasih", true);
            DrawString("Selamat Menikmati", true);
        }
    }
}