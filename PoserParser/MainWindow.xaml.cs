using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosedXML.Excel;
using System.Net;
using System.IO;
using PoserParser;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using System.ComponentModel;

namespace PoserParser
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    // https://bdu.fstec.ru/files/documents/thrlist.xlsx
    /// 
    public partial class MainWindow : Window
    {
        const string address = "https://bdu.fstec.ru/files/documents/thrlist.xlsx";
        public List<UBIFull> History = new List<UBIFull>();
        private PagingCollectionView cview;
        public int elmCount = 15;
        (Dictionary<int, UBIFull> fullDB, Dictionary<int, UBIShort> shortDB) dataBase;
        //TODO:
        //добавить фичу с открытием подробностей
        //допилить Report
        public MainWindow()
        {

            try
            {
                WsParse(new XLWorkbook(@"thrlist.xlsx").Worksheet("Sheet"), ref dataBase.shortDB, ref dataBase.fullDB);
            }
            catch (Exception)
            {
                MessageBox.Show($"Базы данных на данном устройстве не обнаружено! База данных будет загружена с сервера!", "Уведомление");
                new WebClient().DownloadFile(address, "thrlist.xlsx");
                WsParse(new XLWorkbook(@"thrlist.xlsx").Worksheet("Sheet"), ref dataBase.shortDB, ref dataBase.fullDB);

            }

            this.cview = new PagingCollectionView(dataBase.shortDB, elmCount);
            this.DataContext = this.cview;
            InitializeComponent();

            if (History.Count == 0)
            {
                HistoryButton.Visibility = Visibility.Hidden;
            }
            
        }
        protected override void OnClosing(CancelEventArgs e)
        {            
            Application.Current.Shutdown();
        }
        private void WsParse(IXLWorksheet ws, ref Dictionary<int, UBIShort> dicShort, ref Dictionary<int, UBIFull> dic)
        {
            dicShort = new Dictionary<int, UBIShort>();
            dic = new Dictionary<int, UBIFull>();
            for (int i = 3; i <= ws.Rows().Count(); i++)
            {
                dicShort.Add(int.Parse(ws.Cell(i, 1).GetString()),
                    new UBIShort("УБИ." + int.Parse(ws.Cell(i, 1).GetString()).ToString("000"), ws.Cell(i, 2).GetString()));

                dic.Add(int.Parse(ws.Cell(i, 1).GetString()), new UBIFull(
                    ws.Cell(i, 1).GetString(), ws.Cell(i, 2).GetString(),
                    ws.Cell(i, 3).GetString(), ws.Cell(i, 4).GetString(),
                    ws.Cell(i, 5).GetString(),
                    ws.Cell(i, 6).GetString() == "0" ? "нет" : "да",
                    ws.Cell(i, 7).GetString() == "0" ? "нет" : "да",
                    ws.Cell(i, 7).GetString() == "0" ? "нет" : "да"));
            }
        }

        private void OnNextClicked(object sender, RoutedEventArgs e)
        {
            this.cview.MoveToNextPage();
        }

        private void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            this.cview.MoveToPreviousPage();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            ComboBoxItem comboBoxItem = ((sender as ComboBox).SelectedItem as ComboBoxItem);
            elmCount = int.Parse(comboBoxItem.Content.ToString());
            cview.ItemsPerPage = elmCount;
            cview.Refresh();
        }

        private void Button_UPD(object sender, RoutedEventArgs e)
        {//допилить проверку на ошибки (try-catch)
            try
            {
                var old = (dataBase.fullDB, dataBase.shortDB); //Если что, можно будет откатить изменения
                new WebClient().DownloadFile(address, "newthrlist.xlsx");
                WsParse(new XLWorkbook(@"newthrlist.xlsx").Worksheet("Sheet"), ref dataBase.shortDB, ref dataBase.fullDB);

                var changes = new Dictionary<int, string>();
                var changed = new List<UBIFull>();

                foreach (var i1 in dataBase.fullDB)
                {
                    foreach (var i2 in old.fullDB)
                    {
                        if (!i1.Value.Equals(i2.Value) && i1.Key == i2.Key)
                        {//поиск свойства
                            string props = "";
                            if (i2.Value.Name != i1.Value.Name)
                                props += i2.Value.Name + "/";
                            if (i2.Value.Description != i1.Value.Description)
                                props += i2.Value.Description + "/";
                            if (i2.Value.HazardSource != i1.Value.HazardSource)
                                props += i2.Value.HazardSource + "/";
                            if (i2.Value.HazardObject != i1.Value.HazardObject)
                                props += i2.Value.HazardObject + "/";
                            if (i2.Value.ConfidentialCheck != i1.Value.ConfidentialCheck)
                                props += i2.Value.ConfidentialCheck + "/";
                            if (i2.Value.IntegrityCheck != i1.Value.IntegrityCheck)
                                props += i2.Value.IntegrityCheck + "/";
                            if (i2.Value.AccessibiltiyCheck != i1.Value.AccessibiltiyCheck)
                                props += i2.Value.AccessibiltiyCheck + "/";
                            //props = props.Remove(props.Length - 1);

                            changes.Add(i1.Key, props);
                            changed.Add(i2.Value);
                            changed.Add(i1.Value);
                        }
                    }
                    if (!old.fullDB.ContainsKey(i1.Key)) //Добавленные
                    {
                        changes.Add(i1.Key, "Added");
                        changed.Add(i1.Value);
                    }
                }

                foreach (var i2 in old.fullDB)
                {
                    if (!dataBase.fullDB.ContainsKey(i2.Key)) //Удаленные
                    {
                        changes.Add(i2.Key, "Deleted");
                        changed.Add(i2.Value);
                    }
                }
                if (changes.Count == 0)
                {
                    //Ээх адаптировать бы нормально "Is up to date"
                    MessageBox.Show("База данных уже обновлена", "Отчет");
                }
                else
                {
                    var result = MessageBox.Show($"Кол-во обновленных записей: {changes.Count}\n\n" +
                      $"Желаете ли вы посмотреть подробный отчет?", "Отчет", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        HistoryButton.Visibility = Visibility.Visible;
                        DicToColorConverter.ids = changes;
                        var report = new Report();
                        report.Show();
                        report.DataContext = changed;
                        History = changed;
                    }
                    else
                    {
                        HistoryButton.Visibility = Visibility.Visible;
                        DicToColorConverter.ids = changes;
                        var report = new Report();
                        report.DataContext = changed;
                        History = changed;
                    }
                }

                this.cview = new PagingCollectionView(dataBase.shortDB, elmCount);
                this.DataContext = this.cview;
                //File.Delete(@"thrlist.xlsx");
                //File.Move(@"newthrlist.xlsx", @"thrlist.xlsx"); // комменнт для теста
            }
            catch (Exception ex)
            {
                MessageBox.Show("Косяндепало: " + ex.Message);
                File.Delete(@"newthrlist.xlsx");
                WsParse(new XLWorkbook(@"thrlist.xlsx").Worksheet("Sheet"), ref dataBase.shortDB, ref dataBase.fullDB);
            }
        }

        private void UBI_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var id = (UBI.SelectedItem as UBIShort).ID.ToString();
            var fullinfo = new FullInfo(id);
            var el = dataBase.fullDB[int.Parse(id.Split('.')[1])];
            var smth = new List<object>
            {
                new
                {
                    ID = el.ID,
                    Name = el.Name,
                    Description = el.Description,
                    HazardSource = el.HazardSource,
                    HazardObject = el.HazardObject,
                    ConfidentialCheck = el.ConfidentialCheck,
                    IntegrityCheck = el.IntegrityCheck,
                    AccessibiltiyCheck = el.AccessibiltiyCheck
                }
            };
            fullinfo.DataContext = smth;
            fullinfo.Show();
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            SaveFileDialog save = new SaveFileDialog();
            save.FileName = @"thrlist.xlsx";
            save.Filter = "XLSX File (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            save.RestoreDirectory = true;

            try
            {
                save.ShowDialog();
                if ((myStream = save.OpenFile()) != null)
                    myStream.Close();
            }
            catch (Exception) { }
        }

        private void Change_History(object sender, RoutedEventArgs e)
        {
            var report = new Report();
            report.DataContext = History;
            report.Show();
        }
    }
}