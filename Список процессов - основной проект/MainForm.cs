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
    /// Основная форма со списком процессов и их PID'ами
    /// </summary>
    public partial class MainForm : Form
    {
        private ProcessInformation[] processesInformation;
        private Comparison<ProcessInformation> processesInformationComparison = (x, y) => x.Name.CompareTo(y.Name);
        private int listView1_ColumnClickIndex = 0;//индекс столбца, на заголовке которого в последний раз щёлкали мышью
        private int listView1_ColumnClickOrder = 1;//1 для сортировки по возрастанию, -1 по убыванию

        public MainForm()
        {
            InitializeComponent();

            InitializeListView();
            RenewDataInListView();
            timer1.Start();
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

            listView1.Columns.Add("Имя процесса", 200, HorizontalAlignment.Left);
            listView1.Columns.Add("PID", 100, HorizontalAlignment.Left);
        }

        /// <summary>
        /// Обновляет данные в элементе формы listView1
        /// </summary>
        private void RenewDataInListView()
        {
            processesInformation = ProcessInformation.GetProcessesInformation();
            Array.Sort<ProcessInformation>(processesInformation, processesInformationComparison);

            if (processesInformation.Length == listView1.Items.Count)//если число процессов не изменилось, но могли измениться сами процессы,
            {
                for (int i = 0; i < listView1.Items.Count; i++)//то нужно переписать текстовую информацию в listView1
                {
                    if (listView1.Items[i].Text != processesInformation[i].Name)
                        listView1.Items[i].Text = processesInformation[i].Name;
                    if (listView1.Items[i].SubItems[1].Text != String.Format("{0}", processesInformation[i].PID))
                        listView1.Items[i].SubItems[1].Text = String.Format("{0}", processesInformation[i].PID);
                }
            }
            else if (processesInformation.Length > listView1.Items.Count)//если число процессов увеличилось, 
            {
                for (int i = 0; i < listView1.Items.Count; i++)//то нужно переписать текстовую информацию в listView1
                {
                    if (listView1.Items[i].Text != processesInformation[i].Name)
                        listView1.Items[i].Text = processesInformation[i].Name;
                    if (listView1.Items[i].SubItems[1].Text != String.Format("{0}", processesInformation[i].PID))
                        listView1.Items[i].SubItems[1].Text = String.Format("{0}", processesInformation[i].PID);
                }

                ListViewItem listViewItem;
                for (int i = listView1.Items.Count; i < processesInformation.Length; i++)//и добавить в него новые записи
                {
                    listViewItem = new ListViewItem(processesInformation[i].Name);
                    listViewItem.SubItems.Add(String.Format("{0}", processesInformation[i].PID));
                    listView1.Items.Add(listViewItem);
                }
            }
            else if (processesInformation.Length < listView1.Items.Count)//если число процессов уменьшилось
            {
                for (int i = 0; i < processesInformation.Length; i++)//то нужно переписать текстовую информацию в listView1
                {
                    if (listView1.Items[i].Text != processesInformation[i].Name)
                        listView1.Items[i].Text = processesInformation[i].Name;
                    if (listView1.Items[i].SubItems[1].Text != String.Format("{0}", processesInformation[i].PID))
                        listView1.Items[i].SubItems[1].Text = String.Format("{0}", processesInformation[i].PID);
                }

                int numberOfDeletingItems = listView1.Items.Count - processesInformation.Length;//количество записей для удаления нужно посчитать заранее, так как this.listView1.Items.Count немедленно изменяется при удалении элементов 
                for (int i = processesInformation.Length; i < processesInformation.Length + numberOfDeletingItems; i++)//и удалить остальные записи
                {
                    listView1.Items.RemoveAt(processesInformation.Length);//при удалении i-го элемента коллекции, индексы всех последующих элементов уменьшаются на 1
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            RenewDataInListView();
            timer1.Start();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            /*ProcessInformation[] processesInformationToForm2 = new ProcessInformation[processesInformation.Length];
            for (int i = 0; i < processesInformationToForm2.Length; i++)
            {
                processesInformationToForm2[i] = new ProcessInformation(processesInformation[i]);
            }*/

            ListViewItem doubleClickedListViewItem = (sender as ListView).FocusedItem;
            ProcessInformation selectedProcessInformation = Array.Find<ProcessInformation>(processesInformation,
                pi => pi.PID == Int32.Parse(doubleClickedListViewItem.SubItems[1].Text));

            DetailsForm form2 = new DetailsForm(selectedProcessInformation);
            form2.Show();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (e.Column)
            {
                case 0:
                    if (e.Column != listView1_ColumnClickIndex)
                        listView1_ColumnClickOrder *= 1;
                    else
                        listView1_ColumnClickOrder *= -1;

                    processesInformationComparison = (x, y) => listView1_ColumnClickOrder * x.Name.CompareTo(y.Name);
                    RenewDataInListView();
                    break;
                case 1:
                    if (e.Column != listView1_ColumnClickIndex)
                        listView1_ColumnClickOrder *= 1;
                    else
                        listView1_ColumnClickOrder *= -1;
                    processesInformationComparison = (x, y) => listView1_ColumnClickOrder * x.PID.CompareTo(y.PID);
                    RenewDataInListView();
                    break;
                default:
                    goto case 0;
            }

            listView1_ColumnClickIndex = e.Column;
        }
    }
}
