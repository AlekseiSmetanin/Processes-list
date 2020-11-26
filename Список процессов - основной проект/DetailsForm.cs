using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Список_процессов___основной_проект
{
    /// <summary>
    /// Дочерняя форма, содержащая подробное описание свойств процесса
    /// </summary>
    public partial class DetailsForm : Form
    {
        public DetailsForm()
        {
            InitializeComponent();
        }

        public DetailsForm(ProcessInformation selectedProcessInformation)
        {
            InitializeComponent();


            SelectedProcessInformation = selectedProcessInformation;
            Text = String.Format("Свойства: {0} ({1})", SelectedProcessInformation.Name, SelectedProcessInformation.PID);
            InitializeProcessData();
            ActiveControl = label1;
        }

        /// <summary>
        /// Информация о процессе, на котором был сделан двойной клик мышью в MainForm
        /// </summary>
        public ProcessInformation SelectedProcessInformation { private set; get; }

        /// <summary>
        /// Инициализирует данные о процессе
        /// </summary>
        private void InitializeProcessData()
        {
            try
            {
                textBox1.Text = SelectedProcessInformation.Path;
            }
            catch(Exception exc)
            {
                textBox1.Text = "Неизвестен";
            }

            try
            {
                textBox2.Text = String.Format("{0} ({1})",SelectedProcessInformation.ParentName, SelectedProcessInformation.ParentPID);
            }
            catch (NullReferenceException nullRefExc)
            {
                textBox2.Text = String.Format("Несуществующий процесс ({0})", SelectedProcessInformation.ParentPID);
            }
            catch (Exception exc)
            {
                textBox2.Text = "Неизвестен";
            }

            try
            {
                textBox4.Text = SelectedProcessInformation.DEP_and_ASLR;
            }
            catch (Exception exc)
            {
                textBox4.Text = "Недоступно";
            }

            try
            {
                textBox5.Text = SelectedProcessInformation.IntegrityLevel;
            }
            catch (Exception exc)
            {
                textBox5.Text = "Недоступно";
            }

            InitializeListView();
            ListViewItem listViewItem;
            try
            {
                for (int i = 0; i < SelectedProcessInformation.DLL_List.Count; i++)
                {
                    listViewItem = new ListViewItem(SelectedProcessInformation.DLL_List[i].ModuleName);
                    listView1.Items.Add(listViewItem);
                }
            }
            catch (Exception exc)
            {
                label6.Text = "Недоступно";
            }
        }

        /// <summary>
        /// Инициализирует элемент формы listView1 перед его первым использованием
        /// </summary>
        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.AllowColumnReorder = true;
            listView1.FullRowSelect = true;
            listView1.Sorting = SortOrder.Ascending;

            listView1.Columns.Add("Имя DLL", 200, HorizontalAlignment.Left);
        }
    }
}
