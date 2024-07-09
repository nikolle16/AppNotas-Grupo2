namespace App_Notas___Grupo_2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Views.Login());
        }
    }
}
