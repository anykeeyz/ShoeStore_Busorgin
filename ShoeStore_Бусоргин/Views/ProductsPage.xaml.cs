using ShoeStore_Бусоргин.Helpers;
using ShoeStore_Бусоргин.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;

namespace ShoeStore_Бусоргин.Views
{
    /// <summary>
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        public static ObservableCollection<ProductInfo> _products { get; set; }
        public static Users _user;
        public static string fullName { get; set; }
        public static List<string> _suppliers = new List<string>();
        public bool?  UpOrDown { get; set; }
        private CollectionViewSource _collectionViewSource;
        public ICollectionView ItemsView { get; set; }
        public ProductsPage(Users user = null)
        {
            InitializeComponent();
            DataContext = this;

            _user = user;
            fullName = AppData.CurrentUser.FullName ?? "";
            if (user?.RoleId == null || user?.RoleId == 4) 
            {
                MenuBorder.Visibility = Visibility.Collapsed;
                ToOrderBtn.Visibility = Visibility.Collapsed;
            }
            if (user?.RoleId != 1)
            {
                ShoesContextMenu.Visibility = Visibility.Collapsed;
                AddTovarBtn.Visibility = Visibility.Collapsed;
            }
                
            LoadData();

            _collectionViewSource = new CollectionViewSource();
            _collectionViewSource.Source = _products;
            ItemsView = _collectionViewSource.View;

            ProductsListView.ItemsSource = ItemsView;        
            SuppCmb.ItemsSource = _suppliers;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthorizationPage());
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsView.CanSort)
                ItemsView.SortDescriptions.Clear();
            ItemsView.Filter = item =>
            {
                return true;
            };
            ItemsView.Refresh();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter();             
        }

        private void Filter()
        {
            var str = SearchTxt.Text;
            
            ItemsView.Filter = item =>
            {
                bool state;
                var model = item as ProductInfo;

                bool a = true;
                if (str.Length > 3)
                {
                    a = model.Description.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        model.ProdName.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        model.Category.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        model.Producer.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0;
                }
                
                bool b = true;
                if (SuppCmb.SelectedValue?.ToString() == "Все Поставщики")
                {
                    b = true;
                }
                else if (SuppCmb.SelectedIndex >= 1)
                    b = model.Supplier.CompareTo(SuppCmb.SelectedValue?.ToString()) == 0;
                state = a && b;

                return state;

            };
        }

        private void Sort()
        {
            if (ItemsView.CanSort)
            {
                ItemsView.SortDescriptions.Clear();
                if (UpOrDown == true)
                {

                    ItemsView.SortDescriptions.Add(
                        new SortDescription("OnStock", ListSortDirection.Ascending));
                }

                if (UpOrDown == false)
                {

                    ItemsView.SortDescriptions.Add(
                        new SortDescription("OnStock", ListSortDirection.Descending));
                }
            }
        }

        private void SuppCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppCmb.SelectedIndex == 0)
            {
                Filter();
                return;
            }
            if (SuppCmb.SelectedIndex == -1)
            {
                return;
            }
            Filter();
        }

        private void UpBtn_Click(object sender, RoutedEventArgs e)
        {
            UpOrDown = true;
            Sort();
        }

        private void DownBtn_Click(object sender, RoutedEventArgs e)
        {
            UpOrDown = false;
            Sort();
        }

        private void LoadData()
        {
            var temp = ShoesShop2Entities.GetContext().Products
                .Include(p => p.Categories)
                .Include(p => p.ProdNames)
                .Include(p => p.Producers)
                .Include(p => p.Suppliers)
                .Include(p => p.Units)
                .Select(p => new ProductInfo
                {
                    Id = p.Id,
                    Description = p.Describtion,
                    Category = p.Categories.Category,
                    ProdName = p.ProdNames.ProdName,
                    Producer = p.Producers.Producer,
                    Supplier = p.Suppliers.Supplier,
                    Price = p.Price,
                    Unit = p.Units.Unit,
                    OnStock = p.OnStock,
                    Discount = p.Discount,
                    PhotoPath = p.PhotoPath
                }).AsNoTracking().ToList();

            _products = new ObservableCollection<ProductInfo>(temp);

            _suppliers.Add("Все Поставщики");
            _suppliers.AddRange(_products.Select(p => p.Supplier).Distinct().ToList());
            ShoesShop2Entities.GetContext().Suppliers.Select(p => p.Supplier).ToList();
        }

        private void EditProductClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_user?.RoleId == 1)
            {
                var product = ProductsListView.SelectedItem as ProductInfo;
                NavigationService.Navigate(new AddEditTovarPage(product));
                LoadData();
                ItemsView.Refresh();
            }          
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListView.SelectedItem != null)
            {
                if (MessageBox.Show("Удалить товар? Отменить это действие будет невозможно.", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var product = ProductsListView.SelectedItem as ProductInfo;
                    var temp = ShoesShop2Entities.GetContext().Products.Where(p => p.Id == product.Id).FirstOrDefault();

                    ShoesShop2Entities.GetContext().Entry(temp).Collection(p => p.Order_Products).Load();
                    var currentOrders = ShoesShop2Entities.GetContext().Orders.Where(o => o.StatusId == 2).Select(o => o.Id).ToList();
                    var arr = temp.Order_Products.ToArray();

                    if (arr.Length > 0)
                    {
                        MessageBox.Show("Невозможно удалить товар, так как он присутствует в заказах");
                        return;
                    }

                    ShoesShop2Entities.GetContext().Products.Remove(temp);

                    ItemsView.Filter = item =>
                    {
                        var check = item as ProductInfo;
                        if (check.Id == product.Id)
                            return false;
                        return true;
                    };

                    try
                    {
                        ShoesShop2Entities.GetContext().SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else
                MessageBox.Show("Товар не выбран");
        }

        private void AddTovar_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditTovarPage());
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadData();

            ItemsView.Filter = item =>
            {
                return true;
            };
        }

        protected async void OnNavigatedTo(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                LoadData();

                ItemsView.Filter = item =>
                {
                    return true;
                };
            }
        }

        private void ToOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrdersPage());
        }
    }
}
