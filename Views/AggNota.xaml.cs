namespace App_Notas___Grupo_2.Views;

public partial class AggNota : ContentPage
{
	public AggNota()
	{
		InitializeComponent();
	}

    private void btnRegresar_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}