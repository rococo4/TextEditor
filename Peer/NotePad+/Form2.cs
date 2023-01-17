using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotePad_
{
    /// <summary>
    /// Окно ввода времени таймера.
    /// </summary>
    public partial class Form2 : Form
    {
        /// <summary>
        /// Время автоматического сохранения.
        /// </summary>
        public uint PeriodOfTime {get; set;}
        
        /// <summary>
        /// Таймер.
        /// </summary>
        public Form2()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Проверка ввода времени.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>

        private void TextBox1TextChanged(object sender, EventArgs e)
        {
            if((textBox1.Text != "") && (uint.TryParse(textBox1.Text, out uint time) && (time <= 10) && (time >= 1)))
            {
                PeriodOfTime = time;
            }
            else
            {
                MessageBox.Show("Введите число от 1 до 10 - количество минут.");
                textBox1.Text = "";
                PeriodOfTime = 0;
            }
        }

        /// <summary>
        /// Закрытие формы.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void Form2FormClosing(object sender, FormClosingEventArgs e)
        {
            if (PeriodOfTime == 0)
            {
                e.Cancel = true;
            }
        }
    }
}
