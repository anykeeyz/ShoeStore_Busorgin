using Microsoft.Win32;
using ShoeStore_Бусоргин.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ShoeStore_Бусоргин.Views
{
    /// <summary>
    /// Логика взаимодействия для AddEditTovarPage.xaml
    /// </summary>
    public partial class AddEditTovarPage : Page
    {
        public ProductInfo _product { get; set; }
        private string _photoPath {  get; set; }
        private string _fullPhotoPath { get; set; }
        private bool _isImageChanged { get; set; } = false;
        public Categories[] _categories { get; set; }
        public Producers[] _producers { get; set; }
        public Suppliers[] _suppliers { get; set; }
        //private Regex reg = new Regex(@"^\d+\.\d\d$");
        public AddEditTovarPage(ProductInfo product = null)
        {

            InitializeComponent();

            _product = product;
            DataContext = this;

            _categories = ShoesShop2Entities.GetContext().Categories.ToArray();
            _producers = ShoesShop2Entities.GetContext().Producers.ToArray();
            _suppliers = ShoesShop2Entities.GetContext().Suppliers.ToArray();

            if (product != null)
            {
                CatComboBox.SelectedIndex = _categories.Where(p => p.Category == _product.Category).Select(p => p.Id).First() - 1;
                ProdComboBox.SelectedIndex = _producers.Where(p => p.Producer == _product.Producer).Select(p => p.Id).First() - 1;
                SuppComboBox.SelectedIndex = _suppliers.Where(p => p.Supplier == _product.Supplier).Select(p => p.Id).First() - 1;
                _photoPath = _product.PhotoPath;
                _fullPhotoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _photoPath);
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthorizationPage());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ChooseImg_Click(object sender, RoutedEventArgs e)
        {
            _isImageChanged = true;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".png";
            dialog.Filter = "Изображения | *.jpg; *.jpeg; *.png; *.bmp; *.gif";
            if (dialog.ShowDialog() == true)
            {
                string fullPath = dialog.FileName;
                _photoPath = Path.Combine("Res", dialog.SafeFileName.ToString());
                PhotoPathTxt.Text = dialog.SafeFileName;

                string newPath = Path.Combine(Environment.CurrentDirectory, _photoPath);
                File.Create(newPath).Close();
                File.Copy(fullPath, newPath, overwrite: true);

                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(_photoPath, UriKind.Relative);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                ShoesImg.Source = img;
                _fullPhotoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _photoPath);
            }
        }

        private int GetProdNameId(string name = null)
        {
            List<ProdNames> prodNames = ShoesShop2Entities.GetContext().ProdNames.ToList();
            prodNames = prodNames.Select(p => new ProdNames() { Id = p.Id, ProdName = p.ProdName.ToLower().Trim() }).ToList();

            int check = 0;
            if (name  != null) 
                check = prodNames.Where(p => p.ProdName == name.ToLower().Trim()).Select(p => p.Id).FirstOrDefault(); 
                
            if (check != 0)
                return check;
            else {
                var temp = ShoesShop2Entities.GetContext().ProdNames.Add(new ProdNames() { ProdName = name });
                return temp.Id;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(PriceTxt.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом!"); return;
            }
            if (!int.TryParse(OnStockTxt.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int stock) || stock < 0)
            {
                MessageBox.Show("Количество на складе не может быть отрицательным!"); return;
            }
            if (!byte.TryParse(DiscountTxt.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out byte discount) || discount > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100!"); return;
            }
            if (String.IsNullOrWhiteSpace(ProdNameTxt.Text))
            {
                MessageBox.Show("Укажите название товара"); return;
            }
            if (String.IsNullOrWhiteSpace(UnitTxt.Text))
            {
                MessageBox.Show("Укажите ед. измерения"); return;
            }
            if ( CatComboBox.SelectedItem == null ||
                ProdComboBox.SelectedItem == null || SuppComboBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все выпадающие списки!"); return;
            }

            string finalPhotoPath = _photoPath;
            if (_isImageChanged && _photoPath != ImageHelper.DefaultImagePath)
            {
                if (_product != null && !string.IsNullOrEmpty(_product.PhotoPath))
                {
                    ImageHelper.DeleteOldImage(_product.PhotoPath);
                }
                finalPhotoPath = ImageHelper.ResizeAndSaveImage(_photoPath);
            }

            if (_product == null)
            {
                var newProduct = new Products
                {
                    Id = GenerateNewId(),
                    ProdNameId = GetProdNameId(ProdNameTxt.Text),
                    CategoryId = Convert.ToInt32(CatComboBox.SelectedValue),
                    ProducerId = Convert.ToInt32(ProdComboBox.SelectedValue),
                    SupplierId = Convert.ToInt32(SuppComboBox.SelectedValue),
                    UnitId = 1,
                    Price = price,
                    OnStock = stock,
                    Discount = discount,
                    Describtion = DescriptionTxt.Text.Trim(),
                    PhotoPath = "/Res/" + Path.GetFileName(finalPhotoPath),
                };
                ShoesShop2Entities.GetContext().Products.Add(newProduct);
            }
            else
            {
                var existingProduct = ShoesShop2Entities.GetContext().Products.Find(_product.Id);
                if (existingProduct != null)
                {
                    existingProduct.ProdNameId = GetProdNameId(ProdNameTxt.Text);
                    existingProduct.CategoryId = Convert.ToInt32(CatComboBox.SelectedValue);
                    existingProduct.ProducerId = Convert.ToInt32(ProdComboBox.SelectedValue);
                    existingProduct.SupplierId = Convert.ToInt32(SuppComboBox.SelectedValue);
                    existingProduct.UnitId = 1;
                    existingProduct.Price = price;
                    existingProduct.OnStock = stock;
                    existingProduct.Discount = discount;
                    existingProduct.Describtion = DescriptionTxt.Text.Trim();
                    existingProduct.PhotoPath = "/Res/" + Path.GetFileName(finalPhotoPath);
                }
            }

            ShoesShop2Entities.GetContext().SaveChanges();
            MessageBox.Show("Изменения сохранены", "Сохранение", MessageBoxButton.OK);
            NavigationService.GoBack();
        }

        private string GenerateNewId()
        {
            var products = ShoesShop2Entities.GetContext().Products.Select(p => p.Id).ToList();

            
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new StringBuilder(7);
            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
                result.Append(chars[rnd.Next(chars.Length)]);
            }

            if (products.Contains(result.ToString()))
                GenerateNewId();

            return result.ToString();
        }

        private void PriceTxt_Validation(object sender, TextCompositionEventArgs e)
        {
            PriceTxt.Background = Brushes.White;

            if (!Char.IsDigit(e.Text.ToString(), 0) && !Char.IsPunctuation(e.Text.ToString(), 0))
            {
                e.Handled = true;
                return;
            }
        }

        private void OnStockTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text.ToString(), 0))
            {
                e.Handled = true;
                return;
            }
        }

        private void DiscountTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text.ToString(), 0))
            {
                e.Handled = true;
                return;
            }
            Clipboard.SetText(e.ToString());
        }
    }
}
