using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Data;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace AirsafeDrawCurve
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private ObservableDataSource<System.Windows.Point> DataSourceChannelOne = new ObservableDataSource<System.Windows.Point>();
        private ObservableDataSource<System.Windows.Point> DataSourceChannelTwo = new ObservableDataSource<System.Windows.Point>();
        private ObservableDataSource<System.Windows.Point> DataSourceChannelThree = new ObservableDataSource<System.Windows.Point>();
        private ObservableDataSource<System.Windows.Point> DataSourceChannelFour = new ObservableDataSource<System.Windows.Point>();
        private ObservableDataSource<System.Windows.Point> DataSourceChannelFive = new ObservableDataSource<System.Windows.Point>();
        private ObservableDataSource<System.Windows.Point> DataSourceChannelSix = new ObservableDataSource<System.Windows.Point>();
        private ObservableDataSource<System.Windows.Point> DataSourceChannelSeven = new ObservableDataSource<System.Windows.Point>();
        private ObservableDataSource<System.Windows.Point> DataSourceChannelEight = new ObservableDataSource<System.Windows.Point>();

        private LineGraph graphChannelOne = new LineGraph();
        private LineGraph graphChannelTwo = new LineGraph();
        private LineGraph graphChannelThree = new LineGraph();
        private LineGraph graphChannelFour = new LineGraph();
        private LineGraph graphChannelFive = new LineGraph();
        private LineGraph graphChannelSix = new LineGraph();
        private LineGraph graphChannelSeven = new LineGraph();
        private LineGraph graphChannelEight = new LineGraph();

        private double[] ValueChannelOne;
        private double[] ValueChannelTwo;
        private double[] ValueChannelThree;
        private double[] ValueChannelFour;
        private double[] ValueChannelFive;
        private double[] ValueChannelSix;
        private double[] ValueChannelSeven;
        private double[] ValueChannelEight;

        private int[] Count;

        public MainWindow()
        {
            InitializeComponent();
        }

        //选择Excel文件
        DataTable TargetDataTable;        
        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "(*.xls)|*.xls|(*.xlsx)|*.xlsx";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TargetDataTable=ExcelToDataTable(openFileDialog.FileName,false);
                GenerateArrayData();                                
                DrawChannelFive();
                DrawChannelSix();
                DrawChannelSeven();
                DrawChannelEight();

            }
        }

        /// <summary>  
        /// 将excel导入到datatable  
        /// </summary>  
        /// <param name="filePath">excel路径</param>  
        /// <param name="isColumnName">第一行是否是列名</param>  
        /// <returns>返回datatable</returns>  
        public static DataTable ExcelToDataTable(string filePath, bool isColumnName)
        {
            DataTable dataTable = null;
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            int startRow = 0;
            try
            {
                using (fs = File.OpenRead(filePath))
                {
                    // 2007版本  
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本  
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);

                    if (workbook != null)
                    {
                        sheet = workbook.GetSheetAt(0);//读取第一个sheet，当然也可以循环读取每个sheet  
                        dataTable = new DataTable();
                        if (sheet != null)
                        {
                            int rowCount = sheet.LastRowNum;//总行数  
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(0);//第一行  
                                int cellCount = firstRow.LastCellNum;//列数  

                                //构建datatable的列  
                                if (isColumnName)
                                {
                                    startRow = 1;//如果第一行是列名，则从第二行开始读取  
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        column = new DataColumn("column" + (i + 1));
                                        dataTable.Columns.Add(column);
                                    }
                                }

                                //填充行  
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;

                                    dataRow = dataTable.NewRow();
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        if (cell == null)
                                        {
                                            dataRow[j] = "";
                                        }
                                        else
                                        {
                                            //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
                                            switch (cell.CellType)
                                            {
                                                case CellType.Blank:
                                                    dataRow[j] = "";
                                                    break;
                                                case CellType.Numeric:
                                                    short format = cell.CellStyle.DataFormat;
                                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        dataRow[j] = cell.DateCellValue;
                                                    else
                                                        dataRow[j] = cell.NumericCellValue;
                                                    break;
                                                case CellType.String:
                                                    dataRow[j] = cell.StringCellValue;
                                                    break;
                                            }
                                        }
                                    }
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                return null;
            }
        }
    
        //从DataTable获取数据保存至数组
        int rowsCount;
        public void GenerateArrayData()
        {            
            rowsCount = TargetDataTable.Rows.Count;

            ValueChannelOne = new double[rowsCount];
            ValueChannelTwo = new double[rowsCount];
            ValueChannelThree = new double[rowsCount];
            ValueChannelFour = new double[rowsCount];
            ValueChannelFive = new double[rowsCount];
            ValueChannelSix = new double[rowsCount];
            ValueChannelSeven = new double[rowsCount];
            ValueChannelEight = new double[rowsCount];
            Count = new int[rowsCount];

            for (int i = 0; i < rowsCount; i++)
            {
                ValueChannelOne[i] = Convert.ToDouble(TargetDataTable.Rows[i][1]);
                ValueChannelTwo[i] = Convert.ToDouble(TargetDataTable.Rows[i][2]);
                ValueChannelThree[i] = Convert.ToDouble(TargetDataTable.Rows[i][3]);
                ValueChannelFour[i] = Convert.ToDouble(TargetDataTable.Rows[i][4]);
                ValueChannelFive[i] = Convert.ToDouble(TargetDataTable.Rows[i][5]);
                ValueChannelSix[i] = Convert.ToDouble(TargetDataTable.Rows[i][6]);
                ValueChannelSeven[i] = Convert.ToDouble(TargetDataTable.Rows[i][7]);
                ValueChannelEight[i] = Convert.ToDouble(TargetDataTable.Rows[i][8]);
                Count[i] = i + 1;
            }
        }

        #region 绘制8条曲线
        //绘制通道1
        public void DrawChannelOne()
        {
            graphChannelOne = DynamicChart.AddLineGraph(DataSourceChannelOne, Colors.Black, 2, "通道1");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelOne[i]);
                DataSourceChannelOne.AppendAsync(base.Dispatcher, point);
            }
        }

        //绘制通道2
        public void DrawChannelTwo()
        {
            graphChannelTwo = DynamicChart.AddLineGraph(DataSourceChannelTwo, Colors.Indigo, 2, "通道2");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelTwo[i]);
                DataSourceChannelTwo.AppendAsync(base.Dispatcher, point);
            }
        }

        //绘制通道3
        public void DrawChannelThree()
        {
            graphChannelThree = DynamicChart.AddLineGraph(DataSourceChannelThree, Colors.Yellow, 2, "通道3");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelThree[i]);
                DataSourceChannelThree.AppendAsync(base.Dispatcher, point);
            }
        }

        //绘制通道4
        public void DrawChannelFour()
        {
            graphChannelFour = DynamicChart.AddLineGraph(DataSourceChannelFour, Colors.Green, 2, "通道4");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelFour[i]);
                DataSourceChannelFour.AppendAsync(base.Dispatcher, point);
            }
        }

        //绘制通道5
        public void DrawChannelFive()
        {
            graphChannelFive = DynamicChart.AddLineGraph(DataSourceChannelFive, Colors.Blue, 2, "通道5");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelFive[i]);
                DataSourceChannelFive.AppendAsync(base.Dispatcher, point);
            }
        }

        //绘制通道6
        public void DrawChannelSix()
        {
            graphChannelSix = DynamicChart.AddLineGraph(DataSourceChannelSix, Colors.Orange, 2, "通道6");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelSix[i]);
                DataSourceChannelSix.AppendAsync(base.Dispatcher, point);
            }
        }

        //绘制通道7
        public void DrawChannelSeven()
        {
            graphChannelSeven = DynamicChart.AddLineGraph(DataSourceChannelSeven, Colors.Purple, 2, "通道7");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelSeven[i]);
                DataSourceChannelSeven.AppendAsync(base.Dispatcher, point);
            }
        }

        //绘制通道8
        public void DrawChannelEight()
        {
            graphChannelEight = DynamicChart.AddLineGraph(DataSourceChannelEight, Colors.Red, 2, "通道8");

            for (int i = 0; i < rowsCount; i++)
            {
                System.Windows.Point point = new System.Windows.Point(Count[i], ValueChannelEight[i]);
                DataSourceChannelEight.AppendAsync(base.Dispatcher, point);
            }
        }
        #endregion

        #region 显示8条曲线        
        //显示通道1
        private void SelectChannelOne_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelOne);
            DataSourceChannelOne = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelOne.IsChecked==true)
            {
                DrawChannelOne();
            }
        }

        //显示通道2
        private void SelectChannelTwo_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelTwo);
            DataSourceChannelTwo = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelTwo.IsChecked == true)
            {
                DrawChannelTwo();
            }
        }

        //显示通道3
        private void SelectChannelThree_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelThree);
            DataSourceChannelThree = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelThree.IsChecked == true)
            {
                DrawChannelThree();
            }
        }

        //显示通道4
        private void SelectChannelFour_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelFour);
            DataSourceChannelFour = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelFour.IsChecked == true)
            {
                DrawChannelFour();
            }
        }

        //显示通道5
        private void SelectChannelFive_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelFive);
            DataSourceChannelFive = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelFive.IsChecked == true)
            {
                DrawChannelFive();
            }
        }

        //显示通道6
        private void SelectChannelSix_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelSix);
            DataSourceChannelSix = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelSix.IsChecked == true)
            {
                DrawChannelSix();
            }
        }

        //显示通道7
        private void SelectChannelSeven_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelSeven);
            DataSourceChannelSeven = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelSeven.IsChecked == true)
            {
                DrawChannelSeven();
            }
        }

        //显示通道8
        private void SelectChannelEight_Click(object sender, RoutedEventArgs e)
        {
            DynamicChart.Children.Remove(graphChannelEight);
            DataSourceChannelEight = new ObservableDataSource<System.Windows.Point>();

            if (SelectChannelEight.IsChecked == true)
            {
                DrawChannelEight();
            }
        }
        #endregion
    }
}
