using ShoeStore_Бусоргин.Helpers;
using ShoeStore_Бусоргин.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
namespace ShoeStore_Бусоргин.Views
{
    /// <summary>
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        public static string fullName { get; set; }
        public List<Orders> orders { get; set; }
        private CollectionViewSource _collectionViewSource;
        public ICollectionView ItemsView { get; set; }
        public OrdersPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadData();

            if (AppData.CurrentUser.RoleId != 1)
            {
                MenuBorder.Visibility = Visibility.Collapsed;
                ShoesContextMenu.Visibility = Visibility.Collapsed;
            }

            fullName = AppData.CurrentUser.FullName ?? "";

            _collectionViewSource = new CollectionViewSource();
            _collectionViewSource.Source = orders;
            ItemsView = _collectionViewSource.View;

            OrdersListView.ItemsSource = ItemsView;

        }

        private void LoadData()
        {
            orders = ShoesShop2Entities.GetContext().Orders
                    .Include(p => p.OrderStatuses)
                    .Include(p => p.Addresses)
                    .Include(p => p.Users)
                    .AsNoTracking().ToList();
        }

        private void btnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditOrderPage());
        }

        private void btnEditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListView.SelectedItem is Orders selectedOrder)
            NavigationService.Navigate(new AddEditOrderPage(selectedOrder));
        }

        private void btnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {

            if (OrdersListView.SelectedItem is Orders selectedOrder)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить заказ?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var orderToDelete = ShoesShop2Entities.GetContext().Orders.Find(selectedOrder.Id);
                    if (orderToDelete != null)
                    {
                        var _orderProducts = ShoesShop2Entities.GetContext().Order_Products.Where(op => op.OrderId == orderToDelete.Id);
                        ShoesShop2Entities.GetContext().Order_Products.RemoveRange(_orderProducts);

                        ShoesShop2Entities.GetContext().Orders.Remove(orderToDelete);
                        ShoesShop2Entities.GetContext().SaveChanges();

                        ItemsView.Filter = item =>
                        {
                            var check = item as Orders;
                            if (check.Id == selectedOrder.Id)
                                return false;
                            return true;
                        };
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления.");
            }
        }
        
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthorizationPage());   
        }

        private void ToProducts_Click(object sender, RoutedEventArgs e)
        {
            Users user = new Users();
            NavigationService.Navigate(new ProductsPage());
        }

        private void OrdersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrdersListView.SelectedItem is Orders selectedOrder)
                NavigationService.Navigate(new AddEditOrderPage(selectedOrder));
        }
    }
}
