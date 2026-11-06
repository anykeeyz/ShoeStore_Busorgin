using ShoeStore_Бусоргин.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ShoeStore_Бусоргин.Views
{
    /// <summary>
    /// Логика взаимодействия для AddEditOrderPage.xaml
    /// </summary>
    public partial class AddEditOrderPage : Page
    {
        public Orders _order {  get; set; }
        public List<OrderStatuses> _statuses { get; set; }
        public List<Addresses> _addresses { get; set; }  
        public List<Users> _clients { get; set; }

        private ObservableCollection<OrderProductInfo> _orderProducts;
        public AddEditOrderPage(Orders order = null)
        {
            InitializeComponent();
            _orderProducts = new ObservableCollection<OrderProductInfo>();
            if (order != null )
            {
                _order = order;
            }
            else
            {
                dpOrderDate.SelectedDate = DateTime.Now;
                dpDeliveryDate.SelectedDate = DateTime.Now.AddDays(7);
            }
            LoadOrderData();
            LoadProducts();
            
        }

        private void LoadOrderData()
        {
            _statuses = ShoesShop2Entities.GetContext().OrderStatuses.ToList();
            _addresses = ShoesShop2Entities.GetContext().Addresses.ToList();
            _clients = ShoesShop2Entities.GetContext().Users.ToList();

            cmbStatus.ItemsSource = _statuses;
            cmbAddress.ItemsSource = _addresses;
            cmbClient.ItemsSource = _clients;

            if (_order != null)
            {
                txtCode.Text = _order.Code.ToString();
                dpOrderDate.SelectedDate = _order.OrderDate;
                dpDeliveryDate.SelectedDate = _order.Deliverydate;

                cmbStatus.SelectedItem = _statuses.FirstOrDefault(s => s.Id == _order.StatusId);
                cmbAddress.SelectedItem = _addresses.FirstOrDefault(a => a.Id == _order.AddressId);
                cmbClient.SelectedItem = _clients.FirstOrDefault(u => u.FullName == _order.Users.FullName);

                var orderProducts = ShoesShop2Entities.GetContext().Order_Products
                .Where(op => op.OrderId == _order.Id)
                .Select(op => new OrderProductInfo
                {
                    Id = op.ProductId,
                    ProductName = op.Products.ProdNames.ProdName,
                    Count = op.ProductCount,
                    Price = op.Products.Price,
                    Total = op.ProductCount * op.Products.Price
                })
                .ToList();

                decimal totalSum = 0;
                foreach (var product in orderProducts)
                {
                    _orderProducts.Add(product);
                    totalSum += product.Total;
                }

                txtTotalSum.Text = totalSum.ToString();
                dgOrderProducts.ItemsSource = _orderProducts;
            }
            
        }

        private void LoadProducts()
        {
            var products = ShoesShop2Entities.GetContext().Products
                .Select(p => new ProductComboBoxItem
                {
                    ProductId = p.Id,
                    ProductName = p.ProdNames.ProdName,
                    DisplayText = "",
                    Price = p.Price
                }).ToList();
            for ( var i = 0; i < products.Count; i++ )
            {
                products[i].DisplayText = $"{products[i].ProductName} (Артикул: {products[i].ProductId}, Цена: {products[i].Price:F2} руб.)";
            }

            cmbProduct.ItemsSource = products;
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtCode.Text, out int code) || code <= 0)
            {
                MessageBox.Show("Артикул должен быть положительным числом!");
                return;
            }

            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус заказа!");
                return;
            }

            if (cmbAddress.SelectedItem == null)
            {
                MessageBox.Show("Выберите адрес пункта выдачи!");
                return;
            }

            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента!");
                return;
            }

            if (dpOrderDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заказа!");
                return;
            }

            if (dpDeliveryDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату выдачи!");
                return;
            }

            if (dpDeliveryDate.SelectedDate < dpOrderDate.SelectedDate)
            {
                MessageBox.Show("Дата выдачи не может быть раньше даты заказа!");
                return;
            }

            if (_orderProducts.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в заказ!");
                return;
            }

            int orderId;

            if (_order == null)
            {
                var newOrder = new Orders
                {
                    Code = code,
                    StatusId = ((OrderStatuses)cmbStatus.SelectedItem).Id,
                    AddressId = ((Addresses)cmbAddress.SelectedItem).Id,
                    ClientId = ((Users)cmbClient.SelectedItem).Id,
                    OrderDate = dpOrderDate.SelectedDate.Value,
                    Deliverydate = dpDeliveryDate.SelectedDate.Value
                };

                ShoesShop2Entities.GetContext().Orders.Add(newOrder);
                ShoesShop2Entities.GetContext().SaveChanges();
                orderId = newOrder.Id;
            }
            else
            {
                var existingOrder = ShoesShop2Entities.GetContext().Orders.Find(_order.Id);
                if (existingOrder != null)
                {
                    existingOrder.Code = code;
                    existingOrder.StatusId = ((OrderStatuses)cmbStatus.SelectedItem).Id;
                    existingOrder.AddressId = ((Addresses)cmbAddress.SelectedItem).Id;
                    existingOrder.ClientId = ((Users)cmbClient.SelectedItem).Id;
                    existingOrder.OrderDate = dpOrderDate.SelectedDate.Value;
                    existingOrder.Deliverydate = dpDeliveryDate.SelectedDate.Value;
                    orderId = existingOrder.Id;

                    var oldOrderProducts = ShoesShop2Entities.GetContext().Order_Products.Where(op => op.OrderId == orderId);
                    ShoesShop2Entities.GetContext().Order_Products.RemoveRange(oldOrderProducts);
                }
                else
                {
                    MessageBox.Show("Заказ не найден!");
                    return;
                }
            }

            foreach (var product in _orderProducts)
            {
                var orderProduct = new Order_Products
                {
                    OrderId = orderId,
                    ProductId = product.Id,
                    ProductCount = product.Count
                };
                ShoesShop2Entities.GetContext().Order_Products.Add(orderProduct);
            }

            ShoesShop2Entities.GetContext().SaveChanges();
            NavigationService.GoBack();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Выйти? Все несохраненные изменения будут потерянны.", "Отмена", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (Convert.ToBoolean(res))
                NavigationService.GoBack();
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для добавления!");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество должно быть положительным числом!");
                return;
            }

            var selectedProduct = (ProductComboBoxItem)cmbProduct.SelectedItem;

            var existingProduct = _orderProducts.FirstOrDefault(p => p.Id == selectedProduct.ProductId);

            if (existingProduct != null)
            {
                existingProduct.Count += quantity;
                existingProduct.Total = existingProduct.Count * existingProduct.Price;
            }
            else
            {
                if (dgOrderProducts.Items.Count == 0)
                    dgOrderProducts.ItemsSource = _orderProducts;
                _orderProducts.Add(new OrderProductInfo
                {
                    Id = selectedProduct.ProductId,
                    ProductName = selectedProduct.ProductName,
                    Count = quantity,
                    Price = selectedProduct.Price,
                    Total = quantity * selectedProduct.Price
                });
            }

            UpdateTotalSum();
            txtQuantity.Text = "1";
        }

        private void btnRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrderProducts.SelectedItem is OrderProductInfo selectedProduct)
            {
                _orderProducts.Remove(selectedProduct);
                UpdateTotalSum();
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления!");
            }
        }

        private void UpdateTotalSum()
        {
            decimal totalSum = _orderProducts.Sum(p => p.Total);
            txtTotalSum.Text = $"Общая сумма: {totalSum:F2} руб.";
        }
    }

    public class OrderProductInfo
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }

    public class ProductComboBoxItem
    {
        public string ProductId { get; set; }
        public string DisplayText { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }
}
