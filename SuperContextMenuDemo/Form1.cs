using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Zhwang.SuperContextMenu;

namespace SuperContextMenuDemo
{
    public partial class Form1 : Form
    {
        SuperContextMenu _menu = new SuperContextMenu();

        public Form1()
        {
            InitializeComponent();

            ContextMenu = _menu;

            _menu.MenuItems.Add(new SuperMenuItem() { Text = "Delete something", Image = Properties.Resources.DeleteHS });

            Timer a = new Timer() { Interval = 3000 };
            a.Tick += (sender, e) =>
            {
                _menu.MenuItems.Add(new SuperMenuItem() { Text = "No wai!" });
                _menu.FixAfterAdd();
            };
            a.Start();
        }

    }
}
