namespace App_Notas___Grupo_2.Views;

public partial class AggNotaAudio : ContentPage
{
	public AggNotaAudio()
	{
		InitializeComponent();
	}

    private void btnRegresar_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}