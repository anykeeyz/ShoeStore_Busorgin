using ShoeStore_Бусоргин.Helpers;
using ShoeStore_Бусоргин.Models;
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

namespace ShoeStore_Бусоргин.Views
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void GuestEnter(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage());
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTxt.Text;
            string pass = PaswordTxt.Password;
            if (String.IsNullOrEmpty(login))
            {
                MessageBox.Show("Заполните логин");
                return;
            }

            if (String.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Заполните пароль");
                return;
            }

            Users user = ShoesShop2Entities.GetContext().Users.Where(p => p.Login == login && p.Password == pass).FirstOrDefault();

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                return;
            }

            AppData.CurrentUser = user;
            NavigationService.Navigate(new ProductsPage(user));

        }
    }
}
