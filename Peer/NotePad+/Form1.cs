using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotePad_
{
    /// <summary>
    /// Главное окно приложения.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Приватное поле, в котором хранятся пути всех открытых файлов.
        /// </summary>
        private StringCollection? pathes = new();
        /// <summary>
        /// Флаг для открытия нового окна.
        /// </summary>
        private bool newWindowOpen { get; set; }

        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Открытие файла через меню.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog1.Filter = "text document|*.txt|rtf documents |*.rtf|all |*.*";

                if ((OpenFileDialog1.ShowDialog() == DialogResult.OK) && (OpenFileDialog1.FileName.Length > 0))
                {
                    var pathToOpen = OpenFileDialog1.FileName;
                    var newPage = new TabPage($"{Path.GetFileName(pathToOpen)}");
                    var newTextBox = new RichTextBox();
                    newTextBox.Dock = DockStyle.Fill;
                    newTextBox.Text = "";
                    newTextBox.ContextMenuStrip = ContextMenuStrip1;
                    newTextBox.BackColor = Properties.Settings.Default.SavedColorMenu;
                    pathes.Add(pathToOpen);
                    // Если пользователь хочет сохранить изменения на начальной странице при открытии нового файла.
                    if (String.IsNullOrEmpty(tabPage1.Controls[0].Text))
                    {
                        tabControl1.Controls.Remove(tabPage1);
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show($"Вы хотите сохранить изменения у начальной страницы?", "Внимание!", MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            SaveFileAs();
                        }
                    }
                    // Запись текста в текстовое поле.
                    tabControl1.TabPages.Add(newPage);
                    newPage.Controls.Add(newTextBox);
                    switch (Path.GetExtension(pathToOpen))
                    {
                        case ".txt":
                            newTextBox.Text = File.ReadAllText(pathToOpen);
                            break;
                        case ".rtf":
                            newTextBox.LoadFile(pathToOpen);
                            break;
                        default:
                            newTextBox.Text = File.ReadAllText(pathToOpen);
                            break;
                    }
                    tabControl1.SelectTab(newPage);
                }
            }
            catch (Exception allExceptions)
            {
                MessageBox.Show(allExceptions.ToString());
            }
        }

        /// <summary>
        /// Сохранение файла с выбором пути через меню.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            SaveFileAs();
        }

        /// <summary>
        /// Основной метод для сохранения.
        /// </summary>
        private void SaveFileAs()
        {
            try
            {
                SaveFileDialog1.Filter = "text document|*.txt|rtf documents |*.rtf";
                SaveFileDialog1.RestoreDirectory = true;
                if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var pathToSave = SaveFileDialog1.FileName;
                    if (Path.GetExtension(pathToSave).ToLower() == ".rtf")
                    {
                        RichTextBox textBoxtmp = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                        textBoxtmp.SaveFile(pathToSave, RichTextBoxStreamType.RichText);
                    }
                    else if (Path.GetExtension(pathToSave).ToLower() == ".txt")
                    {
                        File.WriteAllText(pathToSave, tabControl1.SelectedTab.Controls[0].Text);
                    }
                }

            }
            catch (Exception allExceptions)
            {
                MessageBox.Show(allExceptions.ToString());
            }
        }

        /// <summary>
        /// Закрытие формы и сохранение открытых вкладок и насроек.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            try
            {
                if (pathes != null)
                {
                    for (var i = 0; i < tabControl1.TabPages.Count; i++)
                    {
                        try
                        {
                            CheckSaving(i);
                        }
                        catch (Exception)
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception allExceptions)
            {
                MessageBox.Show(allExceptions.ToString());
            }
        }

        /// <summary>
        /// Проверка изменения и запрос о сохранении.
        /// </summary>
        /// <param name="i">Индекс вкладки. </param>
        /// <exception cref="ArgumentException">Выбрасывается если нужно выйти из цикла.</exception>
        private void CheckSaving(int i)
        {
            try
            {
                if (File.Exists(pathes[i]))
                {
                    if (Path.GetExtension(pathes[i]) == ".rtf")
                    {
                        var textBoxTmp = (RichTextBox)tabControl1.TabPages[i].Controls[0];
                        // Создание нового текст бокса в который загружается старая версия изменяемого файла Rtf.
                        var newTextBoxForCompare = new RichTextBox();
                        newTextBoxForCompare.LoadFile(pathes[i]);
                        if (textBoxTmp.Rtf != newTextBoxForCompare.Rtf)
                        {
                            DialogResult result = MessageBox.Show($"Вы хотите сохранить изменения у {Path.GetFileName(pathes[i])}?", "Внимание!", MessageBoxButtons.YesNoCancel);
                            if (result == DialogResult.Yes)
                            {
                                textBoxTmp.SaveFile(pathes[i], RichTextBoxStreamType.RichText);
                            }
                            else if (result == DialogResult.Cancel)
                            {
                                throw new ArgumentException();
                            }
                        }
                    }
                    else if (Path.GetExtension(pathes[i]) == ".txt")
                    {
                        if (tabControl1.TabPages[i].Controls[0].Text != File.ReadAllText(pathes[i]))
                        {
                            DialogResult result = MessageBox.Show($"Вы хотите сохранить изменения у {Path.GetFileName(pathes[i])}?", "Внимание!", MessageBoxButtons.YesNoCancel);
                            if (result == DialogResult.Yes)
                            {
                                File.WriteAllText(pathes[i], tabControl1.TabPages[i].Controls[0].Text);
                            }
                            else if (result == DialogResult.Cancel)
                            {
                                throw new ArgumentException();
                            }
                        }
                    }

                }
                else
                {
                    // Если файл не rtf и не txt, скорее всего новая вкладка.
                    DialogResult result = MessageBox.Show($"Вы хотите сохранить {pathes[i]}", "Внимание!", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        SaveFileAs();
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        throw new ArgumentException();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        /// <summary>
        /// Изменение шрифта.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void Font1(object sender, EventArgs e)
        {
            try
            {
                var fontDialog1 = new FontDialog();
                fontDialog1.ShowColor = true;
                RichTextBox textBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                fontDialog1.Font = textBox.Font;
                fontDialog1.Color = textBox.ForeColor;

                if (fontDialog1.ShowDialog() != DialogResult.Cancel)
                {
                    textBox.Font = fontDialog1.Font;
                    textBox.ForeColor = fontDialog1.Color;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        /// <summary>
        /// Редактирование выделенного текста.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void FormatingToolStripMenuItemClick(object sender, EventArgs e)
        {
            SelectedChanges();
        }

        /// <summary>
        /// Установка период автоматического сохранения.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void PeriodOfSavingToolStripMenuItemClick(object sender, EventArgs e)
        {
            var timeForm = new Form2();
            timeForm.ShowDialog();
        }

        /// <summary>
        /// Один тик таймера.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void Timer1Tick(object sender, EventArgs e)
        {
            AutoSaveFile();
        }

        /// <summary>
        /// Автоматическое сохранение.
        /// </summary>
        private void AutoSaveFile()
        {
            try
            {
                for (var i = 0; i < tabControl1.TabCount; i++)
                {
                    if ((pathes[i] != null) && File.Exists(pathes[i]))
                    {
                        if (tabControl1.TabPages[i] == tabPage1)
                        {
                            MessageBox.Show("Для сохранения файла, нужно указать его путь.");
                            SaveFileAs();
                        }
                        else
                        {
                            if (Path.GetExtension(pathes[i]).ToLower() == ".rtf")
                            {
                                RichTextBox textBoxtmp = (RichTextBox)tabControl1.TabPages[i].Controls[0];
                                textBoxtmp.SaveFile(pathes[i], RichTextBoxStreamType.RichText);
                            }
                            else if (Path.GetExtension(pathes[i]).ToLower() == ".txt")
                            {
                                File.WriteAllText(pathes[i], tabControl1.TabPages[i].Controls[0].Text);
                            }
                        }

                    }
                }
            }
            catch (Exception allExceptions)
            {
                MessageBox.Show(allExceptions.ToString());
            }
        }

        /// <summary>
        /// Загрузка формы.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void Form1Load(object sender, EventArgs e)
        {
            try
            {
                tabControl1.TabPages.Remove(tabPage1);
                if (Properties.Settings.Default.SavedPathes != null)
                {
                    pathes = Properties.Settings.Default.SavedPathes;
                }
                if (Properties.Settings.Default.SavedColorMenu != Color.Empty)
                {
                    MenuStrip1.BackColor = Properties.Settings.Default.SavedColorMenu;
                }
                if (!newWindowOpen)
                {
                    OpenTabs(pathes);
                }
                var timeForm = new Form2();
                timeForm.ShowDialog();
                Timer1.Interval = (int)timeForm.PeriodOfTime * 1000 * 60;
                Timer1.Start();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

        }

        /// <summary>
        /// Сохранение файла без указания пути.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.SelectedTab == tabPage1)
                {
                    MessageBox.Show("Для сохранения файла, нужно указать его путь.");
                    SaveFileAs();
                }
                else
                {
                    if (Path.GetExtension(pathes[tabControl1.SelectedIndex]).ToLower() == ".rtf")
                    {
                        RichTextBox textBoxtmp = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                        textBoxtmp.SaveFile(pathes[tabControl1.SelectedIndex], RichTextBoxStreamType.RichText);
                    }
                    else if (Path.GetExtension(pathes[tabControl1.SelectedIndex]).ToLower() == ".txt")
                    {
                        File.WriteAllText(pathes[tabControl1.SelectedIndex], tabControl1.SelectedTab.Controls[0].Text);
                    }
                }
            }
            catch (Exception allExceptions)
            {
                MessageBox.Show(allExceptions.ToString());
            }
        }

        /// <summary>
        /// Создания нового окна.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void NewWindowToolStripMenuItemClick(object sender, EventArgs e)
        {
            var newWindow = new Form1();
            newWindow.newWindowOpen = true;
            newWindow.Show();
        }

        /// <summary>
        /// Создание новой вкладки.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void NewTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                var newPage = new TabPage();
                var txtBox = new RichTextBox();
                txtBox.Dock = DockStyle.Fill;
                txtBox.ContextMenuStrip = ContextMenuStrip1;
                if (tabControl1.TabCount < 8)
                {
                    if (Properties.Settings.Default.SavedColorMenu != Color.Empty)
                    {
                        MenuStrip1.BackColor = Properties.Settings.Default.SavedColorMenu;
                    }
                    if (Properties.Settings.Default.SavedColorTetxBox != Color.Empty)
                    {
                        MenuStrip1.BackColor = Properties.Settings.Default.SavedColorTetxBox;
                    }
                    txtBox.Text = "";
                    pathes.Add("empty wrong way");
                    newPage.Controls.Add(txtBox);
                    tabControl1.Controls.Add(newPage);
                }
                else
                {
                    MessageBox.Show("Максимальное кол-во вкладок 7");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        /// <summary>
        /// Сохранения всех вкладок.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void SaveAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            AutoSaveFile();
        }

        /// <summary>
        /// Выбор всего текста.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            RichTextBox textBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];
            textBox.SelectAll();
        }

        /// <summary>
        /// Вырезание выбранного фрагмента текста.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void CutOutToolStripMenuItemClick(object sender, EventArgs e)
        {
            SendKeys.Send("^{X}");
        }

        /// <summary>
        /// Копирование.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void CopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            SendKeys.Send("^{C}");
        }

        /// <summary>
        /// Вставка из буфера обмена.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void PasteToolStripMenuItemClick(object sender, EventArgs e)
        {
            SendKeys.Send("^{V}");
        }

        /// <summary>
        /// Форматирование выбранного фрагмента текста.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void FormatToolStripMenuItem1Click(object sender, EventArgs e)
        {
            SelectedChanges();
        }

        /// <summary>
        /// Форматирование текста.
        /// </summary>
        private void SelectedChanges()
        {
            try
            {
                var fontDialog1 = new FontDialog();

                fontDialog1.ShowColor = true;
                RichTextBox textBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                fontDialog1.Font = textBox.SelectionFont;
                fontDialog1.Color = textBox.SelectionColor;

                if (fontDialog1.ShowDialog() != DialogResult.Cancel)
                {
                    textBox.SelectionFont = fontDialog1.Font;
                    textBox.SelectionColor = fontDialog1.Color;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        /// <summary>
        /// CTRL + Z.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void CancelActionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var MyBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];
            MyBox.Undo();
        }

        /// <summary>
        /// CTRL+SHIFT+Z.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void RepeatToolStripMenuItemClick(object sender, EventArgs e)
        {
            SendKeys.Send("^{Y}");
        }

        /// <summary>
        /// Сохранение открытых вкладок и настроек.
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.SavedPathes = pathes;
                Properties.Settings.Default.SavedColorMenu = MenuStrip1.BackColor;
                Properties.Settings.Default.SavedColorTetxBox = MenuStrip1.BackColor;
                Properties.Settings.Default.Save();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        /// <summary>
        /// Открытие вкладок, которые были открыты при закрытие.
        /// </summary>
        /// <param name="path"></param>
        private void OpenTabs(StringCollection path)
        {
            try
            {
                if (path != null)
                {
                    for (var i = 0; i < path.Count; i++)
                    {
                        if ((path[i] != null) && File.Exists(path[i]))
                        {
                            // Создание нового RichTextBox и прикрепление его к новой вкладке.
                            var pathToOpen = path[i];
                            var newPage = new TabPage($"{Path.GetFileName(pathToOpen)}");
                            var newTextBox = new RichTextBox();
                            newTextBox.Dock = DockStyle.Fill;
                            if (Properties.Settings.Default.SavedColorTetxBox != Color.Empty)
                            {
                                newTextBox.BackColor = Properties.Settings.Default.SavedColorTetxBox;
                            }
                            newTextBox.Text = "";
                            newTextBox.ContextMenuStrip = ContextMenuStrip1;
                            tabControl1.TabPages.Add(newPage);
                            newPage.Controls.Add(newTextBox);
                            tabControl1.TabPages.Remove(tabPage1);

                            switch (Path.GetExtension(pathToOpen))
                            {
                                case ".txt":
                                    newTextBox.Text = File.ReadAllText(pathToOpen);
                                    break;
                                case ".rtf":
                                    newTextBox.LoadFile(pathToOpen);
                                    break;
                                default:
                                    newTextBox.Text = File.ReadAllText(pathToOpen);
                                    break;
                            }
                            tabControl1.SelectTab(newPage);
                        }
                        // Если такого файла нет или его путь изменился, он удаляется из pathes. 
                        else
                        {
                            pathes.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception allExceptions)
            {
                MessageBox.Show(allExceptions.ToString());
            }
        }

        /// <summary>
        /// Стандартная тема.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void StandartToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                MenuStrip1.BackColor = Color.White;
                int countOfPages = tabControl1.TabPages.Count;
                for (var i = 0; i < countOfPages; i++)
                {
                    tabControl1.TabPages[i].Controls[0].BackColor = Color.White;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        /// <summary>
        /// Морская тема.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void SeaToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                MenuStrip1.BackColor = Color.Aquamarine;
                int countOfPages = tabControl1.TabPages.Count;
                for (var i = 0; i < countOfPages; i++)
                {
                    tabControl1.TabPages[i].Controls[0].BackColor = Color.Aquamarine;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

        }
        /// <summary>
        /// Закрытие выбранной вкладки.
        /// </summary>
        /// <param name="sender">Издатель.</param>
        /// <param name="e">Ифнормация о событии.</param>
        private void CloseTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.TabCount > 1)
                {

                    CheckSaving(tabControl1.SelectedIndex);
                    pathes.RemoveAt(tabControl1.SelectedIndex);
                    tabControl1.Controls.RemoveAt(tabControl1.SelectedIndex);
                }
                else
                {
                    MessageBox.Show("Вы не можете закрыть последнюю вкладку.");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }
    }
}

