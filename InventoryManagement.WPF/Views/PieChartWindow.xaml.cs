using InventoryManagement.EntityFramework.Database;
using OxyPlot;
using OxyPlot.Series;
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
using System.Windows.Shapes;

namespace InventoryManagement.WPF.Views
{
    /// <summary>
    /// Interaction logic for PieChartWindow.xaml
    /// </summary>
    public partial class PieChartWindow : Window
    {
        private readonly InventoryManagementDbContext _context;

        public PieChartWindow()
        {
            InitializeComponent();
            _context = new InventoryManagementDbContext();
            LoadPieChartData();
        }

        private void LoadPieChartData()
        {
            var orders = _context.Orders.ToList();
            var totalOrders = orders.Count;
            if (totalOrders == 0)
            {
                return;
            }

            var statusCounts = orders.GroupBy(o => o.Status).Select(g => new 
            { 
                Status = g.Key, Count = g.Count()
            }).ToList();

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                AngleSpan = 360,
                StartAngle = 0,
                InsideLabelPosition = 0.8,
                OutsideLabelFormat = "{0}: {1} ({2:P})",
                TextColor = OxyColors.Black
            };

            foreach (var status in statusCounts)
            {
                pieSeries.Slices.Add(new PieSlice(status.Status, status.Count)
                {
                    IsExploded = false,
                    Fill = GetColorForStatus(status.Status)
                });
            }

            var plotModel = new PlotModel { Title = "Order Status Distribution" };
            plotModel.Series.Add(pieSeries);
            OrderPieChart.Model = plotModel;
        }

        private OxyColor GetColorForStatus(string status)
        {
            return status switch
            {
                "Pending" => OxyColors.Red,
                "Arrived" => OxyColors.Blue,
                "Added to warehouse" => OxyColors.Green,
                _ => OxyColors.Gray
            };
        }
    }
}
